using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class FfmpegHelper
    {
        private const string Command_Convert = " -i \"{0}\" -vn -f mp3 -ab 192k \"{1}\"";
        private const string Command_Crop_From = " -ss {0} -i \"{1}\" -acodec copy \"{2}\"";
        private const string Command_Crop_From_To = " -ss {0} -i \"{1}\" -to {2} -acodec copy \"{3}\"";

        public static void ConvertToMP3(string input, string output)
        {
            string[] args = new string[] { input, output };
            string arguments = string.Format(FfmpegHelper.Command_Convert, args);

            FfmpegHelper.StartProcess(arguments);
        }

        public static void CutMP3(string input, string output, string start)
        {
            if (input == output) // Don't know if ffmpeg overwrites or not yet
            {
                // Add '_out' to the end of the file, before the extension.
                string folder = Path.GetDirectoryName(output);
                string filename = Path.GetFileNameWithoutExtension(output);
                string extension = Path.GetExtension(output);

                output = string.Format("{0}\\{1}_cut{2}", folder, filename, extension);
            }

            TimeSpan from = TimeSpan.Parse(start);

            string[] args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", from.Hours, from.Minutes, from.Seconds, from.Milliseconds),
                input,
                output
            };

            string arguments = string.Format(Command_Crop_From, args);

            FfmpegHelper.StartProcess(arguments);
        }

        public static void CutMP3(string input, string output, string start, string end)
        {
            if (input == output) // Don't know if ffmpeg overwrites or not yet
            {
                // Add '_out' to the end of the file, before the extension.
                string folder = Path.GetDirectoryName(output);
                string filename = Path.GetFileNameWithoutExtension(output);
                string extension = Path.GetExtension(output);

                output = string.Format("{0}\\{1}_cut{2}", folder, filename, extension);
            }

            TimeSpan from = TimeSpan.Parse(start);
            TimeSpan to = TimeSpan.Parse(end);
            TimeSpan length = new TimeSpan((long)Math.Abs(from.Ticks - to.Ticks));

            string[] args = new string[]
            {
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", from.Hours, from.Minutes, from.Seconds, from.Milliseconds),
                input,
                string.Format("{0:00}:{1:00}:{2:00}.{3:000}", length.Hours, length.Minutes, length.Seconds, length.Milliseconds),
                output
            };

            string arguments = string.Format(Command_Crop_From_To, args);

            FfmpegHelper.StartProcess(arguments);
        }

        private static string StartProcess(string arguments)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = Application.StartupPath + "\\ffmpeg";
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.StandardOutput.ReadToEnd();

            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return error;
        }
    }
}
