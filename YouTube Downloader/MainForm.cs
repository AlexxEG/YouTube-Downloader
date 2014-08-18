using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader.Classes;
using YouTube_Downloader.Operations;
using YouTube_Downloader.Properties;

/* ToDo: 
 *
 * - Clean up after combining when downloading playlist.
 * - Handle aborting operations better when closing form.
 * - Combine DASH audio & video size to get total when downloading. (Get total filesize from downloader?)
 * - Handle DASH audio & video the same way for playlists as single videos does.
 * - Move all files that gets created (logs, json files, stacktraces) to local app data (Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
 */

namespace YouTube_Downloader
{
    public partial class MainForm : Form
    {
        private string[] args;
        private VideoInfo selectedVideo;

        private delegate void UpdateFileSize(object sender, FileSizeUpdateEventArgs e);

        public MainForm()
        {
            InitializeComponent();
            InitializeMainMenu();

            lvQueue.ContextMenu = contextMenu1;
        }

        public MainForm(string[] args)
            : this()
        {
            this.args = args;
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

                e.Cancel = true;
                return;
            }

            if (selectedVideo != null)
                selectedVideo.AbortUpdateFileSizes();

            Settings.Default.WindowStates.Get(this.Name).SaveForm(this);
            Settings.Default.SaveToDirectories.Clear();

            string[] paths = new string[cbSaveTo.Items.Count];
            cbSaveTo.Items.CopyTo(paths, 0);

            Settings.Default.SaveToDirectories.AddRange(paths);
            Settings.Default.SelectedDirectory = cbSaveTo.SelectedIndex;
            Settings.Default.AutoConvert = chbAutoConvert.Checked;
            Settings.Default.Save();

            Settings.Default.Save();

            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            /* Upgrade settings between new versions. 
             * 
             * More information: http://www.ngpixel.com/2011/05/05/c-keep-user-settings-between-versions/ */
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }

            if (Settings.Default.WindowStates == null)
            {
                Settings.Default.WindowStates = new WindowStates();
            }

            if (!Settings.Default.WindowStates.Contains(this.Name))
            {
                Settings.Default.WindowStates.Add(this.Name, new WindowState(this.Name));
            }

            Settings.Default.WindowStates.Get(this.Name).RestoreForm(this);

            if (Settings.Default.SaveToDirectories == null)
            {
                Settings.Default.SaveToDirectories = new System.Collections.Specialized.StringCollection();
            }

            string[] directories = new string[Settings.Default.SaveToDirectories.Count];

            Settings.Default.SaveToDirectories.CopyTo(directories, 0);

            cbSaveTo.Items.AddRange(directories);
            cbPlaylistSaveTo.Items.AddRange(directories);

            if (cbSaveTo.Items.Count > 0)
            {
                cbSaveTo.SelectedIndex = Settings.Default.SelectedDirectory;
            }

            if (cbPlaylistSaveTo.Items.Count > 0)
            {
                cbPlaylistSaveTo.SelectedIndex = Settings.Default.SelectedDirectoryPlaylist;
            }

            chbAutoConvert.Checked = Settings.Default.AutoConvert;

            cbPlaylistQuality.SelectedIndex = Settings.Default.PreferedQualityPlaylist;
            chbPlaylistDASH.Checked = Settings.Default.UseDashPlaylist;

            if (!string.IsNullOrEmpty(Settings.Default.LastYouTubeUrl))
            {
                txtYoutubeLink.Text = Settings.Default.LastYouTubeUrl;
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (args != null)
            {
                txtYoutubeLink.Text = args[0];
                btnGetVideo.PerformClick();
                args = null;
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

                string link = (string)tag;

                Process.Start(link);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open link.");
            }
        }

