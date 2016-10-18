using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace YouTube_Downloader_DLL.Classes
{
    /// <summary>
    /// Used to quickly list all videos in a playlist.
    /// </summary>
    public class QuickPlaylist
    {
        public static QuickVideoInfo[] GetAll(string playlistUrl)
        {
            var wc = new WebClient();
            var videos = new List<QuickVideoInfo>();
            int videoIndex = 0;
            string source = wc.DownloadString(playlistUrl);
            Match m = null;

            // Find the load more button
            var loadmore = new Regex(
                @"data-uix-load-more-href=""([^ ""]*)",
                RegexOptions.Compiled);

            // Find video id and title in any order
            var titleId = new Regex(
                @"<tr(?=.*?data-video-id=""(.*?)"")(?=.*?data-title=""(.*?)"").*?pl-video-edit-options",
                RegexOptions.Compiled | RegexOptions.Singleline);

            // Find duration. Private/deleted e.g. videos does not have duration
            var duration = new Regex(
                @"timestamp"">.*?>(.*?)<",
                RegexOptions.Compiled | RegexOptions.Singleline);

            do
            {
                if (m != null)
                {
                    source = wc.DownloadString(@"https://www.youtube.com" + m.Groups[1].Value);

                    source = Regex.Unescape(source);
                    source = HttpUtility.HtmlDecode(source);
                }

                foreach (Match match in titleId.Matches(source))
                {
                    string fullMatch = match.Groups[0].Value;
                    string resultId = match.Groups[1].Value;
                    string resultTitle = match.Groups[2].Value;
                    string resultDuration = string.Empty;

                    Match mDuration;
                    if ((mDuration = duration.Match(fullMatch)).Success)
                        resultDuration = mDuration.Groups[1].Value;

                    videos.Add(new QuickVideoInfo(videoIndex + 1, // Not zero-based
                                                  resultId,
                                                  resultTitle,
                                                  resultDuration));

                    videoIndex++;
                }
            }
            while ((m = loadmore.Match(source)).Success);

            return videos.ToArray();
        }
    }
}
