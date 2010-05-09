using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Toolshed
{
	public class ToolAttribute : Attribute
	{
		public string Name;

		public ToolAttribute() : this(null)
		{
		}

		public ToolAttribute(string name)
		{
			Name = name;
		}
	}

	/// <summary>
	/// Marks a method to be excluded from the Tool-search.
	/// </summary>
	public class NoToolAttribute : Attribute
	{
		
	}
}
