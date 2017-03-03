namespace YouTube_Downloader.Controls
{
    partial class DurationPicker
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Seconds = new YouTube_Downloader.Controls.DurationPickerTextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.Minutes = new YouTube_Downloader.Controls.DurationPickerTextBox();
            this.Hours = new YouTube_Downloader.Controls.DurationPickerTextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Seconds
            // 
            this.Seconds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Seconds.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Seconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Seconds.Location = new System.Drawing.Point(43, 1);
            this.Seconds.Margin = new System.Windows.Forms.Padding(3, 1, 0, 0);
            this.Seconds.MaxDuration = 59;
            this.Seconds.Name = "Seconds";
            this.Seconds.Size = new System.Drawing.Size(14, 16);
            this.Seconds.TabIndex = 3;
            this.Seconds.Text = "00";
            this.Seconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(37, -1);
            this.label15.Margin = new System.Windows.Forms.Padding(0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(9, 15);
            this.label15.TabIndex = 18;
            this.label15.Text = ":";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Minutes
            // 
            this.Minutes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Minutes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Minutes.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Minutes.Location = new System.Drawing.Point(23, 1);
            this.Minutes.Margin = new System.Windows.Forms.Padding(3, 1, 0, 0);
            this.Minutes.MaxDuration = 59;
            this.Minutes.Name = "Minutes";
            this.Minutes.Size = new System.Drawing.Size(14, 16);
            this.Minutes.TabIndex = 1;
            this.Minutes.Text = "00";
            this.Minutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Hours
            // 
            this.Hours.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Hours.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Hours.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hours.Location = new System.Drawing.Point(3, 1);
            this.Hours.Margin = new System.Windows.Forms.Padding(3, 1, 0, 0);
            this.Hours.MaxDuration = 24;
            this.Hours.Name = "Hours";
            this.Hours.Size = new System.Drawing.Size(14, 16);
            this.Hours.TabIndex = 0;
            this.Hours.Text = "00";
            this.Hours.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(17, -1);
            this.label11.Margin = new System.Windows.Forms.Padding(0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(9, 15);
            this.label11.TabIndex = 16;
            this.label11.Text = ":";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DurationPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.Seconds);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.Minutes);
            this.Controls.Add(this.Hours);
            this.Controls.Add(this.label11);
            this.Name = "DurationPicker";
            this.Size = new System.Drawing.Size(66, 18);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DurationPickerTextBox Seconds;
        private System.Windows.Forms.Label label15;
        private DurationPickerTextBox Minutes;
        private DurationPickerTextBox Hours;
        private System.Windows.Forms.Label label11;
    }
}
