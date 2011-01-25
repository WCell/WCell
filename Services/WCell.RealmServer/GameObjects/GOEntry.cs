using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Data;
using NLog;
using System.Collections;
using WCell.Constants;
using System;
using WCell.Constants.Factions;
using WCell.RealmServer.Factions;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// Should not be changed, once a GameObject of a certain Entry has been added to the World
	/// </summary>
	[DataHolder("Type")]
	[DependingProducer(GameObjectType.Door, typeof(GODoorEntry))]
	[DependingProducer(GameObjectType.Button, typeof(GOButtonEntry))]
	[DependingProducer(GameObjectType.QuestGiver, typeof(GOQuestGiverEntry))]
	[DependingProducer(GameObjectType.Chest, typeof(GOChestEntry))]
	[DependingProducer(GameObjectType.Binder, typeof(GOBinderEntry))]
	[DependingProducer(GameObjectType.Generic, typeof(GOGenericEntry))]
	[DependingProducer(GameObjectType.Trap, typeof(GOTrapEntry))]
	[DependingProducer(GameObjectType.Chair, typeof(GOChairEntry))]
	[DependingProducer(GameObjectType.SpellFocus, typeof(GOSpellFocusEntry))]
	[DependingProducer(GameObjectType.Text, typeof(GOTextEntry))]
	[DependingProducer(GameObjectType.Goober, typeof(GOGooberEntry))]
	[DependingProducer(GameObjectType.Transport, typeof(GOTransportEntry))]
	[DependingProducer(GameObjectType.AreaDamage, typeof(GOAreaDamageEntry))]
	[DependingProducer(GameObjectType.Camera, typeof(GOCameraEntry))]
	[DependingProducer(GameObjectType.MapObject, typeof(GOMapObjectEntry))]
	[DependingProducer(GameObjectType.MOTransport, typeof(GOMOTransportEntry))]
	[DependingProducer(GameObjectType.DuelFlag, typeof(GODuelFlagEntry))]
	[DependingProducer(GameObjectType.FishingNode, typeof(GOFishingNodeEntry))]
	[DependingProducer(GameObjectType.SummoningRitual, typeof(GOSummoningRitualEntry))]
	[DependingProducer(GameObjectType.Mailbox, typeof(GOMailboxEntry))]
	[DependingProducer(GameObjectType.DONOTUSE, typeof(GOAuctionHouseEntry))]
	[DependingProducer(GameObjectType.GuardPost, typeof(GOGuardPostEntry))]
	[DependingProducer(GameObjectType.SpellCaster, typeof(GOSpellCasterEntry))]
	[DependingProducer(GameObjectType.MeetingStone, typeof(GOMeetingStoneEntry))]
	[DependingProducer(GameObjectType.FlagStand, typeof(GOFlagStandEntry))]
	[DependingProducer(GameObjectType.FishingHole, typeof(GOFishingHoleEntry))]
	[DependingProducer(GameObjectType.FlagDrop, typeof(GOFlagDropEntry))]
	[DependingProducer(GameObjectType.MiniGame, typeof(GOMiniGameEntry))]
	[DependingProducer(GameObjectType.LotteryKiosk, typeof(GOLotteryKioskEntry))]
	[DependingProducer(GameObjectType.CapturePoint, typeof(GOCapturePointEntry))]
	[DependingProducer(GameObjectType.AuraGenerator, typeof(GOAuraGeneratorEntry))]
	[DependingProducer(GameObjectType.DungeonDifficulty, typeof(GODungeonDifficultyEntry))]
	[DependingProducer(GameObjectType.BarberChair, typeof(GOBarberChairEntry))]
	[DependingProducer(GameObjectType.DestructibleBuilding, typeof(GODestructibleBuildingEntry))]
	[DependingProducer(GameObjectType.GuildBank, typeof(GOGuildBankEntry))]
	[DependingProducer(GameObjectType.TrapDoor, typeof(GOTrapDoorEntry))]

	[DependingProducer(GameObjectType.Custom, typeof(GOCustomEntry))]
	public abstract partial class GOEntry : IQuestHolderEntry, IDataHolder
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Entry Id
		/// </summary>
		public uint Id
		{
			get;
			set;
		}

		public uint DisplayId;
		public FactionTemplateId FactionId;
		public GameObjectFlags Flags;
		public GameObjectType Type;
		public float DefaultScale = 1;

		[NotPersistent]
		public GOEntryId GOId;

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

		[Persistent(GOConstants.EntryFieldCount)]
		public int[] Fields = new int[GOConstants.EntryFieldCount];

		/// <summary>
		/// All Templates that use this GOEntry
		/// </summary>
		[NotPersistent]
		public readonly List<GOSpawnEntry> SpawnEntries = new List<GOSpawnEntry>();

		public GOSpawnEntry FirstSpawnEntry
		{
			get { return SpawnEntries.Count > 0 ? SpawnEntries[0] : null; }
		}

		public IWorldLocation[] GetInWorldTemplates()
		{
			return SpawnEntries.ToArray();
		}

		#region Custom Fields
		[NotPersistent]
		public Faction Faction;

		/// <summary>
		/// Whether this GO vanishes after using
		/// </summary>
		[NotPersistent]
		public bool IsConsumable;

		/// <summary>
		/// Whether one may be mounted when using GOs of this Entry.
		/// </summary>
		[NotPersistent]
		public bool AllowMounted;

		/// <summary>
		/// Whether one needs LoS to use this GO
		/// </summary>
		[NotPersistent]
		public bool LosOk;

		/// <summary>
		/// Whether this GO's loot can be taken by the whole Group.
		/// </summary>
		[NotPersistent]
		public bool UseGroupLoot;

		/// <summary>
		/// The Id of a GOTrapEntry that is associated with this chest.
		/// </summary>
		[NotPersistent]
		public uint LinkedTrapId;

		/// <summary>
		/// The Trap that is linked to this Object (if any)
		/// </summary>
		[NotPersistent]
		public GOTrapEntry LinkedTrap;

		[NotPersistent]
		public uint SummonSlotId;

		/// <summary>
		/// The lock of this GO (if any)
		/// </summary>
		[NotPersistent]
		public LockEntry Lock;

		[NotPersistent]
		public Func<GameObject> GOCreator;

		[NotPersistent]
		public Func<GameObjectHandler> HandlerCreator;

		public virtual bool IsTransport
		{
			get { return false; }
		}

		public ResolvedLootItemList GetLootEntries()
		{
			if (this is IGOLootableEntry)
			{
				return LootMgr.GetEntries(LootEntryType.GameObject, ((IGOLootableEntry)this).LootId);
			}
			return null;
		}
		#endregion

		#region Quests
		/// <summary>
		/// The QuestHolderEntry of this NPCEntry, if this is a QuestGiver
		/// </summary>
		[NotPersistent]
		public QuestHolderInfo QuestHolderInfo
		{
			get;
			set;
		}

		[NotPersistent]
		public readonly List<QuestTemplate> RequiredQuests = new List<QuestTemplate>(3);

		/// <summary>
		/// Whether only users of the same Party as the owner
		/// may use this
		/// </summary>
		public virtual bool IsPartyOnly
		{
			get { return false; }
		}
		#endregion

		internal protected virtual void InitEntry() { }

		/// <summary>
		/// Is called when the given new GameObject has been created.
		/// </summary>
		/// <param name="go"></param>
		internal protected virtual void InitGO(GameObject go)
		{
		}


		public override string ToString()
		{
			return DefaultName + " (ID: " + (int)Id + ", " + Id + ")";
		}

		public virtual void FinalizeDataHolder()
		{
			if (Id != 0)
			{
				GOId = (GOEntryId)Id;
			}
			else
			{
				Id = (uint)GOId;
			}

			if (FactionId != 0)
			{
				Faction = FactionMgr.Get(FactionId);
			}

			InitEntry();

			if (HandlerCreator == null)
			{
				HandlerCreator = GOMgr.Handlers[(int)Type];
			}

			if (GOCreator == null)
			{
				if (IsTransport)
				{
					GOCreator = () => new Transport();
				}
				else
				{
					GOCreator = () => new GameObject();
				}
			}

			if (Fields != null)		// ignore invalid ones
			{
				//Fields = null;
				GOMgr.Entries[Id] = this;
			}
		}

		#region Create & Spawn
		public GameObject Spawn(IWorldLocation location)
		{
			return Spawn(location, location as Unit);
		}

		public GameObject Spawn(IWorldLocation where, Unit owner)
		{
			var go = GameObject.Create(this, where, FirstSpawnEntry);
			go.Owner = owner;
			return go;
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given map
		/// </summary>
		/// <param name="owner">Can be null, if the GO is not owned by anyone</param>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(MapId map, Vector3 pos)
		{
			return Spawn(map, pos, null);
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given map
		/// </summary>
		/// <param name="owner">Can be null, if the GO is not owned by anyone</param>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(MapId map, Vector3 pos, Unit owner)
		{
			return Spawn(World.GetMap(map), pos, owner);
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given map
		/// </summary>
		/// <param name="owner">Can be null, if the GO is not owned by anyone</param>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(Map map, Vector3 pos, Unit owner = null)
		{
			if (map == null) throw new ArgumentNullException("map");

			return Spawn(new WorldLocation(map, pos), owner);
		}
		#endregion

		/// <summary>
		/// Returns the GOTemplate of this entry that is closest to the given location
		/// </summary>
		public GOSpawnEntry GetClosestTemplate(IWorldLocation pos)
		{
			return SpawnEntries.GetClosestEntry(pos);
		}

		/// <summary>
		/// Returns the GOTemplate of this entry that is closest to the given location
		/// </summary>
		public GOSpawnEntry GetClosestTemplate(MapId rgn, Vector3 pos)
		{
			return SpawnEntries.GetClosestEntry(new WorldLocationStruct(rgn, pos));
		}

		/// <summary>
		/// Returns the GOTemplate of this entry that is closest to the given location
		/// </summary>
		public GOSpawnEntry GetClosestTemplate(Map rgn, Vector3 pos)
		{
			return SpawnEntries.GetClosestEntry(new WorldLocationStruct(rgn, pos));
		}

		#region Events
		internal bool NotifyUsed(GameObject go, Character user)
		{
			var evt = Used;
			if (evt != null)
			{
				return evt(go, user);
			}
			return true;
		}

		internal void NotifyActivated(GameObject go)
		{
			var evt = Activated;
			if (evt != null)
			{
				evt(go);
			}
		}
		#endregion

		public static IEnumerable<GOEntry> GetAllDataHolders()
		{
			return GOMgr.Entries.Values;
		}
	}
}