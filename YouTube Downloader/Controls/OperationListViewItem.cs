using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Operations;

namespace YouTube_Downloader.Controls
{
    /// <summary>
    /// Wrapper for <see cref="YouTube_Downloader_DLL.Operations.Operation"/> to display in ListView.
    /// </summary>
    public class OperationListViewItem
    {
        private const int ProgressMaximum = 100;
        private const int ProgressMinimum = 0;
        private const int ProgressUpdateDelay = 250;

        int _progress;
        string _duration;
        string _filesize;
        string _input;
        string _status;
        string _title;
        string _workingText;

        Stopwatch sw;

        public int Progress
        {
            get { return _progress; }
            set
            {
                if (_progress == value)
                    return;

                _progress = value;
                this.OnAspectChanged();
            }
        }

        public string Duration
        {
            get { return _duration; }
            set
            {
                if (_duration == value)
                    return;

                _duration = value;
                this.OnAspectChanged();
            }
        }
        public string FileSize
        {
            get { return _filesize; }
            set
            {
                if (_filesize == value)
                    return;

                _filesize = value;
                this.OnAspectChanged();
            }
        }
        public string Input
        {
            get { return _input; }
            set
            {
                if (_input == value)
                    return;

                _input = value;
                this.OnAspectChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status == value)
                    return;

                _status = value;
                this.OnAspectChanged();
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;

                _title = value;
                this.OnAspectChanged();
            }
        }
        public string WorkingText
        {
            get { return _workingText; }
            set
            {
                if (_workingText == value)
                    return;

                _workingText = value;
                this.OnAspectChanged();
            }
        }

        public Operation Operation { get; private set; }

        public event EventHandler AspectChanged;
        public event OperationEventHandler OperationComplete;

        public OperationListViewItem(string text, string input, Operation operation)
            : this(text, input, input, operation)
        {
        }

        public OperationListViewItem(string text, string input, string inputText, Operation operation)
        {
            this.Title = text;
            this.Input = inputText;

            this.Operation = operation;
            this.Operation.Completed += Operation_Completed;
            this.Operation.ProgressChanged += Operation_ProgressChanged;
            this.Operation.PropertyChanged += Operation_PropertyChanged;
            this.Operation.ReportsProgressChanged += Operation_ReportsProgressChanged;
            this.Operation.Started += Operation_Started;
            this.Operation.StatusChanged += Operation_StatusChanged;

            // Set Status text, so it's not empty until a StatusChanged event is fired
            this.Operation_StatusChanged(this, EventArgs.Empty);
        }

        private void OnAspectChanged()
        {
            this.AspectChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            this.OperationComplete?.Invoke(this, e);
        }

        private void Operation_Completed(object sender, OperationEventArgs e)
        {
            sw.Stop();
            sw = null;

            if (File.Exists(this.Operation.Output))
            {
                this.FileSize = Helper.GetFileSizeFormatted(this.Operation.Output);
            }
            else if (Directory.Exists(this.Operation.Output))
            {
                /* Get total file size of all affected files
                 *
                 * Directory can contain unrelated files, so use make use of List properties
                 * from Operation that contains the affected files only.
                 */
                string[] fileList = null;

                if (this.Operation is ConvertOperation)
                    fileList = (this.Operation as ConvertOperation).ProcessedFiles.ToArray();
                else if (this.Operation is PlaylistOperation)
                    fileList = (this.Operation as PlaylistOperation).DownloadedFiles.ToArray();
                else
                    throw new Exception("Couldn't get affected file list from operation " + this.Operation.GetType().Name);

                long fileSize = 0;

                foreach (string file in fileList)
                    fileSize += Helper.GetFileSize(file);

                this.FileSize = Helper.FormatFileSize(fileSize);
            }

            this.Progress = ProgressMaximum;
            this.OnOperationComplete(e);
        }

        private void Operation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Progress = Math.Min(ProgressMaximum,
                                     Math.Max(ProgressMinimum, e.ProgressPercentage));

            if (!string.IsNullOrEmpty(this.WorkingText))
                this.Status = this.WorkingText;
            else
            {
                if (this.Wait())
                    return;

                if (sw != null)
                    sw.Restart();

                this.Status = this.Operation.Speed + this.Operation.ETA;
            }
        }

        private void Operation_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Operation.Duration):
                    this.Duration = Helper.FormatVideoLength(this.Operation.Duration);
                    break;
                case nameof(Operation.FileSize):
                    this.FileSize = Helper.FormatFileSize(this.Operation.FileSize);
                    break;
                case nameof(Operation.Input):
                    this.Input = this.Operation.Input;
                    break;
                case nameof(Operation.Title):
                    this.Title = this.Operation.Title;
                    break;
            }
        }

        private void Operation_ReportsProgressChanged(object sender, EventArgs e)
        {
            if (this.Operation.ReportsProgress)
            {
                // ToDo: Show normal progress bar
            }
            else
            {
                // ToDo: Show looping progres bar
            }
        }

        private void Operation_Started(object sender, EventArgs e)
        {
            sw = new Stopwatch();
            sw.Start();
        }

        private void Operation_StatusChanged(object sender, EventArgs e)
        {
            switch (this.Operation.Status)
            {
                case OperationStatus.Success:
                    this.Status = "Completed";
                    break;
                case OperationStatus.Canceled:
                case OperationStatus.Failed:
                case OperationStatus.Paused:
                case OperationStatus.Queued:
                    this.Status = this.Operation.Status.ToString();
                    break;
                case OperationStatus.Working:
                    if (!string.IsNullOrEmpty(this.WorkingText))
                        this.Status = this.WorkingText;
                    break;
            }
        }

        private bool Wait()
        {
            // Limit the progress update to avoid flickering.
            if (sw == null || !sw.IsRunning)
                return false;

            return sw.ElapsedMilliseconds < ProgressUpdateDelay;
        }
    }
}