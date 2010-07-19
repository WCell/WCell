using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NLog;
using WCell.Constants.Factions;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;
using WCell.Constants.World;
using WCell.Constants;

namespace WCell.RealmServer.NPCs
{
	public delegate void NPCTypeHandler(NPC npc);
	public delegate void NPCSpawnTypeHandler(SpawnEntry spawnEntry);

	/// <summary>
	/// Static helper and srcCont class for all NPCs and NPC-related information
	/// </summary>
	[GlobalMgr]
	public static class NPCMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		#region Global Variables
		[Variable("NormalCorpseDecayDelay")]
		/// <summary>
		/// Delay before Corpse of normal NPC starts to decay without being looted in seconds (Default: 1 minute)
		/// </summary>
		public static float DecayDelayNormal = 60f;

		[Variable("RareCorpseDecayDelay")]
		/// <summary>
		/// Delay before Corpse of rare NPC starts to decay without being looted in seconds (Default: 5 minutes)
		/// </summary>
		public static float DecayDelayRare = 300f;

		[Variable("EpicCorpseDecayDelay")]
		/// <summary>
		/// Delay before Corpse of epic NPC starts to decay without being looted in seconds (Default: 1 h)
		/// </summary>
		public static float DecayDelayEpic = 3600f;

		public static float DefaultNPCFlySpeed = 16;

		public static float DefaultNPCRunSpeed = 8;

		public static float DefaultNPCWalkSpeed = 2;

		private static FactionId defaultFactionId;

		static int s_lastNPCUID;

		public static int DefaultInteractionDistance = 15;
		public static int DefaultInteractionDistanceSq = DefaultInteractionDistance * DefaultInteractionDistance;

		/// <summary>
		/// This is the default FactionId for all NPCs whose Faction could not be found/does not exist.
		/// </summary>
		public static FactionId DefaultFactionId
		{
			get { return defaultFactionId; }
			set
			{
				defaultFactionId = value;
				var faction = FactionMgr.Get(value);
				if (faction != null)
				{
					DefaultFaction = faction;
				}
			}
		}

		/// <summary>
		/// This is the default faction for all NPCs whose Faction could not be found/does not exist.
		/// </summary>
		public static Faction DefaultFaction
		{
			get;
			internal set;
		}

		/// <summary>
		/// Default amount of NPCs to spawn if at one spawn point, if not specified otherwise
		/// </summary>
		public static uint DefaultSpawnAmount = 1;

		/// <summary>
		/// Default min delay in milliseconds after death of a unit to spawn a new one
		/// </summary>
		public static int DefaultMinRespawnDelay = 3 * 60 * 1000;

		/// <summary>
		/// Default max delay in milliseconds after death of a unit to spawn a new one
		/// </summary>
		public static uint DefaultMaxRespawnDelay = 5 * 60 * 1000;

		/// <summary>
		/// If a mob is in combat and is further away from his home spot than this and there was no combat
		/// for DefaultCombatRetreatDelay, it will start to retreat.
		/// Don't use this in code other than for informational reasons.
		/// </summary>
		public static float DefaultMaxHomeDistanceInCombat
		{
			get { return (float)Math.Sqrt(DefaultMaxHomeDistanceInCombatSq); }
			set { DefaultMaxHomeDistanceInCombatSq = value * value; }
		}

		[NotVariable]
		/// <summary>
		/// <see cref="DefaultMaxHomeDistanceInCombat"/>
		/// </summary>
		public static float DefaultMaxHomeDistanceInCombatSq = 10000f;

		/// <summary>
		/// If a mob is in combat and is further away from his home spot than DefaultMaxHomeDistanceInCombat and there was no combat
		/// for this, it will start to evade (in millis).
		/// </summary>
		public static uint GiveUpCombatDelay = 6000;

		/// <summary>
		/// The delay between the time the NPC finished fighting (everyone killed) and 
		/// evading (in millis).
		/// </summary>
		public static uint CombatEvadeDelay = 2000;
		#endregion

		#region Global Containers & Get Methods
		[NotVariable]
		/// <summary>
		/// All NPCEntries by their Entry-Id
		/// </summary>
		internal static readonly NPCEntry[] Entries = new NPCEntry[(int)NPCId.End];

