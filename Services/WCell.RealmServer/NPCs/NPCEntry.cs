using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using System;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.AI;
using WCell.RealmServer.Items;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;
using WCell.Constants.World;
using WCell.Constants.Updates;
using WCell.Util.NLog;

namespace WCell.RealmServer.NPCs
{
	public delegate NPC NPCCreator(NPCEntry entry);

	/// <summary>
	/// NPC Entry
	/// </summary>
	[DataHolder]
	public partial class NPCEntry : IQuestHolderEntry, INPCDataHolder
	{
		public uint Id
		{
			get;
			set;
		}

		[Persistent((int)ClientLocale.End)]
		public string[] Names = new string[(int)ClientLocale.End];

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

		[Persistent((int)ClientLocale.End)]
		public string[] Titles = new string[(int)ClientLocale.End];

		[NotPersistent]
		public string DefaultTitle
		{
			get { return Titles.LocalizeWithDefaultLocale(); }
			set
			{
				if (Titles == null)
				{
					Titles = new string[(int)ClientLocale.End];
				}
				Titles[(int)RealmServerConfiguration.DefaultLocale] = value;
			}
		}

		public string InfoString = "";

		public CreatureType Type;

		public CreatureFamilyId FamilyId;

		public CreatureRank Rank;

		public uint SpellGroupId;

		/// <summary>
		/// Whether a new NPC should be completely idle (not react to anything that happens)
		/// </summary>
		public bool IsIdle;

		[Persistent(4)]
		public uint[] DisplayIds = new uint[4];

		public float HpModifier;

		public float ManaModifier;

		public bool IsLeader;

		/// <summary>
		/// 
		/// </summary>
		public FactionTemplateId HordeFactionId, AllianceFactionId;

		public int MaxLevel;

		public int MinLevel;

		public uint MinHealth;

		public uint MaxHealth;

		public int MinMana;

		public int MaxMana;

		public float Scale;

		public NPCEntryFlags EntryFlags;

		public NPCFlags NPCFlags;

		public UnitFlags UnitFlags;

		public UnitDynamicFlags DynamicFlags;

		public int AttackTime;

		public int RangedAttackTime;

		public int OffhandAttackTime;

		public int AttackPower;

		public int RangedAttackPower;

		// public int OffhandAttackPower;

		public DamageSchool DamageSchool;

		public float MinDamage;

		public float MaxDamage;

		public float RangedMinDamage;

		public float RangedMaxDamage;

		public float OffhandMinDamage;

		public float OffhandMaxDamage;

		public uint EquipmentId;

		public bool IsBoss;

		public uint MoneyDrop;

		public InvisType InvisibilityType;

		public UnitExtraFlags ExtraFlags;

		public MovementType MovementType;

		/// <summary>
		/// The factor to be applied to the default speed for this kind of NPC
		/// </summary>
		public float SpeedFactor = 1;

		public float WalkSpeed;

		public float RunSpeed;

		public float FlySpeed;

		public uint LootId;

		public uint SkinLootId;

		public uint PickPocketLootId;

		public InhabitType InhabitType = InhabitType.Anywhere;

		public bool Regenerates;

		public int RespawnMod;

		public int ArmorMod;

		public int HealthMod;

		public UpdateFlags ExtraA9Flags;

		public ClassId ClassId;

		public RaceId RaceId;

		// addon data
		public NPCAddonData AddonData
		{
			get;
			set;
		}

		private GossipMenu m_DefaultGossip;

		[NotPersistent]
		public NPCEntry Entry
		{
			get { return this; }
		}

		[NotPersistent]
		public bool GeneratesXp;

		[NotPersistent]
		public NPCEquipmentEntry Equipment;

		[NotPersistent]
		public UnitModelInfo[] ModelInfos;

		[NotPersistent]
		public GossipMenu DefaultGossip
		{
			get { return m_DefaultGossip; }
			set
			{
				m_DefaultGossip = value;
			}
		}

		/// <summary>
		/// Ids of quests that this NPC is responsible for
		/// </summary>
		[Persistent(4)]
		public uint[] QuestIds = new uint[4];

		/// <summary>
		/// A set of default Spells for this NPC
		/// </summary>
		[Persistent(4)]
		public SpellId[] FixedSpells = new SpellId[4];

