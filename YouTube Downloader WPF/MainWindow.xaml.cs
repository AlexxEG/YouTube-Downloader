using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using YouTube_Downloader_WPF.Classes;
using YouTube_Downloader_WPF.Dialogs;
using YouTube_Downloader_WPF.Enums;
using YouTube_Downloader_WPF.Operations;
using YouTube_Downloader_WPF.Properties;
using WinForms = System.Windows.Forms;

namespace YouTube_Downloader_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, WinForms.IWin32Window, INotifyPropertyChanged
    {
        Settings settings = Properties.Settings.Default;

        private ObservableCollection<PlaylistItem> _playlistItems;
        private ObservableCollection<Operation> _queue;

        public ObservableCollection<PlaylistItem> PlaylistItems
        {
            get { return _playlistItems; }
            set
            {
                _playlistItems = value;
                this.OnPropertyChanged();
            }
        }
        public ObservableCollection<Operation> Queue
        {
            get { return _queue; }
            set
            {
                _queue = value;
                this.OnPropertyChanged();
            }
        }

        public IntPtr Handle
        {
            get
            {
                var interopHelper = new WindowInteropHelper(this);
                return interopHelper.Handle;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            if (settings.SaveToDirectories == null)
                settings.SaveToDirectories = new StringCollection();

            this.PlaylistItems = new ObservableCollection<PlaylistItem>();
            this.Queue = new ObservableCollection<Operation>();
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.GetIsWorking())
            {
                WinForms.DialogResult result = WinForms.MessageBox.Show(this,
                    "Files are being downloaded/converted/cut.\n\nAre you sure you want to quit?",
                    "Confirmation",
                    WinForms.MessageBoxButtons.YesNo,
                    WinForms.MessageBoxIcon.Warning);

                if (result == WinForms.DialogResult.Yes)
                {
                    // Hides window while waiting for threads to finish, except downloads which will abort.
                    this.CancelOperations();
                }

                e.Cancel = true;
                return;
            }

            if (this.VideoInformation != null)
                this.VideoInformation.AbortUpdateFileSizes();

            settings.WindowStates[this.Name].SaveWindow(this);

            settings.Save();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadSettings();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsFlyout.IsOpen = true;
        }

        private void VideoFormats_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chb = sender as CheckBox;

            if (chb.IsChecked == true)
                return;

            if (!IncludeDASH.IsChecked == true &&
                !IncludeNonDASH.IsChecked == true &&
                !IncludeNormal.IsChecked == true)
            {
                chb.IsChecked = true;
            }
        }

        private void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Operation operation = button.Tag as Operation;

            switch (button.ToolTip.ToString())
            {
                case "Open":
                    if (operation.CanOpen())
                        operation.Open();
                    break;
                case "Open Containing Folder":
                    operation.OpenContainingFolder();
                    break;
                case "Convert to MP3":
                    if (operation is DownloadOperation && operation.Status == OperationStatus.Success)
                    {
                        string input = (operation as DownloadOperation).Output;
                        string output = Path.GetDirectoryName(input) + "\\" + Path.GetFileNameWithoutExtension(input) + ".mp3";

                        ConvertInput.Text = input;
                        ConvertOutput.Text = output;
                        TabControl.SelectedIndex = 2;
                        break;
                    }
                    break;
                case "Pause":
                    if (operation.CanPause())
                        operation.Pause();
                    break;
                case "Resume":
                    if (operation.CanResume())
                        operation.Resume();
                    break;
                case "Remove":
                    if (!operation.CanStop())
                        return;

                    operation.Stop(true);
                    this.Queue.Remove(operation);
                    break;
            }
        }

        #region Download Tab

        bool _canGetVideo = true;
        VideoFormat _selectedFormat;
        VideoInfo _videoInformation;

        /// <summary>
        /// Used to disable controls in the Download tab when getting video information in the background.
        /// </summary>
        public bool CanGetVideo
        {
            get { return _canGetVideo; }
            set
            {
                _canGetVideo = value;
                this.OnPropertyChanged();
            }
        }

        public VideoFormat SelectedFormat
        {
            get { return _selectedFormat; }
            set
            {
                _selectedFormat = value;
                this.OnPropertyChanged();
            }
        }

        public VideoInfo VideoInformation
        {
            get { return _videoInformation; }
            set
            {
                _videoInformation = value;
                this.OnPropertyChanged();
            }
        }

        private void VideoLinkPasteButton_Click(object sender, RoutedEventArgs e)
        {
            VideoLink.Text = Regex.Replace(Clipboard.GetText(), "\r\n", " \u200B");
        }

        private void GetVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Helper.IsValidYouTubeUrl(VideoLink.Text))
            {
                MessageBox.Show(this, "You entered invalid YouTube URL, Please correct it.\r\n\n" +
                    "Note: URL should start with: " + @"http://www.youtube.com/watch?",
                    "Invalid URL", MessageBoxButton.OK);
            }
            else
            {
                VideoLink.Text = Helper.FixUrl(VideoLink.Text);

                settings.LastYouTubeUrl = VideoLink.Text;

                this.VideoInformation = null;

                // Disabled controls while getting video information.
                this.CanGetVideo = false;

                YoutubeDlHelper.GetVideoInfoAsync(VideoLink.Text, GetVideoInfoResult);
            }
        }

        private void VideoBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            if (Directory.Exists(this.VideoSaveTo.Text))
                ofd.InitialFolder = Path.GetDirectoryName(this.VideoSaveTo.Text);
            else
                ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (ofd.ShowDialog(this) == WinForms.DialogResult.OK)
            {
                settings.SaveToDirectories.Add(ofd.Folder);
                this.VideoSaveTo.SelectedIndex = this.VideoSaveTo.Items.Count - 1;
                this.VideoSaveTo.Items.Refresh();
            }
        }

        private void VideoDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.VideoInformation == null)
                return;

            // Validate the filename, checking for illegal characters. 
            // Prompts the user to remove these characters automatically.
            if (!this.ValidateFilename(this.VideoInformation.Title))
                return;

            string path = VideoSaveTo.Text;

            // Make sure download directory exists, 
            // prompting the user to create it if it doesn't.
            if (!this.ValidateDirectory(path))
                return;

            if (!settings.SaveToDirectories.Contains(path))
                settings.SaveToDirectories.Add(path);

            try
            {
                string filename = string.Format("{0}.{1}",
                    this.VideoInformation.Title,
                    this.SelectedFormat.Extension);

                if (File.Exists(Path.Combine(path, filename)))
                {
                    MessageBoxResult result = MessageBox.Show(this,
                        string.Format("File '{1}' already exists.{0}{0}Overwrite?", Environment.NewLine, filename),
                        "Overwrite?",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                        return;

                    File.Delete(Path.Combine(path, filename));
                }

                DownloadOperation operation = new DownloadOperation(this.SelectedFormat);

                operation.OperationComplete += DownloadOperation_OperationComplete;

                this.Queue.Add(operation);
                this.SelectOneItem(operation);

                if (this.SelectedFormat.AudioOnly || this.SelectedFormat.FormatType == FormatType.Normal)
                    operation.Start(DownloadOperation.Args(this.SelectedFormat.DownloadUrl, Path.Combine(path, filename)));
                else
                {
                    VideoFormat audio = Helper.GetAudioFormat(this.SelectedFormat);

                    operation.Start(DownloadOperation.Args(audio.DownloadUrl, this.SelectedFormat.DownloadUrl, Path.Combine(path, filename)));
                }

                TabControl.SelectedIndex = 3;
            }
            catch (Exception ex)
            {
                App.SaveException(ex);

                WinForms.MessageBox.Show(this,
                    ex.Message, ex.Source,
                    WinForms.MessageBoxButtons.OK,
                    WinForms.MessageBoxIcon.Error);
            }
        }

        private void FilteredFormats_Filter(object sender, FilterEventArgs e)
        {
            VideoFormat f = e.Item as VideoFormat;

            if (f.Extension.Contains("webm") ||
                !Settings.Default.IncludeNonDASH && f.Format.ToLower().Contains("nondash") ||
                !Settings.Default.IncludeDASH && f.Format.ToLower().Contains("dash") ||
                !Settings.Default.IncludeNormal && !f.Format.ToLower().Contains("dash"))
            {
                e.Accepted = false;
            }
        }

        private void DownloadOperation_OperationComplete(object sender, OperationEventArgs e)
        {
            Operation operation = (Operation)e.Operation;

            if (AutoConvert.IsEnabled == true && AutoConvert.IsChecked == true && operation.Status == OperationStatus.Success)
            {
                string output = Path.Combine(Path.GetDirectoryName(operation.Output),
                    Path.GetFileNameWithoutExtension(operation.Output)) + ".mp3";

                this.Convert(operation.Output, output, false);
            }
        }

        private void GetVideoInfoResult(VideoInfo videoInfo)
        {
            this.VideoInformation = videoInfo;
            this.CanGetVideo = true;
        }

        #endregion

        #region Playlist Tab

        bool _canDownloadPlaylist = false;
        bool _canGetPlaylist = true;
        bool _isPlaylistLinkValid = false;
        private BackgroundWorker _backgroundWorkerPlaylist;

        public bool CanDownloadPlaylist
        {
            get { return _canDownloadPlaylist; }
            set
            {
                _canDownloadPlaylist = value;
                this.OnPropertyChanged();
            }
        }

        public bool CanGetPlaylist
        {
            get { return _canGetPlaylist; }
            set
            {
                _canGetPlaylist = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsPlaylistLinkValid
        {
            get { return _isPlaylistLinkValid; }
            set
            {
                if (value == _isPlaylistLinkValid)
                    return;

                _isPlaylistLinkValid = value;
                this.OnPropertyChanged();
            }
        }

        private void PlaylistBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog();

            if (Directory.Exists(this.PlaylistSaveTo.Text))
                ofd.InitialFolder = Path.GetDirectoryName(this.PlaylistSaveTo.Text);
            else
                ofd.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (ofd.ShowDialog(this) == WinForms.DialogResult.OK)
            {
                settings.SaveToDirectories.Add(ofd.Folder);
                this.PlaylistSaveTo.SelectedIndex = this.PlaylistSaveTo.Items.Count - 1;
                this.PlaylistSaveTo.Items.Refresh();
            }
        }

        private void PlaylistGet_Click(object sender, RoutedEventArgs e)
        {
            if (this.PlaylistGet.Content.ToString() == "Get Playlist")
            {
                this.CanGetPlaylist = false;
                this.PlaylistGet.Content = "Cancel";

                _backgroundWorkerPlaylist = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                _backgroundWorkerPlaylist.DoWork += _backgroundWorkerPlaylist_DoWork;
                _backgroundWorkerPlaylist.ProgressChanged += _backgroundWorkerPlaylist_ProgressChanged;
                _backgroundWorkerPlaylist.RunWorkerCompleted += _backgroundWorkerPlaylist_RunWorkerCompleted;
                _backgroundWorkerPlaylist.RunWorkerAsync(PlaylistLink.Text);

                // Save playlist url
                settings.LastPlaylistUrl = PlaylistLink.Text;
            }
            else if (this.PlaylistGet.Content.ToString() == "Cancel")
            {
                _backgroundWorkerPlaylist.CancelAsync();
            }
        }

        private void PlaylistDownloadAll_Click(object sender, RoutedEventArgs e)
        {
            this.StartPlaylistOperation(null);
        }

        private void PlaylistDownloadSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedVideos = GetSelectedPlaylistVideos();

            if (selectedVideos.Length < 1)
                return;

            List<VideoInfo> videos = new List<VideoInfo>();

            foreach (var video in selectedVideos)
            {
                videos.Add(video.VideoInfo as VideoInfo);
            }

            this.StartPlaylistOperation(videos);
        }

        private void PlaylistLink_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                this.IsPlaylistLinkValid = Helper.IsPlaylist(PlaylistLink.Text);
            }
            catch (Exception)
            {
                this.IsPlaylistLinkValid = false;
            }
        }

        private void PlaylistListMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            switch (item.Header.ToString())
            {
                case "Select All":
                    {
                        foreach (var video in this.PlaylistItems)
                            video.Selected = true;
                    } break;
                case "Select None":
                    {
                        foreach (var video in this.PlaylistItems)
                            video.Selected = false;
                    } break;
            }
        }

        private void _backgroundWorkerPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            string playlistUrl = e.Argument as string;
            PlaylistReader reader = new PlaylistReader(playlistUrl);
            VideoInfo video;

            while ((video = reader.Next()) != null)
            {
                if (_backgroundWorkerPlaylist.CancellationPending)
                {
                    e.Result = false;
                    break;
                }

                PlaylistItem item = new PlaylistItem(video);

                _backgroundWorkerPlaylist.ReportProgress(1, item);
            }

            e.Result = true;
        }

        private void _backgroundWorkerPlaylist_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PlaylistItem item = e.UserState as PlaylistItem;

            this.PlaylistItems.Add(item);
            this.PlaylistVideos.ScrollIntoView(item);
        }

        private void _backgroundWorkerPlaylist_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool result = (bool)e.Result;

            this.PlaylistGet.Content = "Get Playlist";
            this.CanDownloadPlaylist = result;

            if (!result)
            {
                this.PlaylistItems.Clear();
            }

            this.CanGetPlaylist = true;
        }

        private void StartPlaylistOperation(ICollection<VideoInfo> videos)
        {
            string path = this.PlaylistSaveTo.Text;

            // Make sure download directory exists.
            if (!this.ValidateDirectory(path))
                return;

            if (!settings.SaveToDirectories.Contains(path))
                settings.SaveToDirectories.Add(path);

            settings.LastPlaylistUrl = this.PlaylistLink.Text;

            try
            {
                PlaylistOperation item = new PlaylistOperation();

                this.Queue.Add(item);
                this.SelectOneItem(item);

                item.Start(PlaylistOperation.Args(this.PlaylistLink.Text, path, settings.UseDashPlaylist, videos));

                TabControl.SelectedIndex = 3;
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show(this,
                    ex.Message, ex.Source,
                    WinForms.MessageBoxButtons.OK,
                    WinForms.MessageBoxIcon.Error);
            }
        }

        private PlaylistItem[] GetSelectedPlaylistVideos()
        {
            List<PlaylistItem> videos = new List<PlaylistItem>();

            foreach (var video in this.PlaylistItems)
                if (video.Selected) videos.Add(video);

            return videos.ToArray();
        }

        #endregion

        #region Convert Tab

        OpenFileDialog openFileDialog = new OpenFileDialog();
        SaveFileDialog saveFileDialog = new SaveFileDialog();

        private void ConvertInputBrowse_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.FileName = Path.GetFileName(ConvertInput.Text);

            if (openFileDialog.ShowDialog(this) == true)
            {
                ConvertInput.Text = openFileDialog.FileName;

                if (ConvertOutput.Text == string.Empty)
                {
                    // Suggest file name
                    string output = Path.GetDirectoryName(openFileDialog.FileName);

                    output = Path.Combine(output, Path.GetFileNameWithoutExtension(openFileDialog.FileName));
                    output += ".mp3";

                    ConvertOutput.Text = output;
                }
            }
        }

        private void ConvertOutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ConvertInput.Text))
            {
                saveFileDialog.FileName = Path.GetFileName(ConvertInput.Text);
            }
            else
            {
                saveFileDialog.FileName = Path.GetFileName(ConvertOutput.Text);
            }

            if (saveFileDialog.ShowDialog(this) == true)
            {
                ConvertOutput.Text = saveFileDialog.FileName;
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if (!FFmpegHelper.CanConvertMP3(ConvertInput.Text))
            {
                string text = "Can't convert input file to MP3. File doesn't appear to have audio.";

                WinForms.MessageBox.Show(this,
                    text, "Missing Audio",
                    WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                return;
            }

            if (File.Exists(ConvertOutput.Text))
            {
                string filename = Path.GetFileName(ConvertOutput.Text);
                string text = "File '" + filename + "' already exists.\n\nOverwrite?";

                if (WinForms.MessageBox.Show(this,
                    text, "Overwrite",
                    WinForms.MessageBoxButtons.YesNo) == WinForms.DialogResult.No)
                {
                    return;
                }

            }

            if (ConvertInput.Text == ConvertOutput.Text ||
                // If they match, the user probably wants to crop. Right?
                Path.GetExtension(ConvertInput.Text) == Path.GetExtension(ConvertOutput.Text))
            {
                this.Crop(ConvertInput.Text, ConvertOutput.Text);
            }
            else
            {
                this.Convert(ConvertInput.Text, ConvertOutput.Text, true);
            }

            ConvertInput.Clear();
            ConvertOutput.Clear();

            TabControl.SelectedIndex = 3;
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChangedExplicit(propertyName);
        }

        private void OnPropertyChangedExplicit(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /// <summary>
        /// Inserts a video url &amp; retrieve video info automatically.
        /// </summary>
        /// <param name="url">The url to insert.</param>
        public void InsertVideo(string url)
        {
            VideoLink.Text = url;
            GetVideoButton_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Cancels all active IOperations.
        /// </summary>
        private void CancelOperations()
        {
            foreach (Operation operation in App.RunningOperations)
            {
                // Stop & delete unfinished files
                if (operation.CanStop())
                    operation.Stop(true);
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

            if (crop && CropFrom.IsChecked == true)
            {
                // Validate cropping input. Shows error messages automatically.
                if (!this.ValidateCropping())
                    return;

                start = TimeSpan.Parse(CropFromTextBox.Text);
                end = TimeSpan.Parse(CropToTextBox.Text);
            }

            var operation = new ConvertOperation();

            this.Queue.Add(operation);
            this.SelectOneItem(operation);

            operation.Start(ConvertOperation.Args(input, output, start, end));
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

            TimeSpan start = TimeSpan.Parse(CropFromTextBox.Text);
            TimeSpan end = TimeSpan.Parse(CropToTextBox.Text);

            CroppingOperation item = new CroppingOperation();

            this.Queue.Add(item);
            this.SelectOneItem(item);

            item.Start(CroppingOperation.Args(input, output, start, end));
        }

        /// <summary>
        /// Returns true if there is a working IOperation.
        /// </summary>
        private bool GetIsWorking()
        {
            foreach (var operation in this.Queue)
            {
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

            // Add WindowState for form if WindowStates doesn't have a entry for it
            if (!settings.WindowStates.Contains(this.Name))
            {
                settings.WindowStates.Add(this.Name);
            }

            // Restore form location, size & window state, if not null
            settings.WindowStates[this.Name].RestoreWindow(this);

            // Restore last used links
            if (settings.LastYouTubeUrl != null) VideoLink.Text = settings.LastYouTubeUrl;
            if (settings.LastPlaylistUrl != null) PlaylistLink.Text = settings.LastPlaylistUrl;
        }

        /// <summary>
        /// Deselects all other items in given ListViewItem's ListView except the given item.
        /// </summary>
        /// <param name="item">The ListViewItem to select.</param>
        private void SelectOneItem(object item)
        {
            this.QueueList.SelectedItems.Clear();
            this.QueueList.SelectedItem = item;
        }

        /// <summary>
        /// Returns true if cropping information can be validated. Fills empty space with zeros.
        /// </summary>
        private bool ValidateCropping()
        {
            try
            {
                // Fill in empty space with zeros
                CropFromTextBox.Text = CropFromTextBox.Text.Replace('_', '0');

                // Validate TimeSpan object
                TimeSpan.Parse(CropFromTextBox.Text);

                if (CropTo.IsEnabled == true && CropTo.IsChecked == true)
                {
                    // Fill in empty space with zeros
                    CropToTextBox.Text = CropToTextBox.Text.Replace('_', '0');

                    // Validate TimeSpan object
                    TimeSpan.Parse(CropToTextBox.Text);
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

                    if (MessageBox.Show(this, text, "Missing Folder", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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

                if (MessageBox.Show(this, text, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this.VideoInformation.Title = newFilename;
                    valid = true;
                }
            }

            return valid;
        }
    }
}
