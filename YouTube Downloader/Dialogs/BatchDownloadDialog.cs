using System;
using System.Linq;
using System.Windows.Forms;
using YouTube_Downloader.Properties;
using YouTube_Downloader_DLL.Classes;
using YouTube_Downloader_DLL.Enums;

namespace YouTube_Downloader.Dialogs
{
    public partial class BatchDownloadDialog : Form
    {
        public string[] Lines { get; set; }
        public PreferredQuality PreferredQuality { get; set; }

        private Settings settings = Settings.Default;

        public BatchDownloadDialog()
        {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x100;
            var keyCode = (Keys)(msg.WParam.ToInt32() &
                                  Convert.ToInt32(Keys.KeyCode));
            if ((msg.Msg == WM_KEYDOWN && keyCode == Keys.A)
                && (ModifierKeys == Keys.Control)
                && txtInput.Focused)
            {
                txtInput.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BatchDownloadDialog_Load(object sender, EventArgs e)
        {
            cbPreferredQuality.Items.AddRange(Enum.GetNames(typeof(PreferredQuality)));
            cbPreferredQuality.SelectedIndex = (int)settings.PreferredQualityBatch;
        }

        private void cbPreferredQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            settings.PreferredQualityBatch = (PreferredQuality)cbPreferredQuality.SelectedIndex;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (txtInput.Lines.All(x => Helper.IsValidYouTubeUrl(x)))
            {
                this.Lines = txtInput.Lines;

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(this, "One or more URLs are not valid.");
            }
        }

        public new static BatchDownloadResult ShowDialog(IWin32Window owner)
        {
            var dialog = new BatchDownloadDialog();
            return new BatchDownloadResult((dialog as Form).ShowDialog(owner), dialog.Lines);
        }
    }
}
