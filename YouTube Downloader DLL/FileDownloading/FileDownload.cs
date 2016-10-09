using IO = System.IO;

namespace YouTube_Downloader_DLL.FileDownloading
{
    public class FileDownload
    {
        public bool AlwaysCleanupOnCancel { get; set; }
        public bool IsFinished { get; set; }

        public long Progress { get; set; }
        public long TotalFileSize { get; set; }

        public string Directory
        {
            get { return IO.Path.GetDirectoryName(this.Path); }
        }
        public string Name
        {
            get { return IO.Path.GetFileName(this.Path); }
        }
        public string Path { get; set; }
        public string Url { get; set; }

        public FileDownload(string path, string url)
        {
            this.Path = path;
            this.Url = url;
        }

        public FileDownload(string path, string url, bool alwaysCleanupOnCancel)
            : this(path, url)
        {
            this.AlwaysCleanupOnCancel = alwaysCleanupOnCancel;
        }
    }
}
