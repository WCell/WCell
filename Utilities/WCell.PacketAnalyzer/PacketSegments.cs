using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Util.Logging;
using WCell.Core.Network;
using WCell.Util;

namespace WCell.PacketAnalysis
{
	/// <summary>
	/// The same as ListPacketSegmentStructure, just that the length of the list is a constant
	/// </summary>
	public class StaticListPacketSegmentStructure : ListPacketSegmentStructure
	{
		public StaticListPacketSegmentStructure()
		{
		}

		public StaticListPacketSegmentStructure(int length, string name, params PacketSegmentStructure[] segments)
			: base(SimpleType.NotSimple, name, segments)
		{
			Length = length;
		}

		[XmlAttribute]
		public int Length
		{
			get;
			set;
		}

		protected override int GetLength(PacketParser parser, ParsedSegment parent)
		{
			return Length;
		}
	}

	public class FinalListPacketSegmentStructure : ListPacketSegmentStructure
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public FinalListPacketSegmentStructure()
		{
		}

		public FinalListPacketSegmentStructure(int length, string name, params PacketSegmentStructure[] segments)
			: base(SimpleType.NotSimple, name, segments)
		{
			Length = length;
		}

		[XmlAttribute]
		public int Length
		{
			get;
			set;
		}

		public override void Parse(PacketParser parser, ParsedSegment parent)
		{
			var list = new List<ParsedSegment>();
			while (parser.Packet.Position < parser.Packet.Length)
			{
				var element = new ParsedSegment(this, null);
				list.Add(element);

				foreach (var segment in Segments)
				{
					segment.Parse(parser, element);
				}
			}

			var parsedSegment = new ParsedSegment(this, list.Count);
			parsedSegment.List.AddRange(list);
			parent.AddChild(parser, parsedSegment);
		}

