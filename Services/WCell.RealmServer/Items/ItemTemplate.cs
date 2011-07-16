using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;

namespace WCell.RealmServer.Items
{
	[DataHolder]
	public partial class ItemTemplate : IDataHolder, IMountableItem, IQuestHolderEntry
	{
		#region Standard Fields
		[Persistent((int)ClientLocale.End)]
		public string[] Names;

		[NotPersistent]
		public string DefaultName
		{
			get { return Names.LocalizeWithDefaultLocale(); }
			set
			{
				if (Names == null)
				{
					Names = new string[(int)ClientLocale.End];
				}
				Names[(int)RealmServerConfiguration.DefaultLocale] = value;
			}
		}

		public uint Id { get; set; }

		[NotPersistent]
		public ItemId ItemId;

		public ItemClass Class;

		public ItemSubClass SubClass;

		public int Unk0;

		public uint DisplayId;

		public ItemQuality Quality;

		public ItemFlags Flags;

		public ItemFlags2 Flags2;

		public uint BuyPrice;

		public uint SellPrice;

		public InventorySlotType InventorySlotType;

		public ClassMask RequiredClassMask;

		public RaceMask RequiredRaceMask;

		public uint Level;

		public uint RequiredLevel;

		public SkillId RequiredSkillId;

		public uint RequiredSkillValue;

		public SpellId RequiredProfessionId;

		public uint RequiredPvPRank;

		public uint UnknownRank;

		public FactionId RequiredFactionId;

		public StandingLevel RequiredFactionStanding;

		public int UniqueCount;

		public uint ScalingStatDistributionId;

		public uint ScalingStatValueFlags;

		public uint ItemLimitCategoryId;

		public uint HolidayId;

		/// <summary>
		/// The size of a stack of this item.
		/// </summary>
		public int MaxAmount;

		public int ContainerSlots;

		public RequiredSpellTargetType RequiredTargetType;

		public uint RequiredTargetId;

		[Persistent(ItemConstants.MaxStatCount)]
		public StatModifier[] Mods;

		[Persistent(ItemConstants.MaxDmgCount)]
		public DamageInfo[] Damages;

		[Persistent(ItemConstants.MaxResCount)]
		public int[] Resistances;

		public int AttackTime;

		public ItemProjectileType ProjectileType;

		public float RangeModifier;

		public ItemBondType BondType;

		[Persistent((int)ClientLocale.End)]
		public string[] Descriptions;

		[NotPersistent]
		public string DefaultDescription
		{
			get { return Descriptions.LocalizeWithDefaultLocale(); }
			set
			{
				if (Names == null)
				{
					Names = new string[(int)ClientLocale.End];
				}
				Descriptions[(int)RealmServerConfiguration.DefaultLocale] = value;
			}
		}

		public uint PageTextId;

		public ChatLanguage LanguageId;

		public PageMaterial PageMaterial;

		/// <summary>
		/// The Id of the Quest that will be started when this Item is used
		/// </summary>
		public uint QuestId;

		public uint LockId;

		public Material Material;

		public SheathType SheathType;

		public uint RandomPropertiesId;

		public uint RandomSuffixId;

		public uint BlockValue;

		public ItemSetId SetId;

		public int MaxDurability;

		public ZoneId ZoneId;

		public MapId MapId;

		public ItemBagFamilyMask BagFamily;

		public ToolCategory ToolCategory;

		[Persistent(ItemConstants.MaxSocketCount)]
		public SocketInfo[] Sockets;

		/// <summary>
		/// 
		/// </summary>
		public uint SocketBonusEnchantId;

		[NotPersistent]
		public ItemEnchantmentEntry SocketBonusEnchant;

		public uint GemPropertiesId;

		[NotPersistent]
		public GemProperties GemProperties;

		public int RequiredDisenchantingLevel;

		public float ArmorModifier;

		public int Duration;

		public PetFoodType m_PetFood;

