using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YouTube_Downloader_DLL.Classes
{
    public class LogCleanup
    {
        /// <summary>
        /// Folders to cleanup.
        /// </summary>
        public static string[] Folders = new string[]
        {
            Common.GetJsonDirectory(),
            Common.GetLogsDirectory(),
            Common.GetStackTracesDirectory()
        };

        /// <summary>
        /// Max log age. Default is 3 days.
        /// </summary>
        public static TimeSpan MaxLogAge = new TimeSpan(3, 0, 0, 0);

        public static async void RunAsync()
        {
            await Task.Run(delegate
            {
                var cleanupList = new List<string>();

                foreach (string dir in Folders)
                {
                    var dirInfo = new DirectoryInfo(dir);

                    if (!dirInfo.Exists)
                        continue;

                    foreach (var file in dirInfo.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        var age = DateTime.Now.Subtract(file.CreationTime);

                        if (age >= MaxLogAge)
                            cleanupList.Add(file.FullName);
                    }
                }

                if (cleanupList.Count > 0)
                    Helper.DeleteFiles(cleanupList.ToArray());
            });
        }
    }
}
