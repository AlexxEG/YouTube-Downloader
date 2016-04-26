using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace YouTube_Downloader_DLL.Classes
{
    public class PlaylistReader
    {
        public const string CmdJSONInfoPlaylist = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json \"{2}\"";

        int _index = 0;

        bool _processFinished = false;

        string _arguments;
        string _playlist_id;
        string _url;

        List<string> _jsonPaths = new List<string>();

        Regex _regexPlaylistInfo = new Regex(@"^\[youtube:playlist\] playlist (.*):.*Downloading\s+(\d+)\s+.*$", RegexOptions.Compiled);
        Regex _regexVideoJson = new Regex(@"^\[info\].*JSON.*:\s(.*)$", RegexOptions.Compiled);
        ProcessLogger _youtubeDl;

        public Playlist Playlist { get; set; }

        public PlaylistReader(string url)
        {
            string json_dir = Common.GetJsonDirectory();

            _playlist_id = Helper.GetPlaylistId(url);
            _arguments = string.Format(CmdJSONInfoPlaylist, json_dir, _playlist_id, url);
            _url = url;

            _youtubeDl = YoutubeDlHelper.CreateLogger(_arguments);
            _youtubeDl.Header = YoutubeDlHelper.BuildLogHeader(_arguments, "PlaylistReader(string url)");
            _youtubeDl.Footer = YoutubeDlHelper.BuildLogFooter();
            _youtubeDl.Process.Exited += delegate { _processFinished = true; };
            _youtubeDl.StartProcess(OutputReadLine, ErrorReadLine);
        }

        public void OutputReadLine(string line)
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

        public void ErrorReadLine(string line)
        {

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
                }
            }

            // If it's the end of the stream finish up the process.
            if (jsonPath == null)
            {
                if (!_youtubeDl.Process.HasExited)
                    _youtubeDl.Process.WaitForExit();

                return null;
            }

            // Sometimes youtube-dl is slower to create the json file, try a couple times
            while (attempts < 10)
            {
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
    }
}