		public List<ItemRandomEnchantEntry> RandomPrefixes
		{
			get
			{
				if (RandomPropertiesId != 0)
				{
					return EnchantMgr.RandomEnchantEntries.Get(RandomPropertiesId);
				}
				return null;
			}
		}

		public List<ItemRandomEnchantEntry> RandomSuffixes
		{
			get
			{
				if (RandomPropertiesId != 0)
				{
					return EnchantMgr.RandomEnchantEntries.Get(RandomSuffixId);
				}
				return null;
			}
		}

		[Persistent(ItemConstants.MaxSpellCount)]
		public ItemSpell[] Spells;

		public ItemSpell GetSpell(ItemSpellTrigger trigger)
		{
			return Spells.Where(itemSpell => itemSpell != null && itemSpell.Trigger == trigger && itemSpell.Id != 0).FirstOrDefault();
		}

		public int GetResistance(DamageSchool school)
		{
			return Resistances[(int)school];
		}
		#endregion

		#region Vendor-Info
		public uint StockRefillDelay;

		public int StockAmount;

		/// <summary>
		/// Amount of Items to be sold in one stack
		/// </summary>
		public int BuyStackSize;
		#endregion

		#region Auto-generated fields
		[NotPersistent]
		public InventorySlotTypeMask InventorySlotMask
		{
			get;
			set;
		}

		[NotPersistent]
		public uint RandomSuffixFactor;

		[NotPersistent]
		public SkillLine RequiredSkill;

		[NotPersistent]
		public ItemSubClassMask SubClassMask;

		[NotPersistent]
		/// <summary>
		/// Spell to be casted when using this item
		/// </summary>
		public ItemSpell UseSpell;

		[NotPersistent]
		/// <summary>
		/// Spell that is casted once and then consumes this Item (usually teaching formulars, patterns, designs etc)
		/// </summary>
		public ItemSpell TeachSpell;

		[NotPersistent]
		/// <summary>
		/// Spell to be casted when equipping
		/// </summary>
		public Spell[] EquipSpells;

		[NotPersistent]
		/// <summary>
		/// Spell to be casted when being hit
		/// </summary>
		public Spell[] HitSpells;

		[NotPersistent]
		/// <summary>
		/// Spell to be casted when using Soulstone
		/// </summary>
		public Spell SoulstoneSpell;

		[NotPersistent]
		/// <summary>
		/// The ItemSet to which this Item belongs (if any)
		/// </summary>
		public ItemSet Set;

		[NotPersistent]
		public LockEntry Lock;

		[NotPersistent]
		public Faction RequiredFaction;

		[NotPersistent]
		public Spell RequiredProfession;

		[NotPersistent]
		/// <summary>
		/// EquipmentSlots to which this item can be equipped
		/// </summary>
		public EquipmentSlot[] EquipmentSlots;

		[NotPersistent]
		/// <summary>
		/// whether this is ammo
		/// </summary>
		public bool IsAmmo;

		[NotPersistent]
		/// <summary>
		/// whether this is a bag (includes quivers)
		/// </summary>
		public bool IsBag;

		[NotPersistent]
		/// <summary>
		/// whether this is a container (includes chests, clams, bags, quivers, etc.)
		/// </summary>
		public bool IsContainer;

		[NotPersistent]
		/// <summary>
		/// whether this is a key
		/// </summary>
		public bool IsKey;

		[NotPersistent]
		/// <summary>
		/// whether this can be stacked
		/// </summary>
		public bool IsStackable;

		[NotPersistent]
		/// <summary>
		/// whether this is a weapon
		/// </summary>
		public bool IsWeapon;

		[NotPersistent]
		/// <summary>
		/// whether this is a ranged weapon
		/// </summary>
		public bool IsRangedWeapon;

		[NotPersistent]
		public bool IsMeleeWeapon;

		[NotPersistent]
		public bool IsThrowable;

