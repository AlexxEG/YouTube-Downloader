﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.FFmpeg;

namespace YouTube_Downloader_DLL.Operations
{
    public class ConvertOperation : Operation
    {
        class ArgKeys
        {
            public const int Min = 3;
            public const int Max = 4;
            public const string Input = "input";
            public const string Output = "output";
            public const string Start = "start";
            public const string End = "end";
            public const string SearchPattern = "search_pattern";
        }

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

        #region Operation members

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
                if (_mode == ConvertingMode.File)
                {
                    Helper.DeleteFiles(this.Output);
                }
                else
                {
                    Helper.DeleteFiles(_currentOutput);
                }
            }

            return true;
        }

        #endregion

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            if (_mode == ConvertingMode.File)
            {
                try
                {
                    using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                    {
                        var ffmpeg = new FFmpegProcess(logger);

                        ffmpeg.Convert(this.Input, this.Output, this.ReportProgress, _cts.Token);

                        // Crop if not operation wasn't canceled and _start has a valid value
                        if (!this.CancellationPending && _start != TimeSpan.MinValue)
                        {
                            // Crop to end of file, unless _end has a valid value
                            if (_end == TimeSpan.MinValue)
                                ffmpeg.Crop(this.Output, this.Output, _start, this.ReportProgress, _cts.Token);
                            else
                                ffmpeg.Crop(this.Output, this.Output, _start, _end, this.ReportProgress, _cts.Token);
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
                    var ffmpeg = new FFmpegProcess(logger);

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
                                { nameof(Title), Path.GetFileName(input) },
                                { nameof(Duration), (int)FFmpegProcess.GetDuration(input).Value.TotalSeconds },
                                { nameof(FileSize), Helper.GetFileSize(input) }
                            });

                            _currentOutput = output;
                            ffmpeg.Convert(input, output, this.ReportProgress, _cts.Token);
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

        protected override void WorkerStart(Dictionary<string, object> args)
        {
            if (!(args.Count.Any(ArgKeys.Min, ArgKeys.Max)))
                throw new ArgumentException($"{nameof(ConvertOperation)}: Invalid argument count: ({args.Count}).");

            this.Input = (string)args[ArgKeys.Input];
            this.Output = (string)args[ArgKeys.Output];

            if (args.Count == ArgKeys.Min)
            {
                _mode = ConvertingMode.Folder;

                this.Title = Path.GetFileName(this.Input);
                this.ValidateSearchPattern((string)args[ArgKeys.SearchPattern], out _searchPattern);
            }
            else if (args.Count == ArgKeys.Max)
            {
                _start = (TimeSpan)args[ArgKeys.Start];
                _end = (TimeSpan)args[ArgKeys.End];
                _mode = ConvertingMode.File;

                this.Duration = (long)FFmpegProcess.GetDuration(this.Input).Value.TotalSeconds;
                this.Title = Path.GetFileName(this.Output);
            }

            this.Text = "Converting...";
        }

        public ConvertOperation()
        {
            this.ReportsProgress = true;
        }

        public Dictionary<string, object> Args(string input,
                                               string output,
                                               string searchPattern)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Input, input },
                { ArgKeys.Output, output },
                { ArgKeys.SearchPattern, searchPattern }
            };
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
