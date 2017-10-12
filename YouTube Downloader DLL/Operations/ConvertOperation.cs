using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.FFmpegHelpers;

namespace YouTube_Downloader_DLL.Operations
{
    public class ConvertOperation : Operation
    {
        public const int UpdateProperties = -1;

        int _count = 0;
        int _failures = 0;
        string _currentOutput;
        string _searchPattern;
        TimeSpan _start = TimeSpan.MinValue;
        TimeSpan _end = TimeSpan.MinValue;
        ConvertingMode _mode = ConvertingMode.File;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public List<string> ProcessedFiles { get; set; } = new List<string>();

        private ConvertOperation(string input,
                                 string output)
        {
            this.ReportsProgress = true;
            this.Input = input;
            this.Output = output;
            this.ProgressText = "Converting...";
            this.Title = Path.GetFileName(this.Output);
        }

        public ConvertOperation(string input,
                                string output,
                                string searchPattern)
            : this(input, output)
        {
            _mode = ConvertingMode.Folder;
            this.ValidateSearchPattern(searchPattern, out _searchPattern);
        }

        public ConvertOperation(string input,
                                string output,
                                TimeSpan start,
                                TimeSpan end)
            : this(input, output)
        {
            _mode = ConvertingMode.File;
            _start = start;
            _end = end;

            this.Duration = (long)FFmpeg.GetDuration(this.Input).Value.TotalSeconds;
        }

        #region Operation members

        public override bool CanOpen()
        {
            return this.IsSuccessful;
        }

        public override bool CanStop()
        {
            return this.IsWorking || this.IsQueued;
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
            if (this.IsPaused || this.IsWorking || this.IsQueued)
            {
                try
                {
                    _cts?.Cancel();
                    this.CancelAsync();
                    this.Status = OperationStatus.Canceled;
                }
                catch (Exception ex)
                {
                    Common.SaveException(ex);
                    return false;
                }
            }

            if (cleanup && !this.IsSuccessful)
            {
                switch (_mode)
                {
                    case ConvertingMode.File:
                        Helper.DeleteFiles(this.Output);
                        break;
                    case ConvertingMode.Folder:
                        Helper.DeleteFiles(_currentOutput);
                        break;
                }
            }

            return true;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (_mode == ConvertingMode.File)
            {
                if (this.IsSuccessful)
                    this.FileSize = Helper.GetFileSize(this.Output);
            }
            else
            {
                if (_failures == 0)
                    this.Title = string.Format("Converted {0} videos", _count);
                else
                    this.Title = string.Format("Converted {0} of {1} videos, {2} failed",
                                        _count - _failures, _count, _failures);
            }
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            if (_mode == ConvertingMode.File)
            {
                try
                {
                    using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                    {
                        FFmpeg.Convert(this.Input, this.Output, this.ReportProgress, _cts.Token, logger);

                        // Crop if not operation wasn't canceled and _start has a valid value
                        if (!this.CancellationPending && _start != TimeSpan.MinValue)
                        {
                            // Crop to end of file, unless _end has a valid value
                            if (_end == TimeSpan.MinValue)
                                FFmpeg.Crop(this.Output, this.Output, _start, this.ReportProgress, _cts.Token, logger);
                            else
                                FFmpeg.Crop(this.Output, this.Output, _start, _end, this.ReportProgress, _cts.Token, logger);
                        }
                    }

                    // Reset variables
                    _start = _end = TimeSpan.MinValue;
                }
                catch (Exception ex)
                {
                    Common.SaveException(ex);
                    Helper.DeleteFiles(this.Output);
                    e.Result = OperationStatus.Failed;
                }
            }
            else
            {
                using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                {
                    foreach (string input in Directory.GetFiles(this.Input, _searchPattern))
                    {
                        if (this.CancellationPending)
                            break;

                        _count++;
                        try
                        {
                            string output = string.Format("{0}\\{1}.mp3",
                                                            this.Output,
                                                            Path.GetFileNameWithoutExtension(input));

                            this.ReportProgress(UpdateProperties, new Dictionary<string, object>()
                            {
                                { nameof(Operation.Title), Path.GetFileName(input) },
                                { nameof(Operation.Duration), (int)FFmpeg.GetDuration(input).Value.TotalSeconds },
                                { nameof(Operation.FileSize), Helper.GetFileSize(input) }
                            });

                            _currentOutput = output;
                            FFmpeg.Convert(input, output, this.ReportProgress, _cts.Token, logger);
                            _currentOutput = null;

                            this.ProcessedFiles.Add(output);
                        }
                        catch (Exception ex)
                        {
                            _failures++;
                            Common.SaveException(ex);
                            Helper.DeleteFiles(_currentOutput);
                            continue;
                        }
                    }
                }
            }

            // Set operation result
            e.Result = this.CancellationPending ? OperationStatus.Canceled : OperationStatus.Success;
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
                return;

            // Used to set multiple properties
            if (e.UserState is Dictionary<string, object>)
            {
                foreach (KeyValuePair<string, object> pair in (e.UserState as Dictionary<string, object>))
                {
                    this.GetType().GetProperty(pair.Key).SetValue(this, pair.Value);
                }
            }
        }

        private bool ValidateSearchPattern(string searchPattern, out string fixedSearchPattern)
        {
            fixedSearchPattern = searchPattern;

            // Just remove all * and . characters and re-add them to the start.
            // Should work in most cases since the search pattern should only be used for file extensions.
            fixedSearchPattern = fixedSearchPattern.Trim('.', '*');
            fixedSearchPattern = "*." + fixedSearchPattern;

            return true;
        }
    }
}
