using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Helpers;

namespace YouTube_Downloader_DLL.Operations
{
    public class CroppingOperation : Operation
    {
        class ArgsConstants
        {
            public const int Count = 4;
            public const int Input = 0;
            public const int Output = 1;
            public const int Start = 2;
            public const int End = 3;
        }

        TimeSpan _start = TimeSpan.MinValue;
        TimeSpan _end = TimeSpan.MinValue;
        Process _process;

        public CroppingOperation()
        {
            this.ReportsProgress = true;
        }

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

        public override bool CanOpen()
        {
            return this.Status == OperationStatus.Success;
        }

        public override bool CanStop()
        {
            return this.Status == OperationStatus.Working;
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
                    Common.SaveException(ex);
                    return false;
                }
            }

            if (cleanup && this.Status != OperationStatus.Success)
            {
                if (File.Exists(this.Output))
                    Helper.DeleteFiles(this.Output);
            }

            return true;
        }

        #endregion

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            try
            {
                if (_end == TimeSpan.MinValue)
                    FFmpegHelper.Crop(this.ReportProgress, this.Input, this.Output, _start);
                else
                    FFmpegHelper.Crop(this.ReportProgress, this.Input, this.Output, _start, _end);

                _start = _end = TimeSpan.MinValue;

                e.Result = this.CancellationPending ? OperationStatus.Canceled : OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.UserState is Process)
            {
                // FFmpegHelper will return the ffmpeg process so it can be used to cancel.
                this._process = (Process)e.UserState;
            }
        }

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (this.Status == OperationStatus.Success)
            {
                this.Duration = (long)FFmpegHelper.GetDuration(this.Input).Value.TotalSeconds;
                this.FileSize = Helper.GetFileSize(this.Output);
            }
        }

        protected override void WorkerStart(object[] args)
        {
            if (args.Length != ArgsConstants.Count)
                throw new ArgumentException();

            this.Input = (string)args[ArgsConstants.Input];
            this.Output = (string)args[ArgsConstants.Output];

            _start = (TimeSpan)args[ArgsConstants.Start];
            _end = (TimeSpan)args[ArgsConstants.End];

            this.Duration = (long)FFmpegHelper.GetDuration(this.Input).Value.TotalSeconds;
            this.Text = "Cropping...";
            this.Title = Path.GetFileName(this.Output);
        }

        public object[] Args(string input, string output, TimeSpan start, TimeSpan end)
        {
            return new object[] { input, output, start, end };
        }
    }
}
