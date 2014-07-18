using System.Collections.Generic;
using System.Windows.Forms;

namespace YouTube_Downloader.Classes
{
    public class VideoInfo
    {
        public long Duration { get; set; }
        public IList<VideoFormat> Formats { get; set; }
        public string FullTitle { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Url { get; set; }

        public event FileSizeUpdateHandler FileSizeUpdated;

        public VideoInfo()
        {
            this.Formats = new List<VideoFormat>();
        }

        /// <summary>
        /// Aborts all the requests for file size for each video format.
        /// </summary>
        public void AbortUpdateFileSizes()
        {
            foreach (VideoFormat format in this.Formats)
            {
                format.AbortUpdateFileSize();
            }
        }

        internal void OnFileSizeUpdated(VideoFormat videoFormat)
        {
            if (this.FileSizeUpdated != null)
                this.FileSizeUpdated(this, new FileSizeUpdateEventArgs(videoFormat));
        }
    }
}
