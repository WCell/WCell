using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Initialization;

namespace WCell.Addons.Default.MiscFixes
{
	/// <summary>
	/// Place to address class-specific inconsistencies
	/// </summary>
	public class ClassFixes
	{
		/// <summary>
		/// Initialize after ArchetypeMgr (Pass 7)
		/// </summary>
		[Initialization(InitializationPass.Eighth)]
		public static void FixHunters()
		{
			
		}
	}
}
