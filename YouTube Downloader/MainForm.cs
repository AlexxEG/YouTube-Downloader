using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using YouTube_Downloader;

namespace YouTube_Downloader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            InitializeContextMenu();

            SettingsEx.Load();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SettingsEx.WindowStates[this.Name].SaveForm(this);

            string[] paths = new string[cbSaveTo.Items.Count];

            cbSaveTo.Items.CopyTo(paths, 0);
            SettingsEx.SaveToDirectories.Clear();
            SettingsEx.SaveToDirectories.AddRange(paths);

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
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.Silver, 2), new Point(0, 1), new Point(panel1.Width, 1));
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            txtYoutubeLink.Text = Clipboard.GetText();
        }

        private void btnGetVideo_Click(object sender, EventArgs e)
        {
            if (!Helper.isValidUrl(txtYoutubeLink.Text) || !txtYoutubeLink.Text.ToLower().Contains("www.youtube.com/watch?"))
                MessageBox.Show(this, "You enter invalid YouTube URL, Please correct it.\r\n\nNote: URL should start with:\r\nhttp://www.youtube.com/watch?",
                    "Invalid URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                btnGetVideo.Enabled = txtYoutubeLink.Enabled = false;
                backgroundWorker1.RunWorkerAsync(txtYoutubeLink.Text);
            }
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string path = string.Empty;

            try
            {
                path = cbSaveTo.Text;

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!cbSaveTo.Items.Contains(path))
                    cbSaveTo.Items.Add(path);
            }
            catch
            {
                MessageBox.Show("Couldn't create directory.");
            }

            try
            {
                YouTubeVideoQuality tempItem = cbQuality.SelectedItem as YouTubeVideoQuality;

                DownloadListViewItem item = new DownloadListViewItem(tempItem.VideoTitle + "." + tempItem.Extension);

                item.SubItems.Add("0 %");
                item.SubItems.Add("");

                TimeSpan videoLength = TimeSpan.FromSeconds(tempItem.Length);
                if (videoLength.Hours > 0)
                    item.SubItems.Add(String.Format("{0}:{1}:{2}", videoLength.Hours, videoLength.Minutes, videoLength.Seconds));
                else
                    item.SubItems.Add(String.Format("{0}:{1}", videoLength.Minutes, videoLength.Seconds));

                item.SubItems.Add(String.Format(new FileSizeFormatProvider(), "{0:fs}", tempItem.VideoSize));
                item.SubItems.Add(tempItem.VideoUrl);

                lvQueue.Items.Add(item);

                item.Download(tempItem.DownloadUrl, Path.Combine(path, tempItem.VideoTitle + "." + tempItem.Extension));
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
                cbSaveTo.Text = ofd.Folder;
                cbSaveTo.Items.Add(ofd.Folder);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = YouTubeDownloader.GetYouTubeVideoUrls(e.Argument + "");
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<YouTubeVideoQuality> urls = e.Result as List<YouTubeVideoQuality>;

            cbQuality.DataSource = urls;
            lTitle.Text = string.Format("Title: {0}", urls[0].VideoTitle);

            btnGetVideo.Enabled = txtYoutubeLink.Enabled = true;
            btnDownload.Enabled = true;
        }

        #region contextMenu1

        ContextMenu contextMenu1;
        MenuItem openMenuItem;
        MenuItem openContainingFolderMenuItem;
        MenuItem convertToMP3MenuItem;
        MenuItem resumeMenuItem;
        MenuItem pauseMenuItem;
        MenuItem stopMenuItem;
        MenuItem removeMenuItem;

        public void InitializeContextMenu()
        {
            contextMenu1 = new ContextMenu();

            openMenuItem = new MenuItem("Open", openMenuItem_Click);
            openContainingFolderMenuItem = new MenuItem("Open Containing Folder", openContainingFolderMenuItem_Click);
            convertToMP3MenuItem = new MenuItem("Convert to MP3", convertToMP3MenuItem_Click);
            resumeMenuItem = new MenuItem("Resume", resumeMenuItem_Click);
            pauseMenuItem = new MenuItem("Pause", pauseMenuItem_Click);
            stopMenuItem = new MenuItem("Stop", stopMenuItem_Click);
            removeMenuItem = new MenuItem("Remove", removeMenuItem_Click);

            contextMenu1.MenuItems.Add(openMenuItem);
            contextMenu1.MenuItems.Add(openContainingFolderMenuItem);
            contextMenu1.MenuItems.Add(new MenuItem("-"));
            contextMenu1.MenuItems.Add(convertToMP3MenuItem);
            contextMenu1.MenuItems.Add(new MenuItem("-"));
            contextMenu1.MenuItems.Add(resumeMenuItem);
            contextMenu1.MenuItems.Add(pauseMenuItem);
            contextMenu1.MenuItems.Add(stopMenuItem);
            contextMenu1.MenuItems.Add(removeMenuItem);

            contextMenu1.Collapse += contextMenu1_Collapse;
            contextMenu1.Popup += contextMenu1_Popup;

            lvQueue.ContextMenu = contextMenu1;
        }

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

            if (lvQueue.SelectedItems[0] is ConvertListViewItem)
            {
                ConvertListViewItem item = lvQueue.SelectedItems[0] as ConvertListViewItem;

                convertToMP3MenuItem.Enabled = false;
                resumeMenuItem.Visible = pauseMenuItem.Visible = stopMenuItem.Visible = false;

                switch (item.Status)
                {
                    case ConvertStatus.Converting:
                        openMenuItem.Enabled = removeMenuItem.Enabled = false;
                        break;
                    case ConvertStatus.Failed:
                        openMenuItem.Enabled = false;
                        break;
                    case ConvertStatus.Success:
                        break;
                }
            }
            else if (lvQueue.SelectedItems[0] is DownloadListViewItem)
            {
                DownloadListViewItem item = lvQueue.SelectedItems[0] as DownloadListViewItem;

                convertToMP3MenuItem.Enabled = item.DownloadStatus == DownloadStatus.Success;

                // Only need to REMOVE MenuItems, not show since it's done automatically
                switch (item.DownloadStatus)
                {
                    case DownloadStatus.Success:
                        resumeMenuItem.Visible = pauseMenuItem.Visible = stopMenuItem.Visible = false;
                        break;
                    case DownloadStatus.Downloading:
                        openMenuItem.Visible = resumeMenuItem.Visible = false;
                        break;
                    case DownloadStatus.Paused:
                        openMenuItem.Visible = pauseMenuItem.Visible = false;
                        break;
                    case DownloadStatus.Canceled:
                    case DownloadStatus.Failed:
                        openMenuItem.Visible = pauseMenuItem.Visible = stopMenuItem.Visible = resumeMenuItem.Visible = false;
                        break;
                }
            }
        }

        private void contextMenu1_Collapse(object sender, EventArgs e)
        {
            foreach (MenuItem menuItem in contextMenu1.MenuItems)
            {
                menuItem.Visible = true;
            }
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = string.Empty;

                if (lvQueue.SelectedItems[0] is ConvertListViewItem)
                {
                    path = (lvQueue.SelectedItems[0] as ConvertListViewItem).Output;
                }
                else if (lvQueue.SelectedItems[0] is DownloadListViewItem)
                {
                    path = (lvQueue.SelectedItems[0] as DownloadListViewItem).File;
                }

                Process.Start(path);
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open file.");
            }
        }

        private void openContainingFolderMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = string.Empty;

                if (lvQueue.SelectedItems[0] is ConvertListViewItem)
                {
                    path = (lvQueue.SelectedItems[0] as ConvertListViewItem).Output;
                }
                else if (lvQueue.SelectedItems[0] is DownloadListViewItem)
                {
                    path = (lvQueue.SelectedItems[0] as DownloadListViewItem).File;
                }

                Process.Start(Path.GetDirectoryName(path));
            }
            catch
            {
                MessageBox.Show(this, "Couldn't open folder.");
            }
        }

        private void convertToMP3MenuItem_Click(object sender, EventArgs e)
        {
            DownloadListViewItem item = lvQueue.SelectedItems[0] as DownloadListViewItem;

            ConvertListViewItem newItem = new ConvertListViewItem(Path.GetFileNameWithoutExtension(item.Text) + ".mp3");

            newItem.SubItems.Add("Converting");
            newItem.SubItems.Add("-");
            newItem.SubItems.Add(item.SubItems[3].Text);
            newItem.SubItems.Add("-");
            newItem.SubItems.Add("-");

            lvQueue.Items.Add(newItem);

            newItem.Convert(item.File);
        }

        private void resumeMenuItem_Click(object sender, EventArgs e)
        {
            DownloadListViewItem item = lvQueue.SelectedItems[0] as DownloadListViewItem;

            item.Resume();
        }

        private void pauseMenuItem_Click(object sender, EventArgs e)
        {
            DownloadListViewItem item = lvQueue.SelectedItems[0] as DownloadListViewItem;

            item.Pause();
        }

        private void stopMenuItem_Click(object sender, EventArgs e)
        {
            DownloadListViewItem item = lvQueue.SelectedItems[0] as DownloadListViewItem;

            item.Stop();
        }

        private void removeMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem item = lvQueue.SelectedItems[0];

            if (item is DownloadListViewItem)
                (item as DownloadListViewItem).Stop();

            item.Remove();
        }

        #endregion

        public static string FormatTitle(string title)
        {
            return title.Replace(@"\", "").Replace("&#39;", "'").Replace("&quot;", "'").Replace("&lt;", "(").Replace("&gt;", ")").Replace("+", " ").Replace(":", "-");
        }
    }

    public class FfmpegHelper
    {
        public static string ConvertToMP3(string input, string output)
        {
            string arguments = string.Format(" -i \"{0}\" -vn -f mp3 -ab 192k \"{1}\"", input, output);

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = Application.StartupPath + "\\ffmpeg";
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.StandardOutput.ReadToEnd();

            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!process.HasExited)
                process.Kill();

            return error;
        }
    }

    public enum ConvertStatus { Converting, Success, Failed }

    public class ConvertListViewItem : ListViewItem
    {
        public string File { get; set; }
        public string Output { get; set; }
        public ConvertStatus Status;

        public ConvertListViewItem(string text)
            : base(text)
        {
        }

        public void Convert(string file)
        {
            this.File = file;

            converter = new BackgroundWorker();
            converter.DoWork += converter_DoWork;
            converter.RunWorkerCompleted += converter_RunWorkerCompleted;
            converter.RunWorkerAsync(this.File);
            this.Status = ConvertStatus.Converting;
        }

        #region converter

        private BackgroundWorker converter;

        private void converter_DoWork(object sender, DoWorkEventArgs e)
        {
            string output = Path.Combine(Path.GetDirectoryName((string)e.Argument), Path.GetFileNameWithoutExtension((string)e.Argument) + ".mp3");

            FfmpegHelper.ConvertToMP3((string)e.Argument, output);

            e.Result = output;
        }

        private void converter_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Output = (string)e.Result;

            // Display MP3 size
            using (var stream = System.IO.File.Open(this.Output, FileMode.Open, FileAccess.Read))
            {
                string size = String.Format(new FileSizeFormatProvider(), "{0:fs}", stream.Length);

                this.SubItems[4].Text = size;
                this.SubItems[1].Text = "Success";

                stream.Close();
            }

            this.Status = ConvertStatus.Success;
        }

        #endregion
    }

    public class DownloadListViewItem : ListViewItem
    {
        public DownloadStatus DownloadStatus { get { return downloader.DownloadStatus; } }
        public string File { get; set; }

        public DownloadListViewItem(string text)
            : base(text)
        {

        }

        public void Download(string url, string saveTo)
        {
            this.File = saveTo;
            var folder = Path.GetDirectoryName(saveTo);
            string file = Path.GetFileName(saveTo);
            downloader = new FileDownloader(url, folder, file);
            downloader.ProgressChanged += downloader_ProgressChanged;
            downloader.RunWorkerCompleted += downloader_RunWorkerCompleted;

            downloader.RunWorkerAsync();
        }

        public void Pause()
        {
            downloader.Pause();
        }

        public void Resume()
        {
            downloader.Resume();
        }

        public void Stop()
        {
            downloader.Abort();
        }

        #region downloader

        FileDownloader downloader;

        private void downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshStatus();
        }

        private void downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (processing) return;
            if (ListView.InvokeRequired) ListView.Invoke(new ProgressChangedEventHandler(downloader_ProgressChanged), sender, e);
            else
            {
                try
                {
                    processing = true;
                    string speed = String.Format(new FileSizeFormatProvider(), "{0:s}", downloader.DownloadSpeed);
                    string ETA = downloader.ETA == 0 ? "" : "  [ " + FormatLeftTime.Format(((long)downloader.ETA) * 1000) + " ]";
                    this.SubItems[1].Text = e.ProgressPercentage + " %";
                    this.SubItems[2].Text = speed + ETA;
                    RefreshStatus();
                }
                catch { }
                finally { processing = false; }
            }
        }

        #endregion

        private bool processing;

        private void RefreshStatus()
        {
            if (downloader.DownloadStatus == DownloadStatus.Success)
            {
                this.SubItems[1].Text = "Completed";
            }
            else if (downloader.DownloadStatus == DownloadStatus.Paused)
            {
                this.SubItems[1].Text = "Paused";
            }
        }
    }
}