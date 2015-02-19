using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ListViewEmbeddedControls;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Delegates;

namespace YouTube_Downloader.Operations
{
    public class PlaylistOperation : ListViewItem, IOperation, IDisposable
    {
        /// <summary>
        /// The amount of time to wait for progress updates in milliseconds.
        /// </summary>
        private const int ProgressDelay = 1000;
        private const int ProgressBarMarquee = 1;
        private const int ProgressBarContinuous = 2;
        private const int ResetProgressBar = 3;

        /// <summary>
        /// Gets the playlist url input.
        /// </summary>
        public string Input { get; private set; }
        /// <summary>
        /// Gets the output directory.
        /// </summary>
        public string Output { get; private set; }
        /// <summary>
        /// Gets the operation status.
        /// </summary>
        public OperationStatus Status { get; private set; }

        /// <summary>
        /// Occurs when the operation is complete.
        /// </summary>
        public event OperationEventHandler OperationComplete;

        bool combining, processing, remove, useDash;
        bool? downloaderSuccessful;
        Stopwatch sw;

        ~PlaylistOperation()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public PlaylistOperation()
        {
            // Temporary text.
            this.Text = "Getting playlist info...";

            // Fill sub items.
            this.SubItems.AddRange(new string[] { "", "", "", "", "" });
        }

        /// <summary>
        /// Returns whether the output can be opened.
        /// </summary>
        public bool CanOpen()
        {
            // There isn't a single output, so open is not supported.
            return false;
        }

        /// <summary>
        /// Returns whether the operation can be paused.
        /// </summary>
        public bool CanPause()
        {
            return !combining && downloader != null && downloader.CanPause;
        }

        /// <summary>
        /// Returns whether the operation can be resumed.
        /// </summary>
        public bool CanResume()
        {
            return !combining && downloader != null && downloader.CanResume;
        }

        /// <summary>
        /// Returns whether the operation can be stopped.
        /// </summary>
        public bool CanStop()
        {
            return !combining && downloader != null && downloader.CanStop;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                if (worker != null)
                {
                    worker.Dispose();
                    worker = null;
                }
                if (downloader != null)
                {
                    downloader.Dispose();
                    downloader = null;
                }
                OperationComplete = null;
            }
        }

        /// <summary>
        /// Starts the playlist download.
        /// </summary>
        /// <param name="url">The playlist url.</param>
        /// <param name="output">The output directory to save all videos.</param>
        /// <param name="dash">True to download DASH, false if not.</param>
        public void Download(string url, string output, bool dash)
        {
            this.Download(url, output, dash, null);
        }

        /// <summary>
        /// Starts the playlist download.
        /// </summary>
        /// <param name="url">The playlist url.</param>
        /// <param name="output">The output directory to save all videos.</param>
        /// <param name="dash">True to download DASH, false if not.</param>
        /// <param name="videos">Videos to download.</param>
        public void Download(string url, string output, bool dash, ICollection<VideoInfo> videos)
        {
            this.Input = url;
            this.Output = output;
            this.SubItems[5].Text = this.Input;

            useDash = dash;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(videos);

            this.Status = OperationStatus.Working;

            Program.RunningOperations.Add(this);
        }

        /// <summary>
        /// Not supported cause there is no single output.
        /// </summary>
        public bool Open()
        {
            throw new NotSupportedException("There is no single output.");
        }

