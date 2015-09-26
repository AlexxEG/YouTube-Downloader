using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace YouTube_Downloader_DLL.Classes
{
    public class PlaylistReader
    {
        public const string Cmd_JSON_Info_Playlist = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json \"{2}\"";

        int _currentIndex = -1;
        bool _working = false;

        string _arguments;
        string _playlist_id;
        string _url;

        List<string> _jsonFiles = new List<string>();
        Regex _regexPlaylistInfo = new Regex(@"^\[youtube:playlist\] playlist (.*):.*downloading\s+(\d+)\s+.*$", RegexOptions.Compiled);
        Regex _regexVideoJson = new Regex(@"^\[info\].*JSON.*:\s(.*)$", RegexOptions.Compiled);
        ProcessLogger _youtubeDl;

        public Playlist Playlist { get; set; }

        public PlaylistReader(string url)
        {
            _playlist_id = Helper.GetPlaylistId(url);

            string json_dir = Common.GetJsonDirectory();

            _arguments = string.Format(Cmd_JSON_Info_Playlist, json_dir, _playlist_id, url);
            _url = url;

            _youtubeDl = YoutubeDlHelper.CreateProcess(_arguments);
            _youtubeDl.NewLineOutput += youtubeDl_NewLineOutput;
            _youtubeDl.Start();
            _youtubeDl.WaitForExitAsync(delegate
            {
                _working = false;
            });

            _working = true;
        }

        private void youtubeDl_NewLineOutput(string line)
        {
            Match m;

            if ((m = _regexVideoJson.Match(line)).Success)
            {
                // New json found
                if ((m = Regex.Match(line, @"^\[info\].*JSON.*:\s(.*)$")).Success)
                {
                    string file = m.Groups[1].Value.Trim();

                    // Store all the json files in List
                    _jsonFiles.Add(file);
                }
            }
            else if ((m = _regexPlaylistInfo.Match(line)).Success)
            {
                // Get the playlist info
                string name = m.Groups[1].Value;
                int onlineCount = int.Parse(m.Groups[2].Value);

                this.Playlist = new Playlist(_playlist_id, name, onlineCount);
            }
        }

        public VideoInfo Next()
        {
            // Last item already handled, start returning null
            if (!_working && _currentIndex == _jsonFiles.Count - 1)
                return null;

            _currentIndex++;

            // Might be /r/softwaregore worthy
        tryAgain:
            if (_currentIndex == _jsonFiles.Count)
                if (!_working)
                    return null;
                else
                    goto tryAgain;

            string json_path = _jsonFiles[_currentIndex];
            int attempts = 0;
            VideoInfo video = null;

            // Sometimes youtube-dl is slower to create the json file, try a couple times
            while (attempts < 10)
            {
                try
                {
                    video = new VideoInfo(json_path);
                    break;
                }
                catch (IOException)
                {
                    attempts++;
                    System.Threading.Thread.Sleep(100);
                }
            }

            if (video == null)
                throw new FileNotFoundException("File not found.", json_path);

            this.Playlist.Videos.Add(video);

            return video;
        }
    }
}