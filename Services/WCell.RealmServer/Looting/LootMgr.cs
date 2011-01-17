using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.Util;

namespace WCell.RealmServer.Looting
{
	/// <summary>
	/// Static utility and container class for Looting.
	/// </summary>
	[GlobalMgr]
	public static class LootMgr
	{
		/// <summary>
		/// The highest roll that someone can get when rolling for Need before Greed.
		/// </summary>
		public const int HighestRoll = 100;

		#region Global Variables
		/// <summary>
		/// Everyone in the Group who is within this Radius, can loot and gets a share of the money
		/// </summary>
		public static float LootRadius = 200f;

		/// <summary>
		/// The factor to be applied to each Item's drop-chance before determining whether it will drop or not.
		/// </summary>
		public static float LootItemDropFactor = 1f;

		/// <summary>
		/// If set, LootItems below threshold always are distributed using RoundRobin rules.
		/// Set it to false to have FFA rules apply by all default Items below threshold.
		/// </summary>
		public static bool RoundRobinDefault = true;

		/// <summary>
		/// Maximum amount of <see cref="LootItem">Items</see> in the loot of one object or corpse.
		/// </summary>
		public static int MaxLootCount = 15;

		/// <summary>
		/// The timeout for a LootRoll in milliseconds.
		/// Default: 1 min
		/// TODO: Implement timeout timer
		/// </summary>
		public static int DefaultLootRollTimeout = 60 * 1000;
		#endregion

		public static readonly ResolvedLootItemList[][] LootEntries = new ResolvedLootItemList[(int)LootEntryType.Count][];

		internal static readonly List<KeyValuePair<ResolvedLootItemList, LootItemEntry>> ReferenceEntries = new List<KeyValuePair<ResolvedLootItemList, LootItemEntry>>();

		#region Init & Load
		static LootMgr()
		{
			LootEntries[(int)LootEntryType.Disenchanting] = new ResolvedLootItemList[2000];
			LootEntries[(int)LootEntryType.Fishing] = new ResolvedLootItemList[2000];
			LootEntries[(int)LootEntryType.GameObject] = new ResolvedLootItemList[5000];
			LootEntries[(int)LootEntryType.Item] = new ResolvedLootItemList[5000];
			LootEntries[(int)LootEntryType.NPCCorpse] = new ResolvedLootItemList[330000];
			LootEntries[(int)LootEntryType.PickPocketing] = new ResolvedLootItemList[10000];
			LootEntries[(int)LootEntryType.Prospecting] = new ResolvedLootItemList[20];
			LootEntries[(int)LootEntryType.Skinning] = new ResolvedLootItemList[400];
			LootEntries[(int)LootEntryType.Milling] = new ResolvedLootItemList[400];
			LootEntries[(int)LootEntryType.Reference] = new ResolvedLootItemList[10000];
		}

		private static bool loaded;

		public static bool Loaded
		{
			get { return loaded; }
			private set
			{
				if (loaded = value)
				{
					RealmServer.InitMgr.SignalGlobalMgrReady(typeof(LootMgr));
				}
			}
		}

#if !DEV
		[Initialization(InitializationPass.Tenth, "Load Loot")]
#endif
		public static void LoadAll()
		{
			if (!Loaded)
			{
				ContentMgr.Load<NPCLootItemEntry>();
				ContentMgr.Load<ItemLootItemEntry>();
				ContentMgr.Load<GOLootItemEntry>();
				ContentMgr.Load<FishingLootItemEntry>();
				ContentMgr.Load<MillingLootItemEntry>();
				ContentMgr.Load<PickPocketLootItemEntry>();
				ContentMgr.Load<ProspectingLootItemEntry>();
				ContentMgr.Load<DisenchantingLootItemEntry>();
				ContentMgr.Load<ReferenceLootItemEntry>();

				for (var i = ReferenceEntries.Count - 1; i >= 0; i--)
				{
					var pair = ReferenceEntries[i];
					pair.Key.Remove(pair.Value);
					LookupRef(pair.Key, pair.Value);
				}

				Loaded = true;
			}
		}