        private void bwGetVideo_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = YouTubeDLHelper.GetVideoInfo((string)e.Argument);
        }

        private void bwGetVideo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            VideoInfo videoInfo = e.Result as VideoInfo;

            selectedVideo = videoInfo;

            videoInfo.FileSizeUpdated += videoInfo_FileSizeUpdated;

            cbQuality.Items.AddRange(this.CheckFormats(videoInfo.Formats));
            cbQuality.SelectedIndex = cbQuality.Items.Count - 1;

            lTitle.Text = Helper.FormatTitle(videoInfo.Title);

            TimeSpan videoLength = TimeSpan.FromSeconds(videoInfo.Duration);
            if (videoLength.Hours > 0)
                videoThumbnail.Tag = string.Format("{0}:{1:00}:{2:00}", videoLength.Hours, videoLength.Minutes, videoLength.Seconds);
            else
                videoThumbnail.Tag = string.Format("{0}:{1:00}", videoLength.Minutes, videoLength.Seconds);
            videoThumbnail.Refresh();

            btnGetVideo.Enabled = txtYoutubeLink.Enabled = true;
            cbQuality.Enabled = videoInfo.Formats.Count > 0;
            btnDownload.Enabled = true;
            videoThumbnail.ImageLocation = videoInfo.ThumbnailUrl;

            Program.RunningWorkers.Remove(bwGetVideo);
        }

        private void videoInfo_FileSizeUpdated(object sender, FileSizeUpdateEventArgs e)
        {
            /* Display the updated file size if the selected item was updated. */
            if (lFileSize.InvokeRequired)
            {
                lFileSize.Invoke(new UpdateFileSize(videoInfo_FileSizeUpdated), sender, e);
            }
            else
            {
                if (e.VideoFormat == cbQuality.SelectedItem)
                {
                    lFileSize.Text = Helper.FormatFileSize(e.VideoFormat.FileSize);
                }
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
                MessageBox.Show(this, "You entered invalid YouTube URL, Please correct it.\r\n\nNote: URL should start with:\r\nhttp://www.youtube.com/watch?",
                    "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                Settings.Default.LastYouTubeUrl = txtYoutubeLink.Text;

                lTitle.Text = "-";
                cbQuality.Items.Clear();
                btnGetVideo.Enabled = txtYoutubeLink.Enabled = btnDownload.Enabled = cbQuality.Enabled = false;
                videoThumbnail.Tag = null;

                bwGetVideo.RunWorkerAsync(txtYoutubeLink.Text);

                Program.RunningWorkers.Add(bwGetVideo);
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
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string path = cbSaveTo.Text;

            /* Make sure download directory exists. */
            if (!this.ValidateDownloadDirectory(path))
                return;

            if (!cbSaveTo.Items.Contains(path))
                cbSaveTo.Items.Add(path);

            try
            {
                VideoFormat tempFormat = cbQuality.SelectedItem as VideoFormat;
                string filename = string.Format("{0}.{1}", Helper.FormatTitle(tempFormat.VideoInfo.Title), tempFormat.Extension);

                if (File.Exists(Path.Combine(path, filename)))
                {
                    DialogResult result = MessageBox.Show(this, "File '" + filename + "' already exists\n\nOverwrite?", "Overwrite?", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        File.Delete(Path.Combine(path, filename));
                    }
                    else if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                DownloadOperation item = new DownloadOperation(Path.GetFileName(filename));

                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add(Helper.FormatVideoLength(tempFormat.VideoInfo.Duration));
                item.SubItems.Add(Helper.FormatFileSize(tempFormat.FileSize));
                item.SubItems.Add(tempFormat.VideoInfo.Url);
                item.OperationComplete += downloadItem_OperationComplete;

                lvQueue.Items.Add(item);

                SelectOneItem(item);

                ProgressBar pb = new ProgressBar()
                {
                    Maximum = 100,
                    Minimum = 0,
                    Value = 0
                };
                lvQueue.AddEmbeddedControl(pb, 1, item.Index);

                LinkLabel ll = new LinkLabel()
                {
                    Text = tempFormat.VideoInfo.Url,
                    Tag = tempFormat.VideoInfo.Url
                };
                ll.LinkClicked += linkLabel_LinkClicked;

                lvQueue.AddEmbeddedControl(ll, 5, item.Index);

                if (!tempFormat.DASH)
                    item.Download(tempFormat.DownloadUrl, Path.Combine(path, filename));
                else
                {
                    VideoFormat audio = Helper.GetAudioFormat(selectedVideo);

                    item.DownloadDASH(audio.DownloadUrl, tempFormat.DownloadUrl, Path.Combine(path, filename));
                }

                tabControl1.SelectedTab = queueTabPage;
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            /* Display file size. */
            VideoFormat format = (VideoFormat)cbQuality.SelectedItem;

            if (format == null)
            {
                lFileSize.Text = "";
            }
            else if (format.FileSize == 0)
            {
                lFileSize.Text = "Getting file size...";
            }
            else
            {
                lFileSize.Text = Helper.FormatFileSize((cbQuality.SelectedItem as VideoFormat).FileSize);
            }
        }

        private void downloadItem_OperationComplete(object sender, OperationEventArgs e)
        {
            IOperation operation = (IOperation)e.Item;

            if (chbAutoConvert.Enabled && chbAutoConvert.Checked && operation.Status == OperationStatus.Success)
            {
                string output = Path.Combine(Path.GetDirectoryName(operation.Output),
                    Path.GetFileNameWithoutExtension(operation.Output)) + ".mp3";

                this.Convert(operation.Output, output, false);
            }
        }

        #endregion

        #region Playlist Tab

        private void btnPlaylistBrowse_Click(object sender, EventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            if (Directory.Exists(cbPlaylistSaveTo.Text))
                ofd.InitialFolder = cbPlaylistSaveTo.Text;
            else
                ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                cbPlaylistSaveTo.SelectedIndex = cbPlaylistSaveTo.Items.Add(ofd.Folder);
            }
        }

        private void btnPlaylistDownload_Click(object sender, EventArgs e)
        {
            string path = string.Empty;

            try
            {
                path = cbPlaylistSaveTo.Text;

                if (path == string.Empty)
                {
                    MessageBox.Show(this, "Download path is empty.");
                    return;
                }

                if (!Directory.Exists(path))
                {
                    if (MessageBox.Show(this, "Download path doesn't exists.\n\nDo you want to create it?", "Missing Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        return;
                    }
                }

                if (!cbPlaylistSaveTo.Items.Contains(path))
                    cbPlaylistSaveTo.Items.Add(path);
            }
            catch
            {
                MessageBox.Show("Couldn't create directory.");
                return;
            }

            try
            {
                PlaylistOperation item = new PlaylistOperation();

                // item.OperationComplete += downloadItem_OperationComplete;

                lvQueue.Items.Add(item);

                SelectOneItem(item);

                ProgressBar pb = new ProgressBar()
                {
                    Maximum = 100,
                    Minimum = 0,
                    Value = 0
                };
                lvQueue.AddEmbeddedControl(pb, 1, item.Index);

                LinkLabel ll = new LinkLabel()
                {
                    Text = txtPlaylistLink.Text,
                    Tag = txtPlaylistLink.Text
                };
                ll.LinkClicked += linkLabel_LinkClicked;
                lvQueue.AddEmbeddedControl(ll, 5, item.Index);

                item.Download(txtPlaylistLink.Text, path, chbPlaylistDASH.Checked);

                tabControl1.SelectedTab = queueTabPage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtPlaylistLink_TextChanged(object sender, EventArgs e)
        {
            try
            {
                btnPlaylistDownload.Enabled = Helper.IsPlaylist(txtPlaylistLink.Text);
            }
            catch (Exception)
            {
                btnPlaylistDownload.Enabled = false;
            }
        }

        private void cbPlaylistSaveTo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.SelectedDirectoryPlaylist = cbPlaylistSaveTo.SelectedIndex;
        }

        private void cbPlaylistQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.PreferedQualityPlaylist = cbPlaylistQuality.SelectedIndex;
        }

        private void chbPlaylistDASH_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.UseDashPlaylist = chbPlaylistDASH.Checked;
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
            if (!FFmpegHelper.CanConvertMP3(txtInput.Text))
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

        private void InitializeMainMenu()
        {
            MenuItem[] fileMenuItems = new MenuItem[]
            {
                exitMenuItem = new MenuItem("&Exit", exitMenuItem_Click, Shortcut.CtrlQ)
            };

            fileMenuItem = new MenuItem("&File");
            fileMenuItem.MenuItems.AddRange(fileMenuItems);

            MenuItem[] toolsMenuItems = new MenuItem[]
            {
                optionsMenuItem = new MenuItem("&Options", optionsMenuItem_Click),
            };

            toolsMenuItem = new MenuItem("&Tools");
            toolsMenuItem.MenuItems.AddRange(toolsMenuItems);

            mainMenu1 = new MainMenu();
            mainMenu1.MenuItems.Add(fileMenuItem);
            mainMenu1.MenuItems.Add(toolsMenuItem);

            this.Menu = mainMenu1;
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void optionsMenuItem_Click(object sender, EventArgs e)
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

            bool hasDownloadItem = false;

            // Check if SelectedItems contains a DownloadListViewItem,
            // which means Converting, Pause & Resume should be available.
            foreach (var item in lvQueue.SelectedItems)
            {
                if (item is DownloadOperation)
                {
                    hasDownloadItem = true;
                    break;
                }
            }

            if (!hasDownloadItem)
            {
                convertToMP3MenuItem.Enabled = false;
                resumeMenuItem.Visible = pauseMenuItem.Visible = false;
            }
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

            foreach (IOperation operation in lvQueue.SelectedItems)
            {
                try
                {
                    Process.Start(operation.Output);
                }
                catch
                {
                    fails++;
                }
            }

            if (fails > 0)
            {
                MessageBox.Show(this, "Couldn't open " + fails + " file(s).");
            }
        }

        private void openContainingFolderMenuItem_Click(object sender, EventArgs e)
        {
            int fails = 0;

            foreach (IOperation operation in lvQueue.SelectedItems)
            {
                try
                {
                    Process.Start(Path.GetDirectoryName(operation.Output));
                }
                catch
                {
                    fails++;
                }
            }

            if (fails > 0)
            {
                MessageBox.Show(this, "Couldn't open " + fails + " folder(s).");
            }
        }

        private void convertToMP3MenuItem_Click(object sender, EventArgs e)
        {
            foreach (var item in lvQueue.SelectedItems)
            {
                if (item is DownloadOperation)
                {
                    string input = (item as DownloadOperation).Output;
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
            foreach (ListViewItem item in lvQueue.SelectedItems)
            {
                if (item is DownloadOperation)
                {
                    (item as DownloadOperation).Resume();
                }
            }
        }

        private void pauseMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvQueue.SelectedItems)
            {
                if (item is DownloadOperation)
                {
                    (item as DownloadOperation).Pause();
                }
            }
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            List<string> files = new List<string>();

            foreach (IOperation operation in lvQueue.SelectedItems)
            {
                if (operation.Status != OperationStatus.Success)
                {
                    operation.Stop();

                    if (File.Exists(operation.Output))
                    {
                        files.Add(operation.Output);
                    }
                }
            }

            if (files.Count > 0)
            {
                Helper.DeleteFiles(files.ToArray());
            }
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            stopMenuItem.PerformClick();

            while (lvQueue.SelectedItems.Count > 0)
            {
                lvQueue.SelectedItems[0].Remove();
            }
        }

        #endregion

        public void InsertVideo(string url)
        {
            txtYoutubeLink.Text = url;
            btnGetVideo.PerformClick();
        }

        private void CancelOperations()
        {
            this.Hide();

            // Store files & attempts in Dictionary.
            Dictionary<string, int> files = new Dictionary<string, int>();

            foreach (ListViewItem item in lvQueue.Items)
            {
                IOperation operation = (IOperation)item;

                if (operation.Stop())
                {
                    if (!(operation.Status == OperationStatus.Success))
                    {
                        if (!files.ContainsKey(operation.Output))
                        {
                            files.Add(operation.Output, 0);
                        }
                    }
                }
            }

            bool done = false;

            while (!done)
            {
                // If there are no files left & all BackgroundWorkers are done,
                // then it's safe to exit the application.
                if (files.Count < 1 && Program.RunningWorkers.Count < 1)
                {
                    done = true;
                }

                string[] keys = new string[files.Count];

                files.Keys.CopyTo(keys, 0);

                foreach (string key in keys)
                {
                    int attempts = files[key];

                    if (attempts < 10)
                    {
                        try
                        {
                            File.Delete(key);

                            files.Remove(key);
                        }
                        catch
                        {
                            files[key]++;
                        }
                    }
                }

                Application.DoEvents();
            }

            this.Close();
        }

        private void Convert(string input, string output, bool crop)
        {
            string start = string.Empty;
            string end = string.Empty;

            if (crop && chbCropFrom.Checked)
            {
                try
                {
                    // Call 'ValidateText().ToString()' because it will throw a
                    // exception if there is a error in the text.
                    mtxtFrom.ValidateText().ToString();
                    start = mtxtFrom.Text;
                    if (chbCropTo.Enabled && chbCropTo.Checked)
                    {
                        mtxtTo.ValidateText().ToString();
                        end = mtxtTo.Text;
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Cropping information error.");
                    return;
                }
            }

            var item = new ConvertOperation(Path.GetFileName(output));

            item.SubItems.Add("");
            item.SubItems.Add("Converting");
            item.SubItems.Add(Helper.FormatVideoLength(FFmpegHelper.GetDuration(input)));
            item.SubItems.Add(Helper.GetFileSize(input));
            item.SubItems.Add("");

            lvQueue.Items.Add(item);

            SelectOneItem(item);

            ProgressBar pb = new ProgressBar()
            {
                Maximum = 100,
                Minimum = 0,
                Value = 0
            };
            lvQueue.AddEmbeddedControl(pb, 1, item.Index);

            LinkLabel ll = new LinkLabel()
            {
                Text = Path.GetFileName(input),
                Tag = input
            };
            ll.LinkClicked += linkLabel_LinkClicked;
            lvQueue.AddEmbeddedControl(ll, 5, item.Index);

            item.Convert(input, output, start, end);
        }

        private void Crop(string input, string output)
        {
            string start = string.Empty;
            string end = string.Empty;

            try
            {
                // Call 'ValidateText().ToString()' because it will throw a
                // exception if there is a error in the text.
                mtxtFrom.ValidateText().ToString();
                start = mtxtFrom.Text;
                if (chbCropTo.Enabled && chbCropTo.Checked)
                {
                    mtxtTo.ValidateText().ToString();
                    end = mtxtTo.Text;
                }
            }
            catch
            {
                MessageBox.Show(this, "Cropping information error.");
                return;
            }

            CroppingOperation item = new CroppingOperation(Path.GetFileName(output));

            item.SubItems.Add("");
            item.SubItems.Add("Cropping");
            item.SubItems.Add(Helper.FormatVideoLength(FFmpegHelper.GetDuration(input)));
            item.SubItems.Add(Helper.GetFileSize(input));
            item.SubItems.Add("");

            lvQueue.Items.Add(item);

            SelectOneItem(item);

            ProgressBar pb = new ProgressBar()
            {
                Maximum = 100,
                Minimum = 0,
                Value = 0
            };
            lvQueue.AddEmbeddedControl(pb, 1, item.Index);

            LinkLabel ll = new LinkLabel()
            {
                Text = Path.GetFileName(input),
                Tag = input
            };
            ll.LinkClicked += linkLabel_LinkClicked;
            lvQueue.AddEmbeddedControl(ll, 5, item.Index);

            item.Crop(input, output, start, end);
        }

        private bool IsWorking()
        {
            foreach (ListViewItem item in lvQueue.Items)
            {
                IOperation operation = (IOperation)item;

                if (operation.Status == OperationStatus.Working)
                {
                    return true;
                }
            }
            return false;
        }

        private void SelectOneItem(ListViewItem item)
        {
            foreach (ListViewItem lvi in lvQueue.Items)
                lvi.Selected = false;

            item.Selected = true;
        }

        private bool ValidateDownloadDirectory(string directory)
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
        /// Removes unsupported &amp; unnecessary formats.
        /// </summary>
        /// <param name="list">The list of VideoFormat to check.</param>
        private VideoFormat[] CheckFormats(IList<VideoFormat> list)
        {
            List<VideoFormat> formats = new List<VideoFormat>(list);

            for (int i = formats.Count - 1; i >= 0; i--)
            {
                VideoFormat f = formats[i];

                if (f.Extension.Contains("webm"))
                    formats.RemoveAt(i);
                else if (f.Format.Contains("audio only (DASH audio)"))
                    formats.RemoveAt(i);
            }

            return formats.ToArray();
        }
    }

    public enum OperationStatus { Canceled, Failed, None, Paused, Success, Working }

    public interface IOperation
    {
        string Input { get; set; }
        string Output { get; set; }
        OperationStatus Status { get; set; }

        event OperationEventHandler OperationComplete;

        bool Stop();
    }

    public class OperationEventArgs : EventArgs
    {
        public ListViewItem Item { get; set; }
        public OperationStatus Status { get; set; }

        public OperationEventArgs(ListViewItem item, OperationStatus status)
        {
            this.Item = item;
            this.Status = status;
        }
    }

    public delegate void OperationEventHandler(object sender, OperationEventArgs e);
}