		/// <summary>
		/// Custom entries are put in a Dictionary, so that the Entries array won't explode
		/// </summary>
		internal static readonly Dictionary<uint, NPCEntry> CustomEntries = new Dictionary<uint, NPCEntry>();

		public static int EntryCount
		{
			get;
			internal set;
		}

		public static IEnumerable<NPCEntry> GetAllEntries()
		{
			return new EntryIterator();
		}

		/// <summary>
		/// All Vehicle Entries 
		/// </summary>
		[NotVariable]
		public static Dictionary<int, VehicleEntry> VehicleEntries = new Dictionary<int, VehicleEntry>();

		/// <summary>
		/// All VehicleSeat Enteries
		/// </summary>
		[NotVariable]
		public static Dictionary<int, VehicleSeatEntry> VehicleSeatEntries = new Dictionary<int, VehicleSeatEntry>();

		/// <summary>
		/// All barber shop style entries.
		/// </summary>
		[NotVariable]
		public static Dictionary<int, BarberShopStyleEntry> BarberShopStyles = new Dictionary<int, BarberShopStyleEntry>();

		[NotVariable]
		/// <summary>
		/// All NPCSpawnEntries by their Id
		/// </summary>
		public static SpawnEntry[] SpawnEntries = new SpawnEntry[40000];

		[NotVariable]
		/// <summary>
		/// All NPCSpawnEntries by MapId
		/// </summary>
		public static List<SpawnEntry>[] SpawnEntriesByMap = new List<SpawnEntry>[600];

		[NotVariable]
		public static NPCEquipmentEntry[] EquipmentEntries = new NPCEquipmentEntry[600];

		[NotVariable]
		public static Dictionary<int, CreatureFamily> CreatureFamilies = new Dictionary<int, CreatureFamily>(1);

		/// <summary>
		/// All existing Mounts by their corresponding EntryId
		/// </summary>
		public static readonly Dictionary<MountId, NPCEntry> Mounts = new Dictionary<MountId, NPCEntry>(100);

		[NotVariable]
		public static uint[] BankBagSlotPrices;

		[NotVariable]
		internal static Spell[][] PetSpells;

		public static NPCEntry GetEntry(uint id)
		{
			if (id >= Entries.Length)
			{
				NPCEntry entry;
				CustomEntries.TryGetValue(id, out entry);
				return entry;
			}
			return Entries[id];
		}

		public static NPCEntry GetEntry(NPCId id)
		{
			if (id >= (NPCId)Entries.Length)
			{
				NPCEntry entry;
				CustomEntries.TryGetValue((uint)id, out entry);
				return entry;
			}
			return Entries[(uint)id];
		}

		/// <summary>
		/// Creates a new custom NPCEntry with the given Id.
		/// The id must be hardcoded, so the client will always recognize it in its cache.
		/// </summary>
		public static void AddEntry<E>(uint id, E entry)
			where E : NPCEntry, new()
		{
			if (id < (uint)NPCId.End)
			{
				throw new ArgumentException("Cannot create an NPCEntry with id < NPCId.End (" + (int)NPCId.End + ")");
			}
			entry.Id = id;
			CustomEntries.Add(id, entry);
			entry.FinalizeDataHolder();
		}

		/// <summary>
		/// Creates a new custom NPCEntry with the given Id.
		/// The id must be hardcoded, so the client will always recognize it in its cache.
		/// </summary>
		public static void AddEntry(uint id, NPCEntry entry)
		{
			if (id < (uint)NPCId.End)
			{
				throw new ArgumentException("Cannot create an NPCEntry with id < NPCId.End (" + (int)NPCId.End + ")");
			}
			entry.Id = id;
			CustomEntries.Add(id, entry);
			entry.FinalizeDataHolder();
		}

		public static SpawnEntry GetSpawnEntry(uint id)
		{
			if (id >= SpawnEntries.Length)
			{
				return null;
			}
			return SpawnEntries[id];
		}

		public static List<SpawnEntry> GetSpawnEntries(MapId map)
		{
			return SpawnEntriesByMap[(int)map];
		}

		public static NPCEquipmentEntry GetEquipment(uint equipId)
		{
			return EquipmentEntries.Get(equipId);
		}

		internal static uint GenerateUniqueLowId()
		{
			return (uint)Interlocked.Increment(ref s_lastNPCUID);
		}

		internal static void GenerateId(NPC npc)
		{
			GenerateId(npc, HighId.Unit);
		}

