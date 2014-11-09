using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace YouTube_Downloader.Classes
{
    public class VideoInfo
    {
        public long Duration { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Url { get; set; }
        public IList<VideoFormat> Formats { get; set; }

        public event FileSizeUpdateHandler FileSizeUpdated;

        public VideoInfo(string json_file)
        {
            this.Formats = new List<VideoFormat>();

            this.DeserializeJson(json_file);
        }

        /// <summary>
        /// Aborts all the requests for file size for each video format.
        /// </summary>
        public void AbortUpdateFileSizes()
        {
            foreach (VideoFormat format in this.Formats)
            {
                format.AbortUpdateFileSize();
            }
        }

        internal void OnFileSizeUpdated(VideoFormat videoFormat)
        {
            if (this.FileSizeUpdated != null)
                this.FileSizeUpdated(this, new FileSizeUpdateEventArgs(videoFormat));
        }

        private void DeserializeJson(string json_file)
        {
            string json = "";

            // Should try for about 10 seconds. */
            int attempts = 0; int maxAttempts = 20;

            while ((attempts++) <= maxAttempts)
            {
                try
                {
                    json = File.ReadAllText(json_file);
                    break;
                }
                catch (IOException ex)
                {
                    if (ex.Message.EndsWith("because it is being used by another process."))
                    {
                        Console.WriteLine(ex);
                        Thread.Sleep(500);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            JObject jObject = JObject.Parse(json);

            this.Duration = long.Parse(jObject["duration"].ToString());
            this.Title = jObject["fulltitle"].ToString();

            string displayId = jObject["display_id"].ToString();

            this.ThumbnailUrl = string.Format("https://i.ytimg.com/vi/{0}/mqdefault.jpg", displayId);
            this.Url = jObject["webpage_url"].ToString();

            JArray array = (JArray)jObject["formats"];

            foreach (JToken token in array)
            {
                this.Formats.Add(new VideoFormat(this, token));
            }
        }
    }
}
