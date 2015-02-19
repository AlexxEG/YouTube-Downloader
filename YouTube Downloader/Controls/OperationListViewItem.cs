using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public string Duration
        {
            get { return this.SubItems[3].Text; }
            set { this.SubItems[3].Text = value; }
        }

        public string Input
        {
            get { return this.SubItems[5].Text; }
            set { this.SubItems[5].Text = value; }
        }

        public string Progress
        {
            get { return this.SubItems[1].Text; }
            set { this.SubItems[1].Text = value; }
        }

        public string FileSize
        {
            get { return this.SubItems[4].Text; }
            set { this.SubItems[4].Text = value; }
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
                Text = inputText,
                Tag = input
            };
            _inputLabel.LinkClicked += _inputLabel_LinkClicked;

            this.GetListViewEx().AddEmbeddedControl(_progressBar, 1, this.Index);
            this.GetListViewEx().AddEmbeddedControl(_inputLabel, 5, this.Index);

            this.Operation = operation;
            this.Operation.OperationComplete += Operation_OperationComplete;
            this.Operation.ProgressChanged += Operation_ProgressChanged;
            this.Operation.ReportsProgressChanged += Operation_ReportsProgressChanged;
            this.Operation.StatusChanged += Operation_StatusChanged;
            this.Operation.TitleChanged += Operation_TitleChanged;
        }

        private void Operation_OperationComplete(object sender, OperationEventArgs e)
        {
            this.FileSize = Helper.FormatFileSize(Helper.GetFileSize(this.Operation.Output));

            if (_progressBar != null)
                _progressBar.Value = _progressBar.Maximum;

            this.OnOperationComplete(e);
        }

        private void Operation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_progressBar == null)
                return;

            _progressBar.Value = e.ProgressPercentage;
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

        private void Operation_StatusChanged(object sender, EventArgs e)
        {
            switch (this.Operation.Status)
            {
                case OperationStatus.Canceled:
                    this.Progress = "Canceled";
                    break;
                case OperationStatus.Failed:
                    this.Progress = "Failed";
                    break;
                case OperationStatus.Success:
                    this.Progress = "Completed";
                    break;
                case OperationStatus.Paused:
                    this.Progress = "Paused";
                    break;
                case OperationStatus.Working:
                    if (!string.IsNullOrEmpty(this.WorkingText))
                        this.Progress = this.WorkingText;
                    break;
            }
        }

        private void Operation_TitleChanged(object sender, EventArgs e)
        {
            this.Text = this.Operation.Title;
        }

        private void _inputLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start((string)_inputLabel.Tag);
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

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (this.OperationComplete != null)
                this.OperationComplete(this, e);
        }
    }
}