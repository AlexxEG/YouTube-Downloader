﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using LeaxDev.WindowStates;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Controls;
using YouTube_Downloader.Dialogs;
using YouTube_Downloader.Properties;
using YouTube_Downloader.Renderers;
using YouTube_Downloader_DLL;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Dialogs;
using YouTube_Downloader_DLL.DummyOperations;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.FFmpegHelpers;
using YouTube_Downloader_DLL.Helpers;
using YouTube_Downloader_DLL.Operations;
using YouTube_Downloader_DLL.Updating;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader
{
    public partial class MainForm : Form
    {
        private VideoInfo _selectedVideo;
        private YTDAuthentication _auth = null;
        Thread _maxSimDownloadsApplyThread;

        private delegate void UpdateFileSize(object sender, FileSizeUpdateEventArgs e);

        public MainForm()
        {
            InitializeComponent();
            InitializeMainMenu();

            this.SetupQueue();

            olvQueue.ContextMenu = contextMenu1;
            lvPlaylistVideos.ContextMenu = cmPlaylistList;

            mtxtTo.ValidatingType = typeof(TimeSpan);
            mtxtFrom.ValidatingType = typeof(TimeSpan);

            // Remove file size label text, should be empty when first starting.
            lFileSize.Text = string.Empty;

            // Fix progress sorting
            olvQueue.CustomSorter = delegate (OLVColumn column, SortOrder order)
            {
                if (column == olvColumn2)
                {
                    olvQueue.ListViewItemSorter = new BarTextProgressComparer(order);
                }
            };

            rbConvertFile.BackColor = Color.FromArgb(249, 249, 249);
            rbConvertFolder.BackColor = Color.FromArgb(249, 249, 249);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.IsWorking())
            {
                string text = "Files are being downloaded/converted/cut.\n\nAre you sure you want to quit?";

                if (MessageBox.Show(this, text, "Confirmation",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Hide form while waiting for threads to finish,
                    // except downloads which will abort.
                    this.CancelOperations();
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }

            _selectedVideo?.AbortUpdateFileSizes();

            this.SaveSettings();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
            this.ShowUpdateNotification();



#if DEBUG
            this.ReadDebugFile();
#endif
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (Program.Arguments.Count > 0)
            {
                txtYoutubeLink.Text = Program.Arguments[0];
                btnGetVideo.PerformClick();
            }

            // Disable & enable functions depending on if FFmpeg is available, and
            // display error for the user.
            groupBox2.Enabled = chbAutoConvert.Enabled = Program.FFmpegAvailable;
            lFFmpegMissing.Visible = btnCheckAgain.Visible = !Program.FFmpegAvailable;
        }

        private void olvQueue_HyperlinkClicked(object sender, HyperlinkClickedEventArgs e)
        {
            e.Handled = true;

            try
            {
                var model = e.Model as OperationModel;

                if (string.IsNullOrEmpty(model.Input))
                    return;

                Process.Start(model.Input);
            }
            catch (Exception)
            {
                MessageBox.Show(this, "An error occured attemping to open input.");
            }
        }

        private void nudMaxSimDownloads_ValueChanged(object sender, EventArgs e)
        {
            _maxSimDownloadsApplyThread?.Abort();
            _maxSimDownloadsApplyThread = new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(1000);

                Settings.Default.MaxSimDownloads = (int)nudMaxSimDownloads.Value;
                DownloadQueueHandler.MaxDownloads = (int)nudMaxSimDownloads.Value;
            }));
            _maxSimDownloadsApplyThread.Start();
        }

        private void chbMaxSimDownloads_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ShowMaxSimDownloads = chbMaxSimDownloads.Checked;
            nudMaxSimDownloads.Enabled = Settings.Default.ShowMaxSimDownloads;
            DownloadQueueHandler.LimitDownloads = Settings.Default.ShowMaxSimDownloads;
        }

        private void OperationModel_AspectChanged(object sender, EventArgs e)
        {
            olvQueue.RefreshObject(sender);
            olvQueue.Sort();
        }

        #region Download Tab

        private void btnGetVideo_Click(object sender, EventArgs e)
        {
            if (!Helper.IsValidUrl(txtYoutubeLink.Text))
            {
                MessageBox.Show(this, "Input link is not a valid Twitch/YouTube link.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                txtYoutubeLink.Text = Helper.FixUrl(txtYoutubeLink.Text);

                Settings.Default.LastYouTubeUrl = txtYoutubeLink.Text;

                txtTitle.Text = string.Empty;
                cbQuality.Items.Clear();
                btnGetVideo.Enabled = txtYoutubeLink.Enabled = btnDownload.Enabled = cbQuality.Enabled = false;
                videoThumbnail.Tag = null;

                bwGetVideo.RunWorkerAsync(txtYoutubeLink.Text);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFolderDialog())
            {
                if (Directory.Exists(cbSaveTo.Text))
                    ofd.InitialFolder = cbSaveTo.Text;
                else
                    ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    cbSaveTo.SelectedIndex = cbSaveTo.Items.Add(ofd.Folder);
                    cbPlaylistSaveTo.Items.Add(ofd.Folder);
                }
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            // Validate the filename, checking for illegal characters. 
            // Prompts the user to remove these characters automatically.
            if (!this.ValidateFilename(txtTitle.Text))
                return;

            string path = cbSaveTo.Text;

            // Make sure download directory exists, 
            // prompting the user to create it if it doesn't.
            if (!this.ValidateDirectory(path))
                return;

            if (!cbSaveTo.Items.Contains(path))
                cbSaveTo.Items.Add(path);

            try
            {
                var format = cbQuality.SelectedItem as VideoFormat;
                var filename = string.Format("{0}.{1}", txtTitle.Text, format.Extension);
                var output = Path.Combine(path, filename);

                if (File.Exists(output))
                {
                    if (MessageBox.Show(this,
                            string.Format("File '{1}' already exists.{0}{0}Overwrite?", Environment.NewLine, filename),
                            "Overwrite?",
                            MessageBoxButtons.YesNo) == DialogResult.No)
                        return;

                    File.Delete(output);
                }

                Operation operation;

                switch (_selectedVideo.VideoSource)
                {
                    case VideoSource.Twitch:
                        if (!chbDownloadClipFrom.Checked)
                            operation = new TwitchOperation(format, output);
                        else
                        {
                            operation = new TwitchOperation(format, output,
                                dpDownloadClipFrom.Duration,
                                dpDownloadClipTo.Duration);
                        }
                        break;
                    case VideoSource.YouTube:
                        if (format.AudioOnly || format.HasAudioAndVideo)
                            operation = new DownloadOperation(format, output);
                        else
                        {
                            operation = new DownloadOperation(format,
                                Helper.GetAudioFormat(format),
                                output);
                        }
                        break;
                    default:
                        throw new Exception($"Unknown video source: {_selectedVideo.VideoSource}");
                }

                this.AddQueueItem(operation, true);
                DownloadQueueHandler.Add(operation);
            }
            catch (Exception ex)
            {
                Common.SaveException(ex);

                MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void videoThumbnail_Paint(object sender, PaintEventArgs e)
        {
            if (videoThumbnail.Tag != null)
            {
                var length = (string)videoThumbnail.Tag;
                var mFont = new Font(this.Font.Name, 10.0F, FontStyle.Bold, GraphicsUnit.Point);
                var mSize = e.Graphics.MeasureString(length, mFont);
                var mRec = new Rectangle((int)(videoThumbnail.Width - mSize.Width - 6),
                                         (int)(videoThumbnail.Height - mSize.Height - 6),
                                         (int)(mSize.Width + 2),
                                         (int)(mSize.Height + 2));

                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(200, Color.Black)), mRec);
                e.Graphics.DrawString(length, mFont, new SolidBrush(Color.Gainsboro), new PointF((videoThumbnail.Width - mSize.Width - 5),
                    (videoThumbnail.Height - mSize.Height - 5)));
            }
        }

        private void cbQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Display file size.
            var format = (VideoFormat)cbQuality.SelectedItem;

            if (format == null || _selectedVideo.VideoSource == VideoSource.Twitch)
            {
                lFileSize.Text = string.Empty;
            }
            else if (format.FileSize == 0)
            {
                lFileSize.Text = "Getting file size...";
            }
            else
            {
                long total = format.FileSize;

                // If the format is VideoOnly, combine audio and video size.
                if (format.VideoOnly)
                    total += Helper.GetAudioFormat(format).FileSize;

                lFileSize.Text = Helper.FormatFileSize(total);
            }
        }

        private void chbDownloadClipFrom_CheckedChanged(object sender, EventArgs e)
        {
            dpDownloadClipFrom.Enabled = chbDownloadClipTo.Enabled = chbDownloadClipFrom.Checked;
            dpDownloadClipTo.Enabled = chbDownloadClipFrom.Checked && chbDownloadClipTo.Checked;
        }

        private void chbDownloadClipTo_CheckedChanged(object sender, EventArgs e)
        {
            dpDownloadClipTo.Enabled = chbDownloadClipTo.Checked;
        }

        private void videoInfo_FileSizeUpdated(object sender, FileSizeUpdateEventArgs e)
        {
            if (this._selectedVideo.VideoSource == VideoSource.Twitch)
                return;

            if (lFileSize.InvokeRequired)
            {
                lFileSize.Invoke(new UpdateFileSize(videoInfo_FileSizeUpdated), sender, e);
            }
            else
            {
                // Display the updated file size if the selected item was updated.
                if (e.VideoFormat == cbQuality.SelectedItem)
                {
                    lFileSize.Text = Helper.FormatFileSize(e.VideoFormat.FileSize);
                }
            }
        }

        private void bwGetVideo_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = YTD.GetVideoInfo((string)e.Argument, _auth);
        }

        private void bwGetVideo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var videoInfo = e.Result as VideoInfo;

            if (videoInfo.RequiresAuthentication)
            {
                var auth = Dialogs.LoginDialog.Show(this);

                if (auth != null)
                {
                    _auth = auth;
                    btnGetVideo_Click(bwGetVideo, EventArgs.Empty);
                    return;
                }
            }
            else if (videoInfo.Failure)
            {
                MessageBox.Show(this, "Couldn't retrieve video. Reason:\n\n" + videoInfo.FailureReason);
            }
            else
            {
                _selectedVideo = videoInfo;

                videoInfo.FileSizeUpdated += videoInfo_FileSizeUpdated;

                cbQuality.Items.AddRange(this.CheckFormats(videoInfo.Formats));
                cbQuality.SelectedIndex = cbQuality.Items.Count - 1;

                txtTitle.Text = Helper.FormatTitle(videoInfo.Title);

                var videoLength = TimeSpan.FromSeconds(videoInfo.Duration);
                if (videoLength.Hours > 0)
                    videoThumbnail.Tag = string.Format("{0}:{1:00}:{2:00}", videoLength.Hours, videoLength.Minutes, videoLength.Seconds);
                else
                    videoThumbnail.Tag = string.Format("{0}:{1:00}", videoLength.Minutes, videoLength.Seconds);

                videoThumbnail.Refresh();
                videoThumbnail.ImageLocation = videoInfo.ThumbnailUrl;

                // Show clipping tool for twitch
                flpDownloadClip.Visible = videoInfo.VideoSource == VideoSource.Twitch;
            }

            btnGetVideo.Enabled = txtYoutubeLink.Enabled = true;
            cbQuality.Enabled = videoInfo.Formats.Count > 0;
            btnDownload.Enabled = true;
        }

        private void OperationModel_OperationComplete(object sender, OperationEventArgs e)
        {
            if (!((sender as OperationModel).Operation is DownloadOperation))
                return;

            var operation = (sender as OperationModel).Operation;

            if (chbAutoConvert.Enabled && chbAutoConvert.Checked && operation.Status == OperationStatus.Success)
            {
                string output = Path.Combine(Path.GetDirectoryName(operation.Output),
                    Path.GetFileNameWithoutExtension(operation.Output)) + ".mp3";

                this.Convert(operation.Output, output, false);
            }
        }

        #endregion

        #region Playlist Tab

        private bool _playlistCancel, _playlistReversed;
        private BackgroundWorker _backgroundWorkerPlaylist;
        private OrderedDictionary _playlistIgnored = new OrderedDictionary();
        private QuickPlaylist _playlist;

        private void btnPlaylistBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFolderDialog())
            {
                if (Directory.Exists(cbPlaylistSaveTo.Text))
                    ofd.InitialFolder = cbPlaylistSaveTo.Text;
                else
                    ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    cbSaveTo.Items.Add(ofd.Folder);
                    cbPlaylistSaveTo.SelectedIndex = cbPlaylistSaveTo.Items.Add(ofd.Folder);
                }
            }
        }

        private void btnGetPlaylist_Click(object sender, EventArgs e)
        {
            if (_playlistCancel)
            {
                _backgroundWorkerPlaylist.CancelAsync();
            }
            else
            {
                // Reset playlist variables
                _playlistIgnored.Clear();
                lvPlaylistVideos.Items.Clear();

                _playlistCancel = true;
                btnGetPlaylist.Text = "Cancel";
                btnPlaylistDownloadAll.Enabled = false;
                btnPlaylistDownloadSelected.Enabled = false;

                _backgroundWorkerPlaylist = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                _backgroundWorkerPlaylist.DoWork += _backgroundWorkerPlaylist_DoWork;
                _backgroundWorkerPlaylist.ProgressChanged += _backgroundWorkerPlaylist_ProgressChanged;
                _backgroundWorkerPlaylist.RunWorkerCompleted += _backgroundWorkerPlaylist_RunWorkerCompleted;
                _backgroundWorkerPlaylist.RunWorkerAsync(txtPlaylistLink.Text);

                lvPlaylistVideos.UseWaitCursor = true;

                // Save playlist url
                Settings.Default.LastPlaylistUrl = txtPlaylistLink.Text;
            }
        }

        private void _backgroundWorkerPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            var playlistUrl = e.Argument as string;
            var playlist = new QuickPlaylist(playlistUrl).Load();
            var videos = playlist.Videos.Cast<QuickVideoInfo>();

            _backgroundWorkerPlaylist.ReportProgress(-1, playlist);

            if (playlistReverseMenuItem.Checked)
                videos = videos.Reverse();

            int count = 0;

            foreach (var video in videos)
            {
                if (_backgroundWorkerPlaylist.CancellationPending)
                {
                    e.Result = false;
                    break;
                }

                var title = WebUtility.HtmlDecode(video.Title);
                var item = new ListViewItem((++count).ToString());
                item.SubItems.Add(Helper.FormatTitle(title));
                item.SubItems.Add(video.Duration);
                item.Checked = true;
                item.Tag = video;

                _backgroundWorkerPlaylist.ReportProgress(-1, item);
            }

            e.Result = true;
        }

        private void _backgroundWorkerPlaylist_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is ListViewItem)
            {
                var item = e.UserState as ListViewItem;

                lvPlaylistVideos.Items.Add(item);
                lvPlaylistVideos.TopItem = item;
            }
            else if (e.UserState is QuickPlaylist)
            {
                _playlist = e.UserState as QuickPlaylist;
            }
        }

        private void _backgroundWorkerPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result = (bool)e.Result;

            _playlistCancel = false;
            btnGetPlaylist.Text = "Get";
            btnPlaylistDownloadAll.Enabled = result;
            btnPlaylistDownloadSelected.Enabled = result;
            lvPlaylistVideos.UseWaitCursor = false;

            this.FilterPlaylist();

            if (!result)
            {
                lvPlaylistVideos.Items.Clear();
            }
        }

        private void cmsPlaylistOptions_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Settings.Default.PlaylistNamedFolder = playlistNamedFolderMenuItem.Checked;
            Settings.Default.PlaylistIgnoreExisting = playlistIgnoreExistingMenuItem.Checked;
            Settings.Default.PlaylistReverse = playlistReverseMenuItem.Checked;
            Settings.Default.PlaylistNumberPrefix = playlistNumberPrefixMenuItem.Checked;
        }

        private void cmsPlaylistOptions_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        }

        private void cmsPlaylistOptions_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistIgnoreExistingMenuItem)
            {
                this.FilterPlaylist();
            }
            else if (e.ClickedItem == playlistReverseMenuItem)
            {
                this.PlaylistReverse();
            }
        }

        private void cmsPlaylistOptions_Opening(object sender, CancelEventArgs e)
        {
            playlistNamedFolderMenuItem.Checked = Settings.Default.PlaylistNamedFolder;
            playlistIgnoreExistingMenuItem.Checked = Settings.Default.PlaylistIgnoreExisting;
            playlistReverseMenuItem.Checked = Settings.Default.PlaylistReverse;
            playlistNumberPrefixMenuItem.Checked = Settings.Default.PlaylistNumberPrefix;
        }

        private void btnPlaylistDownloadSelected_Click(object sender, EventArgs e)
        {
            if (lvPlaylistVideos.CheckedItems.Count < 1)
            {
                MessageBox.Show(this, "No videos selected.");
                return;
            }

            this.StartPlaylistOperation(lvPlaylistVideos.CheckedItems
                                            .Cast<ListViewItem>()
                                            .Select(x => x.Tag as QuickVideoInfo));
        }

        private void btnPlaylistDownloadAll_Click(object sender, EventArgs e)
        {
            this.StartPlaylistOperation(lvPlaylistVideos.Items
                                            .Cast<ListViewItem>()
                                            .Select(x => x.Tag as QuickVideoInfo));
        }

        private void cbPlaylistSaveTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.SelectedDirectoryPlaylist = cbPlaylistSaveTo.SelectedIndex;
            this.FilterPlaylistReset();
            this.FilterPlaylist();
        }

        private void cbPlaylistQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.PreferredQualityPlaylist = (PreferredQuality)cbPlaylistQuality.SelectedIndex;
        }

        private void playlistOpenMenuItem_Click(object sender, EventArgs e)
        {
            bool error = false;

            foreach (ListViewItem item in lvPlaylistVideos.SelectedItems)
            {
                try
                {
                    Process.Start($"https://www.youtube.com/watch?v={ (item.Tag as QuickVideoInfo).ID}");
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
                MessageBox.Show(this, "Some links couldn't be opened.");
        }

        private void playlistCopyMenuItem_Click(object sender, EventArgs e)
        {
            string text = string.Join(
                Environment.NewLine,
                lvPlaylistVideos.SelectedItems
                    .Cast<ListViewItem>()
                    .Select(x => x.SubItems[1].Text));

            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't set the clipboard text.");
            }
        }

        private void playlistSelectAllMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvPlaylistVideos.Items)
                item.Checked = true;
        }

        private void playlistSelectNoneMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvPlaylistVideos.Items)
                item.Checked = false;
        }

        private ListViewItem[] GetFilteredItems()
        {
            var items = new List<ListViewItem>();
            var filters = txtPlaylistFilter.Text.Split(';');

            foreach (string filter in filters)
                foreach (ListViewItem item in lvPlaylistVideos.Items
                                                .Cast<ListViewItem>()
                                                .Where(i => i.SubItems[1].Text.ToLower().Contains(filter.ToLower())))
                    items.Add(item);

            return items.ToArray();
        }

        private void btnPlaylistRemove_Click(object sender, EventArgs e)
        {
            var items = this.GetFilteredItems();

            if (items.Length == 0)
                return; // Do nothing

            if (MessageBox.Show(this,
                    $"Are you sure you want to delete {items.Length} item(s)?",
                    "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            lvPlaylistVideos.BeginUpdate();

            foreach (var item in items)
            {
                lvPlaylistVideos.Items.Remove(item);
            }

            lvPlaylistVideos.EndUpdate();
        }

        private void btnPlaylistToggle_Click(object sender, EventArgs e)
        {
            lvPlaylistVideos.BeginUpdate();

            foreach (var item in this.GetFilteredItems())
            {
                item.Checked = !item.Checked;
            }

            lvPlaylistVideos.EndUpdate();
        }

        private void btnPlaylistSearch_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvPlaylistVideos.Items)
                item.BackColor = SystemColors.Window;

            if (string.IsNullOrEmpty(txtPlaylistFilter.Text))
                return;

            lvPlaylistVideos.BeginUpdate();

            foreach (var item in this.GetFilteredItems())
            {
                item.BackColor = Color.LightGray;
            }

            lvPlaylistVideos.EndUpdate();
        }

        private void StartPlaylistOperation(IEnumerable<QuickVideoInfo> videos)
        {
            string path = cbPlaylistSaveTo.Text;

            // Make sure download directory exists.
            if (!this.ValidateDirectory(path))
                return;

            if (!cbPlaylistSaveTo.Items.Contains(path))
                cbPlaylistSaveTo.Items.Add(path);

            Settings.Default.LastPlaylistUrl = txtPlaylistLink.Text;

            if (playlistNamedFolderMenuItem.Checked)
            {
                path = Path.Combine(path, Helper.FormatTitle(_playlist.Title));

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            try
            {
                var operation = new PlaylistOperation(txtPlaylistLink.Text,
                                                      path,
                                                      Settings.Default.PreferredQualityPlaylist,
                                                      playlistReverseMenuItem.Checked,
                                                      playlistNumberPrefixMenuItem.Checked,
                                                      videos);
                operation.FileDownloadComplete += playlistOperation_FileDownloadComplete;

                this.AddQueueItem(operation, true);
                DownloadQueueHandler.Add(operation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void playlistOperation_FileDownloadComplete(object sender, string file)
        {
            if (chbAutoConvert.Enabled && chbAutoConvert.Checked)
            {
                string output = Path.Combine(Path.GetDirectoryName(file),
                    Path.GetFileNameWithoutExtension(file)) + ".mp3";

                var item = this.Convert(file, output, false);

                // Automatically remove convert operation from queue when done
                item.OperationComplete += delegate
                {
                    olvQueue.RemoveObject(item);
                };
            }
        }

        private void FilterPlaylist()
        {
            // Reset if necessary
            if (!playlistIgnoreExistingMenuItem.Checked && _playlistIgnored.Count > 0)
            {
                this.FilterPlaylistReset();
            }
            else if (playlistIgnoreExistingMenuItem.Checked)
            {
                string path = cbPlaylistSaveTo.Text;

                if (_playlist != null && playlistNamedFolderMenuItem.Checked)
                    path = Path.Combine(path, _playlist.Title);

                if (!Directory.Exists(path))
                    return;

                string[] files = Directory.GetFiles(path)
                    .Select(x => Path.GetFileNameWithoutExtension(x)).ToArray();

                for (int i = lvPlaylistVideos.Items.Count - 1; i >= 0; i--)
                {
                    if (files.Contains(lvPlaylistVideos.Items[i].Text))
                    {
                        _playlistIgnored.Insert(0, i, lvPlaylistVideos.Items[i]);
                        lvPlaylistVideos.Items.RemoveAt(i);
                    }
                }
            }
        }

        private void FilterPlaylistReset()
        {
            if (_playlistIgnored.Count == 0)
                return;

            var indexes = _playlistIgnored.Keys.Cast<int>().ToArray();
            var items = _playlistIgnored.Values.Cast<ListViewItem>().ToArray();

            for (int i = 0; i < _playlistIgnored.Count; i++)
                lvPlaylistVideos.Items.Insert(indexes[i], items[i]);

            _playlistIgnored.Clear();
        }

        private void PlaylistReverse()
        {
            lvPlaylistVideos.BeginUpdate();

            // Re-add filtered items so they can be reversed
            this.FilterPlaylistReset();

            var existing = new List<ListViewItem>(lvPlaylistVideos.Items.Cast<ListViewItem>());
            existing.Reverse();

            lvPlaylistVideos.Items.Clear();
            lvPlaylistVideos.Items.AddRange(existing.ToArray());

            // Re-filter, now with reversed indexes
            this.FilterPlaylist();

            lvPlaylistVideos.EndUpdate();

            _playlistReversed = !_playlistReversed;
        }

        #endregion

        #region Batch Tab

        private void chbBatchCreateFolder_CheckedChanged(object sender, EventArgs e)
        {
            txtBatchFolder.Enabled = chbBatchCreateFolder.Checked;
        }

        private void cbBatchPreferredQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.PreferredQualityBatch = (PreferredQuality)cbBatchPreferredQuality.SelectedIndex;
        }

        private void btnBatchBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFolderDialog())
            {
                if (Directory.Exists(cbBatchSaveTo.Text))
                    ofd.InitialFolder = cbBatchSaveTo.Text;
                else
                    ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    cbSaveTo.Items.Add(ofd.Folder);
                    cbBatchSaveTo.SelectedIndex = cbBatchSaveTo.Items.Add(ofd.Folder);
                }
            }
        }

        private void btnBatchDownload_Click(object sender, EventArgs e)
        {
            if (txtBatchLinks.Lines.All(x => Helper.IsValidYouTubeUrl(x)))
            {
                this.StartBatchDownloadOperation(txtBatchLinks.Lines, Settings.Default.PreferredQualityBatch);
            }
            else
            {
                MessageBox.Show(this, "One or more URLs are not valid.");
            }
        }

        private void StartBatchDownloadOperation(ICollection<string> videos, PreferredQuality preferredQuality)
        {
            string path = cbBatchSaveTo.Text;

            // Make sure download directory exists.
            if (!this.ValidateDirectory(path))
                return;

            if (chbBatchCreateFolder.Checked)
            {
                path = Path.Combine(path, txtBatchFolder.Text);
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    MessageBox.Show(this, $"Couldn\'t create \"{txtBatchFolder.Text}\" folder. Might be invalid character(s).");
                    return;
                }
            }

            try
            {
                var operation = new BatchOperation(path,
                                                   videos,
                                                   preferredQuality,
                                                   chbBatchIgnoreExisting.Checked,
                                                   chbBatchNumberPrefix.Checked);

                this.AddQueueItem(operation, true);
                DownloadQueueHandler.Add(operation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Convert Tab

        OpenFolderDialog openFolderDialog = new OpenFolderDialog();

        private void ConvertRadioButtons_CheckedChanged(object sender, EventArgs e)
        {
            pConvertFile.Visible = rbConvertFile.Checked;
            pConvertFolder.Visible = rbConvertFolder.Checked;

            // Disable cropping tool when selecting folder
            gCropping.Enabled = rbConvertFile.Checked;
        }

        private void btnBrowseInputFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = Path.GetFileName(txtInputFile.Text);

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtInputFile.Text = openFileDialog1.FileName;

                if (txtOutputFile.Text == string.Empty)
                {
                    // Suggest file name
                    string output = Path.GetDirectoryName(openFileDialog1.FileName);

                    output = Path.Combine(output, Path.GetFileNameWithoutExtension(openFileDialog1.FileName));
                    output += ".mp3";

                    txtOutputFile.Text = output;
                }
            }
        }

        private void btnBrowseOutputFile_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtInputFile.Text))
            {
                saveFileDialog1.FileName = Path.GetFileName(txtInputFile.Text);
            }
            else
            {
                saveFileDialog1.FileName = Path.GetFileName(txtOutputFile.Text);
            }

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFile.Text = saveFileDialog1.FileName;
            }
        }

        private void btnBrowseInputFolder_Click(object sender, EventArgs e)
        {
            openFolderDialog.InitialFolder = Path.GetFileName(txtInputFolder.Text);

            if (openFolderDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtInputFolder.Text = openFolderDialog.Folder;

                if (string.IsNullOrEmpty(txtOutputFolder.Text))
                {
                    // Suggest output path
                    txtOutputFolder.Text = txtInputFolder.Text;
                }
            }
        }

        private void btnBrowseOutputFolder_Click(object sender, EventArgs e)
        {
            openFolderDialog.InitialFolder = Path.GetFileName(txtOutputFolder.Text);

            if (openFolderDialog.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFolder.Text = openFolderDialog.Folder;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (rbConvertFile.Checked)
            {
                if (!File.Exists(txtInputFile.Text))
                {
                    MessageBox.Show(this, "Input file not found.", "Missing File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (!FFmpeg.CanConvertToMP3(txtInputFile.Text).Value)
                {
                    MessageBox.Show(this, "Can't convert input file to MP3. File doesn't appear to have audio.",
                        "Missing Audio",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                if (File.Exists(txtOutputFile.Text))
                {
                    string filename = Path.GetFileName(txtOutputFile.Text);
                    string text = "File '" + filename + "' already exists.\n\nOverwrite?";

                    if (MessageBox.Show(this, text, "Overwrite", MessageBoxButtons.YesNo) == DialogResult.No)
                    {
                        return;
                    }
                }
                else
                {
                    try
                    {
                        string folder = Path.GetDirectoryName(txtOutputFile.Text);

                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        File.WriteAllText(txtOutputFile.Text, string.Empty);
                        File.Delete(txtOutputFile.Text);
                    }
                    catch
                    {
                        MessageBox.Show(this, "Output path was invalid, check if it contains invalid characters.",
                            "Output Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }

                if (chbCropFrom.Checked &&
                    (txtInputFile.Text == txtOutputFile.Text ||
                    // If they match, the user probably wants to crop. Right?
                    Path.GetExtension(txtInputFile.Text) == Path.GetExtension(txtOutputFile.Text)))
                {
                    this.Crop(txtInputFile.Text, txtOutputFile.Text);
                }
                else
                {
                    this.Convert(txtInputFile.Text, txtOutputFile.Text, chbCropFrom.Checked);
                }

                txtInputFile.Clear();
                txtOutputFile.Clear();
            }
            else
            {
                if (!Directory.Exists(txtInputFolder.Text))
                    Directory.CreateDirectory(txtInputFolder.Text);

                if (!Directory.Exists(txtOutputFolder.Text))
                    Directory.CreateDirectory(txtOutputFolder.Text);

                this.ConvertFolder(txtInputFolder.Text,
                                   txtOutputFolder.Text,
                                   txtExtension.Text.TrimStart('.'));
            }

            tabControl1.SelectedTab = queueTabPage;
        }

        private void btnCheckAgain_Click(object sender, EventArgs e)
        {
            Program.FFmpegAvailable = File.Exists(FFmpeg.FFmpegPath);

            groupBox2.Enabled = chbAutoConvert.Enabled = Program.FFmpegAvailable;
            lFFmpegMissing.Visible = btnCheckAgain.Visible = !Program.FFmpegAvailable;

            MessageBox.Show(this, Program.FFmpegAvailable ? "Found FFmmpeg, enabling related functions." : "Did not find FFmpeg.");
        }

        private void chbCropFrom_CheckedChanged(object sender, EventArgs e)
        {
            mtxtFrom.Enabled = chbCropTo.Enabled = chbCropFrom.Checked;
            mtxtTo.Enabled = chbCropFrom.Checked && chbCropTo.Checked;
        }

        private void chbCropTo_CheckedChanged(object sender, EventArgs e)
        {
            mtxtTo.Enabled = chbCropTo.Checked;
        }

        #endregion

        #region mainMenu1

        MainMenu mainMenu1;
        MenuItem fileMenuItem;
        MenuItem exitMenuItem;
        MenuItem toolsMenuItem;
        MenuItem optionsMenuItem;
        MenuItem helpMenuItem;
        MenuItem checkForUpdateMenuItem;
        MenuItem aboutMenuItem;

        private void InitializeMainMenu()
        {
            var fileMenuItems = new MenuItem[]
            {
                exitMenuItem = new MenuItem("&Exit", exitMenuItem_Click, Shortcut.CtrlQ)
            };
            var toolsMenuItems = new MenuItem[]
            {
                optionsMenuItem = new MenuItem("&Options", optionsMenuItem_Click)
            };
            var helpMenuItems = new MenuItem[]
            {
                checkForUpdateMenuItem = new MenuItem("&Check for updates", checkForUpdateMenuItem_Click),
                aboutMenuItem = new MenuItem("&About", aboutMenuItem_Click)
            };

            mainMenu1 = new MainMenu();
            mainMenu1.MenuItems.Add(fileMenuItem = new MenuItem("&File", fileMenuItems));
            mainMenu1.MenuItems.Add(toolsMenuItem = new MenuItem("&Tools", toolsMenuItems));
            mainMenu1.MenuItems.Add(helpMenuItem = new MenuItem("&Help", helpMenuItems));

            mainMenu1.Collapse += delegate (object sender, EventArgs e)
            {
                // Remove update notification from Help menu item
                helpMenuItem.Text = "&Help";
            };

            this.Menu = mainMenu1;
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsMenuItem_Click(object sender, EventArgs e)
        {
            using (var of = new OptionsForm())
            {
                if (of.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh video formats, in case included formats has changed
                    if (_selectedVideo != null)
                    {
                        cbQuality.Items.Clear();
                        cbQuality.Items.AddRange(this.CheckFormats(_selectedVideo.Formats));
                        cbQuality.SelectedIndex = cbQuality.Items.Count - 1;
                    }
                }
            }
        }

        private void checkForUpdateMenuItem_Click(object sender, EventArgs e)
        {
            new UpdateDownloader().ShowDialog(this);
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            new AboutForm().ShowDialog(this);
        }

        #endregion

        #region contextMenu1

        private void contextMenu1_Popup(object sender, EventArgs e)
        {
            if (olvQueue.SelectedObjects.Count == 0)
            {
                foreach (MenuItem menuItem in contextMenu1.MenuItems)
                    if (menuItem != clearCompletedMenuItem)
                        menuItem.Visible = false;

                return;
            }

            viewErrorsMenuItem.Visible = viewErrorsSeparator.Visible = olvQueue.SelectedObjects
                .Cast<OperationModel>()
                .All(
                    x => x.Operation.Exception != null
                 );

            bool canOpen = false, canPause = false, canResume = false, canStop = false, canConvert = false;

            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                // These variables should never be set to false again if they're true
                if (operation.CanOpen())
                    canOpen = true;

                if (operation.CanPause())
                    canPause = true;

                if (operation.CanResume())
                    canResume = true;

                if (operation.CanStop())
                    canStop = true;

                if (operation is DownloadOperation && operation.Status == OperationStatus.Success)
                    canConvert = true;
            }

            openMenuItem.Visible = canOpen;
            pauseMenuItem.Visible = canPause;
            resumeMenuItem.Visible = canResume;
            stopMenuItem.Visible = canStop;
            menuItem5.Visible = convertToMP3MenuItem.Visible = canConvert; // menuItem5 = splitter under
        }

        private void contextMenu1_Collapse(object sender, EventArgs e)
        {
            foreach (MenuItem menuItem in contextMenu1.MenuItems)
            {
                menuItem.Enabled = true;
                menuItem.Visible = true;
            }
        }

        private void viewErrorsMenuItem_Click(object sender, EventArgs e)
        {
            Dialogs.ExceptionDialog.ShowDialog(this,
                (olvQueue.SelectedObjects[0] as OperationModel).Operation.Exception.Message,
                "Error",
                (olvQueue.SelectedObjects[0] as OperationModel).Operation.Exception);
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            int fails = 0;

            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (operation.CanOpen() && !operation.Open())
                    fails++;
            }

            if (fails > 0)
            {
                MessageBox.Show(this, $"Failed to open {fails} file(s).");
            }
        }

        private void openContainingFolderMenuItem_Click(object sender, EventArgs e)
        {
            int fails = 0;

            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (!operation.OpenContainingFolder())
                    fails++;
            }

            if (fails > 0)
            {
                MessageBox.Show(this, $"Failed to open {fails} folder(s).");
            }
        }

        private void convertToMP3MenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (operation is DownloadOperation && operation.Status == OperationStatus.Success)
                {
                    string input = (operation as DownloadOperation).Output;
                    string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".mp3";

                    txtInputFile.Text = input;
                    txtOutputFile.Text = output;
                    tabControl1.SelectedTab = convertTabPage;
                    break;
                }
            }
        }

        private void resumeMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (operation.CanResume())
                    operation.Resume();
            }
        }

        private void pauseMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (operation.CanPause())
                    operation.Pause();
            }
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationModel item in olvQueue.SelectedObjects)
            {
                var operation = item.Operation;

                if (operation.CanStop())
                    operation.Stop();
            }
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = olvQueue.SelectedObjects.Count - 1; i >= 0; i--)
            {
                var item = (OperationModel)olvQueue.SelectedObjects[i];
                var operation = item.Operation;

                operation.Stop();

                olvQueue.RemoveObject(item);
            }
        }

        private void clearCompletedMenuItem_Click(object sender, EventArgs e)
        {
            var models = new List<OperationModel>();

            foreach (OperationModel model in olvQueue.Objects)
                if (model.Operation.IsDone)
                    models.Add(model);

            olvQueue.RemoveObjects(models);
        }

        #endregion

        /// <summary>
        /// Inserts a video url &amp; retrieve video info automatically.
        /// </summary>
        /// <param name="url">The url to insert.</param>
        public void InsertVideo(string url)
        {
            txtYoutubeLink.Text = url;
            btnGetVideo.PerformClick();
        }

        /// <summary>
        /// Cancels all active Operations.
        /// </summary>
        private async void CancelOperations()
        {
            this.Hide();

            foreach (Operation operation in Operation.Running)
            {
                // Stop & delete unfinished files
                if (operation.CanStop())
                    operation.Stop();
            }

            if (bwGetVideo.IsBusy)
                bwGetVideo.CancelAsync();

            await Task.Run(delegate
            {
                while (Operation.Running.Count > 0 || bwGetVideo.IsBusy)
                {
                    // Wait for everything to finish
                }
            });

            this.Close();
        }

        private OperationModel AddQueueItem(Operation operation, bool switchTab = false)
        {
            var item = new OperationModel(operation);

            item.AspectChanged += OperationModel_AspectChanged;
            item.OperationComplete += OperationModel_OperationComplete;

            olvQueue.AddObject(item);
            olvQueue.SelectedObject = item;

            if (switchTab)
                tabControl1.SelectedTab = queueTabPage;

            return item;
        }

        /// <summary>
        /// Starts a ConvertOperation.
        /// </summary>
        /// <param name="input">The file to convert.</param>
        /// <param name="output">The path to save converted file.</param>
        /// <param name="crop">True if converted file should be cropped.</param>
        private OperationModel Convert(string input, string output, bool crop)
        {
            TimeSpan start, end;
            start = end = TimeSpan.MinValue;

            if (crop && chbCropFrom.Checked)
            {
                // Validate cropping input. Shows error messages automatically.
                if (!this.ValidateCropping())
                    return null;

                start = TimeSpan.Parse(mtxtFrom.Text);
                end = TimeSpan.Parse(mtxtTo.Text);
            }

            var operation = new ConvertOperation(input, output, start, end);

            operation.Start();

            return this.AddQueueItem(operation, false);
        }

        /// <summary>
        /// Starts a ConvertOperation to convert all files in folder, matching extension.
        /// </summary>
        /// <param name="input">The folder to convert files from.</param>
        /// <param name="output">The output folder, where all converted files will be placed.</param>
        /// <param name="extension">The extension to match.</param>
        private void ConvertFolder(string input, string output, string extension)
        {
            var operation = new ConvertOperation(input, output, extension);

            operation.Start();

            this.AddQueueItem(operation, false);
        }

        /// <summary>
        /// Starts a CroppingOperation.
        /// </summary>
        /// <param name="input">The file to crop.</param>
        /// <param name="output">The path to save cropped file.</param>
        private void Crop(string input, string output)
        {
            // Validate cropping input. Shows error messages automatically.
            if (!this.ValidateCropping())
                return;

            TimeSpan start = TimeSpan.Parse(mtxtFrom.Text);
            TimeSpan end = TimeSpan.Parse(mtxtTo.Text);

            if (start > end)
                end = TimeSpan.MinValue;

            var operation = new CroppingOperation(input, output, start, end);

            operation.Start();

            this.AddQueueItem(operation, false);
        }

        /// <summary>
        /// Returns string array of column widths, ordered by index.
        /// </summary>
        private string[] GetColumnWidths()
        {
            return olvQueue.AllColumns
                    .Select(x => x.Width.ToString())
                    .ToArray();
        }

        /// <summary>
        /// Returns true if there is a working <see cref="Operation"/>.
        /// </summary>
        private bool IsWorking()
        {
            return olvQueue.Objects
                    .Cast<OperationModel>()
                    .Any(x => x.Operation.IsWorking);
        }

        /// <summary>
        /// Returns string of Booleans as 1s and 0s for the visibility of each column, ordered by index.
        /// </summary>
        private string GetVisibleColumns()
        {
            return string.Join(",", olvQueue.AllColumns
                                    .Select(x => System.Convert.ToInt32(x.IsVisible)));
        }

        /// <summary>
        /// Loads & applies all application settings.
        /// </summary>
        private void LoadSettings()
        {
            var settings = Settings.Default;

            // Upgrade settings between new versions. 
            // More information:
            // http://www.ngpixel.com/2011/05/05/c-keep-user-settings-between-versions/
            if (settings.UpdateSettings)
            {
                settings.Upgrade();
                settings.UpdateSettings = false;
                settings.Save();
            }

            // Initialize WindowStates collection if null
            if (settings.WindowStates == null)
            {
                settings.WindowStates = new WindowStates();
            }

            // Restore form location, size & window state, if not null
            settings.WindowStates.Restore(this, false);

            // Initialize StringCollection if null
            if (settings.SaveToDirectories == null)
            {
                settings.SaveToDirectories = new StringCollection();
            }

            // Copy StringCollection to string array
            string[] directories = new string[settings.SaveToDirectories.Count];

            settings.SaveToDirectories.CopyTo(directories, 0);

            // Add string array to ComboBoxes
            cbSaveTo.Items.AddRange(directories);
            cbPlaylistSaveTo.Items.AddRange(directories);

            // Restore ComboBox.SelectedIndex if it's not empty
            if (cbSaveTo.Items.Count > 0 && settings.SelectedDirectory < cbSaveTo.Items.Count)
                cbSaveTo.SelectedIndex = settings.SelectedDirectory;

            if (cbPlaylistSaveTo.Items.Count > 0)
                cbPlaylistSaveTo.SelectedIndex = settings.SelectedDirectoryPlaylist;

            cbPlaylistQuality.Items.AddRange(Enum.GetNames(typeof(PreferredQuality)));
            cbPlaylistQuality.SelectedIndex = (int)settings.PreferredQualityPlaylist;

            // Batch
            // ========================================
            cbBatchSaveTo.Items.AddRange(directories);

            if (cbBatchSaveTo.Items.Count > 0 && settings.SelectedDirectory < cbBatchSaveTo.Items.Count)
                cbBatchSaveTo.SelectedIndex = settings.SelectedDirectoryBatch;

            cbBatchPreferredQuality.Items.AddRange(Enum.GetNames(typeof(PreferredQuality)));
            cbBatchPreferredQuality.SelectedIndex = (int)settings.PreferredQualityBatch;
            // ========================================

            // Restore CheckBox.Checked
            chbAutoConvert.Checked = settings.AutoConvert;

            // Restore last used links
            if (settings.LastYouTubeUrl != null) txtYoutubeLink.Text = settings.LastYouTubeUrl;
            if (settings.LastPlaylistUrl != null) txtPlaylistLink.Text = settings.LastPlaylistUrl;

            chbMaxSimDownloads.Checked = settings.ShowMaxSimDownloads;
            nudMaxSimDownloads.Enabled = settings.ShowMaxSimDownloads;
            nudMaxSimDownloads.Value = settings.MaxSimDownloads;

            // Restore visible columns
            string[] cols = settings.VisibleColumns.Split(',');
            for (int i = 0; i < olvQueue.AllColumns.Count; i++)
                olvQueue.GetColumn(i).IsVisible = System.Convert.ToBoolean(int.Parse(cols[i]));

            // Restore column widths
            if (settings.ColumnWidths == null)
                settings.ColumnWidths = new StringCollection();
            for (int i = 0; i < settings.ColumnWidths.Count; i++)
                olvQueue.GetColumn(i).Width = int.Parse(settings.ColumnWidths[i]);

            olvQueue.RebuildColumns();
        }

        /// <summary>
        /// Saves all application settings.
        /// </summary>
        private void SaveSettings()
        {
            var settings = Settings.Default;

            settings.WindowStates.Save(this);
            settings.SaveToDirectories.Clear();

            string[] paths = new string[cbSaveTo.Items.Count];
            cbSaveTo.Items.CopyTo(paths, 0);
            string[] pathsPlaylist = new string[cbPlaylistSaveTo.Items.Count];
            cbPlaylistSaveTo.Items.CopyTo(pathsPlaylist, 0);

            // Merge paths, removing duplicates
            paths = paths.Union(pathsPlaylist).ToArray();

            settings.SaveToDirectories.AddRange(paths);
            settings.SelectedDirectory = cbSaveTo.SelectedIndex;
            settings.AutoConvert = chbAutoConvert.Checked;
            settings.MaxSimDownloads = (int)nudMaxSimDownloads.Value;
            settings.VisibleColumns = this.GetVisibleColumns();

            settings.ColumnWidths.Clear();
            settings.ColumnWidths.AddRange(this.GetColumnWidths());

            settings.Save();
        }

        /// <summary>
        /// Setups Queue ObjectListView with renderers etc...
        /// </summary>
        private void SetupQueue()
        {
            OLVColumn col = this.olvQueue.Columns[1] as OLVColumn;
            col.Renderer = new BarTextRenderer(0, 100)
            {
                TextBrush = Brushes.Black
            };

            this.olvQueue.SetObjects(new object[0]);
        }

        /// <summary>
        /// Shows update notification if update is available.
        /// </summary>
        private async void ShowUpdateNotification()
        {
            if (await UpdateHelper.IsUpdateAvailableAsync())
            {
                helpMenuItem.Text += " (Update available)";
            }
        }

        /// <summary>
        /// Returns true if cropping information can be validated. Fills empty space with zeros.
        /// </summary>
        private bool ValidateCropping()
        {
            try
            {
                // Fill in empty space with zeros
                mtxtFrom.Text = mtxtFrom.Text.Replace(' ', '0');
                while (mtxtFrom.Text.Length < 12) mtxtFrom.Text += "0";

                // Validate TimeSpan object
                mtxtFrom.ValidateText();

                if (chbCropTo.Enabled && chbCropTo.Checked)
                {
                    // Fill in empty space with zeros
                    mtxtTo.Text = mtxtTo.Text.Replace(' ', '0');
                    while (mtxtTo.Text.Length < 12) mtxtTo.Text += "0";

                    // Validate TimeSpan object
                    mtxtTo.ValidateText();
                }

                return true;
            }
            catch
            {
                MessageBox.Show(this, "Cropping information error.");
                return false;
            }
        }

        /// <summary>
        /// Returns true if directory is not null & exists. Prompts the user to create directory if it doesn't exist.
        /// </summary>
        /// <param name="directory">The directory to validate.</param>
        private bool ValidateDirectory(string directory)
        {
            try
            {
                if (string.IsNullOrEmpty(directory))
                {
                    MessageBox.Show(this, "Download path is empty.");
                }
                else if (Directory.Exists(directory))
                {
                    return true;
                }
                else
                {
                    string text = "Download path doesn't exists.\n\nDo you want to create it?";

                    if (MessageBox.Show(this, text, "Missing Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(directory);
                        return true;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Couldn't create directory.");
            }

            return false;
        }

        /// <summary>
        /// Returns true if the filename doesn't contain illegal characters, or has been formatted after prompting user.
        /// </summary>
        /// <param name="filename">The filename to validate.</param>
        private bool ValidateFilename(string filename)
        {
            bool valid = true;
            char[] illegalChars = Path.GetInvalidFileNameChars();

            // Check if filename contains illegal characters
            // Returning true for some reason: valid = filename.Any(x => illegalChars.Contains(x));
            valid = filename.IndexOfAny(illegalChars) <= -1;

            if (!valid)
            {
                string new_filename = Helper.FormatTitle(filename);
                string text = "Filename contains illegal characters, do you want to automatically remove these characters?\n\n" +
                    $"New: '{new_filename}'\n\n" +
                    "Clicking 'No' will cancel the download.";

                if (MessageBox.Show(this, text, "Illegal Characters", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    txtTitle.Text = new_filename;
                    valid = true;
                }
            }

            return valid;
        }

        /// <summary>
        /// Removes unsupported &amp; unnecessary formats.
        /// </summary>
        /// <param name="list">The list of VideoFormat to check.</param>
        private VideoFormat[] CheckFormats(IList<VideoFormat> list)
        {
            var formats = new List<VideoFormat>(list);

            formats.RemoveAll(f => f.Extension.Contains("webm") ||
                                   (f.VideoInfo.VideoSource == VideoSource.YouTube && f.HasAudioAndVideo) ||
                                   f.FormatID == "meta");

            return formats.ToArray();
        }

#if DEBUG
        private void AddDummyDownloadOperation(long workTimeMS)
        {
            Operation operation = new DummyDownloadOperation(workTimeMS);

            var item = new OperationModel(operation);

            item.AspectChanged += OperationModel_AspectChanged;

            olvQueue.AddObject(item);

            DownloadQueueHandler.Add(operation);
        }

        private void ReadDebugFile()
        {
            string file = @"..\..\..\DEBUG_FILE.txt";

            if (!File.Exists(file))
                return;

            using (var reader = new StreamReader(file))
            {
                string line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#"))
                        continue;
                    else if (line.StartsWith("!"))
                    {
                        int count = int.Parse(line.Replace("!", ""));

                        for (int i = 0; i < count; i++)
                            this.AddDummyDownloadOperation(100000);
                    }
                    else
                    {
                        string[] data = line.Split('|');
                        object obj = null;
                        obj = this.GetType().GetField(data[0], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this);
                        object converted_value = System.Convert.ChangeType(data[3], Type.GetType(data[2]));
                        obj.GetType().GetProperty(data[1]).SetValue(obj, converted_value);
                    }
                }
            }
        }
#endif
    }
}