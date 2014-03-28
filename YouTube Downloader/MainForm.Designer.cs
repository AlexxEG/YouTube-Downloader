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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnToDo = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
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
            this.lTitle = new System.Windows.Forms.Label();
            this.videoThumbnail = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.downloadTabPage = new System.Windows.Forms.TabPage();
            this.convertTabPage = new System.Windows.Forms.TabPage();
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
            this.lvQueue = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoThumbnail)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.downloadTabPage.SuspendLayout();
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
            this.groupBox1.Text = "Youtube Video";
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
            this.btnBrowse.Location = new System.Drawing.Point(466, 73);
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
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Save To";
            // 
            // cbSaveTo
            // 
            this.cbSaveTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSaveTo.FormattingEnabled = true;
            this.cbSaveTo.Location = new System.Drawing.Point(191, 73);
            this.cbSaveTo.Name = "cbSaveTo";
            this.cbSaveTo.Size = new System.Drawing.Size(269, 21);
            this.cbSaveTo.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(132, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(30, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Title:";
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.Enabled = false;
            this.btnDownload.Location = new System.Drawing.Point(503, 73);
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
            this.cbQuality.Location = new System.Drawing.Point(191, 46);
            this.cbQuality.Name = "cbQuality";
            this.cbQuality.Size = new System.Drawing.Size(387, 21);
            this.cbQuality.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.Controls.Add(this.btnToDo);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 321);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5);
            this.panel1.Size = new System.Drawing.Size(628, 46);
            this.panel1.TabIndex = 1;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // btnToDo
            // 
            this.btnToDo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToDo.Location = new System.Drawing.Point(460, 11);
            this.btnToDo.Name = "btnToDo";
            this.btnToDo.Size = new System.Drawing.Size(75, 23);
            this.btnToDo.TabIndex = 2;
            this.btnToDo.UseVisualStyleBackColor = true;
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
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
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
            this.groupBox3.Controls.Add(this.lTitle);
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
            this.groupBox3.Size = new System.Drawing.Size(584, 102);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Information";
            // 
            // lTitle
            // 
            this.lTitle.AutoSize = true;
            this.lTitle.Location = new System.Drawing.Point(188, 22);
            this.lTitle.Name = "lTitle";
            this.lTitle.Size = new System.Drawing.Size(10, 13);
            this.lTitle.TabIndex = 10;
            this.lTitle.Text = "-";
            // 
            // videoThumbnail
            // 
            this.videoThumbnail.BackColor = System.Drawing.Color.Gainsboro;
            this.videoThumbnail.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.videoThumbnail.Location = new System.Drawing.Point(6, 22);
            this.videoThumbnail.Name = "videoThumbnail";
            this.videoThumbnail.Size = new System.Drawing.Size(120, 68);
            this.videoThumbnail.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
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
            this.tabControl1.Controls.Add(this.convertTabPage);
            this.tabControl1.Controls.Add(this.queueTabPage);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(604, 297);
            this.tabControl1.TabIndex = 9;
            // 
            // downloadTabPage
            // 
            this.downloadTabPage.Controls.Add(this.groupBox1);
            this.downloadTabPage.Controls.Add(this.groupBox3);
            this.downloadTabPage.Location = new System.Drawing.Point(4, 22);
            this.downloadTabPage.Name = "downloadTabPage";
            this.downloadTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.downloadTabPage.Size = new System.Drawing.Size(596, 271);
            this.downloadTabPage.TabIndex = 0;
            this.downloadTabPage.Text = "Download";
            this.downloadTabPage.UseVisualStyleBackColor = true;
            // 
            // convertTabPage
            // 
            this.convertTabPage.Controls.Add(this.groupBox2);
            this.convertTabPage.Location = new System.Drawing.Point(4, 22);
            this.convertTabPage.Name = "convertTabPage";
            this.convertTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.convertTabPage.Size = new System.Drawing.Size(596, 271);
            this.convertTabPage.TabIndex = 2;
            this.convertTabPage.Text = "Convert";
            this.convertTabPage.UseVisualStyleBackColor = true;
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
            this.groupBox2.Size = new System.Drawing.Size(584, 194);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Converting Options";
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
            this.groupBox4.Location = new System.Drawing.Point(6, 124);
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
            this.btnConvert.Location = new System.Drawing.Point(503, 165);
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
            this.queueTabPage.Controls.Add(this.lvQueue);
            this.queueTabPage.Location = new System.Drawing.Point(4, 22);
            this.queueTabPage.Name = "queueTabPage";
            this.queueTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.queueTabPage.Size = new System.Drawing.Size(596, 271);
            this.queueTabPage.TabIndex = 1;
            this.queueTabPage.Text = "Queue";
            this.queueTabPage.UseVisualStyleBackColor = true;
            // 
            // lvQueue
            // 
            this.lvQueue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.lvQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvQueue.FullRowSelect = true;
            this.lvQueue.Location = new System.Drawing.Point(3, 3);
            this.lvQueue.Name = "lvQueue";
            this.lvQueue.Size = new System.Drawing.Size(590, 265);
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
            this.columnHeader3.Text = "Speed";
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
            this.columnHeader6.Text = "Link";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "MP4 files|*.mp4|MP3 files|*.mp3|All files|*.*";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "MP3 files|*.mp3|MP4 files|*.mp4|All files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 367);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "YouTube Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.videoThumbnail)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.downloadTabPage.ResumeLayout(false);
            this.convertTabPage.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.queueTabPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnGetVideo;
        private System.Windows.Forms.ComboBox cbQuality;
        private System.Windows.Forms.TextBox txtYoutubeLink;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnToDo;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnPaste;
        private ListViewEmbeddedControls.ListViewEx lvQueue;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
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
        private System.Windows.Forms.Label lTitle;
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
    }
}

