using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.FFmpeg;

// ToDo: Catch errors from Process

namespace YouTube_Downloader_DLL.Classes
{
    public class FFmpegHelper
    {
        public static class Commands
        {
            public const string CombineDash = " -report -y -i \"{0}\" -i \"{1}\" -vcodec copy -acodec libvo_aacenc \"{2}\"";
            /* Convert options:
             *
             * -y  - Overwrite output file without asking
             * -i  - Input file name
             * -vn - Disables video recording.
             * -f  - Forces file format, but isn't needed if output has .mp3 extensions
             * -ab - Sets the audio bitrate
             */
            public const string Convert = " -report -y -i \"{0}\" -vn -f mp3 -ab {1}k \"{2}\"";
            public const string CropFrom = " -report -y -ss {0} -i \"{1}\" -acodec copy{2} \"{3}\"";
            public const string CropFromTo = " -report -y -ss {0} -i \"{1}\" -to {2} -acodec copy{3} \"{4}\"";
            public const string GetFileInfo = " -report -i \"{0}\"";
            public const string Version = " -version";
        }

        private const string LogFilename = "ffmpeg-{0}.log";
        private const string RegexFindReportFile = "Report written to \"(.*)\"";
        private const string ReportFile = "ffreport-%t.log";

        /// <summary>
        /// Gets the path to FFmpeg executable.
        /// </summary>
        public static string FFmpegPath = Path.Combine(Application.StartupPath, "Externals", "ffmpeg.exe");

        /// <summary>
        /// Returns true if given file can be converted to a MP3 file, false otherwise.
        /// </summary>
        /// <param name="file">The file to check.</param>
        public static FFmpegResult<bool> CanConvertToMP3(string file)
        {
            bool hasAudioStream = false;
            string line = string.Empty;
            StringBuilder lines = new StringBuilder();
            var process = CreateProcess(string.Format(Commands.GetFileInfo, file));

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                if (line.StartsWith("Stream #") && line.Contains("Audio"))
                {
                    // File has audio stream
                    hasAudioStream = true;
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<bool>(process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(hasAudioStream);
        }

        /// <summary>
        /// Returns true if given audio &amp; video file can be combined, false otherwise.
        /// </summary>
        /// <param name="audio">The input audio file.</param>
        /// <param name="video">The input video file.</param>
        public static FFmpegResult<bool> CanCombine(string audio, string video)
        {
            List<string> errors = new List<string>();

            errors.AddRange(CanCombineAudio(string.Format(Commands.GetFileInfo, audio)));
            errors.AddRange(CanCombineVideo(string.Format(Commands.GetFileInfo, video)));

            if (errors.Count == 0)
                return new FFmpegResult<bool>(true);
            else
                return new FFmpegResult<bool>(false, 1, errors);
        }

        /// <summary>
        /// The audio check portion of 'CheckCombine' function.
        /// </summary>
        /// <param name="cmd_args">The arguments used for creating the Process.</param>
        public static IEnumerable<string> CanCombineAudio(string cmd_args)
        {
            bool hasAudio = false;
            string line = string.Empty;
            List<string> errors = new List<string>();
            var process = CreateProcess(cmd_args);

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("major_brand")) // check for dash
                {
                    string value = line.Split(':')[1].Trim();

                    if (!value.Contains("dash"))
                    {
                        errors.Add("Audio doesn't appear to be a DASH file. Non-critical.");
                    }
                }
                else if (line.StartsWith("Stream #")) // check audio stream
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

            process.WaitForExit();

            if (!hasAudio)
            {
                errors.Add("Audio file doesn't have an audio stream.");
            }

            return errors;
        }

        /// <summary>
        /// The video check portion of 'CheckCombine' function.
        /// </summary>
        /// <param name="cmd_args">The arguments used for creating the Process.</param>
        public static IEnumerable<string> CanCombineVideo(string cmd_args)
        {
            bool hasVideo = false;
            string line = string.Empty;
            List<string> errors = new List<string>();
            var process = CreateProcess(cmd_args);

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("major_brand")) // check for dash
                {
                    string value = line.Split(':')[1].Trim();

                    if (!value.Contains("dash"))
                    {
                        errors.Add("Video doesn't appear to be a DASH file. Non-critical.");
                    }
                }
                else if (line.StartsWith("Stream #")) // check video stream
                {
                    if (line.Contains("Video"))
                    {
                        hasVideo = true;
                    }
                    else if (line.Contains("Audio"))
                    {
                        errors.Add("Video file also has an audio stream.");
                    }
                }
            }

