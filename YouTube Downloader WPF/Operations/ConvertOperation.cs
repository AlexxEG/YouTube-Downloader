using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF.Operations
{
    public class ConvertOperation : Operation
    {
        TimeSpan _start = TimeSpan.MinValue;
        TimeSpan _end = TimeSpan.MinValue;
        Process _process;

        #region Operation members

        public override void Dispose()
        {
            base.Dispose();

            // Free managed resources
            if (_process != null)
            {
                _process.Dispose();
                _process = null;
            }
        }

        public override bool Open()
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

        public override bool OpenContainingFolder()
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

        public override bool Stop(bool cleanup)
        {
            bool success = true;

            if (this.Status == OperationStatus.Paused || this.Status == OperationStatus.Working)
            {
                try
                {
                    this.CancelAsync();

                    if (_process != null && !_process.HasExited)
                        _process.StandardInput.WriteLine("\x71");

                    this.Status = OperationStatus.Canceled;
                }
                catch (Exception ex)
                {
                    App.SaveException(ex);
                    return false;
                }
            }

            if (cleanup && this.Status != OperationStatus.Success)
            {
                if (File.Exists(this.Output))
                    Helper.DeleteFiles(this.Output);
            }

            return success;
        }

        public override bool CanOpen()
        {
            return this.Status == OperationStatus.Success;
        }

        public override bool CanStop()
        {
            // Can stop if working.
            return this.Status == OperationStatus.Working;
        }

        #endregion

        protected override void OnWorkerDoWork(DoWorkEventArgs e)
        {
            try
            {
                FFmpegHelper.Convert(this.ReportProgress, this.Input, this.Output);

                if (!this.CancellationPending && _start != TimeSpan.MinValue)
                {
                    if (_end == TimeSpan.MinValue)
                    {
                        FFmpegHelper.Crop(this.ReportProgress, this.Output, this.Output, _start);
                    }
                    else
                    {
                        FFmpegHelper.Crop(this.ReportProgress, this.Output, this.Output, _start, _end);
                    }
                }

                _start = _end = TimeSpan.MinValue;

                if (this.CancellationPending)
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
                App.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        protected override void OnWorkerProgressChanged(ProgressChangedEventArgs e)
        {
            base.OnWorkerProgressChanged(e);

            if (e.UserState is Process)
            {
                // FFmpegHelper will return the ffmpeg process so it can be used to cancel.
                _process = (Process)e.UserState;
            }
        }

        protected override void OnWorkerRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            base.OnWorkerRunWorkerCompleted(e);

            if (this.Status == OperationStatus.Success)
            {
                this.FileSize = Helper.GetFileSize(this.Output);
            }
        }

        protected override void OnWorkerStart(object[] args)
        {
            this.ReportsProgress = true;

            this.Input = (string)args[0];
            this.Output = (string)args[1];

            _start = (TimeSpan)args[2];
            _end = (TimeSpan)args[3];

            this.Duration = (long)FFmpegHelper.GetDuration(this.Input).TotalSeconds;
            this.Text = "Converting...";
            this.Title = Path.GetFileName(this.Output);
        }

        public static object[] Args(string input, string output, TimeSpan start, TimeSpan end)
        {
            return new object[] { input, output, start, end };
        }
    }
}
