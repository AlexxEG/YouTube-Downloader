using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace YouTube_Downloader.Classes
{
    public class WindowState : IXmlSerializable
    {
        public string FormName { get; set; }
        public Dictionary<string, int> ColumnSizes { get; set; }
        public Dictionary<string, int> SplitterSizes { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public FormWindowState FormWindowState { get; set; }

        public WindowState()
        {
            this.ColumnSizes = new Dictionary<string, int>();
            this.SplitterSizes = new Dictionary<string, int>();
            this.Location = Point.Empty;
            this.Size = Size.Empty;
            this.FormWindowState = FormWindowState.Normal;
        }

        public WindowState(string formName)
        {
            this.FormName = formName;
            this.ColumnSizes = new Dictionary<string, int>();
            this.SplitterSizes = new Dictionary<string, int>();
            this.Location = Point.Empty;
            this.Size = Size.Empty;
            this.FormWindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// Restores <see cref="System.Windows.Forms.Form"/>'s
        /// size, location and window state. Also restores <see cref="System.Windows.Forms.ColumnHeader"/>
        /// width and <see cref="System.Windows.Forms.SplitContainer"/> splitter distance.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Forms.Form"/> to restore.</param>
        public void RestoreForm(Form form)
        {
            if (!this.Location.IsEmpty)
            {
                form.Location = this.Location;
            }

            if (!this.Size.IsEmpty)
            {
                form.Size = this.Size;
            }

            form.WindowState = this.FormWindowState;

            RestoreColumns(form);
            RestoreSplitContainers(form);
        }

        /// <summary>
        /// Saves <see cref="System.Windows.Forms.Form"/>'s
        /// size, location and window state. Also saves <see cref="System.Windows.Forms.ColumnHeader"/>
        /// width and <see cref="System.Windows.Forms.SplitContainer"/> splitter distance.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Forms.Form"/> to save.</param>
        public void SaveForm(Form form)
        {
            if (!(form.WindowState == FormWindowState.Maximized))
            {
                this.Location = form.Location;
                this.Size = form.Size;
            }
            else
            {
                this.Location = form.RestoreBounds.Location;
                this.Size = form.RestoreBounds.Size;
            }

            this.FormWindowState = form.WindowState;

            foreach (ColumnHeader col in GetColumns(form.Controls))
            {
                // Skip column if Name is null or empty, since it can't be identified.
                if (string.IsNullOrEmpty(col.ListView.Name))
                    continue;

                string key = string.Format("{0} - {1}", col.ListView.Name, col.DisplayIndex);

                if (this.ColumnSizes.ContainsKey(key))
                {
                    this.ColumnSizes[key] = col.Width;
                }
                else
                {
                    this.ColumnSizes.Add(key, col.Width);
                }
            }

            foreach (SplitContainer splitContainer in GetSplitContainers(form.Controls))
            {
                string key = splitContainer.Name;

                // Skip container if Name is null or empty, since it can't be identified.
                if (string.IsNullOrEmpty(key))
                    continue;

                if (this.SplitterSizes.ContainsKey(key))
                {
                    this.SplitterSizes[key] = splitContainer.SplitterDistance;
                }
                else
                {
                    this.SplitterSizes.Add(key, splitContainer.SplitterDistance);
                }
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool booIsEmpty = reader.IsEmptyElement;

            if (booIsEmpty)
                return;

            this.FormName = reader.GetAttribute("name");
            this.Location = new Point(int.Parse(reader.GetAttribute("x")), int.Parse(reader.GetAttribute("y")));
            this.Size = new Size(int.Parse(reader.GetAttribute("width")), int.Parse(reader.GetAttribute("height")));
            this.FormWindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), reader.GetAttribute("windowState"), true);

            while (reader.Read())
            {
                switch (reader.LocalName)
                {
                    case "column":
                        {
                            string name = reader.GetAttribute("name");
                            int width = int.Parse(reader.GetAttribute("width"));

                            this.ColumnSizes.Add(name, width);
                            break;
                        }
                    case "splitter":
                        {
                            string name = reader.GetAttribute("name");
                            int distance = int.Parse(reader.GetAttribute("distance"));

                            this.SplitterSizes.Add(name, distance);
                            break;
                        }
                }
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("name", this.FormName);
            writer.WriteAttributeString("x", this.Location.X.ToString());
            writer.WriteAttributeString("y", this.Location.Y.ToString());
            writer.WriteAttributeString("width", this.Size.Width.ToString());
            writer.WriteAttributeString("height", this.Size.Height.ToString());
            writer.WriteAttributeString("windowState", this.FormWindowState.ToString());

            foreach (KeyValuePair<string, int> pair in this.ColumnSizes)
            {
                writer.WriteStartElement("column");
                writer.WriteAttributeString("name", pair.Key);
                writer.WriteAttributeString("width", pair.Value.ToString());
                writer.WriteEndElement();
            }

            foreach (KeyValuePair<string, int> pair in this.SplitterSizes)
            {
                writer.WriteStartElement("splitter");
                writer.WriteAttributeString("name", pair.Key);
                writer.WriteAttributeString("distance", pair.Value.ToString());
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Returns all <see cref="System.Windows.Forms.ColumnHeader"/> in
        /// <see cref="System.Windows.Forms.ListView"/> by looking through given
        /// <see cref="System.Windows.Forms.Control.ControlCollection"/>.
        /// </summary>
        /// <param name="controls">The <see cref="System.Windows.Forms.Control.ControlCollection"/> to look through.</param>
        private ICollection<ColumnHeader> GetColumns(Control.ControlCollection controls)
        {
            List<ColumnHeader> columns = new List<ColumnHeader>();

            foreach (Control c in controls)
            {
                if (c is ListView)
                {
                    foreach (ColumnHeader col in (c as ListView).Columns)
                    {
                        columns.Add(col);
                    }
                }
                else if (c.Controls.Count > 0)
                {
                    columns.AddRange(GetColumns(c.Controls));
                }
            }

            return columns.ToArray();
        }

        /// <summary>
        /// Returns all <see cref="System.Windows.Forms.SplitContainer"/> in given
        /// <see cref="System.Windows.Forms.Control.ControlCollection"/>.
        /// </summary>
        /// <param name="controls">The <see cref="System.Windows.Forms.Control.ControlCollection"/> to look through.</param>
        private ICollection<SplitContainer> GetSplitContainers(Control.ControlCollection controls)
        {
            List<SplitContainer> columns = new List<SplitContainer>();

            foreach (Control c in controls)
            {
                if (c is SplitContainer)
                {
                    columns.Add((SplitContainer)c);
                }
                else if (c.Controls.Count > 0)
                {
                    columns.AddRange(GetSplitContainers(c.Controls));
                }
            }

            return columns.ToArray();
        }

        /// <summary>
        /// Restores all <see cref="System.Windows.Forms.ColumnHeader.Width"/> in
        /// <see cref="System.Windows.Forms.Form"/>.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Forms.Form"/> to restore columns from.</param>
        private void RestoreColumns(Form form)
        {
            foreach (ColumnHeader col in GetColumns(form.Controls))
            {
                string key = string.Format("{0} - {1}", col.ListView.Name, col.DisplayIndex);

                if (this.ColumnSizes.ContainsKey(key))
                {
                    col.Width = this.ColumnSizes[key];
                }
            }
        }

        /// <summary>
        /// Restores all <see cref="System.Windows.Forms.SplitContainer.SplitterDistance"/> in
        /// <see cref="System.Windows.Forms.Form"/>.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Forms.Form"/> to restore columns from.</param>
        private void RestoreSplitContainers(Form form)
        {
            foreach (SplitContainer splitContainer in GetSplitContainers(form.Controls))
            {
                string key = splitContainer.Name;

                if (this.SplitterSizes.ContainsKey(key))
                {
                    splitContainer.SplitterDistance = this.SplitterSizes[key];
                }
            }
        }
    }
}