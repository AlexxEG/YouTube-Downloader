using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Controls;
using YouTube_Downloader.Properties;
using YouTube_Downloader_DLL;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Dialogs;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.Operations;

/* ToDo: 
 *
 * - Handle aborting operations better when closing form.
 * - Make sure OperationStatus is set for operations in BackgroundWorker.DoWork.
 */

namespace YouTube_Downloader
{
    public partial class MainForm : Form
    {
        private string[] _args;
        private VideoInfo _selectedVideo;
        private Settings _settings = Settings.Default;

        private delegate void UpdateFileSize(object sender, FileSizeUpdateEventArgs e);

        public MainForm()
        {
            InitializeComponent();
            InitializeMainMenu();

            lvQueue.ContextMenu = contextMenu1;
            lvPlaylistVideos.ContextMenu = cmPlaylistList;

            mtxtTo.ValidatingType = typeof(TimeSpan);
            mtxtFrom.ValidatingType = typeof(TimeSpan);

            // Remove file size label text, should be empty when first starting.
            lFileSize.Text = string.Empty;
        }

        public MainForm(string[] args)
            : this()
        {
            this._args = args;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.GetIsWorking())
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

                e.Cancel = true;
                return;
            }

            if (_selectedVideo != null)
                _selectedVideo.AbortUpdateFileSizes();

            _settings.WindowStates[this.Name].SaveForm(this);
            _settings.SaveToDirectories.Clear();

            string[] paths = new string[cbSaveTo.Items.Count];
            cbSaveTo.Items.CopyTo(paths, 0);
            string[] pathsPlaylist = new string[cbPlaylistSaveTo.Items.Count];
            cbPlaylistSaveTo.Items.CopyTo(pathsPlaylist, 0);

            // Merge paths, removing duplicates
            paths = paths.Union(pathsPlaylist).ToArray();

            _settings.SaveToDirectories.AddRange(paths);
            _settings.SelectedDirectory = cbSaveTo.SelectedIndex;
            _settings.AutoConvert = chbAutoConvert.Checked;

            _settings.Save();

            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (_args != null)
            {
                txtYoutubeLink.Text = _args[0];
                btnGetVideo.PerformClick();
                _args = null;
            }

            // Disable & enable functions depending on if FFmpeg is available, and
            // display error for the user.
            groupBox2.Enabled = chbAutoConvert.Enabled = Program.FFmpegAvailable;
            lFFmpegMissing.Visible = btnCheckAgain.Visible = !Program.FFmpegAvailable;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bottomPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw line on top of panel.
            e.Graphics.DrawLine(new Pen(Color.Silver, 2), new Point(0, 1), new Point(bottomPanel.Width, 1));
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                object tag = (sender as LinkLabel).Tag;

                if (tag == null)
                    return;

