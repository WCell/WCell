using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.DB.Xml
{
	public class DefVersion
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

		[XmlAttribute]
		public float MinVersion
		{
			get;
			set;
		}

		[XmlAttribute]
		public float MaxVersion
		{
			get;
			set;
		}

		public bool IsValid
		{
			get { return !string.IsNullOrEmpty(Column) && !string.IsNullOrEmpty(Table); }
		}

        public override string ToString()
        {
            return "Table: " + Table + ", Column: " + Column;
        }
	}
}
