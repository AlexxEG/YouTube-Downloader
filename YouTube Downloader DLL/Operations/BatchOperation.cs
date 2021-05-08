using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.FileDownloading;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader_DLL.Operations
{
    public class BatchOperation : Operation
    {
        public const int EventFileDownloadComplete = 1002;

        int _downloads;
        int _failures;
        bool _cancel;
        bool _processing;
        bool? _downloaderSuccessful;
        bool _ignoreExisting;
        bool _prefix;
        PreferredQuality _preferredQuality;

        Exception _operationException;
        FileDownloader _downloader;
        OperationLogger _logger;

        public List<string> DownloadedFiles { get; set; } = new List<string>();
        public List<string> Inputs { get; private set; } = new List<string>();
        public List<VideoInfo> Videos { get; private set; } = new List<VideoInfo>();

        /// <summary>
        /// Occurs when a single download from the batch download is complete.
        /// </summary>
        public event EventHandler<string> FileDownloadComplete;

        public BatchOperation(string output, ICollection<string> inputs, PreferredQuality preferredQuality, bool ignoreExisting, bool prefix)
        {
            this.Title = $"Batch download (0/{inputs.Count} videos)";
            this.ReportsProgress = true;
            this.Input = string.Join("|", inputs);
            this.Output = output;
            this.Inputs.AddRange(inputs);

            _preferredQuality = preferredQuality;
            _ignoreExisting = ignoreExisting;
            _prefix = prefix;
            _logger = OperationLogger.Create(OperationLogger.YTDLogFile);

            _downloader = new FileDownloader();
            // Attach events
            _downloader.Canceled += downloader_Canceled;
            _downloader.Completed += downloader_Completed;
            _downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            _downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            _downloader.ProgressChanged += downloader_ProgressChanged;
        }

        #region FileDownloader events

        private void downloader_Canceled(object sender, EventArgs e)
        {
            _downloaderSuccessful = false;
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            // If the download didn't fail & wasn't canceled it was most likely successful.
            if (_downloaderSuccessful == null) _downloaderSuccessful = true;
        }

        private void downloader_FileDownloadFailed(object sender, FileDownloadFailedEventArgs e)
        {
            // If one or more files fail, whole operation failed. Might handle it more
            // elegantly in the future.
            _downloaderSuccessful = false;

            Common.SaveException(e.Exception);
        }

        private void downloader_CalculatedTotalFileSize(object sender, EventArgs e)
        {
            this.FileSize = _downloader.TotalSize;
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (_processing)
                return;

            try
            {
                _processing = true;

                string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", _downloader.Speed);
                long longETA = Helper.GetETA(_downloader.Speed, _downloader.TotalSize, _downloader.TotalProgress);
                string eta = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

                this.ETA = eta;
                this.Speed = speed;
                this.Progress = _downloader.TotalProgress;
                this.ReportProgress((int)_downloader.TotalPercentage(), null);
            }
            catch { }
            finally
            {
                _processing = false;
            }
        }

        #endregion

        #region Operation members

        public override void Dispose()
        {
            base.Dispose();

            // Free managed resources
            _downloader?.Dispose();
            _downloader = null;
        }

        public override bool CanPause()
        {
            // Can only pause if currently downloading
            return _downloader?.CanPause == true;
        }

        public override bool CanResume()
        {
            // Can only resume downloader
            return _downloader?.CanResume == true;
        }

        public override bool CanStop()
        {
            return this.IsPaused || this.IsWorking || this.IsQueued;
        }

        public override bool OpenContainingFolder()
        {
            try
            {
                Process.Start(Path.GetDirectoryName(this.Output));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Pause()
        {
            if (_downloader.CanPause)
                _downloader.Pause();

            this.Status = OperationStatus.Paused;
        }

        public override void Queue()
        {
            if (_downloader.CanPause)
                _downloader.Pause();

            this.Status = OperationStatus.Queued;
        }

        protected override void ResumeInternal()
        {
            if (_downloader.CanResume)
                _downloader.Resume();

            this.Status = OperationStatus.Working;
        }

        public override bool Stop()
        {
            if (this.IsBusy)
                this.CancelAsync();

            this.Status = OperationStatus.Canceled;
            _cancel = true;
            return true;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            switch ((OperationStatus)e.Result)
            {
                case OperationStatus.Canceled:
                    // Tell user how many videos was downloaded before being canceled, if any
                    if (this.Inputs.Count == 0)
                        this.Title = $"Batch download canceled";
                    else
                        this.Title = $"Batch download canceled. {_downloads} of {this.Inputs.Count} video(s) downloaded";
                    return;
                case OperationStatus.Failed:
                    // Tell user about known exceptions. Otherwise just a simple failed message
                    if (_operationException is TimeoutException)
                        this.Title = $"Timeout. Couldn't get playlist information";
                    else
                    {
                        this.Title = $"Batch download failed";
                    }
                    return;
            }

            // If code reaches here, it means operation was successful
            if (_failures == 0)
            {
                // All videos downloaded successfully
                this.Title = $"Batch download. Downloaded {this.Inputs.Count} video(s)";
            }
            else
            {
                // Some or all videos failed. Tell user how many
                this.Title = string.Format("Batch download. Downloaded {0} of {1} video(s), {2} failed",
                    _downloads, this.Inputs.Count, _failures);
            }
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            var task = YTD.GetVideoInfoBatchAsync(this.Inputs, video =>
            {
                this.Videos.Add(video);
            }, null, _logger);

            try
            {
                int count = 0;

                while (count < this.Videos.Count || !task.IsCompleted)
                {
                    if (this.CancellationPending)
                        break;

                    // Wait for more videos?
                    while (count == this.Videos.Count)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    // Reset variable(s)
                    _downloaderSuccessful = null;
                    _downloader.Files.Clear();

                    // Get video, then increment count variable after
                    var video = this.Videos[count++];

                    if (video.Failure)
                    {
                        // Something failed retrieving video info
                        _failures++;
                        continue;
                    }

                    var format = Helper.GetPreferredFormat(video, _preferredQuality);

                    // Update properties for new video
                    this.ReportProgress(-1, new Dictionary<string, object>()
                    {
                        { nameof(Title), $"({count}/{this.Videos.Count}) {video.Title}" },
                        { nameof(Duration), video.Duration },
                        { nameof(FileSize), format.FileSize }
                    });

                    string prefix = _prefix ? count + ". " : string.Empty;
                    string finalFile = Path.Combine(this.Output,
                        $"{prefix}{Helper.FormatTitle(format.VideoInfo.Title)}.{format.Extension}");

                    this.DownloadedFiles.Add(finalFile);

                    if (_ignoreExisting == false)
                    {
                        // Overwrite if finalFile already exists
                        Helper.DeleteFiles(finalFile);
                    }
                    else if (File.Exists(finalFile))
                    {
                        _downloads++;
                        this.ReportProgress(EventFileDownloadComplete, finalFile);
                        _logger.LogLine($"Skipped existing {finalFile}...");
                        goto finish;
                    }

                    if (format.AudioOnly)
                    {
                        _downloader.Files.Add(new FileDownload(finalFile, format.DownloadUrl));
                    }
                    else
                    {
                        VideoFormat audioFormat = Helper.GetAudioFormat(format);
                        // Add '_audio' & '_video' to end of filename. Only get filename, not full path.
                        string audioFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_audio$2");
                        string videoFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_video$2");

                        // Download audio and video
                        _downloader.Files.Add(new FileDownload(audioFile, audioFormat.DownloadUrl));
                        _downloader.Files.Add(new FileDownload(videoFile, format.DownloadUrl));

                        // Delete _audio and _video files in case they exists from a previous attempt
                        Helper.DeleteFiles(_downloader.Files[0].Path,
                                           _downloader.Files[1].Path);
                    }

                    _downloader.Start();

                    // Wait for downloader to finish
                    while (_downloader.IsBusy || _downloader.IsPaused)
                    {
                        if (this.CancellationPending)
                        {
                            _downloader.Stop();
                            break;
                        }

                        Thread.Sleep(200);
                    }

                    if (_downloaderSuccessful == true)
                    {
                        // Combine video and audio if necessary
                        if (!format.AudioOnly)
                        {
                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { nameof(Progress), 0 }
                            });
                            this.ReportProgress(ProgressMax, null);

                            Exception combineException;

                            if (!OperationHelpers.Combine(
                                    _downloader.Files[0].Path,
                                    _downloader.Files[1].Path,
                                    this.Title,
                                    _logger,
                                    out combineException,
                                    this.ReportProgress))
                            {
                                _failures++;
                            }

                            this.ErrorsInternal.Add(combineException.Message);

                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { nameof(Progress), 0 }
                            });
                        }

                        _downloads++;
                        this.ReportProgress(EventFileDownloadComplete, finalFile);
                    }
                    else if (_downloaderSuccessful == false)
                    {
                        // Download failed, cleanup and continue
                        _failures++;
                        // Delete all related files. Helper method will check if it exists, throwing no errors
                        Helper.DeleteFiles(_downloader.Files.Select(x => x.Path).ToArray());
                    }

                finish:

                    // Reset before starting new download.
                    this.ReportProgress(ProgressMin, null);
                }

                // Throw stored exception if it exists. For example TimeoutException from 'GetPlaylistInfoAsync'
                if (_operationException != null)
                    throw _operationException;

                e.Result = this.CancellationPending ? OperationStatus.Canceled : OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);
                e.Result = OperationStatus.Failed;
                _operationException = ex;
            }
            finally
            {
                _logger?.Close();
                _logger = null;
            }
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case EventFileDownloadComplete:
                    this.OnFileDownloadComplete(e.UserState as string);
                    break;
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

        private void OnFileDownloadComplete(string file)
        {
            this.FileDownloadComplete?.Invoke(this, file);
        }
    }
}
