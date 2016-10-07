using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.FFmpeg;

namespace YouTube_Downloader_DLL.Operations
{
    public class CroppingOperation : Operation
    {
        class ArgKeys
        {
            public const int Count = 4;
            public const string Input = "input";
            public const string Output = "output";
            public const string Start = "start";
            public const string End = "end";
        }

        TimeSpan _start = TimeSpan.MinValue;
        TimeSpan _end = TimeSpan.MinValue;
        Process _process;
        CancellationTokenSource _cts = new CancellationTokenSource();

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
                    _cts.Cancel();
                    this.CancelAsync();
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
                using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                {
                    if (_end == TimeSpan.MinValue)
                        FFmpegHelper.Crop(logger, this.ReportProgress, this.Input, this.Output, _start, _cts.Token);
                    else
                        FFmpegHelper.Crop(logger, this.ReportProgress, this.Input, this.Output, _start, _end, _cts.Token);
                }

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

        protected override void WorkerStart(Dictionary<string, object> args)
        {
            if (args.Count != ArgKeys.Count)
                throw new ArgumentException();

            this.Input = (string)args[ArgKeys.Input];
            this.Output = (string)args[ArgKeys.Output];

            _start = (TimeSpan)args[ArgKeys.Start];
            _end = (TimeSpan)args[ArgKeys.End];

            this.Duration = (long)FFmpegHelper.GetDuration(this.Input).Value.TotalSeconds;
            this.Text = "Cropping...";
            this.Title = Path.GetFileName(this.Output);
        }

        public Dictionary<string, object> Args(string input,
                                               string output,
                                               TimeSpan start,
                                               TimeSpan end)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Input, input },
                { ArgKeys.Output, output },
                { ArgKeys.Start, start },
                { ArgKeys.End, end }
            };
        }
    }
}
