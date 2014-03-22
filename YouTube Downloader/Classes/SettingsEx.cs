using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace YouTube_Downloader
{
    public class SettingsEx
    {
        public static Dictionary<string, WindowState> WindowStates = new Dictionary<string, WindowState>();

        public static void Load()
        {
            string file = Application.StartupPath + "\\YouTube Downloader.xml";

            SettingsEx.WindowStates = new Dictionary<string, WindowState>();

            if (!File.Exists(file))
                return;

            XmlDocument document = new XmlDocument();

            document.LoadXml(File.ReadAllText(file));

            if (!document.HasChildNodes)
                return;

            foreach (XmlNode node in document.GetElementsByTagName("form"))
            {
                WindowState windowState = new WindowState(node);

                SettingsEx.WindowStates.Add(windowState.FormName, windowState);
            }
        }

        public static void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;

            string file = Application.StartupPath + "\\YouTube Downloader.xml";

            using (XmlWriter w = XmlWriter.Create(file, settings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("properties");

                foreach (WindowState windowState in SettingsEx.WindowStates.Values)
                {
                    windowState.SerializeXml(w);
                }

                w.WriteEndElement();
                w.WriteEndDocument();

                w.Flush();
                w.Close();
            }
        }
    }
}