using NLog;
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
		private static Logger log = LogManager.GetCurrentClassLogger();

		public delegate void EnchantHandler(Item item, ItemEnchantmentEffect effect);

		[NotVariable]
		public static readonly EnchantHandler[] ApplyEnchantHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		[NotVariable]
		public static readonly EnchantHandler[] RemoveEnchantHandlers = new EnchantHandler[(uint)ItemEnchantmentType.Count];

		public static MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter> EnchantmentEntryReader;
		public static MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter> EnchantmentConditionReader;
		public static MappedDBCReader<GemProperties, GemPropertiesConverter> GemPropertiesReader;

		[NotVariable]
		public static List<ItemRandomEnchantEntry>[] RandomEnchantEntries = new List<ItemRandomEnchantEntry>[9000];

		internal static void Init()
		{
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.CombatSpell] = ApplyCombatSpell;
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.Damage] = ApplyDamage;
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.EquipSpell] = ApplyEquipSpell;
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.Resistance] = ApplyResistance;
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.Stat] = ApplyStat;
			ApplyEnchantHandlers[(uint)ItemEnchantmentType.Totem] = ApplyTotem;

			RemoveEnchantHandlers[(uint)ItemEnchantmentType.CombatSpell] = RemoveCombatSpell;
			RemoveEnchantHandlers[(uint)ItemEnchantmentType.Damage] = RemoveDamage;
			RemoveEnchantHandlers[(uint)ItemEnchantmentType.EquipSpell] = RemoveEquipSpell;
			RemoveEnchantHandlers[(uint)ItemEnchantmentType.Resistance] = RemoveResistance;
			RemoveEnchantHandlers[(uint)ItemEnchantmentType.Stat] = RemoveStat;
			RemoveEnchantHandlers[(uint)ItemEnchantmentType.Totem] = RemoveTotem;

			EnchantmentConditionReader = new MappedDBCReader<ItemEnchantmentCondition, ItemEnchantmentConditionConverter>(
                RealmServerConfiguration.GetDBCFile(WCellDef.DBC_SPELLITEMENCHANTMENTCONDITION));

			EnchantmentEntryReader = new MappedDBCReader<ItemEnchantmentEntry, ItemEnchantmentConverter>(
				RealmServerConfiguration.GetDBCFile(WCellDef.DBC_SPELLITEMENCHANTMENT));

			GemPropertiesReader = new MappedDBCReader<GemProperties, GemPropertiesConverter>(
				RealmServerConfiguration.GetDBCFile(WCellDef.DBC_GEMPROPERTIES));
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
		internal static void ApplyEffect(Item item, ItemEnchantmentEffect effect)
		{
			ApplyEnchantHandlers[(uint)effect.Type](item, effect);
		}

		/// <summary>
		/// Removes the given EnchantEffect from the given Item and the wearer of the Item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="effect"></param>
		internal static void RemoveEffect(Item item, ItemEnchantmentEffect effect)
		{
			RemoveEnchantHandlers[(uint)effect.Type](item, effect);
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

		private static void ApplyDamage(Item item, ItemEnchantmentEffect effect)
		{
			var owner = item.OwningCharacter;
			owner.AddDamageDoneMod(DamageSchool.Physical, effect.MaxAmount);
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

		private static void ApplyResistance(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.AddResistanceBuff((DamageSchool)effect.Misc, effect.MaxAmount);
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

		private static void RemoveDamage(Item item, ItemEnchantmentEffect effect)
		{
			var owner = item.OwningCharacter;
			owner.RemoveDamageDoneMod(DamageSchool.Physical, effect.MaxAmount);
			owner.UpdateAllDamages();
		}

		private static void RemoveEquipSpell(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.Auras.Remove((SpellId)effect.Misc);
		}

		private static void RemoveResistance(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.RemoveResistanceBuff((DamageSchool)effect.Misc, effect.MaxAmount);
		}

		private static void RemoveStat(Item item, ItemEnchantmentEffect effect)
		{
			item.OwningCharacter.RemoveStatMod((ItemModType)effect.Misc, effect.MaxAmount);
		}

		private static void RemoveTotem(Item item, ItemEnchantmentEffect effect)
		{

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