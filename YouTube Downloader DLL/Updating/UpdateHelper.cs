using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YouTube_Downloader_DLL.Classes;

namespace YouTube_Downloader_DLL.Updating
{
    public class UpdateHelper
    {
        public const string GetReleasesAPIUrl = "https://api.github.com/repos/AlexxEG/YouTube-Downloader/releases";
        public const string UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2;)";

        public static async Task<Update> GetLatestUpdateAsync()
        {
            var response = await GetJsonResponseAsync(GetReleasesAPIUrl);
            string downloadUrl = response[0]["assets"][0]["browser_download_url"].ToString();
            string version = response[0]["tag_name"].ToString().TrimStart('v');

            return new Update(downloadUrl, version);
        }

        /// <summary>
        /// Returns true if given update is newer than local version.
        /// </summary>
        public static bool IsUpdate(Update update)
        {
            return Common.Version < update.Version;
        }

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                return IsUpdate(await GetLatestUpdateAsync());
            }
            catch
            {
                return false;
            }
        }

        private static async Task<JArray> GetJsonResponseAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(GetReleasesAPIUrl);
            request.KeepAlive = false;
            request.UserAgent = UserAgent;

            string jsonString = string.Empty;
            JArray json = null;

            await Task.Run(delegate
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                jsonString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                json = JsonConvert.DeserializeObject<JArray>(jsonString);
            });

            return json;
        }
    }
}