		/// <summary>
		/// Spell to be casted when a Character talks to the NPC
		/// </summary>
		[NotPersistent]
		public Spell InteractionSpell;

		[Persistent(ItemConstants.MaxResCount)]
		public int[] Resistances = new int[ItemConstants.MaxResCount];

		public void SetResistance(DamageSchool school, int value)
		{
			Resistances[(uint)school] = value;
		}

		public int GetResistance(DamageSchool school)
		{
			if (Resistances == null)
			{
				return 0;
			}
			return Resistances.Get((uint)school);
		}

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPC is created
		/// </summary>
		public NPCTypeHandler[] InstanceTypeHandlers;

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPCSpawnEntry is created
		/// </summary>
		public NPCSpawnTypeHandler[] SpawnTypeHandlers;

		[NotPersistent]
		public Faction HordeFaction, AllianceFaction;

		public Faction Faction { get { return HordeFaction; } }

		[NotPersistent]
		public NPCId NPCId
		{
			get;
			private set;
		}

		[NotPersistent]
		/// <summary>
		/// All bits of <see cref="Flags"/> that are set
		/// </summary>
		public uint[] SetFlagIndices;

		public uint GetRandomHealth()
		{
			return Utility.Random(MinHealth, MaxHealth);
		}

		public int GetRandomMana()
		{
			return Utility.Random(MinMana, MaxMana);
		}

		public int GetRandomLevel()
		{
			return Utility.Random(MinLevel, MaxLevel);
		}

		#region Weapons
		/// <summary>
		/// Creates the default MainHand Weapon for all Spawns of this SpawnPoint
		/// </summary>
		public IWeapon CreateMainHandWeapon()
		{
			if (Type == CreatureType.None || Type == CreatureType.Totem || Type == CreatureType.NotSpecified)
			{
				// these kinds of NPCs do not attack ever
				return GenericWeapon.Peace;
			}

			return new GenericWeapon(InventorySlotTypeMask.WeaponMainHand, MinDamage, MaxDamage, AttackTime);
		}

		/// <summary>
		/// Creates the default OffHand Weapon for all Spawns of this SpawnPoint (might be null)
		/// </summary>
		public IWeapon CreateOffhandWeapon()
		{
			if (OffhandAttackTime > 0 && OffhandMaxDamage > 0 && OffhandMinDamage > 0)
			{
				return new GenericWeapon(InventorySlotTypeMask.WeaponOffHand, OffhandMinDamage, OffhandMaxDamage, OffhandAttackTime);
			}
			return null;
		}

		/// <summary>
		/// Creates the default Ranged Weapon for all Spawns of this SpawnPoint (might be null)
		/// </summary>
		public IWeapon CreateRangedWeapon()
		{
			if (RangedAttackTime > 0)
			{
				return new GenericWeapon(InventorySlotTypeMask.WeaponRanged, RangedMinDamage, RangedMaxDamage, RangedAttackTime);
			}
			return null;
		}
		#endregion

		private uint m_VehicleId;

		public uint VehicleId
		{
			get { return m_VehicleId; }
			set
			{
				m_VehicleId = value;
				if (value > 0)
				{
					NPCMgr.VehicleEntries.TryGetValue((int)VehicleId, out VehicleEntry);

					if (IsVehicle && (NPCCreator == null || NPCCreator == DefaultCreator))
					{
						// set Vehicle creator by default
						NPCCreator = entry => new Vehicle();
					}
				}
			}
		}

		public float VehicleAimAdjustment;

		public float HoverHeight;

		public IWorldLocation[] GetInWorldTemplates()
		{
			return SpawnEntries.ToArray();
		}

		[NotPersistent]
		public CreatureFamily Family;

		[NotPersistent]
		public PetLevelStatInfo[] PetLevelStatInfos;

