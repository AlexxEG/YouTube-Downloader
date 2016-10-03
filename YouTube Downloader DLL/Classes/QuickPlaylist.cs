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

            string source = wc.DownloadString(playlistUrl);

            Match m = null;

            var loadmore = new Regex(@"data-uix-load-more-href=""([^ ""]*)", RegexOptions.Compiled);
            var id = new Regex(@"<tr.*data-video-id=""([^""]*)""", RegexOptions.Compiled);
            var title = new Regex(@"<tr.*data-title=""([^""]*)""", RegexOptions.Compiled);
            var duration = new Regex(@"<div class=""timestamp""><[^>]*>([^<]*)", RegexOptions.Compiled);

            do
            {
                if (m != null)
                {
                    source = wc.DownloadString(@"https://www.youtube.com" + m.Groups[1].Value);

                    source = Regex.Unescape(source);
                    source = HttpUtility.HtmlDecode(source);
                }

                var mId = id.Matches(source);
                var mTitle = title.Matches(source);
                var mDuration = duration.Matches(source);

                for (int i = 0; i < mId.Count; i++)
                {
                    string resultId = mId[i].Groups[1].Value;
                    string resultTitle = mTitle[i].Groups[1].Value;
                    string resultDuration = string.Empty;

                    if (mDuration.Count - 1 >= i)
                        resultDuration = mDuration[i].Groups[1].Value;
                    else
                        continue;

                    videos.Add(new QuickVideoInfo(resultId,
                                                  resultTitle,
                                                  resultDuration));
                }
            }
            while ((m = loadmore.Match(source)).Success);

            return videos.ToArray();
        }
    }
}
