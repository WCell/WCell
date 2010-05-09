using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	public class XmlWhereClause
	{
		[XmlAttribute]
		public string Table
		{
			get;
			set;
		}

		[XmlAttribute]
		public string Column
		{
			get;
			set;
		}

		/// <summary>
		/// The column of the table that this WhereStatement belongs to and should be compared
		/// to Column2 of Table2.
		/// </summary>
		[XmlElement("Equals")]
		public string EqualColumn
		{
			get;
			set;
		}
	}
}
