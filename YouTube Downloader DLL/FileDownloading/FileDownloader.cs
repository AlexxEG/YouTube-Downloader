// Heavly inspired by:
// http://www.codeproject.com/Articles/35954/C-NET-Background-File-Downloader
//
// Made to fit my own needs and code style.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using IO = System.IO;

namespace YouTube_Downloader_DLL.FileDownloading
{
    public class FileDownloader : IDisposable
    {
        BackgroundWorker _downloader;

        public event EventHandler CalculatedTotalFileSize;
        public event EventHandler Canceled;
        public event EventHandler Completed;
        public event EventHandler Paused;
        public event EventHandler ProgressChanged;
        public event EventHandler Resumed;
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event FileDownloadEventHandler FileDownloadComplete;
        public event FileDownloadEventHandler FileDownloadSucceeded;
        public event FileDownloadFailedEventHandler FileDownloadFailed;

        public int PackageSize { get; set; } = 4096;
        public int Speed { get; set; }

        public bool CanPause
        {
            get { return this.IsBusy && !this.IsPaused && !_downloader.CancellationPending; }
        }
        public bool CanResume
        {
            get { return this.IsBusy && this.IsPaused && !_downloader.CancellationPending; }
        }
        public bool CanStart
        {
            get { return !this.IsBusy; }
        }
        public bool CanStop
        {
            get { return this.IsBusy && !_downloader.CancellationPending; }
        }
        public bool DeleteUnfinishedFilesOnCancel { get; set; }
        public bool IsBusy { get; private set; }
        public bool IsPaused { get; private set; }
        public bool WasCanceled { get; set; }

        public long TotalProgress { get; set; }
        public long TotalSize { get; private set; }

        public FileDownload CurrentFile { get; private set; }

        public List<FileDownload> Files { get; set; }

        public delegate void FileDownloadEventHandler(object sender, FileDownloadEventArgs e);
        public delegate void FileDownloadFailedEventHandler(object sender, FileDownloadFailedEventArgs e);

