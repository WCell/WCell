using System.Collections.Generic;
using Cell.Core;
using NLog;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// Represents a collectable Set of Items
	/// 
	/// TODO: ID - enum
	/// </summary>
	public class ItemSet
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The bag to be used when creating a new set
		/// </summary>
		const ItemId BagTemplateId = ItemId.FororsCrateOfEndlessResistGearStorage;

		/// <summary>
		/// Maximum amount of items per set
		/// </summary>
		const int MaxBonusCount = 8;

		/// <summary>
		/// offset of items in the DBC file
		/// </summary>
		const int ItemsOffset = 18;

		/// <summary>
		/// End of items in the DBC file
		/// </summary>
		const int ItemsEnd = 18;

		/// <summary>
		/// Offset of Set-Bonuses in the DBC file
		/// </summary>
		const int BonusesOffset = 19;
		/// <summary>
		/// Offset of the item-order in the DBC file
		/// </summary>
		const int ItemBonusOrderOffset = 27;

		public uint Id;

		public string Name;

		/// <summary>
		/// The templates of items that belong to this set
		/// </summary>
		public ItemTemplate[] Templates;

		/// <summary>
		/// An array of array of spells that get applied for each (amount-1) of items of this set.
		/// Eg. all spells to be applied when equipping the first item would be at Boni[0] etc
		/// </summary>
		public Spell[][] Boni;

		/// <summary>
		/// We need this skill in order to wear items of this set
		/// </summary>
		public SkillLine RequiredSkill;

		/// <summary>
		/// We need at least this much of the RequiredSkill, in order to wear items of this set
		/// </summary>
		public uint RequiredSkillValue;

		#region DBC
		public class ItemSetDBCConverter : AdvancedDBCRecordConverter<ItemSet>
		{
			public override ItemSet ConvertTo(byte[] rawData, ref int id)
			{
				ItemSet set = new ItemSet();

				id = (int)(set.Id = rawData.GetUInt32(0));
				set.Name = GetString(rawData, 1);

				set.Templates = new ItemTemplate[10];

				// 17 (or less) items
				//for (uint i = ItemsOffset; i <= ItemsEnd; i++)
				//{
				//    var itemId = rawData.GetUInt32(i);
				//    if (itemId != 0)
				//    {
				//        var templ = ItemMgr.GetTemplate(itemId);
				//        if (templ != null)
				//        {
				//            items.Add(templ);
				//        }
				//    }
				//}

				var boni = new Spell[MaxBonusCount];
				for (uint i = BonusesOffset; i < BonusesOffset + MaxBonusCount; i++)
				{
					var spellId = rawData.GetUInt32(i);
					if (spellId != 0)
					{
						Spell spell;
						if ((spell = SpellHandler.Get(spellId)) != null)
						{
							var index = i - BonusesOffset;
							boni[index] = spell;
						}
					}
				}

				var orderedBoni = new List<Spell>[MaxBonusCount];
				uint highestIndex = 0;
				for (uint i = ItemBonusOrderOffset; i < ItemBonusOrderOffset + MaxBonusCount; i++)
				{
					var amount = rawData.GetUInt32(i);
					if (amount > 0)
					{
						var orderedIndex = amount - 1;
						if (highestIndex < orderedIndex)
						{
							highestIndex = orderedIndex;
						}

						var spells = orderedBoni[orderedIndex];
						if (spells == null)
						{
							orderedBoni[orderedIndex] = spells = new List<Spell>(3);
						}

						var bonusSlot = i - ItemBonusOrderOffset;
						var spell = boni[bonusSlot];
						if (spell != null)
						{
							spells.Add(spell);
						}
					}
				}

				set.Boni = new Spell[highestIndex + 1][];
				for (int i = 0; i <= highestIndex; i++)
				{
					if (orderedBoni[i] != null)
					{
						set.Boni[i] = orderedBoni[i].ToArray();
					}
				}

				var skillId = (SkillId)rawData.GetUInt32(51);
				if (skillId > 0)
				{
					SkillLine skill = SkillHandler.Get(skillId);
					if (skill != null)
					{
						set.RequiredSkill = skill;
						set.RequiredSkillValue = rawData.GetUInt32(52);
					}
				}

				return set;
			}
		}
		#endregion

		/// <summary>
		/// If there is a free equippable bag slot: Adds all items of this set to a new bag in that slot
		/// </summary>
		/// <returns>False if their was no space left or an internal error occured and not all items could be added</returns>
		public static bool CreateSet(Character owner, ItemSetId id)
		{
			if ((uint)id >= ItemMgr.Sets.Length)
				return false;

			var set = ItemMgr.Sets[(uint)id];
			if (set == null)
				return false;

			return set.Create(owner);
		}

		/// <summary>
		/// If there is a free equippable bag slot: Adds all items of this set to a new bag in that slot
		/// </summary>
		/// <returns>False if their was no space left or an internal error occured and not all items could be added</returns>
		public bool Create(Character owner)
		{
			var inventory = owner.Inventory;
			var bags = inventory.EquippedContainers;
			var slot = bags.FindFreeSlot();
			if (slot != BaseInventory.INVALID_SLOT)
			{
				var bag = Item.CreateItem(BagTemplateId, owner, 1) as Container;
				if (bag == null)
				{
					log.Error("Invalid container template id for ItemSet: " + BagTemplateId);
				}
				else
				{
					var err = bags.TryAdd(slot, bag, true);
					if (err == InventoryError.OK)
					{
						var bagInventory = bag.BaseInventory;
						for (var i = 0; i < Templates.Length; i++)
						{
							int amount = 1;
							err = bagInventory.TryAdd(Templates[i], ref amount);
							if (err != InventoryError.OK)
							{
								ItemHandler.SendInventoryError(owner.Client, null, null, err);
								log.Error("Failed to add item (Template: {0}) to bag on {1} ({2})", Templates[i], owner, err);
								return false;
							}
						}
						return true;
					}
				    log.Error("Failed to add ItemSet-bag to owner {0} ({1})", owner, err);
				    return false;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return Name + " (Id: " + Id + ")"; //" with " + Templates.Count() + " items";
		}
	}
}
