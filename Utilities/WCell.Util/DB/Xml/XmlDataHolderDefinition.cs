using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using IEnumerator=System.Collections.IEnumerator;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
	public class XmlDataHolderDefinition : IHasDataFieldDefinitions
	{
		[XmlAttribute]
		public string Name
		{
			get;
			set;
		}

		[XmlElement("DefaultTable")]
		public string[] DefaultTables
		{
			get;
			set;
		}

		[XmlElement("Flat", typeof(SimpleFlatFieldDefinition))]
		[XmlElement("Nested", typeof(NestedSimpleFieldDefinition))]
		[XmlElement("FlatArray", typeof(FlatArrayFieldDefinition))]
		[XmlElement("NestedArray", typeof(NestedArrayFieldDefinition))]
		public DataFieldDefinition[] Fields
		{
			get;
			set;
		}

		public string DataHolderName
		{
			get { return Name; }
		}

		public IEnumerator GetEnumerator()
		{
			return Fields.GetEnumerator();
		}

		public override string ToString()
		{
			return Name + " (" + DefaultTables.ToString(", ") + ")";
		}
	}
}