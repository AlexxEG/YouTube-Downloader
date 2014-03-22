//Copy rights are reserved for Akram kamal qassas
//Email me, Akramnet4u@hotmail.com
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace YouTube_Downloader
{
    public static class FormatLeftTime
    {
        private static string[] TimeUnitsNames = { "Milli", "Sec", "Min", "Hour", "Day", "Month", "Year", "Decade", "Century" };
        private static int[] TimeUnitsValue = { 1000, 60, 60, 24, 30, 12, 10, 10 };//refrernce unit is milli
        public static string Format(long millis)
        {
            string format = "";
            for (int i = 0; i < TimeUnitsValue.Length; i++)
            {
                long y = millis % TimeUnitsValue[i];
                millis = millis / TimeUnitsValue[i];
                if (y == 0) continue;
                format = y + " " + TimeUnitsNames[i] + " , " + format;
            }

            format = format.Trim(',', ' ');
            if (format == "") return "0 Sec";
            else return format;
        }
    }
    public static class Helper
    {

        /// <summary>
        /// Decode a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(string str)
        {
            return System.Web.HttpUtility.UrlDecode(str);
        }

        public static bool isValidUrl(string url)
        {
            string pattern = @"^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }
        /// <summary>
        /// Gets the txt that lies between these two strings
        /// </summary>
        public static string GetTxtBtwn(string input, string start, string end, int startIndex)
        {
            return GetTxtBtwn(input, start, end, startIndex, false);
        }
        /// <summary>
        /// Gets the txt that lies between these two strings
        /// </summary>
        public static string GetLastTxtBtwn(string input, string start, string end, int startIndex)
        {
            return GetTxtBtwn(input, start, end, startIndex, true);
        }
        /// <summary>
        /// Gets the txt that lies between these two strings
        /// </summary>
        private static string GetTxtBtwn(string input, string start, string end, int startIndex, bool UseLastIndexOf)
        {
            int index1 = UseLastIndexOf ? input.LastIndexOf(start, startIndex) :
                                          input.IndexOf(start, startIndex);
            if (index1 == -1) return "";
            index1 += start.Length;
            int index2 = input.IndexOf(end, index1);
            if (index2 == -1) return input.Substring(index1);
            return input.Substring(index1, index2 - index1);
        }

        /// <summary>
        /// Split the input text for this pattren
        /// </summary>
        public static string[] Split(string input, string pattren)
        {
            return Regex.Split(input, pattren);
        }


        /// <summary>
        /// Returns the content of a given web adress as string.
        /// </summary>
        /// <param name="Url">URL of the webpage</param>
        /// <returns>Website content</returns>
        public static string DownloadWebPage(string Url)
        {
            return DownloadWebPage(Url, null);
        }

        private static string DownloadWebPage(string Url, string stopLine)
        {
            try
            {
                // Open a connection
                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(Url);
                WebRequestObject.Proxy = InitialProxy();
                // You can also specify additional header values like 
                // the user agent or the referer:
                WebRequestObject.UserAgent = ".NET Framework/2.0";

                // Request response:
                WebResponse Response = WebRequestObject.GetResponse();

                // Open data stream:
                Stream WebStream = Response.GetResponseStream();

                // Create reader object:
                StreamReader Reader = new StreamReader(WebStream);
                string PageContent = "", line;
                if (stopLine == null)
                    PageContent = Reader.ReadToEnd();
                else while (!Reader.EndOfStream)
                    {
                        line = Reader.ReadLine();
                        PageContent += line + Environment.NewLine;
                        if (line.Contains(stopLine)) break;
                    }
                // Cleanup
                Reader.Close();
                WebStream.Close();
                Response.Close();

                return PageContent;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Get the ID of a youtube video from its URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetVideoIDFromUrl(string url)
        {
            url = url.Substring(url.IndexOf("?") + 1);
            string[] props = url.Split('&');

            string videoid = "";
            foreach (string prop in props)
            {
                if (prop.StartsWith("v="))
                    videoid = prop.Substring(prop.IndexOf("v=") + 2);
            }

            return videoid;
        }


        public static IWebProxy InitialProxy()
        {
            string address = address = getIEProxy();
            if (!string.IsNullOrEmpty(address))
            {
                WebProxy proxy = new WebProxy(address);
                proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                return proxy;
            }
            else return null;
        }
        private static string getIEProxy()
        {
            var p = WebRequest.DefaultWebProxy;
            if (p == null) return null;
            WebProxy webProxy = null;
            if (p is WebProxy) webProxy = p as WebProxy;
            else
            {
                Type t = p.GetType();
                var s = t.GetProperty("WebProxy", (BindingFlags)0xfff).GetValue(p, null);
                webProxy = s as WebProxy;
            }
            if (webProxy == null || webProxy.Address == null || string.IsNullOrEmpty(webProxy.Address.AbsolutePath))
                return null;
            return webProxy.Address.Host;
        }
    }
}
