using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using ListViewEmbeddedControls;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Operations;

namespace YouTube_Downloader.Controls
{
    /// <summary>
    /// Wrapper for <see cref="YouTube_Downloader.Operations.Operation"/> to display in ListView.
    /// </summary>
    public class OperationListViewItem : ListViewItem
    {
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

        public string Speed
        {
            get { return this.SubItems[2].Text; }
            set { this.SubItems[2].Text = value; }
        }

        public string WorkingText { get; set; }

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

            this.FileSize = Helper.FormatFileSize(Helper.GetFileSize(this.Operation.Output));

            if (_progressBar != null)
                _progressBar.Value = _progressBar.Maximum;

            this.OnOperationComplete(e);
        }

        private void Operation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_progressBar != null)
                _progressBar.Value = e.ProgressPercentage;

            if (!string.IsNullOrEmpty(this.WorkingText))
                this.Speed = this.WorkingText;
            else
            {
                if (this.Wait())
                    return;

                if (sw != null)
                    sw.Restart();

                this.Speed = this.Operation.Speed + this.Operation.ETA;
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

            this.GetListViewEx().AddEmbeddedControl(_progressBar, 1, this.Index);
            this.GetListViewEx().AddEmbeddedControl(_inputLabel, 5, this.Index);
        }

        private void Operation_StatusChanged(object sender, EventArgs e)
        {
            switch (this.Operation.Status)
            {
                case OperationStatus.Canceled:
                    this.Speed = "Canceled";
                    break;
                case OperationStatus.Failed:
                    this.Speed = "Failed";
                    break;
                case OperationStatus.Success:
                    this.Speed = "Completed";
                    break;
                case OperationStatus.Paused:
                    this.Speed = "Paused";
                    break;
                case OperationStatus.Working:
                    if (!string.IsNullOrEmpty(this.WorkingText))
                        this.Speed = this.WorkingText;
                    break;
            }
        }

        private void _inputLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start((string)_inputLabel.Text);
            }
            catch
            {
                MessageBox.Show(this.ListView.FindForm(), "Couldn't open link.");
            }
        }

        private ListViewEx GetListViewEx()
        {
            if (this.ListView == null)
                return null;

            return this.ListView as ListViewEx;
        }

        private bool Wait()
        {
            // Limit the progress update to once a second to avoid flickering.
            if (sw == null || !sw.IsRunning)
                return false;

            return sw.ElapsedMilliseconds < 1000;
        }
    }
}