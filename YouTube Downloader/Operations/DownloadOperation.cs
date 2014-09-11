using DeDauwJeroen;
using ListViewEmbeddedControls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Delegates;

namespace YouTube_Downloader.Operations
{
    public class DownloadOperation : ListViewItem, IOperation, IDisposable
    {
        /// <summary>
        /// The amount of time to wait for progress updates in milliseconds.
        /// </summary>
        private const int ProgressDelay = 1000;

        /// <summary>
        /// Gets the video download url input.
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

        bool dash, processing, remove;
        Stopwatch sw;

        public DownloadOperation(string text)
            : base(text)
        {
            sw = new Stopwatch();
            sw.Start();

            this.Status = OperationStatus.None;
        }

        ~DownloadOperation()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        /// <summary>
        /// Returns whether the output can be opened.
        /// </summary>
        public bool CanOpen()
        {
            return this.Status == OperationStatus.Success;
        }

        /// <summary>
        /// Returns whether the operation can be paused.
        /// </summary>
        public bool CanPause()
        {
            /* Only downloader can pause. */
            return downloader.CanPause && this.Status == OperationStatus.Working;
        }

        /// <summary>
        /// Returns whether the operation can be resumed.
        /// </summary>
        public bool CanResume()
        {
            /* Only downloader can resume. */
            return downloader.CanResume && this.Status == OperationStatus.Paused;
        }

        /// <summary>
        /// Returns whether the operation can be stopped.
        /// </summary>
        public bool CanStop()
        {
            /* Only downloader can stop. */
            return combiner == null && downloader.CanStop && (this.Status == OperationStatus.Working || this.Status == OperationStatus.Paused);
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
                if (downloader != null)
                {
                    downloader.Dispose();
                    downloader = null;
                }
                if (combiner != null)
                {
                    combiner.Dispose();
                    combiner = null;
                }
                OperationComplete = null;
            }
        }

        /// <summary>
        /// Starts the video download.
        /// </summary>
        /// <param name="url">The video download url.</param>
        /// <param name="output">The output file to save video.</param>
        public void Download(string url, string output)
        {
            this.Input = url;
            this.Output = output;

            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();

            downloader = new FileDownloader(true);
            downloader.LocalDirectory = folder;

            FileDownloader.FileInfo fileInfo = new FileDownloader.FileInfo(url);

            /* Give proper filename to downloaded file. */
            fileInfo.Name = file;

            downloader.Files.Add(fileInfo);

            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.FileSizesCalculationComplete += downloader_FileSizesCalculationComplete;
            downloader.ProgressChanged += downloader_ProgressChanged;

            downloader.Start();

            this.Status = OperationStatus.Working;

            Program.RunningOperations.Add(this);
        }

        /// <summary>
        /// Starts the DASH video download.
        /// </summary>
        /// <param name="audio">The audio download url.</param>
        /// <param name="video">The video download url.</param>
        /// <param name="output">The output file to save video.</param>
        public void DownloadDASH(string audio, string video, string output)
        {
            dash = true;

            this.Input = string.Format("{0}|{1}", audio, video);
            this.Output = output;

            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();

            downloader = new FileDownloader(true);
            downloader.LocalDirectory = folder;

            Regex regex = new Regex(@"^\w:.*\\(.*)(\..*)$");
            FileDownloader.FileInfo[] fileInfos = new FileDownloader.FileInfo[2]
            {
                new FileDownloader.FileInfo(audio)
                {
                    Name = regex.Replace(output, "$1_audio$2")
                },
                new FileDownloader.FileInfo(video)
                {
                    Name = regex.Replace(output, "$1_video$2")
                }
            };

            downloader.Files.AddRange(fileInfos);

            /* Attach events. */
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.FileSizesCalculationComplete += downloader_FileSizesCalculationComplete;
            downloader.ProgressChanged += downloader_ProgressChanged;

            downloader.Start();

            this.Status = OperationStatus.Working;

            Program.RunningOperations.Add(this);
        }

        /// <summary>
        /// Opens the output file.
        /// </summary>
        public bool Open()
        {
            try
            {
                Process.Start(this.Output);
            }
            catch
            {
                return false;
            }
            return true;
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
            downloader.Pause();

            this.Status = OperationStatus.Paused;
        }

