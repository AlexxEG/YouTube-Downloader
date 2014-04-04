using ListViewEmbeddedControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using YouTube_Downloader.Classes;

namespace YouTube_Downloader
{
    public partial class MainForm : Form
    {
        private string[] args;

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

            SettingsEx.WindowStates[this.Name].SaveForm(this);

            SettingsEx.SaveToDirectories.Clear();

            string[] paths = new string[cbSaveTo.Items.Count];
            cbSaveTo.Items.CopyTo(paths, 0);

            SettingsEx.SaveToDirectories.AddRange(paths);
            SettingsEx.SelectedDirectory = cbSaveTo.SelectedIndex;

            SettingsEx.AutoConvert = chbAutoConvert.Checked;

            SettingsEx.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!SettingsEx.WindowStates.ContainsKey(this.Name))
            {
                SettingsEx.WindowStates.Add(this.Name, new WindowState(this.Name));
            }

            SettingsEx.WindowStates[this.Name].RestoreForm(this);

            cbSaveTo.Items.AddRange(SettingsEx.SaveToDirectories.ToArray());
            cbSaveTo.SelectedIndex = SettingsEx.SelectedDirectory;

            chbAutoConvert.Checked = SettingsEx.AutoConvert;
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
                cbQuality.DataSource = null;
                btnGetVideo.Enabled = txtYoutubeLink.Enabled = btnDownload.Enabled = cbQuality.Enabled = false;
                videoThumbnail.Tag = null;
                videoThumbnail.ImageLocation = string.Format("http://i3.ytimg.com/vi/{0}/default.jpg", Helper.GetVideoIDFromUrl(txtYoutubeLink.Text));

                bwGetVideo.RunWorkerAsync(txtYoutubeLink.Text);

                Program.RunningWorkers.Add(bwGetVideo);
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
                    if (MessageBox.Show(this, "Download path doesn't exists.\n\nDo you want to create it?") == DialogResult.Yes)
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
                YouTubeVideoQuality tempItem = cbQuality.SelectedItem as YouTubeVideoQuality;
                string filename = string.Format("{0}.{1}", FormatTitle(tempItem.VideoTitle), tempItem.Extension);

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

                DownloadListViewItem item = new DownloadListViewItem(Path.GetFileName(filename));

