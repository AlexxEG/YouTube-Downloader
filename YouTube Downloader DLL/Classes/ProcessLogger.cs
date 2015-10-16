using System;
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
        private Encoding _logEncoding = Encoding.UTF8;
        private Process _process;
        private StringBuilder _log;

        public string Header { get; set; }
        public string Footer { get; set; }
        public string LogFile { get; set; }
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
            _log = new StringBuilder();
            _process = new Process();

            this.LogFile = logFile;
        }

        public void Log(string line)
        {
            this.Log(line, string.Empty);
        }

        public void Log(string format, params string[] args)
        {
            if (_log == null)
                return;

            _log.AppendLine(string.Format(format, args));
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
            _process.WaitForExit();
        }

        public string ReadLineError()
        {
            string line = _process.StandardError.ReadLine();

            if (!string.IsNullOrEmpty(line))
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

        /// <summary>
        /// Invokes the callback when process is done.
        /// </summary>
        public async void WaitForExitAsync(Action callback)
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                _process.WaitForExit();
            });
            callback.Invoke();
        }

        private async void WaitForExitAsync()
        {
            await System.Threading.Tasks.Task.Run(delegate
            {
                _process.WaitForExit();
            });

            this.Log(this.Footer);

            using (var logger = new FileStream(this.LogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] bytes = _logEncoding.GetBytes(_log.ToString());

                logger.Write(bytes, 0, bytes.Length);
                logger.Flush();
            }
        }
    }
}
