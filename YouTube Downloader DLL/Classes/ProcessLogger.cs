using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace YouTube_Downloader_DLL.Classes
{
    /// <summary>
    /// Wrapper for Process class to automatically log output and error streams to log file.
    /// </summary>
    public class ProcessLogger
    {
        public static List<ProcessLogger> ActiveLoggers = new List<ProcessLogger>();

        private Encoding _logEncoding = Encoding.UTF8;
        private StreamWriter _log;

        public string Header { get; set; }
        public string Footer { get; set; }
        public string LogFile { get; private set; }
        public Process Process { get; private set; }

        public ProcessLogger(Process process, string logFile)
        {
            this.LogFile = logFile;

            // Make sure the directory exists. File will be created by FileStream below
            if (!Directory.Exists(Path.GetDirectoryName(this.LogFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));

            _log = new StreamWriter(new FileStream(this.LogFile, FileMode.Append, FileAccess.Write))
            {
                AutoFlush = true
            };
            this.Process = process;
            this.Process.Exited += Process_Exited;

            ProcessLogger.ActiveLoggers.Add(this);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            this.Log(this.Footer);

            _log.Flush();
            _log.Close();

            ProcessLogger.ActiveLoggers.Remove(this);
        }

        public void Log(string line)
        {
            this.Log(line, string.Empty);
        }

        public void Log(string format, params string[] args)
        {
            if (_log == null)
                return;

            _log.WriteLine(string.Format(format, args));
        }

        public void StartProcess(Action<string> output, Action<string> error)
        {
            this.Log(this.Header);

            this.Process.OutputDataReceived += delegate (object process, DataReceivedEventArgs e)
            {
                if (e == null || string.IsNullOrEmpty(e.Data))
                    return;

                this.Log(e.Data);

                output?.Invoke(e.Data);
            };
            this.Process.ErrorDataReceived += delegate (object process, DataReceivedEventArgs e)
            {
                if (e == null || string.IsNullOrEmpty(e.Data))
                    return;

                this.Log(e.Data);

                error?.Invoke(e.Data);
            };
            this.Process.Start();
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
        }

        public static void KillAll()
        {
            ProcessLogger[] loggers = new ProcessLogger[ActiveLoggers.Count];
            ActiveLoggers.CopyTo(loggers);

            foreach (ProcessLogger pl in loggers)
            {
                pl.Process.Kill();
            }
        }
    }
}
