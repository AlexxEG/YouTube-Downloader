using YouTube_Downloader_DLL.Helpers;

namespace YouTube_Downloader_DLL.YoutubeDl
{
    public class YTDAuthentication
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string TwoFactor { get; private set; }

        public YTDAuthentication(string username, string password, string twoFactor)
        {
            this.Username = username;
            this.Password = password;
            this.TwoFactor = twoFactor;
        }

        public string ToCmdArgument()
        {
            string twoFactor = this.TwoFactor;

            if (!string.IsNullOrEmpty(twoFactor))
                twoFactor = string.Format(YoutubeDlHelper.Commands.TwoFactor, twoFactor);

            return string.Format(YoutubeDlHelper.Commands.Authentication,
                this.Username,
                this.Password) + twoFactor;
        }
    }
}