		internal static void GenerateId(NPC npc, HighId highId)
		{
			npc.EntityId = new EntityId(GenerateUniqueLowId(), npc.EntryId, highId);
		}

		#endregion

		#region Fixing
		public static void Apply(this Action<NPCEntry> cb, params NPCId[] ids)
		{
			foreach (var id in ids)
			{
				var entry = GetEntry(id);
				if (entry != null)
				{
					cb(entry);
				}
			}
		}
		#endregion

		#region Type Handlers
		/// <summary>
		/// NPCTypeHandlers indexed by set bits of NPCFlags.
		/// </summary>
		public static readonly NPCTypeHandler[] NPCTypeHandlers = new NPCTypeHandler[32];

		/// <summary>
		/// NPCSpawnTypeHandlers indexed by set bits of NPCFlags.
		/// </summary>
		public static readonly NPCSpawnTypeHandler[] NPCSpawnTypeHandlers = new NPCSpawnTypeHandler[32];

		/// <summary>
		/// Returns all NPCTypeHandlers for the given NPCPrototype
		/// </summary>
		public static NPCTypeHandler[] GetNPCTypeHandlers(NPCEntry entry)
		{
			var handlers = new NPCTypeHandler[entry.SetFlagIndices.Length];

			for (var i = 0; i < handlers.Length; i++)
			{
				handlers[i] = NPCTypeHandlers[entry.SetFlagIndices[i]];
			}

			return handlers;
		}

		/// <summary>
		/// Calls all NPCTypeHandlers of the given spawnEntry.
		/// </summary>
		internal static void CallNPCTypeHandlers(NPC npc)
		{
			foreach (var handler in npc.Entry.InstanceTypeHandlers)
			{
				handler(npc);
			}
		}

		/// <summary>
		/// Returns all NPCSpawnTypeHandlers for the given NPCPrototype
		/// </summary>
		internal static NPCSpawnTypeHandler[] GetNPCSpawnTypeHandlers(NPCEntry entry)
		{
			var handlers = new NPCSpawnTypeHandler[entry.SetFlagIndices.Length];

			for (int i = 0; i < handlers.Length; i++)
			{
				handlers[i] = NPCSpawnTypeHandlers[entry.SetFlagIndices[i]];
			}

			return handlers;
		}
		#endregion

		#region Initializing and Loading
		[Initialization(InitializationPass.Fourth, "Initialize NPCs")]
		public static void Initialize()
		{
			InitDefault();

#if !DEV
			LoadNPCDefs();
#endif
		}

		public static void ForceInitialize()
		{
			if (!Loaded)
			{
				InitDefault();
				LoadNPCDefs();
			}
		}

		public static void InitDefault()
		{
			var npcSpells = new MappedDBCReader<Spell[], DBCCreatureSpellConverter>(
				RealmServerConfiguration.GetDBCFile("CreatureSpellData.dbc")).Entries;

			PetSpells = new Spell[10000][];
			foreach (var pair in npcSpells)
			{
				ArrayUtil.Set(ref PetSpells, (uint)pair.Key, pair.Value);
			}
			ArrayUtil.Prune(ref PetSpells);

			CreatureFamilies = new MappedDBCReader<CreatureFamily, DBCCreatureFamilyConverter>(
				RealmServerConfiguration.GetDBCFile("CreatureFamily.dbc")).Entries;

			BankBagSlotPrices = new ListDBCReader<uint, DBCBankBagSlotConverter>(
				RealmServerConfiguration.GetDBCFile("BankBagSlotPrices.dbc")).EntryList.ToArray();

			DefaultFaction = FactionMgr.ById[(uint)FactionId.Creature];

			VehicleSeatEntries = new MappedDBCReader<VehicleSeatEntry, DBCVehicleSeatConverter>(
				RealmServerConfiguration.GetDBCFile("VehicleSeat.dbc")).Entries;

			VehicleEntries = new MappedDBCReader<VehicleEntry, DBCVehicleConverter>(
				RealmServerConfiguration.GetDBCFile("Vehicle.dbc")).Entries;

			BarberShopStyles = new MappedDBCReader<BarberShopStyleEntry, BarberShopStyleConverter>(
				RealmServerConfiguration.GetDBCFile(WCellDef.DBC_BARBERSHOPSTYLE)).Entries;

			InitTypeHandlers();
		}

