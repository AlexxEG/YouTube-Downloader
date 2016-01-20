namespace YouTube_Downloader_DLL.Classes
{
    public class TwitchOperationProgress
    {
        public decimal ProgressPercentage { get; private set; }
        public decimal TotalSize { get; private set; }
        public string TotalSizeSuffix { get; private set; }
        public decimal Speed { get; private set; }
        public string SpeedSuffix { get; private set; }
        public string ETA { get; private set; }

        public TwitchOperationProgress(
            decimal progressPercentage,
            decimal totalSize, string totalSizeSuffix,
            decimal speed, string speedSuffix,
            string eta)
        {
            this.ProgressPercentage = progressPercentage;
            this.TotalSize = totalSize;
            this.TotalSizeSuffix = totalSizeSuffix;
            this.Speed = speed;
            this.SpeedSuffix = speedSuffix;
            this.ETA = eta;
        }
    }
}
