using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Items;
using WCell.Core.Initialization;
using WCell.RealmServer.Items;

namespace WCell.Addons.Default.Items
{
	/// <summary>
	/// Fixes items that are used with player abilities. Mana gems etc
	/// </summary>
	public static class PlayerAbilityItemFixes
	{
		/// <summary>
		/// This must not be called *after* items have been loaded!
		/// </summary>
		[Initialization(InitializationPass.First)]
		public static void FixMe()
		{
			// some mana gems are not consumables, like they should be:
			ItemMgr.Apply(item =>
			{
				item.Class = ItemClass.Consumable;
			}, ItemId.ManaAgate);
		}
	}
}
