using System.Collections.Generic;
using System.Xml.Serialization;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
	public interface IArray
	{
		string Table
		{
			get;
			set;
		}

		//int Length
		//{
		//    get;
		//    set;
		//}
	}

	public abstract class BaseFieldArrayDefinition : DataFieldDefinition
	{
		/// <summary>
		/// Specify an array through a pattern where the <see cref="Patterns"/> 
		/// class defines possible constants.
		/// </summary>
		[XmlAttribute]
		public string Pattern
		{
			get;
			set;
		}

		/// <summary>
		/// Offset for pattern
		/// </summary>
		[XmlAttribute]
		public int Offset
		{
			get;
			set;
		}

		[XmlAttribute]
		public string Table
		{
			get;
			set;
		}

		/// <summary>
		/// An alternative way:
		/// Specify all columns of the Array explicitely
		/// </summary>
		[XmlElement("Column")]
		public Column[] ExpliciteColumns
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Flat array
	/// </summary>
	public class FlatArrayFieldDefinition : BaseFieldArrayDefinition, IFlatField, IArray
	{
		public SimpleFlatFieldDefinition[] GetColumns(int length)
		{
			var cols = ExpliciteColumns;
			if (Pattern == null && cols == null)
			{
				throw new DataHolderException("Array-field \"{0}\" had no Pattern NOR an explicit set of Columns - Make sure either of them are set and valid.", this);
			}

			if (Pattern != null && cols != null)
			{
				throw new DataHolderException("Array-field \"{0}\" defined Pattern AND an explicit set of Columns - Make sure to only specify one.", this);
			}

			if (Pattern != null)
			{
				var list = new List<SimpleFlatFieldDefinition>();
				for (var i = Offset; i < Offset + length; i++)
				{
					list.Add(new SimpleFlatFieldDefinition(Table, Patterns.Compile(Pattern, i)));
				}
				return list.ToArray();
			}

			//if (cols.Length > length)
			//{
			//    throw new DataHolderException("Array-field \"{0}\" had an invalid amount of Columns ({1}) - Required: {2}", this, ExpliciteColumns.Length, length);
			//}


			length = cols.Length;
			var defs = new SimpleFlatFieldDefinition[length];
			for (var i = 0; i < length; i++)
			{
				defs[i] = new SimpleFlatFieldDefinition(cols[i].Table, cols[i].Name);
			}
			return defs;
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.FlatArray; }
		}
	}

	public class NestedArrayFieldDefinition : DataFieldDefinition, IArray
	{
		//[XmlAttribute]
		//public int Length
		//{
		//    get;
		//    set;
		//}

		[XmlAttribute]
		public string Table
		{
			get;
			set;
		}

		[XmlElement("Flat", typeof(FlatArrayFieldDefinition))]
		//[XmlElement("Nested", typeof(NestedArrayFieldDefinition))]
		public FlatArrayFieldDefinition[] Segments
		{
			get;
			set;
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.NestedArray; }
		}
	}
}