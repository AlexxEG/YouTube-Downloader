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
