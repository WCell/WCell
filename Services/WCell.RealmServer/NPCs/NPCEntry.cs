using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.NPCs
{
    public delegate NPC NPCCreator(NPCEntry entry);

    /// <summary>
    /// NPC Entry
    /// </summary>
    [DataHolder]
    public partial class NPCEntry : ObjectTemplate, INPCDataHolder
    {
        #region Global Variables

        // <summary>
        // This is added to the CombatReach of all Units
        // </summary>
        //public static float BaseAttackReach = 1f;

        /// <summary>
        /// Default base-range in which a mob will aggro (in yards).
        /// Also see <see cref="AggroRangePerLevel"/>
        /// </summary>
        public static float AggroBaseRangeDefault = 20;

        /// <summary>
        /// Amount of yards to add to the <see cref="AggroBaseRangeDefault"/> per level difference.
        /// </summary>
        public static float AggroRangePerLevel = 1;

        /// <summary>
        /// Mobs with a distance >= this will not start aggressive actions
        /// </summary>
        public static float AggroRangeMaxDefault = 45;

        private static float aggroRangeMinDefault = 5;

        /// <summary>
        /// Mobs within this range will *definitely* aggro
        /// </summary>
        public static float AggroRangeMinDefault
        {
            get { return aggroRangeMinDefault; }
            set
            {
                aggroRangeMinDefault = value;
                AggroMinRangeSq = value * value;
            }
        }

        [NotVariable]
        public static float AggroMinRangeSq = aggroRangeMinDefault * aggroRangeMinDefault;

        #endregion Global Variables

        public ClassId ClassId;

        public RaceId RaceId;

        public CreatureType Type;

        public CreatureFamilyId FamilyId;

        [NotPersistent]
        public CreatureFamily Family;

        public CreatureRank Rank;

        public bool IsLeader;

        /// <summary>
        /// Whether a new NPC should be completely idle (not react to anything that happens)
        /// </summary>
        public bool IsIdle;

        /// <summary>
        /// Whether an NPC is a special event trigger like the little
        /// elementals used to cast spells or trigger concerts
        /// </summary>
        [NotPersistent]
        public bool IsEventTrigger
        {
            get;
            private set;
        }

        public uint EquipmentId;

        [NotPersistent]
        public NPCEquipmentEntry Equipment;

        public bool IsBoss;

        public InvisType InvisibilityType;

        public InhabitType InhabitType = InhabitType.Ground;

        public bool Regenerates;

        public uint TrainerTemplateId;

        public uint VendorTemplateId;

        // addon data
        public NPCAddonData AddonData
        {
            get;
            set;
        }

        /// <summary>
        /// Ids of creatures which simulate a death when this entry die
        /// </summary>
        [Persistent(2)]
        public uint[] KillCreditIds = new uint[UnitConstants.MaxKillCredits];

        /// <summary>
        /// Ids of items this NPC drops in relation to quests (sent in a packet)
        /// </summary>
        [Persistent(6)]
        public uint[] QuestItems = new uint[6];

        [NotPersistent]
        public bool GeneratesXp;

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

        public bool IsVendor
        {
            get { return VendorItems != null; }
        }

        [NotPersistent]
        public float AggroBaseRange
        {
            get;
            set;
        }

        #region Entry and substitute Entries

        [NotPersistent]
        public NPCId NPCId
        {
            get;
            private set;
        }

        [NotPersistent]
        public NPCEntry Entry
        {
            get { return this; }
        }

        [Persistent((int)RaidDifficulty.End - 1)]
        public NPCId[] DifficultyOverrideEntryIds = new NPCId[(int)RaidDifficulty.End - 1];

        /// <summary>
        /// Returns the NPCEntry for the given difficulty
        /// </summary>
        public NPCEntry GetEntry(uint difficultyIndex)
        {
            var id = DifficultyOverrideEntryIds.Get(difficultyIndex);
            if (id != 0)
            {
                var entry = NPCMgr.GetEntry(id);
                if (entry != null)
                {
                    return entry;
                }
            }
            return this;
        }

        public MapTemplate GetMapTemplate()
        {
            var spawn = SpawnEntries.FirstOrDefault();
            if (spawn != null)
            {
                return World.GetMapTemplate(spawn.MapId);
            }
            return null;
        }

        #endregion Entry and substitute Entries

        #region Strings

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

        #endregion Strings

        #region Display & Model

        [Persistent(4)]
        public uint[] DisplayIds = new uint[4];

        [NotPersistent]
        public UnitModelInfo[] ModelInfos;

        public UnitModelInfo GetRandomModel()
        {
            return ModelInfos[Utility.Random(0, ModelInfos.Length - 1)];
        }

        #endregion Display & Model

        #region Stats

        public int MaxLevel;

        public int MinLevel;

        public int MinHealth;

        public int MaxHealth;

        public int MinMana;

        public int MaxMana;

        public float ModHealth;

        public float ModMana;

        public DamageSchool DamageSchool;

        public int AttackTime;

        public int RangedAttackTime;

        public int OffhandAttackTime;

        public int AttackPower;

        public int RangedAttackPower;

        // public int OffhandAttackPower;

        public float MinDamage;

        public float MaxDamage;

        public float RangedMinDamage;

        public float RangedMaxDamage;

        public float OffhandMinDamage;

        public float OffhandMaxDamage;

        public int GetRandomLevel()
        {
            return Utility.Random(MinLevel, MaxLevel);
        }

        public void SetLevel(int value)
        {
            MinLevel = value;
            MaxLevel = value;
        }

        public void SetLevel(int minLevel, int maxLevel)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }

        public int GetRandomHealth()
        {
            return (int)(Utility.Random(MinHealth, MaxHealth) * NPCMgr.DefaultNPCHealthFactor + 0.999999f);
        }

        public void SetHealth(int value)
        {
            MinHealth = value;
            MaxHealth = value;
        }

        public void SetHealth(int minHealth, int maxHealth)
        {
            MinHealth = minHealth;
            MaxHealth = maxHealth;
        }

        public int GetRandomMana()
        {
            return Utility.Random(MinMana, MaxMana);
        }

        public void SetMana(int mana)
        {
            MinMana = mana;
            MaxMana = mana;
        }

        public void SetMana(int minMana, int maxMana)
        {
            MinMana = minMana;
            MaxMana = maxMana;
        }

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

        #endregion Stats

        #region Stat Scaling

        public bool HasScalableStats
        {
            get { return PetLevelStatInfos != null; }
        }

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

        #endregion Stat Scaling

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

        #endregion Weapons

        #region Flags

        public NPCEntryFlags EntryFlags;

        public NPCFlags NPCFlags;

        public UnitFlags UnitFlags;

        public UnitDynamicFlags DynamicFlags;

        [NotPersistent]
        public bool IsDead
        {
            get { return DynamicFlags.HasFlag(UnitDynamicFlags.Dead); }
            set { DynamicFlags |= UnitDynamicFlags.Dead; }
        }

        public UnitExtraFlags ExtraFlags;

        [NotPersistent]
        /// <summary>
        /// All bits of <see cref="Flags"/> that are set
        /// </summary>
        public uint[] SetFlagIndices;

        public bool IsTamable
        {
            get { return EntryFlags.HasFlag(NPCEntryFlags.Tamable); }
        }

        public bool IsExoticPet
        {
            get { return EntryFlags.HasFlag(NPCEntryFlags.ExoticCreature); }
        }

        #endregion Flags

        #region Movement & Speed

        //TODO: Rename to something more meaningful
        public AIMotionGenerationType MovementType;

        //public MovementType MovementType;

        /// <summary>
        /// The factor to be applied to the default speed for this kind of NPC
        /// </summary>
        public float SpeedFactor = 1;

        public float WalkSpeed;

        public float RunSpeed;

        public float FlySpeed;

        public uint MovementId;

        #endregion Movement & Speed

        #region Loot

        public uint LootId;

        public uint SkinLootId;

        public uint PickPocketLootId;

        public uint MoneyDrop;

        public override ResolvedLootItemList GetLootEntries()
        {
            return LootMgr.GetEntries(LootEntryType.NPCCorpse, LootId);
        }

        public ResolvedLootItemList GetSkinningLoot()
        {
            return LootMgr.GetEntries(LootEntryType.Skinning, SkinLootId);
        }

        public ResolvedLootItemList GetPickPocketLoot()
        {
            return LootMgr.GetEntries(LootEntryType.PickPocketing, PickPocketLootId);
        }

        #endregion Loot

        #region Factions

        private FactionTemplateId m_HordeFactionId;
        private FactionTemplateId m_AllianceFactionId;

        /// <summary>
        ///
        /// </summary>
        public FactionTemplateId HordeFactionId
        {
            get { return m_HordeFactionId; }
            set
            {
                m_HordeFactionId = value;
                if (HordeFaction != null)
                {
                    HordeFaction = FactionMgr.Get(value);
                }
            }
        }

        public FactionTemplateId AllianceFactionId
        {
            get { return m_AllianceFactionId; }
            set
            {
                m_AllianceFactionId = value;
                if (AllianceFaction != null)
                {
                    AllianceFaction = FactionMgr.Get(value);
                }
            }
        }

        [NotPersistent]
        public Faction HordeFaction
        {
            get;
            private set;
        }

        [NotPersistent]
        public Faction AllianceFaction
        {
            get;
            private set;
        }

        public Faction RandomFaction
        {
            get
            {
                return Utility.HeadsOrTails() ? HordeFaction : AllianceFaction;
            }
        }

        public Faction GetFaction(FactionGroup fact)
        {
            return fact == FactionGroup.Alliance ? AllianceFaction : HordeFaction;
        }

        #endregion Factions

        #region Spells

        public uint SpellGroupId;

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

        /// <summary>
        /// Usable Spells to be casted by Mobs of this Type
        /// </summary>
        [NotPersistent]
        public Dictionary<SpellId, Spell> Spells;

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
                Spells = new Dictionary<SpellId, Spell>(5);
            }
            OnSpellAdded(spell);
            Spells[spell.SpellId] = spell;
        }

        private void OnSpellAdded(Spell spell)
        {
        }

        #endregion Spells

        #region Spawns

        [NotPersistent]
        public List<NPCSpawnEntry> SpawnEntries = new List<NPCSpawnEntry>(3);

        [NotPersistent]
        public NPCSpawnEntry FirstSpawnEntry
        {
            get { return SpawnEntries.Count > 0 ? SpawnEntries[0] : null; }
        }

        #endregion Spawns

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

        #endregion Interactable NPCs

        #region Vehicles

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

        [NotPersistent]
        public VehicleEntry VehicleEntry;

        public bool IsVehicle
        {
            get { return VehicleEntry != null; }
        }

        public float HoverHeight;

        public float VehicleAimAdjustment;

        #endregion Vehicles

        #region Decay

        /// <summary>
        /// The default decay delay in seconds.
        /// </summary>
        [NotPersistent]
        public int DefaultDecayDelayMillis;

        /// <summary>
        /// The default delay before removing the NPC after it died when not looted.
        /// </summary>
        private int _DefaultDecayDelayMillis
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

        #endregion Decay

        #region FinalizeDataHolder

        /// <summary>
        /// Is called to initialize the object; usually after a set of other operations have been performed or if
        /// the right time has come and other required steps have been performed.
        /// </summary>
        public void FinalizeDataHolder()
        {
            if (string.IsNullOrEmpty(DefaultName))
            {
                ContentMgr.OnInvalidDBData("NPCEntry has no name: " + this);
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

            AggroBaseRange = AggroBaseRangeDefault;

            NPCId = (NPCId)Id;

            DefaultDecayDelayMillis = _DefaultDecayDelayMillis;
            Family = NPCMgr.GetFamily(FamilyId);

            if (Type == CreatureType.NotSpecified || VehicleEntry != null)
            {
                IsIdle = true;
            }

            if (Type == CreatureType.NotSpecified && UnitFlags.HasFlag((UnitFlags.Passive | UnitFlags.NotSelectable)))
            {
                IsEventTrigger = true;
                IsIdle = false;
            }

            if (Resistances == null)
            {
                Resistances = new int[ItemConstants.MaxResCount];
            }

            SetFlagIndices = Utility.GetSetIndices((uint)NPCFlags);

            // set/fix factions
            HordeFaction = FactionMgr.Get(HordeFactionId);
            AllianceFaction = FactionMgr.Get(AllianceFactionId);
            if (HordeFaction == null)
            {
                HordeFaction = AllianceFaction;
                HordeFactionId = AllianceFactionId;
            }
            else if (AllianceFaction == null)
            {
                AllianceFaction = HordeFaction;
                AllianceFactionId = HordeFactionId;
            }
            if (AllianceFaction == null)
            {
                ContentMgr.OnInvalidDBData("NPCEntry has no valid Faction: " + this);
                HordeFaction = AllianceFaction = NPCMgr.DefaultFaction;
                HordeFactionId = AllianceFactionId = (FactionTemplateId)HordeFaction.Template.Id;
            }

            // speeds
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
                ContentMgr.OnInvalidDBData("NPCEntry has no valid DisplayId: {0} ({1})", this, DisplayIds.ToString(", "));
                return;
            }

            if (TrainerTemplateId != 0)
            {
                if (!NPCMgr.TrainerSpellTemplates.ContainsKey(TrainerTemplateId))
                {
                    ContentMgr.OnInvalidDBData("NPCEntry has invalid TrainerTemplateId: {0} ({1})", this, TrainerTemplateId);
                }
                else
                {
                    if (TrainerEntry == null)
                    {
                        TrainerEntry = new TrainerEntry();
                    }
                    foreach (var trainerSpell in NPCMgr.TrainerSpellTemplates[TrainerTemplateId])
                    {
                        TrainerEntry.AddSpell(trainerSpell);
                    }
                }
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

        #endregion FinalizeDataHolder

        #region Creators

        [NotPersistent]
        public Func<NPC, IBrain> BrainCreator;

        [NotPersistent]
        public NPCCreator NPCCreator;

        public NPC Create(uint difficulty = uint.MaxValue)
        {
            var npc = NPCCreator(GetEntry(difficulty));
            npc.SetupNPC(this, null);
            return npc;
        }

        public NPC Create(NPCSpawnPoint spawn)
        {
            var npc = NPCCreator(GetEntry(spawn.Map.DifficultyIndex));
            npc.SetupNPC(this, spawn);
            return npc;
        }

        public NPC SpawnAt(Map map, Vector3 pos, bool hugGround = false)
        {
            var npc = Create(map.DifficultyIndex);
            if (hugGround && InhabitType == InhabitType.Ground)
            {
                pos.Z = map.Terrain.GetGroundHeightUnderneath(pos);
            }
            map.AddObject(npc, pos);
            return npc;
        }

        public NPC SpawnAt(IWorldZoneLocation loc, bool hugGround = false)
        {
            var npc = Create(loc.Map.DifficultyIndex);
            npc.Zone = loc.GetZone();
            var pos = loc.Position;
            if (hugGround && InhabitType == InhabitType.Ground)
            {
                pos.Z = loc.Map.Terrain.GetGroundHeightUnderneath(pos);
            }
            loc.Map.AddObject(npc, pos);
            return npc;
        }

        public NPC SpawnAt(IWorldLocation loc, bool hugGround = false)
        {
            var npc = Create(loc.Map.DifficultyIndex);
            var pos = loc.Position;
            if (hugGround && InhabitType == InhabitType.Ground)
            {
                pos.Z = loc.Map.Terrain.GetGroundHeightUnderneath(pos);
            }
            loc.Map.AddObject(npc, loc.Position);
            npc.Phase = loc.Phase;
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

        #endregion Creators

        #region Events

        internal void NotifyActivated(NPC npc)
        {
            var evt = Activated;
            if (evt != null)
            {
                evt(npc);
            }
        }

        internal void NotifyDeleted(NPC npc)
        {
            var evt = Deleted;
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

        #endregion Events

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

            if (DifficultyOverrideEntryIds != null && DifficultyOverrideEntryIds.Any(id => id != 0))
            {
                var parts = new List<string>(4);
                for (var i = 0u; i < 3; i++)
                {
                    var id = DifficultyOverrideEntryIds[i];
                    if (id != 0)
                    {
                        var entry = NPCMgr.GetEntry(id);
                        MapTemplate map;
                        MapDifficultyEntry diff;
                        if (entry != null && (map = GetMapTemplate()) != null && (diff = map.GetDifficulty(i)) != null)
                        {
                            parts.Add(string.Format("{0} ({1}) = " + id + " (" + (uint)id + ")", diff.IsHeroic ? "Heroic" : "Normal", diff.MaxPlayerCount));
                        }
                        else
                        {
                            parts.Add("(unknown difficulty) = " + id + " (" + (uint)id + ")");
                        }
                    }
                }
                writer.WriteLine("DifficultyOverrides: {0}", parts.ToString("; "));
            }

            if (KillCreditIds != null && KillCreditIds.Any(id => id != 0))
            {
                var parts = new List<string>(2);
                for (var i = 0u; i < UnitConstants.MaxKillCredits; i++)
                {
                    var id = KillCreditIds[i];
                    if (id != 0)
                    {
                        var entry = NPCMgr.GetEntry(id);
                        if (entry != null)
                        {
                            parts.Add(id + " (" + (uint)id + ")");
                        }
                    }
                }
                writer.WriteLine("KillCredits: {0}", parts.ToString("; "));
            }

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

        #endregion Dump

        public override IWorldLocation[] GetInWorldTemplates()
        {
            return SpawnEntries.ToArray();
        }

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