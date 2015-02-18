using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF.Operations
{
    public class PlaylistOperation : Operation
    {
        bool _combining, _processing, _useDash;
        bool? _downloaderSuccessful;
        ICollection<VideoInfo> _videos;
        FileDownloader downloader;

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

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            // If one or more files fail, whole operation failed. Might handle it more
            // elegantly in the future.
            _downloaderSuccessful = false;
            downloader.Stop(false);
        }

        private void downloader_FileSizesCalculationComplete(object sender, EventArgs e)
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
                this.ProgressPercentage = downloader.TotalPercentage();
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

        protected override void OnWorkerDoWork(DoWorkEventArgs e)
        {
            try
            {
                int count = 0;

                if (_videos == null)
                {
                    VideoInfo video;
                    PlaylistReader reader = new PlaylistReader(this.Input);

                    _videos = new List<VideoInfo>();

                    while (!this.CancellationPending && (video = reader.Next()) != null)
                    {
                        _videos.Add(video);
                    }
                }

                foreach (VideoInfo video in _videos)
                {
                    if (this.CancellationPending)
                        break;

                    count++;

                    VideoFormat videoFormat = Helper.GetPreferedFormat(video, _useDash);

                    this.Title = string.Format("({0}/{1}) {2}", count, _videos.Count, video.Title);
                    this.Duration = video.Duration;
                    this.FileSize = videoFormat.FileSize;

                    DownloadFile[] downloadFiles;

                    string finalFile = Path.Combine(this.Output, Helper.FormatTitle(videoFormat.VideoInfo.Title) + "." + videoFormat.Extension);

                    if (!_useDash)
                    {
                        downloadFiles = new DownloadFile[]
                        {
                            new DownloadFile(finalFile, videoFormat.DownloadUrl)
                        };
                    }
                    else
                    {
                        VideoFormat audioFormat = Helper.GetAudioFormat(videoFormat);
                        // Add '_audio' & '_video' to end of filename. Only get filename, not full path.
                        string audioFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_audio$2");
                        string videoFile = Regex.Replace(finalFile, @"^(.*)(\..*)$", "$1_video$2");

                        downloadFiles = new DownloadFile[]
                        {
                            new DownloadFile(audioFile, audioFormat.DownloadUrl),
                            new DownloadFile(videoFile, videoFormat.DownloadUrl)
                        };
                    }

                    // Reset variable(s)
                    _downloaderSuccessful = null;

                    downloader.Files.Clear();
                    downloader.Files.AddRange(downloadFiles);
                    downloader.Start();

                    // Wait for downloader to finish
                    while (downloader.IsBusy || downloader.IsPaused)
                        Thread.Sleep(200);

                    if (_useDash && _downloaderSuccessful == true)
                    {
                        this.Text = "Combining...";
                        this.ReportsProgress = false;
                        this.Combine();
                        this.ReportsProgress = true;
                        this.Text = string.Empty;
                    }

                    // Reset before starting new download.
                    this.ProgressPercentage = Min_Progress;
                }

                e.Cancel = this.CancellationPending;
                e.Result = e.Cancel ? OperationStatus.Canceled : OperationStatus.Success;
            }
            catch (Exception ex)
            {
                App.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        protected override void OnWorkerStart(object[] args)
        {
            if (!(args.Length == 3 || args.Length == 4))
                throw new ArgumentException();

            // Temporary title.
            this.Title = "Getting playlist info...";
            this.ReportsProgress = true;

            this.Input = (string)args[0];
            this.Output = (string)args[1];
            this.Link = this.Input;

            _useDash = (bool)args[2];

            if (args.Length == 4)
                _videos = (ICollection<VideoInfo>)args[3];

            downloader = new FileDownloader();
            downloader.Directory = this.Output;

            // Attach downloader events
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.CalculatedTotalFileSize += downloader_FileSizesCalculationComplete;
            downloader.ProgressChanged += downloader_ProgressChanged;
        }

        private bool Combine()
        {
            string audio = Path.Combine(downloader.Directory, downloader.Files[0].Name);
            string video = Path.Combine(downloader.Directory, downloader.Files[1].Name);
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
                App.SaveException(ex);
                return false;
            }
            finally
            {
                _combining = false;
            }

            return true;
        }

        public static object[] Args(string url, string output, bool dash)
        {
            return new object[] { url, output, dash };
        }

        public static object[] Args(string url, string output, bool dash, ICollection<VideoInfo> videos)
        {
            return new object[] { url, output, dash, videos };
        }
    }
}
