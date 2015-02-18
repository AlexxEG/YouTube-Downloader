using System;

namespace YouTube_Downloader_WPF.Classes
{
    public delegate void FileSizeUpdateHandler(object sender, FileSizeUpdateEventArgs e);

    public class FileSizeUpdateEventArgs : EventArgs
    {
        public VideoFormat VideoFormat { get; set; }

        public FileSizeUpdateEventArgs(VideoFormat videoFormat)
        {
            this.VideoFormat = videoFormat;
        }
    }
}
