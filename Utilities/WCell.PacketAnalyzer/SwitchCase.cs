using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.Util;

namespace WCell.PacketAnalysis
{
	public enum ComparisonType
	{
		Equal = 0,
		NotEqual,
		GreaterThan,
		LessThan,
		GreaterOrEqual,
		LessOrEqual,
		And,
		AndExclusive,
		AndNot,
		Either,
		Neither,
		Count
	}

	/// <summary>
	/// Represents a condition to be matched for determinig a <c>SwitchPacketSegmentStructure</c>
	/// </summary>
	public class SwitchCase
	{
		public delegate bool SwitchComparer(SwitchCase switchCase, object val);

		protected static SwitchComparer[] Matchers = new SwitchComparer[(int)ComparisonType.Count];

		static SwitchCase()
		{
			Matchers[(int)ComparisonType.Equal] = (condition, value) =>
			{
				return condition.m_value.Equals(value);
			};
			Matchers[(int)ComparisonType.NotEqual] = (condition, value) =>
			{
				return !condition.m_value.Equals(value);
			};
			Matchers[(int)ComparisonType.GreaterThan] = (condition, value) =>
			{
				return condition.ToComparable(condition.m_value).CompareTo(condition.ToComparable(value)) < 0;
			};
			Matchers[(int)ComparisonType.LessThan] = (condition, value) =>
			{
				return condition.ToComparable(condition.m_value).CompareTo(condition.ToComparable(value)) > 0;
			};
			Matchers[(int)ComparisonType.GreaterOrEqual] = (condition, value) =>
			{
				return condition.ToComparable(condition.m_value).CompareTo(condition.ToComparable(value)) <= 0;
			};
			Matchers[(int)ComparisonType.LessOrEqual] = (condition, value) =>
			{
				return condition.ToComparable(condition.m_value).CompareTo(condition.ToComparable(value)) >= 0;
			};
			Matchers[(int)ComparisonType.And] = (condition, value) =>
			{
				var cmpFlags = (long)Convert.ChangeType(condition.m_value, typeof(long));
				var flags = (long)Convert.ChangeType(value, typeof(long));
				return (cmpFlags & flags) != 0;
			};
			Matchers[(int)ComparisonType.AndExclusive] = (condition, value) =>
			{
				var cmpFlags = (long)Convert.ChangeType(condition.m_value, typeof(long));
				var flags = (long)Convert.ChangeType(value, typeof(long));
				return (cmpFlags & flags) == cmpFlags;
			};
			Matchers[(int)ComparisonType.AndNot] = (condition, value) =>
			{
				var originalFlags = (long)Convert.ChangeType(condition.m_value, typeof(long));
				var flags = (long)Convert.ChangeType(value, typeof(long));
				return (originalFlags & flags) == 0;
			};
			Matchers[(int)ComparisonType.Either] = (condition, value) =>
			{
				return condition.m_valueList.Contains(value);
			};
			Matchers[(int)ComparisonType.Neither] = (condition, value) =>
			{
				return !condition.m_valueList.Contains(value);
			};
		}

		protected IComparable ToComparable(object value)
		{
			value = m_switch.ConvertToUnderlyingType(value);
			if (!(value is IComparable))
			{
				throw new Exception(
					string.Format("Could not parse given value '{0}' in Switch '{1}' since its Type '{2}' is not implementing IComparable",
					value, m_switch, value.GetType()));
			}
			return (IComparable)value;
		}

		protected object m_value;

		/// <summary>
		/// Might be relevant for some switch-cases
		/// </summary>
		protected List<object> m_valueList;

		/// <summary>
		/// This is mutual referencing (since Switches also hold a reference to Condition objects).
		/// If you often create/destroy Conditions on a running system, add a cleanup method to unset this variable.
		/// </summary>
		protected SwitchPacketSegmentStructure m_switch;

		public SwitchCase() { }

		public SwitchCase(ComparisonType type, object value, params PacketSegmentStructure[] segments) :
			this(value, segments)
		{
			m_comparer = Matchers[(int)type];
		}

		public SwitchCase(ComparisonType type, object value, List<PacketSegmentStructure> segments) :
			this(value, segments)
		{
			m_comparer = Matchers[(int)type];
		}

		public SwitchCase(ComparisonType type, string strValue, params PacketSegmentStructure[] segments) :
			this(strValue, segments)
		{
			m_comparer = Matchers[(int)type];
		}


		public SwitchCase(SwitchComparer comparer, object value, params PacketSegmentStructure[] segments) :
			this(value, segments)
		{
			m_comparer = comparer;
		}

		public SwitchCase(SwitchComparer comparer, object value, List<PacketSegmentStructure> segments) :
			this(value, segments)
		{
			m_comparer = comparer;
		}

