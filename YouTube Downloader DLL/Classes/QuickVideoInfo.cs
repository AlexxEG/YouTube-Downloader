namespace YouTube_Downloader_DLL.Classes
{
    public class QuickVideoInfo
    {
        public int Index { get; private set; }
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Duration { get; private set; }

        public QuickVideoInfo(int index, string id, string title, string duration)
        {
            this.Index = index;
            this.ID = id;
            this.Title = title;
            this.Duration = duration;
        }
    }
}
