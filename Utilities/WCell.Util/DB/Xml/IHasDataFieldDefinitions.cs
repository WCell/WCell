using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
	public interface IHasDataFieldDefinitions
	{
		DataFieldDefinition[] Fields
		{
			get;
			set;
		}
	}
}