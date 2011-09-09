using System.Collections;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	public class NestedSimpleFieldDefinition : DataFieldDefinition, INestedFieldDefinition
	{
		/// <summary>
		/// The fields of the type the DataField has
		/// </summary>
		[XmlElement("Flat", typeof(SimpleFlatFieldDefinition))]
		[XmlElement("Nested", typeof(NestedSimpleFieldDefinition))]
		public DataFieldDefinition[] Fields
		{
			get;
			set;
		}

		public IEnumerator GetEnumerator()
		{
			return Fields.GetEnumerator();
		}

		public override DataFieldType DataFieldType
		{
			get { return DataFieldType.NestedSimple; }
		}
	}
}