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
        bool _combine, _processing, _downloadSuccessful;
        FileDownloader downloader;

        private DownloadOperation(VideoFormat format)
        {
            this.ReportsProgress = true;
            this.Duration = format.VideoInfo.Duration;
            this.FileSize = format.FileSize;
            this.Link = format.VideoInfo.Url;
            this.Thumbnail = format.VideoInfo.ThumbnailUrl;
            this.Title = format.VideoInfo.Title;

            downloader = new FileDownloader();
            // Attach events
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            downloader.ProgressChanged += downloader_ProgressChanged;
        }

        public DownloadOperation(VideoFormat format,
                                 string output)
            : this(format)
        {
            this.Input = format.DownloadUrl;
            this.Output = output;

            string file = Path.GetFileName(this.Output).Trim();

            downloader.Files.Add(new FileDownload(this.Output, this.Input));
        }

        public DownloadOperation(VideoFormat format,
                                 VideoFormat audio,
                                 string output)
            : this(format)
        {
            _combine = true;
            this.Input = $"{audio.DownloadUrl}|{format.DownloadUrl}";
            this.Output = output;

            Regex regex = new Regex(@"^(\w:.*\\.*)(\..*)$");

            downloader.Files.Add(new FileDownload(regex.Replace(this.Output, "$1_audio$2"),
                                                  audio.DownloadUrl,
                                                  true));
            downloader.Files.Add(new FileDownload(regex.Replace(this.Output, "$1_video$2"),
                                                  format.DownloadUrl,
                                                  true));

            // Delete _audio and _video files in case they exists from a previous attempt
            Helper.DeleteFiles(downloader.Files[0].Path,
                               downloader.Files[1].Path);
        }

        private void downloader_Canceled(object sender, EventArgs e)
        {
            if (this.Status == OperationStatus.Failed)
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
                // string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";
                string ETA = longETA == 0 ? "" : "  " + TimeSpan.FromMilliseconds(longETA * 1000).ToString(@"hh\:mm\:ss");

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

        #region Operation members

        public override bool CanOpen()
        {
            return this.IsSuccessful;
        }

        public override bool CanPause()
        {
            // Only downloader can pause.
            return downloader?.CanPause == true && this.IsWorking;
        }

        public override bool CanResume()
        {
            // Only downloader can resume.
            return downloader?.CanResume == true && (this.IsPaused || this.IsQueued);
        }

        public override bool CanStop()
        {
            return this.IsPaused || this.IsWorking || this.IsQueued;
        }

        public override void Dispose()
        {
            base.Dispose();

            downloader?.Dispose();
            downloader = null;
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

        public override void Queue()
        {
            downloader.Pause();

            this.Status = OperationStatus.Queued;
        }

        protected override void ResumeInternal()
        {
            downloader.Resume();

            this.Status = OperationStatus.Working;
        }

        public override bool Stop()
        {
            // Stop downloader if still running.
            if (downloader?.CanStop == true)
                downloader.Stop();

            // Don't set status to canceled if already successful.
            if (!this.IsSuccessful)
                this.Status = OperationStatus.Canceled;

            return true;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            while (downloader?.IsBusy == true)
                Thread.Sleep(200);

            if (_combine && _downloadSuccessful)
            {
                string audio = downloader.Files[0].Path;
                string video = downloader.Files[1].Path;

                this.ReportProgress(-1, new Dictionary<string, object>()
                {
                    { nameof(Progress), 0 }
                });
                this.ReportProgress(ProgressMax, null);

                try
                {
                    FFmpegResult<bool> result;

                    this.ReportProgress(-1, new Dictionary<string, object>()
                    {
                        { nameof(ProgressText), "Combining..." }
                    });

                    using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                    {
                        result = new FFmpegProcess(logger).Combine(video, audio, this.Output, delegate (int percentage)
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

            // Used to set multiple properties
            if (e.UserState is Dictionary<string, object>)
            {
                foreach (var pair in (e.UserState as Dictionary<string, object>))
                {
                    this.GetType().GetProperty(pair.Key).SetValue(this, pair.Value);
                }
            }
        }

        protected override void WorkerStart()
        {
            downloader.Start();
        }
    }
}