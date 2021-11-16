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
        public const string GitHubAPIGetReleases = "https://api.github.com/repos/AlexxEG/YouTube-Downloader/releases";
        public const string UserAgent = "YouTube-Downloader Update Checker";

        public static async Task<Update> GetLatestUpdateAsync()
        {
            var response = await GetJsonResponseAsync(GitHubAPIGetReleases);
            string downloadUrl = response[0]["assets"][0]["browser_download_url"].ToString();
            string version = response[0]["tag_name"].ToString().TrimStart('v');

            return new Update(downloadUrl, version);
        }

        /// <summary>
        /// Returns true if given update is newer than local version.
        /// </summary>
        public static bool IsUpdate(Update update) => Common.Version < update.Version;

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
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.KeepAlive = false;
            request.UserAgent = UserAgent;

            var response = await request.GetResponseAsync() as HttpWebResponse;
            var jsonString = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();

            return JsonConvert.DeserializeObject<JArray>(jsonString);
        }
    }
}
