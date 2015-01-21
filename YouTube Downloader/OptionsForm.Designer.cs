namespace YouTube_Downloader
{
    partial class OptionsForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chbIncludeDASH = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chbIncludeNonDASH = new System.Windows.Forms.CheckBox();
            this.chbIncludeNormal = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(360, 115);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(216, 176);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(297, 176);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chbIncludeDASH
            // 
            this.chbIncludeDASH.AutoSize = true;
            this.chbIncludeDASH.Location = new System.Drawing.Point(13, 19);
            this.chbIncludeDASH.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.chbIncludeDASH.Name = "chbIncludeDASH";
            this.chbIncludeDASH.Size = new System.Drawing.Size(94, 17);
            this.chbIncludeDASH.TabIndex = 0;
            this.chbIncludeDASH.Text = "Include DASH";
            this.chbIncludeDASH.UseVisualStyleBackColor = true;
            this.chbIncludeDASH.CheckedChanged += new System.EventHandler(this.videoFormatsCheckBoxes_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.chbIncludeNormal);
            this.groupBox2.Controls.Add(this.chbIncludeNonDASH);
            this.groupBox2.Controls.Add(this.chbIncludeDASH);
            this.groupBox2.Location = new System.Drawing.Point(180, 19);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(174, 90);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Video Formats";
            // 
            // chbIncludeNonDASH
            // 
            this.chbIncludeNonDASH.AutoSize = true;
            this.chbIncludeNonDASH.Location = new System.Drawing.Point(13, 42);
            this.chbIncludeNonDASH.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.chbIncludeNonDASH.Name = "chbIncludeNonDASH";
            this.chbIncludeNonDASH.Size = new System.Drawing.Size(115, 17);
            this.chbIncludeNonDASH.TabIndex = 1;
            this.chbIncludeNonDASH.Text = "Include non-DASH";
            this.chbIncludeNonDASH.UseVisualStyleBackColor = true;
            this.chbIncludeNonDASH.CheckedChanged += new System.EventHandler(this.videoFormatsCheckBoxes_CheckedChanged);
            // 
            // chbIncludeNormal
            // 
            this.chbIncludeNormal.AutoSize = true;
            this.chbIncludeNormal.Location = new System.Drawing.Point(13, 65);
            this.chbIncludeNormal.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.chbIncludeNormal.Name = "chbIncludeNormal";
            this.chbIncludeNormal.Size = new System.Drawing.Size(105, 17);
            this.chbIncludeNormal.TabIndex = 2;
            this.chbIncludeNormal.Text = "Include \"normal\"";
            this.chbIncludeNormal.UseVisualStyleBackColor = true;
            this.chbIncludeNormal.CheckedChanged += new System.EventHandler(this.videoFormatsCheckBoxes_CheckedChanged);
            // 
            // OptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(384, 211);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.groupBox1);
            this.Name = "OptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chbIncludeNonDASH;
        private System.Windows.Forms.CheckBox chbIncludeDASH;
        private System.Windows.Forms.CheckBox chbIncludeNormal;
    }
}