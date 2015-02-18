using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using DeDauwJeroen;
using YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF.Operations
{
    public class DownloadOperation : Operation
    {
        bool _combining, _dash, _processing;
        FileDownloader downloader;

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
                this.Status = OperationStatus.Success;
        }

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            // If one or more files fail, whole operation failed. Might handle it more
            // elegantly in the future.
            this.Status = OperationStatus.Failed;
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

                string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";

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

        public override bool Stop(bool cleanup)
        {
            // Stop downloader if still running.
            if (downloader != null && downloader.CanStop)
                downloader.Stop(false);

            // Don't set status to canceled if already successful.
            if (this.Status != OperationStatus.Success)
                this.Status = OperationStatus.Canceled;

            // Cleanup if cleanup is true and download wasn't successful.
            if (cleanup && this.Status != OperationStatus.Success)
            {
                if (File.Exists(this.Output))
                    Helper.DeleteFiles(this.Output);
            }

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
            if (this.Status == OperationStatus.Paused || this.Status == OperationStatus.Working)
                // Only downloader can stop, not the combiner currently.
                return !_combining && downloader.CanStop;
            else
                return true;
        }

        #endregion

        protected override void OnWorkerDoWork(DoWorkEventArgs e)
        {
            while (downloader.IsBusy)
                Thread.Sleep(200);

            if (_dash && this.Status == OperationStatus.Success)
            {
                _combining = true;

                string audio = downloader.LocalDirectory + "\\" + downloader.Files[0].Name;
                string video = downloader.LocalDirectory + "\\" + downloader.Files[1].Name;

                this.Text = "Combining...";
                this.ReportsProgress = false;
                this.Progress = 0;
                this.ProgressPercentage = Max_Progress;

                try
                {
                    FFmpegHelper.CombineDash(video, audio, this.Output);

                    // Delete the separate audio & video files.
                    Helper.DeleteFiles(audio, video);

                    e.Result = OperationStatus.Success;
                }
                catch (Exception ex)
                {
                    App.SaveException(ex);
                    e.Result = OperationStatus.Failed;
                }
            }

            e.Result = this.Status;
        }

        protected override void OnWorkerProgressChanged(ProgressChangedEventArgs e)
        {
            base.OnWorkerProgressChanged(e);
        }

        protected override void OnWorkerStart(object[] args)
        {
            this.ReportsProgress = true;

            FileDownloader.FileInfo[] fileInfos;

            if (args.Length == 2)
            {
                this.Input = (string)args[0];
                this.Output = (string)args[1];

                string file = Path.GetFileName(this.Output).Trim();

                fileInfos = new FileDownloader.FileInfo[]
                {
                    new FileDownloader.FileInfo(this.Input)
                    {
                        Name = file
                    }
                };
            }
            else if (args.Length == 3)
            {
                _dash = true;
                this.Input = string.Format("{0}|{1}", args[0], args[1]);
                this.Output = (string)args[2];

                Regex regex = new Regex(@"^\w:.*\\(.*)(\..*)$");

                fileInfos = new FileDownloader.FileInfo[]
                {
                    new FileDownloader.FileInfo((string)args[0])
                    {
                        Name = regex.Replace(this.Output, "$1_audio$2")
                    },
                    new FileDownloader.FileInfo((string)args[1])
                    {
                        Name = regex.Replace(this.Output, "$1_video$2")
                    }
                };
            }
            else
            {
                throw new ArgumentException();
            }

            string folder = Path.GetDirectoryName(this.Output);

            downloader = new FileDownloader(true);
            downloader.Files.AddRange(fileInfos);
            downloader.LocalDirectory = folder;

            // Attach events.
            downloader.Canceled += downloader_Canceled;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += downloader_FileDownloadFailed;
            downloader.FileSizesCalculationComplete += downloader_FileSizesCalculationComplete;
            downloader.ProgressChanged += downloader_ProgressChanged;

            downloader.Start();
        }

        public static object[] Args(string url, string output)
        {
            return new object[] { url, output };
        }

        public static object[] Args(string audio, string video, string output)
        {
            return new object[] { audio, video, output };
        }
    }
}