        /// <summary>
        /// Opens the output directory.
        /// </summary>
        public bool OpenContainingFolder()
        {
            try
            {
                Process.Start(Path.GetDirectoryName(this.Output));
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pauses the operation.
        /// </summary>
        public void Pause()
        {
            // Only the downloader can be paused.
            if (downloader.CanPause)
            {
                downloader.Pause();
                this.Status = OperationStatus.Paused;
            }
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        public void Resume()
        {
            // Only the downloader can be resumed.
            if (downloader.CanResume)
            {
                downloader.Resume();
                this.Status = OperationStatus.Working;
            }
        }

        /// <summary>
        /// Stops the operation.
        /// </summary>
        /// <param name="remove">True to remove the operation from it's ListView.</param>
        /// <param name="cleanup">True to delete unfinished files.</param>
        public bool Stop(bool remove, bool cleanup)
        {
            this.remove = remove;

            if (downloader.CanStop)
            {
                downloader.Stop(cleanup);
                downloaderSuccessful = false;
            }

            if (worker.IsBusy)
                worker.CancelAsync();
            else
                this.Remove();

            this.Status = OperationStatus.Canceled;

            return true;
        }

        #region worker

        private BackgroundWorker worker;

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int count = 0;
                List<VideoInfo> videos = new List<VideoInfo>();

                if (e.Argument == null)
                {
                    PlaylistReader reader = new PlaylistReader(this.Input);
                    VideoInfo video;

                    while (!worker.CancellationPending && (video = reader.Next()) != null)
                    {
                        videos.Add(video);
                    }
                }
                else
                {
                    ICollection<VideoInfo> arg = e.Argument as ICollection<VideoInfo>;

                    videos.AddRange(arg);
                }

                foreach (VideoInfo video in videos)
                {
                    if (worker.CancellationPending)
                        break;

                    count++;

                    VideoFormat videoFormat = Helper.GetPreferedFormat(video, useDash);

                    this.SetText(string.Format("({0}/{1}) {2}", count, videos.Count, video.Title));
                    this.SetItemText(this.SubItems[3], Helper.FormatVideoLength(video.Duration));
                    this.SetItemText(this.SubItems[4], Helper.FormatFileSize(videoFormat.FileSize));

                    downloader = new FileDownloader();
                    downloader.Directory = this.Output;

                    DownloadFile[] fileInfos;

                    string finalFile = Path.Combine(this.Output, Helper.FormatTitle(videoFormat.VideoInfo.Title) + "." + videoFormat.Extension);

                    if (!useDash)
                    {
                        fileInfos = new DownloadFile[]
                        {
                            new DownloadFile(finalFile, videoFormat.DownloadUrl)
                        };
                    }
                    else
                    {
                        VideoFormat audioFormat = Helper.GetAudioFormat(videoFormat);
                        // Add '_audio' & '_video' to end of filename. Only get filename, not full path.
                        string audioFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_audio$2");
                        string videoFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_video$2");

                        fileInfos = new DownloadFile[]
                        {
                            new DownloadFile(audioFile, audioFormat.DownloadUrl),
                            new DownloadFile(videoFile, videoFormat.DownloadUrl)
                        };
                    }

                    downloader.Files.AddRange(fileInfos);

                    // Attach downloader events
                    downloader.Canceled += downloader_Canceled;
                    downloader.Completed += downloader_Completed;
                    downloader.FileDownloadFailed += downloader_FileDownloadFailed;
                    downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
                    downloader.ProgressChanged += downloader_ProgressChanged;

                    // Reset variable(s)
                    downloaderSuccessful = null;

                    downloader.Start();

                    // Wait for downloader to finish
                    while (downloader.IsBusy || downloader.IsPaused)
                        Thread.Sleep(200);

                    if (useDash && downloaderSuccessful == true)
                    {
                        this.SetItemText(this.SubItems[2], "Combining...");
                        worker.ReportProgress(ProgressBarMarquee);

                        if (this.Combine())
                        {
                            // Combined successfully
                        }
                        else
                        {
                            // Combining failed
                        }

                        worker.ReportProgress(ProgressBarContinuous);
                    }

                    // Reset ProgressBar before starting new download.
                    worker.ReportProgress(ResetProgressBar);
                }

                e.Cancel = worker.CancellationPending;
                e.Result = e.Cancel ? OperationStatus.Canceled : OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Program.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case ProgressBarContinuous:
                    this.GetProgressBar().Style = ProgressBarStyle.Continuous;
                    this.GetProgressBar().MarqueeAnimationSpeed = 0;
                    this.GetProgressBar().Value = this.GetProgressBar().Maximum;
                    break;
                case ProgressBarMarquee:
                    this.GetProgressBar().Value = this.GetProgressBar().Minimum;
                    this.GetProgressBar().Style = ProgressBarStyle.Marquee;
                    this.GetProgressBar().MarqueeAnimationSpeed = 30;
                    break;
                case ResetProgressBar:
                    this.GetProgressBar().Value = this.GetProgressBar().Minimum;
                    break;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.Status = OperationStatus.Canceled;
            }
            else
            {
                this.Status = (OperationStatus)e.Result;
            }

            this.RefreshStatus();
            this.GetProgressBar().Value = this.GetProgressBar().Maximum;

            Program.RunningOperations.Remove(this);

            OnOperationComplete(new OperationEventArgs(this, this.Status));

            if (this.remove && this.ListView != null)
            {
                this.Remove();
            }
        }

        #endregion

        #region downloader

        private FileDownloader downloader;

        private void downloader_Canceled(object sender, EventArgs e)
        {
            // Pass the event along to a almost identical event handler.
            downloader_Completed(sender, e);
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            sw.Stop();

            // If the download didn't fail & wasn't canceled it was most likely successful.
            if (downloaderSuccessful == null) downloaderSuccessful = true;
        }

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            /* If one or more files fail, whole operation failed. Might handle it more
             * elegantly in the future. */
            downloaderSuccessful = false;
            downloader.Stop(false);
        }

