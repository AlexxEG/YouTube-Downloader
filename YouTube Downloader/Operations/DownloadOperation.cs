using DeDauwJeroen;
using ListViewEmbeddedControls;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader.Operations
{
    public class DownloadOperation : ListViewItem, IOperation, IDisposable
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status
        {
            get
            {
                if (downloader == null)
                    return OperationStatus.None;

                if (downloader.HasBeenCanceled) /* Canceled */
                {
                    return OperationStatus.Canceled;
                }
                else if (successful)
                {
                    return OperationStatus.Success;
                }
                else if (failed)
                {
                    return OperationStatus.Failed;
                }
                else if (downloader.IsPaused)
                {
                    return OperationStatus.Paused;
                }
                else if (!downloader.IsPaused) /* Downloading */
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
        private FileDownloader downloader;

        private bool dash = false;

        /* downloader statuses */
        private bool failed = false;
        private bool successful = false;

        public DownloadOperation(string text)
            : base(text)
        {
        }

        ~DownloadOperation()
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
                if (downloader != null)
                {
                    downloader.Dispose();
                    downloader = null;
                }
                OperationComplete = null;
            }
        }

        public void Download(string url, string output)
        {
            this.Input = url;
            this.Output = output;

            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();

            /* Reset some variables in-case downloader is restarted. */
            failed = successful = false;

            downloader = new FileDownloader(true);
            downloader.LocalDirectory = folder;

            FileDownloader.FileInfo fileInfo = new FileDownloader.FileInfo(url);

            /* Give proper filename to downloaded file. */
            fileInfo.Name = file;

            downloader.Files.Add(fileInfo);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += delegate { failed = true; };
            downloader.FileDownloadSucceeded += delegate { successful = true; };
            downloader.Start();

            Program.RunningDownloaders.Add(downloader);
        }

        public void DownloadDASH(string audio, string video, string output)
        {
            dash = true;

            this.Input = string.Format("{0}|{1}", audio, video);
            this.Output = output;

            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();

            failed = successful = false;

            downloader = new FileDownloader(true);
            downloader.LocalDirectory = folder;

            FileDownloader.FileInfo[] fileInfos = new FileDownloader.FileInfo[2]
            {
                new FileDownloader.FileInfo(audio)
                {
                    Name = Path.GetFileNameWithoutExtension(output) + "_audio" + Path.GetExtension(output)
                },
                new FileDownloader.FileInfo(video)
                {
                    Name = Path.GetFileNameWithoutExtension(output) + "_video" + Path.GetExtension(output)
                }
            };

            downloader.Files.AddRange(fileInfos);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += delegate
            {
                /* If one or more files fail, whole operation failed. Might handle it more
                 * elegantly in the future. */
                failed = true;
                downloader.Stop(false);
            };
            downloader.Start();

            Program.RunningDownloaders.Add(downloader);
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

        #region downloader

        private bool processing;

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (ListView.InvokeRequired)
                ListView.Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                    string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";

                    this.SubItems[1].Text = downloader.TotalPercentage() + " %";
                    this.SubItems[2].Text = speed + ETA;

                    ProgressBar progressBar = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);

                    progressBar.Value = (int)downloader.TotalPercentage();

                    RefreshStatus();
                }
                catch { }
                finally { processing = false; }
            }
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            if (!failed)
                successful = true;

            if (dash)
            {
                string audio = downloader.LocalDirectory + "\\" + downloader.Files[0].Name;
                string video = downloader.LocalDirectory + "\\" + downloader.Files[1].Name;

                /* ToDo: Show the combining operation in 'Speed' column. */
                FFmpegHelper.CombineDash(video, audio, this.Output);
            }

            RefreshStatus();

            Program.RunningDownloaders.Remove(downloader);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

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

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }
}