		/// <summary>
		/// Initializes NPCTypeHandlers
		/// </summary>
		static void InitTypeHandlers()
		{
			NPCTypeHandlers[(int)NPCEntryType.Vendor] = OnNewVendor;
			NPCTypeHandlers[(int)NPCEntryType.Trainer] = OnNewTrainer;
			NPCTypeHandlers[(int)NPCEntryType.InnKeeper] = OnNewInnKeeper;
			NPCTypeHandlers[(int)NPCEntryType.Auctioneer] = OnNewAuctioneer;
		}

		public static bool Loaded
		{
			get { return entriesLoaded && spawnsLoaded; }
		}

		public static bool SpawnsLoaded
		{
			get { return spawnsLoaded; }
			private set
			{
				spawnsLoaded = value;
				CheckLoaded();
			}
		}

		public static bool EntriesLoaded
		{
			get { return entriesLoaded; }
			private set
			{
				entriesLoaded = value;
				CheckLoaded();
			}
		}

		private static void CheckLoaded()
		{
			if (Loaded)
			{
				RealmServer.InitMgr.SignalGlobalMgrReady(typeof(NPCMgr));
			}
		}

		private static bool entriesLoaded, spawnsLoaded;

		public static void LoadNPCDefs()
		{
			LoadEntries();
			LoadSpawns();
		}

		public static void LoadEntries()
		{
			if (entriesLoaded)
			{
				return;
			}
			FactionMgr.Initialize();

			ContentHandler.Load<NPCEquipmentEntry>();
			ContentHandler.Load<NPCEntry>();

			EntriesLoaded = true;

			//foreach (var entry in Entries)
			//{
			//    if (entry != null && entry.Template == null)
			//    {
			//        ContentHandler.OnInvalidData("NPCEntry had no corresponding NPCTemplate: " + entry.NPCId);
			//    }
			//}

			LoadTrainers();

			GossipMgr.EnsureInitialized();

			// mount-entries
			//foreach (var spell in SpellHandler.ById)
			//{
			//    if (spell != null && spell.IsMount)
			//    {
			//        var id = (MountId)spell.Effects[0].MiscValue;
			//        if ((int)id >= Entries.Length)
			//        {
			//            log.Warn("Invalid Mount Id: " + id);
			//            spell.Effects[0].EffectType = SpellEffectType.Dummy;		// reach-around fix for the time being
			//            continue;
			//        }
			//        var entry = Entries[(int)id];
			//        if (entry != null)
			//        {
			//            Mounts[id] = entry;
			//        }
			//    }
			//}
		}

		public static void LoadSpawns()
		{
			OnlyLoadSpawns();
			LoadWaypoints();
		}

		public static void OnlyLoadSpawns()
		{
			if (spawnsLoaded)
			{
				return;
			}
			ContentHandler.Load<SpawnEntry>();
			SpawnsLoaded = true;
		}

		public static void LoadWaypoints()
		{
			ContentHandler.Load<WaypointEntry>();
		}

		[Initialization]
		[DependentInitialization(typeof(ItemMgr))]
		[DependentInitialization(typeof(NPCMgr))]
		public static void EnsureNPCItemRelations()
		{
			LoadVendors();
			EnsureNPCLootRelations();
		}

		[Initialization]
		[DependentInitialization(typeof(LootMgr))]
		[DependentInitialization(typeof(ItemMgr))]
		[DependentInitialization(typeof(NPCMgr))]
		public static void EnsureNPCLootRelations()
		{
			var loots = LootMgr.GetEntries(LootEntryType.Skinning);
			foreach (var entry in GetAllEntries())
			{
				if (entry != null && entry.SkinLootId != 0)
				{
					//entry.SkinningLoot = loots.Get(entry.Template.SkinLootId);
					entry.SkinningLoot = loots.Get(entry.SkinLootId);
				}
			}
		}

		#endregion

		public static CreatureFamily GetFamily(CreatureFamilyId id)
		{
			CreatureFamily family;
			CreatureFamilies.TryGetValue((int)id, out family);
			return family;
		}

		public static Spell[] GetSpells(NPCId id)
		{
			return PetSpells.Get((uint)id);
		}

		public static VehicleSeatEntry GetVehicleSeatEntry(uint id)
		{
			VehicleSeatEntry entry;
			VehicleSeatEntries.TryGetValue((int)id, out entry);
			return entry;
		}

