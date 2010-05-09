using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.PacketAnalyzer.Xml
{
	public class Condition
	{
		public Condition()
		{

		}

		[XmlElement("If")]
		public string Value
		{
			get;
			set;
		}

		[XmlElement("List", typeof(ListPacketSegment))]
		[XmlElement("Complex", typeof(ComplexPacketSegment))]
		[XmlElement("Conditional", typeof(ConditionalPacketSegment))]
		[XmlElement("Switch", typeof(SwitchPacketSegment))]
		[XmlElement("Simple", typeof(PacketSegment))]
		public PacketSegment Segment
		{
			get;
			set;
		}

		public static Condition[] Convert(IDictionary<object, PacketSegment> map)
		{
			Condition[] entries = new Condition[map.Count];

			int i = 0;
			foreach (var pair in map)
			{
				entries[i++] = new Condition { Value = pair.Key.ToString(), Segment = pair.Value };
			}
			return entries;
		}

		public static void Fill(Condition[] entries, IDictionary<object, PacketSegment> map, Type enumType)
		{
			map.Clear();
			foreach (var entry in entries)
			{
				var obj = Enum.Parse(enumType, entry.Value);
				map.Add(obj, entry.Segment);
			}
		}
	}
}
