namespace YouTube_Downloader
{
    partial class UpdaterForm
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
            this.lFFmpegUpdateAvailable = new System.Windows.Forms.Label();
            this.lFFmpegInstalled = new System.Windows.Forms.Label();
            this.lFFmpegOnline = new System.Windows.Forms.Label();
            this.btnFFmpegInstall = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lYoutubeDlUpdateAvailable = new System.Windows.Forms.Label();
            this.btnYoutubeDlInstall = new System.Windows.Forms.Button();
            this.lYoutubeDlInstalled = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lYoutubeDlOnline = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lFFmpegUpdateAvailable);
            this.groupBox1.Controls.Add(this.lFFmpegInstalled);
            this.groupBox1.Controls.Add(this.lFFmpegOnline);
            this.groupBox1.Controls.Add(this.btnFFmpegInstall);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 100);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FFmpeg";
            // 
            // lFFmpegUpdateAvailable
            // 
            this.lFFmpegUpdateAvailable.ForeColor = System.Drawing.Color.DarkGreen;
            this.lFFmpegUpdateAvailable.Location = new System.Drawing.Point(6, 76);
            this.lFFmpegUpdateAvailable.Name = "lFFmpegUpdateAvailable";
            this.lFFmpegUpdateAvailable.Size = new System.Drawing.Size(228, 13);
            this.lFFmpegUpdateAvailable.TabIndex = 5;
            this.lFFmpegUpdateAvailable.Text = "Update available";
            this.lFFmpegUpdateAvailable.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lFFmpegUpdateAvailable.Visible = false;
            // 
            // lFFmpegInstalled
            // 
            this.lFFmpegInstalled.AutoSize = true;
            this.lFFmpegInstalled.Location = new System.Drawing.Point(104, 44);
            this.lFFmpegInstalled.Margin = new System.Windows.Forms.Padding(5);
            this.lFFmpegInstalled.Name = "lFFmpegInstalled";
            this.lFFmpegInstalled.Size = new System.Drawing.Size(10, 13);
            this.lFFmpegInstalled.TabIndex = 4;
            this.lFFmpegInstalled.Text = "-";
            // 
            // lFFmpegOnline
            // 
            this.lFFmpegOnline.AutoSize = true;
            this.lFFmpegOnline.Location = new System.Drawing.Point(104, 21);
            this.lFFmpegOnline.Margin = new System.Windows.Forms.Padding(5);
            this.lFFmpegOnline.Name = "lFFmpegOnline";
            this.lFFmpegOnline.Size = new System.Drawing.Size(10, 13);
            this.lFFmpegOnline.TabIndex = 3;
            this.lFFmpegOnline.Text = "-";
            // 
            // btnFFmpegInstall
            // 
            this.btnFFmpegInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFFmpegInstall.Enabled = false;
            this.btnFFmpegInstall.Location = new System.Drawing.Point(240, 71);
            this.btnFFmpegInstall.Name = "btnFFmpegInstall";
            this.btnFFmpegInstall.Size = new System.Drawing.Size(114, 23);
            this.btnFFmpegInstall.TabIndex = 2;
            this.btnFFmpegInstall.Text = "Install";
            this.btnFFmpegInstall.UseVisualStyleBackColor = true;
            this.btnFFmpegInstall.Click += new System.EventHandler(this.btnFFmpegInstall_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 44);
            this.label2.Margin = new System.Windows.Forms.Padding(5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Installed version:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Online version:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lYoutubeDlUpdateAvailable);
            this.groupBox2.Controls.Add(this.btnYoutubeDlInstall);
            this.groupBox2.Controls.Add(this.lYoutubeDlInstalled);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.lYoutubeDlOnline);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(12, 118);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(360, 100);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "youtube-dl";
            // 
            // lYoutubeDlUpdateAvailable
            // 
            this.lYoutubeDlUpdateAvailable.ForeColor = System.Drawing.Color.DarkGreen;
            this.lYoutubeDlUpdateAvailable.Location = new System.Drawing.Point(6, 76);
            this.lYoutubeDlUpdateAvailable.Name = "lYoutubeDlUpdateAvailable";
            this.lYoutubeDlUpdateAvailable.Size = new System.Drawing.Size(228, 13);
            this.lYoutubeDlUpdateAvailable.TabIndex = 6;
            this.lYoutubeDlUpdateAvailable.Text = "Update available";
            this.lYoutubeDlUpdateAvailable.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lYoutubeDlUpdateAvailable.Visible = false;
            // 
            // btnYoutubeDlInstall
            // 
            this.btnYoutubeDlInstall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnYoutubeDlInstall.Enabled = false;
            this.btnYoutubeDlInstall.Location = new System.Drawing.Point(240, 71);
            this.btnYoutubeDlInstall.Name = "btnYoutubeDlInstall";
            this.btnYoutubeDlInstall.Size = new System.Drawing.Size(114, 23);
            this.btnYoutubeDlInstall.TabIndex = 5;
            this.btnYoutubeDlInstall.Text = "Install";
            this.btnYoutubeDlInstall.UseVisualStyleBackColor = true;
            this.btnYoutubeDlInstall.Click += new System.EventHandler(this.btnYoutubeDlInstall_Click);
            // 
            // lYoutubeDlInstalled
            // 
            this.lYoutubeDlInstalled.AutoSize = true;
            this.lYoutubeDlInstalled.Location = new System.Drawing.Point(104, 44);
            this.lYoutubeDlInstalled.Margin = new System.Windows.Forms.Padding(5);
            this.lYoutubeDlInstalled.Name = "lYoutubeDlInstalled";
            this.lYoutubeDlInstalled.Size = new System.Drawing.Size(10, 13);
            this.lYoutubeDlInstalled.TabIndex = 8;
            this.lYoutubeDlInstalled.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 21);
            this.label6.Margin = new System.Windows.Forms.Padding(5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Online version:";
            // 
            // lYoutubeDlOnline
            // 
            this.lYoutubeDlOnline.AutoSize = true;
            this.lYoutubeDlOnline.Location = new System.Drawing.Point(104, 21);
            this.lYoutubeDlOnline.Margin = new System.Windows.Forms.Padding(5);
            this.lYoutubeDlOnline.Name = "lYoutubeDlOnline";
            this.lYoutubeDlOnline.Size = new System.Drawing.Size(10, 13);
            this.lYoutubeDlOnline.TabIndex = 7;
            this.lYoutubeDlOnline.Text = "-";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 44);
            this.label5.Margin = new System.Windows.Forms.Padding(5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Installed version:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(297, 224);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // UpdaterForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 259);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UpdaterForm";
            this.Text = "Check for updates...";
            this.Load += new System.EventHandler(this.UpdaterForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lFFmpegInstalled;
        private System.Windows.Forms.Label lFFmpegOnline;
        private System.Windows.Forms.Button btnFFmpegInstall;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnYoutubeDlInstall;
        private System.Windows.Forms.Label lYoutubeDlInstalled;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lYoutubeDlOnline;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lFFmpegUpdateAvailable;
        private System.Windows.Forms.Label lYoutubeDlUpdateAvailable;
    }
}