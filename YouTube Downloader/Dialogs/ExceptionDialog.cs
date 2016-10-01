using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTube_Downloader.Dialogs
{
    public partial class ExceptionDialog : Form
    {
        const int ExtraHeight = 60;
        const int MaximumWidth = 494;

        public ExceptionDialog()
        {
            InitializeComponent();
            lText.Font = SystemFonts.MessageBoxFont;
        }

        public ExceptionDialog(string text, string caption)
            : this()
        {
            this.Text = caption;
            lText.Text = text;
            this.FitText();
        }

        private void llToggleDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void FitText()
        {
            var size = this.CreateGraphics().MeasureString(lText.Text,
                                                           lText.Font,
                                                           MaximumWidth - (lText.Padding.Left + lText.Padding.Right)
                                                          ).ToSize();
            var height = this.Height - lText.Height;
            var width = this.Width - lText.Width;

            this.Height = height + size.Height + ExtraHeight;
            this.Width = width + size.Width;
        }

        public static void ShowDialog(string text, string caption)
        {
            new ExceptionDialog(text, caption).ShowDialog();
        }
    }
}
