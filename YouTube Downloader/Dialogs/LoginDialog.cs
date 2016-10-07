using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using YouTube_Downloader_DLL.YoutubeDl;

namespace YouTube_Downloader.Dialogs
{
    public partial class LoginDialog : Form
    {
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEmailUsername.Text) ||
                string.IsNullOrEmpty(txtPassword.Text))
            {
                this.ShowWarningAsync();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private async void ShowWarningAsync()
        {
            if (lWarning.Visible)
                return;

            lWarning.Visible = true;

            await Task.Delay(3000);

            lWarning.Visible = false;
        }

        public new static YTDAuthentication Show(IWin32Window owner)
        {
            var ld = new LoginDialog();

            if (ld.ShowDialog(owner) == DialogResult.OK)
            {
                return new YTDAuthentication(ld.txtEmailUsername.Text,
                    ld.txtPassword.Text,
                    ld.txtTwoFactor.Text);
            }
            else
            {
                return null;
            }
        }
    }
}
