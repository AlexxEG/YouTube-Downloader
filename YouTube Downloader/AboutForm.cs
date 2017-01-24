using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            lVersion.Text += Common.VersionString;
            lBuildDate.Text += DateTime.Parse(Properties.Resources.BuildDate).ToString("dddd, MMMM d, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
        }

        private void llGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(llGitHub.Text);
            }
            catch
            {
                MessageBox.Show(this, "The link couldn't be opened.");
            }
        }
    }
}
