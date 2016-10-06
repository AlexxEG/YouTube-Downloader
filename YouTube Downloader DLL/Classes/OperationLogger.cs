using System;
using System.IO;
using System.Text;
using YouTube_Downloader_DLL.Operations;

namespace YouTube_Downloader_DLL.Classes
{
    public class OperationLogger : IDisposable
    {
        public const string FFmpegDLogFile = @"ffmpeg\ffmpeg-{0}.log";
        public const string YTDLogFile = @"youtube-dl\youtube-dl-{0}.log";

        bool _closed;
        Encoding _logEncoding = Encoding.UTF8;
        StreamWriter _log;

        public string LogFile { get; private set; }

        public OperationLogger(string logFile)
        {
            this.LogFile = logFile;

            // Make sure the directory exists. File will be created by FileStream below
            if (!Directory.Exists(Path.GetDirectoryName(this.LogFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(this.LogFile));

            _log = new StreamWriter(new FileStream(this.LogFile, FileMode.Create, FileAccess.ReadWrite))
            {
                AutoFlush = true
            };
        }

        public void Close()
        {
            _log.Flush();
            _log.Close();

            _closed = true;
        }

        public void Dispose()
        {
            if (!_closed)
                this.Close();

            ((IDisposable)this._log).Dispose();
        }

        public void Log(string line)
        {
            this.Log(line, string.Empty);
        }

        public void Log(string format, params string[] args)
        {
            if (string.IsNullOrEmpty(format))
                return;

            _log.WriteLine(string.Format(format, args));
        }

        public static OperationLogger Create(string logFilename)
        {
            string filename = string.Format(logFilename, DateTime.Now.ToString("yyyyMMdd-HHmmss-ff"));
            string fullpath = Path.Combine(Common.GetLogsDirectory(), filename);

            return new OperationLogger(fullpath);
        }
    }
}
