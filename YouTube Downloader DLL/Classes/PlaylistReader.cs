using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader_DLL.Classes
{
    public class PlaylistReader
    {
        public const string CmdPlaylistInfo = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json{2}{3} \"{4}\"";
        public const string CmdPlaylistRange = " --playlist-items {0}";
        public const string CmdPlaylistReverse = " --playlist-reverse";

        int _currentVideoPlaylistIndex = -1;
        int _index = 0;

        bool _processFinished = false;

        string _arguments;
        string _playlist_id;
        string _url;
        string _currentVideoID;

        List<string> _jsonPaths = new List<string>();

        Regex _regexPlaylistInfo = new Regex(@"^\[youtube:playlist\] playlist (.*):.*Downloading\s+(\d+)\s+.*$", RegexOptions.Compiled);
        Regex _regexVideoJson = new Regex(@"^\[info\].*JSON.*:\s(.*)$", RegexOptions.Compiled);
        Regex _regexPlaylistIndex = new Regex(@"\[download\]\s\w*\s\w*\s(\d+)", RegexOptions.Compiled);
        Regex _regexVideoID = new Regex(@"\[youtube\]\s(.*):", RegexOptions.Compiled);
        Process _youtubeDl;
        OperationLogger _logger;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public bool Canceled { get; set; } = false;
        public Playlist Playlist { get; set; } = null;

        public PlaylistReader(string url, int[] videos, bool reverse)
        {
            string json_dir = Common.GetJsonDirectory();

            _playlist_id = Helper.GetPlaylistId(url);

            string range = string.Empty;

            if (videos != null && videos.Length > 0)
            {
                // Make sure the video indexes is sorted, otherwise reversing wont do anything
                Array.Sort(videos);
                var items = new StringBuilder(videos[0].ToString());

                for (int i = 1; i < videos.Length; i++)
                    items.Append("," + videos[i]);

                range = string.Format(CmdPlaylistRange, items);
            }

            string reverseS = reverse ? CmdPlaylistReverse : string.Empty;

            _arguments = string.Format(CmdPlaylistInfo, json_dir, _playlist_id, range, reverseS, url);
            _url = url;

            _logger = OperationLogger.Create(OperationLogger.YTDLogFile);

            var ytd = new YoutubeDlProcess(_logger, null);

            ytd.LogHeader(_arguments);

            _youtubeDl = Helper.StartProcess(YoutubeDlProcess.YouTubeDlPath,
                _arguments,
                OutputReadLine,
                ErrorReadLine,
                null,
                _logger);
            _youtubeDl.Exited += delegate
            {
                _processFinished = true;
                ytd.LogFooter();
            };
        }

        public void OutputReadLine(Process process, string line)
        {
            Match m;

            if (line.StartsWith("[youtube:playlist]"))
            {
                if ((m = _regexPlaylistInfo.Match(line)).Success)
                {
                    // Get the playlist info
                    string name = m.Groups[1].Value;
                    int onlineCount = int.Parse(m.Groups[2].Value);

                    this.Playlist = new Playlist(_playlist_id, name, onlineCount);
                }
            }
            else if (line.StartsWith("[info]"))
            {
                // New json found, break & create a VideoInfo instance
                if ((m = _regexVideoJson.Match(line)).Success)
                {
                    _jsonPaths.Add(m.Groups[1].Value.Trim());
                }
            }
            else if (line.StartsWith("[download]"))
            {
                if ((m = _regexPlaylistIndex.Match(line)).Success)
                {
                    int i = -1;
                    if (int.TryParse(m.Groups[1].Value, out i))
                        _currentVideoPlaylistIndex = i;
                    else
                        throw new Exception($"PlaylistReader: Couldn't parse '{m.Groups[1].Value}' to integer for '{nameof(_currentVideoPlaylistIndex)}'");
                }
            }
            else if (line.StartsWith("[youtube]"))
            {
                if ((m = _regexVideoID.Match(line)).Success)
                {
                    _currentVideoID = m.Groups[1].Value;
                }
            }
        }

        public void ErrorReadLine(Process process, string line)
        {
            _jsonPaths.Add($"[ERROR:{_currentVideoID}] {line}");
        }

        public void Stop()
        {
            _youtubeDl.Kill();
            _cts.Cancel();

            this.Canceled = true;
        }

        public VideoInfo Next()
        {
            int attempts = 0;
            string jsonPath = null;
            VideoInfo video = null;

            while (!_processFinished)
            {
                if (_jsonPaths.Count > _index)
                {
                    jsonPath = _jsonPaths[_index];
                    _index++;
                    break;
                }
            }

            // If it's the end of the stream finish up the process.
            if (jsonPath == null)
            {
                if (!_youtubeDl.HasExited)
                    _youtubeDl.WaitForExit();

                return null;
            }

            // Sometimes youtube-dl is slower to create the json file, try a couple times
            while (attempts < 10)
            {
                if (_cts.IsCancellationRequested)
                    return null;

                if (jsonPath.StartsWith("[ERROR"))
                {
                    Match m = new Regex(@"\[ERROR:(.*)]\s(.*)").Match(jsonPath);
                    video = new VideoInfo();
                    video.ID = m.Groups[1].Value;
                    video.Failure = true;
                    video.FailureReason = m.Groups[2].Value;
                    break;
                }
                else
                {
                    try
                    {
                        video = new VideoInfo(jsonPath);
                        video.PlaylistIndex = _currentVideoPlaylistIndex;
                        break;
                    }
                    catch (IOException)
                    {
                        attempts++;
                        Thread.Sleep(100);
                    }
                }
            }

            if (video == null)
                throw new FileNotFoundException("File not found.", jsonPath);

            this.Playlist.Videos.Add(video);

            return video;
        }

        public Playlist WaitForPlaylist(int timeoutMS = 30000)
        {
            var sw = new Stopwatch();
            Exception exception = null;

            sw.Start();

            while (this.Playlist == null)
            {
                if (_cts.Token.IsCancellationRequested)
                    break;

                Thread.Sleep(50);

                if (sw.ElapsedMilliseconds > timeoutMS)
                {
                    exception = new TimeoutException("Couldn't get Playlist information.");
                    break;
                }
            }

            if (exception != null)
                throw exception;

            return this.Playlist;
        }
    }
}