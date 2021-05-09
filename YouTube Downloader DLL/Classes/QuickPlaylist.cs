using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace YouTube_Downloader_DLL.Classes
{
    /// <summary>
    /// Used to quickly list all videos in a playlist.
    /// </summary>
    public class QuickPlaylist
    {
        public bool IgnoreDuplicates { get; set; } = true;
        public string Title { get; private set; }
        public string Url { get; private set; }
        public List<QuickVideoInfo> Videos { get; private set; }

        public QuickPlaylist(string playlistUrl)
        {
            this.Url = playlistUrl;
            this.Videos = new List<QuickVideoInfo>();
        }

        public QuickPlaylist Load()
        {
            var wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            int videoIndex = 0;

            // Change playlist URL to embed so we can retrieve whole playlist without infinite scrolling issues
            this.Url = this.Url.Replace("https://www.youtube.com/playlist?list=", "https://www.youtube.com/embed/videoseries?list=");

            string source = wc.DownloadString(this.Url);
            Match m = null;

            // Find playlist title
            var playlistname = new Regex(
                @"titleText.*?text\\"":\\""(.*?)\\""}",
                RegexOptions.Singleline);

            this.Title = playlistname.Match(source).Groups[1].Value.Trim();
            this.Title = Regex.Unescape(this.Title);

            // Find video id and title in any order
            var titleId = new Regex(
                @"playlistPanelVideoRenderer.*?title.*?text\\"":\\""(.*?)\\""}.*?videoId\\"":\\""(.*?)\\""",
                RegexOptions.Compiled | RegexOptions.Singleline);

            // Find duration. Private/deleted e.g. videos does not have duration
            var duration = new Regex(
                @"timestamp"">.*?>(.*?)<",
                RegexOptions.Compiled | RegexOptions.Singleline);

            // Leaving this as null allow duplicates
            List<string> ids = this.IgnoreDuplicates ? new List<string>() : null;

            foreach (Match match in titleId.Matches(source))
            {
                string fullMatch = match.Groups[0].Value;
                string resultTitle = Regex.Unescape(match.Groups[1].Value);
                string resultId = match.Groups[2].Value;
                string resultDuration = string.Empty;

                if (ids?.Contains(resultId) == true)
                    continue;

                Match mDuration;
                if ((mDuration = duration.Match(fullMatch)).Success)
                    resultDuration = mDuration.Groups[1].Value;
                resultDuration = "??:??";

                ids?.Add(resultId);
                this.Videos.Add(new QuickVideoInfo(videoIndex + 1, // Not zero-based
                                                   resultId,
                                                   resultTitle,
                                                   resultDuration));

                videoIndex++;
            }


            return this;
        }

        public static QuickVideoInfo[] GetAll(string playlistUrl)
        {
            return new QuickPlaylist(playlistUrl).Load().Videos.ToArray();
        }
    }
}