		static void LookupRef(ResolvedLootItemList list, LootItemEntry entry)
		{
			// TODO: Loot groups (see http://udbwiki.webhop.net/index.php/Gameobject_loot_template#groupid)

			var referencedEntries = GetEntries(LootEntryType.Reference, entry.ReferencedEntryId);
			if (referencedEntries != null)
			{
				if (referencedEntries.ResolveStatus < 1)
				{
					referencedEntries.ResolveStatus = 1;
					foreach (var refEntry in referencedEntries)
					{
						if (refEntry is LootItemEntry)
						{
							var itemEntry = (LootItemEntry)refEntry;
							if (itemEntry.ReferencedEntryId > 0)
							{
								LookupRef(list, itemEntry);
								continue;
							}
						}
						AddRef(list, refEntry);
					}
					referencedEntries.ResolveStatus = 2;
				}
				else if (list.ResolveStatus == 1)
				{
					LogManager.GetCurrentClassLogger().Warn("Infinite loop in Loot references detected in: " + entry);
				}
				else
				{
					foreach (var refEntry in referencedEntries)
					{
						AddRef(list, refEntry);
					}
				}
			}
		}

		private static void AddRef(ResolvedLootItemList list, LootEntity ent)
		{
			list.Add(ent);
		}

		public static void AddEntry(LootItemEntry entry)
		{
			var entries = LootEntries[(uint)entry.LootType];
			if (entry.EntryId >= entries.Length)
			{
				ArrayUtil.EnsureSize(ref entries, (int)(entry.EntryId * ArrayUtil.LoadConstant) + 1);
				LootEntries[(uint)entry.LootType] = entries;
			}

			var list = entries[entry.EntryId];
			if (list == null)
			{
				entries[entry.EntryId] = list = new ResolvedLootItemList();
			}

			if (entry.ReferencedEntryId > 0 || entry.GroupId > 0)
			{
				ReferenceEntries.Add(new KeyValuePair<ResolvedLootItemList, LootItemEntry>(list, entry));
			}

			// add entry sorted
			var added = false;
			for (var i = 0; i < list.Count; i++)
			{
				var ent = list[i];
				if (ent.DropChance > entry.DropChance)
				{
					added = true;
					list.Insert(i, entry);
					break;
				}
			}

			if (!added)
			{
				list.Add(entry);
			}
		}
		#endregion

		public static ResolvedLootItemList[] GetEntries(LootEntryType type)
		{
			return LootEntries[(uint)type];
		}

		public static ResolvedLootItemList GetEntries(LootEntryType type, uint id)
		{
			var entries = LootEntries[(uint)type];
			var list = entries.Get(id);
			return list;
		}

		#region Loot Generation
		/// <summary>
		/// Creates a new Loot object and returns it or null, if there is nothing to be looted.
		/// </summary>
		/// <typeparam name="T"><see cref="ObjectLoot"/> or <see cref="NPCLoot"/></typeparam>
		/// <param name="lootable"></param>
		/// <param name="initialLooter"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static T CreateLoot<T>(ILootable lootable, Character initialLooter, LootEntryType type, bool heroic, MapId mapid)
			where T : Loot, new()
		{
			var looters = FindLooters(lootable, initialLooter);

			var items = GenerateItemLoot(lootable.GetLootId(type), type, heroic, looters);
			var money = lootable.LootMoney;
			if (items.Length == 0 && money == 0)
			{
				if (lootable is GameObject)
				{
					money = 1;
				}
			}

			if (items.Length > 0 || money > 0)
			{
				var loot = new T { Lootable = lootable, Money = money, Items = items };
				loot.Initialize(initialLooter, looters, mapid);
				return loot;
			}
			else
			{
				//var loot = new T { Lootable = lootable, Money = 1, Items = LootItem.EmptyArray };
				//loot.Initialize(initialLooter, looters);
				//return loot;
				return null;
			}
		}

		/// <summary>
		/// Generates loot for Items and GOs. 
		/// </summary>
		/// <param name="lootable">The Object or Unit that is being looted</param>
		/// <returns>The object's loot or null if there is nothing to get or the given Character can't access the loot.</returns>
		public static ObjectLoot CreateAndSendObjectLoot(ILootable lootable, Character initialLooter,
			LootEntryType type, bool heroic)
		{
			var oldLoot = initialLooter.LooterEntry.Loot;
			if (oldLoot != null)
			{
				oldLoot.ForceDispose();
			}
			var looters = FindLooters(lootable, initialLooter);

			var loot = CreateLoot<ObjectLoot>(lootable, initialLooter, type, heroic, 0); // TODO: pass mapid
			if (loot != null)
			{
				initialLooter.LooterEntry.Loot = loot;
				loot.Initialize(initialLooter, looters, 0); // TODO: pass mapid
				LootHandler.SendLootResponse(initialLooter, loot);
			}
			else
			{
				//lootable.OnFinishedLooting();
				// empty Item -> Don't do anything
			}
			return loot;
		}

