using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace YouTube_Downloader_DLL.Classes
{
    public class PlaylistReader
    {
        public const string Cmd_JSON_Info_Playlist = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json \"{2}\"";

        string _arguments;
        string _line;
        string _playlist_id;
        string _url;

        Regex _regexPlaylistInfo = new Regex(@"^\[youtube:playlist\] playlist (.*):.*Downloading\s+(\d+)\s+.*$", RegexOptions.Compiled);
        Regex _regexVideoJson = new Regex(@"^\[info\].*JSON.*:\s(.*)$", RegexOptions.Compiled);
        ProcessLogger _youtubeDl;

        public Playlist Playlist { get; set; }

        public PlaylistReader(string url)
        {
            string json_dir = Common.GetJsonDirectory();

            _playlist_id = Helper.GetPlaylistId(url);
            _arguments = string.Format(Cmd_JSON_Info_Playlist, json_dir, _playlist_id, url);
            _url = url;

            _youtubeDl = YoutubeDlHelper.CreateProcess(_arguments);
            _youtubeDl.Header = YoutubeDlHelper.BuildLogHeader(_arguments);
            _youtubeDl.Footer = YoutubeDlHelper.BuildLogFooter();
            _youtubeDl.Start();

            ReadPlaylistInfo();
        }

        public VideoInfo Next()
        {
            int attempts = 0;
            string json_path = string.Empty;
            VideoInfo video = null;

            while ((_line = _youtubeDl.ReadLineOutput()) != null)
            {
                Match m;

                // New json found, break & create a VideoInfo instance
                if ((m = _regexVideoJson.Match(_line)).Success)
                {
                    json_path = m.Groups[1].Value.Trim();
                    break;
                }
            }

            // If it's the end of the stream finish up the process.
            if (_line == null)
            {
                _youtubeDl.WaitForExit();

                return null;
            }

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

        private void ReadPlaylistInfo()
        {
            int onlineCount = 0;
            string name = string.Empty,
                   line = string.Empty;

            while ((line = _youtubeDl.ReadLineOutput()) != null)
            {
                Match m;

                if ((m = _regexPlaylistInfo.Match(line)).Success)
                {
                    // Get the playlist info
                    name = m.Groups[1].Value;
                    onlineCount = int.Parse(m.Groups[2].Value);
                    break;
                }
            }

            this.Playlist = new Playlist(_playlist_id, name, onlineCount);
        }
    }
}