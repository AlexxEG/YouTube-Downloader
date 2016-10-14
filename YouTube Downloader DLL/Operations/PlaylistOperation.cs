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
using YouTube_Downloader_DLL.FFmpeg;
using YouTube_Downloader_DLL.FileDownloading;

namespace YouTube_Downloader_DLL.Operations
{
    public class PlaylistOperation : Operation
    {
        class ArgKeys
        {
            public const int Max = 4;
            public const int Min = 3;
            public const string Input = "input";
            public const string Output = "output";
            public const string PreferredQuality = "preferred_quality";
            public const string Videos = "videos";
        }

        public const int EventCombined = 1000;
        public const int EventCombining = 1001;
        public const int EventFileDownloadComplete = 1002;

        int _downloads = 0;
        int _failures = 0;
        int _preferredQuality;
        int _selectedVideosCount = 0;
        bool _queryingVideos = false;
        bool _processing;
        bool? _downloaderSuccessful;

        List<QuickVideoInfo> _videos = new List<QuickVideoInfo>();

        Exception _operationException;
        FileDownloader _downloader;
        OperationLogger _ffmpegLogger;

        public string PlaylistName { get; private set; }
        public List<string> DownloadedFiles { get; set; } = new List<string>();
        public List<VideoInfo> Videos { get; set; } = new List<VideoInfo>();

        public event EventHandler Combined;
        public event EventHandler Combining;
        /// <summary>
        /// Occurs when a single file download from the playlist is complete.
        /// </summary>
        public event EventHandler<string> FileDownloadComplete;

        public PlaylistOperation()
        {
        }

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
            return this.IsPaused || this.IsWorking;
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
            return true;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            switch ((OperationStatus)e.Result)
            {
                case OperationStatus.Canceled:
                    // Tell user how many videos was downloaded before being canceled, if any
                    if (this.Videos.Count == 0)
                        this.Title = $"Playlist canceled";
                    else
                        this.Title = $"\"{PlaylistName}\" canceled. {_downloads} of {Videos.Count} video(s) downloaded";
                    return;
                case OperationStatus.Failed:
                    // Tell user about known exceptions. Otherwise just a simple failed message
                    if (_operationException is TimeoutException)
                        this.Title = $"Timeout. Couldn't get playlist information";
                    else
                    {
                        if (string.IsNullOrEmpty(PlaylistName))
                            this.Title = $"Couldn't download playlist";
                        else
                            this.Title = $"Couldn't download \"{PlaylistName}\"";
                    }
                    return;
            }

            // If code reaches here, it means operation was successful
            if (_failures == 0)
            {
                // All videos downloaded successfully
                this.Title = string.Format("Downloaded \"{0}\" playlist. {1} video(s)",
                    this.PlaylistName, this.Videos.Count);
            }
            else
            {
                // Some or all videos failed. Tell user how many
                this.Title = string.Format("Downloaded \"{0}\" playlist. {1} of {2} video(s), {3} failed",
                    this.PlaylistName, _downloads, this.Videos.Count, _failures);
            }
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            /* ToDo:
             * 
             * [ ] Handle TimeoutException from PlaylistReader in 'GetPlaylistInfoAsync' somehow.
             *     Can't catch it here, needs to catch it inside 'GetPlaylistInfoAsync'.
             */

            this.GetPlaylistInfoAsync();

            try
            {
                int count = 0;

                while (count < this.Videos.Count || _queryingVideos)
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

                    count++;

                    var video = this.Videos[count - 1];
                    VideoFormat format = Helper.GetPreferredFormat(video, _preferredQuality);

                    // Update properties for new video
                    this.ReportProgress(-1, new Dictionary<string, object>()
                    {
                        { nameof(Title), $"({count}/{_selectedVideosCount}) {video.Title}" },
                        { nameof(Duration), video.Duration },
                        { nameof(FileSize), format.FileSize }
                    });

                    string finalFile = Path.Combine(this.Output,
                        $"{Helper.FormatTitle(format.VideoInfo.Title)}.{format.Extension}");

                    // Overwrite if finalFile already exists
                    Helper.DeleteFiles(finalFile);

                    this.DownloadedFiles.Add(finalFile);

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
                                { nameof(Text), "Combining..." },
                                { nameof(Progress), 0 },
                                { nameof(ProgressTextOverride), "Combining..." }
                            });
                            this.ReportProgress(ProgressMax, null);

