using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace YouTube_Downloader.Classes
{
    public class WindowStates : IXmlSerializable
    {
        Dictionary<string, WindowState> windowStates;

        public WindowStates()
        {
            windowStates = new Dictionary<string, WindowState>();
        }

        public void Add(string form, WindowState windowState)
        {
            this.windowStates.Add(form, windowState);
        }

        public bool Contains(string form)
        {
            return this.windowStates.ContainsKey(form);
        }

        public WindowState Get(string form)
        {
            return this.windowStates[form];
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool booIsEmpty = reader.IsEmptyElement;

            reader.ReadStartElement();

            if (booIsEmpty)
                return;

            while (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName == "WindowState")
            {
                string strWindowName = reader["name"];

                XmlSerializer xsrWindowState = new XmlSerializer(typeof(WindowState));
                windowStates.Add(strWindowName, (WindowState)xsrWindowState.Deserialize(reader));

                windowStates.ToString();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (KeyValuePair<string, WindowState> kvpWindowState in windowStates)
            {
                XmlSerializer xsrLocationInfo = new XmlSerializer(typeof(WindowState));
                xsrLocationInfo.Serialize(writer, kvpWindowState.Value);
            }
        }
    }
}
