using ListViewEmbeddedControls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader.Operations
{
    public class CroppingOperation : ListViewItem, IOperation, IDisposable
    {
        public string Input { get; private set; }
        public string Output { get; private set; }
        public OperationStatus Status { get; private set; }

        public event OperationEventHandler OperationComplete;

        bool remove;

        public CroppingOperation(string text)
            : base(text)
        {
        }

        ~CroppingOperation()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public bool CanOpen()
        {
            return this.Status == OperationStatus.Success;
        }

        public bool CanPause()
        {
            /* Doesn't support pausing. */
            return false;
        }

        public bool CanResume()
        {
            /* Doesn't support resuming. */
            return false;
        }

        public bool CanStop()
        {
            /* Can stop if working. */
            return this.Status == OperationStatus.Working;
        }

        public void Crop(string input, string output, string start, string end)
        {
            this.Input = input;
            this.Output = output;
            this.cropStart = start;
            this.cropEnd = end;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();

            Program.RunningOperations.Add(this);

            this.Status = OperationStatus.Working;
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
                if (backgroundWorker != null)
                {
                    backgroundWorker.Dispose();
                    backgroundWorker = null;
                }
                if (process != null)
                {
                    process.Dispose();
                    process = null;
                }
                OperationComplete = null;
            }
        }

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

        public void Pause()
        {
            throw new NotSupportedException();
        }

        public void Resume()
        {
            throw new NotSupportedException();
        }

        public bool Stop(bool remove)
        {
            this.remove = remove;

            if (this.Status == OperationStatus.Paused || this.Status == OperationStatus.Working)
            {
                try
                {
                    backgroundWorker.CancelAsync();

                    if (process != null && !process.HasExited)
                        process.StandardInput.WriteLine("\x71");

                    this.Status = OperationStatus.Canceled;
                    this.RefreshStatus();
                }
                catch (Exception ex)
                {
                    Program.SaveException(ex);
                    return false;
                }
            }

            return true;
        }

        public bool Stop(bool remove, bool deleteUnfinishedFiles)
        {
            bool success = this.Stop(remove);

            if (deleteUnfinishedFiles && !(this.Status == OperationStatus.Success))
            {
                if (File.Exists(this.Output))
                    Helper.DeleteFiles(this.Output);
            }

            return success;
        }

        #region backgroundWorker

        private BackgroundWorker backgroundWorker;
        private string cropStart = string.Empty;
        private string cropEnd = string.Empty;
        private Process process;

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (cropEnd == string.Empty)
                    FFmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart);
                else
                    FFmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart, cropEnd);

                cropStart = cropEnd = string.Empty;

                if (backgroundWorker.CancellationPending)
                {
                    e.Result = OperationStatus.Canceled;
                }
                else
                {
                    e.Result = OperationStatus.Success;
                }
            }
            catch (Exception ex)
            {
                Program.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.GetProgressBar().Value = e.ProgressPercentage;

            if (e.UserState is Process)
            {
                // FFmpegHelper will return the ffmpeg process so it can be used to cancel.
                this.process = (Process)e.UserState;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Status = (OperationStatus)e.Result;
            this.RefreshStatus();

            if (this.Status == OperationStatus.Success)
            {
                this.SubItems[3].Text = Helper.FormatVideoLength(FFmpegHelper.GetDuration(this.Input));
                this.SubItems[4].Text = Helper.GetFileSize(this.Output);
            }

            Program.RunningOperations.Remove(this);

            OnOperationComplete(new OperationEventArgs(this, this.Status));

            if (this.remove && this.ListView != null)
            {
                this.Remove();
            }
        }

        #endregion

        private ProgressBar GetProgressBar()
        {
            return (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);
        }

        private void RefreshStatus()
        {
            if (this.Status == OperationStatus.Canceled)
            {
                this.SubItems[2].Text = "Canceled";
            }
            else if (this.Status == OperationStatus.Failed)
            {
                this.SubItems[2].Text = "Failed";
            }
            else if (this.Status == OperationStatus.Success)
            {
                this.SubItems[2].Text = "Completed";
            }
            else
            {
                this.SubItems[2].Text = "???";
            }
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }
}
