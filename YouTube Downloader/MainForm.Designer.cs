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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnPaste = new System.Windows.Forms.Button();
            this.btnGetVideo = new System.Windows.Forms.Button();
            this.txtYoutubeLink = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbSaveTo = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.cbQuality = new System.Windows.Forms.ComboBox();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.btnExit = new System.Windows.Forms.Button();
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
            this.lvPlaylistVideos = new System.Windows.Forms.ListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.btnPlaylistDownloadSelected = new System.Windows.Forms.Button();
            this.btnGetPlaylist = new System.Windows.Forms.Button();
            this.chbPlaylistDASH = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.cbPlaylistQuality = new System.Windows.Forms.ComboBox();
            this.btnPlaylistBrowse = new System.Windows.Forms.Button();
            this.btnPlaylistDownloadAll = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.cbPlaylistSaveTo = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtPlaylistLink = new System.Windows.Forms.TextBox();
            this.convertTabPage = new System.Windows.Forms.TabPage();
            this.btnCheckAgain = new System.Windows.Forms.Button();
            this.lFFmpegMissing = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.chbCropFrom = new System.Windows.Forms.CheckBox();
            this.mtxtTo = new System.Windows.Forms.MaskedTextBox();
            this.chbCropTo = new System.Windows.Forms.CheckBox();
            this.mtxtFrom = new System.Windows.Forms.MaskedTextBox();
            this.btnConvert = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.queueTabPage = new System.Windows.Forms.TabPage();
            this.chbAutoConvert = new System.Windows.Forms.CheckBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.cmPlaylistList = new System.Windows.Forms.ContextMenu();
            this.playlistSelectAllMenuItem = new System.Windows.Forms.MenuItem();
            this.playlistSelectNoneMenuItem = new System.Windows.Forms.MenuItem();
            this.lvQueue = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoThumbnail)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.downloadTabPage.SuspendLayout();
            this.playlistTabPage.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.convertTabPage.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
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
            this.groupBox1.Size = new System.Drawing.Size(584, 74);
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
            this.btnPaste.Location = new System.Drawing.Point(422, 45);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(75, 23);
            this.btnPaste.TabIndex = 2;
            this.btnPaste.Text = "Paste";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // btnGetVideo
            // 
            this.btnGetVideo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetVideo.Location = new System.Drawing.Point(503, 45);
            this.btnGetVideo.Name = "btnGetVideo";
            this.btnGetVideo.Size = new System.Drawing.Size(75, 23);
            this.btnGetVideo.TabIndex = 2;
            this.btnGetVideo.Text = "Get Video";
            this.btnGetVideo.UseVisualStyleBackColor = true;
            this.btnGetVideo.Click += new System.EventHandler(this.btnGetVideo_Click);
            // 
            // txtYoutubeLink
            // 
            this.txtYoutubeLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtYoutubeLink.Location = new System.Drawing.Point(39, 19);
            this.txtYoutubeLink.Name = "txtYoutubeLink";
            this.txtYoutubeLink.Size = new System.Drawing.Size(539, 20);
            this.txtYoutubeLink.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowse.Location = new System.Drawing.Point(547, 73);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(31, 23);
            this.btnBrowse.TabIndex = 7;
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
            this.cbSaveTo.Size = new System.Drawing.Size(359, 21);
            this.cbSaveTo.TabIndex = 5;
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
            this.btnDownload.Location = new System.Drawing.Point(503, 102);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 23);
            this.btnDownload.TabIndex = 3;
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
            this.cbQuality.Size = new System.Drawing.Size(278, 21);
            this.cbQuality.TabIndex = 1;
            this.cbQuality.SelectedIndexChanged += new System.EventHandler(this.cbQuality_SelectedIndexChanged);
            // 
            // bottomPanel
            // 
            this.bottomPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.bottomPanel.Controls.Add(this.btnExit);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 341);
            this.bottomPanel.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Padding = new System.Windows.Forms.Padding(5);
            this.bottomPanel.Size = new System.Drawing.Size(628, 46);
            this.bottomPanel.TabIndex = 1;
            this.bottomPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.bottomPanel_Paint);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(541, 11);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
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
            this.groupBox3.Size = new System.Drawing.Size(584, 131);
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
            this.txtTitle.Size = new System.Drawing.Size(359, 20);
            this.txtTitle.TabIndex = 12;
            // 
            // lFileSize
            // 
            this.lFileSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lFileSize.AutoSize = true;
            this.lFileSize.Location = new System.Drawing.Point(466, 49);
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
            this.tabControl1.Size = new System.Drawing.Size(604, 317);
            this.tabControl1.TabIndex = 9;
            // 
            // downloadTabPage
            // 
            this.downloadTabPage.Controls.Add(this.groupBox1);
            this.downloadTabPage.Controls.Add(this.groupBox3);
            this.downloadTabPage.Location = new System.Drawing.Point(4, 22);
            this.downloadTabPage.Name = "downloadTabPage";
            this.downloadTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.downloadTabPage.Size = new System.Drawing.Size(596, 291);
            this.downloadTabPage.TabIndex = 0;
            this.downloadTabPage.Text = "Download";
            this.downloadTabPage.UseVisualStyleBackColor = true;
            // 
            // playlistTabPage
            // 
            this.playlistTabPage.Controls.Add(this.lvPlaylistVideos);
            this.playlistTabPage.Controls.Add(this.groupBox7);
            this.playlistTabPage.Location = new System.Drawing.Point(4, 22);
            this.playlistTabPage.Name = "playlistTabPage";
            this.playlistTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.playlistTabPage.Size = new System.Drawing.Size(596, 291);
            this.playlistTabPage.TabIndex = 4;
            this.playlistTabPage.Text = "Playlist";
            this.playlistTabPage.UseVisualStyleBackColor = true;
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
            this.lvPlaylistVideos.Location = new System.Drawing.Point(6, 171);
            this.lvPlaylistVideos.Name = "lvPlaylistVideos";
            this.lvPlaylistVideos.Size = new System.Drawing.Size(584, 114);
            this.lvPlaylistVideos.TabIndex = 1;
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
            this.groupBox7.Controls.Add(this.btnPlaylistDownloadSelected);
            this.groupBox7.Controls.Add(this.btnGetPlaylist);
            this.groupBox7.Controls.Add(this.chbPlaylistDASH);
            this.groupBox7.Controls.Add(this.label14);
            this.groupBox7.Controls.Add(this.cbPlaylistQuality);
            this.groupBox7.Controls.Add(this.btnPlaylistBrowse);
            this.groupBox7.Controls.Add(this.btnPlaylistDownloadAll);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Controls.Add(this.cbPlaylistSaveTo);
            this.groupBox7.Controls.Add(this.label12);
            this.groupBox7.Controls.Add(this.txtPlaylistLink);
            this.groupBox7.Location = new System.Drawing.Point(6, 6);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(584, 159);
            this.groupBox7.TabIndex = 0;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Playlist Link";
            // 
            // btnPlaylistDownloadSelected
            // 
            this.btnPlaylistDownloadSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistDownloadSelected.Enabled = false;
            this.btnPlaylistDownloadSelected.Location = new System.Drawing.Point(367, 130);
            this.btnPlaylistDownloadSelected.Name = "btnPlaylistDownloadSelected";
            this.btnPlaylistDownloadSelected.Size = new System.Drawing.Size(115, 23);
            this.btnPlaylistDownloadSelected.TabIndex = 16;
            this.btnPlaylistDownloadSelected.Text = "Download Selected";
            this.btnPlaylistDownloadSelected.UseVisualStyleBackColor = true;
            this.btnPlaylistDownloadSelected.Click += new System.EventHandler(this.btnPlaylistDownloadSelected_Click);
            // 
            // btnGetPlaylist
            // 
            this.btnGetPlaylist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetPlaylist.Location = new System.Drawing.Point(281, 130);
            this.btnGetPlaylist.Name = "btnGetPlaylist";
            this.btnGetPlaylist.Size = new System.Drawing.Size(80, 23);
            this.btnGetPlaylist.TabIndex = 15;
            this.btnGetPlaylist.Text = "Get Playlist";
            this.btnGetPlaylist.UseVisualStyleBackColor = true;
            this.btnGetPlaylist.Click += new System.EventHandler(this.btnGetPlaylist_Click);
            // 
            // chbPlaylistDASH
            // 
            this.chbPlaylistDASH.AutoSize = true;
            this.chbPlaylistDASH.Location = new System.Drawing.Point(522, 48);
            this.chbPlaylistDASH.Name = "chbPlaylistDASH";
            this.chbPlaylistDASH.Size = new System.Drawing.Size(56, 17);
            this.chbPlaylistDASH.TabIndex = 14;
            this.chbPlaylistDASH.Text = "DASH";
            this.chbPlaylistDASH.UseVisualStyleBackColor = true;
            this.chbPlaylistDASH.CheckedChanged += new System.EventHandler(this.chbPlaylistDASH_CheckedChanged);
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
            this.cbPlaylistQuality.TabIndex = 12;
            this.cbPlaylistQuality.SelectedIndexChanged += new System.EventHandler(this.cbPlaylistQuality_SelectedIndexChanged);
            // 
            // btnPlaylistBrowse
            // 
            this.btnPlaylistBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistBrowse.Location = new System.Drawing.Point(547, 71);
            this.btnPlaylistBrowse.Name = "btnPlaylistBrowse";
            this.btnPlaylistBrowse.Size = new System.Drawing.Size(31, 23);
            this.btnPlaylistBrowse.TabIndex = 11;
            this.btnPlaylistBrowse.Text = "...";
            this.btnPlaylistBrowse.UseVisualStyleBackColor = true;
            this.btnPlaylistBrowse.Click += new System.EventHandler(this.btnPlaylistBrowse_Click);
            // 
            // btnPlaylistDownloadAll
            // 
            this.btnPlaylistDownloadAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPlaylistDownloadAll.Enabled = false;
            this.btnPlaylistDownloadAll.Location = new System.Drawing.Point(488, 130);
            this.btnPlaylistDownloadAll.Name = "btnPlaylistDownloadAll";
            this.btnPlaylistDownloadAll.Size = new System.Drawing.Size(90, 23);
            this.btnPlaylistDownloadAll.TabIndex = 8;
            this.btnPlaylistDownloadAll.Text = "Download All";
            this.btnPlaylistDownloadAll.UseVisualStyleBackColor = true;
            this.btnPlaylistDownloadAll.Click += new System.EventHandler(this.btnPlaylistDownloadAll_Click);
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
            this.cbPlaylistSaveTo.Size = new System.Drawing.Size(481, 21);
            this.cbPlaylistSaveTo.TabIndex = 9;
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
            this.txtPlaylistLink.Size = new System.Drawing.Size(518, 20);
            this.txtPlaylistLink.TabIndex = 4;
            this.txtPlaylistLink.TextChanged += new System.EventHandler(this.txtPlaylistLink_TextChanged);
            // 
            // convertTabPage
            // 
            this.convertTabPage.Controls.Add(this.btnCheckAgain);
            this.convertTabPage.Controls.Add(this.lFFmpegMissing);
            this.convertTabPage.Controls.Add(this.groupBox2);
            this.convertTabPage.Location = new System.Drawing.Point(4, 22);
            this.convertTabPage.Name = "convertTabPage";
            this.convertTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.convertTabPage.Size = new System.Drawing.Size(596, 291);
            this.convertTabPage.TabIndex = 2;
            this.convertTabPage.Text = "Convert";
            this.convertTabPage.UseVisualStyleBackColor = true;
            // 
            // btnCheckAgain
            // 
            this.btnCheckAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckAgain.Location = new System.Drawing.Point(484, 216);
            this.btnCheckAgain.Name = "btnCheckAgain";
            this.btnCheckAgain.Size = new System.Drawing.Size(106, 23);
            this.btnCheckAgain.TabIndex = 11;
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
            this.lFFmpegMissing.Location = new System.Drawing.Point(6, 216);
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
            this.groupBox2.Controls.Add(this.btnBrowseOutput);
            this.groupBox2.Controls.Add(this.btnBrowseInput);
            this.groupBox2.Controls.Add(this.groupBox4);
            this.groupBox2.Controls.Add(this.btnConvert);
            this.groupBox2.Controls.Add(this.txtOutput);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtInput);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(584, 204);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Convert to MP3";
            // 
            // btnBrowseOutput
            // 
            this.btnBrowseOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseOutput.Location = new System.Drawing.Point(554, 44);
            this.btnBrowseOutput.Name = "btnBrowseOutput";
            this.btnBrowseOutput.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseOutput.TabIndex = 7;
            this.btnBrowseOutput.Text = "...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseInput.Location = new System.Drawing.Point(554, 18);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(24, 22);
            this.btnBrowseInput.TabIndex = 6;
            this.btnBrowseInput.Text = "...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox4.Controls.Add(this.chbCropFrom);
            this.groupBox4.Controls.Add(this.mtxtTo);
            this.groupBox4.Controls.Add(this.chbCropTo);
            this.groupBox4.Controls.Add(this.mtxtFrom);
            this.groupBox4.Location = new System.Drawing.Point(6, 134);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(372, 64);
            this.groupBox4.TabIndex = 5;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Crop";
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
            this.mtxtFrom.ValidatingType = typeof(System.TimeSpan);
            // 
            // btnConvert
            // 
            this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConvert.Location = new System.Drawing.Point(503, 175);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 4;
            this.btnConvert.Text = "Convert";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutput.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtOutput.Location = new System.Drawing.Point(51, 45);
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.Size = new System.Drawing.Size(497, 20);
            this.txtOutput.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(39, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Output";
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtInput.Location = new System.Drawing.Point(51, 19);
            this.txtInput.Name = "txtInput";
            this.txtInput.ReadOnly = true;
            this.txtInput.Size = new System.Drawing.Size(497, 20);
            this.txtInput.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(31, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Input";
            // 
            // queueTabPage
            // 
            this.queueTabPage.Controls.Add(this.chbAutoConvert);
            this.queueTabPage.Controls.Add(this.lvQueue);
            this.queueTabPage.Location = new System.Drawing.Point(4, 22);
            this.queueTabPage.Name = "queueTabPage";
            this.queueTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.queueTabPage.Size = new System.Drawing.Size(596, 291);
            this.queueTabPage.TabIndex = 1;
            this.queueTabPage.Text = "Queue";
            this.queueTabPage.UseVisualStyleBackColor = true;
            // 
            // chbAutoConvert
            // 
            this.chbAutoConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chbAutoConvert.AutoSize = true;
            this.chbAutoConvert.Location = new System.Drawing.Point(6, 268);
            this.chbAutoConvert.Name = "chbAutoConvert";
            this.chbAutoConvert.Size = new System.Drawing.Size(164, 17);
            this.chbAutoConvert.TabIndex = 3;
            this.chbAutoConvert.Text = "Convert to MP3 automatically";
            this.chbAutoConvert.UseVisualStyleBackColor = true;
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
            this.lvQueue.Size = new System.Drawing.Size(584, 256);
            this.lvQueue.TabIndex = 2;
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 387);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.bottomPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(644, 425);
            this.Name = "MainForm";
            this.Text = "YouTube Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
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
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.queueTabPage.ResumeLayout(false);
            this.queueTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGetVideo;
        private System.Windows.Forms.ComboBox cbQuality;
        private System.Windows.Forms.TextBox txtYoutubeLink;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.Button btnExit;
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
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chbCropFrom;
        private System.Windows.Forms.MaskedTextBox mtxtFrom;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.MaskedTextBox mtxtTo;
        private System.Windows.Forms.CheckBox chbCropTo;
        private System.Windows.Forms.Button btnBrowseOutput;
        private System.Windows.Forms.Button btnBrowseInput;
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
        private System.Windows.Forms.CheckBox chbPlaylistDASH;
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
    }
}

