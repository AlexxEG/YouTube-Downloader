using System;

namespace YouTube_Downloader_DLL.Classes
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