		protected override int GetLength(PacketParser parser, ParsedSegment parent)
		{
			throw new Exception(GetType().Name + " has no fixed length.");
		}
	}

	public class ListPacketSegmentStructure : ComplexPacketSegmentStructure
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		private const string LenExprVarName = "{0}";

		public const int MaxListLength = 30000;

		public ListPacketSegmentStructure()
		{
		}

		public ListPacketSegmentStructure(SimpleType type, string name, params PacketSegmentStructure[] segments)
			: base(name, segments)
		{
			Type = type;
		}

		public ListPacketSegmentStructure(string name, string lengthSegmentName,
			params PacketSegmentStructure[] segments)
			: base(name, segments)
		{
			LengthSegmentName = lengthSegmentName;
		}

		public ListPacketSegmentStructure(string name, PacketSegmentStructure lengthSegment,
			params PacketSegmentStructure[] segments)
			: base(name, segments)
		{
			LengthSegment = lengthSegment;
		}

		/// <summary>
		/// The name of the Segment that contains the length (usually not necessary)
		/// </summary>
		[XmlAttribute("LengthSegment")]
		public string LengthSegmentName { get; set; }

		[XmlAttribute("LengthExpr")]
		public string LengthSegmentExpr { get; set; }

		[XmlIgnore]
		public override SimpleType Type
		{
			get
			{
				return LengthSegment != null ? LengthSegment.Type : base.Type;
			}
			set
			{
				if (LengthSegment != null)
				{
					throw new Exception("Cannot override Type of Lists that have the Length defined in a seperate segment.");
					//LengthSegment.Type = value;
				}
				base.Type = value;
			}
		}

		/// <summary>
		/// The Segment that contains the length (usually not necessary)
		/// </summary>
		[XmlIgnore]
		public PacketSegmentStructure LengthSegment
		{
			get;
			set;
		}

		/// <summary>
		/// Reads a single element by default
		/// </summary>
		[XmlIgnore]
		public override int MaxLength
		{
			get
			{
				//var len = LengthSegment == null ? 1 : 0;
				var len = 1;
				foreach (var segment in Segments)
				{
					len += segment.MaxLength;
				}
				return len;
			}
		}

		[XmlIgnore]
		public override int Index
		{
			get
			{
				return m_index;
			}
			internal set
			{
				m_index = value;
				value++;
				foreach (var segment in Segments)
				{
					segment.Index = value;
					value += segment.MaxLength;
				}
			}
		}

		internal override void Init(PacketDefinition def)
		{
			if (Segments.Count == 0)
			{
				throw new Exception("List " + Name + " in Defintion for " + def.PacketIds.ToString(", ") + " has no structure defined.");
			}

			if (string.IsNullOrEmpty(Name))
			{
				throw new Exception("List in " + def.PacketIds.ToString(", ") + " has no Name but Name is required.");
			}

			if (LengthSegment == null)
			{
				if (!string.IsNullOrEmpty(LengthSegmentName))
				{
					if ((LengthSegment = def.GetSegment(LengthSegmentName)) == null)
					{
						throw new ArgumentException("Length-Segment \"" + LengthSegmentName + "\" for ListSegment " + this + " does not exist.");
					}
				}
			}

			foreach (var segment in Segments)
			{
				segment.Init(def);
			}
		}

		public override void Parse(PacketParser parser, ParsedSegment parent)
		{
			var length = GetLength(parser, parent);
			if (length > MaxListLength)
			{
				log.Warn("Found list with length {0} which exceeds the MaxLength: {1} in PacketDefinition {2}, Segment {3} - " +
					"Packet definition is probably outdated.",
					length, MaxListLength, parser.Definition, this);
#if DEBUG
				return;
#endif
			}
			var parsedSegment = new ParsedSegment(this, length);
			parent.AddChild(parser, parsedSegment);
			while (length-- > 0)
			{
				var element = new ParsedSegment(this, null);
				parsedSegment.List.Add(element);

				foreach (var segment in Segments)
				{
					segment.Parse(parser, element);
				}
			}
		}

		protected virtual int GetLength(PacketParser parser, ParsedSegment parent)
		{
			try
			{
				if (LengthSegment != null)
				{
					ParsedSegment lengthSegment;

					if (!parser.LastParsedSegmentsByName.TryGetValue(LengthSegmentName, out lengthSegment))
					{
						throw new Exception(string.Format(
							"Could not find one occurance of List {0}'s Length-Segment '{1}' in parsed packet {2}",
							this, LengthSegmentName, parser.Packet));
					}

					var len = (int)Convert.ChangeType(lengthSegment.Value, typeof(int));

					var expr = LengthSegmentExpr;
					if (!string.IsNullOrEmpty(expr))
					{
						len = ParseLengthExpr(len);
					}
					return len;
				}

				return ListCountReaders[(int)Type](parser.Packet);
			}
			catch (Exception e)
			{
				throw new Exception("Could not parse List-Count for List " + Name + " in Packet " + parser.Packet, e);
			}
		}

		private int ParseLengthExpr(int len)
		{
			if (!LengthSegmentExpr.Contains(LenExprVarName))
			{
				throw new Exception("Length Expression for List " + this + " does not contain variable replacement (" + LenExprVarName + ")");
			}
			// parse expression
			var longLen = (long)len;
			var expr = LengthSegmentExpr.Replace(LenExprVarName, len.ToString());
			object err = null;
			if (!Utility.Eval(typeof(int), ref longLen, expr, ref err, false))
			{
				throw new Exception("Unable to evaluate expression (" + expr + ") in Segment: " + this + " - " + err);
			}
			return (int)longLen;
		}
	}

	public class ComplexPacketSegmentStructure : PacketSegmentStructure
	{
		[XmlElement("StaticList", typeof(StaticListPacketSegmentStructure))]
		[XmlElement("List", typeof(ListPacketSegmentStructure))]
		[XmlElement("FinalList", typeof(FinalListPacketSegmentStructure))]
		[XmlElement("Complex", typeof(ComplexPacketSegmentStructure))]
		[XmlElement("Switch", typeof(SwitchPacketSegmentStructure))]
		[XmlElement("Simple", typeof(PacketSegmentStructure))]
		public List<PacketSegmentStructure> Segments;

		public ComplexPacketSegmentStructure()
		{
			Segments = new List<PacketSegmentStructure>();
		}

		public ComplexPacketSegmentStructure(List<PacketSegmentStructure> segments)
			: this(null, segments)
		{
		}

		public ComplexPacketSegmentStructure(params PacketSegmentStructure[] segments)
			: this(null, segments)
		{
		}

		public ComplexPacketSegmentStructure(string name, params PacketSegmentStructure[] segments)
			: this(name, segments.ToList())
		{
		}

		public ComplexPacketSegmentStructure(string name, List<PacketSegmentStructure> segments)
			: base(SimpleType.NotSimple, name)
		{
			Segments = segments;
		}

		/// <summary>
		/// Reads a single element by default
		/// </summary>
		[XmlIgnore]
		public override int MaxLength
		{
			get
			{
				var len = 0;
				foreach (var segment in Segments)
				{
					len += segment.MaxLength;
				}
				return len;
			}
		}

		[XmlIgnore]
		public override int Index
		{
			get
			{
				return m_index;
			}
			internal set
			{
				m_index = value;
				foreach (var segment in Segments)
				{
					segment.Index = value;
					value += segment.MaxLength;
				}
			}
		}

		internal override void Init(PacketDefinition def)
		{
			foreach (var segment in Segments)
			{
				segment.Init(def);
			}
		}

		public override void Parse(PacketParser parser, ParsedSegment parent)
		{
			if (Segments.Count > 1 && Name != null)
			{
				var parsedSegment = new ParsedSegment(this, null);
				parent.AddChild(parser, parsedSegment);
				parent = parsedSegment;
			}

			foreach (var segment in Segments)
			{
				segment.Parse(parser, parent);
			}
		}

		public override IEnumerator<PacketSegmentStructure> GetEnumerator()
		{
			foreach (var segment in Segments)
			{
				yield return segment;
			}
		}

		public static implicit operator ComplexPacketSegmentStructure(List<PacketSegmentStructure> segments)
		{
			return new ComplexPacketSegmentStructure(segments);
		}
	}

	/// <summary>
	/// A PacketSegmentStructure-wrapper whose content decides a switch in another segment of the packet
	/// </summary>
	public class SwitchPacketSegmentStructure : PacketSegmentStructure
	{
		public SwitchPacketSegmentStructure()
		{
			Cases = new List<SwitchCase>();
		}

		public SwitchPacketSegmentStructure(string name, string refName, params SwitchCase[] map)
			: base(SimpleType.NotSimple, name)
		{
			Cases = map.ToList();
			ReferenceName = refName;
		}

		public SwitchPacketSegmentStructure(string name, PacketSegmentStructure refSegment, params SwitchCase[] map)
			: base(SimpleType.NotSimple, name)
		{
			Cases = map.ToList();
			ReferenceSegment = refSegment;
		}

		[XmlIgnore]
		public override SimpleType Type
		{
			get
			{
				return base.Type;
			}
			set
			{
				base.Type = value;
			}
		}

		[XmlAttribute("CompareWith")]
		public string ReferenceName
		{
			get;
			set;
		}

		[XmlIgnore]
		public PacketSegmentStructure ReferenceSegment
		{
			get;
			set;
		}

		[XmlElement("Case", typeof(SwitchCase))]
		public List<SwitchCase> Cases
		{
			get;
			set;
		}

		[XmlIgnore]
		public override Type SegmentType
		{
			get
			{
				return base.SegmentType;
			}
			internal set
			{
				base.SegmentType = value;
			}
		}

		public List<PacketSegmentStructure> MatchSegments(object value)
		{
			var list = new List<PacketSegmentStructure>();
			foreach (var condition in Cases)
			{
				if (condition.Matches(value))
				{
					list.Add(condition.Structure);
				}
			}
			return list;
		}

		internal override void Init(PacketDefinition def)
		{
			if (Cases.Count == 0)
			{
				throw new ArgumentNullException(string.Format("No Conditions found in Switch {0} for packet {1}", this, def));
			}

			if (ReferenceSegment == null)
			{
				if (string.IsNullOrEmpty(ReferenceName))
				{
					throw new ArgumentException(def.PacketIds.ToString(", ") + " defines SwitchSegment without CompareWith-Attribute.");
				}
				if ((ReferenceSegment = def.GetSegment(ReferenceName)) == null)
				{
					throw new ArgumentException("CompareWith-Segment \"" + ReferenceName + "\" for SwitchSegment " + this + " does not exist.");
				}
			}

			foreach (var condition in Cases)
			{
				try
				{
					condition.Init(this, def);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format("Failed to parse Switch {0} - Invalid Case: {1}", this, condition), e);
				}
			}
		}

		/// <summary>
		/// The maximum length of this segment
		/// </summary>
		[XmlIgnore]
		public override int MaxLength
		{
			get
			{
				var len = 0;
				foreach (var condition in Cases)
				{
					len += condition.Structure.MaxLength;
				}
				return len;
			}
		}

		[XmlIgnore]
		public override int Index
		{
			get
			{
				return m_index;
			}
			internal set
			{
				m_index = value;
				foreach (var condition in Cases)
				{
					condition.Structure.Index = value;
				}
			}
		}

		public override void Parse(PacketParser parser, ParsedSegment parent)
		{
			ParsedSegment refSegment;
			if (!parser.LastParsedSegmentsByName.TryGetValue(ReferenceName, out refSegment))
			{
				throw new Exception(string.Format(
					"Could not find one occurance of Switch {0}'s CompareWith-Segment '{1}' in parsed packet {2}",
					this, ReferenceName, parser.Packet));
			}

			var value = refSegment.Value;
			if (value == null)
			{
				throw new Exception(string.Format(
					"Value of Switch {0}'s CompareWith-Segment '{1}' in parsed packet {2} is null.",
					this, ReferenceName, parser.Packet));
			}

			var segments = MatchSegments(value);
			foreach (var segment in segments)
			{
				segment.Parse(parser, parent);
			}
		}

		public override IEnumerator<PacketSegmentStructure> GetEnumerator()
		{
			foreach (var segment in Cases)
			{
				yield return segment.Structure;
			}
		}

		public override string ToString()
		{
			return Name + " (ComparesWith: " + ReferenceName + ")";
		}
	}

	[XmlRoot("Segment")]
	public class PacketSegmentStructure
	{
		static readonly EmptySegmentIterator EmptyStructure = new EmptySegmentIterator();
		public static readonly Func<PacketIn, int>[] ListCountReaders = new Func<Func<PacketIn, int>[]>(() => {
			var funcs = new Func<PacketIn, int>[(int)SimpleType.Count];

			funcs[(int)SimpleType.UShort] = packet => packet.RemainingLength >= 2 ? (int)packet.ReadUInt16() : 0;
			funcs[(int)SimpleType.UInt] = packet => packet.RemainingLength >= 4 ? (int)packet.ReadUInt32() : 0;
			funcs[(int)SimpleType.Byte] = packet => packet.RemainingLength >= 1 ?(int)packet.ReadByte() : 0;
			funcs[(int)SimpleType.Short] = packet => packet.RemainingLength >= 2 ? packet.ReadInt16() : (short)0;
			funcs[(int)SimpleType.Int] = packet => packet.RemainingLength >= 4 ? packet.ReadInt32() : 0;
			return funcs;
		})();

		int nextUnknownId = 1;
		Type m_segmentType;

		/// <summary>
		/// An optional Name for this Segment
		/// </summary>
		[XmlElement]
		public String Name;

		protected int m_index;

		public PacketSegmentStructure()
		{
		}

		public PacketSegmentStructure(SimpleType type, string name, string segmentType)
			: this(type, name)
		{
			SegmentTypeName = segmentType;
		}

		public PacketSegmentStructure(SimpleType type, string name, Type segmentType)
			: this(type, name)
		{
			SegmentType = segmentType;
		}

		public PacketSegmentStructure(SimpleType type, string name)
		{
			Type = type;
			Name = name;
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement]
		public virtual SimpleType Type
		{
			get;
			set;
		}

		[XmlElement("SegmentType")]
		public string SegmentTypeName
		{
			get
			{
				if (SegmentType != null)
				{
					return SegmentType.FullName;
				}
				return "";
			}
			set
			{
				if (value.Length > 0)
				{
					SegmentType = Utility.GetType(value);
				}
			}
		}

		/// <summary>
		/// The type in which values should be converted after parsing (should be an enum)
		/// </summary>
		[XmlIgnore]
		public virtual Type SegmentType
		{
			get
			{
				return m_segmentType;
			}
			internal set
			{
				m_segmentType = value;
			}
		}

		[XmlElement("Encoding")]
		public string EncodingName
		{
			get;
			set;
		}

		/// <summary>
		/// The index of this segment within the packet's structure
		/// </summary>
		[XmlIgnore]
		public virtual int Index
		{
			get
			{
				return m_index;
			}
			internal set
			{
				m_index = value;
			}
		}

		[XmlIgnore]
		public virtual Encoding Encoding
		{
			get;
			set;
		}

		/// <summary>
		/// A Single element by default
		/// </summary>
		[XmlIgnore]
		public virtual int MaxLength
		{
			get
			{
				return 1;
			}
		}

		[XmlIgnore]
		public string NotEmptyName
		{
			get
			{
				return Name ?? "Unnamed" + nextUnknownId++;
			}
		}

		/// <summary>
		/// Converts and returns the given value into the underlying SimpleType of this Segment.
		/// </summary>
		public object ConvertToUnderlyingType(object value)
		{
			//return SimpleTypes.SimpleTypeConverters[(int)Type](value);
			if (SegmentType != null)
			{
				
			}
			return value;
		}

		internal virtual void Init(PacketDefinition def)
		{
			if (string.IsNullOrEmpty(EncodingName))
			{
				Encoding = Encoding.ASCII;
			}
			else
			{
				Encoding = Encoding.GetEncoding(EncodingName);
			}
		}


		public virtual void Parse(PacketParser parser, ParsedSegment parent)
		{
			var reader = SimpleTypes.GetReader(Type);
			var value = reader(this, parser);
			if (SegmentType != null)
			{
				try
				{
					value = Utility.ChangeType(value, SegmentType, true);
				}
				catch (OverflowException e)
				{
					throw new Exception(string.Format("Wrong underlying type {0} for Enum {1} in PacketDefinition {2} in Segment {3}",
						Type, m_segmentType, parser.Packet, Name), e);
				}
				catch (Exception e)
				{
					Console.WriteLine("Could not parse value " + value + " in Segment \"" + this + "\": " + e);
				}
			}

			parent.AddChild(parser, new ParsedSegment(this, value));
		}

		public virtual IEnumerator<PacketSegmentStructure> GetEnumerator()
		{
			return EmptyStructure;
		}

		class EmptySegmentIterator : IEnumerator<PacketSegmentStructure>
		{

			#region IEnumerator<PacketSegmentStructure> Members

			public PacketSegmentStructure Current
			{
				get { return null; }
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get { return null; }
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}

			#endregion
		}

		public override string ToString()
		{
			return GetType().Name + " \"" + Name + "\" (" + Type + ")";
		}
	}
}