		[NotPersistent]
		/// <summary>
		/// whether this is a 2h weapon
		/// </summary>
		public bool IsTwoHandWeapon;

		[NotPersistent]
		/// <summary>
		/// whether this teleports one home when using it
		/// </summary>
		public bool IsHearthStone;

		[NotPersistent]
		/// <summary>
		/// The skill needed for this item, for armors, weapons, shields etc
		/// </summary>
		public SkillId ItemProfession;

		[NotPersistent]
		/// <summary>
		/// whether this Item is an equippable Item and not a bag
		/// </summary>
		public bool IsInventory;

		[NotPersistent]
		public bool IsCharter;

		[NotPersistent]
		/// <summary>
		/// The Quests for which this Item needs to be collected
		/// </summary>
		public QuestTemplate[] CollectQuests;

		public bool HasQuestRequirements
		{
			get { return QuestHolderInfo != null || CollectQuests != null; }
		}

		[NotPersistent]
		/// <summary>
		/// Whether this ItemTemplate has any sockets
		/// </summary>
		public bool HasSockets;

		[NotPersistent]
		public bool ConsumesAmount;

		[NotPersistent]
		public Func<Item> Creator;

		/// <summary>
		/// For templates of Containers only, checks whether the given
		/// Template may be added
		/// </summary>
		/// <param name="templ"></param>
		/// <returns></returns>
		public bool MayAddToContainer(ItemTemplate templ)
		{
			return BagFamily == 0 || templ.BagFamily.HasAnyFlag(BagFamily);
		}

		public object GetId()
		{
			return Id;
		}
		#endregion

		#region Init
		/// <summary>
		/// Set custom fields etc
		/// </summary>
		public void FinalizeDataHolder()
		{
			CheckId();
			ArrayUtil.Set(ref ItemMgr.Templates, Id, this);
		}

