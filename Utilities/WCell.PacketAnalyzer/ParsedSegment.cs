using System;
using System.Collections.Generic;
using WCell.Core;
using WCell.Util;

namespace WCell.PacketAnalysis
{
	public interface IParsedSegment
	{
		byte ByteValue
		{
			get;
		}

		short ShortValue
		{
			get;
		}

		ushort UShortValue
		{
			get;
		}

		uint UIntValue
		{
			get;
		}

		int IntValue
		{
			get;
		}

		long LongValue
		{
			get;
		}

		ulong ULongValue
		{
			get;
		}

		EntityId EntityIdValue
		{
			get;
		}

		string StringValue
		{
			get;
		}

		object Value
		{
			get;
			set;
		}

		List<ParsedSegment> List
		{
			get;
		}

		IParsedSegment this[int index]
		{
			get;
		}

		IParsedSegment this[string name]
		{
			get;
		}
	}

	/// <summary>
	/// Represents a part of a parsed packet
	/// </summary>
	public class ParsedSegment : IParsedSegment
	{
		/// <summary>
		/// This is used to prevent Nullrefexceptions and a lot of extra null-checks.
		/// </summary>
		public static readonly IParsedSegment Empty = new EmptyParsedSegment();

		/// <summary>
		/// If set to true, instead of null, the indexer will always return an empty ParsedSegment object.
		/// </summary>
		public static bool ReturnEmptyInsteadOfNull = false;

		readonly PacketSegmentStructure m_structure;
		readonly List<ParsedSegment> m_list;
		readonly Dictionary<string, ParsedSegment> m_subSegments;

		/// <summary>
		/// Root ctor
		/// </summary>
		public ParsedSegment()
		{
			m_subSegments = new Dictionary<string, ParsedSegment>(StringComparer.InvariantCultureIgnoreCase);
		}

		public ParsedSegment(PacketSegmentStructure structure, object value)
		{
			m_subSegments = new Dictionary<string, ParsedSegment>(StringComparer.InvariantCultureIgnoreCase);

			m_structure = structure;

			if (value is List<ParsedSegment>)
			{
				var list = (List<ParsedSegment>)value;
				Value = list.Count;

				if (IsList)
				{
					throw new Exception("Used List-CTor for Segment that is not a list.");
				}
				m_list = list;
			}
			else
			{
				Value = value;

				if (IsList)
				{
					m_list = new List<ParsedSegment>();
				}
			}
		}

		public bool IsSimple
		{
			get
			{
				return m_structure != null && m_structure.Type != SimpleType.NotSimple && !IsList;
			}
		}

		public bool IsList
		{
			get
			{
				return m_structure is ListPacketSegmentStructure;
			}
		}

		public PacketSegmentStructure Structure
		{
			get
			{
				return m_structure;
			}
		}

		public IDictionary<string, ParsedSegment> SubSegments
		{
			get
			{
				return m_subSegments;
			}
		}

		public List<ParsedSegment> List
		{
			get
			{
				return m_list;
			}
		}

