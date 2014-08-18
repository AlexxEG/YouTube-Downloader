using DeDauwJeroen;
using ListViewEmbeddedControls;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader.Operations
{
    public class PlaylistOperation : ListViewItem, IOperation, IDisposable
    {
        /* ToDo:
         * 
         * - Show combining operation in status so that multiple instances doesn't access log file
         * - Use the combining time to get content length since it takes time.
         */

        private const int Reset_Controls = 1;

        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status
        {
            get
            {
                if (downloader == null)
                    return OperationStatus.None;

                /* Canceled */
                if (downloader.HasBeenCanceled)
                {
                    return OperationStatus.Canceled;
                }
                /* Successful */
                else if (successful)
                {
                    return OperationStatus.Success;
                }
                /* Failed */
                else if (failed)
                {
                    return OperationStatus.Failed;
                }
                /* Paused */
                else if (downloader.IsPaused)
                {
                    return OperationStatus.Paused;
                }
                /* Downloading */
                else if (!downloader.IsPaused)
                {
                    return OperationStatus.Working;
                }
                else
                {
                    return OperationStatus.None;
                }
            }
            set
            {

            }
        }

        public event OperationEventHandler OperationComplete;
        private delegate void SetTextDelegate(string text);
        private delegate void SetItemTextDelegate(ListViewSubItem item, string text);

        private FileDownloader downloader;

        private BackgroundWorker worker;
        private bool processing;

        /* downloader statuses */
        private bool failed = false;
        private bool successful = false;

        private bool useDash = false;

        public PlaylistOperation()
        {
            this.Text = "Getting playlist info...";
            /* Fill sub items. */
            this.SubItems.AddRange(new string[] { "", "", "", "", "" });
        }

        ~PlaylistOperation()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
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

        public void Download(string url, string output, bool dash)
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
            worker.RunWorkerAsync();

            Program.RunningWorkers.Add(worker);
        }

        public void Pause()
        {
            downloader.Pause();
        }

        public void Resume()
        {
            downloader.Resume();
        }

        public bool Stop()
        {
            try
            {
                downloader.Stop(false);

                return true;
            }
            catch
            {
                return false;
            }
        }

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
            if (successful)
            {
                this.SubItems[2].Text = "Completed";
            }
            else if (downloader.IsPaused)
            {
                this.SubItems[2].Text = "Paused";
            }
            else if (downloader.HasBeenCanceled)
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

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            PlaylistReader reader = new PlaylistReader(this.Input);
            VideoInfo video;

            while ((video = reader.Next()) != null)
            {
                count++;

                VideoFormat videoFormat = Helper.GetPreferedFormat(video, useDash);

                this.SetText(string.Format("({0}/{1}) {2}", count, reader.Playlist.OnlineCount, video.Title));
                this.SetItemText(this.SubItems[3], Helper.FormatVideoLength(video.Duration));
                this.SetItemText(this.SubItems[4], Helper.FormatFileSize(videoFormat.FileSize));

                downloader = new FileDownloader(true);
                downloader.LocalDirectory = this.Output;

                FileDownloader.FileInfo[] fileInfos;

                string finalFile = Path.Combine(this.Output, Helper.FormatTitle(videoFormat.VideoInfo.Title) + "." + videoFormat.Extension);

                if (!useDash)
                {
                    fileInfos = new FileDownloader.FileInfo[1]
                    {
                        new FileDownloader.FileInfo(videoFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(finalFile)
                        }
                    };
                }
                else
                {
                    VideoFormat audioFormat = Helper.GetAudioFormat(video);
                    /* Add '_audio' & '_video' to end of filename. */
                    string audioFile = Path.Combine(this.Output, Path.GetFileNameWithoutExtension(finalFile)) + "_audio.m4a";
                    string videoFile = Path.Combine(this.Output, Path.GetFileNameWithoutExtension(finalFile)) + "_video." + videoFormat.Extension;

                    fileInfos = new FileDownloader.FileInfo[2]
                    {
                        new FileDownloader.FileInfo(videoFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(videoFile)
                        },
                        new FileDownloader.FileInfo(audioFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(audioFile)
                        }
                    };
                }

                downloader.Files.AddRange(fileInfos);

                /* Attach events. */
                downloader.Completed += downloader_Completed;
                downloader.FileDownloadFailed += downloader_FileDownloadFailed;
                downloader.ProgressChanged += downloader_ProgressChanged;

                downloader.Start();

                Program.RunningDownloaders.Add(downloader);

                /* If downloader is busy or paused, wait till it's done. */
                while (downloader.IsBusy || downloader.IsPaused)
                    Thread.Sleep(200);

                worker.ReportProgress(Reset_Controls);
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case Reset_Controls:
                    this.GetProgressBar().Value = 0;
                    break;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!failed)
                successful = true;

            RefreshStatus();

            Program.RunningWorkers.Remove(worker);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            if (failed)
            {
                successful = false;
            }
            else if (useDash)
            {
                /* Queue DASH combine on a new thread so next download can start. */
                string audioFile = Path.Combine(downloader.LocalDirectory, downloader.Files[0].Name);
                string videoFile = Path.Combine(downloader.LocalDirectory, downloader.Files[1].Name);
                string finalFile = videoFile.Replace("_video", string.Empty);

                FFmpegHelper.CombineDashThread(videoFile, audioFile, finalFile);
            }

            RefreshStatus();

            Program.RunningDownloaders.Remove(downloader);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            /* If one or more files fail, whole operation failed. Might handle it more
             * elegantly in the future. */
            failed = true;
            downloader.Stop(false);
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (downloader != sender)
                return;

            if (this.ListView.InvokeRequired)
                this.ListView.Invoke(new EventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                    string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

                    this.SubItems[1].Text = downloader.TotalPercentage() + " %";
                    this.SubItems[2].Text = speed + ETA;

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
    }
}
