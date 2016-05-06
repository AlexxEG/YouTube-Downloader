using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.Helpers;

namespace YouTube_Downloader_DLL.Operations
{
    public class ConvertOperation : Operation
    {
        int _count = 0;
        int _failures = 0;
        string _searchPattern;
        TimeSpan _start = TimeSpan.MinValue;
        TimeSpan _end = TimeSpan.MinValue;
        Process _process;
        ConvertingMode _mode = ConvertingMode.File;

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
                    Common.SaveException(ex);
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

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            if (_mode == ConvertingMode.File)
            {
                try
                {
                    FFmpegHelper.Convert(this.ReportProgress, this.Input, this.Output);

                    // Crop if not operation wasn't canceled and _start has a valid value
                    if (!this.CancellationPending && _start != TimeSpan.MinValue)
                    {
                        // Crop to end of file, unless _end has a valid value
                        if (_end == TimeSpan.MinValue)
                            FFmpegHelper.Crop(this.ReportProgress, this.Output, this.Output, _start);
                        else
                            FFmpegHelper.Crop(this.ReportProgress, this.Output, this.Output, _start, _end);
                    }

                    // Reset variables
                    _start = _end = TimeSpan.MinValue;
                }
                catch (Exception ex)
                {
                    Common.SaveException(ex);
                    e.Result = OperationStatus.Failed;
                }
            }
            else
            {
                foreach (string input in Directory.GetFiles(this.Input, _searchPattern))
                {
                    _count++;
                    try
                    {
                        string output = string.Format("{0}\\{1}.mp3",
                                                        this.Output,
                                                        Path.GetFileNameWithoutExtension(input));

                        this.ReportProgress(-1, new Dictionary<string, object>()
                        {
                            { "Title", Path.GetFileName(input) },
                            { "Duration", (int)FFmpegHelper.GetDuration(input).Value.TotalSeconds },
                            { "FileSize", Helper.GetFileSize(input) }
                        });

                        FFmpegHelper.Convert(this.ReportProgress, input, output);
                    }
                    catch (Exception ex)
                    {
                        _failures++;
                        Common.SaveException(ex);
                        continue;
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

            if (e.UserState is Process)
            {
                // FFmpegHelper will return the ffmpeg process so it can be used to cancel.
                _process = (Process)e.UserState;
            }
            // Used to set multiple properties
            else if (e.UserState is Dictionary<string, object>)
            {
                foreach (KeyValuePair<string, object> pair in (e.UserState as Dictionary<string, object>))
                {
                    this.GetType().GetProperty(pair.Key).SetValue(this, pair.Value);
                }
            }
        }

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            if (_mode == ConvertingMode.File)
            {
                if (this.Status == OperationStatus.Success)
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

        protected override void WorkerStart(object[] args)
        {
            if (!(args.Length == 3 || args.Length == 4))
                throw new ArgumentException("ConvertOperation: Invalid argument count. (" + args.Length + ")");

            this.ReportsProgress = true;

            this.Input = (string)args[0];
            this.Output = (string)args[1];

            if (args.Length == 3)
            {
                _mode = ConvertingMode.Folder;

                this.Title = Path.GetFileName(this.Input);
                this.ValidateSearchPattern((string)args[2], out _searchPattern);
            }
            else if (args.Length == 4)
            {
                _start = (TimeSpan)args[2];
                _end = (TimeSpan)args[3];
                _mode = ConvertingMode.File;

                this.Duration = (long)FFmpegHelper.GetDuration(this.Input).Value.TotalSeconds;
                this.Title = Path.GetFileName(this.Output);
            }

            this.Text = "Converting...";
        }

        public object[] Args(string input, string output, string searchPattern)
        {
            return new object[] { input, output, searchPattern };
        }

        public object[] Args(string input, string output, TimeSpan start, TimeSpan end)
        {
            return new object[] { input, output, start, end };
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
