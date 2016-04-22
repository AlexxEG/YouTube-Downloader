using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YouTube_Downloader_DLL.Classes
{
    /// <summary>
    /// Wrapper for Process class to automatically log output and error streams to log file.
    /// </summary>
    public class ProcessLogger
    {
        public static List<ProcessLogger> ActiveLoggers = new List<ProcessLogger>();

        private bool _exited = false;
        private bool _finished = false;
        private Encoding _logEncoding = Encoding.UTF8;
        private Process _process;
        private StreamWriter _log;

        public int ExitCode
        {
            get { return _process.ExitCode; }
        }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string LogFile { get; private set; }
        public ProcessStartInfo StartInfo
        {
            get { return _process.StartInfo; }
            set { _process.StartInfo = value; }
        }

        public ProcessLogger()
        {
            _process = new Process();

            this.LogFile = null;
            _log = null;
        }

        public ProcessLogger(string logFile)
        {
            this.LogFile = logFile;

            _log = new StreamWriter(new FileStream(this.LogFile, FileMode.Append, FileAccess.Write))
            {
                AutoFlush = true
            };
            _process = new Process();

            ProcessLogger.ActiveLoggers.Add(this);
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

        public void Kill()
        {
            if (!_process.HasExited)
                _process.Kill();
        }

        /// <summary>
        /// Starts the Process and logs the output, if enabled.
        /// </summary>
        public void Start()
        {
            this.Log(this.Header);

            _process.Start();

            if (_log != null)
                this.WaitForExitAsync();
        }

        public void WaitForExit()
        {
            // Process has already exited, return immediately
            if (_process.HasExited)
                return;

            // If log is disabled just wait for Process normally, since we
            // don't have to wait for footer to be logged
            if (_log == null)
                _process.WaitForExit();

            // Wait for 'WaitForExitAsync' to set '_exited' to true,
            // which makes sure footer is written before closing stream
            while (!_exited)
                Thread.Sleep(200);
        }

        public string ReadLineError()
        {
            string line = _process.StandardError.ReadLine();

            if (line != null)
                this.Log(line);

            return line;
        }

        public string ReadLineOutput()
        {
            string line = _process.StandardOutput.ReadLine();

            if (!string.IsNullOrEmpty(line))
                this.Log(line);

            return line;
        }

        private void Finish()
        {
            if (_finished)
                return;

            _finished = true;

            this.Log(this.Footer);

            _log.Flush();
            _log.Close();

            _exited = true;

            ProcessLogger.ActiveLoggers.Remove(this);
        }

        private async void WaitForExitAsync()
        {
            await Task.Run(delegate
            {
                _process.WaitForExit();
            });

            this.Finish();
        }

        public static void KillAll()
        {
            ProcessLogger[] loggers = new ProcessLogger[ActiveLoggers.Count];
            ActiveLoggers.CopyTo(loggers);

            foreach (ProcessLogger pl in loggers)
            {
                pl.Kill();
                pl.Finish();
            }
        }
    }
}
