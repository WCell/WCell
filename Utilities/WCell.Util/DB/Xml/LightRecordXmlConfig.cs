using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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