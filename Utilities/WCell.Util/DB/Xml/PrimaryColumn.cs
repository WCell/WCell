using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	public class PrimaryColumn
	{
		[XmlIgnore]
		public object DefaultValue;

		[XmlIgnore]
		public SimpleDataColumn DataColumn;

		/// <summary>
		/// The name of the Column
		/// </summary>
		[XmlAttribute]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the type of the PrimaryColumn
		/// </summary>
		[XmlAttribute]
		public string TypeName
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the type of the PrimaryColumn
		/// </summary>
		[XmlAttribute("DefaultValue")]
		public string DefaultValueString
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