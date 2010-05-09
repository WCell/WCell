using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.DB.Xml
{
	/// <summary>
	/// Defines length and the Column of an Array
	/// </summary>
	public class ArrayConstraint
	{
		public string Column
		{
			get;
			set;
		}

		public int Length
		{
			get;
			set;
		}
	}
}