		#region Spawning

		#endregion

		#region Trainers
		private static void LoadTrainers()
		{
			ContentHandler.Load<TrainerSpellEntry>();
		}

		static void OnNewTrainer(NPC npc)
		{
			npc.NPCFlags &= ~NPCFlags.ClassTrainer;
			npc.NPCFlags |= NPCFlags.ProfessionTrainer | NPCFlags.UnkTrainer;
			npc.TrainerEntry = npc.Entry.TrainerEntry;
		}

		public static void TalkToTrainer(this NPC trainer, Character chr)
		{
			if (!trainer.CheckTrainerCanTrain(chr))
				return;

			chr.OnInteract(trainer);
			trainer.SendTrainerList(chr, trainer.TrainerEntry.Spells.Values, trainer.TrainerEntry.Message);
		}

		public static bool CanLearn(this Character chr, TrainerSpellEntry trainerSpell)
		{
			if (trainerSpell.Spell == null)
				return false;

			if (trainerSpell.GetTrainerSpellState(chr) != TrainerSpellState.Available)
				return false;
			return true;
		}

		public static void BuySpell(this NPC trainer, Character chr, SpellId spellEntryId)
		{
			if (!trainer.CheckTrainerCanTrain(chr))
				return;

			var trainerSpell = trainer.TrainerEntry.GetSpellEntry(spellEntryId);
			if (trainerSpell == null)
				return;

			if (!trainer.CheckBuySpellConditions(chr, trainerSpell))
				return;

			// Charge for the spell
			chr.Money -= trainerSpell.GetDiscountedCost(chr, trainer);

			// Send a success packet to the client.
			NPCHandler.SendTrainerBuySucceeded(chr.Client, trainer, trainerSpell);

			// spell visual (Silence effect)
			SpellHandler.SendVisual(trainer, 179);

			// Teach the player the new spell
			//chr.Spells.Replace(trainerSpell.DeleteSpell, trainerSpell.Spell);
			if (trainerSpell.Spell.IsTeachSpell)
			{
				trainer.SpellCast.Trigger(trainerSpell.Spell, chr);
			}
			else
			{
				if (chr.PowerType == PowerType.Mana || trainerSpell.Spell.PreviousRank == null)
				{
					// spell casters get all ranks of a spell
					chr.Spells.AddSpell(trainerSpell.Spell);
					trainer.TalkToTrainer(chr);
				}
				else
				{
					// only add the highest rank
					chr.Spells.Replace(trainerSpell.Spell.PreviousRank, trainerSpell.Spell);
				}
			}
		}

		private static bool CheckBuySpellConditions(this NPC trainer, Character curChar, TrainerSpellEntry trainerSpell)
		{
			if (curChar.CanLearn(trainerSpell))
			{
				return curChar.Money >= trainerSpell.GetDiscountedCost(curChar, trainer);
			}
			return false;
		}

		private static bool CheckTrainerCanTrain(this NPC trainer, Character curChar)
		{
			if (!trainer.IsTrainer)
				return false;

			if (!trainer.CanInteractWith(curChar))
				return false;

			if (!trainer.CanTrain(curChar))
				return false;

			// Remove Auras for vendor interaction
			curChar.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

			return true;
		}

		#endregion

		#region Vendors
		public static Dictionary<int, ItemExtendedCostEntry> ItemExtendedCostEntries;
		public static Dictionary<uint, List<VendorItemEntry>> VendorLists = new Dictionary<uint, List<VendorItemEntry>>(5000);

		static void LoadVendors()
		{
			LoadItemExtendedCostEntries();
			ContentHandler.Load<VendorItemEntry>();
		}

		static void LoadItemExtendedCostEntries()
		{
			var reader = new MappedDBCReader<ItemExtendedCostEntry, DBCItemExtendedCostConverter>(
				RealmServerConfiguration.GetDBCFile("ItemExtendedCost.dbc"));

			ItemExtendedCostEntries = reader.Entries;
			ItemExtendedCostEntries.Add(0, ItemExtendedCostEntry.NullEntry);

		}

		internal static List<VendorItemEntry> GetOrCreateVendorList(NPCId npcId)
		{
			return GetOrCreateVendorList((uint) npcId);
		}

