using System;

namespace YouTube_Downloader_DLL.Classes
{
    public class FileDownloadFailedEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public FileDownload FileDownload { get; private set; }

        public FileDownloadFailedEventArgs(Exception exception, FileDownload fileDownload)
        {
            this.Exception = exception;
            this.FileDownload = fileDownload;
        }
    }
}
