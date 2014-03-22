using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace YouTube_Downloader
{
    public class WindowState
    {
        public string FormName { get; set; }
        public Dictionary<string, int> ColumnSizes { get; set; }
        public Dictionary<string, int> SplitterSizes { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public FormWindowState FormWindowState { get; set; }

        public WindowState(string formName)
        {
            this.FormName = formName;
            this.ColumnSizes = new Dictionary<string, int>();
            this.SplitterSizes = new Dictionary<string, int>();
            this.Location = Point.Empty;
            this.Size = Size.Empty;
            this.FormWindowState = FormWindowState.Normal;
        }

        public WindowState(XmlNode node)
        {
            string formName = node.Attributes["name"].Value;
            Point location = new Point(int.Parse(node.Attributes["x"].Value), int.Parse(node.Attributes["y"].Value));
            Size size = new Size(int.Parse(node.Attributes["width"].Value), int.Parse(node.Attributes["height"].Value));
            FormWindowState state = (FormWindowState)Enum.Parse(typeof(FormWindowState), node.Attributes["windowState"].Value, true);

            this.FormName = formName;
            this.Location = location;
            this.Size = size;
            this.FormWindowState = state;
            this.ColumnSizes = new Dictionary<string, int>();
            this.SplitterSizes = new Dictionary<string, int>();

            foreach (XmlNode childNode in node.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "column":
                        string columnName = childNode.Attributes["name"].Value;
                        int width = int.Parse(childNode.Attributes["width"].Value);

                        this.ColumnSizes.Add(columnName, width);
                        break;
                    case "splitter":
                        string splitterName = childNode.Attributes["name"].Value;
                        int distance = int.Parse(childNode.Attributes["distance"].Value);

                        this.SplitterSizes.Add(splitterName, distance);
                        break;
                    default:
                        continue;
                }
            }
        }

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

        public void SerializeXml(XmlWriter writer)
        {
            writer.WriteStartElement("form");
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

            writer.WriteEndElement();
        }
    }
}