        private void downloader_CalculatedTotalFileSize(object sender, EventArgs e)
        {
            this.SetItemText(this.SubItems[4], Helper.FormatFileSize(downloader.TotalSize));
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (this.ListView.InvokeRequired)
                this.ListView.Invoke(new EventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    if (this.CanUpdateText())
                    {
                        string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.Speed);
                        long longETA = Helper.GetETA(downloader.Speed, downloader.TotalSize, downloader.TotalProgress);
                        string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

                        this.SubItems[1].Text = downloader.TotalPercentage() + " %";
                        this.SubItems[2].Text = speed + ETA;

                        sw.Restart();
                    }

                    this.GetProgressBar().Value = (int)downloader.TotalPercentage();

                    RefreshStatus();
                }
                catch { }
                finally
                {
                    processing = false;
                }
            }
        }

        #endregion

        private bool CanUpdateText()
        {
            if (sw == null)
                sw = new Stopwatch();

            if (!sw.IsRunning)
                sw.Restart();

            // Limit the progress update to once a second, to avoid flickering.
            return sw.ElapsedMilliseconds > ProgressDelay;
        }

        /// <summary>
        /// Combines DASH audio &amp; video, and returns true if it was successful.
        /// </summary>
        private bool Combine()
        {
            string audio = downloader.Files[0].Path;
            string video = downloader.Files[1].Path;
            // Remove '_video' from video file to get a final filename.
            string output = video.Replace("_video", string.Empty);

            combining = true;

            try
            {
                FFmpegHelper.CombineDash(video, audio, output);

                // Cleanup the extra files.
                Helper.DeleteFiles(audio, video);
            }
            catch (Exception ex)
            {
                Program.SaveException(ex);
                return false;
            }
            finally
            {
                combining = false;
            }

            return true;
        }

        /// <summary>
        /// Returns the operation's ProgressBar.
        /// </summary>
        private ProgressBar GetProgressBar()
        {
            return (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }

        private void RefreshStatus()
        {
            if (this.Status == OperationStatus.Success)
            {
                this.SubItems[2].Text = "Completed";
            }
            else if (this.Status == OperationStatus.Paused)
            {
                this.SubItems[2].Text = "Paused";
            }
            else if (this.Status == OperationStatus.Canceled)
            {
                this.SubItems[2].Text = "Canceled";
            }
        }

        private void SetText(string text)
        {
            if (this.ListView.InvokeRequired)
            {
                this.ListView.Invoke(new SetTextDelegate(SetText), text);
            }
            else
            {
                this.Text = text;
            }
        }

        private void SetItemText(ListViewSubItem item, string text)
        {
            if (this.ListView.InvokeRequired)
            {
                this.ListView.Invoke(new SetItemTextDelegate(SetItemText), item, text);
            }
            else
            {
                item.Text = text;
            }
        }
    }
}
