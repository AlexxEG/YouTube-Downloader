using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader_DLL.Operations
{
    public class PlaylistOperation : Operation
    {
        /// <summary>
        /// Constant used to identify a FileDownloadComplete event in ProgressChanged handler.
        /// </summary>
        const int EventFileDownloadComplete = 1000;

        int _downloads = 0;
        int _failures = 0;
        int _preferredQuality;
        bool _combining, _processing, _useDash;
        bool? _downloaderSuccessful;

        Exception _operationException;
        FileDownloader downloader;

        public string PlaylistName { get; private set; }
        public List<string> DownloadedFiles { get; set; }
        public List<VideoInfo> Videos { get; set; }

        /// <summary>
        /// Occurs when a single file download from the playlist is complete.
        /// </summary>
        public event EventHandler<string> FileDownloadComplete;

        public PlaylistOperation()
        {
            this.DownloadedFiles = new List<string>();
            this.Videos = new List<VideoInfo>();
        }

        private void downloader_Canceled(object sender, EventArgs e)
        {
            // Pass the event along to a almost identical event handler.
            downloader_Completed(sender, e);
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
                string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

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
            return !_combining && downloader != null && downloader.CanPause;
        }

        public override bool CanResume()
        {
            return !_combining && downloader != null && downloader.CanResume;
        }

        public override bool CanStop()
        {
            return !_combining && downloader != null && downloader.CanStop;
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
            if (downloader.CanStop)
            {
                downloader.Stop(cleanup);
                _downloaderSuccessful = false;
            }

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
                    if (string.IsNullOrEmpty(this.Title))
                        this.Title = $"Playlist canceled";
                    else
                        this.Title = $"\"{PlaylistName}\" canceled. {_downloads} of {Videos.Count} videos downloaded";
                    return;
                case OperationStatus.Failed:
                    // Tell user about known exceptions. Otherwise just a simple failed message
                    if (_operationException is TimeoutException)
                        this.Title = $"Timeout. Couldn't get playlist information";
                    else
                    {
                        if (string.IsNullOrEmpty(this.Title))
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
                this.Title = string.Format("Downloaded \"{0}\" playlist. {1} videos",
                    this.PlaylistName, this.Videos.Count);
            }
            else
            {
                // Some or all videos failed. Tell user how many
                this.Title = string.Format("Downloaded \"{0}\" playlist. {1} of {2} videos, {3} failed",
                    this.PlaylistName, _downloads, this.Videos.Count, _failures);
            }
        }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            try
            {
                if (this.Videos.Count == 0)
                    this.GetPlaylistInfo();
            }
            catch (TimeoutException ex)
            {
                e.Result = OperationStatus.Failed;
                _operationException = ex;
                return;
            }

            try
            {
                int count = 0;

                foreach (VideoInfo video in this.Videos)
                {
                    if (this.CancellationPending)
                        break;

                    count++;

                    VideoFormat videoFormat = Helper.GetPreferredFormat(video, _useDash, _preferredQuality);

                    this.ReportProgress(-1, new Dictionary<string, object>()
                    {
                        { "Title", string.Format("({0}/{1}) {2}", count, this.Videos.Count, video.Title) },
                        { "Duration", video.Duration },
                        { "FileSize", videoFormat.FileSize }
                    });

                    FileDownload[] fileDownloads;
                    string finalFile = Path.Combine(this.Output, Helper.FormatTitle(videoFormat.VideoInfo.Title) + "." + videoFormat.Extension);

                    this.DownloadedFiles.Add(finalFile);

                    if (!_useDash)
                    {
                        fileDownloads = new FileDownload[]
                        {
                            new FileDownload(finalFile, videoFormat.DownloadUrl)
                        };
                    }
                    else
                    {
                        VideoFormat audioFormat = Helper.GetAudioFormat(videoFormat);
                        // Add '_audio' & '_video' to end of filename. Only get filename, not full path.
                        string audioFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_audio$2");
                        string videoFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_video$2");

                        fileDownloads = new FileDownload[]
                        {
                            new FileDownload(audioFile, audioFormat.DownloadUrl),
                            new FileDownload(videoFile, videoFormat.DownloadUrl)
                        };
                    }

                    // Reset variable(s)
                    _downloaderSuccessful = null;

                    downloader.Files.Clear();
                    downloader.Files.AddRange(fileDownloads);
                    downloader.Start();

                    // Wait for downloader to finish
                    while (downloader.IsBusy || downloader.IsPaused)
                        Thread.Sleep(200);

                    // Download successful. Combine video & audio if download is a DASH video
                    if (_downloaderSuccessful == true)
                    {
                        if (_useDash)
                        {
                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { "Text", "Combining..." },
                                { "ReportsProgress", false }
                            });

                            this.Combine();

                            this.ReportProgress(-1, new Dictionary<string, object>()
                            {
                                { "Text", string.Empty },
                                { "ReportsProgress", true }
                            });
                        }

                        _downloads++;
                        this.ReportProgress(1000, finalFile);
                    }
                    // Download failed, cleanup and continue
                    else if (_downloaderSuccessful == false)
                    {
                        _failures++;
                        // Delete all related files. Helper method will check if it exists, throwing no errors
                        Helper.DeleteFiles(fileDownloads.Select(x => x.Path).ToArray());
                    }

                    // Reset before starting new download.
                    this.ReportProgress(ProgressMin, null);
                }

                e.Cancel = this.CancellationPending;
                e.Result = e.Cancel ? OperationStatus.Canceled : OperationStatus.Success;
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
            else if (e.ProgressPercentage == 1000) // FileDownloadComplete
            {
                OnFileDownloadComplete(e.UserState as string);
            }
        }

        protected override void WorkerStart(object[] args)
        {
            if (!(args.Length == 4 || args.Length == 6))
                throw new ArgumentException();

            // Temporary title.
            this.Title = "Getting playlist info...";
            this.ReportsProgress = true;

            this.Input = (string)args[0];
            this.Output = (string)args[1];
            this.Link = this.Input;

            _useDash = (bool)args[2];
            _preferredQuality = (int)args[3];

            if (args.Length == 6)
            {
                this.PlaylistName = (string)args[4];

                if (args[5] != null)
                    this.Videos.AddRange((IEnumerable<VideoInfo>)args[5]);
            }

            downloader = new FileDownloader();

            // Attach downloader events
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.CalculatedTotalFileSize += downloader_CalculatedTotalFileSize;
            downloader.ProgressChanged += downloader_ProgressChanged;
        }

        private bool Combine()
        {
            string audio = downloader.Files[0].Path;
            string video = downloader.Files[1].Path;
            // Remove '_video' from video file to get a final filename.
            string output = video.Replace("_video", string.Empty);

            _combining = true;

            try
            {
                FFmpegHelper.CombineDash(video, audio, output);

                // Cleanup the extra files.
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
            }

            return true;
        }

        private void OnFileDownloadComplete(string file)
        {
            FileDownloadComplete?.Invoke(this, file);
        }

        public void GetPlaylistInfo()
        {
            var reader = new PlaylistReader(this.Input);
            VideoInfo video;

            this.PlaylistName = reader.WaitForPlaylist(1000).Name;

            while (!this.CancellationPending && (video = reader.Next()) != null)
            {
                this.Videos.Add(video);
            }
        }

        public object[] Args(string url, string output, bool dash, int preferredQuality)
        {
            return new object[] { url, output, dash, preferredQuality };
        }

        public object[] Args(string url, string output, bool dash, int preferredQuality, string playlistName, ICollection<VideoInfo> videos)
        {
            return new object[] { url, output, dash, preferredQuality, playlistName, videos };
        }
    }
}