            process.WaitForExit();

            if (!hasVideo)
            {
                errors.Add("Video file doesn't have a video stream.");
            }

            return errors;
        }

        /// <summary>
        /// Combines DASH audio &amp; video to a single MP4 file.
        /// </summary>
        /// <param name="video">The input video file.</param>
        /// <param name="audio">The input audio file.</param>
        /// <param name="output">Where to save the output file.</param>
        public static FFmpegResult<bool> CombineDash(string video, string audio, string output)
        {
            string[] argsInfo = new string[] { video, audio, output };
            string processArgs = string.Format(Commands.CombineDash, argsInfo);

            var process = FFmpegHelper.CreateProcess(processArgs);

            process.Start();

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line.Trim());
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Converts file to MP3.
        /// Possibly more formats in the future.
        /// </summary>
        /// <param name="reportProgress">The method to call when there is progress. Can be null.</param>
        /// <param name="input">The input file.</param>
        /// <param name="output">Where to save the output file.</param>
        public static FFmpegResult<bool> Convert(Action<int, object> reportProgress, string input, string output)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            string[] argsInfo = new string[] { input, GetBitRate(input).Value.ToString(), output };
            string processArgs = string.Format(Commands.Convert, argsInfo);

            var process = FFmpegHelper.CreateProcess(processArgs);

            if (reportProgress != null)
                reportProgress.Invoke(0, process);

            bool started = false;
            double milliseconds = 0;

            process.Start();

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                // If reportProgress is null it can't be invoked. So skip code below
                if (reportProgress == null)
                    continue;

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