		internal void InitializeTemplate()
		{
			if (Names == null)
			{
				Names = new string[(int)ClientLocale.End];
			}

			if (Descriptions == null)
			{
				Descriptions = new string[(int)ClientLocale.End];
			}

			if (DefaultDescription == null)
			{
				DefaultDescription = "";
			}

			if (string.IsNullOrEmpty(DefaultName) || Id == 0)
			{
				// something's off with these entries
				return;
			}

			ItemId = (ItemId)Id;
			//Faction = (FactionId)Faction; // faction, 3.2.2
			RequiredSkill = SkillHandler.Get(RequiredSkillId);
			Set = ItemMgr.GetSet(SetId);
			Lock = LockEntry.Entries.Get(LockId);
			RequiredFaction = FactionMgr.Get(RequiredFactionId);
			RequiredProfession = SpellHandler.Get(RequiredProfessionId);
			SubClassMask = (ItemSubClassMask)(1 << (int)SubClass);
			EquipmentSlots = ItemMgr.EquipmentSlotsByInvSlot.Get((uint)InventorySlotType);
			InventorySlotMask = (InventorySlotTypeMask)(1 << (int)InventorySlotType);
			IsAmmo = InventorySlotType == InventorySlotType.Ammo;
			IsKey = Class == ItemClass.Key;
			IsBag = InventorySlotType == InventorySlotType.Bag;
			IsContainer = Class == ItemClass.Container || Class == ItemClass.Quiver;

			// enchantables can't be stacked
			IsStackable = MaxAmount > 1 && RandomSuffixId == 0 && RandomPropertiesId == 0;
			IsTwoHandWeapon = InventorySlotType == InventorySlotType.TwoHandWeapon;
			SetIsWeapon();

			if (ToolCategory != 0)// && TotemCategory != TotemCategory.SkinningKnife)
			{
				ItemMgr.FirstTotemsPerCat[(uint)ToolCategory] = this;
			}

			if (GemPropertiesId != 0)
			{
				GemProperties = EnchantMgr.GetGemproperties(GemPropertiesId);
				if (GemProperties != null)
				{
					GemProperties.Enchantment.GemTemplate = this;
				}
			}

			if (Sockets == null)
			{
				Sockets = new SocketInfo[ItemConstants.MaxSocketCount];
			}
			else if (Sockets.Contains(sock => sock.Color != 0))
			{
				HasSockets = true;
			}

			if (Damages == null)
			{
				Damages = DamageInfo.EmptyArray;
			}

			if (Resistances == null)
			{
				Resistances = new int[(int)DamageSchool.Count];
			}

			if (SocketBonusEnchantId != 0)
			{
				SocketBonusEnchant = EnchantMgr.GetEnchantmentEntry(SocketBonusEnchantId);
			}

			switch (Class)
			{
				case ItemClass.Weapon:
					ItemProfession = ItemProfessions.WeaponSubClassProfessions.Get((uint)SubClass);
					break;
				case ItemClass.Armor:
					ItemProfession = ItemProfessions.ArmorSubClassProfessions.Get((uint)SubClass);
					break;
			}

			if (SheathType == SheathType.Undetermined)
			{
				// TODO: Read sheath-id from Item.dbc
			}

			// spells
			if (Spells != null)
			{
				ArrayUtil.Prune(ref Spells);
				for (int i = 0; i < 5; i++)
				{
					Spells[i].Index = (uint)i;
					Spells[i].FinalizeAfterLoad();
				}
			}
			else
			{
				Spells = ItemSpell.EmptyArray;
			}

			UseSpell = Spells.Where(itemSpell => itemSpell.Trigger == ItemSpellTrigger.Use && itemSpell.Spell != null).FirstOrDefault();
			if (UseSpell != null)
			{
				UseSpell.Spell.RequiredTargetType = RequiredTargetType;
				UseSpell.Spell.RequiredTargetId = RequiredTargetId;
			}

			EquipSpells = Spells.Where(spell => spell.Trigger == ItemSpellTrigger.Equip && spell.Spell != null).Select(itemSpell =>
					itemSpell.Spell).ToArray();

			SoulstoneSpell = Spells.Where(spell => spell.Trigger == ItemSpellTrigger.Soulstone && spell.Spell != null).Select(itemSpell =>
					itemSpell.Spell).FirstOrDefault();

			HitSpells = Spells.Where(spell => spell.Trigger == ItemSpellTrigger.ChanceOnHit && spell.Spell != null).Select(itemSpell =>
					itemSpell.Spell).ToArray();

			if (UseSpell != null && (UseSpell.Id == SpellId.Learning || UseSpell.Id == SpellId.Learning_2))
			{
				// Teaching
				TeachSpell = Spells.Where(spell => spell.Trigger == ItemSpellTrigger.Consume).FirstOrDefault();
			}

			ConsumesAmount =
				(Class == ItemClass.Consumable ||
				Spells.Contains(spell => spell.Trigger == ItemSpellTrigger.Consume)) &&
				(UseSpell == null || !UseSpell.HasCharges);

			IsHearthStone = UseSpell != null && UseSpell.Spell.IsHearthStoneSpell;

			IsInventory = InventorySlotType != InventorySlotType.None &&
				InventorySlotType != InventorySlotType.Bag &&
				InventorySlotType != InventorySlotType.Quiver &&
				InventorySlotType != InventorySlotType.Relic;

			// find set
			if (SetId != 0)
			{
				var set = ItemMgr.Sets.Get((uint)SetId);
				if (set != null)
				{
					ArrayUtil.Add(ref set.Templates, this);
				}
			}

			// truncate arrays
			if (Mods != null)
			{
				ArrayUtil.TruncVals(ref Mods);
			}
			else
			{
				Mods = StatModifier.EmptyArray;
			}

			IsCharter = Flags.HasFlag(ItemFlags.Charter);

			RandomSuffixFactor = EnchantMgr.GetRandomSuffixFactor(this);

			if (IsCharter)
			{
				Creator = () => new PetitionCharter();
			}
			else if (IsContainer)
			{
				Creator = () => new Container();
			}
			else
			{
				Creator = () => new Item();
			}
		}
		#endregion

