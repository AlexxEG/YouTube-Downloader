using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class FfmpegHelper
    {
        private const string Command_Convert = " -y -i \"{0}\" -vn -f mp3 -ab 192k \"{1}\"";
        private const string Command_Crop_From = " -y -ss {0} -i \"{1}\" -acodec copy \"{2}\"";
        private const string Command_Crop_From_To = " -y -ss {0} -i \"{1}\" -to {2} -acodec copy \"{3}\"";

        public static void ConvertToMP3(string input, string output)
        {
            bool deleteInput = false;

            if (input == output)
            {
                string dest = Path.Combine(Path.GetDirectoryName(input), System.Guid.NewGuid().ToString());
                dest += Path.GetExtension(input);

                File.Move(input, dest);

                input = dest;
                deleteInput = true;
            }

            string[] args = new string[] { input, output };
            string arguments = string.Format(FfmpegHelper.Command_Convert, args);

            FfmpegHelper.StartProcess(arguments);

            if (deleteInput)
            {
                MainForm.DeleteFile(input);
            }
        }

        public static void CutMP3(string input, string output, string start)
        {
            bool deleteInput = false;

            if (input == output)
            {
                string dest = Path.Combine(Path.GetDirectoryName(input), System.Guid.NewGuid().ToString());
                dest += Path.GetExtension(input);

                File.Move(input, dest);

                input = dest;
                deleteInput = true;
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

            if (deleteInput)
            {
                MainForm.DeleteFile(input);
            }
        }

        public static void CutMP3(string input, string output, string start, string end)
        {
            bool deleteInput = false;

            if (input == output)
            {
                string dest = Path.Combine(Path.GetDirectoryName(input), System.Guid.NewGuid().ToString());
                dest += Path.GetExtension(input);

                File.Move(input, dest);

                input = dest;
                deleteInput = true;
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

            if (deleteInput)
            {
                MainForm.DeleteFile(input);
            }
        }

        public static TimeSpan GetDuration(string input)
        {
            TimeSpan result = TimeSpan.Zero;
            string arguments = string.Format(" -i \"{0}\"", input);

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