                item.SubItems.Add("");
                item.SubItems.Add("");
                item.SubItems.Add(FormatVideoLength(tempItem.Length));
                item.SubItems.Add(string.Format(new FileSizeFormatProvider(), "{0:fs}", tempItem.VideoSize));
                item.SubItems.Add(tempItem.VideoUrl);
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
                    Text = tempItem.VideoUrl,
                    Tag = tempItem.VideoUrl
                };
                ll.LinkClicked += linkLabel_LinkClicked;
                lvQueue.AddEmbeddedControl(ll, 5, item.Index);

                item.Download(tempItem.DownloadUrl, Path.Combine(path, filename));

                tabControl1.SelectedTab = queueTabPage;
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
            if (File.Exists(txtOutput.Text))
            {
                var filename = Path.GetFileName(txtOutput.Text);
                var result = MessageBox.Show(this, "File '" + filename + "' already exists.\n\nOverwrite?",
                    "Overwrite", MessageBoxButtons.YesNo);

                if (result == DialogResult.No)
                    return;
            }

            if (txtInput.Text == txtOutput.Text ||
                Path.GetExtension(txtInput.Text) == Path.GetExtension(txtOutput.Text)) // If they match, the user probably wants to crop. Right?
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
            Program.FFmpegAvailable = File.Exists(Path.Combine(Application.StartupPath, "ffmpeg.exe"));

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

        private void bwGetVideo_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = YouTubeDownloader.GetYouTubeVideoUrls(e.Argument + "");
        }

        private void bwGetVideo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<YouTubeVideoQuality> urls = e.Result as List<YouTubeVideoQuality>;

            cbQuality.DataSource = urls;
            foreach (YouTubeVideoQuality item in cbQuality.Items)
            {
                // Look for the mp4 format, because I assume it's more commonly used.
                if (item.Extension.Equals("mp4"))
                {
                    cbQuality.SelectedItem = item;
                    break;
                }
            }
            lTitle.Text = FormatTitle(urls[0].VideoTitle);

            TimeSpan videoLength = TimeSpan.FromSeconds(urls[0].Length);
            if (videoLength.Hours > 0)
                videoThumbnail.Tag = string.Format("{0}:{1:00}:{2:00}", videoLength.Hours, videoLength.Minutes, videoLength.Seconds);
            else
                videoThumbnail.Tag = string.Format("{0}:{1:00}", videoLength.Minutes, videoLength.Seconds);
            videoThumbnail.Refresh();

            btnGetVideo.Enabled = txtYoutubeLink.Enabled = true;
            cbQuality.Enabled = urls.Count > 0;
            btnDownload.Enabled = true;

            Program.RunningWorkers.Remove(bwGetVideo);
        }

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
                if (item is DownloadListViewItem)
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
                if (item is DownloadListViewItem)
                {
                    string input = (item as DownloadListViewItem).Output;
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
                if (item is DownloadListViewItem)
                {
                    (item as DownloadListViewItem).Resume();
                }
            }
        }

        private void pauseMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvQueue.SelectedItems)
            {
                if (item is DownloadListViewItem)
                {
                    (item as DownloadListViewItem).Pause();
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

            var item = new ConvertListViewItem(Path.GetFileName(output));

            item.SubItems.Add("");
            item.SubItems.Add("Converting");
            item.SubItems.Add(FormatVideoLength(FfmpegHelper.GetDuration(input)));
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

            CroppingListViewItem item = new CroppingListViewItem(Path.GetFileName(output));

            item.SubItems.Add("");
            item.SubItems.Add("Cropping");
            item.SubItems.Add(FormatVideoLength(FfmpegHelper.GetDuration(input)));
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

        private string FormatTitle(string title)
        {
            string[] illegalCharacters = new string[] { "/", @"\", "*", "?", "\"", "<", ">" };

            var replace = new Dictionary<string, string>()
            {
                {"|", "-"},
                {"&#39;", "'"},
                {"&quot;", "'"},
                {"&lt;", "("},
                {"&gt;", ")"},
                {"+", " "},
                {":", "-"},
                {"amp;", "&"}
            };

            var sb = new System.Text.StringBuilder(title);

            foreach (string s in illegalCharacters)
                sb.Replace(s, string.Empty);

            foreach (KeyValuePair<string, string> s in replace)
                sb.Replace(s.Key, s.Value);

            return sb.ToString().Trim();
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

        public static string FormatVideoLength(TimeSpan duration)
        {
            if (duration.Hours > 0)
                return string.Format("{0}:{1:00}:{2:00}", duration.Hours, duration.Minutes, duration.Seconds);
            else
                return string.Format("{0}:{1:00}", duration.Minutes, duration.Seconds);
        }

        public static string FormatVideoLength(long duration)
        {
            return FormatVideoLength(TimeSpan.FromSeconds(duration));
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

    public class ConvertListViewItem : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status { get; set; }

        public event OperationEventHandler OperationComplete;

        public ConvertListViewItem(string text)
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
            FfmpegHelper.Convert(backgroundWorker, this.Input, this.Output);

            if (!string.IsNullOrEmpty(converterStart))
            {
                if (string.IsNullOrEmpty(converterEnd))
                {
                    FfmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart);
                }
                else
                {
                    FfmpegHelper.Crop(backgroundWorker, this.Output, this.Output, converterStart, converterEnd);
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

    public class CroppingListViewItem : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status { get; set; }

        public event OperationEventHandler OperationComplete;

        public CroppingListViewItem(string text)
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
                FfmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart);
            else
                FfmpegHelper.Crop(backgroundWorker, this.Input, this.Output, cropStart, cropEnd);

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
            this.SubItems[3].Text = MainForm.FormatVideoLength(FfmpegHelper.GetDuration(this.Input));
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

    public class DownloadListViewItem : ListViewItem, IOperation
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public OperationStatus Status
        {
            get
            {
                if (downloader == null)
                    return OperationStatus.None;

                switch (downloader.DownloadStatus)
                {
                    case DownloadStatus.Canceled:
                        return OperationStatus.Canceled;
                    case DownloadStatus.Downloading:
                        return OperationStatus.Working;
                    case DownloadStatus.Failed:
                        return OperationStatus.Failed;
                    case DownloadStatus.Paused:
                        return OperationStatus.Paused;
                    case DownloadStatus.Success:
                        return OperationStatus.Success;
                    default:
                        return OperationStatus.None;
                }
            }
            set
            {

            }
        }

        public event OperationEventHandler OperationComplete;

        public DownloadListViewItem(string text)
            : base(text)
        {
        }

        public void Download(string url, string output)
        {
            this.Input = url;
            string folder = Path.GetDirectoryName(output);
            string file = Path.GetFileName(output).Trim();
            this.Output = Path.Combine(folder, file);

            downloader = new FileDownloader(url, folder, file);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.RunWorkerCompleted += downloader_RunWorkerCompleted;
            downloader.RunWorkerAsync();

            Program.RunningWorkers.Add(downloader);
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
                downloader.Abort();

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region downloader

        private bool processing;
        FileDownloader downloader;

        private void downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (processing) return;
            if (ListView.InvokeRequired) ListView.Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;
                    string speed = string.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    string ETA = downloader.ETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)downloader.ETA) * 1000) + " ]";
                    this.SubItems[1].Text = e.ProgressPercentage + " %";
                    this.SubItems[2].Text = speed + ETA;
                    ProgressBar progressBar = (ProgressBar)((ListViewEx)this.ListView).GetEmbeddedControl(1, this.Index);
                    progressBar.Value = e.ProgressPercentage;
                    RefreshStatus();
                }
                catch { }
                finally { processing = false; }
            }
        }

        private void downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshStatus();

            Program.RunningWorkers.Remove(downloader);

            OnOperationComplete(new OperationEventArgs(this, this.Status));
        }

        #endregion

        private void RefreshStatus()
        {
            if (downloader.DownloadStatus == DownloadStatus.Success)
            {
                this.SubItems[2].Text = "Completed";
            }
            else if (downloader.DownloadStatus == DownloadStatus.Paused)
            {
                this.SubItems[2].Text = "Paused";
            }
            else if (downloader.DownloadStatus == DownloadStatus.Canceled)
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
}