		/// <summary>
		/// 
		/// </summary>
		public PetLevelStatInfo GetPetLevelStatInfo(int level)
		{
			if (PetLevelStatInfos == null)
			{
				//LogManager.GetCurrentClassLogger().Warn("Tried to get PetLevelStatInfo for NPCEntry {0} (Level {1}), which has no PetLevelStatInfos", this, level);
				// info = PetMgr.GetDefaultPetLevelStatInfo(level);
				return null;
			}
			else
			{
				var info = PetLevelStatInfos.Get(level);
				if (info == null)
				{
					//LogManager.GetCurrentClassLogger().Warn("Tried to get PetLevelStatInfo for NPCEntry {0} (Level {1}), which has no PetLevelStatInfos", this, level);
					//info = PetMgr.GetDefaultPetLevelStatInfo(level);
				}
				return info;
			}
		}

		#region Spells
		/// <summary>
		/// Usable Spells to be casted by Mobs of this Type
		/// </summary>
		[NotPersistent]
		public Dictionary<uint, Spell> Spells;

		[NotPersistent]
		public SpellTriggerInfo SpellTriggerInfo;

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(SpellId spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell != null)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(uint spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell != null)
			{
				AddSpell(spell);
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpells(params SpellId[] ids)
		{
			foreach (var id in ids)
			{
				var spell = SpellHandler.Get(id);
				if (spell != null)
				{
					AddSpell(spell);
				}
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpells(params uint[] ids)
		{
			foreach (var id in ids)
			{
				var spell = SpellHandler.Get(id);
				if (spell != null)
				{
					AddSpell(spell);
				}
			}
		}

		/// <summary>
		/// Adds a Spell that will be used by all NPCs of this Entry
		/// </summary>
		public void AddSpell(Spell spell)
		{
			if (Spells == null)
			{
				Spells = new Dictionary<uint, Spell>(5);
			}
			OnSpellAdded(spell);
			Spells[spell.Id] = spell;
		}

		private void OnSpellAdded(Spell spell)
		{
		}

		#endregion

		#region Spawns
		[NotPersistent]
		public List<SpawnEntry> SpawnEntries = new List<SpawnEntry>(3);

		public SpawnEntry AddSpawnEntry(MapId map, Vector3 location, int respawnSeconds)
		{
			return AddSpawnEntry(map, location, respawnSeconds, respawnSeconds);
		}

		public SpawnEntry AddSpawnEntry(MapId map, Vector3 location, int minRespawnSeconds, int maxRespawnSeconds)
		{
			return AddSpawnEntry(map, location, minRespawnSeconds, maxRespawnSeconds, true);
		}

		public SpawnEntry AddSpawnEntry(MapId map, Vector3 location, int minRespawnSeconds, int maxRespawnSeconds, bool autoSpawn)
		{
			return AddSpawnEntry(map, location, 1, minRespawnSeconds, maxRespawnSeconds, autoSpawn);
		}

		public SpawnEntry AddSpawnEntry(MapId map, Vector3 location, int amount, int minRespawnSeconds, int maxRespawnSeconds)
		{
			return AddSpawnEntry(map, location, amount, minRespawnSeconds, maxRespawnSeconds, true);
		}

		public SpawnEntry AddSpawnEntry(MapId map, Vector3 location, int amount, int minRespawnSeconds, int maxRespawnSeconds, bool autoSpawn)
		{
			var entry = new SpawnEntry
			{
				EntryId = NPCId,
				Entry = this,
				MaxAmount = amount,
				RespawnSecondsMin = minRespawnSeconds,
				RespawnSecondsMax = maxRespawnSeconds,
				AutoSpawn = autoSpawn,
				RegionId = map
			};

			entry.FinalizeDataHolder();
			return entry;
		}

		/// <summary>
		/// Creates but doesn't add the SpawnEntry
		/// </summary>
		public SpawnEntry CreateSpawnEntry(Vector3 location, int amount, int respawnSeconds)
		{
			return CreateSpawnEntry(location, amount, respawnSeconds, respawnSeconds);
		}

		/// <summary>
		/// Creates but doesn't add the SpawnEntry
		/// </summary>
		public SpawnEntry CreateSpawnEntry(Vector3 location, int amount, int minRespawnSeconds, int maxRespawnSeconds)
		{
			return CreateSpawnEntry(location, amount, minRespawnSeconds, maxRespawnSeconds, true);
		}

		/// <summary>
		/// Creates but doesn't add the SpawnEntry
		/// </summary>
		public SpawnEntry CreateSpawnEntry(Vector3 location, int amount, int minRespawnSeconds, int maxRespawnSeconds, bool autoSpawn)
		{
			var entry = new SpawnEntry
			{
				EntryId = NPCId,
				Entry = this,
				MaxAmount = amount,
				RespawnSecondsMin = minRespawnSeconds,
				RespawnSecondsMax = maxRespawnSeconds,
				AutoSpawn = autoSpawn
			};

			entry.FinalizeDataHolder();
			return entry;
		}

		[NotPersistent]
		public SpawnEntry FirstSpawnEntry
		{
			get { return SpawnEntries.Count > 0 ? SpawnEntries[0] : null; }
		}
		#endregion

		#region Interactable NPCs
		/// <summary>
		/// Trainers
		/// </summary>
		public TrainerEntry TrainerEntry;

		/// <summary>
		/// Vendors
		/// </summary>
		[NotPersistent]
		public List<VendorItemEntry> VendorItems
		{
			get
			{
				List<VendorItemEntry> list;
				NPCMgr.VendorLists.TryGetValue(Id, out list);
				return list;
			}
		}

		/// <summary>
		/// BattleMasters
		/// </summary>
		[NotPersistent]
		public BattlegroundTemplate BattlegroundTemplate;
		#endregion

		#region Quests

		[NotPersistent]
		/// <summary>
		/// The QuestHolderEntry of this NPCEntry, if this is a QuestGiver
		/// </summary>
		public QuestHolderInfo QuestHolderInfo { get; set; }

		#endregion

		[NotPersistent]
		public VehicleEntry VehicleEntry;

		public bool IsVehicle
		{
			get { return VehicleEntry != null; }
		}

		/// <summary>
		/// The default decay delay in seconds.
		/// </summary>
		[NotPersistent]
		public int DefaultDecayDelayMillis;

		[NotPersistent]
		public ResolvedLootItemList SkinningLoot;

		/// <summary>
		/// Whether the spawns from this entry should roam on randomly generated WPs
		/// </summary>
		[NotPersistent]
		public bool MovesRandomly = true;

		public uint NameGossipId;

		public UnitModelInfo GetRandomModel()
		{
			return ModelInfos[Utility.Random(0, ModelInfos.Length - 1)];
		}

		/// <summary>
		/// The default delay before removing the NPC after it died when not looted.
		/// </summary>
		int _DefaultDecayDelayMillis
		{
			get
			{
				if (Rank == CreatureRank.Normal)
				{
					return NPCMgr.DecayDelayNormalMillis;
				}
				if (Rank == CreatureRank.Rare)
				{
					return NPCMgr.DecayDelayRareMillis;
				}
				return NPCMgr.DecayDelayEpicMillis;
			}
		}

		/// <summary>
		/// Is called to initialize the object; usually after a set of other operations have been performed or if
		/// the right time has come and other required steps have been performed.
		/// </summary>
		public void FinalizeDataHolder()
		{
			if (string.IsNullOrEmpty(DefaultName))
			{
				ContentHandler.OnInvalidDBData("NPCEntry has no name: " + this);
				return;
			}

			if (Titles == null)
			{
				Titles = new string[(int)ClientLocale.End];
			}

			if (DefaultTitle == null)
			{
				DefaultTitle = "";
			}

			if (SpellGroupId != 0 && NPCMgr.PetSpells != null)
			{
				var spells = NPCMgr.PetSpells.Get(SpellGroupId) ?? Spell.EmptyArray;
				foreach (var spell in spells)
				{
					AddSpell(spell);
				}
			}

			if (MinMana > MaxMana)
			{
				MaxMana = MinMana;
			}

			if (MaxDamage > 0)
			{
				if (MinDamage < 1)
				{
					MinDamage = 1;
				}
				if (MaxDamage < MinDamage)
				{
					MaxDamage = MinDamage;
				}
			}

			if (RangedMaxDamage > 0 && RangedMinDamage > 0)
			{
				if (RangedMaxDamage < RangedMinDamage)
				{
					RangedMaxDamage = RangedMinDamage;
				}
				if (RangedAttackTime == 0)
				{
					RangedAttackTime = AttackTime;
				}
			}

			MovesRandomly = NPCFlags == NPCFlags.None;

			NPCId = (NPCId)Id;

			DefaultDecayDelayMillis = _DefaultDecayDelayMillis;
			Family = NPCMgr.GetFamily(FamilyId);

			if (Type == CreatureType.NotSpecified)
			{
				IsIdle = true;
			}

			const uint gossipStartId = 200231u; // random fixed Id
			NameGossipId = gossipStartId + Id;
			new GossipEntry(NameGossipId, DefaultName + " (" + Id + ")");		// entry adds itself to GossipMgr

			if (Resistances == null)
			{
				Resistances = new int[ItemConstants.MaxResCount];
			}

			SetFlagIndices = Utility.GetSetIndices((uint)NPCFlags);
			HordeFaction = FactionMgr.Get(HordeFactionId);
			AllianceFaction = FactionMgr.Get(AllianceFactionId);

			if (HordeFaction == null)
			{
				HordeFaction = AllianceFaction;
			}
			else if (AllianceFaction == null)
			{
				AllianceFaction = HordeFaction;
			}

			if (AllianceFaction == null)
			{
				ContentHandler.OnInvalidDBData("NPCEntry has no valid Faction: " + this);
				AllianceFaction = NPCMgr.DefaultFaction;
				HordeFaction = AllianceFaction;
			}

			if (SpeedFactor < 0.01)
			{
				SpeedFactor = 1;
			}

			if (WalkSpeed == 0)
			{
				WalkSpeed = NPCMgr.DefaultNPCWalkSpeed * SpeedFactor;
			}
			if (RunSpeed == 0)
			{
				RunSpeed = NPCMgr.DefaultNPCRunSpeed * SpeedFactor;
			}
			if (FlySpeed == 0)
			{
				FlySpeed = NPCMgr.DefaultNPCFlySpeed * SpeedFactor;
			}

			// Add all default spells
			if (FixedSpells != null)
			{
				foreach (var spell in FixedSpells)
				{
					AddSpell(spell);
				}
			}

			InstanceTypeHandlers = NPCMgr.GetNPCTypeHandlers(this);
			SpawnTypeHandlers = NPCMgr.GetNPCSpawnTypeHandlers(this);

			//ArrayUtil.PruneVals(ref DisplayIds);

			if (EquipmentId != 0)
			{
				Equipment = NPCMgr.GetEquipment(EquipmentId);
			}

			ModelInfos = new UnitModelInfo[DisplayIds.Length];

			GeneratesXp = (Type != CreatureType.Critter && Type != CreatureType.None && !ExtraFlags.HasFlag(UnitExtraFlags.NoXP));

			var x = 0;
			for (var i = 0; i < DisplayIds.Length; i++)
			{
				var did = DisplayIds[i];
				if (did > 0)
				{
					var model = UnitMgr.GetModelInfo(did);
					if (model != null)
					{
						ModelInfos[x++] = model;
					}
				}
			}

			if (x == 0)
			{
				ContentHandler.OnInvalidDBData("NPCEntry has no valid DisplayId: {0} ({1})", this, DisplayIds.ToString(", "));
				return;
			}

			if (AddonData != null)
			{
				AddonData.InitAddonData(this);
			}

			if (x < ModelInfos.Length)
			{
				Array.Resize(ref ModelInfos, x);
			}

			// add to container
			if (Id < NPCMgr.Entries.Length)
			{
				NPCMgr.Entries[Id] = this;
			}
			else
			{
				NPCMgr.CustomEntries[Id] = this;
			}
			++NPCMgr.EntryCount;

			if (BrainCreator == null)
			{
				BrainCreator = DefaultBrainCreator;
			}

			if (NPCCreator == null)
			{
				NPCCreator = DefaultCreator;
			}
		}

		public bool IsVendor
		{
			get { return VendorItems != null; }
		}

		public bool IsTamable
		{
			get { return EntryFlags.HasFlag(NPCEntryFlags.Tamable); }
		}

		public bool IsExoticPet
		{
			get { return EntryFlags.HasFlag(NPCEntryFlags.ExoticCreature); }
		}

		#region Creators
		[NotPersistent]
		public Func<NPC, IBrain> BrainCreator;

		[NotPersistent]
		public NPCCreator NPCCreator;

		public NPC Create()
		{
			return Create((SpawnPoint)null);
		}

		public NPC Create(SpawnPoint spawn)
		{
			var npc = NPCCreator(this);
			npc.SetupNPC(this, spawn);
			return npc;
		}

		public NPC Create(Region rgn, Vector3 pos)
		{
			var npc = NPCCreator(this);
			npc.SetupNPC(this, null);
			rgn.AddObject(npc, pos);
			return npc;
		}

		public NPC Create(IWorldZoneLocation loc)
		{
			var npc = NPCCreator(this);
			npc.SetupNPC(this, null);
			npc.Zone = loc.GetZone();
			loc.Region.AddObject(npc, loc.Position);
			return npc;
		}

		public NPC Create(IWorldLocation loc)
		{
			var npc = NPCCreator(this);
			npc.SetupNPC(this, null);
			loc.Region.AddObject(npc, loc.Position);
			return npc;
		}

		public static NPCCreator DefaultCreator = entry => new NPC();

		public IBrain DefaultBrainCreator(NPC npc)
		{
			return new MobBrain(npc, IsIdle ? BrainState.Idle : BrainState.Roam)
			{
				IsAggressive = true
			};
		}
		#endregion

		#region Events
		internal void NotifyActivated(NPC npc)
		{
			var evt = Activated;
			if (evt != null)
			{
				evt(npc);
			}
		}

		internal void NotifyInteracting(NPC npc, Character chr)
		{
			if (InteractionSpell != null)
			{
				chr.SpellCast.TriggerSelf(InteractionSpell);
			}

			var evt = Interacting;
			if (evt != null)
			{
				evt(chr, npc);
			}
		}

		internal bool NotifyBeforeDeath(NPC npc)
		{
			var evt = BeforeDeath;
			if (evt != null)
			{
				return evt(npc);
			}
			return true;
		}

		internal void NotifyDied(NPC npc)
		{
			var evt = Died;
			if (evt != null)
			{
				evt(npc);
			}
		}

		internal void NotifyLeveledChanged(NPC npc)
		{
			var evt = LevelChanged;
			if (evt != null)
			{
				evt(npc);
			}
		}
		#endregion

		#region Dump

		public void Dump(IndentTextWriter writer)
		{
			writer.WriteLine("{3}{0} (Id: {1}, {2})", DefaultName, Id, NPCId, Rank != 0 ? Rank + " " : "");
			if (!string.IsNullOrEmpty(DefaultTitle))
			{
				writer.WriteLine("Title: " + DefaultTitle);
			}
			if (Type != 0)
			{
				writer.WriteLine("Type: " + Type);
			}
			if (EntryFlags != 0)
			{
				writer.WriteLine("EntryFlags: " + EntryFlags);
			}
			if (Family != null)
			{
				writer.WriteLine("Family: " + Family);
			}
			if (IsLeader)
			{
				writer.WriteLine("Leader");
			}

			writer.WriteLine("DisplayIds: " + DisplayIds.ToString(", "));
			if (TrainerEntry != null)
			{
				writer.WriteLine("Trainer "
					//+ "for {0} {1}",
					//             TrainerEntry.RequiredRace != 0 ? TrainerEntry.RequiredRace.ToString() : "",
					//             TrainerEntry.RequiredClass != 0 ? TrainerEntry.RequiredClass.ToString() : ""
								 );
			}

			WriteFaction(writer);
			writer.WriteLineNotDefault(IsBoss, "Boss");
			writer.WriteLine("Level: {0} - {1}", MinLevel, MaxLevel);
			writer.WriteLine("Health: {0} - {1}", MinHealth, MaxHealth);

			writer.WriteLineNotDefault(MinMana, "Mana: {0} - {1}", MinMana, MaxMana);
			writer.WriteLineNotDefault(NPCFlags, "Flags: " + NPCFlags);
			writer.WriteLineNotDefault(DynamicFlags, "DynamicFlags: " + DynamicFlags);
			writer.WriteLineNotDefault(UnitFlags, "UnitFlags: " + UnitFlags);
			writer.WriteLineNotDefault(ExtraFlags, "ExtraFlags: " + string.Format("0x{0:X}", ExtraFlags));
			writer.WriteLineNotDefault(AttackTime + OffhandAttackTime, "AttackTime: " + AttackTime, "Offhand: " + OffhandAttackTime);
			writer.WriteLineNotDefault(RangedAttackTime, "RangedAttackTime: " + RangedAttackTime);
			writer.WriteLineNotDefault(AttackPower, "AttackPower: " + AttackPower);
			writer.WriteLineNotDefault(RangedAttackPower, "RangedAttackPower: " + RangedAttackPower);
			//writer.WriteLineNotDefault(OffhandAttackPower, "OffhandAttackPower: " + OffhandAttackPower);
			writer.WriteLineNotDefault(MinDamage + MaxDamage, "Damage: {0} - {1}", MinDamage, MaxDamage);
			writer.WriteLineNotDefault(RangedMinDamage + RangedMaxDamage, "RangedDamage: {0} - {1}", RangedMinDamage,
									   RangedMaxDamage);
			writer.WriteLineNotDefault(OffhandMinDamage + OffhandMaxDamage, "OffhandDamage: {0} - {1}", OffhandMinDamage,
									   OffhandMaxDamage);
			var resistances = new List<string>(8);
			for (var i = 0; i < Resistances.Length; i++)
			{
				var res = Resistances[i];
				if (res > 0)
				{
					resistances.Add(string.Format("{0}: {1}", (DamageSchool)i, res));
				}
			}
			if (Scale != 1)
			{
				writer.WriteLine("Scale: " + Scale);
			}

			var cr = GetRandomModel().CombatReach;
			var br = GetRandomModel().BoundingRadius;
			writer.WriteLine("CombatReach: " + cr);
			writer.WriteLine("BoundingRadius: " + br);
			writer.WriteLineNotDefault(resistances.Count, "Resistances: " + resistances.ToString(", "));
			writer.WriteLineNotDefault(MoneyDrop, "MoneyDrop: " + MoneyDrop);
			writer.WriteLineNotDefault(InvisibilityType, "Invisibility: " + InvisibilityType);
			writer.WriteLineNotDefault(MovementType, "MovementType: " + MovementType);
			writer.WriteLineNotDefault(WalkSpeed + RunSpeed + FlySpeed, "Speeds - Walking: {0}, Running: {1}, Flying: {2} ",
									   WalkSpeed, RunSpeed, FlySpeed);
			writer.WriteLineNotDefault(LootId + SkinLootId + PickPocketLootId, "{0}{1}{2}",
									   LootId != 0 ? "Lootable " : "",
									   SkinLootId != 0 ? "Skinnable " : "",
									   PickPocketLootId != 0 ? "Pickpocketable" : "");
			if (AddonData != null)
			{
				writer.WriteLineNotDefault(AddonData.MountModelId, "Mount: " + AddonData.MountModelId);
				writer.WriteLineNotDefault(AddonData.Auras.Count, "Auras: " + AddonData.Auras.ToString(", "));
			}
			var spells = Spells;
			if (spells != null && spells.Count > 0)
			{
				writer.WriteLine("Spells: " + Spells.ToString(", "));
			}
			if (Equipment != null)
			{
				writer.WriteLine("Equipment: {0}", Equipment.ItemIds.Where(id => id != 0).ToString(", "));
			}
			writer.WriteLineNotDefault(ExtraA9Flags, "ExtraA9Flags: " + ExtraA9Flags);
			//if (inclFaction)	
			//{
			//    writer.WriteLineNotDefault(DefaultFactionId, "Faction: " + DefaultFactionId);
			//}
		}

		private void WriteFaction(IndentTextWriter writer)
		{
			writer.WriteLineNotNull(HordeFaction, "HordeFaction: " + HordeFaction);
			writer.WriteLineNotNull(AllianceFaction, "AllianceFaction: " + AllianceFaction);
		}

		#endregion

		public override string ToString()
		{
			return "Entry: " + DefaultName + " (Id: " + Id + ")";
		}

		public static IEnumerable<NPCEntry> GetAllDataHolders()
		{
			return new NPCMgr.EntryIterator();
		}
	}

	[Flags]
	public enum InhabitType
	{
		Ground = 1,
		Water = 2,
		Amphibious = Ground | Water,
		Air = 4,
		Anywhere = Ground | Water | Air
	}
}