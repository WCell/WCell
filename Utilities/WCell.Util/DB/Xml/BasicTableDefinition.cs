using System.Xml.Serialization;
using WCell.Util.Variables;

namespace WCell.Util.DB.Xml
{
	public class BasicTableDefinition
	{
		[XmlAttribute]
		public string Name
		{
			get;
			set;
		}

		[XmlAttribute]
		public string MainDataHolder
		{
			get;
			set;
		}

		[XmlElement("PrimaryColumn")]
		public PrimaryColumn[] PrimaryColumns
		{
			get;
			set;
		}

		[XmlElement("Var")]
		public VariableDefinition[] Variables
		{
			get;
			set;
		}

		/// <summary>
		/// For all rows that match all these conditions, 
		/// the values from this Table will be copied to the DataHolder.
		/// </summary>
		//[XmlElement("Where")]
		//public XmlWhereClause[] WhereClauses
		//{
		//    get;
		//    set;
		//}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement("ArrayConstraint")]
		public ArrayConstraint[] ArrayConstraints
		{
			get;
			set;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}