        private enum BackgroundEvents
        {
            CalculatedTotalFileSize,
            FileDownloadComplete,
            FileDownloadSucceeded,
            ProgressChanged
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects)
                    _downloader.Dispose();
                }
                // Free your own state (unmanaged objects)
                // Set large fields to null
                this.Files = null;
            }
        }

        #endregion

        public FileDownloader()
        {
            _downloader = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _downloader.DoWork += downloader_DoWork;
            _downloader.ProgressChanged += downloader_ProgressChanged;
            _downloader.RunWorkerCompleted += downloader_RunWorkerCompleted;

            this.DeleteUnfinishedFilesOnCancel = true;
            this.Files = new List<FileDownload>();
        }

        public void Pause()
        {
            if (!this.IsBusy || this.IsPaused)
                return;

            this.IsPaused = true;
            this.OnPaused();
        }

        public void Resume()
        {
            if (!this.IsBusy || !this.IsPaused)
                return;

            this.IsPaused = false;
            this.OnResumed();
        }

        public void Start()
        {
            this.IsBusy = true;
            this.WasCanceled = false;
            this.TotalProgress = 0;

            _downloader.RunWorkerAsync();

            this.OnStarted();
        }

        public void Stop()
        {
            this.IsBusy = false;
            this.IsPaused = false;
            this.WasCanceled = true;

            _downloader.CancelAsync();

            this.OnCanceled();
        }

        public double TotalPercentage()
        {
            return Math.Round((double)this.TotalProgress / this.TotalSize * 100, 2);
        }

        private void CalculateTotalFileSize()
        {
            this.TotalSize = 0;

            foreach (var file in this.Files)
            {
                try
                {
                    WebRequest webReq = (WebRequest)WebRequest.Create(file.Url);
                    WebResponse webResp = (WebResponse)webReq.GetResponse();

                    this.TotalSize += webResp.ContentLength;

                    webResp.Close();
                }
                catch (Exception) { }
            }

            _downloader.ReportProgress(-1, BackgroundEvents.CalculatedTotalFileSize);
        }

        private void CleanupFiles()
        {
            new Thread(delegate ()
            {
                var dict = new Dictionary<string, int>();
                var keys = new List<string>();

                foreach (var file in this.Files)
                {
                    if (file.AlwaysCleanupOnCancel || !file.IsFinished)
                    {
                        dict.Add(file.Path, 0);
                        keys.Add(file.Path);
                    }
                }

                while (dict.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            if (File.Exists(key))
                                File.Delete(key);

                            // Remove file from dictionary since it either got deleted
                            // or it doesn't exist anymore.
                            dict.Remove(key);
                        }
                        catch
                        {
                            if (dict[key] == 10)
                            {
                                dict.Remove(key);
                            }
                            else
                            {
                                dict[key]++;
                            }
                        }
                    }

                    Thread.Sleep(2000);
                }
            }).Start();
        }

        private void DownloadFile()
        {
            long totalSize = 0;

            byte[] readBytes = new byte[this.PackageSize];
            int currentPackageSize;
            Stopwatch speedTimer = new Stopwatch();
            Exception exception = null;

            long existLen = 0;

            if (File.Exists(this.CurrentFile.Path))
            {
                existLen = new FileInfo(this.CurrentFile.Path).Length;
            }

            FileStream writer;

            if (existLen > 0)
                writer = new FileStream(this.CurrentFile.Path, FileMode.Append, FileAccess.Write);
            else
                writer = new FileStream(this.CurrentFile.Path, FileMode.Create, FileAccess.Write);

            HttpWebRequest webReq;
            HttpWebResponse webResp = null;

            try
            {
                webReq = (HttpWebRequest)WebRequest.Create(this.CurrentFile.Url);
                webReq.AddRange(existLen);

                webResp = (HttpWebResponse)webReq.GetResponse();

                totalSize = existLen + webResp.ContentLength;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            this.CurrentFile.TotalFileSize = totalSize;

            if (exception != null)
            {
                _downloader.ReportProgress(0, exception);
            }
            else
            {
                this.CurrentFile.Progress = existLen;
                this.TotalProgress += existLen;

                long prevSize = 0;
                int speedInterval = 100;

                while (this.CurrentFile.Progress < totalSize && !_downloader.CancellationPending)
                {
                    while (this.IsPaused)
                        Thread.Sleep(100);

                    speedTimer.Start();

                    currentPackageSize = webResp.GetResponseStream().Read(readBytes, 0, this.PackageSize);

                    this.CurrentFile.Progress += currentPackageSize;
                    this.TotalProgress += currentPackageSize;

                    // Raise ProgressChanged event
                    this.RaiseEventFromBackground(BackgroundEvents.ProgressChanged, EventArgs.Empty);

                    writer.Write(readBytes, 0, currentPackageSize);

                    if (speedTimer.Elapsed.TotalMilliseconds >= speedInterval)
                    {
                        long downloadedBytes = writer.Length - prevSize;
                        prevSize = writer.Length;

                        this.Speed = (int)downloadedBytes * (speedInterval == 100 ? 10 : 1);

                        // Only update speed once a second after initial update
                        speedInterval = 1000;

                        speedTimer.Reset();
                    }
                }

                speedTimer.Stop();
                writer.Close();
                webResp.Close();

                if (!_downloader.CancellationPending)
                {
                    this.CurrentFile.IsFinished = true;

                    this.RaiseEventFromBackground(BackgroundEvents.FileDownloadSucceeded,
                        new FileDownloadEventArgs(this.CurrentFile));
                }
            }
        }

        private void RaiseEvent(BackgroundEvents evt, EventArgs e)
        {
            switch (evt)
            {
                case BackgroundEvents.CalculatedTotalFileSize:
                    this.OnCalculatedTotalFileSize();
                    break;
                case BackgroundEvents.FileDownloadComplete:
                    this.OnFileDownloadComplete((FileDownloadEventArgs)e);
                    break;
                case BackgroundEvents.FileDownloadSucceeded:
                    this.OnFileDownloadSucceeded((FileDownloadEventArgs)e);
                    break;
                case BackgroundEvents.ProgressChanged:
                    this.OnProgressChanged();
                    break;
            }
        }

        private void RaiseEventFromBackground(BackgroundEvents evt, EventArgs e)
        {
            _downloader.ReportProgress(-1, new object[] { evt, e });
        }

        private void downloader_DoWork(object sender, DoWorkEventArgs e)
        {
            this.CalculateTotalFileSize();

            foreach (var file in this.Files)
            {
                this.CurrentFile = file;

                if (!Directory.Exists(file.Directory))
                    Directory.CreateDirectory(file.Directory);

                this.DownloadFile();

                this.RaiseEventFromBackground(BackgroundEvents.FileDownloadComplete,
                        new FileDownloadEventArgs(file));

                if (_downloader.CancellationPending)
                {
                    this.CleanupFiles();
                }
            }
        }

        private void downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is Exception)
            {
                this.OnFileDownloadFailed(e.UserState as Exception, this.CurrentFile);
            }
            else if (e.UserState is object[])
            {
                object[] obj = (object[])e.UserState;

                if (obj[0] is BackgroundEvents)
                {
                    this.RaiseEvent((BackgroundEvents)obj[0], (EventArgs)obj[1]);
                }
            }
        }

        private void downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsBusy = this.IsPaused = false;

            if (!this.WasCanceled)
                this.OnCompleted();
            else
                this.OnCanceled();

            this.OnStopped();
        }

        protected void OnCalculatedTotalFileSize()
        {
            if (this.CalculatedTotalFileSize != null)
                this.CalculatedTotalFileSize(this, EventArgs.Empty);
        }

        protected void OnCanceled()
        {
            if (this.Canceled != null)
                this.Canceled(this, EventArgs.Empty);
        }

        protected void OnCompleted()
        {
            if (this.Completed != null)
                this.Completed(this, EventArgs.Empty);
        }

        protected void OnFileDownloadComplete(FileDownloadEventArgs e)
        {
            if (FileDownloadComplete != null)
                FileDownloadComplete(this, e);
        }

        protected void OnFileDownloadFailed(Exception exception, FileDownload fileDownload)
        {
            if (this.FileDownloadFailed != null)
                this.FileDownloadFailed(this, new FileDownloadFailedEventArgs(exception, fileDownload));
        }

        protected void OnFileDownloadSucceeded(FileDownloadEventArgs e)
        {
            if (FileDownloadSucceeded != null)
                FileDownloadSucceeded(this, e);
        }

        protected void OnPaused()
        {
            if (Paused != null)
                Paused(this, EventArgs.Empty);
        }

        protected void OnProgressChanged()
        {
            if (ProgressChanged != null)
                ProgressChanged(this, EventArgs.Empty);
        }

        protected void OnResumed()
        {
            if (Resumed != null)
                Resumed(this, EventArgs.Empty);
        }

        protected void OnStarted()
        {
            if (this.Started != null)
                this.Started(this, EventArgs.Empty);
        }

        protected void OnStopped()
        {
            if (this.Stopped != null)
                this.Stopped(this, EventArgs.Empty);
        }
    }
}