        /// <summary>
        /// Resumes the operation.
        /// </summary>
        public void Resume()
        {
            downloader.Resume();

            this.Status = OperationStatus.Working;
        }

        /// <summary>
        /// Stops the operation.
        /// </summary>
        /// <param name="remove">True to remove the operation from it's ListView.</param>
        /// <param name="cleanup">True to delete unfinished files.</param>
        public bool Stop(bool remove, bool cleanup)
        {
            this.remove = remove;

            // Stop downloader if still running.
            if (downloader != null && downloader.CanStop)
                downloader.Stop(false);

            // Don't set status to canceled if already successful.
            if (this.Status != OperationStatus.Success)
                this.Status = OperationStatus.Canceled;

            // Cleanup if cleanup is true and download wasn't successful.
            if (cleanup && this.Status != OperationStatus.Success)
            {
                if (File.Exists(this.Output))
                    Helper.DeleteFiles(this.Output);
            }

            OnOperationComplete();

            return true;
        }

        #region combiner

        BackgroundWorker combiner;

        private void combiner_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string[] arguments = e.Argument as string[];
                string audio = arguments[0];
                string video = arguments[1];

                FFmpegHelper.CombineDash(video, audio, this.Output);

                /* Delete the separate audio & video files. */
                Helper.DeleteFiles(arguments);

                e.Result = OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Program.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        private void combiner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Status = (OperationStatus)e.Result;

            this.GetProgressBar().Style = ProgressBarStyle.Continuous;
            this.GetProgressBar().MarqueeAnimationSpeed = 0;
            this.GetProgressBar().Value = this.GetProgressBar().Maximum;

            OnOperationComplete();
        }

        #endregion

        #region downloader

        FileDownloader downloader;

        private void downloader_Canceled(object sender, EventArgs e)
        {
            // Pass the event along to a almost identical event handler.
            this.Status = OperationStatus.Canceled;

            OnOperationComplete();
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            // Set status to successful if no download(s) failed.
            if (this.Status != OperationStatus.Failed)
                this.Status = OperationStatus.Success;

            if (dash && this.Status == OperationStatus.Success)
            {
                this.Combine();
            }
            else
            {
                OnOperationComplete();
            }
        }

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            /* If one or more files fail, whole operation failed. Might handle it more
             * elegantly in the future. */
            this.Status = OperationStatus.Failed;
            downloader.Stop(false);
        }

        private void downloader_FileSizesCalculationComplete(object sender, EventArgs e)
        {
            this.SetItemText(this.SubItems[4], Helper.FormatFileSize(downloader.TotalSize));
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (this.ListView == null)
                return;

            if (ListView.InvokeRequired)
                ListView.Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    if (!this.Wait())
                    {
                        /* Only update text once per second. */
                        string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                        long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                        string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";

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

        /// <summary>
        /// Combines DASH audio &amp; video, and returns true if it was successful.
        /// </summary>
        private void Combine()
        {
            string audio = downloader.LocalDirectory + "\\" + downloader.Files[0].Name;
            string video = downloader.LocalDirectory + "\\" + downloader.Files[1].Name;

            this.SubItems[2].Text = "Combining...";
            this.GetProgressBar().Value = this.GetProgressBar().Minimum;
            this.GetProgressBar().Style = ProgressBarStyle.Marquee;
            this.GetProgressBar().MarqueeAnimationSpeed = 30;

            combiner = new BackgroundWorker();
            combiner.DoWork += combiner_DoWork;
            combiner.RunWorkerCompleted += combiner_RunWorkerCompleted;

            combiner.RunWorkerAsync(new string[] { audio, video });
        }

        /// <summary>
        /// Returns the operation's ProgressBar.
        /// </summary>
        private ProgressBar GetProgressBar()
        {
            return (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);
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

        private void OnOperationComplete()
        {
            sw.Stop();

            // Status should already be handled at some point before this.
            RefreshStatus();
            Program.RunningOperations.Remove(this);

            // Remove from it's ListView if conditions is met.
            if (this.remove && this.ListView != null)
                this.Remove();

            if (OperationComplete != null)
                OperationComplete(this, new OperationEventArgs(this, this.Status));

            Console.WriteLine(this.GetType().Name + ": Operation complete, status: " + this.Status);
        }

        private bool Wait()
        {
            // Limit the progress update to once a second to avoid flickering.
            return sw.ElapsedMilliseconds < ProgressDelay;
        }
    }
}
