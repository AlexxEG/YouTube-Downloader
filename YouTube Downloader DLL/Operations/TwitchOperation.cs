using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.FFmpeg;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader_DLL.Operations
{
    public class TwitchOperation : Operation
    {
        class ArgKeys
        {
            public const int Count = 2;
            public const string Output = "output";
            public const string Format = "format";
        }

        private class ProgressReport
        {
            public long SpeedInBytes { get; private set; }
            public long TotalDownloaded { get; private set; }
            public long TotalEstimated { get; private set; }
            public string Speed { get; private set; }

            public ProgressReport(long speedInBytes, long totalDownloaded, long totalEstimated, string speed)
            {
                this.SpeedInBytes = speedInBytes;
                this.TotalDownloaded = totalDownloaded;
                this.TotalEstimated = totalEstimated;
                this.Speed = speed;
            }
        }

        bool _cancel = false;
        bool _combining = false;
        bool _processing = false;
        bool _pause = false;
        VideoFormat _format;

        public TwitchOperation(VideoFormat format)
        {
            this.Duration = format.VideoInfo.Duration;
            this.FileSize = format.FileSize;
            this.Link = format.VideoInfo.Url;
            this.ReportsProgress = true;
            this.Thumbnail = format.VideoInfo.ThumbnailUrl;
            this.Title = format.VideoInfo.Title;
        }

        #region Operation members

        public override bool CanOpen()
        {
            return this.IsSuccessful;
        }

        public override bool CanPause()
        {
            return !_combining && this.IsWorking;
        }

        public override bool CanResume()
        {
            return _pause || this.IsQueued;
        }

        public override bool CanStop()
        {
            return !_combining && this.IsWorking;
        }

        public override void Dispose()
        {
            base.Dispose();
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
            _pause = true;

            this.Status = OperationStatus.Paused;
        }

        public override void Queue()
        {
            _pause = true;

            this.Status = OperationStatus.Queued;
        }

        protected override void ResumeInternal()
        {
            _pause = false;

            this.Status = OperationStatus.Working;
        }

        public override bool Stop()
        {
            _cancel = true;

            // Don't set status to canceled if already successful.
            if (this.Status != OperationStatus.Success)
                this.Status = OperationStatus.Canceled;

            return true;
        }

        #endregion

        protected override void WorkerCompleted(RunWorkerCompletedEventArgs e) { }

        protected override void WorkerDoWork(DoWorkEventArgs e)
        {
            try
            {
                using (var logger = OperationLogger.Create(OperationLogger.FFmpegDLogFile))
                {
                    string tempFilename = this.Output.Substring(0, this.Output.LastIndexOf('.') + 1) + "ts";

                    if (this.Download(tempFilename))
                        this.Optimize(logger, tempFilename);
                    else
                    {
                        // Download was canceled
                        this.Cleanup(tempFilename);
                    }
                }

                // Make sure progress reaches 100%
                if (this.Progress < ProgressMax)
                    this.ReportProgress(ProgressMax, null);

                e.Result = OperationStatus.Success;
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);
                e.Result = OperationStatus.Failed;
            }
        }

        protected override void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                // Used to set multiple properties
                if (e.UserState is Dictionary<string, object>)
                {
                    foreach (var pair in (e.UserState as Dictionary<string, object>))
                    {
                        this.GetType().GetProperty(pair.Key).SetValue(this, pair.Value);
                    }
                }
            }
            else
            {
                if (_processing)
                    return;

                if (e.UserState is ProgressReport)
                {
                    _processing = true;

                    var progressReport = e.UserState as ProgressReport;
                    long longETA = Helper.GetETA((int)progressReport.SpeedInBytes, progressReport.TotalEstimated, progressReport.TotalDownloaded);
                    string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(longETA * 1000) + " ]";

                    this.ETA = ETA;
                    this.Speed = $"{progressReport.Speed}";
                    this.Progress = progressReport.TotalDownloaded;

                    _processing = false;
                }
            }
        }

        protected override void WorkerStart(Dictionary<string, object> args)
        {
            if (args.Count != ArgKeys.Count)
                throw new ArgumentException();

            this.Output = (string)args[ArgKeys.Output];

            _format = (VideoFormat)args[ArgKeys.Format];
        }

        private void Cleanup(string tempFilename)
        {
            Helper.DeleteFiles(tempFilename);
        }

        private bool Download(string outputFilename)
        {
            var wc = new WebClient();
            var m3u8 = wc.DownloadString(_format.DownloadUrl);
            var sr = new StringReader(m3u8);
            var line = string.Empty;
            int partsDone = 0;
            List<string> parts = new List<string>();

            long partMaxSize = 0;
            long estimatedTotalSize = 0;

            using (var writer = new FileStream(outputFilename,
                                               FileMode.Create,
                                               FileAccess.Write))
            {
                long totalDownloaded = 0;
                long prevFileSize = 0;
                var sw = new Stopwatch();

                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    parts.Add(line);
                }

                foreach (string part in parts)
                {
                    if (_cancel)
                        break;

                    while (_pause)
                        Thread.Sleep(500);

                    partsDone++;
                    sw.Start();

                    string url = _format.DownloadUrl.Substring(0, _format.DownloadUrl.LastIndexOf('/') + 1) + part;
                    byte[] data = wc.DownloadData(url);

                    writer.Write(data, 0, data.Length);
                    totalDownloaded += data.Length;

                    if (data.Length > partMaxSize)
                    {
                        partMaxSize = data.Length;
                        estimatedTotalSize = partMaxSize * parts.Count;

                        this.ReportProgress(-1, new Dictionary<string, object>()
                        {
                            { nameof(FileSize), estimatedTotalSize }
                        });
                    }

                    if (sw.ElapsedMilliseconds >= 1000)
                    {
                        long downloadedBytes = writer.Length - prevFileSize;
                        prevFileSize = writer.Length;

                        string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloadedBytes);
                        double percentage = Math.Round((double)totalDownloaded / estimatedTotalSize * 100, 2);

                        this.ReportProgress((int)percentage, new ProgressReport(downloadedBytes, totalDownloaded, estimatedTotalSize, speed));

                        sw.Reset();
                    }
                }
            }

            return !_cancel;
        }

        private void Optimize(OperationLogger logger, string tsFile)
        {
            new FFmpegProcess(logger).FixM3U8(tsFile, this.Output);
        }

        public static Dictionary<string, object> Args(string output,
                                                      VideoFormat format)
        {
            return new Dictionary<string, object>()
            {
                { ArgKeys.Output,  output },
                { ArgKeys.Format, format }
            };
        }
    }
}