		public SwitchCase(SwitchComparer comparer, string strValue, params PacketSegmentStructure[] segments) :
			this(strValue, segments)
		{
			m_comparer = comparer;
		}


		public SwitchCase(object value, params PacketSegmentStructure[] segments)
		{
			m_value = value;
			SegmentList = segments.ToList();
		}

		public SwitchCase(object value, List<PacketSegmentStructure> segments)
		{
			m_value = value;
			SegmentList = segments;
		}

		public SwitchCase(string strValue, params PacketSegmentStructure[] segments)
		{
			StringValue = strValue;
			SegmentList = segments.ToList();
		}

		SwitchComparer m_comparer;

		[XmlAttribute("Equals")]
		public string EqualValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.Equal];
				StringValue = value;
			}
		}

		[XmlAttribute("NotEqual")]
		public string NotEqualValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.NotEqual];
				StringValue = value;
			}
		}

		[XmlAttribute("GreaterThan")]
		public string GreaterThanValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.GreaterThan];
				StringValue = value;
			}
		}

		[XmlAttribute("GreaterOrEqual")]
		public string GreaterOrEqualValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.GreaterOrEqual];
				StringValue = value;
			}
		}

		[XmlAttribute("LessThan")]
		public string LessThanValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.LessThan];
				StringValue = value;
			}
		}

		[XmlAttribute("LessOrEqual")]
		public string LessOrEqualValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.LessOrEqual];
				StringValue = value;
			}
		}

		[XmlAttribute("And")]
		public string AndValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.And];
				StringValue = value;
			}
		}

		[XmlAttribute("AndExclusive")]
		public string AndExclusiveValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.AndExclusive];
				StringValue = value;
			}
		}

		[XmlAttribute("AndNot")]
		public string AndNotValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.AndNot];
				StringValue = value;
			}
		}

		[XmlAttribute("Either")]
		public string EitherValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.Either];
				StringValue = value;
			}
		}

		[XmlAttribute("Neither")]
		public string NeitherValue
		{
			get { return StringValue; }
			set
			{
				m_comparer = Matchers[(int)ComparisonType.Neither];
				StringValue = value;
			}
		}

		[XmlIgnore]
		public virtual string StringValue
		{
			get;
			set;
		}

		/// <summary>
		/// The first (and maybe only) supplied value
		/// </summary>
		[XmlIgnore]
		public object Value
		{
			get
			{
				return m_value;
			}
		}

		/// <summary>
		/// All values as a list
		/// </summary>
		[XmlIgnore]
		public List<object> ValueList
		{
			get { return m_valueList; }
		}

		[XmlIgnore]
		public PacketSegmentStructure Structure
		{
			get;
			set;
		}

		[XmlElement("StaticList", typeof(StaticListPacketSegmentStructure))]
		[XmlElement("List", typeof(ListPacketSegmentStructure))]
		[XmlElement("FinalList", typeof(FinalListPacketSegmentStructure))]
		[XmlElement("Complex", typeof(ComplexPacketSegmentStructure))]
		[XmlElement("Switch", typeof(SwitchPacketSegmentStructure))]
		[XmlElement("Simple", typeof(PacketSegmentStructure))]
		public List<PacketSegmentStructure> SegmentList
		{
			get;
			set;
		}

		public virtual void Init(SwitchPacketSegmentStructure swtch, PacketDefinition def)
		{
			m_switch = swtch;

			if (StringValue != null)
			{
				var values = StringValue.Split(',');
				if (values.Length > 0)
				{
					m_value = ParseValue(values[0]);
					m_valueList = new List<object>();
					foreach (var val in values)
					{
						m_valueList.Add(ParseValue(val));
					}
				}
			}

			if (m_value == null)
			{
				throw new Exception("No value given in Switch " + m_switch + " Case for " + def + "");
			}

			if (SegmentList.Count > 1 || SegmentList.Count == 0)
			{
				Structure = new ComplexPacketSegmentStructure(SegmentList);
			}
			else
			{
				Structure = SegmentList[0];
			}

			foreach (var segment in SegmentList)
			{
				segment.Init(def);
			}
		}

		object ParseValue(string input)
		{
			input = input.Trim();
			var refSegment = m_switch.ReferenceSegment;
			object value;
			if (refSegment.SegmentType != null)
			{
				value = ConvertType(refSegment.Type.GetActualType(), refSegment.SegmentType, input);
			}
			else
			{
				value = SimpleTypes.ReadString(refSegment.Type, input);
			}
			return value;
		}

		private object ConvertType(Type origType, Type segmentType, string input)
		{
			long val = 0;
			object error = null;
			if (!StringParser.Eval(segmentType, ref val, input, ref error, false))
			{
				throw new Exception(string.Format("Could not parse conditional Value {0}: {1}",
					input, error));
			}

			// now this looks like ridiculous type conversion, but i couldn't seem to find any other way:
			try
			{
				var value = Convert.ChangeType(val, origType);
				Type type;
				if (segmentType.IsEnum)
				{
					type = Enum.GetUnderlyingType(segmentType);
				}
				else
				{
					type = segmentType;
				}
				value = Utility.ChangeType(value, type, true);
				return value;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Could not parse conditional Value {0}: {1}",
					input, e.Message), e);
			}
		}

		public bool Matches(object value)
		{
			if (value == null)
			{
				throw new Exception("Unexpected value: null");
			}

			//var matcher = Matchers[(int)Comparison];
			//if (matcher == null)
			//{
			//    throw new Exception("Invalid Comparison Type \"" + Comparison + "\" in Switch: " + m_switch.Name);
			//}

			try
			{
				if (m_value.GetType() != value.GetType())
				{
					var type = m_value.GetType();
					if (type.IsEnum)
					{
						type = Enum.GetUnderlyingType(type);
					}
					value = Convert.ChangeType(value, type);
				}
				return DoMatch(value);
				//return matcher(this, value);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Switch {0} could not match its value '{1}' against parsed value: {2}", m_switch, m_value, value), e);
			}
		}

		protected bool DoMatch(object value)
		{
			return m_comparer(this, value);
		}

		public override string ToString()
		{
			return string.Format("{0}", StringValue);
		}
	}

	#region Deprecated
	/*
	 * 

	public class EqualCase : SwitchCase
	{
		public EqualCase()
		{
		}

		public EqualCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public EqualCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public EqualCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("Equal")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return value.Equals(m_value);
		}
	}

	public class NotEqualCase : SwitchCase
	{
		public NotEqualCase()
		{
		}

		public NotEqualCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public NotEqualCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public NotEqualCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("NotEqual")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return !value.Equals(m_value);
		}
	}

	public class GreaterThanCase : SwitchCase
	{
		public GreaterThanCase()
		{
		}

		public GreaterThanCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public GreaterThanCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public GreaterThanCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("GreatherThan")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return ToComparable(m_value).CompareTo(ToComparable(value)) < 0;
		}
	}

	public class LessThanCase : SwitchCase
	{
		public LessThanCase()
		{
		}

		public LessThanCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public LessThanCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public LessThanCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("LessThan")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return ToComparable(m_value).CompareTo(ToComparable(value)) > 0;
		}
	}

	public class GreaterOrEqualCase : SwitchCase
	{
		public GreaterOrEqualCase()
		{
		}

		public GreaterOrEqualCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public GreaterOrEqualCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public GreaterOrEqualCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("GreaterOrEqual")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return ToComparable(m_value).CompareTo(ToComparable(value)) <= 0;
		}
	}

	public class LessOrEqualCase : SwitchCase
	{
		public LessOrEqualCase()
		{
		}

		public LessOrEqualCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public LessOrEqualCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public LessOrEqualCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("LessOrEqual")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			return ToComparable(m_value).CompareTo(ToComparable(value)) >= 0;
		}
	}

	public class AndCase : SwitchCase
	{
		public AndCase()
		{
		}

		public AndCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public AndCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public AndCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("And")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			var originalFlags = (long)Convert.ChangeType(m_value, typeof(long));
			var flags = (long)Convert.ChangeType(value, typeof(long));
			return (originalFlags & flags) != 0;
		}
	}

	public class AndNotCase : SwitchCase
	{
		public AndNotCase()
		{
		}

		public AndNotCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public AndNotCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public AndNotCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("AndNot")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			var originalFlags = (long)Convert.ChangeType(m_value, typeof(long));
			var flags = (long)Convert.ChangeType(value, typeof(long));
			return (originalFlags & flags) == 0;
		}
	}

	public class EitherCase : SwitchCase
	{
		public EitherCase()
		{
		}

		public EitherCase(string strValue, params PacketSegmentStructure[] structure)
			: base(strValue, structure)
		{
		}

		public EitherCase(object value, params PacketSegmentStructure[] structure)
			: base(value, structure)
		{
		}

		public EitherCase(object value, List<PacketSegmentStructure> structure)
			: base(value, structure)
		{
		}

		[XmlElement("Either")]
		public override string StringValue
		{
			get;
			set;
		}

		protected override bool DoMatch(object value)
		{
			var originalFlags = (long)Convert.ChangeType(m_value, typeof(long));
			var flags = (long)Convert.ChangeType(value, typeof(long));
			return (originalFlags & flags) == 0;
		}
	}
	 * */
	#endregion
}