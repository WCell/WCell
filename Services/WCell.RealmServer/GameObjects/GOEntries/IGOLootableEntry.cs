using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public interface IGOLootableEntry
	{	
		/// <summary>
		/// The Id of the Loot that can be looted from this Chest
		/// </summary>
		uint LootId { get; set; }

		/// <summary>
		/// Minimum number of consecutive times this object can be opened.
		/// </summary>
		uint MinRestock { get; set; }

		/// <summary>
		/// Maximum number of consecutive times this object can be opened.
		/// </summary>
		uint MaxRestock { get; set; }
	}
}