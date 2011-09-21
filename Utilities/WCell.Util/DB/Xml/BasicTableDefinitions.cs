using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	[XmlRoot("Tables")]
	public class BasicTableDefinitions : XmlFile<BasicTableDefinitions>
	{
		[XmlElement]
		public DefVersion DBVersion
		{
			get;
			set;
		}

		[XmlElement("Table")]
		public BasicTableDefinition[] Tables
		{
			get;
			set;
		}
	}
}