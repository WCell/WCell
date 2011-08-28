using System.Collections;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	[XmlRoot("DataHolders")]
	public class LightRecordXmlConfig : XmlFile<LightRecordXmlConfig>
	{
		private static readonly XmlDataHolderDefinition[] emptyArr = new XmlDataHolderDefinition[0];

		[XmlElement("DataHolder")]
		public XmlDataHolderDefinition[] DataHolders
		{
			get;
			set;
		}

		public IEnumerator GetEnumerator()
		{
			if (DataHolders == null)
			{
				return emptyArr.GetEnumerator();
			}
			return DataHolders.GetEnumerator();
		}
	}
}