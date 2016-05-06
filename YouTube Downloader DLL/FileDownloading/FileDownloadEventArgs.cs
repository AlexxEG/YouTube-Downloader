using System;

namespace YouTube_Downloader_DLL.FileDownloading
{
    public class FileDownloadEventArgs : EventArgs
    {
        public FileDownload FileDownload { get; private set; }

        public FileDownloadEventArgs(FileDownload fileDownload)
        {
            this.FileDownload = fileDownload;
        }
    }
}