		public ParsedSegment GetByName(string name)
		{
			if (m_structure != null && name.Equals(m_structure.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				return this;
			}

			foreach (var child in m_subSegments.Values)
			{
				var segment = child.GetByName(name);
				if (segment != null)
				{
					return segment;
				}
			}
			return null;
		}

		public void AddChild(PacketParser parser, ParsedSegment segment)
		{
			var name = segment.Structure.NotEmptyName;
			parser.LastParsedSegmentsByName[name] = segment;
			if (m_subSegments.ContainsKey(name))
			{
				throw new Exception(string.Format("Segment '{0}' exists twice in PacketDefinition: " + parser.Packet.PacketId, name));
			}
			m_subSegments.Add(name, segment);
		}

		public byte ByteValue
		{
			get
			{
				return (byte)Value;
			}
		}

		public short ShortValue
		{
			get
			{
				return (short)Value;
			}
		}

		public ushort UShortValue
		{
			get
			{
				return (ushort)Value;
			}
		}

		public uint UIntValue
		{
			get
			{
				return (uint)Value;
			}
		}

		public int IntValue
		{
			get
			{
				return (int)Value;
			}
		}

		public long LongValue
		{
			get
			{
				return (long)Value;
			}
		}

		public ulong ULongValue
		{
			get
			{
				return (ulong)Value;
			}
		}

		public EntityId EntityIdValue
		{
			get
			{
				return (EntityId)Value;
			}
		}

		public string StringValue
		{
			get
			{
				return (string)Value;
			}
		}

		public object Value
		{
			get;
			set;
		}

		/// <summary>
		/// Retrieve the element at the given index within a ListPacketSegmentStructure.
		/// Does not work for non-list segments.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IParsedSegment this[int index]
		{
			get
			{
				if (m_list == null)
				{
					throw new InvalidOperationException("Can only access elements of Lists by index: " + m_structure.Name);
				}
				if (index >= m_list.Count || index < 0)
				{
					throw new ArgumentException("Index does not exist in List (length: " + m_list.Count + "): " + index);
				}
				return m_list[index];
			}
		}

		public IParsedSegment this[string name]
		{
			get
			{
				ParsedSegment segment;
				if (!m_subSegments.TryGetValue(name, out segment))
				{
					if (ReturnEmptyInsteadOfNull)
					{
						return Empty;
					}
					throw new Exception(string.Format("PacketSegmentStructure {0} does not have a direct child with the name \"{1}\" - Children are: {2}",
						m_structure.Name, name, m_subSegments.Keys.ToString(", ")));
				}
				return segment;
			}
		}

		public void RenderTo(IndentTextWriter writer)
		{
			if (IsSimple)
			{
				var str = m_structure.Name + ": " + Value;
				if (m_structure.SegmentType != null)
				{
					str += " (" + Utility.ChangeType(Value, m_structure.SegmentType, false) + ")";
				}
				writer.WriteLine(str);
			}
			else
			{
				var indented = m_structure != null && m_structure.Name != null && m_subSegments.Count > 0;
				if (indented)
				{
					writer.IndentLevel++;
					writer.WriteLine(m_structure.Name + ": ");
				}

				if (IsList)
				{
					var i = 0;
					foreach (var element in m_list)
					{
						// ReSharper disable PossibleNullReferenceException
						writer.WriteLine(m_structure.Name + " #" + i++ + ": ");
						// ReSharper restore PossibleNullReferenceException
						writer.IndentLevel++;
						foreach (var elementSegment in element.SubSegments.Values)
						{
							elementSegment.RenderTo(writer);
						}
						writer.IndentLevel--;
					}
				}
				else
				{
					foreach (var segment in m_subSegments.Values)
					{
						segment.RenderTo(writer);
					}
				}

				if (indented)
				{
					writer.IndentLevel--;
				}
			}
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}{2}", Structure.NotEmptyName, Value, IsList ? " (List " + m_list.Count + ")" : "");
		}
	}

	#region EmptyParsedSegment

	class EmptyParsedSegment : IParsedSegment
	{
		public byte ByteValue
		{
			get { return default(byte); }
		}

		public short ShortValue
		{
			get { return default(short); }
		}

		public ushort UShortValue
		{
			get { return default(ushort); }
		}

		public uint UIntValue
		{
			get { return default(uint); }
		}

		public int IntValue
		{
			get { return default(int); }
		}

		public long LongValue
		{
			get { return default(long); }
		}

		public ulong ULongValue
		{
			get { return default(ulong); }
		}

		public EntityId EntityIdValue
		{
			get { return EntityId.Zero; }
		}

		public string StringValue
		{
			get { return null; }
		}

		public object Value
		{
			get
			{
				return null;
			}
			set
			{
				throw new Exception("Cannot change value of Empty ParsedSegment");
			}
		}

		public List<ParsedSegment> List
		{
			get { return null; }
		}

		public IParsedSegment this[int index]
		{
			get
			{
				return ParsedSegment.Empty;
			}
		}

		public IParsedSegment this[string name]
		{
			get
			{
				return ParsedSegment.Empty;
			}
		}
	}
	#endregion
}