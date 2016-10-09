using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.FFmpeg;
using YouTube_Downloader_DLL.FileDownloading;

namespace YouTube_Downloader_DLL.Operations
{
    public class DownloadOperation : Operation
    {
        class ArgKeys
        {
            public const int Min = 2;
            public const int Max = 3;
            public const string Url = "url";
            public const string Audio = "audio";
            public const string Video = "video";
            public const string Output = "output";
        }

        enum Events
        {
            Combining
        }

        bool _combine, _processing, _downloadSuccessful;
        FileDownloader downloader;

        public event EventHandler Combining;

        public DownloadOperation(VideoFormat format)
        {
            this.Duration = format.VideoInfo.Duration;
            this.FileSize = format.FileSize;
            this.Link = format.VideoInfo.Url;
            this.ReportsProgress = true;
            this.Thumbnail = format.VideoInfo.ThumbnailUrl;
            this.Title = format.VideoInfo.Title;
        }

        private void downloader_Canceled(object sender, EventArgs e)
        {
            // Pass the event along to a almost identical event handler.
            this.Status = OperationStatus.Canceled;
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            // Set status to successful if no download(s) failed.
            if (this.Status != OperationStatus.Failed)
                if (_combine)
                    _downloadSuccessful = true;
                else
                    this.Status = OperationStatus.Success;
        }

        private void downloader_FileDownloadFailed(object sender, FileDownloadFailedEventArgs e)
        {
            // If one or more files fail, whole operation failed. Might handle it more
            // elegantly in the future.
            this.Status = OperationStatus.Failed;
            downloader.Stop();
        }

        private void downloader_CalculatedTotalFileSize(object sender, EventArgs e)
        {
            this.FileSize = downloader.TotalSize;
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (_processing)
                return;

            try
            {
                _processing = true;

                string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.Speed);
                long longETA = Helper.GetETA(downloader.Speed, downloader.TotalSize, downloader.TotalProgress);
                string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";

                this.ETA = ETA;
                this.Speed = speed;
                this.Progress = downloader.TotalProgress;
                this.ReportProgress((int)downloader.TotalPercentage(), null);
            }
            catch { }
            finally
            {
                _processing = false;
            }
        }

        private void OnCombining()
        {
            this.Combining?.Invoke(this, EventArgs.Empty);
        }

        #region Operation members

        public override void Dispose()
        {
            base.Dispose();

            // Free managed resources
            if (downloader != null)
            {
                downloader.Dispose();
                downloader = null;
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

        public override void Pause()
        {
            downloader.Pause();

            this.Status = OperationStatus.Paused;
        }

        public override void Resume()
        {
            downloader.Resume();

            this.Status = OperationStatus.Working;
        }

        public override bool Stop()
        {
            // Stop downloader if still running.
            if (downloader != null && downloader.CanStop)
                downloader.Stop();

            // Don't set status to canceled if already successful.
            if (this.Status != OperationStatus.Success)
                this.Status = OperationStatus.Canceled;

            return true;
        }

        public override bool CanOpen()
        {
            return this.Status == OperationStatus.Success;
        }

        public override bool CanPause()
        {
            // Only downloader can pause.
            return downloader.CanPause && this.Status == OperationStatus.Working;
        }

        public override bool CanResume()
        {
            // Only downloader can resume.
            return downloader.CanResume && this.Status == OperationStatus.Paused;
        }

        public override bool CanStop()
        {
            return this.IsPaused || this.IsWorking;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            this.ProgressTextOverride = string.Empty;
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            while (downloader != null && downloader.IsBusy)
                Thread.Sleep(200);

            if (_combine && _downloadSuccessful)
            {
                string audio = downloader.Files[0].Path;
                string video = downloader.Files[1].Path;

                this.ReportProgress(-1, new Dictionary<string, object>()
                {
                    { nameof(Text), "Combining..." },
                    { nameof(Progress), 0 },
                    { nameof(ProgressTextOverride), "Combining..." }
                });
                this.ReportProgress(ProgressMax, null);

                try
                {
                    FFmpegResult<bool> result;

                    this.ReportProgress(-1, Events.Combining);

                    using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                    {
                        result = FFmpegHelper.Combine(logger, video, audio, this.Output, delegate (int percentage)
                        {
                            // Combine progress
                            this.ReportProgress(percentage, null);
                        });
                    }

                    if (result.Value)
                        e.Result = OperationStatus.Success;
                    else
                    {
                        e.Result = OperationStatus.Failed;
                        this.ErrorsInternal.AddRange(result.Errors);
                    }

                    // Cleanup the separate audio and video files
                    Helper.DeleteFiles(audio, video);
                }
                catch (Exception ex)
                {
                    Common.SaveException(ex);
                    e.Result = OperationStatus.Failed;
                }
            }
            else
            {
                e.Result = this.Status;
            }
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.UserState == null)
                return;

            // Raise event on correct thread
            if (e.UserState is Events)
            {
                switch ((Events)e.UserState)
                {
                    case Events.Combining:
                        this.OnCombining();
                        break;
                }
            }

            // Used to set multiple properties
            if (e.UserState is Dictionary<string, object>)
            {
                foreach (var pair in (e.UserState as Dictionary<string, object>))
                {
                    this.GetType().GetProperty(pair.Key).SetValue(this, pair.Value);
                }
            }
        }

        protected override void WorkerStart(Dictionary<string, object> args)
        {
            downloader = new FileDownloader();

            switch (args.Count)
            {
                case ArgKeys.Min:
                    this.Input = (string)args[ArgKeys.Url];
                    this.Output = (string)args[ArgKeys.Output];

                    string file = Path.GetFileName(this.Output).Trim();

                    downloader.Files.Add(new FileDownload(this.Output, this.Input));
                    break;
                case ArgKeys.Max:
                    _combine = true;
                    this.Input = $"{args[ArgKeys.Audio]}|{args[ArgKeys.Video]}";
                    this.Output = (string)args[ArgKeys.Output];

                    Regex regex = new Regex(@"^(\w:.*\\.*)(\..*)$");

                    downloader.Files.Add(new FileDownload(regex.Replace(this.Output, "$1_audio$2"),
                                                          (string)args[ArgKeys.Audio],
                                                          true));
                    downloader.Files.Add(new FileDownload(regex.Replace(this.Output, "$1_video$2"),
                                                          (string)args[ArgKeys.Video],
                                                          true));

                    // Delete _audio and _video files in case they exists from a previous attempt
                    Helper.DeleteFiles(downloader.Files[0].Path,
                                       downloader.Files[1].Path);

                    break;
                default:
                    throw new ArgumentException();
            }

            // Attach events
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            downloader.ProgressChanged += downloader_ProgressChanged;

            downloader.Start();
        }

        public static Dictionary<string, object> Args(string url,
                                                      string output)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Url, url },
                { ArgKeys.Output, output }
            };
        }

        public static Dictionary<string, object> Args(string audio,
                                                      string video,
                                                      string output)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Audio, audio },
                { ArgKeys.Video, video },
                { ArgKeys.Output, output }
            };
        }
    }
}