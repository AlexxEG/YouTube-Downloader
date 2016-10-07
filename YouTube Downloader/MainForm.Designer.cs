namespace YouTube_Downloader
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnPaste = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnGetVideo = new System.Windows.Forms.Button();
            this.txtYoutubeLink = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSaveTo = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.cbQuality = new System.Windows.Forms.ComboBox();
            this.bwGetVideo = new System.ComponentModel.BackgroundWorker();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.openMenuItem = new System.Windows.Forms.MenuItem();
            this.openContainingFolderMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.convertToMP3MenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.resumeMenuItem = new System.Windows.Forms.MenuItem();
            this.pauseMenuItem = new System.Windows.Forms.MenuItem();
            this.stopMenuItem = new System.Windows.Forms.MenuItem();
            this.removeMenuItem = new System.Windows.Forms.MenuItem();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.lFileSize = new System.Windows.Forms.Label();
            this.videoThumbnail = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.downloadTabPage = new System.Windows.Forms.TabPage();
            this.playlistTabPage = new System.Windows.Forms.TabPage();
            this.btnPlaylistDownloadSelected = new System.Windows.Forms.Button();
            this.lvPlaylistVideos = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnPlaylistPaste = new System.Windows.Forms.Button();
            this.btnGetPlaylist = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.cbPlaylistQuality = new System.Windows.Forms.ComboBox();
            this.btnPlaylistBrowse = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.cbPlaylistSaveTo = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtPlaylistLink = new System.Windows.Forms.TextBox();
            this.btnPlaylistDownloadAll = new System.Windows.Forms.Button();
            this.convertTabPage = new System.Windows.Forms.TabPage();
            this.btnCheckAgain = new System.Windows.Forms.Button();
            this.lFFmpegMissing = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.rbConvertFolder = new System.Windows.Forms.RadioButton();
            this.rbConvertFile = new System.Windows.Forms.RadioButton();
            this.pConvertFolder = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.txtExtension = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnBrowseOutputFolder = new System.Windows.Forms.Button();
            this.txtInputFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseInputFolder = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.pConvertFile = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.btnBrowseOutputFile = new System.Windows.Forms.Button();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.btnBrowseInputFile = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.gCropping = new System.Windows.Forms.GroupBox();
            this.chbCropFrom = new System.Windows.Forms.CheckBox();
            this.mtxtTo = new System.Windows.Forms.MaskedTextBox();
            this.chbCropTo = new System.Windows.Forms.CheckBox();
            this.mtxtFrom = new System.Windows.Forms.MaskedTextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.queueTabPage = new System.Windows.Forms.TabPage();
            this.chbAutoConvert = new System.Windows.Forms.CheckBox();
            this.lvQueue = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cmPlaylistList = new System.Windows.Forms.ContextMenu();
            this.playlistSelectAllMenuItem = new System.Windows.Forms.MenuItem();
            this.playlistSelectNoneMenuItem = new System.Windows.Forms.MenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoThumbnail)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.downloadTabPage.SuspendLayout();
            this.playlistTabPage.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.convertTabPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.pConvertFolder.SuspendLayout();
            this.pConvertFile.SuspendLayout();
            this.gCropping.SuspendLayout();
            this.queueTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.btnPaste);
            this.groupBox1.Controls.Add(this.btnGetVideo);
            this.groupBox1.Controls.Add(this.txtYoutubeLink);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 74);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Video Link";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(27, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Link";
            // 
            // btnPaste
            // 
            this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPaste.ImageIndex = 0;
            this.btnPaste.ImageList = this.imageList1;
            this.btnPaste.Location = new System.Drawing.Point(562, 18);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(22, 22);
            this.btnPaste.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnPaste, "Paste");
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Paste-64.png");
            // 
            // btnGetVideo
            // 
            this.btnGetVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetVideo.Location = new System.Drawing.Point(494, 45);
            this.btnGetVideo.Name = "btnGetVideo";
            this.btnGetVideo.Size = new System.Drawing.Size(90, 23);
            this.btnGetVideo.TabIndex = 2;
            this.btnGetVideo.Text = "Get";
            this.btnGetVideo.UseVisualStyleBackColor = true;
            this.btnGetVideo.Click += new System.EventHandler(this.btnGetVideo_Click);
            // 
            // txtYoutubeLink
            // 
            this.txtYoutubeLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYoutubeLink.Location = new System.Drawing.Point(39, 19);
            this.txtYoutubeLink.Name = "txtYoutubeLink";
            this.txtYoutubeLink.Size = new System.Drawing.Size(517, 20);
            this.txtYoutubeLink.TabIndex = 0;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(553, 73);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(31, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Save to";
            // 
            // cbSaveTo
            // 
            this.cbSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSaveTo.FormattingEnabled = true;
            this.cbSaveTo.Location = new System.Drawing.Point(182, 74);
            this.cbSaveTo.Name = "cbSaveTo";
            this.cbSaveTo.Size = new System.Drawing.Size(365, 21);
            this.cbSaveTo.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(132, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(27, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Title";
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.Enabled = false;
            this.btnDownload.Location = new System.Drawing.Point(494, 102);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(90, 23);
            this.btnDownload.TabIndex = 4;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // cbQuality
            // 
            this.cbQuality.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbQuality.Enabled = false;
            this.cbQuality.FormattingEnabled = true;
            this.cbQuality.Location = new System.Drawing.Point(182, 46);
            this.cbQuality.Name = "cbQuality";
            this.cbQuality.Size = new System.Drawing.Size(284, 21);
            this.cbQuality.TabIndex = 1;
            this.cbQuality.SelectedIndexChanged += new System.EventHandler(this.cbQuality_SelectedIndexChanged);
            // 
            // bwGetVideo
            // 
            this.bwGetVideo.WorkerSupportsCancellation = true;
            this.bwGetVideo.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwGetVideo_DoWork);
            this.bwGetVideo.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwGetVideo_RunWorkerCompleted);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.openMenuItem,
            this.openContainingFolderMenuItem,
            this.menuItem3,
            this.convertToMP3MenuItem,
            this.menuItem5,
            this.resumeMenuItem,
            this.pauseMenuItem,
            this.stopMenuItem,
            this.removeMenuItem});
            this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
            this.contextMenu1.Collapse += new System.EventHandler(this.contextMenu1_Collapse);
            // 
            // openMenuItem
            // 
            this.openMenuItem.Index = 0;
            this.openMenuItem.Text = "Open";
            this.openMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
            // 
            // openContainingFolderMenuItem
            // 
            this.openContainingFolderMenuItem.Index = 1;
            this.openContainingFolderMenuItem.Text = "Open Containing Folder";
            this.openContainingFolderMenuItem.Click += new System.EventHandler(this.openContainingFolderMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // convertToMP3MenuItem
            // 
            this.convertToMP3MenuItem.Index = 3;
            this.convertToMP3MenuItem.Text = "Convert to MP3";
            this.convertToMP3MenuItem.Click += new System.EventHandler(this.convertToMP3MenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 4;
            this.menuItem5.Text = "-";
            // 
            // resumeMenuItem
            // 
            this.resumeMenuItem.Index = 5;
            this.resumeMenuItem.Text = "Resume";
            this.resumeMenuItem.Click += new System.EventHandler(this.resumeMenuItem_Click);
            // 
            // pauseMenuItem
            // 
            this.pauseMenuItem.Index = 6;
            this.pauseMenuItem.Text = "Pause";
            this.pauseMenuItem.Click += new System.EventHandler(this.pauseMenuItem_Click);
            // 
            // stopMenuItem
            // 
            this.stopMenuItem.Index = 7;
            this.stopMenuItem.Text = "Stop";
            this.stopMenuItem.Click += new System.EventHandler(this.stopMenuItem_Click);
            // 
            // removeMenuItem
            // 
            this.removeMenuItem.Index = 8;
            this.removeMenuItem.Text = "Remove";
            this.removeMenuItem.Click += new System.EventHandler(this.removeMenuItem_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.txtTitle);
            this.groupBox3.Controls.Add(this.lFileSize);
            this.groupBox3.Controls.Add(this.videoThumbnail);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.btnBrowse);
            this.groupBox3.Controls.Add(this.cbQuality);
            this.groupBox3.Controls.Add(this.btnDownload);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.cbSaveTo);
            this.groupBox3.Location = new System.Drawing.Point(6, 86);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(590, 131);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Video Information";
            // 
            // txtTitle
            // 
            this.txtTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitle.Location = new System.Drawing.Point(182, 20);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(365, 20);
            this.txtTitle.TabIndex = 0;
            // 
            // lFileSize
            // 
            this.lFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lFileSize.AutoSize = true;
            this.lFileSize.Location = new System.Drawing.Point(472, 49);
            this.lFileSize.Name = "lFileSize";
            this.lFileSize.Size = new System.Drawing.Size(44, 13);
            this.lFileSize.TabIndex = 11;
            this.lFileSize.Text = "File size";
            // 
            // videoThumbnail
            // 
            this.videoThumbnail.BackColor = System.Drawing.Color.Gainsboro;
            this.videoThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.videoThumbnail.Location = new System.Drawing.Point(6, 22);
            this.videoThumbnail.Name = "videoThumbnail";
            this.videoThumbnail.Size = new System.Drawing.Size(120, 68);
            this.videoThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoThumbnail.TabIndex = 4;
            this.videoThumbnail.TabStop = false;
            this.videoThumbnail.Paint += new System.Windows.Forms.PaintEventHandler(this.videoThumbnail_Paint);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(132, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Quality";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.downloadTabPage);
            this.tabControl1.Controls.Add(this.playlistTabPage);
            this.tabControl1.Controls.Add(this.convertTabPage);
            this.tabControl1.Controls.Add(this.queueTabPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(610, 387);
            this.tabControl1.TabIndex = 0;
            // 
            // downloadTabPage
            // 
            this.downloadTabPage.Controls.Add(this.groupBox1);
            this.downloadTabPage.Controls.Add(this.groupBox3);
            this.downloadTabPage.Location = new System.Drawing.Point(4, 22);
            this.downloadTabPage.Name = "downloadTabPage";
            this.downloadTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.downloadTabPage.Size = new System.Drawing.Size(602, 361);
            this.downloadTabPage.TabIndex = 0;
            this.downloadTabPage.Text = "Download";
            this.downloadTabPage.UseVisualStyleBackColor = true;
            // 
            // playlistTabPage
            // 
            this.playlistTabPage.Controls.Add(this.btnPlaylistDownloadSelected);
            this.playlistTabPage.Controls.Add(this.lvPlaylistVideos);
            this.playlistTabPage.Controls.Add(this.groupBox7);
            this.playlistTabPage.Controls.Add(this.btnPlaylistDownloadAll);
            this.playlistTabPage.Location = new System.Drawing.Point(4, 22);
            this.playlistTabPage.Name = "playlistTabPage";
            this.playlistTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.playlistTabPage.Size = new System.Drawing.Size(602, 361);
            this.playlistTabPage.TabIndex = 4;
            this.playlistTabPage.Text = "Playlist";
            this.playlistTabPage.UseVisualStyleBackColor = true;
            // 
            // btnPlaylistDownloadSelected
            // 
            this.btnPlaylistDownloadSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistDownloadSelected.Enabled = false;
            this.btnPlaylistDownloadSelected.Location = new System.Drawing.Point(385, 332);
            this.btnPlaylistDownloadSelected.Name = "btnPlaylistDownloadSelected";
            this.btnPlaylistDownloadSelected.Size = new System.Drawing.Size(115, 23);
            this.btnPlaylistDownloadSelected.TabIndex = 1;
            this.btnPlaylistDownloadSelected.Text = "Download Selected";
            this.btnPlaylistDownloadSelected.UseVisualStyleBackColor = true;
            this.btnPlaylistDownloadSelected.Click += new System.EventHandler(this.btnPlaylistDownloadSelected_Click);
            // 
            // lvPlaylistVideos
            // 
            this.lvPlaylistVideos.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvPlaylistVideos.CheckBoxes = true;
            this.lvPlaylistVideos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7,
            this.columnHeader8});
            this.lvPlaylistVideos.FullRowSelect = true;
            this.lvPlaylistVideos.Location = new System.Drawing.Point(6, 143);
            this.lvPlaylistVideos.Name = "lvPlaylistVideos";
            this.lvPlaylistVideos.Size = new System.Drawing.Size(590, 183);
            this.lvPlaylistVideos.TabIndex = 0;
            this.lvPlaylistVideos.UseCompatibleStateImageBehavior = false;
            this.lvPlaylistVideos.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Title";
            this.columnHeader7.Width = 500;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Duration";
            this.columnHeader8.Width = 80;
            // 
            // groupBox7
            // 
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox7.Controls.Add(this.btnPlaylistPaste);
            this.groupBox7.Controls.Add(this.btnGetPlaylist);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.cbPlaylistQuality);
            this.groupBox7.Controls.Add(this.btnPlaylistBrowse);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.cbPlaylistSaveTo);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Controls.Add(this.txtPlaylistLink);
            this.groupBox7.Location = new System.Drawing.Point(6, 6);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(590, 131);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Playlist Link";
            // 
            // btnPlaylistPaste
            // 
            this.btnPlaylistPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistPaste.ImageIndex = 0;
            this.btnPlaylistPaste.ImageList = this.imageList1;
            this.btnPlaylistPaste.Location = new System.Drawing.Point(562, 18);
            this.btnPlaylistPaste.Name = "btnPlaylistPaste";
            this.btnPlaylistPaste.Size = new System.Drawing.Size(22, 22);
            this.btnPlaylistPaste.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnPlaylistPaste, "Paste");
            this.btnPlaylistPaste.UseVisualStyleBackColor = true;
            this.btnPlaylistPaste.Click += new System.EventHandler(this.btnPlaylistPaste_Click);
            // 
            // btnGetPlaylist
            // 
            this.btnGetPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetPlaylist.Location = new System.Drawing.Point(494, 102);
            this.btnGetPlaylist.Name = "btnGetPlaylist";
            this.btnGetPlaylist.Size = new System.Drawing.Size(90, 23);
            this.btnGetPlaylist.TabIndex = 5;
            this.btnGetPlaylist.Text = "Get";
            this.btnGetPlaylist.UseVisualStyleBackColor = true;
            this.btnGetPlaylist.Click += new System.EventHandler(this.btnGetPlaylist_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 48);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(39, 13);
            this.label14.TabIndex = 13;
            this.label14.Text = "Quality";
            // 
            // cbPlaylistQuality
            // 
            this.cbPlaylistQuality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPlaylistQuality.FormattingEnabled = true;
            this.cbPlaylistQuality.Items.AddRange(new object[] {
            "Highest",
            "Medium",
            "Low"});
            this.cbPlaylistQuality.Location = new System.Drawing.Point(60, 45);
            this.cbPlaylistQuality.Name = "cbPlaylistQuality";
            this.cbPlaylistQuality.Size = new System.Drawing.Size(456, 21);
            this.cbPlaylistQuality.TabIndex = 2;
            this.cbPlaylistQuality.SelectedIndexChanged += new System.EventHandler(this.cbPlaylistQuality_SelectedIndexChanged);
            // 
            // btnPlaylistBrowse
            // 
            this.btnPlaylistBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistBrowse.Location = new System.Drawing.Point(553, 71);
            this.btnPlaylistBrowse.Name = "btnPlaylistBrowse";
            this.btnPlaylistBrowse.Size = new System.Drawing.Size(31, 23);
            this.btnPlaylistBrowse.TabIndex = 4;
            this.btnPlaylistBrowse.Text = "...";
            this.btnPlaylistBrowse.UseVisualStyleBackColor = true;
            this.btnPlaylistBrowse.Click += new System.EventHandler(this.btnPlaylistBrowse_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 76);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(44, 13);
            this.label13.TabIndex = 10;
            this.label13.Text = "Save to";
            // 
            // cbPlaylistSaveTo
            // 
            this.cbPlaylistSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPlaylistSaveTo.FormattingEnabled = true;
            this.cbPlaylistSaveTo.Location = new System.Drawing.Point(60, 72);
            this.cbPlaylistSaveTo.Name = "cbPlaylistSaveTo";
            this.cbPlaylistSaveTo.Size = new System.Drawing.Size(487, 21);
            this.cbPlaylistSaveTo.TabIndex = 3;
            this.cbPlaylistSaveTo.SelectedIndexChanged += new System.EventHandler(this.cbPlaylistSaveTo_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(6, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(27, 13);
            this.label12.TabIndex = 5;
            this.label12.Text = "Link";
            // 
            // txtPlaylistLink
            // 
            this.txtPlaylistLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPlaylistLink.Location = new System.Drawing.Point(60, 19);
            this.txtPlaylistLink.Name = "txtPlaylistLink";
            this.txtPlaylistLink.Size = new System.Drawing.Size(496, 20);
            this.txtPlaylistLink.TabIndex = 0;
            this.txtPlaylistLink.TextChanged += new System.EventHandler(this.txtPlaylistLink_TextChanged);
            // 
            // btnPlaylistDownloadAll
            // 
            this.btnPlaylistDownloadAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistDownloadAll.Enabled = false;
            this.btnPlaylistDownloadAll.Location = new System.Drawing.Point(506, 332);
            this.btnPlaylistDownloadAll.Name = "btnPlaylistDownloadAll";
            this.btnPlaylistDownloadAll.Size = new System.Drawing.Size(90, 23);
            this.btnPlaylistDownloadAll.TabIndex = 2;
            this.btnPlaylistDownloadAll.Text = "Download All";
            this.btnPlaylistDownloadAll.UseVisualStyleBackColor = true;
            this.btnPlaylistDownloadAll.Click += new System.EventHandler(this.btnPlaylistDownloadAll_Click);
            // 
            // convertTabPage
            // 
            this.convertTabPage.Controls.Add(this.btnCheckAgain);
            this.convertTabPage.Controls.Add(this.lFFmpegMissing);
            this.convertTabPage.Controls.Add(this.groupBox2);
            this.convertTabPage.Location = new System.Drawing.Point(4, 22);
            this.convertTabPage.Name = "convertTabPage";
            this.convertTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.convertTabPage.Size = new System.Drawing.Size(602, 361);
            this.convertTabPage.TabIndex = 2;
            this.convertTabPage.Text = "Convert";
            this.convertTabPage.UseVisualStyleBackColor = true;
            // 
            // btnCheckAgain
            // 
            this.btnCheckAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckAgain.Location = new System.Drawing.Point(490, 286);
            this.btnCheckAgain.Name = "btnCheckAgain";
            this.btnCheckAgain.Size = new System.Drawing.Size(106, 23);
            this.btnCheckAgain.TabIndex = 0;
            this.btnCheckAgain.Text = "Check Again";
            this.btnCheckAgain.UseVisualStyleBackColor = true;
            this.btnCheckAgain.Visible = false;
            this.btnCheckAgain.Click += new System.EventHandler(this.btnCheckAgain_Click);
            // 
            // lFFmpegMissing
            // 
            this.lFFmpegMissing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lFFmpegMissing.AutoSize = true;
            this.lFFmpegMissing.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lFFmpegMissing.ForeColor = System.Drawing.Color.Red;
            this.lFFmpegMissing.Location = new System.Drawing.Point(6, 286);
            this.lFFmpegMissing.Name = "lFFmpegMissing";
            this.lFFmpegMissing.Size = new System.Drawing.Size(429, 20);
            this.lFFmpegMissing.TabIndex = 10;
            this.lFFmpegMissing.Text = "\'FFmpeg.exe\' was not found. Converting/Cutting is disabled.";
            this.lFFmpegMissing.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox5);
            this.groupBox2.Controls.Add(this.gCropping);
            this.groupBox2.Controls.Add(this.btnConvert);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(590, 274);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Convert to MP3";
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.rbConvertFolder);
            this.groupBox5.Controls.Add(this.rbConvertFile);
            this.groupBox5.Controls.Add(this.pConvertFolder);
            this.groupBox5.Controls.Add(this.pConvertFile);
            this.groupBox5.Location = new System.Drawing.Point(6, 19);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(578, 109);
            this.groupBox5.TabIndex = 8;
            this.groupBox5.TabStop = false;
            // 
            // rbConvertFolder
            // 
            this.rbConvertFolder.AutoSize = true;
            this.rbConvertFolder.BackColor = System.Drawing.SystemColors.Window;
            this.rbConvertFolder.Location = new System.Drawing.Point(55, -2);
            this.rbConvertFolder.Name = "rbConvertFolder";
            this.rbConvertFolder.Size = new System.Drawing.Size(54, 17);
            this.rbConvertFolder.TabIndex = 1;
            this.rbConvertFolder.Text = "Folder";
            this.rbConvertFolder.UseVisualStyleBackColor = false;
            this.rbConvertFolder.CheckedChanged += new System.EventHandler(this.ConvertRadioButtons_CheckedChanged);
            // 
            // rbConvertFile
            // 
            this.rbConvertFile.AutoSize = true;
            this.rbConvertFile.BackColor = System.Drawing.SystemColors.Window;
            this.rbConvertFile.Checked = true;
            this.rbConvertFile.Location = new System.Drawing.Point(8, -2);
            this.rbConvertFile.Name = "rbConvertFile";
            this.rbConvertFile.Size = new System.Drawing.Size(41, 17);
            this.rbConvertFile.TabIndex = 0;
            this.rbConvertFile.TabStop = true;
            this.rbConvertFile.Text = "File";
            this.rbConvertFile.UseVisualStyleBackColor = false;
            this.rbConvertFile.CheckedChanged += new System.EventHandler(this.ConvertRadioButtons_CheckedChanged);
            // 
            // pConvertFolder
            // 
            this.pConvertFolder.Controls.Add(this.label9);
            this.pConvertFolder.Controls.Add(this.txtExtension);
            this.pConvertFolder.Controls.Add(this.label7);
            this.pConvertFolder.Controls.Add(this.btnBrowseOutputFolder);
            this.pConvertFolder.Controls.Add(this.txtInputFolder);
            this.pConvertFolder.Controls.Add(this.btnBrowseInputFolder);
            this.pConvertFolder.Controls.Add(this.label8);
            this.pConvertFolder.Controls.Add(this.txtOutputFolder);
            this.pConvertFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pConvertFolder.Location = new System.Drawing.Point(3, 16);
            this.pConvertFolder.Name = "pConvertFolder";
            this.pConvertFolder.Padding = new System.Windows.Forms.Padding(3);
            this.pConvertFolder.Size = new System.Drawing.Size(572, 90);
            this.pConvertFolder.TabIndex = 3;
            this.pConvertFolder.Visible = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 67);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "Extension filter";
            // 
            // txtExtension
            // 
            this.txtExtension.Location = new System.Drawing.Point(87, 64);
            this.txtExtension.Name = "txtExtension";
            this.txtExtension.Size = new System.Drawing.Size(100, 20);
            this.txtExtension.TabIndex = 4;
            this.txtExtension.Text = "mp4";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(31, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Input";
            // 
            // btnBrowseOutputFolder
            // 
            this.btnBrowseOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputFolder.Location = new System.Drawing.Point(542, 31);
            this.btnBrowseOutputFolder.Name = "btnBrowseOutputFolder";
            this.btnBrowseOutputFolder.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseOutputFolder.TabIndex = 3;
            this.btnBrowseOutputFolder.Text = "...";
            this.btnBrowseOutputFolder.UseVisualStyleBackColor = true;
            this.btnBrowseOutputFolder.Click += new System.EventHandler(this.btnBrowseOutputFolder_Click);
            // 
            // txtInputFolder
            // 
            this.txtInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputFolder.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtInputFolder.Location = new System.Drawing.Point(51, 6);
            this.txtInputFolder.Name = "txtInputFolder";
            this.txtInputFolder.ReadOnly = true;
            this.txtInputFolder.Size = new System.Drawing.Size(485, 20);
            this.txtInputFolder.TabIndex = 0;
            // 
            // btnBrowseInputFolder
            // 
            this.btnBrowseInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseInputFolder.Location = new System.Drawing.Point(542, 5);
            this.btnBrowseInputFolder.Name = "btnBrowseInputFolder";
            this.btnBrowseInputFolder.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseInputFolder.TabIndex = 1;
            this.btnBrowseInputFolder.Text = "...";
            this.btnBrowseInputFolder.UseVisualStyleBackColor = true;
            this.btnBrowseInputFolder.Click += new System.EventHandler(this.btnBrowseInputFolder_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 35);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(39, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Output";
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputFolder.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtOutputFolder.Location = new System.Drawing.Point(51, 32);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.ReadOnly = true;
            this.txtOutputFolder.Size = new System.Drawing.Size(485, 20);
            this.txtOutputFolder.TabIndex = 2;
            // 
            // pConvertFile
            // 
            this.pConvertFile.Controls.Add(this.label5);
            this.pConvertFile.Controls.Add(this.btnBrowseOutputFile);
            this.pConvertFile.Controls.Add(this.txtInputFile);
            this.pConvertFile.Controls.Add(this.btnBrowseInputFile);
            this.pConvertFile.Controls.Add(this.label6);
            this.pConvertFile.Controls.Add(this.txtOutputFile);
            this.pConvertFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pConvertFile.Location = new System.Drawing.Point(3, 16);
            this.pConvertFile.Name = "pConvertFile";
            this.pConvertFile.Padding = new System.Windows.Forms.Padding(3);
            this.pConvertFile.Size = new System.Drawing.Size(572, 90);
            this.pConvertFile.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Input";
            // 
            // btnBrowseOutputFile
            // 
            this.btnBrowseOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutputFile.Location = new System.Drawing.Point(542, 31);
            this.btnBrowseOutputFile.Name = "btnBrowseOutputFile";
            this.btnBrowseOutputFile.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseOutputFile.TabIndex = 7;
            this.btnBrowseOutputFile.Text = "...";
            this.btnBrowseOutputFile.UseVisualStyleBackColor = true;
            this.btnBrowseOutputFile.Click += new System.EventHandler(this.btnBrowseOutputFile_Click);
            // 
            // txtInputFile
            // 
            this.txtInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInputFile.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtInputFile.Location = new System.Drawing.Point(51, 6);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.ReadOnly = true;
            this.txtInputFile.Size = new System.Drawing.Size(485, 20);
            this.txtInputFile.TabIndex = 2;
            // 
            // btnBrowseInputFile
            // 
            this.btnBrowseInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseInputFile.Location = new System.Drawing.Point(542, 5);
            this.btnBrowseInputFile.Name = "btnBrowseInputFile";
            this.btnBrowseInputFile.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseInputFile.TabIndex = 6;
            this.btnBrowseInputFile.Text = "...";
            this.btnBrowseInputFile.UseVisualStyleBackColor = true;
            this.btnBrowseInputFile.Click += new System.EventHandler(this.btnBrowseInputFile_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Output";
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputFile.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtOutputFile.Location = new System.Drawing.Point(51, 32);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.ReadOnly = true;
            this.txtOutputFile.Size = new System.Drawing.Size(485, 20);
            this.txtOutputFile.TabIndex = 3;
            // 
            // gCropping
            // 
            this.gCropping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gCropping.Controls.Add(this.chbCropFrom);
            this.gCropping.Controls.Add(this.mtxtTo);
            this.gCropping.Controls.Add(this.chbCropTo);
            this.gCropping.Controls.Add(this.mtxtFrom);
            this.gCropping.Location = new System.Drawing.Point(6, 204);
            this.gCropping.Name = "gCropping";
            this.gCropping.Size = new System.Drawing.Size(372, 64);
            this.gCropping.TabIndex = 0;
            this.gCropping.TabStop = false;
            this.gCropping.Text = "Crop";
            // 
            // chbCropFrom
            // 
            this.chbCropFrom.AutoSize = true;
            this.chbCropFrom.Location = new System.Drawing.Point(6, 21);
            this.chbCropFrom.Name = "chbCropFrom";
            this.chbCropFrom.Size = new System.Drawing.Size(74, 17);
            this.chbCropFrom.TabIndex = 0;
            this.chbCropFrom.Text = "Crop From";
            this.chbCropFrom.UseVisualStyleBackColor = true;
            this.chbCropFrom.CheckedChanged += new System.EventHandler(this.chbCropFrom_CheckedChanged);
            // 
            // mtxtTo
            // 
            this.mtxtTo.Enabled = false;
            this.mtxtTo.Location = new System.Drawing.Point(237, 19);
            this.mtxtTo.Mask = "00:00:00.000";
            this.mtxtTo.Name = "mtxtTo";
            this.mtxtTo.Size = new System.Drawing.Size(100, 20);
            this.mtxtTo.TabIndex = 3;
            this.mtxtTo.Text = "000000000";
            this.mtxtTo.ValidatingType = typeof(System.TimeSpan);
            // 
            // chbCropTo
            // 
            this.chbCropTo.AutoSize = true;
            this.chbCropTo.Enabled = false;
            this.chbCropTo.Location = new System.Drawing.Point(192, 21);
            this.chbCropTo.Name = "chbCropTo";
            this.chbCropTo.Size = new System.Drawing.Size(39, 17);
            this.chbCropTo.TabIndex = 2;
            this.chbCropTo.Text = "To";
            this.chbCropTo.UseVisualStyleBackColor = true;
            this.chbCropTo.CheckedChanged += new System.EventHandler(this.chbCropTo_CheckedChanged);
            // 
            // mtxtFrom
            // 
            this.mtxtFrom.Enabled = false;
            this.mtxtFrom.Location = new System.Drawing.Point(86, 19);
            this.mtxtFrom.Mask = "00:00:00.000";
            this.mtxtFrom.Name = "mtxtFrom";
            this.mtxtFrom.Size = new System.Drawing.Size(100, 20);
            this.mtxtFrom.TabIndex = 1;
            this.mtxtFrom.Text = "000000000";
            this.mtxtFrom.ValidatingType = typeof(System.TimeSpan);
            // 
            // btnConvert
            // 
            this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvert.Location = new System.Drawing.Point(509, 245);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 1;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // queueTabPage
            // 
            this.queueTabPage.Controls.Add(this.chbAutoConvert);
            this.queueTabPage.Controls.Add(this.lvQueue);
            this.queueTabPage.Location = new System.Drawing.Point(4, 22);
            this.queueTabPage.Name = "queueTabPage";
            this.queueTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.queueTabPage.Size = new System.Drawing.Size(602, 361);
            this.queueTabPage.TabIndex = 1;
            this.queueTabPage.Text = "Queue";
            this.queueTabPage.UseVisualStyleBackColor = true;
            // 
            // chbAutoConvert
            // 
            this.chbAutoConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chbAutoConvert.AutoSize = true;
            this.chbAutoConvert.Location = new System.Drawing.Point(6, 338);
            this.chbAutoConvert.Name = "chbAutoConvert";
            this.chbAutoConvert.Size = new System.Drawing.Size(164, 17);
            this.chbAutoConvert.TabIndex = 1;
            this.chbAutoConvert.Text = "Convert to MP3 automatically";
            this.chbAutoConvert.UseVisualStyleBackColor = true;
            // 
            // lvQueue
            // 
            this.lvQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvQueue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lvQueue.FullRowSelect = true;
            this.lvQueue.Location = new System.Drawing.Point(6, 6);
            this.lvQueue.Name = "lvQueue";
            this.lvQueue.Size = new System.Drawing.Size(590, 326);
            this.lvQueue.TabIndex = 0;
            this.lvQueue.UseCompatibleStateImageBehavior = false;
            this.lvQueue.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Video";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Progress";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Length";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Size";
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Input";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "MP4 files|*.mp4|MP3 files|*.mp3|All files|*.*";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "MP3 files|*.mp3|MP4 files|*.mp4|All files|*.*";
            // 
            // cmPlaylistList
            // 
            this.cmPlaylistList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.playlistSelectAllMenuItem,
            this.playlistSelectNoneMenuItem});
            // 
            // playlistSelectAllMenuItem
            // 
            this.playlistSelectAllMenuItem.Index = 0;
            this.playlistSelectAllMenuItem.Text = "Select all";
            this.playlistSelectAllMenuItem.Click += new System.EventHandler(this.playlistSelectAllMenuItem_Click);
            // 
            // playlistSelectNoneMenuItem
            // 
            this.playlistSelectNoneMenuItem.Index = 1;
            this.playlistSelectNoneMenuItem.Text = "Select none";
            this.playlistSelectNoneMenuItem.Click += new System.EventHandler(this.playlistSelectNoneMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 411);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(644, 425);
            this.Name = "MainForm";
            this.Text = "YouTube Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoThumbnail)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.downloadTabPage.ResumeLayout(false);
            this.playlistTabPage.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.convertTabPage.ResumeLayout(false);
            this.convertTabPage.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.pConvertFolder.ResumeLayout(false);
            this.pConvertFolder.PerformLayout();
            this.pConvertFile.ResumeLayout(false);
            this.pConvertFile.PerformLayout();
            this.gCropping.ResumeLayout(false);
            this.gCropping.PerformLayout();
            this.queueTabPage.ResumeLayout(false);
            this.queueTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGetVideo;
        private System.Windows.Forms.ComboBox cbQuality;
        private System.Windows.Forms.TextBox txtYoutubeLink;
        private System.Windows.Forms.Button btnPaste;
        private ListViewEmbeddedControls.ListViewEx lvQueue;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.ComponentModel.BackgroundWorker bwGetVideo;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbSaveTo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem openMenuItem;
        private System.Windows.Forms.MenuItem openContainingFolderMenuItem;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem convertToMP3MenuItem;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem resumeMenuItem;
        private System.Windows.Forms.MenuItem pauseMenuItem;
        private System.Windows.Forms.MenuItem stopMenuItem;
        private System.Windows.Forms.MenuItem removeMenuItem;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox videoThumbnail;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage downloadTabPage;
        private System.Windows.Forms.TabPage queueTabPage;
        private System.Windows.Forms.TabPage convertTabPage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox gCropping;
        private System.Windows.Forms.CheckBox chbCropFrom;
        private System.Windows.Forms.MaskedTextBox mtxtFrom;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox mtxtTo;
        private System.Windows.Forms.CheckBox chbCropTo;
        private System.Windows.Forms.Button btnBrowseOutputFile;
        private System.Windows.Forms.Button btnBrowseInputFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox chbAutoConvert;
        private System.Windows.Forms.Label lFFmpegMissing;
        private System.Windows.Forms.Button btnCheckAgain;
        private System.Windows.Forms.Label lFileSize;
        private System.Windows.Forms.TabPage playlistTabPage;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button btnPlaylistBrowse;
        private System.Windows.Forms.Button btnPlaylistDownloadAll;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox cbPlaylistSaveTo;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtPlaylistLink;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox cbPlaylistQuality;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.ListView lvPlaylistVideos;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.Button btnPlaylistDownloadSelected;
        private System.Windows.Forms.Button btnGetPlaylist;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ContextMenu cmPlaylistList;
        private System.Windows.Forms.MenuItem playlistSelectAllMenuItem;
        private System.Windows.Forms.MenuItem playlistSelectNoneMenuItem;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Panel pConvertFolder;
        private System.Windows.Forms.Panel pConvertFile;
        private System.Windows.Forms.RadioButton rbConvertFolder;
        private System.Windows.Forms.RadioButton rbConvertFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnBrowseOutputFolder;
        private System.Windows.Forms.TextBox txtInputFolder;
        private System.Windows.Forms.Button btnBrowseInputFolder;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtExtension;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnPlaylistPaste;
    }
}

