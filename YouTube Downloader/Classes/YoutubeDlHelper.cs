using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class YoutubeDlHelper
    {
        public const string Cmd_JSON_Info = " -o \"{0}\\%(title)s\" --no-playlist --skip-download --restrict-filenames --write-info-json \"{1}\"";
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

            return new VideoInfo(json_file);
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
            process.StartInfo.FileName = YoutubeDlHelper.YouTubeDlPath;
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
