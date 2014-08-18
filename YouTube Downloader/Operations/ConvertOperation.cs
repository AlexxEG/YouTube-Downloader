using ListViewEmbeddedControls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader.Operations
{
    public class ConvertOperation : ListViewItem, IOperation, IDisposable
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status { get; set; }

        public event OperationEventHandler OperationComplete;

        public ConvertOperation(string text)
            : base(text)
        {
        }

        ~ConvertOperation()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public void Convert(string input, string output, string start, string end)
        {
            this.Input = input;
            this.Output = output;
            this.converterStart = start;
            this.converterEnd = end;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();

            Program.RunningWorkers.Add(backgroundWorker);

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

        public bool Stop()
        {
            try
            {
                if (process != null && !process.HasExited)
                    process.StandardInput.WriteLine("\x71");

                this.Status = OperationStatus.Canceled;
                this.SubItems[2].Text = "Stopped";

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region backgroundWorker

        private BackgroundWorker backgroundWorker;
        private string converterStart = string.Empty;
        private string converterEnd = string.Empty;
        private Process process;

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            FFmpegHelper.Convert(backgroundWorker, this.Input, this.Output);

            if (!string.IsNullOrEmpty(converterStart))
            {
                if (string.IsNullOrEmpty(converterEnd))
                {
                    FFmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart);
                }
                else
                {
                    FFmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart, converterEnd);
                }
            }

            this.converterStart = this.converterEnd = string.Empty;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar pb = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);

            pb.Value = e.ProgressPercentage;

            if (e.UserState is Process)
            {
                this.process = (Process)e.UserState;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SubItems[2].Text = "Success";
            this.SubItems[4].Text = Helper.GetFileSize(this.Output);

            this.Status = OperationStatus.Success;

            Program.RunningWorkers.Remove(backgroundWorker);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }
}
