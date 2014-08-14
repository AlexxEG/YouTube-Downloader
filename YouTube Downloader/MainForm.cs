using DeDauwJeroen;
using ListViewEmbeddedControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

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

            SettingsEx.Load();
        }

        public MainForm(string[] args)
            : this()
        {
            this.args = args;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsWorking())
            {
                string text = "Files are being downloaded/converted/cut.\n\nAre you sure you want to quit?";

                if (MessageBox.Show(this, text, "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Hide form while waiting for threads to finish,
                    // except downloads which will abort.
                    CancelOperations();
                }

                e.Cancel = true;
                return;
            }

            if (selectedVideo != null)
                selectedVideo.AbortUpdateFileSizes();

            SettingsEx.WindowStates[this.Name].SaveForm(this);
            SettingsEx.SaveToDirectories.Clear();

            string[] paths = new string[cbSaveTo.Items.Count];
            cbSaveTo.Items.CopyTo(paths, 0);

            SettingsEx.SaveToDirectories.AddRange(paths);
            SettingsEx.SelectedDirectory = cbSaveTo.SelectedIndex;
            SettingsEx.AutoConvert = chbAutoConvert.Checked;
            SettingsEx.Save();

            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!SettingsEx.WindowStates.ContainsKey(this.Name))
            {
                SettingsEx.WindowStates.Add(this.Name, new WindowState(this.Name));
            }

            SettingsEx.WindowStates[this.Name].RestoreForm(this);

            cbSaveTo.Items.AddRange(SettingsEx.SaveToDirectories.ToArray());
            cbPlaylistSaveTo.Items.AddRange(SettingsEx.SaveToDirectories.ToArray());

            if (cbSaveTo.Items.Count > 0)
            {
                cbSaveTo.SelectedIndex = SettingsEx.SelectedDirectory;
            }

            if (cbPlaylistSaveTo.Items.Count > 0)
            {
                cbPlaylistSaveTo.SelectedIndex = SettingsEx.SelectedDirectoryPlaylist;
            }

            chbAutoConvert.Checked = SettingsEx.AutoConvert;

            cbPlaylistQuality.SelectedIndex = SettingsEx.PreferedQualityPlaylist;
            chbPlaylistDASH.Checked = SettingsEx.UseDashPlaylist;
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Draw line on top of panel.
            e.Graphics.DrawLine(new Pen(Color.Silver, 2), new Point(0, 1), new Point(panel1.Width, 1));
        }

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

        private void btnBrowseDashVideo_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = Path.GetFileName(txtDashVideo.Text);

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtDashVideo.Text = openFileDialog1.FileName;
            }
        }

        private void btnBrowseDashAudio_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = Path.GetFileName(txtDashAudio.Text);

            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtDashAudio.Text = openFileDialog1.FileName;
            }
        }

        private void btnDashBrowseOutput_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtDashOutput.Text = saveFileDialog1.FileName;
            }
        }

        private void btnDashCombine_Click(object sender, EventArgs e)
        {
            if (txtDashAudio.Text.ToLower() == txtDashVideo.Text.ToLower())
            {
                string text = "Audio & video is the same file.";

                MessageBox.Show(this, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtDashAudio.Text) || !File.Exists(txtDashVideo.Text))
            {
                string text = "One or more files doesn't exist anymore.";

                MessageBox.Show(this, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var errors = FFmpegHelper.CheckCombine(txtDashAudio.Text, txtDashVideo.Text);

            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("There are some errors, though they may be ignored depending on the severity.");
                sb.AppendLine();

                foreach (string error in errors)
                {
                    sb.AppendFormat(" - {0}" + Environment.NewLine, error);
                }

                sb.AppendLine();
                sb.Append("Do you want to continue?");

                if (MessageBox.Show(this, sb.ToString(), "", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            if (File.Exists(txtDashOutput.Text))
            {
                string filename = Path.GetFileName(txtDashOutput.Text);
                string text = "File '" + filename + "' already exists.\n\nOverwrite?";

                if (MessageBox.Show(this, text, "Overwrite", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }

            FFmpegHelper.CombineDash(txtDashVideo.Text, txtDashAudio.Text, txtDashOutput.Text);

            txtDashVideo.Clear();
            txtDashAudio.Clear();
            txtDashOutput.Clear();

            MessageBox.Show(this, "Combined sucessfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            foreach (VideoFormat format in videoInfo.Formats)
            {
                if (format.Extension.Contains("webm"))
                    continue;

                cbQuality.Items.Add(format);
            }

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
            string path = string.Empty;

            try
            {
                path = cbSaveTo.Text;

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

                if (!cbSaveTo.Items.Contains(path))
                    cbSaveTo.Items.Add(path);
            }
            catch
            {
                MessageBox.Show("Couldn't create directory.");
                return;
            }

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
                item.SubItems.Add(string.Format(new FileSizeFormatProvider(), "{0:fs}", tempFormat.FileSize));
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

                item.Download(tempFormat.DownloadUrl, Path.Combine(path, filename));

                tabControl1.SelectedTab = queueTabPage;
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            SettingsEx.SelectedDirectoryPlaylist = cbPlaylistSaveTo.SelectedIndex;
        }

        private void cbPlaylistQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            SettingsEx.PreferedQualityPlaylist = cbPlaylistQuality.SelectedIndex;
        }

        private void chbPlaylistDASH_CheckedChanged(object sender, EventArgs e)
        {
            SettingsEx.UseDashPlaylist = chbPlaylistDASH.Checked;
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
                DeleteFiles(files.ToArray());
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
            item.SubItems.Add(GetFileSize(input));
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
            item.SubItems.Add(GetFileSize(input));
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

        public void InsertVideo(string url)
        {
            txtYoutubeLink.Text = url;
            btnGetVideo.PerformClick();
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

        public static void DeleteFiles(params string[] files)
        {
            new Thread(delegate()
            {
                var dict = new Dictionary<string, int>();
                var keys = new List<string>();

                foreach (string file in files)
                {
                    dict.Add(file, 0);
                    keys.Add(file);
                }

                while (dict.Count > 0)
                {
                    foreach (string key in keys)
                    {
                        try
                        {
                            File.Delete(key);

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

        public static string GetFileSize(string file)
        {
            FileInfo info = new FileInfo(file);

            return string.Format(new FileSizeFormatProvider(), "{0:fs}", info.Length);
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

    public class ConvertOperation : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status { get; set; }

        public event OperationEventHandler OperationComplete;

        public ConvertOperation(string text)
            : base(text)
        {
        }

        public void Convert(string input, string output, string start, string end)
        {
            this.Input = input;
            this.Output = output;
            this.converterStart = start;
            this.converterEnd = end;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();

            Program.RunningWorkers.Add(backgroundWorker);

            this.Status = OperationStatus.Working;
        }

        public bool Stop()
        {
            try
            {
                if (process != null && !process.HasExited)
                    process.StandardInput.WriteLine("\x71");

                this.Status = OperationStatus.Canceled;
                this.SubItems[2].Text = "Stopped";

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region backgroundWorker

        private BackgroundWorker backgroundWorker;
        private string converterStart = string.Empty;
        private string converterEnd = string.Empty;
        private Process process;

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            FFmpegHelper.Convert(backgroundWorker, this.Input, this.Output);

            if (!string.IsNullOrEmpty(converterStart))
            {
                if (string.IsNullOrEmpty(converterEnd))
                {
                    FFmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart);
                }
                else
                {
                    FFmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart, converterEnd);
                }
            }

            this.converterStart = this.converterEnd = string.Empty;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar pb = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);

            pb.Value = e.ProgressPercentage;

            if (e.UserState is Process)
            {
                this.process = (Process)e.UserState;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SubItems[2].Text = "Success";
            this.SubItems[4].Text = MainForm.GetFileSize(this.Output);

            this.Status = OperationStatus.Success;

            Program.RunningWorkers.Remove(backgroundWorker);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }

    public class CroppingOperation : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status { get; set; }

        public event OperationEventHandler OperationComplete;

        public CroppingOperation(string text)
            : base(text)
        {
        }

        public void Crop(string input, string output, string start, string end)
        {
            this.Input = input;
            this.Output = output;
            this.cropStart = start;
            this.cropEnd = end;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();

            Program.RunningWorkers.Add(backgroundWorker);

            this.Status = OperationStatus.Working;
        }

        public bool Stop()
        {
            try
            {
                if (process != null && !process.HasExited)
                    process.StandardInput.WriteLine("\x71");

                this.Status = OperationStatus.Canceled;
                this.SubItems[2].Text = "Stopped";

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region backgroundWorker

        private BackgroundWorker backgroundWorker;
        private string cropStart = string.Empty;
        private string cropEnd = string.Empty;
        private Process process;

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (cropEnd == string.Empty)
                FFmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart);
            else
                FFmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart, cropEnd);

            cropStart = cropEnd = string.Empty;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar pb = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);

            pb.Value = e.ProgressPercentage;

            if (e.UserState is Process)
            {
                this.process = (Process)e.UserState;
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.SubItems[2].Text = "Success";
            this.SubItems[3].Text = Helper.FormatVideoLength(FFmpegHelper.GetDuration(this.Input));
            this.SubItems[4].Text = MainForm.GetFileSize(this.Output);

            this.Status = OperationStatus.Success;

            Program.RunningWorkers.Remove(backgroundWorker);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }

    public class DownloadOperation : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status
        {
            get
            {
                if (downloader == null)
                    return OperationStatus.None;

                if (downloader.HasBeenCanceled) /* Canceled */
                {
                    return OperationStatus.Canceled;
                }
                else if (successful)
                {
                    return OperationStatus.Success;
                }
                else if (failed)
                {
                    return OperationStatus.Failed;
                }
                else if (downloader.IsPaused)
                {
                    return OperationStatus.Paused;
                }
                else if (!downloader.IsPaused) /* Downloading */
                {
                    return OperationStatus.Working;
                }
                else
                {
                    return OperationStatus.None;
                }
            }
            set
            {

            }
        }

        public event OperationEventHandler OperationComplete;
        private FileDownloader downloader;

        /* downloader statuses */
        private bool failed = false;
        private bool successful = false;

        public DownloadOperation(string text)
            : base(text)
        {
        }

        public void Download(string url, string output)
        {
            this.Input = url;
            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();
            this.Output = Path.Combine(folder, file);

            /* Reset some variables in-case downloader is restarted. */
            failed = successful = false;

            downloader = new FileDownloader(true);
            downloader.LocalDirectory = folder;

            FileDownloader.FileInfo fileInfo = new FileDownloader.FileInfo(url);

            /* Give proper filename to downloaded file. */
            fileInfo.Name = file;

            downloader.Files.Add(fileInfo);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.Completed += downloader_Completed;
            downloader.FileDownloadFailed += delegate { failed = true; };
            downloader.FileDownloadSucceeded += delegate { successful = true; };
            downloader.Start();

            Program.RunningDownloaders.Add(downloader);
        }

        public void Pause()
        {
            downloader.Pause();
        }

        public void Resume()
        {
            downloader.Resume();
        }

        public bool Stop()
        {
            try
            {
                downloader.Stop(false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region downloader

        private bool processing;

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (ListView.InvokeRequired)
                ListView.Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                    string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)longETA) * 1000) + " ]";

                    this.SubItems[1].Text = downloader.TotalPercentage() + " %";
                    this.SubItems[2].Text = speed + ETA;

                    ProgressBar progressBar = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);

                    progressBar.Value = (int)downloader.TotalPercentage();

                    RefreshStatus();
                }
                catch { }
                finally { processing = false; }
            }
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            RefreshStatus();

            Program.RunningDownloaders.Remove(downloader);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

        private void RefreshStatus()
        {
            if (successful)
            {
                this.SubItems[2].Text = "Completed";
            }
            else if (downloader.IsPaused)
            {
                this.SubItems[2].Text = "Paused";
            }
            else if (downloader.HasBeenCanceled)
            {
                this.SubItems[2].Text = "Canceled";
            }
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }
    }

    public class PlaylistOperation : ListViewItem, IOperation
    {
        /* ToDo:
         * 
         * - Show combining operation in status so that multiple instances doesn't access log file
         * - Use the combining time to get content length since it takes time.
         */

        private const int Reset_Controls = 1;

        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status
        {
            get
            {
                if (downloader == null)
                    return OperationStatus.None;

                if (downloader.HasBeenCanceled) /* Canceled */
                {
                    return OperationStatus.Canceled;
                }
                else if (successful)
                {
                    return OperationStatus.Success;
                }
                else if (failed)
                {
                    return OperationStatus.Failed;
                }
                else if (downloader.IsPaused)
                {
                    return OperationStatus.Paused;
                }
                else if (!downloader.IsPaused) /* Downloading */
                {
                    return OperationStatus.Working;
                }
                else
                {
                    return OperationStatus.None;
                }
            }
            set
            {

            }
        }

        public event OperationEventHandler OperationComplete;
        private delegate void SetTextDelegate(string text);
        private delegate void SetItemTextDelegate(ListViewSubItem item, string text);

        private FileDownloader downloader;

        private BackgroundWorker worker;
        private bool processing;

        /* downloader statuses */
        private bool failed = false;
        private bool successful = false;

        private bool useDash = false;

        public PlaylistOperation()
        {
            this.Text = "Getting playlist info...";
            /* Fill sub items. */
            this.SubItems.AddRange(new string[] { "", "", "", "", "" });
        }

        public void Download(string url, string output, bool dash)
        {
            this.Input = url;
            this.Output = output;
            this.SubItems[5].Text = this.Input;

            useDash = dash;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            Program.RunningWorkers.Add(worker);
        }

        public void Pause()
        {
            downloader.Pause();
        }

        public void Resume()
        {
            downloader.Resume();
        }

        public bool Stop()
        {
            try
            {
                downloader.Stop(false);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private ProgressBar GetProgressBar()
        {
            return (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);
        }

        private void OnOperationComplete(OperationEventArgs e)
        {
            if (OperationComplete != null)
                OperationComplete(this, e);
        }

        private void RefreshStatus()
        {
            if (successful)
            {
                this.SubItems[2].Text = "Completed";
            }
            else if (downloader.IsPaused)
            {
                this.SubItems[2].Text = "Paused";
            }
            else if (downloader.HasBeenCanceled)
            {
                this.SubItems[2].Text = "Canceled";
            }
        }

        private void SetText(string text)
        {
            if (this.ListView.InvokeRequired)
            {
                this.ListView.Invoke(new SetTextDelegate(SetText), text);
            }
            else
            {
                this.Text = text;
            }
        }

        private void SetItemText(ListViewSubItem item, string text)
        {
            if (this.ListView.InvokeRequired)
            {
                this.ListView.Invoke(new SetItemTextDelegate(SetItemText), item, text);
            }
            else
            {
                item.Text = text;
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            int count = 0;
            PlaylistReader reader = new PlaylistReader(this.Input);
            VideoInfo video;

            while ((video = reader.Next()) != null)
            {
                count++;

                VideoFormat videoFormat = Helper.GetPreferedFormat(video, useDash);

                this.SetText(string.Format("({0}/{1}) {2}", count, reader.Playlist.OnlineCount, video.Title));
                this.SetItemText(this.SubItems[3], Helper.FormatVideoLength(video.Duration));
                this.SetItemText(this.SubItems[4], Helper.FormatFileSize(videoFormat.FileSize));

                downloader = new FileDownloader(true);
                downloader.LocalDirectory = this.Output;

                FileDownloader.FileInfo[] fileInfos;

                string finalFile = Path.Combine(this.Output, Helper.FormatTitle(videoFormat.VideoInfo.Title) + "." + videoFormat.Extension);

                if (!useDash)
                {
                    fileInfos = new FileDownloader.FileInfo[1]
                    {
                        new FileDownloader.FileInfo(videoFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(finalFile)
                        }
                    };
                }
                else
                {
                    VideoFormat audioFormat = Helper.GetAudioFormat(video);
                    /* Add '_audio' & '_video' to end of filename. */
                    string audioFile = Path.Combine(this.Output, Path.GetFileNameWithoutExtension(finalFile)) + "_audio.m4a";
                    string videoFile = Path.Combine(this.Output, Path.GetFileNameWithoutExtension(finalFile)) + "_video." + videoFormat.Extension;

                    fileInfos = new FileDownloader.FileInfo[2]
                    {
                        new FileDownloader.FileInfo(videoFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(videoFile)
                        },
                        new FileDownloader.FileInfo(audioFormat.DownloadUrl)
                        {
                            Name = Path.GetFileName(audioFile)
                        }
                    };
                }

                downloader.Files.AddRange(fileInfos);

                /* Attach events. */
                downloader.Completed += downloader_Completed;
                downloader.FileDownloadFailed += downloader_FileDownloadFailed;
                downloader.ProgressChanged += downloader_ProgressChanged;

                downloader.Start();

                Program.RunningDownloaders.Add(downloader);

                /* If downloader is busy or paused, wait till it's done. */
                while (downloader.IsBusy || downloader.IsPaused)
                    Thread.Sleep(200);

                worker.ReportProgress(Reset_Controls);
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case Reset_Controls:
                    this.GetProgressBar().Value = 0;
                    break;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!failed)
                successful = true;

            RefreshStatus();

            Program.RunningWorkers.Remove(worker);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        private void downloader_Completed(object sender, EventArgs e)
        {
            if (failed)
            {
                successful = false;
            }
            else if (useDash)
            {
                /* Queue DASH combine on a new thread so next download can start. */
                string audioFile = Path.Combine(downloader.LocalDirectory, downloader.Files[0].Name);
                string videoFile = Path.Combine(downloader.LocalDirectory, downloader.Files[1].Name);
                string finalFile = videoFile.Replace("_video", string.Empty);

                FFmpegHelper.CombineDashThread(videoFile, audioFile, finalFile);
            }

            RefreshStatus();

            Program.RunningDownloaders.Remove(downloader);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        private void downloader_FileDownloadFailed(object sender, Exception ex)
        {
            /* If one or more files fail, whole operation failed. Might handle it more
             * elegantly in the future. */
            failed = true;
            downloader.Stop(false);
        }

        private void downloader_ProgressChanged(object sender, EventArgs e)
        {
            if (processing)
                return;

            if (downloader != sender)
                return;

            if (ListView.InvokeRequired)
                ListView.Invoke(new EventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;

                    string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    long longETA = Helper.GetETA(downloader.DownloadSpeed, downloader.TotalSize, downloader.TotalProgress);
                    string ETA = longETA == 0 ? "" : "  [ " + FormatLeftTime.Format((longETA) * 1000) + " ]";

                    this.SubItems[1].Text = downloader.TotalPercentage() + " %";
                    this.SubItems[2].Text = speed + ETA;

                    this.GetProgressBar().Value = (int)downloader.TotalPercentage();

                    RefreshStatus();
                }
                catch { }
                finally
                {
                    processing = false;
                }
            }
        }
    }
}