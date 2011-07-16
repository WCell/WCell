using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Logging;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items.Enchanting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Items
{
	[GlobalMgr]
	public static class ItemMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public const uint MaxId = 2500000;

		/// <summary>
		/// All defined <see cref="ItemTemplate">ItemTemplates</see>.
		/// </summary>
		[NotVariable]
		public static ItemTemplate[] Templates = new ItemTemplate[100000];

		/// <summary>
		/// All ItemSet definitions
		/// </summary>
		[NotVariable]
		public static ItemSet[] Sets = new ItemSet[1000];

		//[NotVariable]
		//public static List<ItemRandomPropertyInfo>[] RandomProperties = new List<ItemRandomPropertyInfo>[20000];

		//[NotVariable]
		//public static List<ItemRandomSuffixInfo>[] RandomSuffixes = new List<ItemRandomSuffixInfo>[20000];

		[NotVariable]
		public static MappedDBCReader<ItemLevelInfo, ItemRandPropPointConverter> RandomPropPointReader;

		[NotVariable]
		public static MappedDBCReader<ItemRandomPropertyEntry, ItemRandomPropertiesConverter> RandomPropertiesReader;

		[NotVariable]
		public static MappedDBCReader<ItemRandomSuffixEntry, ItemRandomSuffixConverter> RandomSuffixReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatDistributionEntry, ScalingStatDistributionConverter> ScalingStatDistributionReader;

        [NotVariable]
        public static MappedDBCReader<ScalingStatValues, ScalingStatValuesConverter> ScalingStatValuesReader;

		/// <summary>
		/// All partial inventory types by InventorySlot
		/// </summary>
		public static readonly PartialInventoryType[] PartialInventoryTypes =
			new PartialInventoryType[(int)InventorySlot.Count];

		/// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
		public static ItemTemplate GetTemplate(ItemId id)
		{
			if ((uint)id >= Templates.Length)
			{
				return null;
			}
			return Templates[(uint)id];
		}

		/// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
		public static ItemTemplate GetTemplateForced(ItemId id)
		{
			ItemTemplate templ;
			if ((uint)id >= Templates.Length)
			{
				templ = null;
			}
			else
			{
				templ = Templates[(uint)id];
			}

			if (templ == null)
			{
				throw new ContentException("Requested ItemTemplate does not exist: {0}", id);
			}
			return templ;
		}

		/// <summary>
		/// Returns the ItemTemplate with the given id
		/// </summary>
		public static ItemTemplate GetTemplate(uint id)
		{
			if (id >= Templates.Length)
			{
				return null;
			}
			return Templates[id];
		}

		/// <summary>
		///  Returns a List of templates with the given ItemClass
		/// </summary>
		public static IEnumerable<ItemTemplate> GetTemplates(ItemClass type)
		{
			return Templates.Where(template => template != null).Where(template => template.Class == type);
		}

		/// <summary>
		/// Returns the ItemSet with the given id
		/// </summary>
		public static ItemSet GetSet(ItemSetId id)
		{
			if ((uint)id >= Sets.Length)
			{
				return null;
			}
			return Sets[(uint)id];
		}

		/// <summary>
		/// Returns the ItemSet with the given id
		/// </summary>
		public static ItemSet GetSet(uint id)
		{
			if (id >= Sets.Length)
			{
				return null;
			}
			return Sets[id];
		}

		#region Slot Mapping

		public static readonly EquipmentSlot[] AllBagSlots = new[] { EquipmentSlot.Bag1, EquipmentSlot.Bag2, EquipmentSlot.Bag3, EquipmentSlot.Bag4 };

		public static EquipmentSlot[] GetEquipmentSlots(InventorySlotType invSlot)
		{
			return EquipmentSlotsByInvSlot[(int)invSlot];
		}

		/// <summary>
		/// Maps a set of available InventorySlots by their corresponding InventorySlotType
		/// </summary>
		public static readonly EquipmentSlot[][] EquipmentSlotsByInvSlot = GetEqByInv();

		public static readonly InventorySlot[][] EquippableInvSlotsByClass = GetEqByCl();

		static EquipmentSlot[][] GetEqByInv()
		{
			var slots = new EquipmentSlot[1 + (int)Utility.GetMaxEnum<InventorySlotType>()][];

			slots[(int)InventorySlotType.Bag] = AllBagSlots;
			slots[(int)InventorySlotType.Body] = new[] { EquipmentSlot.Shirt };
			slots[(int)InventorySlotType.Chest] = new[] { EquipmentSlot.Chest };
			slots[(int)InventorySlotType.Cloak] = new[] { EquipmentSlot.Back };
			slots[(int)InventorySlotType.Feet] = new[] { EquipmentSlot.Boots };
			slots[(int)InventorySlotType.Finger] = new[] { EquipmentSlot.Finger1, EquipmentSlot.Finger2 };
			slots[(int)InventorySlotType.Hand] = new[] { EquipmentSlot.Gloves };
			slots[(int)InventorySlotType.Head] = new[] { EquipmentSlot.Head };
			slots[(int)InventorySlotType.Holdable] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Legs] = new[] { EquipmentSlot.Pants };
			slots[(int)InventorySlotType.Neck] = new[] { EquipmentSlot.Neck };
			slots[(int)InventorySlotType.Quiver] = AllBagSlots;
			slots[(int)InventorySlotType.WeaponRanged] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.RangedRight] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Relic] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Robe] = new[] { EquipmentSlot.Chest };
			slots[(int)InventorySlotType.Shield] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Shoulder] = new[] { EquipmentSlot.Shoulders };
			slots[(int)InventorySlotType.Tabard] = new[] { EquipmentSlot.Tabard };
			slots[(int)InventorySlotType.Thrown] = new[] { EquipmentSlot.ExtraWeapon };
			slots[(int)InventorySlotType.Trinket] = new[] { EquipmentSlot.Trinket1, EquipmentSlot.Trinket2 };
			slots[(int)InventorySlotType.TwoHandWeapon] = new[] { EquipmentSlot.MainHand };
			slots[(int)InventorySlotType.Waist] = new[] { EquipmentSlot.Belt };
			slots[(int)InventorySlotType.Weapon] = new[] { EquipmentSlot.MainHand, EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.WeaponMainHand] = new[] { EquipmentSlot.MainHand };
			slots[(int)InventorySlotType.WeaponOffHand] = new[] { EquipmentSlot.OffHand };
			slots[(int)InventorySlotType.Wrist] = new[] { EquipmentSlot.Wrist };

			// special treatment
			slots[(int)InventorySlotType.Ammo] = null; // new[] { EquipmentSlot.Invalid };
			return slots;
		}

		private static InventorySlot[][] GetEqByCl()
		{
			var slots = new InventorySlot[(int)ItemClass.End][];
			slots[(int)ItemClass.Weapon] = new[] { InventorySlot.MainHand, InventorySlot.OffHand, InventorySlot.ExtraWeapon };
			slots[(int)ItemClass.Armor] = new[] { InventorySlot.Chest, InventorySlot.Boots, InventorySlot.Gloves, InventorySlot.Head,
				InventorySlot.Pants, InventorySlot.Chest, InventorySlot.Shoulders, InventorySlot.Wrist, InventorySlot.Belt };
			return slots;
		}
		#endregion

		#region Slots
		public static readonly InventorySlot[] EquipmentSlots = new[] {
			InventorySlot.Head,
			InventorySlot.Neck,
			InventorySlot.Shoulders,
			InventorySlot.Shirt,
			InventorySlot.Chest,
			InventorySlot.Belt,
			InventorySlot.Pants,
			InventorySlot.Boots,
			InventorySlot.Wrist,
			InventorySlot.Gloves,
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard
		};

		/// <summary>
		/// Contains all InventorySlots that are used as storage on the Character, without bank slots
		/// </summary>
		public static readonly InventorySlot[] StorageSlotsWithoutBank = new[] {
			InventorySlot.BackPack1,
			InventorySlot.BackPack2,
			InventorySlot.BackPack3,
			InventorySlot.BackPack4,
			InventorySlot.BackPack5,
			InventorySlot.BackPack6,
			InventorySlot.BackPack7,
			InventorySlot.BackPack8,
			InventorySlot.BackPack9,
			InventorySlot.BackPack10,
			InventorySlot.BackPack11,
			InventorySlot.BackPack12,
			InventorySlot.BackPack13,
			InventorySlot.BackPack14,
			InventorySlot.BackPack15,
			InventorySlot.BackPackLast,
			InventorySlot.Bag1,
			InventorySlot.Bag2,
			InventorySlot.Bag3,
			InventorySlot.BagLast
		};

		/// <summary>
		/// Contains all Equipment and on-character inventory slots without keys
		/// </summary>
		public static readonly InventorySlot[] InvSlots = new[] {
			InventorySlot.Head,
			InventorySlot.Neck,
			InventorySlot.Shoulders,
			InventorySlot.Shirt,
			InventorySlot.Chest,
			InventorySlot.Belt,
			InventorySlot.Pants,
			InventorySlot.Boots,
			InventorySlot.Wrist,
			InventorySlot.Gloves,
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard,
			InventorySlot.Bag1,
			InventorySlot.Bag2,
			InventorySlot.Bag3,
			InventorySlot.BagLast,
			InventorySlot.BackPack1,
			InventorySlot.BackPack2,
			InventorySlot.BackPack3,
			InventorySlot.BackPack4,
			InventorySlot.BackPack5,
			InventorySlot.BackPack6,
			InventorySlot.BackPack7,
			InventorySlot.BackPack8,
			InventorySlot.BackPack9,
			InventorySlot.BackPack10,
			InventorySlot.BackPack11,
			InventorySlot.BackPack12,
			InventorySlot.BackPack13,
			InventorySlot.BackPack14,
			InventorySlot.BackPack15,
			InventorySlot.BackPackLast,
			InventorySlot.Bag1,
			InventorySlot.Bag2,
			InventorySlot.Bag3,
			InventorySlot.BagLast
		};

		/// <summary>
		/// Contains all InventorySlots that are used as storage on the Character, including bank slots
		/// </summary>
		public static readonly InventorySlot[] InvSlotsWithBank = new[] {
			InventorySlot.Head,
			InventorySlot.Neck,
			InventorySlot.Shoulders,
			InventorySlot.Shirt,
			InventorySlot.Chest,
			InventorySlot.Belt,
			InventorySlot.Pants,
			InventorySlot.Boots,
			InventorySlot.Wrist,
			InventorySlot.Gloves,
			InventorySlot.Finger1,
			InventorySlot.Finger2,
			InventorySlot.Trinket1,
			InventorySlot.Trinket2,
			InventorySlot.Back,
			InventorySlot.MainHand,
			InventorySlot.OffHand,
			InventorySlot.ExtraWeapon,
			InventorySlot.Tabard,
			InventorySlot.Bag1,
			InventorySlot.Bag2,
			InventorySlot.Bag3,
			InventorySlot.BagLast,
		                                                    		InventorySlot.BackPack1,
		                                                    		InventorySlot.BackPack2,
		                                                    		InventorySlot.BackPack3,
		                                                    		InventorySlot.BackPack4,
		                                                    		InventorySlot.BackPack5,
		                                                    		InventorySlot.BackPack6,
		                                                    		InventorySlot.BackPack7,
		                                                    		InventorySlot.BackPack8,
		                                                    		InventorySlot.BackPack9,
		                                                    		InventorySlot.BackPack10,
		                                                    		InventorySlot.BackPack11,
		                                                    		InventorySlot.BackPack12,
		                                                    		InventorySlot.BackPack13,
		                                                    		InventorySlot.BackPack14,
		                                                    		InventorySlot.BackPack15,
		                                                    		InventorySlot.BackPackLast,
		                                                    		InventorySlot.Bag1,
		                                                    		InventorySlot.Bag2,
		                                                    		InventorySlot.Bag3,
		                                                    		InventorySlot.BagLast,
		                                                    		InventorySlot.Bank1,
		                                                    		InventorySlot.Bank2,
		                                                    		InventorySlot.Bank3,
		                                                    		InventorySlot.Bank4,
		                                                    		InventorySlot.Bank5,
		                                                    		InventorySlot.Bank6,
		                                                    		InventorySlot.Bank7,
		                                                    		InventorySlot.Bank8,
		                                                    		InventorySlot.Bank9,
		                                                    		InventorySlot.Bank10,
		                                                    		InventorySlot.Bank11,
		                                                    		InventorySlot.Bank12,
		                                                    		InventorySlot.Bank13,
		                                                    		InventorySlot.Bank14,
		                                                    		InventorySlot.Bank15,
		                                                    		InventorySlot.Bank16,
		                                                    		InventorySlot.Bank17,
		                                                    		InventorySlot.Bank18,
		                                                    		InventorySlot.Bank19,
		                                                    		InventorySlot.Bank20,
		                                                    		InventorySlot.Bank21,
		                                                    		InventorySlot.Bank22,
		                                                    		InventorySlot.Bank23,
		                                                    		InventorySlot.Bank24,
		                                                    		InventorySlot.Bank25,
		                                                    		InventorySlot.Bank26,
		                                                    		InventorySlot.Bank27,
		                                                    		InventorySlot.BankLast,
		                                                    		InventorySlot.BankBag1,
		                                                    		InventorySlot.BankBag2,
		                                                    		InventorySlot.BankBag3,
		                                                    		InventorySlot.BankBag4,
		                                                    		InventorySlot.BankBag5,
		                                                    		InventorySlot.BankBag6,
		                                                    		InventorySlot.BankBagLast
		                                                    	};


		/// <summary>
		/// Contains all BankSlots
		/// </summary>
		public readonly static InventorySlot[] BankSlots = new[] {
			InventorySlot.Bank1,
			InventorySlot.Bank2,
			InventorySlot.Bank3,
			InventorySlot.Bank4,
			InventorySlot.Bank5,
			InventorySlot.Bank6,
			InventorySlot.Bank7,
			InventorySlot.Bank8,
			InventorySlot.Bank9,
			InventorySlot.Bank10,
			InventorySlot.Bank11,
			InventorySlot.Bank12,
			InventorySlot.Bank13,
			InventorySlot.Bank14,
			InventorySlot.Bank15,
			InventorySlot.Bank16,
			InventorySlot.Bank17,
			InventorySlot.Bank18,
			InventorySlot.Bank19,
			InventorySlot.Bank20,
			InventorySlot.Bank21,
			InventorySlot.Bank22,
			InventorySlot.Bank23,
			InventorySlot.Bank24,
			InventorySlot.Bank25,
			InventorySlot.Bank26,
			InventorySlot.Bank27,
			InventorySlot.BankLast
		                                          	};

		/// <summary>
		/// Contains all InventorySlots for BankBags
		/// </summary>
		public static readonly InventorySlot[] BankBagSlots = new[]
		                                             	{
		                                             		InventorySlot.BankBag1,
		                                             		InventorySlot.BankBag2,
		                                             		InventorySlot.BankBag3,
		                                             		InventorySlot.BankBag4,
		                                             		InventorySlot.BankBag5,
		                                             		InventorySlot.BankBag6,
		                                             		InventorySlot.BankBagLast
		                                             	};

		/// <summary>
		/// All slots that can contain Items that actually belong to the Character (all InventorySlots, but BuyBack)
		/// </summary>
		public static readonly InventorySlot[] OwnedSlots = new[]
		                                           	{
		                                           		InventorySlot.Head,
		                                           		InventorySlot.Neck,
		                                           		InventorySlot.Shoulders,
		                                           		InventorySlot.Shirt,
		                                           		InventorySlot.Chest,
		                                           		InventorySlot.Belt,
		                                           		InventorySlot.Pants,
		                                           		InventorySlot.Boots,
		                                           		InventorySlot.Wrist,
		                                           		InventorySlot.Gloves,
		                                           		InventorySlot.Finger1,
		                                           		InventorySlot.Finger2,
		                                           		InventorySlot.Trinket1,
		                                           		InventorySlot.Trinket2,
		                                           		InventorySlot.Back,
		                                           		InventorySlot.MainHand,
		                                           		InventorySlot.OffHand,
		                                           		InventorySlot.ExtraWeapon,
		                                           		InventorySlot.Tabard,
		                                           		InventorySlot.Bag1,
		                                           		InventorySlot.Bag2,
		                                           		InventorySlot.Bag3,
		                                           		InventorySlot.BagLast,
		                                           		InventorySlot.BackPack1,
		                                           		InventorySlot.BackPack2,
		                                           		InventorySlot.BackPack3,
		                                           		InventorySlot.BackPack4,
		                                           		InventorySlot.BackPack5,
		                                           		InventorySlot.BackPack6,
		                                           		InventorySlot.BackPack7,
		                                           		InventorySlot.BackPack8,
		                                           		InventorySlot.BackPack9,
		                                           		InventorySlot.BackPack10,
		                                           		InventorySlot.BackPack11,
		                                           		InventorySlot.BackPack12,
		                                           		InventorySlot.BackPack13,
		                                           		InventorySlot.BackPack14,
		                                           		InventorySlot.BackPack15,
		                                           		InventorySlot.BackPackLast,
		                                           		InventorySlot.Bank1,
		                                           		InventorySlot.Bank2,
		                                           		InventorySlot.Bank3,
		                                           		InventorySlot.Bank4,
		                                           		InventorySlot.Bank5,
		                                           		InventorySlot.Bank6,
		                                           		InventorySlot.Bank7,
		                                           		InventorySlot.Bank8,
		                                           		InventorySlot.Bank9,
		                                           		InventorySlot.Bank10,
		                                           		InventorySlot.Bank11,
		                                           		InventorySlot.Bank12,
		                                           		InventorySlot.Bank13,
		                                           		InventorySlot.Bank14,
		                                           		InventorySlot.Bank15,
		                                           		InventorySlot.Bank16,
		                                           		InventorySlot.Bank17,
		                                           		InventorySlot.Bank18,
		                                           		InventorySlot.Bank19,
		                                           		InventorySlot.Bank20,
		                                           		InventorySlot.Bank21,
		                                           		InventorySlot.Bank22,
		                                           		InventorySlot.Bank23,
		                                           		InventorySlot.Bank24,
		                                           		InventorySlot.Bank25,
		                                           		InventorySlot.Bank26,
		                                           		InventorySlot.Bank27,
		                                           		InventorySlot.BankLast,
		                                           		InventorySlot.BankBag1,
		                                           		InventorySlot.BankBag2,
		                                           		InventorySlot.BankBag3,
		                                           		InventorySlot.BankBag4,
		                                           		InventorySlot.BankBag5,
		                                           		InventorySlot.BankBag6,
		                                           		InventorySlot.BankBagLast,
		                                           		InventorySlot.Key1,
		                                           		InventorySlot.Key2,
		                                           		InventorySlot.Key3,
		                                           		InventorySlot.Key4,
		                                           		InventorySlot.Key5,
		                                           		InventorySlot.Key6,
		                                           		InventorySlot.Key7,
		                                           		InventorySlot.Key8,
		                                           		InventorySlot.Key9,
		                                           		InventorySlot.Key10,
		                                           		InventorySlot.Key11,
		                                           		InventorySlot.Key12,
		                                           		InventorySlot.Key13,
		                                           		InventorySlot.Key14,
		                                           		InventorySlot.Key15,
		                                           		InventorySlot.Key16,
		                                           		InventorySlot.Key17,
		                                           		InventorySlot.Key18,
		                                           		InventorySlot.Key19,
		                                           		InventorySlot.Key20,
		                                           		InventorySlot.Key21,
		                                           		InventorySlot.Key22,
		                                           		InventorySlot.Key23,
		                                           		InventorySlot.Key24,
		                                           		InventorySlot.Key25,
		                                           		InventorySlot.Key26,
		                                           		InventorySlot.Key27,
		                                           		InventorySlot.Key28,
		                                           		InventorySlot.Key29,
		                                           		InventorySlot.Key30,
		                                           		InventorySlot.Key31,
		                                           		InventorySlot.KeyLast
		                                           	};

		[NotVariable]
		/// <summary>
		/// All slots that can contain Items
		/// </summary>
		public static InventorySlot[] AllSlots;

		[NotVariable]
		/// <summary>
		/// Contains true for all InventorySlots that can contain equippable bags (4 bag slots and up to 7 bank bag slots)
		/// </summary>
		public static bool[] ContainerSlotsWithBank;

		[NotVariable]
		/// <summary>
		/// Contains true for all bag-InventorySlots
		/// </summary>
		public static bool[] ContainerSlotsWithoutBank;

		[NotVariable]
		/// <summary>
		/// Contains true for all InventorySlots that represent BankBags
		/// </summary>
		public static bool[] ContainerBankSlots;

		public static bool IsContainerEquipmentSlot(int slot)
		{
			return (slot >= (int)InventorySlot.Bag1 && slot <= (int)InventorySlot.BagLast) ||
				(slot >= (int)InventorySlot.BankBag1 && slot <= (int)InventorySlot.BankBagLast);
		}
		#endregion

		static ItemMgr()
		{
			var list = ((InventorySlot[])Enum.GetValues(typeof(InventorySlot))).ToList();
			list.Remove(InventorySlot.Count);
			list.Remove(InventorySlot.Invalid);
			AllSlots = list.ToArray();
		}

		#region Load & Initialize
		[Initialization(InitializationPass.Fourth, "Initialize Items")]
		public static void Initialize()
		{
			InitMisc();
#if !DEV
			LoadAll();
#endif
		}

		public static void ForceInitialize()
		{
			InitMisc();
			if (!Loaded)
			{
				LoadAll();
			}
		}

		public static void InitMisc()
		{
			if (ContainerSlotsWithBank == null)
			{
				LockEntry.Initialize();
				LoadDBCs();
				LoadSets();

				ContainerSlotsWithBank = new bool[(int)InventorySlot.Count];
				ContainerSlotsWithBank.Fill(true, (int)InventorySlot.Bag1, (int)InventorySlot.BagLast);
				ContainerSlotsWithBank.Fill(true, (int)InventorySlot.BankBag1, (int)InventorySlot.BankBagLast);

				ContainerSlotsWithoutBank = new bool[(int)InventorySlot.Count];
				ContainerSlotsWithoutBank.Fill(true, (int)InventorySlot.Bag1, (int)InventorySlot.BagLast);

				ContainerBankSlots = new bool[(int)InventorySlot.Count];
				ContainerBankSlots.Fill(true, (int)InventorySlot.BankBag1, (int)InventorySlot.BankBagLast);

				InitItemSlotHandlers();
			}
		}

		private static void LoadDBCs()
		{
			RandomPropPointReader = new MappedDBCReader<ItemLevelInfo, ItemRandPropPointConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_RANDPROPPOINTS));

			EnchantMgr.Init();

			RandomPropertiesReader =
				new MappedDBCReader<ItemRandomPropertyEntry, ItemRandomPropertiesConverter>(RealmServerConfiguration.GetDBCFile(
																						WCellConstants.DBC_ITEMRANDOMPROPERTIES));

			RandomSuffixReader =
				new MappedDBCReader<ItemRandomSuffixEntry, ItemRandomSuffixConverter>(RealmServerConfiguration.GetDBCFile(
																					WCellConstants.DBC_ITEMRANDOMSUFFIX));

            ScalingStatDistributionReader = new MappedDBCReader<ScalingStatDistributionEntry, ScalingStatDistributionConverter>(RealmServerConfiguration.GetDBCFile(
                                                                                    WCellConstants.DBC_SCALINGSTATDISTRIBUTION));

            ScalingStatValuesReader = new MappedDBCReader<ScalingStatValues, ScalingStatValuesConverter>(RealmServerConfiguration.GetDBCFile(
                                                                                    WCellConstants.DBC_SCALINGSTATVALUES));
		}

		public static bool Loaded { get; private set; }

		public static void LoadAll()
		{
			if (!Loaded)
			{
				//ContentHandler.Load<ItemRandomSuffixInfo>();
				ContentMgr.Load<ItemTemplate>();
				ContentMgr.Load<ItemRandomEnchantEntry>();

				OnLoaded();

				foreach (var templ in Templates)
				{
					if (templ != null)
					{
						templ.InitializeTemplate();
					}
				}

				TruncSets();

				if (ArchetypeMgr.Loaded)
				{
					ArchetypeMgr.LoadItems();
				}

				SpellHandler.InitTools();
				LoadItemCharRelations();

				AuctionMgr.Instance.LoadItems();

				if (QuestMgr.Loaded)
				{
					EnsureItemQuestRelations();
				}

				RealmServer.InitMgr.SignalGlobalMgrReady(typeof(ItemMgr));
				Loaded = true;
			}
		}

		private static void LoadItemCharRelations()
		{
			foreach (var chr in World.GetAllCharacters())
			{
				var context = chr.ContextHandler;
				if (context != null)
				{
					var character = chr;
					context.AddMessage(() =>
					{
						if (character.IsInWorld)
						{
							character.InitItems();
						}
					});
				}
			}
		}

		internal static void EnsureItemQuestRelations()
		{
			// Collect quests
			foreach (var quest in QuestMgr.Templates)
			{
				if (quest == null)
				{
					continue;
				}
				if (quest.CollectableItems == null)
				{
					continue;
				}

				foreach (var itemInfo in quest.CollectableItems)
				{
					var item = GetTemplate(itemInfo.ItemId);
					if (item == null)
					{
						ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
													   quest, itemInfo);
					}
					else
					{
						if (item.CollectQuests == null)
						{
							item.CollectQuests = new[] { quest };
						}
						else
						{
							ArrayUtil.AddOnlyOne(ref item.CollectQuests, quest);
						}
					}
				}

                foreach (var itemInfo in quest.CollectableSourceItems)
                {
                    var item = GetTemplate(itemInfo.ItemId);
                    if (item == null)
                    {
                        ContentMgr.OnInvalidDBData("QuestTemplate \"{0}\" refered to non-existing Item: {1}",
                                                       quest, itemInfo);
                    }
                    else
                    {
                        if (item.CollectQuests == null)
                        {
                            item.CollectQuests = new[] { quest };
                        }
                        else
                        {
                            ArrayUtil.AddOnlyOne(ref item.CollectQuests, quest);
                        }
                    }
                }
			}

			// Item QuestGivers
			foreach (var item in Templates)
			{
				if (item != null && item.QuestId != 0)
				{
					var quest = QuestMgr.GetTemplate(item.QuestId);
					if (quest == null)
					{
						ContentMgr.OnInvalidDBData("Item {0} had invalid QuestId: {1}", item, item.QuestId);
						continue;
					}
					quest.Starters.Add(item);
				}
			}
		}

		/// <summary>
		/// Load item-set info from the DBCs (automatically called on startup)
		/// </summary>
		public static void LoadSets()
		{
			var reader = new MappedDBCReader<ItemSet, ItemSet.ItemSetDBCConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_ITEMSET));

			foreach (var set in reader.Entries.Values)
			{
				if (set.Id >= Sets.Length)
				{
					Array.Resize(ref Sets, (int)set.Id + 10);
				}
				Sets[(int)set.Id] = set;
			}
		}

		/// <summary>
		/// Resize all Template-Arrays of sets to their actual size.
		/// </summary>
		internal static void TruncSets()
		{
			foreach (var set in Sets)
			{
				if (set != null)
				{
					for (uint i = 0; i < set.Templates.Length; i++)
					{
						if (set.Templates[i] == null)
						{
							// truncate
							Array.Resize(ref set.Templates, (int)i);
							break;
						}
					}
				}
			}
		}
		#endregion

		#region PartialInventoryTypes

		public static void InitItemSlotHandlers()
		{
			PartialInventoryTypes.Fill(PartialInventoryType.Equipment, (int)InventorySlot.Head,
						 (int)InventorySlot.Tabard);
			PartialInventoryTypes.Fill(PartialInventoryType.BackPack, (int)InventorySlot.BackPack1,
						 (int)InventorySlot.BackPackLast);
			PartialInventoryTypes.Fill(PartialInventoryType.EquippedContainers, (int)InventorySlot.Bag1,
						 (int)InventorySlot.BagLast);
			PartialInventoryTypes.Fill(PartialInventoryType.Bank, (int)InventorySlot.Bank1,
						 (int)InventorySlot.BankLast);
			PartialInventoryTypes.Fill(PartialInventoryType.BankBags, (int)InventorySlot.BankBag1,
						 (int)InventorySlot.BankBagLast);
			PartialInventoryTypes.Fill(PartialInventoryType.BuyBack, (int)InventorySlot.BuyBack1,
						 (int)InventorySlot.BuyBackLast);
			PartialInventoryTypes.Fill(PartialInventoryType.KeyRing, (int)InventorySlot.Key1,
						 (int)InventorySlot.KeyLast);
		}

		#endregion

		#region TotemCategories
		[NotVariable]
		public static readonly ItemTemplate[] FirstTotemsPerCat = new ItemTemplate[(uint)ToolCategory.End + 100];

		public static ItemTemplate GetFirstItemOfToolCategory(ToolCategory toolCat)
		{
			return FirstTotemsPerCat[(int)toolCat];
		}

		public static EquipmentSlot[] GetToolCategorySlots(ToolCategory toolCat)
		{
			var templ = FirstTotemsPerCat[(int) toolCat];
			if (templ == null) return null;

			return templ.EquipmentSlots;
		}

		public static Dictionary<int, TotemCategoryInfo> ReadTotemCategories()
		{
			var reader = new MappedDBCReader<TotemCategoryInfo, TotemCatConverter>(RealmServerConfiguration.GetDBCFile(
                                                                                    WCellConstants.DBC_TOTEMCATEGORY));
			return reader.Entries;
		}

		public struct TotemCategoryInfo
		{
			public int Id;
			public string Name;
		}

		public class TotemCatConverter : AdvancedDBCRecordConverter<TotemCategoryInfo>
		{
			public override TotemCategoryInfo ConvertTo(byte[] rawData, ref int id)
			{
				var cat = new TotemCategoryInfo
				{
					Id = (id = GetInt32(rawData, 0)),
					Name = GetString(rawData, 1)
				};

				return cat;
			}
		}
		#endregion

		public static ItemLevelInfo GetLevelInfo(uint itemLevel)
		{
			if (RandomPropPointReader == null)
			{
				LoadDBCs();
			}
			ItemLevelInfo info;
			RandomPropPointReader.Entries.TryGetValue((int)itemLevel, out info);
			return info;
		}

		public static ItemRandomPropertyEntry GetRandomPropertyEntry(uint id)
		{
            if (RandomPropertiesReader == null)
			{
				LoadDBCs();
			}
			ItemRandomPropertyEntry entry;
			RandomPropertiesReader.Entries.TryGetValue((int)id, out entry);
			return entry;
		}

		public static ItemRandomSuffixEntry GetRandomSuffixEntry(uint id)
		{
            if (RandomSuffixReader == null)
			{
				LoadDBCs();
			}
			ItemRandomSuffixEntry entry;
			RandomSuffixReader.Entries.TryGetValue((int)id, out entry);
			return entry;
		}

        public static ScalingStatDistributionEntry GetScalingStatDistributionEntry(uint id)
        {
            if (ScalingStatDistributionReader == null)
            {
                LoadDBCs();
            }
            ScalingStatDistributionEntry entry;
            ScalingStatDistributionReader.Entries.TryGetValue((int)id, out entry);
            return entry;
        }

        public static ScalingStatValues GetScalingStatValue(uint id)
        {
            if (ScalingStatValuesReader == null)
            {
                LoadDBCs();
            }
            ScalingStatValues entry;
            ScalingStatValuesReader.Entries.TryGetValue((int)id, out entry);
            return entry;
        }

		#region Apply changes when loading
		private static readonly List<Tuple<ItemId, Action<ItemTemplate>>> loadHooks = new List<Tuple<ItemId, Action<ItemTemplate>>>();
		private static readonly List<Tuple<ItemClass, Action<ItemTemplate>>> itemClassLoadHooks = new List<Tuple<ItemClass, Action<ItemTemplate>>>();
		/// <summary>
		/// Adds a callback to be called on the given set of ItemTemplates after load and before Item initialization
		/// </summary>
		public static void Apply(Action<ItemTemplate> cb, params ItemId[] ids)
		{
			foreach (var id in ids)
			{
				loadHooks.Add(Tuple.Create(id, cb));
			}
		}

		public static void Apply(Action<ItemTemplate> cb, params ItemClass[] classes)
		{
			foreach (var cls in classes)
			{
				itemClassLoadHooks.Add(Tuple.Create(cls, cb));
			}
		}

		static void OnLoaded()
		{
			// Perform the action on a template
			foreach (var hook in loadHooks)
			{
				hook.Item2(GetTemplateForced(hook.Item1));
			}

			// Perform an action an each member of the itemclasses
			foreach(var hook in itemClassLoadHooks)
			{
				foreach(var template in GetTemplates(hook.Item1))
				{
					hook.Item2(template);
				}
			}
		}
		#endregion
	}
}