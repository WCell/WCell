using System;

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