using System;
using System.Windows.Forms;

namespace YouTube_Downloader
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            chbShowMaxSimDownloads.Checked = Properties.Settings.Default.ShowMaxSimDownloads;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowMaxSimDownloads = chbShowMaxSimDownloads.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
