using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace YouTube_Downloader
{
    public class SettingsEx
    {
        public static bool AutoConvert = false;
        public static int PreferedQuality = 0;
        public static List<string> SaveToDirectories = new List<string>();
        public static int SelectedDirectory = 0;
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

            var properties = document.GetElementsByTagName("properties")[0];

            if (properties != null)
            {
                if (properties.Attributes["auto_convert"] != null)
                {
                    AutoConvert = bool.Parse(properties.Attributes["auto_convert"].Value);
                }
            }

            foreach (XmlNode node in document.GetElementsByTagName("form"))
            {
                WindowState windowState = new WindowState(node);

                SettingsEx.WindowStates.Add(windowState.FormName, windowState);
            }

            if (document.GetElementsByTagName("save_to_directories").Count > 0)
            {
                XmlNode directories = document.GetElementsByTagName("save_to_directories")[0];

                SelectedDirectory = int.Parse(directories.Attributes["selected_directory"].Value);

                foreach (XmlNode node in directories.ChildNodes)
                {
                    if (node.LocalName != "path")
                        continue;

                    SaveToDirectories.Add(node.InnerText);
                }
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
                w.WriteAttributeString("auto_convert", AutoConvert.ToString());

                foreach (WindowState windowState in SettingsEx.WindowStates.Values)
                {
                    windowState.SerializeXml(w);
                }

                w.WriteStartElement("save_to_directories");
                w.WriteAttributeString("selected_directory", SelectedDirectory.ToString());

                foreach (string directory in SaveToDirectories)
                {
                    w.WriteElementString("path", directory);
                }

                w.WriteEndElement();

                w.WriteEndElement();
                w.WriteEndDocument();

                w.Flush();
                w.Close();
            }
        }
    }
}