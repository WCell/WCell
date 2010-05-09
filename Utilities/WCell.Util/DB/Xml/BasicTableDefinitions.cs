using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	[XmlRoot("Tables")]
	public class BasicTableDefinitions : XmlConfig<BasicTableDefinitions>
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
