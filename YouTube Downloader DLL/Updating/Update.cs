using System;

namespace YouTube_Downloader_DLL.Updating
{
    public class Update
    {
        public string DownloadUrl { get; private set; }
        public string VersionString { get; private set; }
        public Version Version { get; private set; }

        public Update(string downloadUrl, string version)
        {
            this.DownloadUrl = downloadUrl;
            this.Version = new Version(version);
            this.VersionString = version;
        }
    }
}
