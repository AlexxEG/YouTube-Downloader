using System;
using System.Windows.Forms;

namespace YouTube_Downloader
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            chbIncludeDASH.Checked = Properties.Settings.Default.IncludeDASH;
            chbIncludeNonDASH.Checked = Properties.Settings.Default.IncludeNonDASH;
            chbIncludeNormal.Checked = Properties.Settings.Default.IncludeNormal;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IncludeDASH = chbIncludeDASH.Checked;
            Properties.Settings.Default.IncludeNonDASH = chbIncludeNonDASH.Checked;
            Properties.Settings.Default.IncludeNormal = chbIncludeNormal.Checked;
            Properties.Settings.Default.Save();
        }

        private void videoFormatsCheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chb = sender as CheckBox;

            if (chb.Checked)
                return;

            if (!chbIncludeDASH.Checked && !chbIncludeNonDASH.Checked && !chbIncludeNormal.Checked)
                chb.Checked = true;
        }
    }
}
