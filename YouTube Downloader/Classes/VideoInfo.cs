using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace YouTube_Downloader.Classes
{
    public class VideoInfo
    {
        /// <summary>
        /// Gets the video duration in seconds.
        /// </summary>
        public long Duration { get; private set; }
        /// <summary>
        /// Gets the video ID.
        /// </summary>
        public string ID { get; private set; }
        /// <summary>
        /// Gets the video title.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Gets the video thumbnail url.
        /// </summary>
        public string ThumbnailUrl { get; private set; }
        /// <summary>
        /// Gets the video url.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Gets all the available formats.
        /// </summary>
        public List<VideoFormat> Formats { get; private set; }

        /// <summary>
        /// Occurs when one of the format's file size has been updated.
        /// </summary>
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
            string json = ReadJSON(json_file);
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

        private static string ReadJSON(string json_file)
        {
            string json = string.Empty;

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

            return json;
        }
    }
}
