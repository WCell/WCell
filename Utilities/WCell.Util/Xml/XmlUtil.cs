using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Threading;
using System.Collections;
using System.Xml.Serialization;
using WCell.Util.Logging;

namespace WCell.Util.Xml
{
	/// <summary>
	/// TODO: Allow case-insensitive node names
	/// </summary>
	public static class XmlUtil
	{
		public static CultureInfo OrigCulture;
		/// <summary>
		/// We needs this for correct parsing.
		/// </summary>
		public static readonly CultureInfo DefaultCulture = CultureInfo.GetCultureInfo("en-US");

		/// <summary>
		/// Ensure a default culture, so float-comma and other values are parsed correctly on all systems
		/// </summary>
		public static void EnsureCulture()
		{
			if (Thread.CurrentThread.CurrentCulture != DefaultCulture)
			{
				OrigCulture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = DefaultCulture;
			}
		}

		/// <summary>
		/// Reset system-culture after parsing
		/// </summary>
		public static void ResetCulture()
		{
			if (OrigCulture != null)
			{
				Thread.CurrentThread.CurrentCulture = OrigCulture;
			}
		}



		public static string ReadString(this XmlNode node, string name)
		{
			return node[name].InnerText;
		}

		public static bool ReadBool(this XmlNode node, string name)
		{
			var value = node[name].InnerText;
			return value == "1" || value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
		}

		public static int ReadInt(this XmlNode node, string name)
		{
			return ReadInt(node, name, 0);
		}

		public static int ReadInt(this XmlNode node, string name, int defaultVal)
		{
			var value = node[name].InnerText;
			int x;
			if (int.TryParse(value, out x))
				return x;
			return defaultVal;
		}

		public static uint ReadUInt(this XmlNode node, string name)
		{
			return ReadUInt(node, name, 0);
		}

		public static uint ReadUInt(this XmlNode node, string name, uint defaultVal)
		{
			var value = node[name].InnerText;
			uint val;
			if (!uint.TryParse(value, out val))
			{
				val = defaultVal;
			}
			return val;
		}

		public static float ReadFloat(this XmlNode node, string name)
		{
			return ReadFloat(node, name, 0);
		}

		public static float ReadFloat(this XmlNode node, string name, float defaultVal)
		{
			var value = node[name].InnerText;
			float val;
			if (!float.TryParse(value, out val))
				val = defaultVal;
			return val;
		}

		public static E ReadEnum<E>(this XmlNode node, string name)
		{
			var value = node[name].InnerText;
			return (E)Enum.Parse(typeof(E), value);
		}

		public static void SkipEmptyNodes(this XmlReader reader)
		{
			while (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.Comment)
			{
				reader.ReadInnerXml();
			}
		}

		public static void WriteCollection(this XmlWriter writer, IEnumerable col, string itemName)
		{
			foreach (var val in col)
			{
				writer.WriteStartElement(itemName);
				if (val == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Invalid null-element in Collection: " + itemName);
				}
				else if (val is IXmlSerializable)
				{
					((IXmlSerializable)val).WriteXml(writer);
				}
				else
				{
					writer.WriteString(val.ToString());
				}
				writer.WriteEndElement();
			}
		}
	}
}