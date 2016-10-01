namespace YouTube_Downloader_DLL.Classes
{
    public class QuickVideoInfo
    {
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Duration { get; private set; }

        public QuickVideoInfo(string id, string title, string duration)
        {
            this.ID = id;
            this.Title = title;
            this.Duration = duration;
        }
    }
}
