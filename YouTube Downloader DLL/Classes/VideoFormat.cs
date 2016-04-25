using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YouTube_Downloader_DLL.Enums;

namespace YouTube_Downloader_DLL.Classes
{
    public class VideoFormat : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the audio bit rate. Returns -1 if not defined.
        /// </summary>
        public int AudioBitRate { get; private set; }

        /// <summary>
        /// Gets whether the format is audio only.
        /// </summary>
        public bool AudioOnly { get; private set; }

        /// <summary>
        /// Gets the download url.
        /// </summary>
        public string DownloadUrl { get; private set; }

        /// <summary>
        /// Gets the file extension, excluding the period.
        /// </summary>
        public string Extension { get; private set; }

        long _fileSize;
        /// <summary>
        /// Gets the file size as bytes count.
        /// </summary>
        public long FileSize
        {
            get { return _fileSize; }
            private set
            {
                _fileSize = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the format text.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// Gets the format ID.
        /// </summary>
        public string FormatID { get; private set; }

        /// <summary>
        /// Gets the format type.
        /// </summary>
        public FormatType FormatType { get; private set; }

        /// <summary>
        /// Gets the frames per second. Null if not defined.
        /// </summary>
        public string FPS { get; private set; }

        /// <summary>
        /// Gets the format title, displaying some basic information.
        /// </summary>
        public string Title { get { return this.ToString(); } }

        /// <summary>
        /// Gets the associated VideoInfo.
        /// </summary>
        public VideoInfo VideoInfo { get; private set; }

        public VideoFormat(VideoInfo videoInfo, JToken token)
        {
            this.AudioBitRate = -1;
            this.VideoInfo = videoInfo;

            this.DeserializeJson(token);
        }

        #region Update file size

        private WebRequest request;
        private CancellationTokenSource cts;

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

        /// <summary>
        /// Starts a WebRequest to update the file size.
        /// </summary>
        public async void UpdateFileSizeAsync()
        {
            if (this.FileSize > 0 || VideoInfo.VideoSource == VideoSource.Twitch)
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

                if (cts != null)
                    cts.Dispose();
                cts = null;
            }
        }

        #endregion

        private void DeserializeJson(JToken token)
        {
            JToken format_note = token.SelectToken("format_note");

            if (format_note == null)
            {
                this.FormatType = FormatType.Normal;
            }
            else
            {
                if (format_note.ToString().ToLower().Contains("nondash"))
                {
                    this.FormatType = FormatType.NonDASH;
                }
                else if (format_note.ToString().ToLower().Contains("dash"))
                {
                    this.FormatType = FormatType.DASH;
                }
            }

            this.DownloadUrl = token["url"].ToString();
            this.Extension = token["ext"].ToString();
            this.Format = token["format"].ToString();
            this.FormatID = token["format_id"].ToString();

            // Check if format is audio only
            this.AudioOnly = this.Format.Contains("audio only");

            // Check for abr token (audio bit rate?)
            JToken abr = token.SelectToken("abr");

            if (abr != null)
                this.AudioBitRate = int.Parse(abr.ToString());

            // Check for filesize token
            JToken filesize = token.SelectToken("filesize");

            if (filesize != null && !string.IsNullOrEmpty(filesize.ToString()))
                this.FileSize = long.Parse(filesize.ToString());

            // Check for 60fps videos. If there is no 'fps' token, default to 30fps.
            JToken fps = token.SelectToken("fps", false);

            this.FPS = fps == null ? "30" : fps.ToString();
            this.UpdateFileSizeAsync();
        }

        public override string ToString()
        {
            string text = string.Empty;

            if (this.AudioOnly)
            {
                text = string.Format("DASH Audio - {0} kbps (.{1})", this.AudioBitRate, this.Extension);
            }
            else
            {
                // Only split by dash with whitespace around
                string[] dash_split = new string[] { " - " };
                string format = this.Format.Split(dash_split, StringSplitOptions.None)[1].Trim();

                if (this.FormatType != FormatType.Normal)
                {
                    format = format.Replace("(DASH video)", string.Empty).Trim();

                    text = string.Format("DASH Video - {0} (.{1})", format, this.Extension);
                }
                else
                {
                    text = string.Format("{0} (.{1})", format, this.Extension);
                }

                if (this.FPS != "30")
                    text = Regex.Replace(text, @"^(\d+x\d+)(\s.*)$", "$1x" + FPS + "$2");
            }

            if (this.FormatType == FormatType.NonDASH)
                text = "non-" + text;

            return text;
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.OnPropertyChangedExplicit(propertyName);
        }

        public void OnPropertyChangedExplicit(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
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
