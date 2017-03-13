using System;
using System.Drawing;
using System.Windows.Forms;

namespace YouTube_Downloader.Dialogs
{
    public partial class ExceptionDialog : Form
    {
        const int ExtraHeight = 60;
        const int MaximumHeight = 420;
        const int MaximumWidth = 494;

        bool _expanded;

        public Exception Exception { get; private set; }

        public ExceptionDialog()
        {
            InitializeComponent();
            lText.Font = SystemFonts.MessageBoxFont;
        }

        public ExceptionDialog(string text, string caption, Exception exception)
            : this()
        {
            this.Text = caption;
            lText.Text = text;
            this.Exception = exception;
            txtException.Text = this.Exception.ToString();
            this.FitText();
        }

        public ExceptionDialog(IWin32Window owner, string text, string caption, Exception exception)
            : this(text, caption, exception)
        {
            this.Owner = (Form)owner;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void llToggleDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var size = TextRenderer.MeasureText(txtException.Text, txtException.Font);
            const int scrollbarH = 30;

            if (_expanded == false)
            {
                this.Height = Math.Min(this.Height + size.Height + scrollbarH, MaximumHeight);
                this.Width = Math.Min(this.Width + size.Width, MaximumWidth);
                tableLayoutPanel1.RowStyles[1] = new RowStyle(SizeType.Absolute, size.Height + scrollbarH);
            }
            else
            {
                this.Height -= size.Height + scrollbarH;
                this.Width = this.Width - size.Width;
                tableLayoutPanel1.RowStyles[1] = new RowStyle(SizeType.Absolute, 0);
            }

            _expanded = !_expanded;
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

        public static void ShowDialog(string text, string caption, Exception exception)
        {
            new ExceptionDialog(text, caption, exception).ShowDialog();
        }

        public static void ShowDialog(IWin32Window owner, string text, string caption, Exception exception)
        {
            new ExceptionDialog(owner, text, caption, exception).ShowDialog();
        }
    }
}
