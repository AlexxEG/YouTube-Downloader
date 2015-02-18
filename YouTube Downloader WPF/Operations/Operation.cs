using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using YouTube_Downloader.Classes;
using YouTube_Downloader_WPF;

namespace YouTube_Downloader.Operations
{
    public abstract class Operation : IDisposable, INotifyPropertyChanged
    {
        protected const double Max_Progress = 100;
        protected const double Min_Progress = 0;
        /// <summary>
        /// The amount of time to wait for progress updates in milliseconds.
        /// </summary>
        protected const int ProgressDelay = 500;

        Stopwatch sw;
        BackgroundWorker _worker;

        public bool CancellationPending
        {
            get
            {
                if (_worker == null)
                    return false;

                return _worker.CancellationPending;
            }
        }

        public bool IsBusy
        {
            get
            {
                if (_worker == null)
                    return false;

                return _worker.IsBusy;
            }
        }

        /// <summary>
        /// Occurs when the operation is complete.
        /// </summary>
        public event OperationEventHandler OperationComplete;

        #region Properties

        long _duration;
        string _eta;
        long _fileSize;
        string _link;
        long _progress;
        double _progressPercentage = 0;
        string _progressText = string.Empty;
        bool _reportsProgress = false;
        string _speed;
        OperationStatus _status = OperationStatus.None;
        string _text;
        string _thumbnail;
        string _title;

        public long Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                this.OnPropertyChanged();
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

        public long FileSize
        {
            get { return _fileSize; }
            set
            {
                _fileSize = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit("ProgressText");
            }
        }

        /// <summary>
        /// Gets the input file or download url.
        /// </summary>
        public string Input { get; set; }

        public bool IsCanceled
        {
            get
            {
                return this.Status == OperationStatus.Canceled;
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

        public bool IsWorking
        {
            get
            {
                return this.Status == OperationStatus.Working;
            }
        }

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

        public long Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit("ProgressText");
            }
        }

        /// <summary>
        /// Gets the operation progress, as a double between 0-100.
        /// </summary>
        public double ProgressPercentage
        {
            get { return _progressPercentage; }
            set
            {
                _progressPercentage = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit("ProgressText");
            }
        }

        public string ProgressText
        {
            get
            {
                if (!string.IsNullOrEmpty(_progressText) && this.Wait())
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

        public bool ReportsProgress
        {
            get { return _reportsProgress; }
            set
            {
                _reportsProgress = value;
                this.OnPropertyChanged();
            }
        }

        public string Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit("ProgressText");
            }
        }

        /// <summary>
        /// Gets the operation status.
        /// </summary>
        public OperationStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnPropertyChanged();
                this.OnPropertyChangedExplicit("IsCanceled");
                this.OnPropertyChangedExplicit("IsPaused");
                this.OnPropertyChangedExplicit("IsSuccessful");
                this.OnPropertyChangedExplicit("IsWorking");
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                this.OnPropertyChanged();
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

        #endregion

        ~Operation()
        {
            // Finalizer calls Dispose(false)
            this.Dispose(false);
        }

        public Operation()
        {
            sw = new Stopwatch();
            sw.Start();

            _worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        bool disposed = false;

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start(object[] args)
        {
            OnWorkerStart(args);

            _worker.RunWorkerAsync(args);

            App.RunningOperations.Add(this);

            this.Status = OperationStatus.Working;
        }

        protected void CancelAsync()
        {
            if (_worker != null)
                _worker.CancelAsync();
        }

        protected void Complete()
        {
            sw.Stop();
            sw = null;

            this.ReportsProgress = true;

            if (this.Status == OperationStatus.Success)
                this.ProgressPercentage = Max_Progress;

            this.OnPropertyChangedExplicit("ProgressText");

            this.Text = string.Empty;

            if (App.RunningOperations.Contains(this))
                App.RunningOperations.Remove(this);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        protected void ReportProgress(int percentProgress, object userState)
        {
            if (_worker != null)
                _worker.ReportProgress(percentProgress, userState);
        }

        protected virtual void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }

        protected abstract void OnWorkerDoWork(DoWorkEventArgs e);

        protected virtual void OnWorkerProgressChanged(ProgressChangedEventArgs e)
        {
            this.ProgressPercentage = e.ProgressPercentage;
        }

        protected virtual void OnWorkerRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            this.Status = (OperationStatus)e.Result;
            this.Complete();
        }

        protected abstract void OnWorkerStart(object[] args);

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                OperationComplete = null;

                if (_worker != null)
                {
                    _worker.Dispose();
                    _worker = null;
                }
            }

            disposed = true;
        }

        private bool Wait()
        {
            // Limit the progress update to once a second to avoid flickering.
            if (sw == null || sw.IsRunning)
                return false;

            return sw.ElapsedMilliseconds < ProgressDelay;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnWorkerDoWork(e);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnWorkerProgressChanged(e);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnWorkerRunWorkerCompleted(e);
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.OnPropertyChangedExplicit(propertyName);
        }

        public void OnPropertyChangedExplicit(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

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
        /// Resumes the operation if supported &amp; available.
        /// </summary>
        public virtual void Resume()
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Stops the operation if supported &amp; available.
        /// </summary>
        /// <param name="remove">Remove operation from it's ListView if set to true.</param>
        /// <param name="cleanup">Delete unfinished files if set to true.</param>
        public virtual bool Stop(bool cleanup)
        {
            throw new NotSupportedException();
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
    }
}
