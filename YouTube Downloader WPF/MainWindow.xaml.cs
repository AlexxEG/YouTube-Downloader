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
using System.Windows.Interop;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using YouTube_Downloader_DLL;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Dialogs;
using YouTube_Downloader_DLL.Enums;
using YouTube_Downloader_DLL.Helpers;
using YouTube_Downloader_DLL.Operations;
using YouTube_Downloader_WPF.Properties;
using WinForms = System.Windows.Forms;
using WPF_Classes = YouTube_Downloader_WPF.Classes;

namespace YouTube_Downloader_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, WinForms.IWin32Window, INotifyPropertyChanged
    {
        Settings settings = Properties.Settings.Default;

        ObservableCollection<PlaylistItem> _playlistItems;
        ObservableCollection<Operation> _queue;

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

        private void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            new UpdateDownloader().ShowDialog(this);
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
            var operation = button.Tag as Operation;

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
                    if (operation.IsDone)
                        this.Queue.Remove(operation);

                    break;
                case "Stop":
                    if (operation.CanStop())
                        operation.Stop(true);

                    break;
            }
        }

        #region Download Tab

        bool _enableDownloadControls = true;
        VideoFormat _selectedFormat;
        VideoInfo _videoInformation;

        /// <summary>
        /// Gets or sets whether to enable controls in the 'Download' tab.
        /// </summary>
        public bool EnableDownloadControls
        {
            get { return _enableDownloadControls; }
            set
            {
                _enableDownloadControls = value;
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
            if (!Helper.IsValidUrl(VideoLink.Text))
            {
                MessageBox.Show(this, "Input link is not a valid Twitch/YouTube link", "Invalid URL", MessageBoxButton.OK);
            }
            else
            {
                VideoLink.Text = Helper.FixUrl(VideoLink.Text);

                settings.LastYouTubeUrl = VideoLink.Text;

                this.VideoInformation = null;

                // Disabled controls while getting video information.
                this.EnableDownloadControls = false;

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
                        string.Format("File '{1}' already exists.{0}{0}Overwrite?",
                            Environment.NewLine,
                            filename),
                            "Overwrite?",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.No)
                        return;

                    File.Delete(Path.Combine(path, filename));
                }

                Operation operation;

                if (VideoInformation.VideoSource == VideoSource.Twitch)
                {
                    operation = new TwitchOperation(this.SelectedFormat);
                }
                else
                {
                    operation = new DownloadOperation(this.SelectedFormat);
                }

                operation.Completed += DownloadOperation_Completed;

                this.Queue.Add(operation);
                this.SelectOneItem(operation);

                if (VideoInformation.VideoSource == VideoSource.Twitch)
                {
                    operation.Prepare(TwitchOperation.Args(Path.Combine(path, filename), this.SelectedFormat));
                }
                else
                {
                    if (this.SelectedFormat.AudioOnly || this.SelectedFormat.FormatType == FormatType.Normal)
                        operation.Prepare(DownloadOperation.Args(this.SelectedFormat.DownloadUrl,
                                                                 Path.Combine(path, filename)));
                    else
                    {
                        VideoFormat audio = Helper.GetAudioFormat(this.SelectedFormat);

                        operation.Prepare(DownloadOperation.Args(audio.DownloadUrl,
                                                                 this.SelectedFormat.DownloadUrl,
                                                                 Path.Combine(path, filename)));
                    }
                }

                TabControl.SelectedIndex = 3;

                DownloadQueueHandler.Add(operation);
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

        private void VideoFormats_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            // Select last item
            this.VideoFormats.SelectedIndex = this.VideoFormats.Items.Count - 1;
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

            this.VideoInformation.SortFormats();
        }

        private void DownloadOperation_Completed(object sender, OperationEventArgs e)
        {
            var operation = (Operation)sender;

            if (AutoConvert.IsEnabled == true && AutoConvert.IsChecked == true && operation.Status == OperationStatus.Success)
            {
                string output = Path.Combine(Path.GetDirectoryName(operation.Output),
                    Path.GetFileNameWithoutExtension(operation.Output)) + ".mp3";

                this.Convert(operation.Output, output, false);
            }
        }

        private void GetVideoInfoResult(VideoInfo videoInfo)
        {
            if (!videoInfo.Failure)
            {
                this.VideoInformation = videoInfo;
            }
            else
            {
                MessageBox.Show(this, "Couldn't retrieve video. Reason:\n\n" + videoInfo.FailureReason);
            }

            this.EnableDownloadControls = true;
        }

        #endregion

        #region Playlist Tab

        bool _canDownloadPlaylist = false;
        bool _enablePlaylistControls = true;
        bool _isPlaylistLinkValid = false;
        string _playlistName = string.Empty;

        BackgroundWorker _backgroundWorkerPlaylist;

        public bool CanDownloadPlaylist
        {
            get { return _canDownloadPlaylist; }
            set
            {
                _canDownloadPlaylist = value;
                this.OnPropertyChanged();
            }
        }

        public bool EnablePlaylistControls
        {
            get { return _enablePlaylistControls; }
            set
            {
                _enablePlaylistControls = value;
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

        private void PlaylistLinkPasteButton_Click(object sender, RoutedEventArgs e)
        {
            PlaylistLink.Text = Regex.Replace(Clipboard.GetText(), "\r\n", " \u200B");
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
            this.EnablePlaylistControls = false;

            // Reset playlist variables
            _playlistName = string.Empty;
            this.PlaylistItems.Clear();

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

        private void PlaylistCancel_Click(object sender, RoutedEventArgs e)
        {
            _backgroundWorkerPlaylist.CancelAsync();
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
                    }
                    break;
                case "Select None":
                    {
                        foreach (var video in this.PlaylistItems)
                            video.Selected = false;
                    }
                    break;
            }
        }

        private void _backgroundWorkerPlaylist_DoWork(object sender, DoWorkEventArgs e)
        {
            string playlistUrl = e.Argument as string;
            PlaylistReader reader = new PlaylistReader(playlistUrl);
            VideoInfo video;

            _playlistName = reader.WaitForPlaylist().Name;

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

            this.CanDownloadPlaylist = result;

            if (!result)
            {
                this.PlaylistItems.Clear();
            }

            this.EnablePlaylistControls = true;
        }

        private void playlistOperation_FileDownloadComplete(object sender, string file)
        {
            if (AutoConvert.IsEnabled && AutoConvert.IsChecked == true)
            {
                string output = Path.Combine(Path.GetDirectoryName(file),
                    Path.GetFileNameWithoutExtension(file)) + ".mp3";

                var operation = this.Convert(file, output, false);

                operation.Completed += delegate { this.Queue.Remove(operation); };
            }
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
                var operation = new PlaylistOperation();

                this.Queue.Add(operation);
                this.SelectOneItem(operation);

                operation.FileDownloadComplete += playlistOperation_FileDownloadComplete;
                operation.Prepare(operation.Args(this.PlaylistLink.Text,
                                    path,
                                    settings.UseDashPlaylist,
                                    Settings.Default.PreferredQualityPlaylist,
                                    _playlistName,
                                    videos)
                                );

                TabControl.SelectedIndex = 3;

                DownloadQueueHandler.Add(operation);
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
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();

        string _convertInputFile;
        string _convertInputFolder;
        string _convertOutputFile;
        string _convertOutputFolder;

        private void FileFolderRadioButtons_Checked(object sender, RoutedEventArgs e)
        {
            // Stop event from running before radio buttons has been initialized
            if (ConvertInput == null || ConvertOutput == null)
                return;

            if (FileRadioButton.IsChecked == true)
            {
                // Store folder paths
                _convertInputFolder = ConvertInput.Text;
                _convertOutputFolder = ConvertOutput.Text;

                // Insert file paths
                ConvertInput.Text = _convertInputFile;
                ConvertOutput.Text = _convertOutputFile;
            }
            else
            {
                // Store file paths
                _convertInputFile = ConvertInput.Text;
                _convertOutputFile = ConvertOutput.Text;

                // Insert folder paths
                ConvertInput.Text = _convertInputFolder;
                ConvertOutput.Text = _convertOutputFolder;
            }
        }

        private void ConvertInputBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (FileRadioButton.IsChecked == true)
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
            else if (FolderRadioButton.IsChecked == true)
            {
                openFolderDialog.InitialFolder = Path.GetFileName(ConvertInput.Text);

                if (openFolderDialog.ShowDialog(this) == WinForms.DialogResult.OK)
                {
                    ConvertInput.Text = openFolderDialog.Folder;

                    if (string.IsNullOrEmpty(ConvertInput.Text))
                    {
                        // Suggest output path
                        ConvertInput.Text = ConvertInput.Text;
                    }
                }
            }
        }

        private void ConvertOutputBrowse_Click(object sender, RoutedEventArgs e)
        {
            if (FileRadioButton.IsChecked == true)
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
            else if (FolderRadioButton.IsChecked == true)
            {
                if (Directory.Exists(ConvertOutput.Text))
                    openFolderDialog.InitialFolder = Path.GetFileName(ConvertOutput.Text);

                if (openFolderDialog.ShowDialog(this) == WinForms.DialogResult.OK)
                {
                    ConvertOutput.Text = openFolderDialog.Folder;
                }
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            if (FileRadioButton.IsChecked == true)
            {
                this.ConvertFile();
            }
            else
            {
                this.ConvertFolder();
            }

            ConvertInput.Clear();
            ConvertOutput.Clear();

            TabControl.SelectedIndex = 3;
        }

        private void ConvertFile()
        {
            if (!FFmpegHelper.CanConvertToMP3(ConvertInput.Text).Value)
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
        }

        private void ConvertFolder()
        {
            if (!Directory.Exists(ConvertInput.Text))
                Directory.CreateDirectory(ConvertInput.Text);

            if (!Directory.Exists(ConvertOutput.Text))
                Directory.CreateDirectory(ConvertOutput.Text);

            this.ConvertFolder(ConvertInput.Text,
                               ConvertInput.Text,
                               ConvertExtension.Text);
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
            foreach (var operation in App.RunningOperations)
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
        private ConvertOperation Convert(string input, string output, bool crop)
        {
            TimeSpan start, end;
            start = end = TimeSpan.MinValue;

            if (crop && CropFrom.IsChecked == true)
            {
                // Validate cropping input. Shows error messages automatically.
                if (!this.ValidateCropping())
                    return null;

                start = TimeSpan.Parse(CropFromTextBox.Text);
                end = TimeSpan.Parse(CropToTextBox.Text);
            }

            var operation = new ConvertOperation();

            this.Queue.Add(operation);
            this.SelectOneItem(operation);

            operation.Prepare(operation.Args(input, output, start, end));

            DownloadQueueHandler.Add(operation);

            return operation;
        }

        /// <summary>
        /// Starts a ConvertOperation to convert all files in folder, matching extension.
        /// </summary>
        /// <param name="input">The folder to convert files from.</param>
        /// <param name="output">The output folder, where all converted files will be placed.</param>
        /// <param name="extension">The extension to match.</param>
        private void ConvertFolder(string input, string output, string extension)
        {
            var operation = new ConvertOperation();

            this.Queue.Add(operation);
            this.SelectOneItem(operation);

            operation.Prepare(operation.Args(input, output, extension));

            DownloadQueueHandler.Add(operation);
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

            var operation = new CroppingOperation();

            this.Queue.Add(operation);
            this.SelectOneItem(operation);

            operation.Prepare(operation.Args(input, output, start, end));

            DownloadQueueHandler.Add(operation);
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
                settings.WindowStates = new WPF_Classes.WindowStates();
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
