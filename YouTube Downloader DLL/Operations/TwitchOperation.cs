using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.FFmpeg;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader_DLL.Operations
{
    public class TwitchOperation : Operation
    {
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
        bool _clipping = false;
        bool _combining = false;
        bool _processing = false;
        bool _pause = false;
        TimeSpan _clipFrom;
        TimeSpan _clipTo;
        VideoFormat _format;

        public TwitchOperation(VideoFormat format, string output)
        {
            this.ReportsProgress = true;
            this.Duration = format.VideoInfo.Duration;
            this.FileSize = format.FileSize;
            this.Link = format.VideoInfo.Url;
            this.Thumbnail = format.VideoInfo.ThumbnailUrl;
            this.Title = format.VideoInfo.Title;

            this.Output = output;
            _format = format;
        }

        public TwitchOperation(VideoFormat format,
                               string output,
                               TimeSpan clipFrom,
                               TimeSpan clipTo)
            : this(format, output)
        {
            _clipFrom = clipFrom;
            _clipTo = clipTo;
            _clipping = true;
        }

        #region Internal Classes

        class M3U8Part
        {
            public string Data { get; private set; }
            public string ExtData { get; private set; }
            public decimal Duration
            {
                get
                {
                    return decimal.Parse(ExtData.Substring(ExtData.IndexOf(':') + 1).TrimEnd(',').Replace('.', ','));
                }
            }

            public M3U8Part(string extData, string data)
            {
                this.Data = data;
                this.ExtData = extData;
            }
        }

        class ClipDurationRange
        {
            public int Start { get; private set; }
            public int Count { get; private set; }

            public ClipDurationRange(int start, int count)
            {
                this.Start = start;
                this.Count = count;
            }
        }

        #endregion

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
            return !_combining && (this.IsPaused || this.IsWorking || this.IsQueued);
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
            if (!this.IsSuccessful)
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
                        Helper.DeleteFiles(tempFilename);
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
                    //string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(longETA * 1000) + " ]";
                    string ETA = longETA == 0 ? "" : "  " + TimeSpan.FromMilliseconds(longETA * 1000).ToString(@"hh\:mm\:ss");

                    this.ETA = ETA;
                    this.Speed = $"{progressReport.Speed}";
                    this.Progress = progressReport.TotalDownloaded;

                    _processing = false;
                }
            }
        }

        private bool Download(string outputFilename)
        {
            var line = string.Empty;
            var extData = string.Empty;
            var parts = new List<M3U8Part>();
            var sw = new Stopwatch();

            long partMaxSize = 0;
            long estimatedTotalSize = 0;
            long totalDownloaded = 0;
            long prevFileSize = 0;

            using (var wc = new WebClient())
            using (var sr = new StringReader(wc.DownloadString(_format.DownloadUrl)))
            using (var writer = new FileStream(outputFilename, FileMode.Create, FileAccess.Write))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        // Skip empty lines
                        continue;
                    else if (line.StartsWith("#EXTINF:"))
                        // Store line with part duration
                        extData = line;
                    else if (line.StartsWith("#"))
                        // Skip other meta data lines
                        continue;
                    else
                    {
                        parts.Add(new M3U8Part(extData, line));
                        extData = string.Empty;
                    }
                }

                int start_index = 0;
                int count = parts.Count;

                if (_clipping)
                {
                    var range = this.GetDurationRange(parts, _clipFrom, _clipTo);
                    start_index = range.Start;
                    count = range.Count;

                    this.ReportProgress(-1, new Dictionary<string, object>()
                    {
                        { nameof(Duration), (long)(_clipTo - _clipFrom).TotalSeconds }
                    });
                }

                for (int i = start_index; i < (start_index + count); i++)
                {
                    if (_cancel)
                        break;

                    while (_pause)
                        Thread.Sleep(500);

                    sw.Start();

                    M3U8Part part = parts[i];
                    string url = _format.DownloadUrl.Substring(0, _format.DownloadUrl.LastIndexOf('/') + 1) + part.Data;
                    byte[] data = wc.DownloadData(url);

                    writer.Write(data, 0, data.Length);
                    totalDownloaded += data.Length;

                    if (data.Length > partMaxSize)
                    {
                        partMaxSize = data.Length;
                        estimatedTotalSize = partMaxSize * count;

                        this.ReportProgress(-1, new Dictionary<string, object>()
                        {
                            { nameof(FileSize), estimatedTotalSize }
                        });
                    }

                    if (sw.ElapsedMilliseconds >= Common.ProgressUpdateDelay)
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
            Helper.DeleteFiles(tsFile);
        }

        private ClipDurationRange GetDurationRange(IEnumerable<M3U8Part> parts, TimeSpan clipFrom, TimeSpan clipTo)
        {
            var lengths = new List<decimal>();
            foreach (M3U8Part part in parts)
            {
                lengths.Add(part.Duration);
            }

            if (clipFrom != TimeSpan.Zero && clipTo <= clipFrom)
                throw new Exception($"{nameof(clipFrom)} duration can't be less than or equal to {nameof(clipTo)} duration.");

            int i = 0;
            int count = 0;
            int start_index = -1;
            decimal l = 0;
            decimal skipped_l = 0;
            for (i = 0; i < lengths.Count; i++)
            {
                // First find start_index
                if (start_index == -1)
                {
                    if (clipFrom == TimeSpan.Zero)
                    {
                        start_index = i;
                        continue;
                    }

                    decimal new_l = skipped_l + lengths[i];

                    if (new_l < (decimal)clipFrom.TotalSeconds)
                    {
                        skipped_l = new_l;
                        continue;
                    }
                    else if (new_l == (decimal)clipFrom.TotalSeconds)
                        start_index = i + 1;
                    else if (new_l > (decimal)clipFrom.TotalSeconds)
                        start_index = i;
                }
                else
                {
                    l += lengths[i];
                    count++;

                    if (l >= (decimal)(clipTo.TotalSeconds - clipFrom.TotalSeconds))
                        break;
                }
            }

            return new ClipDurationRange(start_index, count);
        }
    }
}
