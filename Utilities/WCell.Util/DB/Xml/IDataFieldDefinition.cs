using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DB.Xml
{
	public interface IDataFieldDefinition
	{
		string Name
		{
			get;
			set;
		}

		DataFieldType DataFieldType
		{
			get;
		}
	}
}