		/// <summary>
		/// Adds a new modifier to this Template
		/// </summary>
		public void AddMod(ItemModType modType, int value)
		{
			ArrayUtil.AddOnlyOne(ref Mods, new StatModifier { Type = modType, Value = value });
		}

		#region Checks
		/// <summary>
		/// Returns false if the looter may not take one of these items.
		/// E.g. due to quest requirements, if this is a quest item and the looter does not need it (yet, or anymore).
		/// </summary>
		/// <param name="looter">Can be null</param>
		public bool CheckLootConstraints(Character looter)
		{
			return CheckQuestConstraints(looter);
		}

		public bool CheckQuestConstraints(Character looter)
		{
			if (!HasQuestRequirements)			// no quest requirements
				return true;

			if (looter == null)
			{
				// cannot determine quest constraints if looter is offline
				return false;
			}

			if (QuestHolderInfo != null)
			{
				// starts a quest
				if (QuestHolderInfo.QuestStarts.Any(quest => looter.QuestLog.HasActiveQuest(quest)))
				{
					return false;
				}
			}

			if (CollectQuests != null)
			{
				// is collectable for one or more quests
				// check whether the looter has any of the required quests
				for (var i = 0; i < CollectQuests.Length; i++)
				{
					var q = CollectQuests[i];
					if (q != null)
					{
						if (looter.QuestLog.HasActiveQuest(q.Id))
						{
							for (int it = 0; it < q.CollectableItems.Length; it++)
							{
								if (q.CollectableItems[it].ItemId == ItemId)
								{
									if (q.CollectableItems[it].Amount > looter.QuestLog.GetActiveQuest(q.Id).CollectedItems[it])
									{
										return true;
									}
								}
							}
                            for (int it = 0; it < q.CollectableSourceItems.Length; it++)
                            {
                                if (q.CollectableSourceItems[it].ItemId == ItemId)
                                {
                                    if (q.CollectableSourceItems[it].Amount > looter.QuestLog.GetActiveQuest(q.Id).CollectedSourceItems[it])
                                    {
                                        return true;
                                    }
                                }
                            }
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Returns what went wrong (if anything) when the given unit tries to equip or use Items of this Template.
		/// </summary>
		public InventoryError CheckEquip(Character chr)
		{
			if (chr.GodMode)
			{
				return InventoryError.OK;
			}

			// level
			if (chr.Level < RequiredLevel)
			{
				return InventoryError.YOU_MUST_REACH_LEVEL_N;
			}

			// class
			if (RequiredClassMask != 0 && !RequiredClassMask.HasAnyFlag(chr.ClassMask))
			{
				return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM;
			}

			// race
			if (RequiredRaceMask != 0 && !RequiredRaceMask.HasAnyFlag(chr.RaceMask))
			{
				return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
			}

			// faction
			if (RequiredFaction != null)
			{
				if (chr.Faction != RequiredFaction)
				{
					return InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM2;
				}

				if (RequiredFactionStanding != StandingLevel.Hated &&
					chr.Reputations.GetStandingLevel(RequiredFaction.ReputationIndex) >= RequiredFactionStanding)
				{
					return InventoryError.ITEM_REPUTATION_NOT_ENOUGH;
				}
			}

			// skill
			if (RequiredSkill != null)
			{
				if (!chr.Skills.CheckSkill(RequiredSkill.Id, (int)RequiredSkillValue))
				{
					return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
				}
			}

			// ability
			if (RequiredProfession != null)
			{
				if (!chr.Spells.Contains(RequiredProfessionId))
				{
					return InventoryError.NO_REQUIRED_PROFICIENCY;
				}
			}

			// set
			if (Set != null)
			{
				if (Set.RequiredSkill != null)
				{
					if (!chr.Skills.CheckSkill(Set.RequiredSkill.Id, (int)Set.RequiredSkillValue))
					{
						return InventoryError.SKILL_ISNT_HIGH_ENOUGH;
					}
				}
			}

			// profession
			if (ItemProfession != SkillId.None)
			{
				if (!chr.Skills.Contains(ItemProfession))
				{
					return InventoryError.NO_REQUIRED_PROFICIENCY;
				}
			}

			// Disarmed
			if (IsWeapon && !chr.MayCarry(InventorySlotMask))
			{
				return InventoryError.CANT_DO_WHILE_DISARMED;
			}

			// TODO: Add missing restrictions
			// if (template.RequiredLockpickSkill
			// if (template.RequiredPvPRank
			// if (RequiredArenaRanking

			return InventoryError.OK;
		}

		internal void SetIsWeapon()
		{
			IsThrowable = InventorySlotType == InventorySlotType.Thrown;
			IsRangedWeapon = IsThrowable ||
				InventorySlotType == InventorySlotType.WeaponRanged ||
				InventorySlotType == InventorySlotType.RangedRight;
			IsMeleeWeapon = InventorySlotType == InventorySlotType.TwoHandWeapon ||
							InventorySlotType == InventorySlotType.Weapon ||
							InventorySlotType == InventorySlotType.WeaponMainHand ||
							InventorySlotType == InventorySlotType.WeaponOffHand;
			IsWeapon = IsRangedWeapon || IsMeleeWeapon;
		}

		private void CheckId()
		{
			// sanity check
			if (Id > ItemMgr.MaxId)
			{
				throw new Exception("Found item-template (" + Id + ") with Id > " + ItemMgr.MaxId + ". Items with such a high ID would blow the item storage array.");
			}
		}
		#endregion

		#region Interface implementations
		public ItemTemplate Template
		{
			get { return this; }
		}

		public ItemEnchantment[] Enchantments
		{
			get { return null; }
		}

		public bool IsEquipped
		{
			get { return false; }
		}

		public static IEnumerable<ItemTemplate> GetAllDataHolders()
		{
			return ItemMgr.Templates;
		}

		/// <summary>
		/// Contains the quests that this item can start (items usually can only start one)
		/// </summary>
		public QuestHolderInfo QuestHolderInfo
		{
			get;
			internal set;
		}

		public IWorldLocation[] GetInWorldTemplates()
		{
			return null;
		}

		public Item Create()
		{
			return Creator();
		}
		#endregion

		private void OnRecordCreated(ItemRecord record)
		{
			if (IsCharter)
			{
				if (!record.IsNew)
				{
					// this is executed in the IO-context
					PetitionRecord.LoadRecord(record.OwnerId);
				}
			}
		}


		#region Dump
		public void Dump(TextWriter writer)
		{
			Dump(writer, "");
		}


		public void Dump(TextWriter writer, string indent)
		{
			writer.WriteLine(indent + DefaultName + " (ID: " + Id + " [" + ItemId + "])");

			indent += "\t";
			var origIndent = indent;
			writer.WriteLine(origIndent + "Infos:");
			indent += "\t";
			if (Class != ItemClass.None)
			{
				writer.WriteLine(indent + "Class: " + Class);
			}
			if ((int)SubClass != 0)
			{
				writer.WriteLine(indent + "SubClass " + SubClass);
			}
			//if ((int)Field4 != -1)
			//{
			//    writer.WriteLine(indent + "Field4: " + Field4);
			//}
			if ((int)DisplayId != 0)
			{
				writer.WriteLine(indent + "DisplayId: " + DisplayId);
			}
			writer.WriteLine(indent + "Quality: " + Quality);
			if ((int)Flags != 0)
			{
				writer.WriteLine(indent + "Flags: " + Flags);
			}
			if ((int)Flags2 != 0)
			{
				writer.WriteLine(indent + "Flags2: " + Flags2);
			}
			if ((int)BuyPrice != 0)
			{
				writer.WriteLine(indent + "BuyPrice: " + Utility.FormatMoney(BuyPrice));
			}
			if ((int)SellPrice != 0)
			{
				writer.WriteLine(indent + "SellPrice: " + Utility.FormatMoney(SellPrice));
			}
			if ((int)Level != 0)
			{
				writer.WriteLine(indent + "Level: " + Level);
			}
			if ((int)RequiredLevel != 0)
			{
				writer.WriteLine(indent + "RequiredLevel: " + RequiredLevel);
			}
			if ((int)InventorySlotType != 0)
			{
				writer.WriteLine(indent + "InventorySlotType: " + InventorySlotType);
			}
			if ((int)UniqueCount != 0)
			{
				writer.WriteLine(indent + "UniqueCount: " + UniqueCount);
			}
			if ((int)MaxAmount != 1)
			{
				writer.WriteLine(indent + "MaxAmount: " + MaxAmount);
			}
			if ((int)ContainerSlots != 0)
			{
				writer.WriteLine(indent + "ContainerSlots: " + ContainerSlots);
			}
			if ((int)BlockValue != 0)
			{
				writer.WriteLine(indent + "BlockValue: " + BlockValue);
			}

			var mods = new List<string>(11);
			for (var i = 0; i < Mods.Length; i++)
			{
				var mod = Mods[i];
				if (mod.Value != 0)
				{
					mods.Add((mod.Value > 0 ? "+" : "") + mod.Value + " " + mod.Type);
				}
			}
			if (mods.Count > 0)
			{
				writer.WriteLine(indent + "Modifiers: " + mods.ToString("; "));
			}

			var damages = new List<string>(5);
			for (var i = 0; i < Damages.Length; i++)
			{
				var dmg = Damages[i];
				if (dmg.Maximum != 0)
				{
					damages.Add(dmg.Minimum + "-" + dmg.Maximum + " " + dmg.School);
				}
			}
			if (damages.Count > 0)
			{
				writer.WriteLine(indent + "Damages: " + damages.ToString("; "));
			}

			if (AttackTime != 0)
			{
				writer.WriteLine(indent + "AttackTime: " + AttackTime);
			}


			var resistances = new List<string>(5);
			for (var type = DamageSchool.Physical; type < DamageSchool.Count; type++)
			{
				var res = Resistances[(int)type];
				if (res > 0)
				{
					resistances.Add((res > 0 ? "+" : "") + res + " " + type);
				}
			}
			if (resistances.Count > 0)
			{
				writer.WriteLine(indent + "Resistances: " + resistances.ToString("; "));
			}

			var spells = new List<ItemSpell>();
			foreach (var spell in Spells)
			{
				if (spell.Id != SpellId.None)
				{
					spells.Add(spell);
				}
			}
			if (spells.Count > 0)
			{
				writer.WriteLine(indent + "Spells: " + spells.ToString("; "));
			}


			if ((int)BondType != 0)
			{
				writer.WriteLine(indent + "Binds: " + BondType);
			}
			if ((int)PageTextId != 0)
			{
				writer.WriteLine(indent + "PageId: " + PageTextId);
			}
			if ((int)PageMaterial != 0)
			{
				writer.WriteLine(indent + "PageMaterial: " + PageMaterial);
			}
			if ((int)LanguageId != 0)
			{
				writer.WriteLine(indent + "LanguageId: " + LanguageId);
			}
			if ((int)LockId != 0)
			{
				writer.WriteLine(indent + "Lock: " + LockId);
			}
			if ((uint)Material != uint.MaxValue)
			{
				writer.WriteLine(indent + "Material: " + Material);
			}
			if (Duration != 0)
			{
				writer.WriteLine(indent + "Duration: " + Duration);
			}
			if ((int)SheathType != 0)
			{
				writer.WriteLine(indent + "SheathType: " + SheathType);
			}
			if ((int)RandomPropertiesId != 0)
			{
				writer.WriteLine(indent + "RandomPropertyId: " + RandomPropertiesId);
			}
			// if ((float)RandomPropertyChance != 0) {
			if (RandomSuffixId != 0)
			{
				writer.WriteLine(indent + "RandomSuffixId: " + RandomSuffixId);
			}
			if (SetId != ItemSetId.None)
			{
				writer.WriteLine(indent + "Set: " + SetId);
			}
			if (MaxDurability != 0)
			{
				writer.WriteLine(indent + "MaxDurability: " + MaxDurability);
			}
			if ((int)MapId != 0)
			{
				writer.WriteLine(indent + "Map: " + MapId);
			}
			if ((int)ZoneId != 0)
			{
				writer.WriteLine(indent + "Zone: " + ZoneId);
			}
			if ((int)BagFamily != 0)
			{
				writer.WriteLine(indent + "BagFamily: " + BagFamily);
			}
			if ((int)ToolCategory != 0)
			{
				writer.WriteLine(indent + "TotemCategory: " + ToolCategory);
			}

			var sockets = new List<string>(3);
			foreach (var sock in Sockets)
			{
				if (sock.Color != 0 || sock.Content != 0)
				{
					sockets.Add(sock.Color + " (" + sock.Content + ")");
				}
			}
			if (sockets.Count > 0)
			{
				writer.WriteLine(indent + "Sockets: " + sockets.ToString("; "));
			}

			if (GemProperties != null)
			{
				writer.WriteLine(indent + "GemProperties: " + GemProperties);
			}

			if (ArmorModifier != 0)
			{
				writer.WriteLine(indent + "ArmorModifier: " + ArmorModifier);
			}
			if (RequiredDisenchantingLevel != -1 && RequiredDisenchantingLevel != 0)
			{
				writer.WriteLine(indent + "RequiredDisenchantingLevel: " + RequiredDisenchantingLevel);
			}

			if (DefaultDescription.Length > 0)
			{
				writer.WriteLine(indent + "Desc: " + DefaultDescription);
			}

			// requirements
			writer.WriteLine(origIndent + "Requirements:");
			if ((uint)RequiredClassMask != 0x3FFFF && (int)RequiredClassMask != -1)
			{
				writer.WriteLine(indent + "Classes: " + RequiredClassMask);
			}
			if ((int)RequiredRaceMask != -1)
			{
				writer.WriteLine(indent + "Races: " + RequiredRaceMask);
			}
			if (RequiredSkillId != SkillId.None)
			{
				writer.WriteLine(indent + "Skill: " + RequiredSkillValue + " " + RequiredSkillId);
			}
			if (RequiredProfessionId != SpellId.None)
			{
				writer.WriteLine(indent + "Profession: " + RequiredProfessionId);
			}
			if ((int)RequiredPvPRank != 0)
			{
				writer.WriteLine(indent + "PvPRank: " + RequiredPvPRank);
			}
			if ((int)UnknownRank != 0)
			{
				writer.WriteLine(indent + "UnknownRank: " + UnknownRank);
			}
			if (RequiredFactionId != FactionId.None)
			{
				writer.WriteLine(indent + "Faction: " + RequiredFactionId + " (" + RequiredFactionStanding + ")");
			}
			if ((int)QuestId != 0)
			{
				writer.WriteLine(indent + "Quest: " + QuestId);
			}
			if (QuestHolderInfo != null)
			{
				if (QuestHolderInfo.QuestStarts.Count > 0)
				{
					writer.WriteLine(indent + "QuestStarts: " + QuestHolderInfo.QuestStarts.ToString(", "));
				}
				if (QuestHolderInfo.QuestEnds.Count > 0)
				{
					writer.WriteLine(indent + "QuestEnds: " + QuestHolderInfo.QuestEnds.ToString(", "));
				}
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0} (Id: {1}{2})", DefaultName, Id,
				InventorySlotType != InventorySlotType.None ? " (" + InventorySlotType + ")" : "");
		}
	}
}