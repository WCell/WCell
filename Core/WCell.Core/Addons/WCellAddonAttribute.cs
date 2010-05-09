using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Addons
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WCellAddonAttribute : Attribute
	{
		/// <summary>
		/// NYI
		/// </summary>
		public bool Reloadable = false;

		public bool LoadOnStartup = true;
	}
}
