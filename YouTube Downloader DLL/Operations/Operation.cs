﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader_DLL.Operations
{
    public abstract class Operation : IDisposable, INotifyPropertyChanged
    {
        protected const int ProgressMax = 100;
        protected const int ProgressMin = 0;
        /// <summary>
        /// The amount of time to wait for progress updates in milliseconds.
        /// </summary>
        protected const int ProgressDelay = 500;

        /// <summary>
        /// Store running operations that can be stopped automatically when closing application.
        /// </summary>
        public static List<Operation> Running = new List<Operation>();

        #region Events

        /// <summary>
        /// Occurs when the operation is complete.
        /// </summary>
        public event OperationEventHandler Completed;
        public event ProgressChangedEventHandler ProgressChanged;
        public event EventHandler ReportsProgressChanged;
        public event EventHandler Resumed;
        public event EventHandler Started;
        public event StatusChangedEventHandler StatusChanged;

        protected virtual void OnCompleted(OperationEventArgs e)
        {
            this.Completed?.Invoke(this, e);
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            this.ProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnReportsProgressChanged(EventArgs e)
        {
            this.ReportsProgressChanged?.Invoke(this, e);
        }

        protected virtual void OnStarted(EventArgs e)
        {
            this.Started?.Invoke(this, e);
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            this.StatusChanged?.Invoke(this, e);
        }

        #endregion

        #region Fields

        int _progressPercentage = 0;

        bool _prepared = false;
        bool _reportsProgress = false;

        long _duration;
        long _fileSize;
        long _progress;

        string _eta;
        string _link;
        string _progressText = string.Empty;
        string _progressTextOverride = string.Empty;
        string _speed;
        string _text;
        string _thumbnail;
        string _title;

        List<string> _errors = new List<string>();
        OperationStatus _status = OperationStatus.Queued;

        Stopwatch sw;
        BackgroundWorker _worker;

        #endregion

        #region Properties

        public bool CancellationPending
        {
            get
            {
                if (_worker == null)
                    return false;

                return _worker.CancellationPending;
            }
        }

        public bool HasStarted { get; set; }

        public bool IsBusy
        {
            get
            {
                if (_worker == null)
                    return false;

                return _worker.IsBusy;
            }
        }

        public bool IsCanceled
        {
            get
            {
                return this.Status == OperationStatus.Canceled;
            }
        }

        /// <summary>
        /// Returns True if Operation is done, regardless of result.
        /// </summary>
        public bool IsDone
        {
            get
            {
                return this.Status == OperationStatus.Canceled
                    || this.Status == OperationStatus.Failed
                    || this.Status == OperationStatus.Success;
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.Status == OperationStatus.Paused;
            }
        }

        public bool IsSuccessful
        {
            get
            {
                return this.Status == OperationStatus.Success;
            }
        }

        public bool IsQueued
        {
            get
            {
                return this.Status == OperationStatus.Queued;
            }
        }

        public bool IsWorking
        {
            get
            {
                return this.Status == OperationStatus.Working;
            }
        }

        public bool ReportsProgress
        {
            get { return _reportsProgress; }
            set
            {
                _reportsProgress = value;
                this.OnReportsProgressChanged(EventArgs.Empty);
                this.OnPropertyChanged();
            }
        }

        public long Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                this.OnPropertyChanged();
            }
        }

        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit(nameof(ProgressText));
            }
        }

        public long Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit(nameof(ProgressText));
            }
        }

        public string ETA
        {
            get { return _eta; }
            set
            {
                _eta = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the input file or download url.
        /// </summary>
        public string Input { get; set; }

        public string Link
        {
            get { return _link; }
            set
            {
                _link = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the output file.
        /// </summary>
        public string Output { get; set; }

        public string ProgressText
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ProgressTextOverride))
                    return _progressText = this.ProgressTextOverride;

                if (this.Wait() && !string.IsNullOrEmpty(_progressText))
                    return _progressText;

                if (sw != null)
                    sw.Restart();

                switch (this.Status)
                {
                    case OperationStatus.Working:
                        StringBuilder sb = new StringBuilder();

                        sb.AppendFormat("{0}%", ProgressPercentage);

                        if (!(this.Progress == 0 && this.FileSize == 0))
                        {
                            sb.AppendFormat(" - {0}/{1} - {2}{3}",
                                Helper.FormatFileSize(this.Progress),
                                Helper.FormatFileSize(this.FileSize),
                                this.Speed, this.ETA);
                        }

                        _progressText = sb.ToString();
                        break;
                    case OperationStatus.Success:
                        _progressText = "Complete";
                        break;
                    default:
                        _progressText = this.Status.ToString();
                        break;
                }

                return _progressText;
            }
        }

        public string ProgressTextOverride
        {
            get { return _progressTextOverride; }
            set
            {
                _progressTextOverride = value;
                OnPropertyChanged();
                OnPropertyChangedExplicit(nameof(ProgressText));
            }
        }

        public string Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit(nameof(ProgressText));
            }
        }

        public string Thumbnail
        {
            get { return _thumbnail; }
            set
            {
                _thumbnail = value;
                this.OnPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the operation progress, as a double between 0-100.
        /// </summary>
        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit(nameof(ProgressText));
            }
        }

        /// <summary>
        /// Gets a human readable list of errors caused by the operation.
        /// </summary>
        public ReadOnlyCollection<string> Errors
        {
            get { return new ReadOnlyCollection<string>(_errors); }
        }

        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the operation status.
        /// </summary>
        public OperationStatus Status
        {
            get { return _status; }
            set
            {
                var oldStatus = _status;

                _status = value;

                this.OnStatusChanged(new StatusChangedEventArgs(this, value, oldStatus));
                this.OnPropertyChanged();

                // Send Changed notification to following properties
                foreach (string property in new string[] {
                    nameof(IsCanceled),
                    nameof(IsDone),
                    nameof(IsPaused),
                    nameof(IsSuccessful),
                    nameof(IsWorking) })
                {
                    this.OnPropertyChangedExplicit(property);
                }
            }
        }

        /// <summary>
        /// Gets or sets a editable list of errors.
        /// </summary>
        protected List<string> ErrorsInternal
        {
            get { return _errors; }
            set { _errors = value; }
        }

        /// <summary>
        /// Gets or sets the Operation's arguments.
        /// </summary>
        protected Dictionary<string, object> Arguments { get; set; }

        #endregion

        #region IDisposable members

        bool _disposed = false;

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                this.Completed = null;

                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }
            }

            _disposed = true;
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.OnPropertyChangedExplicit(propertyName);
        }

        public void OnPropertyChangedExplicit(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Prepares the operation before starting, giving it all the data it requires.
        /// </summary>
        /// <param name="args"></param>
        public void Prepare(Dictionary<string, object> args)
        {
            this.Arguments = args;

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_Completed;

            _prepared = true;
        }

        /// <summary>
        /// Resumes the operation if supported &amp; available.
        /// </summary>
        public void Resume()
        {
            if (!this.HasStarted)
                this.Start();
            else
                this.ResumeInternal();

            this.Resumed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resumes the operation if supported &amp; available, but does not fire the Resumed event.
        /// </summary>
        public void ResumeQuiet()
        {
            this.ResumeInternal();
        }

        /// <summary>
        /// Starts the operation.
        /// </summary>
        public void Start()
        {
            if (!_prepared)
                throw new Exception("Operation can't be started before being prepared.");

            WorkerStart(this.Arguments);

            sw = new Stopwatch();
            sw.Start();

            _worker.RunWorkerAsync(this.Arguments);

            Operation.Running.Add(this);

            this.Status = OperationStatus.Working;
            this.OnStarted(EventArgs.Empty);

            this.HasStarted = true;
        }

        /// <summary>
        /// Returns whether 'Open' method is supported and available at the moment.
        /// </summary>
        public virtual bool CanOpen()
        {
            return false;
        }

        /// <summary>
        /// Returns whether 'Pause' method is supported and available at the moment.
        /// </summary>
        public virtual bool CanPause()
        {
            return false;
        }

        /// <summary>
        /// Returns whether 'Resume' method is supported and available at the moment.
        /// </summary>
        public virtual bool CanResume()
        {
            return false;
        }

        /// <summary>
        /// Returns whether 'Stop' method is supported and available at the moment.
        /// </summary>
        public virtual bool CanStop()
        {
            return false;
        }

        /// <summary>
        /// Opens the output file.
        /// </summary>
        /// <returns></returns>
        public virtual bool Open()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Opens the containing folder of the output file(s).
        /// </summary>
        public virtual bool OpenContainingFolder()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pauses the operation if supported &amp; available.
        /// </summary>
        public virtual void Pause()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Queues the operation. Used to pause and put the operation back to being queued.
        /// </summary>
        public virtual void Queue()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Resumes the operation if supported &amp; available.
        /// </summary>
        protected virtual void ResumeInternal()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Stops the operation if supported &amp; available.
        /// </summary>
        /// <param name="remove">Remove operation from it's ListView if set to true.</param>
        /// <param name="cleanup">Delete unfinished files if set to true.</param>
        public virtual bool Stop()
        {
            throw new NotSupportedException();
        }

        protected void CancelAsync()
        {
            _worker?.CancelAsync();
        }

        protected void Complete()
        {
            sw.Stop();
            sw = null;

            this.ReportsProgress = true;

            if (this.Status == OperationStatus.Success)
                this.ProgressPercentage = ProgressMax;

            this.OnPropertyChangedExplicit(nameof(ProgressText));

            OnCompleted(new OperationEventArgs(null, this.Status));
        }

        protected void ReportProgress(int percentProgress, object userState)
        {
            _worker?.ReportProgress(percentProgress, userState);
        }

        protected abstract void WorkerCompleted(RunWorkerCompletedEventArgs e);

        protected abstract void WorkerDoWork(DoWorkEventArgs e);

        protected abstract void WorkerProgressChanged(ProgressChangedEventArgs e);

        protected abstract void WorkerStart(Dictionary<string, object> args);

        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            Operation.Running.Remove(this);

            if (e.Error != null)
            {
                this.Exception = e.Error;
                this.Status = OperationStatus.Failed;
            }
            else
            {
                this.Status = (OperationStatus)e.Result;
            }

            this.Complete();
            this.WorkerCompleted(e);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WorkerDoWork(e);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage >= 0 && e.ProgressPercentage <= 100)
            {
                this.ProgressPercentage = e.ProgressPercentage;
            }

            this.WorkerProgressChanged(e);
            this.OnProgressChanged(e);
        }

        private bool Wait()
        {
            if (sw == null || !sw.IsRunning)
                return false;

            // Limit the progress update to once a second to avoid flickering.
            return sw.ElapsedMilliseconds < ProgressDelay;
        }
    }
}