                            if (!this.Combine())
                                _failures++;

                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { nameof(Text), string.Empty },
                                { nameof(Progress), 0 },
                                { nameof(ProgressTextOverride), string.Empty }
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
                _ffmpegLogger?.Close();
            }
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case EventCombined:
                    this.OnCombined();
                    break;
                case EventCombining:
                    this.OnCombining();
                    break;
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

        protected override void WorkerStart(Dictionary<string, object> args)
        {
            if (!(args.Count.Any(ArgKeys.Min, ArgKeys.Max)))
                throw new ArgumentException();

            // Temporary title.
            this.Title = "Getting playlist info...";
            this.ReportsProgress = true;

            this.Input = (string)args[ArgKeys.Input];
            this.Output = (string)args[ArgKeys.Output];
            this.Link = this.Input;

            _preferredQuality = (int)args[ArgKeys.PreferredQuality];

            if (args.Count == ArgKeys.Max)
            {
                if (args[ArgKeys.Videos] != null)
                    _videos.AddRange((IEnumerable<QuickVideoInfo>)args[ArgKeys.Videos]);
            }

            _downloader = new FileDownloader();

            // Attach events
            _downloader.Canceled += downloader_Canceled;
            _downloader.Completed += downloader_Completed;
            _downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            _downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            _downloader.ProgressChanged += downloader_ProgressChanged;
        }

        private bool Combine()
        {
            string audio = _downloader.Files[0].Path;
            string video = _downloader.Files[1].Path;
            // Remove '_video' from video file to get a final filename.
            string output = video.Replace("_video", string.Empty);
            FFmpegResult<bool> result = null;

            try
            {
                // Raise events on main thread
                this.ReportProgress(EventCombining, null);

                if (_ffmpegLogger == null)
                    _ffmpegLogger = OperationLogger.Create(OperationLogger.FFmpegDLogFile);

                result = FFmpegHelper.Combine(_ffmpegLogger, video, audio, output, delegate (int percentage)
                {
                    // Combine progress
                    this.ReportProgress(percentage, null);
                });

                // Save errors if combining failed
                if (!result.Value)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(this.Title);

                    foreach (string error in result.Errors)
                        sb.AppendLine($" - {error}");

                    this.ErrorsInternal.Add(sb.ToString());
                }

                // Cleanup the separate audio and video files
                Helper.DeleteFiles(audio, video);
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);
                return false;
            }
            finally
            {
                // Raise events on main thread
                this.ReportProgress(EventCombined, null);
            }

            return result.Value;
        }

        private void OnCombined()
        {
            this.Combined?.Invoke(this, EventArgs.Empty);
        }

        private void OnCombining()
        {
            this.Combining?.Invoke(this, EventArgs.Empty);
        }

        private void OnFileDownloadComplete(string file)
        {
            this.FileDownloadComplete?.Invoke(this, file);
        }
        
        private async void GetPlaylistInfoAsync()
        {
            _queryingVideos = true;

            await Task.Run(delegate
            {
                var items = new List<int>();

                foreach (var v in _videos)
                    items.Add(v.Index);

                var reader = new PlaylistReader(this.Input, items.ToArray());
                VideoInfo video;

                try
                {
                    this.PlaylistName = reader.WaitForPlaylist().Name;
                }
                catch (TimeoutException ex)
                {
                    _operationException = ex;
                    _queryingVideos = false;
                    return;
                }

                if (_videos.Count == 0)
                    _selectedVideosCount = reader.Playlist.OnlineCount;
                else
                    _selectedVideosCount = _videos.Count;

                while ((video = reader.Next()) != null)
                {
                    // We're done! (I think)
                    if (_videos.Count > 0 && this.Videos.Count == _videos.Count)
                        break;

                    if (this.CancellationPending)
                    {
                        reader.Stop();
                        break;
                    }

                    if (_videos.Count == 0 || _videos.Any(x => x.ID == video.ID))
                        this.Videos.Add(video);
                }
            });

            _queryingVideos = false;
        }

        public Dictionary<string, object> Args(string url,
                                               string output,
                                               int preferredQuality)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Input, url },
                { ArgKeys.Output, output },
                { ArgKeys.PreferredQuality, preferredQuality }
            };
        }

        public Dictionary<string, object> Args(string url,
                                               string output,
                                               int preferredQuality,
                                               ICollection<QuickVideoInfo> videos)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Input, url },
                { ArgKeys.Output, output },
                { ArgKeys.PreferredQuality, preferredQuality },
                { ArgKeys.Videos, videos }
            };
        }
    }
}