                Process.Start((string)tag);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }

        #region Download Tab

        private void btnPaste_Click(object sender, EventArgs e)
        {
            // Only paste if enabled so that it can't be changed
            // while getting video information.
            if (txtYoutubeLink.Enabled)
            {
                txtYoutubeLink.Text = Clipboard.GetText();
            }
        }

        private void btnGetVideo_Click(object sender, EventArgs e)
        {
            if (!Helper.IsValidUrl(txtYoutubeLink.Text))
            {
                MessageBox.Show(this, "Input link is not a valid Twitch/YouTube link.", "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                txtYoutubeLink.Text = Helper.FixUrl(txtYoutubeLink.Text);

                _settings.LastYouTubeUrl = txtYoutubeLink.Text;

                txtTitle.Text = string.Empty;
                cbQuality.Items.Clear();
                btnGetVideo.Enabled = txtYoutubeLink.Enabled = btnDownload.Enabled = cbQuality.Enabled = false;
                videoThumbnail.Tag = null;

                bwGetVideo.RunWorkerAsync(txtYoutubeLink.Text);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

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
                VideoFormat tempFormat = cbQuality.SelectedItem as VideoFormat;
                string filename = string.Format("{0}.{1}", txtTitle.Text, tempFormat.Extension);

                if (File.Exists(Path.Combine(path, filename)))
                {
                    DialogResult result = MessageBox.Show(this,
                        string.Format("File '{1}' already exists.{0}{0}Overwrite?", Environment.NewLine, filename),
                        "Overwrite?",
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.No)
                        return;

                    File.Delete(Path.Combine(path, filename));
                }

                Operation operation;

                if (_selectedVideo.VideoSource == VideoSource.Twitch)
                {
                    operation = new TwitchOperation(tempFormat);
                }
                else
                {
                    operation = new DownloadOperation(tempFormat);
                }

                var item = new OperationListViewItem(Path.GetFileName(filename), tempFormat.VideoInfo.Url, operation);

                item.Duration = Helper.FormatVideoLength(tempFormat.VideoInfo.Duration);

                // Combine video and audio file size if the format is DASH and not AudioOnly
                if (_selectedVideo.VideoSource == VideoSource.YouTube && tempFormat.FormatType != FormatType.Normal && !tempFormat.AudioOnly)
                    item.FileSize = Helper.FormatFileSize(tempFormat.FileSize + Helper.GetAudioFormat(tempFormat).FileSize);
                else
                    item.FileSize = Helper.FormatFileSize(tempFormat.FileSize);

                item.OperationComplete += downloadItem_OperationComplete;

                lvQueue.Items.Add(item);

                this.SelectOneItem(item);

                if (_selectedVideo.VideoSource == VideoSource.Twitch)
                {
                    operation.Start(TwitchOperation.Args(Path.Combine(path, filename),
                        tempFormat));
                }
                else
                {
                    if (tempFormat.AudioOnly || tempFormat.FormatType == FormatType.Normal)
                        operation.Start(DownloadOperation.Args(tempFormat.DownloadUrl,
                            Path.Combine(path, filename)));
                    else
                    {
                        VideoFormat audio = Helper.GetAudioFormat(tempFormat);

                        operation.Start(DownloadOperation.Args(audio.DownloadUrl,
                            tempFormat.DownloadUrl,
                            Path.Combine(path, filename)));
                    }
                }

                tabControl1.SelectedTab = queueTabPage;
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
                string length = (string)videoThumbnail.Tag;
                Font mFont = new Font(this.Font.Name, 10.0F, FontStyle.Bold, GraphicsUnit.Point);
                SizeF mSize = e.Graphics.MeasureString(length, mFont);
                Rectangle mRec = new Rectangle((int)(videoThumbnail.Width - mSize.Width - 6),
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
            VideoFormat format = (VideoFormat)cbQuality.SelectedItem;

            if (format == null || _selectedVideo.VideoSource == VideoSource.Twitch)
            {
                lFileSize.Text = "";
            }
            else if (format.FileSize == 0)
            {
                lFileSize.Text = "Getting file size...";
            }
            else
            {
                long total = format.FileSize;

                // If the format is DASH and not a AudioOnly format, combine audio and video size.
                if (format.FormatType != FormatType.Normal && !format.AudioOnly)
                    total += Helper.GetAudioFormat(format).FileSize;

                lFileSize.Text = Helper.FormatFileSize(total);
            }
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
            e.Result = YoutubeDlHelper.GetVideoInfo((string)e.Argument);
        }

        private void bwGetVideo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VideoInfo videoInfo = e.Result as VideoInfo;

            if (videoInfo.Failure)
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

                TimeSpan videoLength = TimeSpan.FromSeconds(videoInfo.Duration);
                if (videoLength.Hours > 0)
                    videoThumbnail.Tag = string.Format("{0}:{1:00}:{2:00}", videoLength.Hours, videoLength.Minutes, videoLength.Seconds);
                else
                    videoThumbnail.Tag = string.Format("{0}:{1:00}", videoLength.Minutes, videoLength.Seconds);

                videoThumbnail.Refresh();
                videoThumbnail.ImageLocation = videoInfo.ThumbnailUrl;
            }

            btnGetVideo.Enabled = txtYoutubeLink.Enabled = true;
            cbQuality.Enabled = videoInfo.Formats.Count > 0;
            btnDownload.Enabled = true;
        }

        private void downloadItem_OperationComplete(object sender, OperationEventArgs e)
        {
            Operation operation = (sender as OperationListViewItem).Operation;

            if (chbAutoConvert.Enabled && chbAutoConvert.Checked && operation.Status == OperationStatus.Success)
            {
                string output = Path.Combine(Path.GetDirectoryName(operation.Output),
                    Path.GetFileNameWithoutExtension(operation.Output)) + ".mp3";

                this.Convert(operation.Output, output, false);
            }
        }

        #endregion

        #region Playlist Tab

        private BackgroundWorker _backgroundWorkerPlaylist;

        private void btnPlaylistBrowse_Click(object sender, EventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

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

        private void btnGetPlaylist_Click(object sender, EventArgs e)
        {
            if (btnGetPlaylist.Text == "Get Playlist")
            {
                // Reset playlist variables
                lvPlaylistVideos.Tag = null;
                lvPlaylistVideos.Items.Clear();

                btnGetPlaylist.Text = "Cancel";
                btnPlaylistDownloadAll.Enabled = false;

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
                _settings.LastPlaylistUrl = txtPlaylistLink.Text;
            }
            else if (btnGetPlaylist.Text == "Cancel")
            {
                _backgroundWorkerPlaylist.CancelAsync();
            }
        }

        private void _backgroundWorkerPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            string playlistUrl = e.Argument as string;
            PlaylistReader reader = new PlaylistReader(playlistUrl);
            VideoInfo video;

            // Set playlist list's Tag property to Playlist object
            _backgroundWorkerPlaylist.ReportProgress(1, reader.Playlist);

            while ((video = reader.Next()) != null)
            {
                if (_backgroundWorkerPlaylist.CancellationPending)
                {
                    e.Result = false;
                    break;
                }

                ListViewItem item = new ListViewItem(video.Title);
                item.SubItems.Add(Helper.FormatVideoLength(video.Duration));
                item.Checked = true;
                item.Tag = video;

                _backgroundWorkerPlaylist.ReportProgress(2, item);
            }

            e.Result = true;
        }

        private void _backgroundWorkerPlaylist_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 1:
                    lvPlaylistVideos.Tag = e.UserState as Playlist;
                    break;
                case 2:
                    ListViewItem item = e.UserState as ListViewItem;

                    lvPlaylistVideos.Items.Add(item);
                    lvPlaylistVideos.TopItem = item;
                    break;
            }
        }

        private void _backgroundWorkerPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result = (bool)e.Result;

            btnGetPlaylist.Text = "Get Playlist";
            btnPlaylistDownloadAll.Enabled = result;
            btnPlaylistDownloadSelected.Enabled = result;
            lvPlaylistVideos.UseWaitCursor = false;

            if (!result)
            {
                lvPlaylistVideos.Items.Clear();
            }
        }

        private void btnPlaylistDownloadSelected_Click(object sender, EventArgs e)
        {
            if (lvPlaylistVideos.CheckedItems.Count < 1)
                return;

            List<VideoInfo> videos = new List<VideoInfo>();

            foreach (ListViewItem item in lvPlaylistVideos.CheckedItems)
            {
                videos.Add(item.Tag as VideoInfo);
            }

            this.StartPlaylistOperation(videos);
        }

        private void btnPlaylistDownloadAll_Click(object sender, EventArgs e)
        {
            this.StartPlaylistOperation(null);
        }

        private void txtPlaylistLink_TextChanged(object sender, EventArgs e)
        {
            try
            {
                btnPlaylistDownloadAll.Enabled = Helper.IsPlaylist(txtPlaylistLink.Text);
            }
            catch (Exception)
            {
                btnPlaylistDownloadAll.Enabled = false;
            }
        }

        private void cbPlaylistSaveTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            _settings.SelectedDirectoryPlaylist = cbPlaylistSaveTo.SelectedIndex;
        }

        private void cbPlaylistQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            _settings.PreferredQualityPlaylist = cbPlaylistQuality.SelectedIndex;
        }

        private void chbPlaylistDASH_CheckedChanged(object sender, EventArgs e)
        {
            _settings.UseDashPlaylist = chbPlaylistDASH.Checked;
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

        private void StartPlaylistOperation(ICollection<VideoInfo> videos)
        {
            string path = cbPlaylistSaveTo.Text;

            // Make sure download directory exists.
            if (!this.ValidateDirectory(path))
                return;

            if (!cbPlaylistSaveTo.Items.Contains(path))
                cbPlaylistSaveTo.Items.Add(path);

            _settings.LastPlaylistUrl = txtPlaylistLink.Text;

            try
            {
                var operation = new PlaylistOperation();
                var item = new OperationListViewItem("Getting playlist info...", txtPlaylistLink.Text, operation);

                lvQueue.Items.Add(item);

                this.SelectOneItem(item);

                operation.Start(operation.Args(txtPlaylistLink.Text,
                                    path,
                                    chbPlaylistDASH.Checked,
                                    Settings.Default.PreferredQualityPlaylist,
                                    (lvPlaylistVideos.Tag as Playlist).Name,
                                    videos)
                                );

                tabControl1.SelectedTab = queueTabPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Convert Tab

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = Path.GetFileName(txtInput.Text);

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtInput.Text = openFileDialog1.FileName;

                if (txtOutput.Text == string.Empty)
                {
                    // Suggest file name
                    string output = Path.GetDirectoryName(openFileDialog1.FileName);

                    output = Path.Combine(output, Path.GetFileNameWithoutExtension(openFileDialog1.FileName));
                    output += ".mp3";

                    txtOutput.Text = output;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtInput.Text))
            {
                saveFileDialog1.FileName = Path.GetFileName(txtInput.Text);
            }
            else
            {
                saveFileDialog1.FileName = Path.GetFileName(txtOutput.Text);
            }

            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtOutput.Text = saveFileDialog1.FileName;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!FFmpegHelper.CanConvertToMP3(txtInput.Text).Value)
            {
                string text = "Can't convert input file to MP3. File doesn't appear to have audio.";

                MessageBox.Show(this, text, "Missing Audio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(txtOutput.Text))
            {
                string filename = Path.GetFileName(txtOutput.Text);
                string text = "File '" + filename + "' already exists.\n\nOverwrite?";

                if (MessageBox.Show(this, text, "Overwrite", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }

            }

            if (txtInput.Text == txtOutput.Text ||
                // If they match, the user probably wants to crop. Right?
                Path.GetExtension(txtInput.Text) == Path.GetExtension(txtOutput.Text))
            {
                this.Crop(txtInput.Text, txtOutput.Text);
            }
            else
            {
                this.Convert(txtInput.Text, txtOutput.Text, true);
            }

            txtInput.Clear();
            txtOutput.Clear();

            tabControl1.SelectedTab = queueTabPage;
        }

        private void btnCheckAgain_Click(object sender, EventArgs e)
        {
            Program.FFmpegAvailable = File.Exists(FFmpegHelper.FFmpegPath);

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
            MenuItem[] fileMenuItems = new MenuItem[]
            {
                exitMenuItem = new MenuItem("&Exit", exitMenuItem_Click, Shortcut.CtrlQ)
            };
            MenuItem[] toolsMenuItems = new MenuItem[]
            {
                optionsMenuItem = new MenuItem("&Options", optionsMenuItem_Click),
            };
            MenuItem[] helpMenuItems = new MenuItem[]
            {
                checkForUpdateMenuItem = new MenuItem("&Check for updates", checkForUpdateMenuItem_Click),
                aboutMenuItem = new MenuItem("&About", aboutMenuItem_Click)
            };

            mainMenu1 = new MainMenu();
            mainMenu1.MenuItems.Add(fileMenuItem = new MenuItem("&File", fileMenuItems));
            mainMenu1.MenuItems.Add(toolsMenuItem = new MenuItem("&Tools", toolsMenuItems));
            mainMenu1.MenuItems.Add(helpMenuItem = new MenuItem("&Help", helpMenuItems));

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

        }

        #endregion

        #region contextMenu1

        private void contextMenu1_Popup(object sender, EventArgs e)
        {
            if (lvQueue.SelectedItems.Count == 0)
            {
                foreach (MenuItem menuItem in contextMenu1.MenuItems)
                {
                    menuItem.Visible = false;
                }

                return;
            }

            bool canOpen = false, canPause = false, canResume = false, canStop = false, canConvert = false;

            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

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

            openMenuItem.Enabled = canOpen;
            pauseMenuItem.Enabled = canPause;
            resumeMenuItem.Enabled = canResume;
            stopMenuItem.Enabled = canStop;
            convertToMP3MenuItem.Enabled = canConvert;
        }

        private void contextMenu1_Collapse(object sender, EventArgs e)
        {
            foreach (MenuItem menuItem in contextMenu1.MenuItems)
            {
                menuItem.Enabled = true;
                menuItem.Visible = true;
            }
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            int fails = 0;

            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (operation.CanOpen() && !operation.Open()) fails++;
            }

            if (fails > 0)
            {
                MessageBox.Show(this, "Couldn't open " + fails + " file(s).");
            }
        }

        private void openContainingFolderMenuItem_Click(object sender, EventArgs e)
        {
            int fails = 0;

            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (!operation.OpenContainingFolder()) fails++;
            }

            if (fails > 0)
            {
                MessageBox.Show(this, "Couldn't open " + fails + " folder(s).");
            }
        }

        private void convertToMP3MenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (operation is DownloadOperation && operation.Status == OperationStatus.Success)
                {
                    string input = (operation as DownloadOperation).Output;
                    string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".mp3";

                    txtInput.Text = input;
                    txtOutput.Text = output;
                    tabControl1.SelectedTab = convertTabPage;
                    break;
                }
            }
        }

        private void resumeMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (operation.CanResume()) operation.Resume();
            }
        }

        private void pauseMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (operation.CanPause()) operation.Pause();
            }
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            foreach (OperationListViewItem item in lvQueue.SelectedItems)
            {
                Operation operation = item.Operation;

                if (operation.CanStop()) operation.Stop(true);
            }
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = lvQueue.SelectedItems.Count - 1; i >= 0; i--)
            {
                var item = (OperationListViewItem)lvQueue.SelectedItems[i];
                var operation = item.Operation;

                operation.Stop(true);

                lvQueue.Items.RemoveAt(item.Index);
            }
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
        /// Cancels all active IOperations.
        /// </summary>
        private void CancelOperations()
        {
            this.Hide();

            foreach (Operation operation in Program.RunningOperations)
            {
                // Stop & delete unfinished files
                if (operation.CanStop())
                    operation.Stop(true);
            }

            if (bwGetVideo.IsBusy)
                bwGetVideo.CancelAsync();

            while (Program.RunningOperations.Count > 0 || bwGetVideo.IsBusy)
            {
                // Wait for everything to finish
            }

            this.Close();
        }

        /// <summary>
        /// Starts a ConvertOperation.
        /// </summary>
        /// <param name="input">The file to convert.</param>
        /// <param name="output">The path to save converted file.</param>
        /// <param name="crop">True if converted file should be cropped.</param>
        private void Convert(string input, string output, bool crop)
        {
            TimeSpan start, end;
            start = end = TimeSpan.MinValue;

            if (crop && chbCropFrom.Checked)
            {
                // Validate cropping input. Shows error messages automatically.
                if (!this.ValidateCropping())
                    return;

                start = (TimeSpan)mtxtFrom.ValidateText();
                end = (TimeSpan)mtxtTo.ValidateText();
            }

            var operation = new ConvertOperation();
            var item = new OperationListViewItem(Path.GetFileName(output), input, Path.GetFileName(input), operation);

            item.WorkingText = "Converting";
            item.Duration = Helper.FormatVideoLength(FFmpegHelper.GetDuration(input).Value);
            item.FileSize = Helper.GetFileSizeFormatted(input);

            lvQueue.Items.Add(item);

            this.SelectOneItem(item);

            operation.Start(operation.Args(input, output, start, end));
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

            TimeSpan start = (TimeSpan)mtxtFrom.ValidateText();
            TimeSpan end = (TimeSpan)mtxtTo.ValidateText();

            var operation = new CroppingOperation();
            var item = new OperationListViewItem(Path.GetFileName(output), input, Path.GetFileName(input), operation);

            item.WorkingText = "Cropping";
            item.Duration = Helper.FormatVideoLength(FFmpegHelper.GetDuration(input).Value);
            item.FileSize = Helper.GetFileSizeFormatted(input);

            lvQueue.Items.Add(item);

            this.SelectOneItem(item);

            operation.Start(operation.Args(input, output, start, end));
        }

        /// <summary>
        /// Returns true if there is a working IOperation.
        /// </summary>
        private bool GetIsWorking()
        {
            foreach (OperationListViewItem item in lvQueue.Items)
            {
                Operation operation = item.Operation;

                if (operation.Status == OperationStatus.Working)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Loads & applies all application settings.
        /// </summary>
        private void LoadSettings()
        {
            // Upgrade settings between new versions. 
            // More information: http://www.ngpixel.com/2011/05/05/c-keep-user-settings-between-versions/
            if (_settings.UpdateSettings)
            {
                _settings.Upgrade();
                _settings.UpdateSettings = false;
                _settings.Save();
            }

            // Initialize WindowStates collection if null
            if (_settings.WindowStates == null)
            {
                _settings.WindowStates = new WindowStates();
            }

            // Add WindowState for form if WindowStates doesn't have a entry for it
            if (!_settings.WindowStates.Contains(this.Name))
            {
                _settings.WindowStates.Add(this.Name);
            }

            // Restore form location, size & window state, if not null
            _settings.WindowStates[this.Name].RestoreForm(this);

            // Initialize StringCollection if null
            if (_settings.SaveToDirectories == null)
            {
                _settings.SaveToDirectories = new System.Collections.Specialized.StringCollection();
            }

            // Copy StringCollection to string array
            string[] directories = new string[_settings.SaveToDirectories.Count];

            _settings.SaveToDirectories.CopyTo(directories, 0);

            // Add string array to ComboBoxes
            cbSaveTo.Items.AddRange(directories);
            cbPlaylistSaveTo.Items.AddRange(directories);

            // Restore ComboBox.SelectedIndex if it's not empty
            if (cbSaveTo.Items.Count > 0)
                cbSaveTo.SelectedIndex = _settings.SelectedDirectory;

            if (cbPlaylistSaveTo.Items.Count > 0)
                cbPlaylistSaveTo.SelectedIndex = _settings.SelectedDirectoryPlaylist;

            cbPlaylistQuality.SelectedIndex = _settings.PreferredQualityPlaylist;

            // Restore CheckBox.Checked
            chbAutoConvert.Checked = _settings.AutoConvert;
            chbPlaylistDASH.Checked = _settings.UseDashPlaylist;

            // Restore last used links
            if (_settings.LastYouTubeUrl != null) txtYoutubeLink.Text = _settings.LastYouTubeUrl;
            if (_settings.LastPlaylistUrl != null) txtPlaylistLink.Text = _settings.LastPlaylistUrl;
        }

        /// <summary>
        /// Deselects all other items in given ListViewItem's ListView except the given item.
        /// </summary>
        /// <param name="item">The ListViewItem to select.</param>
        private void SelectOneItem(ListViewItem item)
        {
            foreach (ListViewItem lvi in item.ListView.Items)
                lvi.Selected = false;

            item.Selected = true;
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

            foreach (char ch in illegalChars)
            {
                // filename contains illegal characters, ask to format title.
                if (filename.Contains(ch.ToString()))
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                string newFilename = Helper.FormatTitle(filename);
                string text = "Filename contains illegal characters, do you want to automatically remove these characters?\n\n" +
                    "New filename: \"" + newFilename + "\"\n\n" +
                    "Clicking 'No' will cancel the download.";

                if (MessageBox.Show(this, text, "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    txtTitle.Text = newFilename;
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
            List<VideoFormat> formats = new List<VideoFormat>(list);

            for (int i = formats.Count - 1; i >= 0; i--)
            {
                VideoFormat f = formats[i];

                if (f.Extension.Contains("webm") ||
                    f.FormatID.Contains("meta") ||
                    !Settings.Default.IncludeNonDASH && f.Format.ToLower().Contains("nondash") ||
                    !Settings.Default.IncludeDASH && f.Format.ToLower().Contains("dash") ||
                    !Settings.Default.IncludeNormal && !f.Format.ToLower().Contains("dash"))
                {
                    formats.RemoveAt(i);
                }
            }

            return formats.ToArray();
        }
    }
}