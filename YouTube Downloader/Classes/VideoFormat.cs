using System;
using System.Net;
using System.Threading;

namespace YouTube_Downloader.Classes
{
    public class VideoFormat
    {
        public bool DASH { get; set; }
        public string DownloadUrl { get; set; }
        public string Extension { get; set; }
        public long FileSize { get; set; }
        public string Format { get; set; }
        public VideoInfo VideoInfo { get; set; }

        private HttpWebRequest request;

        public VideoFormat(VideoInfo videoInfo)
        {
            this.VideoInfo = videoInfo;
        }

        /// <summary>
        /// Aborts request for file size.
        /// </summary>
        public void AbortUpdateFileSize()
        {
            if (request != null)
                request.Abort();

            if (updateFileSizeThread != null)
                updateFileSizeThread.Abort();
        }

        Thread updateFileSizeThread;

        public void UpdateFileSize()
        {
            updateFileSizeThread = new Thread(() =>
            {
                HttpWebResponse response = null;

                try
                {
                    request = (HttpWebRequest)HttpWebRequest.Create(this.DownloadUrl);
                    request.Method = "HEAD";
                    response = (HttpWebResponse)request.GetResponse();
                    long bytes = response.ContentLength;

                    this.FileSize = bytes;

                    this.VideoInfo.OnFileSizeUpdated(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Aborted update file size");
                }
                finally
                {
                    if (response != null)
                        response.Close();
                }
            });
            updateFileSizeThread.Start();
        }

        public override string ToString()
        {
            return this.Format.Split('-')[1].Trim() + " (." + this.Extension + ")";
        }
    }
}
