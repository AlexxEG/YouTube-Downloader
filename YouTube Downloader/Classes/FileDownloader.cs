using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace YouTube_Downloader
{
    public enum DownloadStatus { None, Downloading, Paused, Success, Failed, Canceled }

    /// <summary>
    /// Downloads and resumes files from HTTP, FTP, and File (file://) URLS
    /// </summary>
    public class FileDownloader : AbortableBackgroundWorker
    {
        // Block size to download is by default 1K.
        static int DownloadBlockSize = 1024 * 200;
        static long SecondTicks = TimeSpan.FromSeconds(1).Ticks;

        private string downloadingTo;
        private FileStream fileStream;
        private bool _pause;
        private long totalDownloaded;

        public string FileUrl { get; private set; }
        public string DestFolder { get; private set; }
        public string DestFileName { get; private set; }
        /// <summary>
        /// Gets the current DownloadStatus
        /// </summary>
        public DownloadStatus DownloadStatus { get; private set; }
        /// <summary>
        /// Gets the current DownloadData
        /// </summary>
        public DownloadData DownloadData { get; private set; }
        /// <summary>
        /// Gets the current DownloadSpeed
        /// </summary>
        public int DownloadSpeed { get; private set; }
        /// <summary>
        /// Gets the estimate time to finish downloading, the time is in seconds
        /// </summary>
        public long ETA
        {
            get
            {
                if (DownloadData == null || DownloadSpeed == 0)
                    return 0;

                long remainBytes = DownloadData.FileSize - totalDownloaded;

                return remainBytes / DownloadSpeed;
            }
        }
        public int Progress { get; private set; }

        public FileDownloader(string fileUrl, string destFolder, string destFileName)
        {
            this.FileUrl = fileUrl;
            this.DestFolder = destFolder;
            this.DestFileName = destFileName;
            this.DoWork += Download;
        }

        /// <summary>
        /// Make the download to Pause
        /// </summary>
        public void Pause()
        {
            _pause = true;
        }

        /// <summary>
        /// Make the download to resume
        /// </summary>
        public void Resume()
        {
            _pause = false;
        }

        /// <summary>
        /// Begin downloading the file at the specified url, and save it to the given folder.
        /// </summary>
        private void Download(object sender, DoWorkEventArgs e)
        {
            _pause = false;
            DownloadStatus = DownloadStatus.Downloading;
            OnProgressChanged(new ProgressChangedEventArgs(Progress, null));
            DownloadData = DownloadData.Create(FileUrl, DestFolder, this.DestFileName, null);

            if (string.IsNullOrEmpty(DestFileName))
                Path.GetFileName(DownloadData.Response.ResponseUri.ToString());

            this.downloadingTo = Path.Combine(DestFolder, DestFileName);
            FileMode mode = DownloadData.StartPoint > 0 ? FileMode.Append : FileMode.OpenOrCreate;
            fileStream = File.Open(downloadingTo, mode, FileAccess.Write);
            byte[] buffer = new byte[DownloadBlockSize];
            totalDownloaded = DownloadData.StartPoint;
            double totalDownloadedInTime = 0; long totalDownloadedTime = 0;
            OnProgressChanged(new ProgressChangedEventArgs(Progress, null));
            bool callProgess = true;

            while (true)
            {
                callProgess = true;

                if (this.CancellationPending)
                {
                    DownloadSpeed = Progress = 0;
                    e.Cancel = true;
                    break;
                }

                if (_pause)
                {
                    DownloadSpeed = 0;
                    DownloadStatus = DownloadStatus.Paused;
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    DownloadStatus = DownloadStatus.Downloading;
                    long startTime = DateTime.Now.Ticks;
                    int readCount = DownloadData.DownloadStream.Read(buffer, 0, DownloadBlockSize);

                    if (readCount == 0)
                        break;

                    totalDownloadedInTime += readCount;
                    totalDownloadedTime += DateTime.Now.Ticks - startTime;

                    if (callProgess = totalDownloadedTime >= SecondTicks)
                    {
                        DownloadSpeed = (int)(totalDownloadedInTime / TimeSpan.FromTicks(totalDownloadedTime).TotalSeconds);
                        totalDownloadedInTime = 0; totalDownloadedTime = 0;
                    }

                    totalDownloaded += readCount;
                    fileStream.Write(buffer, 0, readCount);
                    fileStream.Flush();
                }

                Progress = (int)(100.0 * totalDownloaded / DownloadData.FileSize);

                if (callProgess && DownloadData.IsProgressKnown)
                    ReportProgress(Progress);
            }

            ReportProgress(Progress);
        }

        protected override void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (DownloadData != null)
                    DownloadData.Close();
            }
            catch { }

            try
            {
                if (fileStream != null)
                    fileStream.Close();
            }
            catch { }

            if (e.Cancelled)
                DownloadStatus = DownloadStatus.Canceled;
            else if (e.Error != null)
                DownloadStatus = DownloadStatus.Failed;
            else
                DownloadStatus = DownloadStatus.Success;

            DownloadSpeed = 0;

            base.OnRunWorkerCompleted(e);
        }
    }

    /// <summary>
    /// Constains the connection to the file server and other statistics about a file
    /// that's downloading.
    /// </summary>
    public class DownloadData
    {
        private IWebProxy proxy = null;
        private WebResponse response;
        private Stream stream;
        private long size;
        private long start;

        public WebResponse Response
        {
            get { return response; }
            set { response = value; }
        }
        public Stream DownloadStream
        {
            get
            {
                if (this.start == this.size)
                    return Stream.Null;
                if (this.stream == null)
                    this.stream = this.response.GetResponseStream();
                return this.stream;
            }
        }
        public long FileSize
        {
            get
            {
                return this.size;
            }
        }
        public long StartPoint
        {
            get
            {
                return this.start;
            }
        }
        public bool IsProgressKnown
        {
            get
            {
                // If the size of the remote url is -1, that means we
                // couldn't determine it, and so we don't know
                // progress information.
                return this.size > -1;
            }
        }

        // Used by the factory method
        private DownloadData()
        {
        }

        private DownloadData(WebResponse response, long size, long start)
        {
            this.response = response;
            this.size = size;
            this.start = start;
            this.stream = null;
        }

        public void Close()
        {
            this.response.Close();
        }

        public static DownloadData Create(string url, string destFolder, string fileName, IWebProxy proxy)
        {

            // This is what we will return
            DownloadData downloadData = new DownloadData();
            downloadData.proxy = proxy;

            long urlSize = downloadData.GetFileSize(url);
            downloadData.size = urlSize;

            WebRequest req = downloadData.GetRequest(url);

            try
            {
                downloadData.response = (WebResponse)req.GetResponse();
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("Error downloading \"{0}\": {1}", url, e.Message), e);
            }

            // Check to make sure the response isn't an error. If it is this method
            // will throw exceptions.
            ValidateResponse(downloadData.response, url);

            string downloadTo = Path.Combine(destFolder, fileName);

            // If we don't know how big the file is supposed to be,
            // we can't resume, so delete what we already have if something is on disk already.
            if (!downloadData.IsProgressKnown && File.Exists(downloadTo))
                File.Delete(downloadTo);

            if (downloadData.IsProgressKnown && File.Exists(downloadTo))
            {
                // We only support resuming on http requests
                if (!(downloadData.Response is HttpWebResponse))
                {
                    File.Delete(downloadTo);
                }
                else
                {
                    // Try and start where the file on disk left off
                    downloadData.start = new FileInfo(downloadTo).Length;

                    // If we have a file that's bigger than what is online, then something 
                    // strange happened. Delete it and start again.
                    if (downloadData.start > urlSize)
                        File.Delete(downloadTo);
                    else if (downloadData.start < urlSize)
                    {
                        // Try and resume by creating a new request with a new start position
                        downloadData.response.Close();
                        req = downloadData.GetRequest(url);
                        ((HttpWebRequest)req).AddRange((int)downloadData.start);
                        downloadData.response = req.GetResponse();

                        if (((HttpWebResponse)downloadData.Response).StatusCode != HttpStatusCode.PartialContent)
                        {
                            // They didn't support our resume request. 
                            File.Delete(downloadTo);
                            downloadData.start = 0;
                        }
                    }
                }
            }

            return downloadData;
        }

        /// <summary>
        /// Checks the file size of a remote file. If size is -1, then the file size
        /// could not be determined.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="progressKnown"></param>
        /// <returns></returns>
        public long GetFileSize(string url)
        {
            WebResponse response = null;
            long size = -1;

            try
            {
                response = GetRequest(url).GetResponse();
                size = response.ContentLength;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return size;
        }

        private WebRequest GetRequest(string url)
        {
            //WebProxy proxy = WebProxy.GetDefaultProxy();
            WebRequest request = WebRequest.Create(url);

            request.Proxy = this.proxy;

            return request;
        }

        /// <summary>
        /// Checks whether a WebResponse is an error.
        /// </summary>
        /// <param name="response"></param>
        private static void ValidateResponse(WebResponse response, string url)
        {
            if (response is HttpWebResponse)
            {
                HttpWebResponse httpResponse = (HttpWebResponse)response;
                // If it's an HTML page, it's probably an error page. Comment this
                // out to enable downloading of HTML pages.
                if (httpResponse.ContentType.Contains("text/html") || httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    string text = "Could not download \"{0}\" - a web page was returned from the web server.";

                    throw new ArgumentException(string.Format(text, url));
                }
            }
            else if (response is FtpWebResponse)
            {
                FtpWebResponse ftpResponse = (FtpWebResponse)response;

                if (ftpResponse.StatusCode == FtpStatusCode.ConnectionClosed)
                {
                    string text = "Could not download \"{0}\" - FTP server closed the connection.";

                    throw new ArgumentException(string.Format(text, url));
                }
            }
            // FileWebResponse doesn't have a status code to check.
        }
    }
}