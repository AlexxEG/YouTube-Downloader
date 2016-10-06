using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ListViewEmbeddedControls;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Operations;

namespace YouTube_Downloader.Controls
{
    /// <summary>
    /// Wrapper for <see cref="YouTube_Downloader.Operations.Operation"/> to display in ListView.
    /// </summary>
    public class OperationListViewItem : ListViewItem
    {
        private const int ColumnProgressBar = 1;
        private const int ColumnInputLabel = 5;
        private const int ProgressUpdateDelay = 250;

        LinkLabel _inputLabel;
        ProgressBar _progressBar;
        Stopwatch sw;

        public string Duration
        {
            get { return this.SubItems[3].Text; }
            set { this.SubItems[3].Text = value; }
        }

        public string FileSize
        {
            get { return this.SubItems[4].Text; }
            set { this.SubItems[4].Text = value; }
        }

        public string Input
        {
            get { return this.SubItems[5].Text; }
            set
            {
                _inputLabel.Text = this.Input;

                this.SubItems[5].Text = value;
            }
        }

        public string Progress
        {
            get { return this.SubItems[1].Text; }
            set { this.SubItems[1].Text = value; }
        }

        public string Status
        {
            get { return this.SubItems[2].Text; }
            set { this.SubItems[2].Text = value; }
        }

        public string WorkingText { get; set; }

        public ListViewEx ListViewEx
        {
            get
            {
                if (this.ListView == null)
                    return null;

                return this.ListView as ListViewEx;
            }
        }
        public Operation Operation { get; private set; }

        public event OperationEventHandler OperationComplete;

        public OperationListViewItem(string text, string input, Operation operation)
            : this(text, input, input, operation)
        {
        }

        public OperationListViewItem(string text, string input, string inputText, Operation operation)
            : base()
        {
            this.Text = text;
            // Fill SubItems
            this.SubItems.AddRange(new string[] { "", "", "", "", "" });

            _progressBar = new ProgressBar()
            {
                Maximum = 100,
                Minimum = 0,
                Value = 0
            };
            _inputLabel = new LinkLabel()
            {
                Text = inputText
            };
            _inputLabel.LinkClicked += _inputLabel_LinkClicked;

            this.Operation = operation;
            this.Operation.Completed += Operation_Completed;
            this.Operation.ProgressChanged += Operation_ProgressChanged;
            this.Operation.PropertyChanged += Operation_PropertyChanged;
            this.Operation.ReportsProgressChanged += Operation_ReportsProgressChanged;
            this.Operation.Started += Operation_Started;
            this.Operation.StatusChanged += Operation_StatusChanged;
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (this.OperationComplete != null)
                this.OperationComplete(this, e);
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
                 * from Operation that contains the affected files only. */
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

            if (_progressBar != null)
                _progressBar.Value = _progressBar.Maximum;

            this.OnOperationComplete(e);
        }

        private void Operation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_progressBar != null)
                _progressBar.Value = Math.Min(_progressBar.Maximum, Math.Max(_progressBar.Minimum, e.ProgressPercentage));

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
                case "Duration":
                    this.Duration = Helper.FormatVideoLength(this.Operation.Duration);
                    break;
                case "FileSize":
                    this.FileSize = Helper.FormatFileSize(this.Operation.FileSize);
                    break;
                case "Input":
                    this.Input = this.Operation.Input;
                    break;
                case "Title":
                    this.Text = this.Operation.Title;
                    break;
            }
        }

        private void Operation_ReportsProgressChanged(object sender, EventArgs e)
        {
            if (_progressBar == null)
                return;

            if (this.Operation.ReportsProgress)
            {
                _progressBar.Style = ProgressBarStyle.Continuous;
                _progressBar.MarqueeAnimationSpeed = 0;
            }
            else
            {
                _progressBar.Style = ProgressBarStyle.Marquee;
                _progressBar.MarqueeAnimationSpeed = 30;
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
                case OperationStatus.Canceled:
                    this.Status = "Canceled";
                    break;
                case OperationStatus.Failed:
                    this.Status = "Failed";
                    break;
                case OperationStatus.Success:
                    this.Status = "Completed";
                    break;
                case OperationStatus.Paused:
                    this.Status = "Paused";
                    break;
                case OperationStatus.Working:
                    if (!string.IsNullOrEmpty(this.WorkingText))
                        this.Status = this.WorkingText;
                    break;
            }
        }

        private void _inputLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(_inputLabel.Text);
            }
            catch
            {
                MessageBox.Show(this.ListView.FindForm(), "Couldn't open link.");
            }
        }

        public void SetupEmbeddedControls()
        {
            this.ListViewEx.AddEmbeddedControl(_progressBar, ColumnProgressBar, this.Index);
            this.ListViewEx.AddEmbeddedControl(_inputLabel, ColumnInputLabel, this.Index);
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