                    reportProgress.Invoke(0, null);
                }
                else if (started && line.StartsWith("size="))
                {
                    int start = line.IndexOf("time=") + 5;
                    int length = "00:00:00.00".Length;

                    string time = line.Substring(start, length);

                    double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                    double percentage = (currentMilli / milliseconds) * 100;

                    reportProgress.Invoke(System.Convert.ToInt32(percentage), null);
                }
                else if (started && line == string.Empty)
                {
                    started = false;

                    reportProgress.Invoke(100, null);
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Creates a Process with the given arguments, then returns it after it has started.
        /// </summary>
        /// <param name="arguments">The process arguments.</param>
        public static ProcessLogger CreateProcess(string arguments, bool noLog = false, [CallerMemberName] string caller = "")
        {
            var psi = new ProcessStartInfo(FFmpegHelper.FFmpegPath, arguments)
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Common.GetLogsDirectory()
            };
            psi.EnvironmentVariables.Add("FFREPORT", string.Format("file={0}:level=8", ReportFile));

            string filename = string.Format(LogFilename, DateTime.Now.ToString("yyyyMMdd-HHmmss-ff"));
            string fullpath = Path.Combine(Common.GetLogsDirectory(), "ffmpeg", filename);
            ProcessLogger process = null;

            if (noLog)
                process = new ProcessLogger();
            else
                process = new ProcessLogger(fullpath)
                {
                    Header = BuildLogHeader(arguments, caller),
                    Footer = BuildLogFooter()
                };

            process.StartInfo = psi;

            return process;
        }

        /// <summary>
        /// Crops file from given start position to end.
        /// </summary>
        /// <param name="reportProgress">The method to call when there is progress. Can be null.</param>
        /// <param name="input">The input file.</param>
        /// <param name="output">Where to save the output file.</param>
        /// <param name="start">The <see cref="System.TimeSpan"/> start position.</param>
        public static FFmpegResult<bool> Crop(Action<int, object> reportProgress, string input, string output, TimeSpan start)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            string[] argsInfo = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", start.Hours, start.Minutes, start.Seconds, start.Milliseconds),
                input,
                GetFileType(input).Value == FFmpegFileType.Video ? " -vcodec copy" : "",
                output
            };
            string processArgs = string.Format(Commands.CropFrom, argsInfo);

            var process = FFmpegHelper.CreateProcess(processArgs);

            if (reportProgress != null)
                reportProgress.Invoke(0, process);

            bool started = false;
            double milliseconds = 0;
            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                // If reportProgress is null it can't be invoked. So skip code below
                if (reportProgress == null)
                    continue;

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

                    reportProgress.Invoke(0, null);
                }
                else if (started && line.StartsWith("size="))
                {
                    int lineStart = line.IndexOf("time=") + 5;
                    int length = "00:00:00.00".Length;

                    string time = line.Substring(lineStart, length);

                    double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                    double percentage = (currentMilli / milliseconds) * 100;

                    reportProgress.Invoke(System.Convert.ToInt32(percentage), null);
                }
                else if (started && line == string.Empty)
                {
                    started = false;

                    reportProgress.Invoke(100, null);
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Crops file from given start position to given end position.
        /// </summary>
        /// <param name="reportProgress">The method to call when there is progress. Can be null.</param>
        /// <param name="input">The input file.</param>
        /// <param name="output">Where to save the output file.</param>
        /// <param name="start">The <see cref="System.TimeSpan"/> start position.</param>
        /// <param name="end">The <see cref="System.TimeSpan"/> end position.</param>
        public static FFmpegResult<bool> Crop(Action<int, object> reportProgress, string input, string output, TimeSpan start, TimeSpan end)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            TimeSpan length = new TimeSpan((long)Math.Abs(start.Ticks - end.Ticks));

            string[] argsInfo = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", start.Hours, start.Minutes, start.Seconds, start.Milliseconds),
                input,
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", length.Hours, length.Minutes, length.Seconds, length.Milliseconds),
                GetFileType(input).Value == FFmpegFileType.Video ? " -vcodec copy" : "",
                output
            };
            string processArgs = string.Format(Commands.CropFromTo, argsInfo);

            var process = FFmpegHelper.CreateProcess(processArgs);

            if (reportProgress != null)
                reportProgress.Invoke(0, process);

            bool started = false;
            double milliseconds = 0;
            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            if (reportProgress != null)
            {
                while ((line = process.ReadLineError()) != null)
                {
                    lines.AppendLine(line = line.Trim());

                    // If reportProgress is null it can't be invoked. So skip code below
                    if (reportProgress == null)
                        continue;

                    milliseconds = end.TotalMilliseconds;

                    if (line == "Press [q] to stop, [?] for help")
                    {
                        started = true;

                        reportProgress.Invoke(0, null);
                    }
                    else if (started && line.StartsWith("size="))
                    {
                        int lineStart = line.IndexOf("time=") + 5;
                        int lineLength = "00:00:00.00".Length;

                        string time = line.Substring(lineStart, lineLength);

                        double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                        double percentage = (currentMilli / milliseconds) * 100;

                        reportProgress.Invoke(System.Convert.ToInt32(percentage), null);
                    }
                    else if (started && line == string.Empty)
                    {
                        started = false;

                        reportProgress.Invoke(100, null);
                    }
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Returns the bit rate of the given file.
        /// </summary>
        public static FFmpegResult<int> GetBitRate(string file)
        {
            int result = -1;
            Regex regex = new Regex(@"^Stream\s#\d:\d.*\s(\d+)\skb/s.*$", RegexOptions.Compiled);
            var process = CreateProcess(string.Format(Commands.GetFileInfo, file));

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                if (line.StartsWith("Stream"))
                {
                    Match m = regex.Match(line);

                    if (m.Success)
                        result = int.Parse(m.Groups[1].Value);
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<int>(process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<int>(result);
        }

        /// <summary>
        /// Returns the <see cref="System.TimeSpan"/> duration of the given file.
        /// </summary>
        /// <param name="file">The file to get <see cref="System.TimeSpan"/> duration from.</param>
        public static FFmpegResult<TimeSpan> GetDuration(string file)
        {
            TimeSpan result = TimeSpan.Zero;
            var process = CreateProcess(string.Format(Commands.GetFileInfo, file));

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                // Example line, including whitespace:
                //  Duration: 00:00:00.00, start: 0.000000, bitrate: *** kb/s
                if (line.StartsWith("Duration"))
                {
                    string[] split = line.Split(' ', ',');

                    result = TimeSpan.Parse(split[1]);
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<TimeSpan>(process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<TimeSpan>(result);
        }

        /// <summary>
        /// Returns the <see cref="FileType"/> of the given file.
        /// </summary>
        /// <param name="file">The file to get <see cref="FileType"/> from.</param>
        public static FFmpegResult<FFmpegFileType> GetFileType(string file)
        {
            FFmpegFileType result = FFmpegFileType.Error;
            var process = CreateProcess(string.Format(Commands.GetFileInfo, file));

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line = line.Trim());

                // Example lines, including whitespace:
                //    Stream #0:0(und): Video: h264 ([33][0][0][0] / 0x0021), yuv420p, 320x240 [SAR 717:716 DAR 239:179], q=2-31, 242 kb/s, 29.01 fps, 90k tbn, 90k tbc (default)
                //    Stream #0:1(eng): Audio: vorbis ([221][0][0][0] / 0x00DD), 44100 Hz, stereo (default)
                if (line.StartsWith("Stream #"))
                {
                    if (line.Contains("Video: "))
                    {
                        // File contains video stream, so it's a video file, possibly without audio.
                        result = FFmpegFileType.Video;
                    }
                    else if (line.Contains("Audio: "))
                    {
                        // File contains audio stream. Keep looking for a video stream,
                        // and if found it's probably a video file, or an audio file if not.
                        result = FFmpegFileType.Audio;
                    }
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<FFmpegFileType>(FFmpegFileType.Error, process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<FFmpegFileType>(result);
        }

        /// <summary>
        /// Returns list of errors, if any, from given FFmpeg report file.
        /// </summary>
        /// <param name="filename">The report file to check.</param>
        private static IEnumerable<string> CheckForErrors(string filename)
        {
            var errors = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                string line = string.Empty;

                // Skip first 2 lines
                reader.ReadLine();
                reader.ReadLine();

                while ((line = reader.ReadLine()) != null)
                {
                    errors.Add(line);
                }
            }

            return errors;
        }

        /// <summary>
        /// Find where the FFmpeg report file is from the output using Regex.
        /// </summary>
        private static string FindReportFile(string lines)
        {
            Match m;
            if ((m = Regex.Match(lines, RegexFindReportFile)).Success)
                return Path.Combine(Common.GetLogsDirectory(), m.Groups[1].Value);

            return string.Empty;
        }

        /// <summary>
        /// Gets current ffmpeg version.
        /// </summary>
        private static FFmpegResult<string> GetVersion()
        {
            string version = string.Empty;
            Regex regex = new Regex("^ffmpeg version (.*) Copyright.*$", RegexOptions.Compiled);
            var process = CreateProcess(Commands.Version);

            string line = string.Empty;
            StringBuilder lines = new StringBuilder();

            process.Start();

            while ((line = process.ReadLineError()) != null)
            {
                lines.AppendLine(line);

                Match match = regex.Match(line);

                if (match.Success)
                {
                    version = match.Groups[1].Value.Trim();
                }
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<string>(process.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<string>(version);
        }

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        private static string BuildLogFooter()
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            return Environment.NewLine;
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        /// <param name="arguments">The arguments to log in header.</param>
        private static string BuildLogHeader(string arguments, string caller)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[" + DateTime.Now + "]");
            sb.AppendLine("caller: " + caller);
            sb.AppendLine("cmd: " + arguments);
            sb.AppendLine();
            sb.AppendLine("OUTPUT");

            return sb.ToString();
        }
    }
}
