using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTube_Downloader_DLL.Operations;

namespace YouTube_Downloader_DLL.Helpers
{
    public static class DownloadQueueHandler
    {
        static bool _stop;

        public static int DownloadingCount
        {
            get
            {
                int count = 0;
                foreach (var operation in Queue)
                    if (IsDownloaderType(operation) && operation.CanPause())
                        count++;
                return count;
            }
        }
        public static int MaxDownloads { get; set; }

        public static bool LimitDownloads { get; set; }

        public static List<Operation> Queue { get; set; }

        static DownloadQueueHandler()
        {
            Queue = new List<Operation>();
        }

        public static void Add(Operation operation)
        {
            operation.Completed += Operation_Completed;
            operation.Resumed += Operation_Resumed;
            operation.StatusChanged += Operation_StatusChanged;

            Queue.Add(operation);
        }

        public static void Remove(Operation operation)
        {
            operation.Completed -= Operation_Completed;
            operation.Resumed -= Operation_Resumed;
            operation.StatusChanged -= Operation_StatusChanged;

            Queue.Remove(operation);
        }

        public static void StartWatching(int maxDownloads)
        {
            MaxDownloads = maxDownloads;
            MainLoop();
        }

        public static void Stop()
        {
            _stop = true;
        }

        public static Operation[] GetQueued()
        {
            var queued = new List<Operation>();
            foreach (var operation in Queue)
                if (IsDownloaderType(operation) && operation.Status == OperationStatus.Queued)
                    queued.Add(operation);
            return queued.ToArray();
        }

        public static Operation[] GetWorking()
        {
            var working = new List<Operation>();
            foreach (var operation in Queue)
                if (IsDownloaderType(operation) && operation.Status == OperationStatus.Working)
                    working.Add(operation);
            return working.ToArray();
        }

        private static bool IsDownloaderType(Operation operation)
        {
            return operation is DownloadOperation ||
                   operation is TwitchOperation ||
                   operation is DummyOperations.DummyDownloadOperation;
        }

        private static async void MainLoop()
        {
            while (!_stop)
            {
                await Task.Delay(1000);

                var queued = GetQueued();

                // If downloads isn't limited, start all queued operations
                if (!LimitDownloads)
                {
                    if (queued.Length == 0)
                        continue;

                    foreach (var operation in queued)
                        if (operation.HasStarted)
                            operation.ResumeQuiet();
                        else
                            operation.Start();
                }
                else if (DownloadingCount < MaxDownloads)
                {
                    // Number of operations to start
                    int count = Math.Min(MaxDownloads - DownloadingCount, queued.Length);

                    for (int i = 0; i < count; i++)
                        if (queued[i].HasStarted)
                            queued[i].ResumeQuiet();
                        else
                            queued[i].Start();
                }
                else if (DownloadingCount > MaxDownloads)
                {
                    // Number of operations to pause
                    int count = DownloadingCount - MaxDownloads;
                    var working = GetWorking();

                    for (int i = DownloadingCount - 1; i > (MaxDownloads - 1); i--)
                        working[i].Queue();
                }
            }
        }

        private static void Operation_Completed(object sender, OperationEventArgs e)
        {
            Remove((sender as Operation));
        }

        private static void Operation_Resumed(object sender, EventArgs e)
        {
            // User resumed operation, prioritize this operation over other queued
            var operation = sender as Operation;

            // Move operation to top of queue, since pausing happens from the bottom.
            // I.E. this operation will only paused if absolutely necessary.
            Queue.Remove(operation);
            Queue.Insert(0, operation);
        }

        private static void Operation_StatusChanged(object sender, StatusChangedEventArgs e)
        {

        }
    }
}
