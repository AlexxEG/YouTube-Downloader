using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader_DLL.Helpers;

namespace YouTube_Downloader_DLL.Classes
{
    public class PlaylistReader
    {
        public const string CmdPlaylistInfo = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json{2} \"{3}\"";
        public const string CmdPlaylistRange = " --playlist-items {0}";

        int _index = 0;

        bool _processFinished = false;

        string _arguments;
        string _playlist_id;
        string _url;

        List<string> _jsonPaths = new List<string>();

        Regex _regexPlaylistInfo = new Regex(@"^\[youtube:playlist\] playlist (.*):.*Downloading\s+(\d+)\s+.*$", RegexOptions.Compiled);
        Regex _regexVideoJson = new Regex(@"^\[info\].*JSON.*:\s(.*)$", RegexOptions.Compiled);
        Process _youtubeDl;
        OperationLogger _logger;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public bool Canceled { get; set; } = false;
        public Playlist Playlist { get; set; } = null;

        public PlaylistReader(string url, int[] videos)
        {
            string json_dir = Common.GetJsonDirectory();

            _playlist_id = Helper.GetPlaylistId(url);

            string range = string.Empty;

            if (videos != null && videos.Length > 0)
            {
                var items = new StringBuilder(videos[0].ToString());

                for (int i = 1; i < videos.Length; i++)
                    items.Append("," + videos[i]);

                range = string.Format(CmdPlaylistRange, items);
            }

            _arguments = string.Format(CmdPlaylistInfo, json_dir, _playlist_id, range, url);
            _url = url;

            _logger = OperationLogger.Create(OperationLogger.YTDLogFile);

            _youtubeDl = YoutubeDlHelper.StartProcess(_logger,
                _arguments,
                "PlaylistReader(string url, int[] videos)",
                OutputReadLine,
                ErrorReadLine);
            _youtubeDl.Exited += delegate { _processFinished = true; };
        }

        public void OutputReadLine(Process process, string line)
        {
            Match m;

            if ((m = _regexPlaylistInfo.Match(line)).Success)
            {
                // Get the playlist info
                string name = m.Groups[1].Value;
                int onlineCount = int.Parse(m.Groups[2].Value);

                this.Playlist = new Playlist(_playlist_id, name, onlineCount);
            }
            // New json found, break & create a VideoInfo instance
            else if ((m = _regexVideoJson.Match(line)).Success)
            {
                _jsonPaths.Add(m.Groups[1].Value.Trim());
            }
        }

        public void ErrorReadLine(Process process, string line)
        {

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

                try
                {
                    video = new VideoInfo(jsonPath);
                    break;
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(100);
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