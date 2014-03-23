using System.Diagnostics;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class FfmpegHelper
    {
        public static string ConvertToMP3(string input, string output)
        {
            string arguments = string.Format(" -i \"{0}\" -vn -f mp3 -ab 192k \"{1}\"", input, output);

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
