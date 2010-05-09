using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	public class Column
	{
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
		/// The table to which this Column belongs (leave blank if default table)
		/// </summary>
		[XmlAttribute]
		public string Table
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		//[XmlAttribute("DefaultValue")]
		//public string DefaultValueString
		//{
		//    get;
		//    set;
		//}
	}
}
