using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace YouTube_Downloader.Classes
{
    public class WindowState : IXmlSerializable
    {
        /// <summary>
        /// Gets the <see cref="System.Windows.Window"/> name.
        /// </summary>
        public string WindowName { get; private set; }
        /// <summary>
        /// Gets or sets the saved <see cref="System.Windows.Window"/> left location.
        /// </summary>
        public double? Left { get; set; }
        /// <summary>
        /// Gets or sets the saved <see cref="System.Windows.Window"/> top location.
        /// </summary>
        public double? Top { get; set; }
        /// <summary>
        /// Gets or sets the saved <see cref="System.Windows.Window"/> width.
        /// </summary>
        public double? Width { get; set; }
        /// <summary>
        /// Gets or sets the saved <see cref="System.Windows.Window"/> height.
        /// </summary>
        public double? Height { get; set; }
        /// <summary>
        /// Gets or sets the saved <see cref="System.Windows.Window"/> window state.
        /// </summary>
        public System.Windows.WindowState WindowWindowState { get; set; }

        /// <summary>
        /// Used by IXmlSerializable, shouldn't be used.
        /// </summary>
        private WindowState()
        {
            this.Left = this.Top = this.Width = this.Height = null;
            this.WindowWindowState = System.Windows.WindowState.Normal;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowState"/> class.
        /// </summary>
        public WindowState(string windowName)
            : this()
        {
            this.WindowName = windowName;
        }

        /// <summary>
        /// Restores <see cref="System.Windows.Window"/>'s
        /// size, location and window state. Also restores <see cref="System.Windows.Forms.ColumnHeader"/>
        /// width and <see cref="System.Windows.Forms.SplitContainer"/> splitter distance.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Window"/> to restore.</param>
        public void RestoreWindow(Window window)
        {
            if (!(this.Left == null || this.Top == null))
            {
                window.Left = (double)this.Left;
                window.Top = (double)this.Top;
            }

            if (!(this.Width == null || this.Height == null))
            {
                window.Width = (double)this.Width;
                window.Height = (double)this.Height;
            }

            window.WindowState = this.WindowWindowState;
        }

        /// <summary>
        /// Saves <see cref="System.Windows.Window"/>'s
        /// size, location and window state. Also saves <see cref="System.Windows.Forms.ColumnHeader"/>
        /// width and <see cref="System.Windows.Forms.SplitContainer"/> splitter distance.
        /// </summary>
        /// <param name="form">The <see cref="System.Windows.Window"/> to save.</param>
        public void SaveWindow(Window window)
        {
            if (!(window.WindowState == System.Windows.WindowState.Maximized))
            {
                this.Left = window.Left;
                this.Top = window.Top;
                this.Width = window.Width;
                this.Height = window.Height;
            }
            else
            {
                this.Left = window.RestoreBounds.Left;
                this.Top = window.RestoreBounds.Top;
                this.Width = window.RestoreBounds.Width;
                this.Height = window.RestoreBounds.Height;
            }

            this.WindowWindowState = window.WindowState;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        #region IXmlSerializable Members

        /// <summary>
        /// Method that returns schema information.  Not implemented.
        /// </summary>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Reads Xml when the <see cref="WindowState"/> is to be deserialized 
        /// from a stream.</summary>
        /// <param name="reader">The stream from which the object will be deserialized.</param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            this.WindowName = reader.GetAttribute("name");

            if (reader.GetAttribute("left") != null)
                this.Left = double.Parse(reader.GetAttribute("left"));

            if (reader.GetAttribute("top") != null)
                this.Top = double.Parse(reader.GetAttribute("top"));

            if (reader.GetAttribute("width") != null)
                this.Width = double.Parse(reader.GetAttribute("width"));

            if (reader.GetAttribute("height") != null)
                this.Height = double.Parse(reader.GetAttribute("height"));

            this.WindowWindowState = (System.Windows.WindowState)Enum.Parse(typeof(System.Windows.WindowState),
                reader.GetAttribute("windowState"), true);
        }

        /// <summary>
        /// Writes Xml articulating the current state of the <see cref="WindowState"/>
        /// object.</summary>
        /// <param name="writer">The stream to which this object will be serialized.</param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("name", this.WindowName);
            writer.WriteAttributeString("left", this.Left == null ? "" : this.Left.ToString());
            writer.WriteAttributeString("top", this.Top == null ? "" : this.Top.ToString());
            writer.WriteAttributeString("width", this.Width == null ? "" : this.Width.ToString());
            writer.WriteAttributeString("height", this.Height == null ? "" : this.Height.ToString());
            writer.WriteAttributeString("windowState", this.WindowWindowState.ToString());
        }

        #endregion
    }
}