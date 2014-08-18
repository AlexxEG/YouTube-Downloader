using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public enum FileType { Audio, Error, Video }

    public class FFmpegHelper
    {
        private const string Cmd_Combine_Dash = " -y -i \"{0}\" -i \"{1}\" -vcodec copy -acodec copy \"{2}\"";
        private const string Cmd_Convert = " -y -i \"{0}\" -vn -f mp3 -ab 192k \"{1}\"";
        private const string Cmd_Crop_From = " -y -ss {0} -i \"{1}\" -acodec copy{2} \"{3}\"";
        private const string Cmd_Crop_From_To = " -y -ss {0} -i \"{1}\" -to {2} -acodec copy{3} \"{4}\"";
        private const string Cmd_Get_File_Info = " -i \"{0}\"";

        public static string FFmpegPath = Path.Combine(Application.StartupPath, "externals", "ffmpeg.exe");

        public static bool CanConvertMP3(string file)
        {
            string arguments = string.Format(Cmd_Get_File_Info, file);

            Process process = StartProcess(arguments);

            string line = "";
            bool hasAudioStream = false;

            /* Write output to log. */
            using (var writer = CreateLogWriter())
            {
                WriteHeader(writer, arguments);

                while ((line = process.StandardError.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                    line = line.Trim();

                    if (line.StartsWith("Stream #") && line.Contains("Audio"))
                    {
                        /* File has audio stream. */
                        hasAudioStream = true;
                    }
                }

                WriteEnd(writer);
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return hasAudioStream;
        }

        public static List<string> CheckCombine(string audio, string video)
        {
            List<string> errors = new List<string>();
            string argsAudio = string.Format(Cmd_Get_File_Info, audio);
            string argsVideo = string.Format(Cmd_Get_File_Info, video);

            Process process;

            using (var writer = CreateLogWriter())
            {
                WriteHeader(writer, argsAudio);

                using (process = StartProcess(argsAudio))
                {
                    string line = "";
                    bool hasAudio = false;

                    while ((line = process.StandardError.ReadLine()) != null)
                    {
                        writer.WriteLine(line);
                        line = line.Trim();

                        if (line.StartsWith("major_brand"))
                        {
                            string value = line.Split(':')[1].Trim();

                            if (!value.Contains("dash"))
                            {
                                errors.Add("Audio doesn't appear to be a DASH file. Non-critical.");
                            }
                        }
                        else if (line.StartsWith("Stream #"))
                        {
                            if (line.Contains("Audio"))
                            {
                                hasAudio = true;
                            }
                            else if (line.Contains("Video"))
                            {
                                errors.Add("Audio file also has a video stream.");
                            }
                        }
                    }

                    if (!hasAudio)
                    {
                        errors.Add("Audio file doesn't audio.");
                    }
                }

                WriteEnd(writer);
            }

            return errors;
        }

        private static StreamWriter CreateLogWriter()
        {
            string folder = Path.Combine(Program.GetLocalAppDataFolder(), "logs");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            StreamWriter writer = new StreamWriter(Path.Combine(folder, "ffmpeg.log"), true)
            {
                AutoFlush = true
            };

            return writer;
        }

        public static void CombineDash(string video, string audio, string output)
        {
            string[] args = new string[] { video, audio, output };
            string arguments = string.Format(FFmpegHelper.Cmd_Combine_Dash, args);

            Process process = FFmpegHelper.StartProcess(arguments);

            string line = "";

            /* Write output to log. */
            using (var writer = CreateLogWriter())
            {
                WriteHeader(writer, arguments);

                while ((line = process.StandardError.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }

                WriteEnd(writer);
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();
        }

        public static void CombineDashThread(string video, string audio, string output)
        {
            new Thread(() =>
                {
                    if (video == output)
                    {
                        string dest = Path.Combine(
                            Path.GetDirectoryName(video),
                            Path.GetFileNameWithoutExtension(video)) + "_video." + Path.GetExtension(video);

                        File.Move(video, dest);

                        video = dest;
                    }

                    FFmpegHelper.CombineDash(video, audio, output);
                }).Start();
        }

        public static void Convert(BackgroundWorker bw, string input, string output)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            string[] args = new string[] { input, output };
            string arguments = string.Format(FFmpegHelper.Cmd_Convert, args);

            Process process = FFmpegHelper.StartProcess(arguments);

            bw.ReportProgress(0, process);

            bool started = false;
            double milliseconds = 0;
            string line = "";

            /* Write output to log. */
            using (var writer = CreateLogWriter())
            {
                WriteHeader(writer, arguments);

                while ((line = process.StandardError.ReadLine()) != null)
                {
                    writer.WriteLine(line);

                    // 'bw' is null, don't report any progress
                    if (bw != null && bw.WorkerReportsProgress)
                    {
                        line = line.Trim();

                        if (line.StartsWith("Duration: "))
                        {
                            int start = "Duration: ".Length;
                            int length = "00:00:00.00".Length;

                            string time = line.Substring(start, length);

                            milliseconds = TimeSpan.Parse(time).TotalMilliseconds;
                        }
                        else if (line == "Press [q] to stop, [?] for help")
                        {
                            started = true;

                            bw.ReportProgress(0);
                        }
                        else if (started && line.StartsWith("size="))
                        {
                            int start = line.IndexOf("time=") + 5;
                            int length = "00:00:00.00".Length;

                            string time = line.Substring(start, length);

                            double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                            double percentage = (currentMilli / milliseconds) * 100;

                            bw.ReportProgress(System.Convert.ToInt32(percentage));
                        }
                        else if (started && line == string.Empty)
                        {
                            started = false;

                            bw.ReportProgress(100);
                        }
                    }
                }

                WriteEnd(writer);
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();
        }

        public static void Crop(BackgroundWorker bw, string input, string output, string start)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            TimeSpan from = TimeSpan.Parse(start);

            string[] args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", from.Hours, from.Minutes, from.Seconds, from.Milliseconds),
                input,
                GetFileType(input) == FileType.Video ? " -vcodec copy" : "",
                output
            };

            string arguments = string.Format(Cmd_Crop_From, args);

            Process process = FFmpegHelper.StartProcess(arguments);

            bw.ReportProgress(0, process);

            bool started = false;
            double milliseconds = 0;
            string line = "";

            while ((line = process.StandardError.ReadLine()) != null)
            {
                // 'bw' is null, don't report any progress
                if (bw != null && bw.WorkerReportsProgress)
                {
                    line = line.Trim();

                    if (line.StartsWith("Duration: "))
                    {
                        int lineStart = "Duration: ".Length;
                        int length = "00:00:00.00".Length;

                        string time = line.Substring(lineStart, length);

                        milliseconds = TimeSpan.Parse(time).TotalMilliseconds;
                    }
                    else if (line == "Press [q] to stop, [?] for help")
                    {
                        started = true;

                        bw.ReportProgress(0);
                    }
                    else if (started && line.StartsWith("size="))
                    {
                        int lineStart = line.IndexOf("time=") + 5;
                        int length = "00:00:00.00".Length;

                        string time = line.Substring(lineStart, length);

                        double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                        double percentage = (currentMilli / milliseconds) * 100;

                        bw.ReportProgress(System.Convert.ToInt32(percentage));
                    }
                    else if (started && line == string.Empty)
                    {
                        started = false;

                        bw.ReportProgress(100);
                    }
                }
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();
        }

        public static void Crop(BackgroundWorker bw, string input, string output, string start, string end)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            TimeSpan from = TimeSpan.Parse(start);
            TimeSpan to = TimeSpan.Parse(end);
            TimeSpan length = new TimeSpan((long)Math.Abs(from.Ticks - to.Ticks));

            string[] args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", from.Hours, from.Minutes, from.Seconds, from.Milliseconds),
                input,
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", length.Hours, length.Minutes, length.Seconds, length.Milliseconds),
                GetFileType(input) == FileType.Video ? " -vcodec copy" : "",
                output
            };

            string arguments = string.Format(Cmd_Crop_From_To, args);

            Process process = FFmpegHelper.StartProcess(arguments);

            bw.ReportProgress(0, process);

            bool started = false;
            double milliseconds = 0;
            string line = "";

            while ((line = process.StandardError.ReadLine()) != null)
            {
                // 'bw' is null, don't report any progress
                if (bw != null && bw.WorkerReportsProgress)
                {
                    line = line.Trim();

                    milliseconds = TimeSpan.Parse(end).TotalMilliseconds;

                    if (line == "Press [q] to stop, [?] for help")
                    {
                        started = true;

                        bw.ReportProgress(0);
                    }
                    else if (started && line.StartsWith("size="))
                    {
                        int lineStart = line.IndexOf("time=") + 5;
                        int lineLength = "00:00:00.00".Length;

                        string time = line.Substring(lineStart, lineLength);

                        double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                        double percentage = (currentMilli / milliseconds) * 100;

                        bw.ReportProgress(System.Convert.ToInt32(percentage));
                    }
                    else if (started && line == string.Empty)
                    {
                        started = false;

                        bw.ReportProgress(100);
                    }
                }
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();
        }

        public static TimeSpan GetDuration(string input)
        {
            TimeSpan result = TimeSpan.Zero;
            string arguments = string.Format(" -i \"{0}\"", input);

            Process process = StartProcess(arguments);

            List<string> lines = new List<string>();

            while (!process.StandardError.EndOfStream)
            {
                lines.Add(process.StandardError.ReadLine().Trim());
            }

            foreach (var line in lines)
            {
                if (line.StartsWith("Duration"))
                {
                    string[] split = line.Split(' ', ',');

                    result = TimeSpan.Parse(split[1]);
                    break;
                }
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return result;
        }

        public static FileType GetFileType(string input)
        {
            FileType result = FileType.Error;
            string arguments = string.Format(" -i \"{0}\"", input);

            Process process = StartProcess(arguments);

            List<string> lines = new List<string>();

            while (!process.StandardError.EndOfStream)
            {
                lines.Add(process.StandardError.ReadLine().Trim());
            }

            foreach (var line in lines)
            {
                if (line.StartsWith("Stream #"))
                {
                    if (line.Contains("Video: "))
                    {
                        result = FileType.Video;
                        break;
                    }
                    else if (line.Contains("Audio: "))
                    {
                        // File contains audio stream, so if a video stream
                        // is not found it's a audio file.
                        result = FileType.Audio;
                    }
                }
            }

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return result;
        }

        /// <summary>
        /// Creates a Process with the given arguments, then returns
        /// it after it has started.
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
            process.StartInfo.FileName = FFmpegHelper.FFmpegPath;
            process.StartInfo.Arguments = arguments;
            process.Start();

            return process;
        }

        private static void WriteEnd(StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine();
        }

        private static void WriteHeader(StreamWriter writer, string arguments)
        {
            /* Log header. */
            writer.WriteLine("[" + DateTime.Now + "]");
            writer.WriteLine("cmd: " + arguments);
            writer.WriteLine();
            writer.WriteLine("OUTPUT");
        }
    }
}