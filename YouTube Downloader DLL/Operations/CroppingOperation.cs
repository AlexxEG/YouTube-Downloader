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

            _process?.Dispose();
            _process = null;
        }

        public override bool CanOpen()
        {
            return this.IsSuccessful;
        }

        public override bool CanStop()
        {
            return this.IsWorking;
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

        public override bool Stop()
        {
            if (this.IsPaused || this.IsWorking)
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

            if (!this.IsSuccessful)
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
                    var ffmpeg = new FFmpegProcess(logger);

                    if (_end == TimeSpan.MinValue)
                        ffmpeg.Crop(this.Input, this.Output, _start, this.ReportProgress, _cts.Token);
                    else
                        ffmpeg.Crop(this.Input, this.Output, _start, _end, this.ReportProgress, _cts.Token);
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
            if (this.IsSuccessful)
            {
                this.Duration = (long)FFmpegProcess.GetDuration(this.Input).Value.TotalSeconds;
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

            this.Duration = (long)FFmpegProcess.GetDuration(this.Input).Value.TotalSeconds;
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