		/// <summary>
		/// Generates normal loot (usually for dead mob-corpses). 
		/// Returns null, if the loot is empty.
		/// </summary>
		/// <param name="lootable">The Object or Unit that is being looted</param>
		public static Loot GetOrCreateLoot(ILootable lootable, Character triggerChar, LootEntryType type, bool heroic)
		{
			var loot = lootable.Loot;
			if (loot != null)
			{
				// apparently mob got killed a 2nd time
				if (loot.IsMoneyLooted && loot.RemainingCount == 0)
				{
					// already looted empty
					return null;
				}
				loot.Looters.Clear();
			}
			else
			{
				lootable.Loot = loot = CreateLoot<NPCLoot>(lootable, triggerChar, type, heroic, 0); // TODO: pass mapid
			}

			return loot;
		}

		/// <summary>
		/// Returns all Items that can be looted off the given lootable
		/// </summary>
		public static LootItem[] GenerateItemLoot(uint lootId, LootEntryType type, bool heroic, IList<LooterEntry> looters)
		{
#if DEBUG
			if (!ItemMgr.Loaded)
			{
				return LootItem.EmptyArray;
			}
#endif
			var entries = GetEntries(type, lootId);
			if (entries == null)
			{
				return LootItem.EmptyArray;
			}

			var items = new LootItem[Math.Min(MaxLootCount, entries.Count)];
			//var i = max;
			var i = 0;
			for (var j = 0; j < entries.Count; j++)
			{
				var entry = entries[j];
				var chance = entry.DropChance * LootItemDropFactor;
				if ((100 * Utility.RandomFloat()) < chance)
				{
					var template = entry.ItemTemplate;
					if (template == null)
					{
						// weird
						continue;
					}

					if (template.CollectQuests != null)
					{
						var drop = false;
						// only drop quest item if one of the Looters needs it
						foreach (var looter in looters)
						{
							if (looter.Owner != null &&
								looter.Owner.QuestLog.RequiresItem(template.ItemId))
							{
								drop = true;
								break;
							}
						}

						if (!drop)
						{
							continue;
						}
					}
					else if (template.StartQuest != null)
					{
						// TODO: Start quest
					}

					items[i] = new LootItem(template,
						Utility.Random(entry.MinAmount, entry.MaxAmount),
						(uint)i,
						template.RandomPropertiesId);

					if (i == MaxLootCount - 1)
					{
						break;
					}
					i++;
				}
			}

			if (i == 0)
			{
				return LootItem.EmptyArray;
			}

			Array.Resize(ref items, i);
			return items;
		}
		#endregion

		public static IList<LooterEntry> FindLooters(ILootable lootable, Character initialLooter)
		{
			var looters = new List<LooterEntry>();
			FindLooters(lootable, initialLooter, looters);
			return looters;
		}

		public static void FindLooters(ILootable lootable, Character initialLooter, IList<LooterEntry> looters)
		{
			if (lootable.UseGroupLoot)
			{
				var groupMember = initialLooter.GroupMember;
				if (groupMember != null)
				{
					var group = groupMember.Group;
					var method = group.LootMethod;
					var usesRoundRobin = method == LootMethod.RoundRobin;

					if (usesRoundRobin)
					{
						var member = group.GetNextRoundRobinMember();
						if (member != null)
						{
							looters.Add(member.Character.LooterEntry);
						}
					}
					else
					{
						group.GetNearbyLooters(lootable, initialLooter, looters);
					}
					return;
				}
			}

			looters.Add(initialLooter.LooterEntry);
		}

		/// <summary>
		/// Returns whether this lockable can be opened by the given Character
		/// </summary>
		/// <param name="lockable"></param>
		/// <returns></returns>
		public static bool CanOpen(this ILockable lockable, Character chr)
		{
			var lck = lockable.Lock;

			if (lck != null && !lck.IsUnlocked && lck.Keys.Length > 0)
			{
				// chests may only be opened if they are unlocked or we have a key
				// Skill-related opening is handled through spells
				var found = false;
				for (var i = 0; i < lck.Keys.Length; i++)
				{
					var key = lck.Keys[i];
					if (chr.Inventory.KeyRing.Contains(key.KeyId))
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryLoot(this ILockable lockable, Character chr)
		{
			if (CanOpen(lockable, chr))
			{
				// just open it
				LockEntry.Loot(lockable, chr);
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Necessary for UDB's awful loot-relations
	/// </summary>
	public class ResolvedLootItemList : List<LootEntity>
	{
		public byte ResolveStatus;

		public List<LootGroup> Groups;
	}
}