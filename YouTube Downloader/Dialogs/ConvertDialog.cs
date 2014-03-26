using System;
using System.IO;
using System.Windows.Forms;

namespace YouTube_Downloader.Dialogs
{
    public partial class ConvertDialog : Form
    {
        public string Input { get; set; }
        public string Output { get; set; }

        public ConvertDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtInput.Text))
            {
                MessageBox.Show(this, "Couldn't find input file.");
                return;
            }

            this.Input = txtInput.Text;
            this.Output = txtOutput.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtInput.Text = openFileDialog1.FileName;
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtOutput.Text = saveFileDialog1.FileName;
            }
        }

        public DialogResult ShowDialog(IWin32Window owner, string input)
        {
            txtInput.Text = input;
            btnBrowseInput.Enabled = false;
            this.Shown += delegate { btnBrowseOutput.PerformClick(); };
            return base.ShowDialog(owner);
        }
    }
}
