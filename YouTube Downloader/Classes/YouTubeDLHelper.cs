using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class YouTubeDLHelper
    {
        public const string Cmd_JSON_Info = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --write-info-json \"{1}\"";
        public const string Cmd_JSON_Info_Playlist = " -i -o \"{0}\\playlist-{1}\\%(playlist_index)s-%(title)s\" --restrict-filenames --skip-download --write-info-json \"{2}\"";

        public static string YouTubeDlPath = Path.Combine(Application.StartupPath, "externals", "youtube-dl.exe");

        public static StreamWriter CreateLogWriter()
        {
            string folder = Program.GetLogsDirectory();

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            StreamWriter writer = new StreamWriter(Path.Combine(folder, "youtube-dl.log"), true)
            {
                AutoFlush = true
            };

            return writer;
        }

        public static VideoInfo GetVideoInfo(string url)
        {
            string json_dir = Program.GetJsonDirectory();

            if (!Directory.Exists(json_dir))
                Directory.CreateDirectory(json_dir);

            /* Fill in json directory & video url. */
            string arguments = string.Format(Cmd_JSON_Info, json_dir, url);

            Process process = StartProcess(arguments);

            string json_file = "";
            string line = "";

            /* Write output to log. */
            using (var writer = CreateLogWriter())
            {
                WriteLogHeader(writer, arguments, url);

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    writer.WriteLine(line);

                    line = line.Trim();

                    if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                    {
                        /* Store file path. */
                        json_file = line.Substring(line.IndexOf(":") + 1).Trim();
                    }
                }

                WriteLogFooter(writer);
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return VideoInfo.DeserializeJson(json_file);
        }

        public static Playlist GetPlaylist(string url)
        {
            string json_dir = Program.GetJsonDirectory();

            if (!Directory.Exists(json_dir))
                Directory.CreateDirectory(json_dir);

            string playlist_id = Helper.GetPlaylistId(url);
            string arguments = string.Format(Cmd_JSON_Info_Playlist, json_dir, playlist_id, url);

            Process process = StartProcess(arguments);

            List<string> jsonFiles = new List<string>();
            string line = string.Empty;

            /* Playlist properties. */
            string name = "";
            int onlineCount = 0;

            /* Write output to log. */
            using (var writer = CreateLogWriter())
            {
                WriteLogHeader(writer, arguments, url);

                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    writer.WriteLine(line);

                    line = line.Trim();

                    /* Store file path. */
                    if (line.StartsWith("[info] Writing video description metadata as JSON to:"))
                    {
                        string file = line.Substring(line.IndexOf(":") + 1).Trim();

                        jsonFiles.Add(Path.Combine(Application.StartupPath, file));
                    }
                    /* Get the playlist count, so  */
                    else if (line.StartsWith("[youtube:playlist] playlist"))
                    {
                        string txt = line.Split(':')[2].Trim();
                        /* Get the count. */
                        string pattern = "^Collected (\\d+) video ids \\(downloading \\d+ of them\\)$";

                        /* Get name between '[youtube:playlist] playlist ' & next ':'. */
                        name = Regex.Match(line, "\\[youtube:playlist] playlist (.*):").Groups[1].Value;
                        onlineCount = int.Parse(Regex.Match(txt, pattern).Groups[1].Value);
                    }
                }

                WriteLogFooter(writer);
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            var videos = new List<VideoInfo>();

            foreach (string json_file in jsonFiles)
            {
                /* Parse JSON */
                VideoInfo info = new VideoInfo();
                string json = File.ReadAllText(json_file);
                JObject jObject = JObject.Parse(json);

                info.Duration = long.Parse(jObject["duration"].ToString());
                info.Title = jObject["fulltitle"].ToString();

                string displayId = jObject["display_id"].ToString();

                info.ThumbnailUrl = string.Format("https://i.ytimg.com/vi/{0}/mqdefault.jpg", displayId);
                info.Url = url;

                JArray array = (JArray)jObject["formats"];

                foreach (JToken token in array)
                {
                    VideoFormat format = new VideoFormat(info);

                    format.DownloadUrl = token["url"].ToString();
                    format.Extension = token["ext"].ToString();
                    format.Format = token["format"].ToString();
                    format.UpdateFileSizeAsync();

                    info.Formats.Add(format);
                }

                videos.Add(info);
            }

            return new Playlist(playlist_id, name, onlineCount, videos);
        }

        /// <summary>
        /// Creates a Process with the given arguments, then returns it after it has started.
        /// </summary>
        public static Process StartProcess(string arguments)
        {
            Process process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = YouTubeDLHelper.YouTubeDlPath;
            process.StartInfo.Arguments = arguments;
            process.Start();

            return process;
        }

        public static void WriteLogFooter(StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine();
        }

        public static void WriteLogHeader(StreamWriter writer, string arguments, string url)
        {
            /* Log header. */
            writer.WriteLine("[" + DateTime.Now + "]");
            writer.WriteLine("url: " + url);
            writer.WriteLine("cmd: " + arguments);
            writer.WriteLine();
            writer.WriteLine("OUTPUT");
        }
    }
}
