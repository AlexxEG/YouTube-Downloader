using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader_DLL.FFmpeg
{
    public class FFmpegHelper
    {
        public static class Commands
        {
            public const string Combine = " -report -y -i \"{0}\" -i \"{1}\" -c:v copy -c:a copy \"{2}\"";
            /* Convert options:
             *
             * -y   - Overwrite output file without asking
             * -i   - Input file name
             * -vn  - Disables video recording.
             * -f   - Forces file format, but isn't needed if output has .mp3 extensions
             * -b:a - Sets the audio bitrate. Output bitrate will not match exact.
             */
            public const string Convert = " -report -y -i \"{0}\" -vn -f mp3 -b:a {1}k \"{2}\"";
            public const string CropFrom = " -report -y -ss {0} -i \"{1}\" -acodec copy{2} \"{3}\"";
            public const string CropFromTo = " -report -y -ss {0} -i \"{1}\" -to {2} -acodec copy{3} \"{4}\"";
            public const string FixM3U8 = " -report -y -i \"{0}\" -c copy -f mp4 -bsf:a aac_adtstoasc \"{1}\"";
            public const string GetFileInfo = " -report -i \"{0}\"";
            public const string Version = " -version";
        }

        private const string RegexFindReportFile = "Report written to \"(.*)\"";
        private const string ReportFile = "ffreport-%t.log";

        /// <summary>
        /// Gets the path to FFmpeg executable.
        /// </summary>
        public static string FFmpegPath = Path.Combine(Application.StartupPath, "Externals", "ffmpeg.exe");

        /// <summary>
        /// Writes log footer to log.
        /// </summary>
        private static void LogFooter(OperationLogger logger)
        {
            // Write log footer to stream.
            // Possibly write elapsed time and/or error in future.
            logger?.LogLine(Environment.NewLine);
        }

        /// <summary>
        /// Writes log header to log.
        /// </summary>
        private static void LogHeader(OperationLogger logger,
                                      string arguments,
                                      [CallerMemberName]string caller = "")
        {
            logger?.LogLine($"[{DateTime.Now}]");
            logger?.LogLine($"function: {caller}");
            logger?.LogLine($"cmd: {arguments}");
            logger?.LogLine();
            logger?.LogLine("OUTPUT");
        }

        /// <summary>
        /// Returns true if given file can be converted to a MP3 file, false otherwise.
        /// </summary>
        /// <param name="file">The file to check.</param>
        public static FFmpegResult<bool> CanConvertToMP3(OperationLogger logger,
                                                         string file)
        {
            bool hasAudioStream = false;
            string arguments = string.Format(Commands.GetFileInfo, file);
            var lines = new StringBuilder();

            LogHeader(logger, arguments);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    lines.AppendLine(line = line.Trim());

                    if (line.StartsWith("Stream #") && line.Contains("Audio"))
                    {
                        // File has audio stream
                        hasAudioStream = true;
                    }
                }, logger);

            p.WaitForExit();
            LogFooter(logger);

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<bool>(p.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(hasAudioStream);
        }

        /// <summary>
        /// Combines separate audio &amp; video to a single MP4 file.
        /// </summary>
        /// <param name="video">The input video file.</param>
        /// <param name="audio">The input audio file.</param>
        /// <param name="output">Where to save the output file.</param>
        public static FFmpegResult<bool> Combine(OperationLogger logger,
                                                 string video,
                                                 string audio,
                                                 string output,
                                                 Action<int> reportProgress)
        {
            string arguments = string.Format(Commands.Combine, video, audio, output);
            var lines = new StringBuilder();

            bool started = false;
            double milliseconds = 0;

            LogHeader(logger, arguments);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    lines.AppendLine(line = line.Trim());

                    // If reportProgress is null it can't be invoked. So skip code below
                    if (reportProgress == null)
                        return;

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

                        reportProgress.Invoke(0);
                    }
                    else if (started && line.StartsWith("frame="))
                    {
                        int lineStart = line.IndexOf("time=") + 5;
                        int length = "00:00:00.00".Length;

                        string time = line.Substring(lineStart, length);

                        double currentMilli = TimeSpan.Parse(time).TotalMilliseconds;
                        double percentage = (currentMilli / milliseconds) * 100;

                        reportProgress.Invoke(System.Convert.ToInt32(percentage));
                    }
                    else if (started && line == string.Empty)
                    {
                        started = false;

                        reportProgress.Invoke(100);
                    }
                }, logger);

            p.WaitForExit();
            LogFooter(logger);

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, p.ExitCode, CheckForErrors(reportFile));
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
        public static FFmpegResult<bool> Convert(OperationLogger logger,
                                                 Action<int, object> reportProgress,
                                                 string input,
                                                 string output,
                                                 CancellationToken ct)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            var args = new string[]
            {
                input,
                GetBitRate(input).Value.ToString(),
                output
            };
            var arguments = string.Format(Commands.Convert, args);

            if (reportProgress != null)
                reportProgress.Invoke(0, null);

            bool canceled = false;
            bool started = false;
            double milliseconds = 0;
            StringBuilder lines = new StringBuilder();

            LogHeader(logger, arguments);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    // Queued lines might still fire even after canceling process, don't actually know
                    if (canceled)
                        return;

                    lines.AppendLine(line = line.Trim());

                    if (ct != null && ct.IsCancellationRequested)
                    {
                        process.StandardInput.WriteLine("q");
                        canceled = true;
                        return;
                    }

                    // If reportProgress is null it can't be invoked. So skip code below
                    if (reportProgress == null)
                        return;

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
                }, logger);

            p.WaitForExit();
            LogFooter(logger);

            if (canceled)
            {
                return new FFmpegResult<bool>(false);
            }
            else if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, p.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Crops file from given start position to end.
        /// </summary>
        /// <param name="reportProgress">The method to call when there is progress. Can be null.</param>
        /// <param name="input">The input file.</param>
        /// <param name="output">Where to save the output file.</param>
        /// <param name="start">The <see cref="System.TimeSpan"/> start position.</param>
        public static FFmpegResult<bool> Crop(OperationLogger logger,
                                              Action<int, object> reportProgress,
                                              string input,
                                              string output,
                                              TimeSpan start,
                                              CancellationToken ct)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            var args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", start.Hours, start.Minutes, start.Seconds, start.Milliseconds),
                input,
                GetFileType(input).Value == FFmpegFileType.Video ? " -vcodec copy" : "",
                output
            };
            var arguments = string.Format(Commands.CropFrom, args);

            if (reportProgress != null)
                reportProgress.Invoke(0, null);

            bool canceled = false;
            bool started = false;
            double milliseconds = 0;
            StringBuilder lines = new StringBuilder();

            LogHeader(logger, arguments);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    // Queued lines might still fire even after canceling process, don't actually know
                    if (canceled)
                        return;

                    lines.AppendLine(line = line.Trim());

                    if (ct != null && ct.IsCancellationRequested)
                    {
                        process.StandardInput.WriteLine("q");
                        canceled = true;
                        return;
                    }

                    // If reportProgress is null it can't be invoked. So skip code below
                    if (reportProgress == null)
                        return;

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
                }, logger);

            p.WaitForExit();
            LogFooter(logger);

            if (canceled)
            {
                return new FFmpegResult<bool>(false);
            }
            else if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, p.ExitCode, CheckForErrors(reportFile));
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
        public static FFmpegResult<bool> Crop(OperationLogger logger,
                                              Action<int, object> reportProgress,
                                              string input,
                                              string output,
                                              TimeSpan start,
                                              TimeSpan end,
                                              CancellationToken ct)
        {
            if (input == output)
            {
                throw new Exception("Input & output can't be the same.");
            }

            TimeSpan length = new TimeSpan((long)Math.Abs(start.Ticks - end.Ticks));

            string[] args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", start.Hours, start.Minutes, start.Seconds, start.Milliseconds),
                input,
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", length.Hours, length.Minutes, length.Seconds, length.Milliseconds),
                GetFileType(input).Value == FFmpegFileType.Video ? " -vcodec copy" : "",
                output
            };
            string arguments = string.Format(Commands.CropFromTo, args);

            if (reportProgress != null)
                reportProgress.Invoke(0, null);

            bool canceled = false;
            bool started = false;
            double milliseconds = 0;
            StringBuilder lines = new StringBuilder();
            Process p = null;

            LogHeader(logger, arguments);

            if (reportProgress == null)
            {
                p = Helper.StartProcess(FFmpegPath, arguments,
                    null,
                    delegate (Process process, string line)
                    {
                        // Queued lines might still fire even after canceling process, don't actually know
                        if (canceled)
                            return;

                        if (ct != null && ct.IsCancellationRequested)
                        {
                            process.StandardInput.WriteLine("q");
                            canceled = true;
                            return;
                        }
                    }, logger);
            }
            else
            {
                p = Helper.StartProcess(FFmpegPath, arguments,
                    null,
                    delegate (Process process, string line)
                    {
                        // Queued lines might still fire even after canceling process, don't actually know
                        if (canceled)
                            return;

                        lines.AppendLine(line = line.Trim());

                        if (ct != null && ct.IsCancellationRequested)
                        {
                            process.StandardInput.WriteLine("q");
                            canceled = true;
                            return;
                        }

                        // If reportProgress is null it can't be invoked. So skip code below
                        if (reportProgress == null)
                            return;

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
                    }, logger);
            }

            p.WaitForExit();
            LogFooter(logger);

            if (canceled)
            {
                return new FFmpegResult<bool>(false);
            }
            else if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, p.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Fixes and optimizes final .ts file from .m3u8 playlist.
        /// </summary>
        public static FFmpegResult<bool> FixM3U8(OperationLogger logger,
                                                 string input,
                                                 string output)
        {
            var lines = new StringBuilder();
            string arguments = string.Format(Commands.FixM3U8, input, output);

            LogHeader(logger, arguments);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    lines.Append(line);
                }, logger);

            p.WaitForExit();
            LogFooter(logger);

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<bool>(false, p.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<bool>(true);
        }

        /// <summary>
        /// Returns the bit rate of the given file.
        /// </summary>
        public static FFmpegResult<int> GetBitRate(string file)
        {
            int result = 128; // Default to 128k bitrate
            Regex regex = new Regex(@"^Stream\s#\d:\d.*\s(\d+)\skb\/s.*$", RegexOptions.Compiled);
            StringBuilder lines = new StringBuilder();
            var arguments = string.Format(Commands.GetFileInfo, file);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    lines.AppendLine(line = line.Trim());

                    if (line.StartsWith("Stream"))
                    {
                        Match m = regex.Match(line);

                        if (m.Success)
                            result = int.Parse(m.Groups[1].Value);
                    }
                }, null);

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<int>(p.ExitCode, CheckForErrors(reportFile));
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
            StringBuilder lines = new StringBuilder();
            var arguments = string.Format(Commands.GetFileInfo, file);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
                {
                    lines.AppendLine(line = line.Trim());

                    // Example line, including whitespace:
                    //  Duration: 00:00:00.00, start: 0.000000, bitrate: *** kb/s
                    if (line.StartsWith("Duration"))
                    {
                        string[] split = line.Split(' ', ',');

                        result = TimeSpan.Parse(split[1]);
                    }
                }, null);

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<TimeSpan>(p.ExitCode, CheckForErrors(reportFile));
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
            StringBuilder lines = new StringBuilder();
            var arguments = string.Format(Commands.GetFileInfo, file);

            var p = Helper.StartProcess(FFmpegPath, arguments,
                null,
                delegate (Process process, string line)
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
                }, null);

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());
                var errors = (List<string>)CheckForErrors(reportFile);

                if (errors[0] != "At least one output file must be specified")
                    return new FFmpegResult<FFmpegFileType>(FFmpegFileType.Error, p.ExitCode, CheckForErrors(reportFile));
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
            StringBuilder lines = new StringBuilder();

            var p = Helper.StartProcess(FFmpegPath, Commands.Version,
                null,
                delegate (Process process, string line)
                {
                    lines.AppendLine(line);

                    Match match = regex.Match(line);

                    if (match.Success)
                    {
                        version = match.Groups[1].Value.Trim();
                    }
                }, null);

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string reportFile = FindReportFile(lines.ToString());

                return new FFmpegResult<string>(p.ExitCode, CheckForErrors(reportFile));
            }

            return new FFmpegResult<string>(version);
        }
    }
}
