using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	/// <summary>
	/// Single column flat field
	/// </summary>
	public class SimpleFlatFieldDefinition : DataFieldDefinition, IFlatField
	{
		private string m_Column;

		public SimpleFlatFieldDefinition()
		{

		}

		public SimpleFlatFieldDefinition(string table, string column)
		{
			Table = table;
			Column = column;
		}

		public SimpleFlatFieldDefinition(string table, string column, string defaultVal)
		{
			Table = table;
			Column = column;
			DefaultStringValue = defaultVal;
		}

		/// <summary>
		/// Optional. By default the first specified table
		/// for the containing DataHolder is used.
		/// </summary>
		[XmlAttribute]
		public string Table
		{
			get;
			set;
		}

		/// <summary>
		/// The column from which to copy the value to this Field.
		/// </summary>
		[XmlAttribute]
		public string Column
		{
			get
			{
				return m_Column;
			}
			set { m_Column = value; }
		}

		/// <summary>
		/// The column from which to copy the value to this Field.
		/// </summary>
		[XmlAttribute("DefaultValue")]
		public string DefaultStringValue
		{
			get;
			set;
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.FlatSimple; }
		}
	}
}