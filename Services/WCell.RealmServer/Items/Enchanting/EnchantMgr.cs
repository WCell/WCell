using System;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells;
using WCell.Util.Variables;
using WCell.RealmServer.Content;
using System.Collections.Generic;

namespace WCell.RealmServer.Items.Enchanting
{
	public static class EnchantMgr
	{
		public delegate void EnchantHandler(Item item, ItemEnchantmentEffect effect);

		[NotVariable]
		public static readonly EnchantHandler[] ApplyEnchantToItemHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		[NotVariable]
		public static readonly EnchantHandler[] RemoveEnchantFromItemHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		[NotVariable]
		public static readonly EnchantHandler[] ApplyEquippedEnchantHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		[NotVariable]
		public static readonly EnchantHandler[] RemoveEquippedEnchantHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		public static MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter> EnchantmentEntryReader;
		public static MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter> EnchantmentConditionReader;
		public static MappedDBCReader<GemProperties, GemPropertiesConverter> GemPropertiesReader;

		[NotVariable]
		public static List<ItemRandomEnchantEntry>[] RandomEnchantEntries = new List<ItemRandomEnchantEntry>[9000];

		internal static void Init()
		{
			ApplyEnchantToItemHandlers[(uint)ItemEnchantmentType.Damage] = ApplyDamageToItem;
			RemoveEnchantFromItemHandlers[(uint)ItemEnchantmentType.Damage] = RemoveDamageFromItem;


			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.Damage] = DoNothing;
			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.Resistance] = DoNothing;
			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.CombatSpell] = ApplyCombatSpell;
			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.EquipSpell] = ApplyEquipSpell;
			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.Stat] = ApplyStat;
			ApplyEquippedEnchantHandlers[(uint)ItemEnchantmentType.Totem] = ApplyTotem;

			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.Damage] = DoNothing;
			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.Resistance] = DoNothing;
			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.CombatSpell] = RemoveCombatSpell;
			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.EquipSpell] = RemoveEquipSpell;
			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.Stat] = RemoveStat;
			RemoveEquippedEnchantHandlers[(uint)ItemEnchantmentType.Totem] = RemoveTotem;

			EnchantmentConditionReader = new MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLITEMENCHANTMENTCONDITION));

			EnchantmentEntryReader = new MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLITEMENCHANTMENT));

			GemPropertiesReader = new MappedDBCReader<GemProperties, GemPropertiesConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_GEMPROPERTIES));
		}

		private static void DoNothing(Item item, ItemEnchantmentEffect effect)
		{
		}

		public static ItemEnchantmentEntry GetEnchantmentEntry(uint id)
		{
			ItemEnchantmentEntry entry;
			EnchantmentEntryReader.Entries.TryGetValue((int)id, out entry);
			return entry;
		}

		public static GemProperties GetGemproperties(uint id)
		{
			GemProperties entry;
			GemPropertiesReader.Entries.TryGetValue((int)id, out entry);
			return entry;
		}

		public static ItemEnchantmentCondition GetEnchantmentCondition(uint id)
		{
			ItemEnchantmentCondition condition;
			EnchantmentConditionReader.Entries.TryGetValue((int)id, out condition);
			return condition;
		}

		/// <summary>
		/// Applies the given EnchantEffect to the given Item and the wearer of the Item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="effect"></param>
		internal static void ApplyEquippedEffect(Item item, ItemEnchantmentEffect effect)
		{
			ApplyEquippedEnchantHandlers[(uint)effect.Type](item, effect);
		}

		internal static void ApplyEnchantToItem(Item item, ItemEnchantment enchant)
		{
			foreach (var effect in enchant.Entry.Effects)
			{
				var handler = ApplyEnchantToItemHandlers[(uint)effect.Type];
				if (handler != null)
				{
					handler(item, effect);
				}
			}
		}

		internal static void RemoveEnchantFromItem(Item item, ItemEnchantment enchant)
		{
			foreach (var effect in enchant.Entry.Effects)
			{
				var handler = RemoveEnchantFromItemHandlers[(uint)effect.Type];
				if (handler != null)
				{
					handler(item, effect);
				}
			}
		}

		/// <summary>
		/// Removes the given EnchantEffect from the given Item and the wearer of the Item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="effect"></param>
		internal static void RemoveEffect(Item item, ItemEnchantmentEffect effect)
		{
			RemoveEquippedEnchantHandlers[(uint)effect.Type](item, effect);
		}

		#region Handlers
		private static void ApplyCombatSpell(Item item, ItemEnchantmentEffect effect)
		{
			var spell = SpellHandler.Get(effect.Misc);
			if (spell == null)
			{
				ContentMgr.OnInvalidClientData("Enchantment Effect {0} had invalid SpellId: {1}", effect, (SpellId)effect.Misc);
			}
			else
			{
				item.OwningCharacter.AddProcHandler(new ItemHitProcHandler(item, spell));
			}
		}

		private static void ApplyEquipSpell(Item item, ItemEnchantmentEffect effect)
		{
			var owner = item.OwningCharacter;
			var spell = SpellHandler.Get((SpellId)effect.Misc);
			if (spell == null)
			{
				LogManager.GetCurrentClassLogger().Warn("{0} had invalid SpellId: {1}", effect, (SpellId)effect.Misc);
				return;
			}
			SpellCast.ValidateAndTriggerNew(spell, owner, owner, null, item);
		}

		private static void ApplyStat(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.ApplyStatMod((ItemModType)effect.Misc, effect.MaxAmount);
		}

		private static void ApplyTotem(Item item, ItemEnchantmentEffect effect)
		{

		}

		private static void RemoveCombatSpell(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.RemoveProcHandler(handler =>
				handler.ProcSpell != null && handler.ProcSpell.Id == effect.Misc);
		}

		private static void RemoveEquipSpell(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.Auras.Remove((SpellId)effect.Misc);
		}

		private static void RemoveStat(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.RemoveStatMod((ItemModType)effect.Misc, effect.MaxAmount);
		}

		private static void RemoveTotem(Item item, ItemEnchantmentEffect effect)
		{

		}


		// effects that apply to item

		private static void ApplyDamageToItem(Item item, ItemEnchantmentEffect effect)
		{
			item.BonusDamage += effect.MaxAmount;
			if (item.IsEquippedItem)
			{
				item.Container.Owner.UpdateDamage((InventorySlot)item.Slot);
			}
		}

		private static void RemoveDamageFromItem(Item item, ItemEnchantmentEffect effect)
		{
			item.BonusDamage -= effect.MaxAmount;
			if (item.IsEquippedItem)
			{
				item.Container.Owner.UpdateDamage((InventorySlot)item.Slot);
			}
		}
		#endregion

		public static ItemSuffixCategory GetSuffixCategory(ItemTemplate template)
		{
			if (template.IsRangedWeapon)
			{
				return ItemSuffixCategory.Ranged;
			}
			if (template.IsWeapon)
			{
				return ItemSuffixCategory.Weapon;
			}
			switch (template.InventorySlotType)
			{
				case InventorySlotType.Head:
				case InventorySlotType.Body:
				case InventorySlotType.Chest:
				case InventorySlotType.Legs:
				case InventorySlotType.Robe:
					return ItemSuffixCategory.MainArmor;
				case InventorySlotType.Shoulder:
				case InventorySlotType.Waist:
				case InventorySlotType.Feet:
				case InventorySlotType.Hand:
				case InventorySlotType.Trinket:
					return ItemSuffixCategory.SecondaryArmor;
				case InventorySlotType.Neck:
				case InventorySlotType.Wrist:
				case InventorySlotType.Finger:
				case InventorySlotType.Shield:
				case InventorySlotType.Cloak:
				case InventorySlotType.Holdable:
					return ItemSuffixCategory.Other;
			}

			return ItemSuffixCategory.None;
		}

		public static uint GetRandomSuffixFactor(ItemTemplate template)
		{
			var suffixCat = GetSuffixCategory(template);
			if (suffixCat >= ItemSuffixCategory.None)
			{
				return 0;
			}

			var levelInfo = ItemMgr.GetLevelInfo(template.Level);
			if (levelInfo != null)
			{
				switch (template.Quality)
				{
					case ItemQuality.Uncommon:
						return levelInfo.UncommonPoints[(uint)suffixCat];
					case ItemQuality.Rare:
						return levelInfo.RarePoints[(uint)suffixCat];
					case ItemQuality.Epic:
					case ItemQuality.Legendary:
					case ItemQuality.Artifact:
						return levelInfo.EpicPoints[(uint)suffixCat];
				}
			}
			return 0;
		}
	}
}