using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class YouTubeDLHelper
    {
        private const string Cmd_JSON_Info = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --write-info-json \"{1}\"";

        public static VideoInfo GetJSONInfo(string url)
        {
            string json_dir = Path.Combine(Application.StartupPath, "json");
            /* Fill in json directory & video url. */
            string arguments = string.Format(Cmd_JSON_Info, json_dir, url);

            Process process = StartProcess(arguments);

            string json_file = "";
            string line = "";

            /* Write output to log. */
            using (var writer = new StreamWriter(Path.Combine(Application.StartupPath, "youtube-dl.log"), true))
            {
                /* Log header. */
                writer.WriteLine();
                writer.WriteLine("[" + DateTime.Now + "]");
                writer.WriteLine("url: " + url);
                writer.WriteLine("cmd: " + arguments);
                writer.WriteLine("-");
                writer.WriteLine("OUTPUT");

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

                writer.WriteLine("END");
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            /* Parse JSON */
            VideoInfo info = new VideoInfo();
            string json = File.ReadAllText(json_file);
            JObject jObject = JObject.Parse(json);

            info.Duration = long.Parse(jObject["duration"].ToString());
            info.FullTitle = jObject["fulltitle"].ToString();
            info.ThumbnailUrl = jObject["thumbnail"].ToString();
            info.Url = url;

            JArray array = (JArray)jObject["formats"];

            foreach (JToken token in array)
            {
                VideoFormat format = new VideoFormat(info);

                format.DownloadUrl = token["url"].ToString();
                format.Extension = token["ext"].ToString();
                format.Format = token["format"].ToString();
                format.UpdateFileSize();

                info.Formats.Add(format);
            }

            return info;
        }

        /// <summary>
        /// Creates a Process with the given arguments, then returns it after it has started.
        /// </summary>
        private static Process StartProcess(string arguments)
        {
            Process process = new Process();

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = Application.StartupPath + "\\youtube-dl.exe";
            process.StartInfo.Arguments = arguments;
            process.Start();

            return process;
        }
    }

    public delegate void FileSizeUpdateHandler(object sender, FileSizeUpdateEventArgs e);

    public class FileSizeUpdateEventArgs : EventArgs
    {
        public VideoFormat VideoFormat { get; set; }

        public FileSizeUpdateEventArgs(VideoFormat videoFormat)
        {
            this.VideoFormat = videoFormat;
        }
    }
}
