using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace YouTube_Downloader.Classes
{
    public class VideoFormat
    {
        public bool DASH { get; set; }
        public string DownloadUrl { get; set; }
        public string Extension { get; set; }
        public long FileSize { get; set; }
        public string Format { get; set; }
        public string FPS { get; set; }
        public VideoInfo VideoInfo { get; set; }

        private WebRequest request;
        private CancellationTokenSource cts;

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

            if (cts != null)
                cts.Cancel();
        }

        public async void UpdateFileSizeAsync()
        {
            if (this.FileSize > 0)
            {
                // Probably already got the file size from .json file.
                this.VideoInfo.OnFileSizeUpdated(this);
                return;
            }

            WebResponse response = null;

            cts = new CancellationTokenSource();

            try
            {
                request = WebRequest.Create(this.DownloadUrl);
                request.Method = "HEAD";
                response = await request.GetResponseAsync(cts.Token);

                long bytes = response.ContentLength;

                this.FileSize = bytes;

                this.VideoInfo.OnFileSizeUpdated(this);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Canceled update file size");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Update file size error");
            }
            finally
            {
                if (response != null)
                    response.Close();

                cts.Dispose();
                cts = null;
            }
        }

        public override string ToString()
        {
            string text = string.Format("{0} (.{1})", Format.Split('-')[1].Trim(), this.Extension);

            if (FPS != "30")
                text = Regex.Replace(text, @"^(\d+x\d+)(\s.*)$", "$1x" + FPS + "$2");

            return text;
        }
    }

    // Source: http://stackoverflow.com/a/19215782
    static class Extensions
    {
        /// <summary>
        /// Same as WebRequest.GetResponseAsync, but supports CancellationToken.
        /// </summary>
        public static async Task<WebResponse> GetResponseAsync(this WebRequest request, CancellationToken ct)
        {
            using (ct.Register(() => request.Abort(), useSynchronizationContext: false))
            {
                try
                {
                    var response = await request.GetResponseAsync();
                    ct.ThrowIfCancellationRequested();
                    return (WebResponse)response;
                }
                catch (WebException ex)
                {
                    // WebException is thrown when request.Abort() is called,
                    // but there may be many other reasons,
                    // propagate the WebException to the caller correctly

                    if (ct.IsCancellationRequested)
                    {
                        // the WebException will be available as Exception.InnerException
                        throw new OperationCanceledException(ex.Message, ex, ct);
                    }

                    // cancellation hasn't been requested, rethrow the original WebException
                    throw;
                }
            }
        }
    }
}
