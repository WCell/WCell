/*************************************************************************
 *
 *   file		: ModIncreaseEnergyPercent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 14:58:12 +0800 (Sat, 07 Mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;
using WCell.Util.Variables;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Same as ModStatPercent, but including item bonuses
	/// TODO: Include item bonuses
	/// </summary>
	public class ModTotalStatPercentHandler : ModStatPercentHandler
	{
		[NotVariable]
		public static readonly ItemModType[] StatTypeToItemModType = new[]
		                                                    	{
		                                                    		ItemModType.Strength,
		                                                    		ItemModType.Agility,
		                                                    		ItemModType.Stamina,
		                                                    		ItemModType.Intellect,
		                                                    		ItemModType.Spirit
		                                                    	};
 		protected override int GetStatValue(StatType stat)
		{
			var val = Owner.GetBaseStatValue(stat);

			var chr = Owner as Character;
			if(chr != null)
			{
				var items = chr.Inventory.Equipment.Items; //All equipped items
				for(int i = 0; i < items.Length; i++)
				{
					if(items[i] != null)
					{
						var itemMods = items[i].Template.Mods; //All mods of the item
						for(int j = 0; j < itemMods.Length; j++)
						{
							var mod = itemMods[j];
							if (mod.Type == StatTypeToItemModType[(int)stat]) //Mod with given stat
								val += mod.Value;
						}
					}
				}
			}
			return val;
		}
	}
};