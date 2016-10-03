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
using YouTube_Downloader_DLL.Helpers;

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
        bool _combining, _processing;
        bool? _downloaderSuccessful;

        List<string> _videoIds = new List<string>();

        Exception _operationException;
        FileDownloader downloader;

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

            e.Exception.Data.Add("FileDownload", e.FileDownload);

            Common.SaveException(e.Exception);
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
                string eta = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

                this.ETA = eta;
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

        public override bool CanPause()
        {
            // Can only pause if currently downloading
            return !_combining && downloader != null && downloader.CanPause;
        }

        public override bool CanResume()
        {
            // Can only resume downloader
            return !_combining && downloader != null && downloader.CanResume;
        }

        public override bool CanStop()
        {
            return this.Status == OperationStatus.Working;
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
            // Only the downloader can be paused.
            if (downloader.CanPause)
            {
                downloader.Pause();
                this.Status = OperationStatus.Paused;
            }
        }

        public override void Resume()
        {
            // Only the downloader can be resumed.
            if (downloader.CanResume)
            {
                downloader.Resume();
                this.Status = OperationStatus.Working;
            }
        }

        public override bool Stop(bool cleanup)
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
             * [x] 'this.Videos' will always be empty now. Instead '_videoIds' is to be used to only download selected videos
             * 
             * [x] Get video information in background while downloading available videos. 
             *     That means it will query video information while it's downloading
             *   
             * [ ] Handle TimeoutException from PlaylistReader in 'GetPlaylistInfoAsync' somehow
             */

            // Start querying for video information
            this.GetPlaylistInfoAsync();

            try
            {
                int count = 0;

                while (count < this.Videos.Count || _queryingVideos)
                {
                    if (this.CancellationPending)
                        break;

                    while (count == this.Videos.Count)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    // Reset variable(s)
                    _downloaderSuccessful = null;
                    downloader.Files.Clear();

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

                    this.DownloadedFiles.Add(finalFile);

                    if (format.AudioOnly)
                    {
                        downloader.Files.Add(new FileDownload(finalFile, format.DownloadUrl));
                    }
                    else
                    {
                        VideoFormat audioFormat = Helper.GetAudioFormat(format);
                        // Add '_audio' & '_video' to end of filename. Only get filename, not full path.
                        string audioFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_audio$2");
                        string videoFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_video$2");

                        // Download audio and video
                        downloader.Files.Add(new FileDownload(audioFile, audioFormat.DownloadUrl));
                        downloader.Files.Add(new FileDownload(videoFile, format.DownloadUrl));
                    }

                    downloader.Start();

                    // Wait for downloader to finish
                    while (downloader.IsBusy || downloader.IsPaused)
                    {
                        if (this.CancellationPending)
                        {
                            downloader.Stop(false);
                            break;
                        }

                        Thread.Sleep(200);
                    }

                    // Download successful. Combine video & audio if necessary
                    if (_downloaderSuccessful == true)
                    {
                        if (!format.AudioOnly)
                        {
                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { nameof(Text), "Combining..." },
                                { nameof(ReportsProgress), false },
                                { nameof(Progress), 0 },
                                { nameof(ProgressTextOverride), "Combining..." }
                            });
                            this.ReportProgress(ProgressMax, null);

                            if (!this.Combine())
                                _failures++;

                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { nameof(Text), string.Empty },
                                { nameof(ReportsProgress), true },
                                { nameof(Progress), 0 },
                                { nameof(ProgressTextOverride), string.Empty }
                            });
                        }

                        _downloads++;
                        this.ReportProgress(EventFileDownloadComplete, finalFile);
                    }
                    // Download failed, cleanup and continue
                    else if (_downloaderSuccessful == false)
                    {
                        _failures++;
                        // Delete all related files. Helper method will check if it exists, throwing no errors
                        Helper.DeleteFiles(downloader.Files.Select(x => x.Path).ToArray());
                    }

                    // Reset before starting new download.
                    this.ReportProgress(ProgressMin, null);
                }

                e.Result = this.CancellationPending ? OperationStatus.Canceled : OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);
                e.Result = OperationStatus.Failed;
                _operationException = ex;
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
                    _videoIds.AddRange((IEnumerable<string>)args[ArgKeys.Videos]);
            }

            downloader = new FileDownloader();

            // Attach events
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            downloader.ProgressChanged += downloader_ProgressChanged;
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

        private bool Combine()
        {
            string audio = downloader.Files[0].Path;
            string video = downloader.Files[1].Path;
            // Remove '_video' from video file to get a final filename.
            string output = video.Replace("_video", string.Empty);
            FFmpegResult<bool> result = null;

            _combining = true;

            try
            {
                // Raise events on main thread
                this.ReportProgress(EventCombining, null);

                result = FFmpegHelper.Combine(video, audio, output);

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
                _combining = false;

                // Raise events on main thread
                this.ReportProgress(EventCombined, null);
            }

            return result.Value;
        }

        private async void GetPlaylistInfoAsync()
        {
            _queryingVideos = true;

            await Task.Run(delegate
            {
                var reader = new PlaylistReader(this.Input);
                VideoInfo video;

                this.PlaylistName = reader.WaitForPlaylist().Name;

                if (_videoIds.Count == 0)
                    _selectedVideosCount = reader.Playlist.OnlineCount;
                else
                    _selectedVideosCount = _videoIds.Count;

                while ((video = reader.Next()) != null)
                {
                    // We're done! (I think)
                    if (this.Videos.Count == _videoIds.Count)
                        break;

                    if (this.CancellationPending)
                    {
                        reader.Stop();
                        break;
                    }

                    if (_videoIds.Count == 0 || _videoIds.Contains(video.ID))
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
                                               ICollection<string> videoIds)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Input, url },
                { ArgKeys.Output, output },
                { ArgKeys.PreferredQuality, preferredQuality },
                { ArgKeys.Videos, videoIds }
            };
        }
    }
}
