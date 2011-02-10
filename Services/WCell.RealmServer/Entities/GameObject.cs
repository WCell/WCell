using System.Threading;
using NLog;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Timers;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.UpdateFields;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Respawning
	/// </summary>
	public partial class GameObject : WorldObject, IOwned, ILockable, IQuestHolder
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.GameObject);

		#region Create & Init
		/// <summary>
		/// Creates the given kind of GameObject with the default Template
		/// </summary>
		public static GameObject Create(GOEntryId id, IWorldLocation location, GOSpawnEntry spawnEntry = null, GOSpawnPoint spawnPoint = null)
		{
			var entry = GOMgr.GetEntry(id);
			if (entry == null)
			{
				return null;
			}
			return Create(entry, location, spawnEntry, spawnPoint);
		}

		/// <summary>
		/// Creates a new GameObject with the given parameters
		/// </summary>
		public static GameObject Create(GOEntryId id, Map map, GOSpawnEntry spawnEntry = null, GOSpawnPoint spawnPoint = null)
		{
			var entry = GOMgr.GetEntry(id);
			if (entry != null)
			{
				return Create(entry, map, spawnEntry, spawnPoint);
			}
			return null;
		}

		public static GameObject Create(GOEntry entry, Map map, GOSpawnEntry spawnEntry = null, GOSpawnPoint spawnPoint = null)
		{
			return Create(entry, new WorldLocation(map, Vector3.Zero), spawnEntry, spawnPoint);
		}

		/// <summary>
		/// Creates a new GameObject with the given parameters
		/// </summary>
		public static GameObject Create(GOEntry entry, IWorldLocation where, GOSpawnEntry spawnEntry = null, GOSpawnPoint spawnPoint = null)
		{
			var go = entry.GOCreator();
			var handlerCreator = entry.HandlerCreator;
			go.Init(entry, spawnEntry, spawnPoint);
			if (handlerCreator != null)
			{
				go.Handler = handlerCreator();
			}
			else
			{
				log.Warn("GOEntry {0} did not have a HandlerCreator set - Type: {1}", entry, entry.Type);
				go.Delete();
				return null;
			}
			go.Phase = where.Phase;
			var pos = where.Position;
			where.Map.AddObject(go, ref pos);

			go.MarkUpdate(GameObjectFields.DYNAMIC);
			return go;
		}

		/// <summary>
		/// Initialize the GO
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="templ"></param>
		internal virtual void Init(GOEntry entry, GOSpawnEntry spawnEntry, GOSpawnPoint spawnPoint)
		{
			EntityId = EntityId.GetGameObjectId((uint)Interlocked.Increment(ref _lastGOUID), entry.GOId);
			Type |= ObjectTypes.GameObject;
			//DynamicFlagsLow = GameObjectDynamicFlagsLow.Activated;
			m_entry = entry;
			m_spawnPoint = spawnPoint;

			DisplayId = entry.DisplayId;
			EntryId = entry.Id;
			GOType = entry.Type;
			Flags = m_entry.Flags;
			m_faction = m_entry.Faction ?? Faction.NullFaction;
			ScaleX = m_entry.Scale;
			GossipMenu = entry.DefaultGossip;

			if (QuestHolderInfo != null && GossipMenu == null)
			{
				// make sure, there is a GossipMenu that allows the player to start/finish quests
				GossipMenu = new GossipMenu();
			}

			spawnEntry = spawnEntry ?? entry.FirstSpawnEntry;
			if (spawnEntry != null)
			{
				Phase = spawnEntry.Phase;
				State = spawnEntry.State;
				if (spawnEntry.Scale != 1)
				{
					ScaleX = spawnEntry.Scale;
				}
				Orientation = spawnEntry.Orientation;
				AnimationProgress = spawnEntry.AnimProgress;
				SetRotationFields(spawnEntry.Rotations);
			}

			m_entry.InitGO(this);
		}
		#endregion

		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicGOHandlers; }
		}

		internal static int _lastGOUID;

		protected GOEntry m_entry;
		protected Faction m_faction;
		protected GOSpawnPoint m_spawnPoint;

		protected GameObjectHandler m_handler;
		protected bool m_respawns;
		protected TimerEntry m_decayTimer;
		protected GameObject m_linkedTrap;
		protected internal bool m_IsTrap;

		/// <summary>
		/// Use the <c>Create()</c> method to create new GameObjects
		/// </summary>
		public GameObject()
		{
		}

		#region Locks and Loot

		public LockEntry Lock
		{
			get { return m_entry.Lock; }
		}

		public override void OnFinishedLooting()
		{
			if (m_entry.IsConsumable)
			{
				Delete();
			}
		}

		public override uint GetLootId(LootEntryType type)
		{
			if (m_entry is IGOLootableEntry)
			{
				return ((IGOLootableEntry)m_entry).LootId;
			}
			return 0;
		}

		public override bool UseGroupLoot
		{
			get { return m_entry.UseGroupLoot; }
		}
		#endregion

		#region Adding and Removing
		protected internal override void OnEnterMap()
		{
			// add Trap
			if (m_entry.LinkedTrap != null)
			{
				m_linkedTrap = m_entry.LinkedTrap.Spawn(this, m_master);
				//if (m_entry.LinkedTrap.DisplayId != 0)
				//{
				//    m_linkedTrap = m_entry.LinkedTrap.Spawn(m_map, m_position, m_Owner);
				//}
				//else
				//{
				//    ActivateTrap(m_entry.LinkedTrap);
				//}
			}

			// add to set of spawned objects of SpawnPoint
			if (m_spawnPoint != null)
			{
				m_spawnPoint.SignalSpawnlingActivated(this);
			}

			// trigger events
			m_entry.NotifyActivated(this);
		}

		protected internal override void OnLeavingMap()
		{
			if (m_master is Character)
			{
				if (m_master.IsInWorld)
				{
					((Character)m_master).OnOwnedGODestroyed(this);
				}
				//Delete();
			}
			m_handler.OnRemove();
			SendDespawn();
			base.OnLeavingMap();
		}
		#endregion

		#region Using & Looting
		public bool IsCloseEnough(Unit unit, float radius = 10)
		{
			return (unit.IsInRadius(this, radius)) || (unit is Character && ((Character)unit).Role.IsStaff);
		}

		public bool CanUseInstantly(Character chr)
		{
			if (!IsCloseEnough(chr))
			{
				return false;
			}

			return Lock == null && CanBeUsedBy(chr);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool CanBeUsedBy(Character chr)
		{
			if (IsEnabled)
			{
				return !Flags.HasFlag(GameObjectFlags.ConditionalInteraction) || chr.QuestLog.IsRequiredForAnyQuest(this);
			}
			return false;
		}

		/// <summary>
		/// Makes the given Unit use this GameObject.
		/// Skill-locked GameObjects cannot be used directly but must be interacted on with spells.
		/// </summary>
		public bool Use(Character chr)
		{
			if ((Lock == null || Lock.IsUnlocked || Lock.Keys.Length > 0) &&
				Handler.TryUse(chr))
			{
				if (Entry.PageId != 0)
				{
					MiscHandler.SendGameObjectTextPage(chr, this);
				}
				if (GossipMenu != null)
				{
					chr.StartGossip(GossipMenu, this);
				}


				chr.QuestLog.OnUse(this);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Lets the given user try to loot this object.
		/// Called on Chests automatically when using Chest-GOs.
		/// </summary>
		public bool TryLoot(Character chr)
		{
			return ((ILockable)this).TryLoot(chr);
		}

		#endregion

		#region Quests
		/// <summary>
		/// All available Quest information, in case that this is a QuestGiver
		/// </summary>
		public QuestHolderInfo QuestHolderInfo
		{
			get
			{
				return m_entry.QuestHolderInfo;
			}
			internal set
			{
				m_entry.QuestHolderInfo = value;
			}
		}

		public bool CanGiveQuestTo(Character chr)
		{
			return IsInRadiusSq(chr, GOMgr.DefaultInteractDistanceSq);
		}

		public void OnQuestGiverStatusQuery(Character chr)
		{
			// re-send dynamic update
			SendSpontaneousUpdate(chr, GameObjectFields.DYNAMIC);
		}
		#endregion

		#region Decay
		void DecayNow(int dt)
		{
			Delete();
		}

		protected internal override void DeleteNow()
		{
			if (m_spawnPoint != null)
			{
				m_spawnPoint.SignalSpawnlingDied(this);
			}
			if (m_linkedTrap != null)
			{
				m_linkedTrap.DeleteNow();
			}
			base.DeleteNow();
		}

		void StopDecayTimer()
		{
			if (m_decayTimer != null)
			{
				m_decayTimer.Stop();
				m_decayTimer = null;
			}
		}

		/// <summary>
		/// Can be set to initialize Decay after the given delay in seconds.
		/// Will stop the timer if set to a value less than 0
		/// </summary>
		public int RemainingDecayDelayMillis
		{
			get
			{
				return m_decayTimer.RemainingInitialDelayMillis;
			}
			set
			{
				if (value < 0)
				{
					StopDecayTimer();
				}
				else
				{
					m_decayTimer = new TimerEntry(DecayNow);
					m_decayTimer.Start(value, 0);
				}
			}
		}

		public override void Update(int dt)
		{
			base.Update(dt);
			if (m_decayTimer != null)
			{
				m_decayTimer.Update(dt);
			}
		}

		#endregion

		#region Update
		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.GameObject; }
		}

		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.StationaryObject | UpdateFlags.Flag_0x10 | UpdateFlags.HasRotation | UpdateFlags.StationaryObjectOnTransport; }
		}

		protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
		{
			if (m_entry is GODuelFlagEntry)
			{
				return UpdateType.CreateSelf;
			}
			return UpdateType.Create;
		}

		protected override void WriteMovementUpdate(PrimitiveWriter packet, UpdateFieldFlags relation)
		{
			// StationaryObjectOnTransport
			if (UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObjectOnTransport))
			{
				EntityId.Zero.WritePacked(packet);
				packet.Write(Position);
				packet.Write(Position); // transport position, but server seemed to send normal position except orientation
				packet.Write(Orientation);
				packet.Write(0.0f);
			}
			else if (UpdateFlags.HasAnyFlag(UpdateFlags.StationaryObject))
			{
				#region UpdateFlag.Flag_0x40 (StationaryObject)

				packet.Write(Position);
				packet.WriteFloat(Orientation);

				#endregion
			}
		}

		protected override void WriteTypeSpecificMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation, UpdateFlags updateFlags)
		{
			// Will only be GameObjects
			if (updateFlags.HasAnyFlag(UpdateFlags.Transport))
			{
				writer.Write(Utility.GetSystemTime());
			}
			if (updateFlags.HasAnyFlag(UpdateFlags.HasRotation))
			{
				writer.Write(Rotation);
			}
		}
		#endregion

		public override string ToString()
		{
			return m_entry.DefaultName + " (SpawnPoint: " + m_spawnPoint + ")";
		}
	}
}