		internal static List<VendorItemEntry> GetOrCreateVendorList(uint npcId)
		{
			List<VendorItemEntry> list;
			if (!VendorLists.TryGetValue(npcId, out list))
			{
				VendorLists.Add(npcId, list = new List<VendorItemEntry>());
			}
			return list;
		}

		static void OnNewVendor(NPC npc)
		{
			npc.VendorEntry = new VendorEntry(npc, npc.Entry.VendorItems);
		}

		#endregion

		#region Misc NPCs
		static void OnNewInnKeeper(NPC npc)
		{
			// TODO: Read bind points

			var point = new NamedWorldZoneLocation
			{
				Position = npc.Position,
				RegionId = npc.Region.Id
			};

			if (npc.Zone != null)
			{
				point.ZoneId = npc.Zone.Id;
			}

			npc.BindPoint = point;
		}

		static void OnNewAuctioneer(NPC npc)
		{
			npc.AuctioneerEntry = new AuctioneerEntry(npc);
		}

		public static void TalkToPetitioner(this NPC petitioner, Character chr)
		{
			if (!petitioner.CheckTrainerCanTrain(chr))
				return;

			chr.OnInteract(petitioner);
			petitioner.SendPetitionList(chr);
		}
		#endregion

		#region Utilities
		#endregion

		public static SpawnPoint SpawnClosestSpawnEntry(IWorldLocation pos)
		{
			var entry = GetClosesSpawnEntry(pos);
			if (entry != null)
			{
				return pos.Region.AddSpawn(entry);
			}
			return null;
		}

		public static SpawnEntry GetClosesSpawnEntry(IWorldLocation pos)
		{
			SpawnEntry closest = null;
			var distanceSq = Single.MaxValue;
			foreach (var spawn in SpawnEntriesByMap[(int)pos.RegionId])
			{
				if (spawn != null)
				{
					var distSq = pos.Position.GetDistanceSquared(spawn.Position);
					if (distSq < distanceSq)
					{
						distanceSq = distSq;
						closest = spawn;
					}
				}
			}
			return closest;
		}

		/// <summary>
		/// Checks which nodes are currently activated by the player and sends the results to the client.
		/// </summary>
		public static void TalkToFM(this NPC taxiVendor, Character chr)
		{
			if (!taxiVendor.CanInteractWith(chr))
				return;

			// Get the taxi node associated with this Taxi Vendor
			var curNode = taxiVendor.VendorTaxiNode;
			if (curNode == null)
				return;

			// If this taxi node is as yet unknown to the player, activate it for them.
			var nodeMask = chr.TaxiNodes;
			if (!nodeMask.IsActive(curNode))
			{
				if (chr.GodMode)
				{
					chr.ActivateAllTaxiNodes();
				}
				else
				{
					nodeMask.Activate(curNode);
					TaxiHandler.SendTaxiPathActivated(chr.Client);
					TaxiHandler.SendTaxiPathUpdate(chr.Client, taxiVendor.EntityId, true);

					// apparently the blizz-like activity is to not send the taxi list until the next
					// vendor interaction. So we'll ditch this taco stand.
					return;
				}
			}

			chr.OnInteract(taxiVendor);

			// The character has previously activated this TaxiNode, send the known nodes list to the client
			TaxiHandler.ShowTaxiList(chr, taxiVendor, curNode);
		}

		#region Iterator
		internal class EntryIterator : IEnumerable<NPCEntry>
		{
			public IEnumerator<NPCEntry> GetEnumerator()
			{
				return new EntryEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		class EntryEnumerator : IEnumerator<NPCEntry>
		{
			private IEnumerator currentIterator;
			private bool custom;

			public EntryEnumerator()
			{
				Reset();
			}

			public bool MoveNext()
			{
				if (!custom)
				{
					while (currentIterator.MoveNext())
					{
						if (currentIterator.Current != null)
						{
							return true;
						}
					}

					currentIterator = CustomEntries.Values.GetEnumerator();
					custom = true;
				}
				return currentIterator.MoveNext();
			}

			public void Reset()
			{
				currentIterator = Entries.GetEnumerator();
			}

			public void Dispose()
			{
				if (currentIterator is IEnumerator<NPCEntry>)
				{
					((IEnumerator<NPCEntry>)currentIterator).Dispose();
				}
			}

			public NPCEntry Current
			{
				get { return (NPCEntry)currentIterator.Current; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}
		#endregion
	}
}