/*************************************************************************
 *
 *   file		: GameObject.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-17 05:08:19 +0100 (on, 17 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1256 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;
using NLog;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.Core.Timers;
using WCell.RealmServer.Factions;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.UpdateFields;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Respawning
	/// </summary>
	public partial class GameObject : WorldObject, IOwned, ILockable, IQuestHolder
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.GameObject);

		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicGOHandlers; }
		}

		internal static int _lastGOUID;

		protected GOEntry m_entry;
		protected Faction m_faction;

		protected GameObjectHandler m_handler;
		protected Unit m_Owner;
		protected bool m_respawns;
		protected GOTemplate m_template;
		protected TimerEntry m_decayTimer, m_respawnTimer;
		protected GameObject m_linkedTrap;

		/// <summary>
		/// Use the <c>Create()</c> method to create new GameObjects
		/// </summary>
		public GameObject()
		{
		}

		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		public GameObjectHandler Handler
		{
			get { return m_handler; }
			set
			{
				m_handler = value;
				m_handler.Initialize(this);
			}
		}

		public override string Name
		{
			get { return m_entry != null ? m_entry.DefaultName : ""; }
			set
			{
				throw new NotImplementedException("Dynamic renaming of GOs is not implementable.");
			}
		}

		public GOEntry Entry
		{
			get { return m_entry; }
		}

		/// <summary>
		/// The Template of this GO (if any was used)
		/// </summary>
		public GOTemplate Template
		{
			get { return m_template; }
		}

		/// <summary>
		/// Traps get removed when their AreaAura gets removed
		/// </summary>
		public bool IsTrap
		{
			get;
			internal set;
		}

		/// <summary>
		/// Whether this GO respawns after removal
		/// </summary>
		public bool Respawns
		{
			get { return m_respawns; }
			set
			{
				if (value != m_respawns)
				{
					m_respawns = value;
					if (!value)
					{
						// TODO: Deactivate respawning
						if (!IsInWorld)
						{
							m_region.m_gos[EntityId.Low] = null;
						}
					}
				}
			}
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.GameObject; }
		}

		public override UpdateFlags UpdateFlags
		{
			get { return UpdateFlags.StationaryObject | UpdateFlags.Flag_0x10 | UpdateFlags.HasRotation | UpdateFlags.StationaryObjectOnTransport; }
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

		/// <summary>
		/// Creates the given kind of GameObject with the default Template
		/// </summary>
		public static GameObject Create(GOEntryId id)
		{
			var entry = GOMgr.GetEntry(id);
			if (entry == null)
			{
				return null;
			}
			return Create(entry, entry.Templates.Count > 0 ? entry.Templates[0] : null);
		}

		/// <summary>
		/// Creates a new GameObject with the given parameters
		/// </summary>
		public static GameObject Create(GOEntryId id, GOTemplate templ)
		{
			var entry = GOMgr.GetEntry(id);
			if (entry != null)
			{
				return Create(entry, templ);
			}
			return null;
		}

		/// <summary>
		/// Creates a new GameObject with the given parameters
		/// </summary>
		public static GameObject Create(GOEntry entry, GOTemplate templ)
		{
			var go = entry.GOCreator();
			var handlerCreator = entry.HandlerCreator;
			go.Init(entry, templ); 
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
			return go;
		}

		/// <summary>
		/// Initialize the GO
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="templ"></param>
		internal virtual void Init(GOEntry entry, GOTemplate templ)
		{
			EntityId = EntityId.GetGameObjectId((uint)Interlocked.Increment(ref _lastGOUID), entry.GOId);
			Type |= ObjectTypes.GameObject;
			//DynamicFlagsLow = GameObjectDynamicFlagsLow.Activated;
			m_entry = entry;
			m_template = templ;

			DisplayId = entry.DisplayId;
			EntryId = entry.Id;
			GOType = entry.Type;
			Flags = m_entry.Flags;
			m_faction = m_entry.Faction ?? Faction.NullFaction;
			ScaleX = m_entry.DefaultScale;

			if (templ != null)
			{
				Phase = templ.PhaseMask;
				State = templ.State;
				if (templ.Scale != 1)
				{
					ScaleX = templ.Scale;
				}
				Orientation = templ.Orientation;
				AnimationProgress = templ.AnimProgress;
				SetRotationFields(templ.Rotations);
			}

			m_entry.InitGO(this);
		}

		private static readonly double RotatationConst = Math.Atan(Math.Pow(2.0f, -20.0f));

		protected void SetRotationFields(float[] rotations)
		{
			if (rotations.Length != 4)
				return;

			SetFloat(GameObjectFields.PARENTROTATION + 0, rotations[0]);
			SetFloat(GameObjectFields.PARENTROTATION + 1, rotations[1]);

			double rotSin = Math.Sin(Orientation / 2.0f),
				   rotCos = Math.Cos(Orientation / 2.0f);

			Rotation = (long)(rotSin / RotatationConst * (rotCos >= 0 ? 1.0f : -1.0f)) & 0x1FFFFF;

			if (rotations[2] == 0 && rotations[3] == 0)
			{
				SetFloat(GameObjectFields.PARENTROTATION + 2, (float)rotSin);
				SetFloat(GameObjectFields.PARENTROTATION + 3, (float)rotCos);
			}
			else
			{
				SetFloat(GameObjectFields.PARENTROTATION + 2, rotations[2]);
				SetFloat(GameObjectFields.PARENTROTATION + 3, rotations[3]);
			}
		}

		protected internal override void OnEnterRegion()
		{
			ArrayUtil.Set(ref m_region.m_gos, EntityId.Low, this);

			// add Trap
			if (m_entry.LinkedTrap != null)
			{
				m_linkedTrap = m_entry.LinkedTrap.Spawn(this, m_Owner);
				//if (m_entry.LinkedTrap.DisplayId != 0)
				//{
				//    m_linkedTrap = m_entry.LinkedTrap.Spawn(m_region, m_position, m_Owner);
				//}
				//else
				//{
				//    ActivateTrap(m_entry.LinkedTrap);
				//}
			}

			m_entry.NotifyActivated(this);
		}

		protected internal override void OnLeavingRegion()
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
			base.OnLeavingRegion();
		}

		protected override UpdateType GetCreationUpdateType(UpdateFieldFlags relation)
		{
			if (m_entry is GODuelFlagEntry)
			{
				return UpdateType.CreateSelf;
			}
			return UpdateType.Create;
		}

		public bool IsCloseEnough(Unit unit, float radius)
		{
			return (unit.IsInRadius(this, radius)) || (unit is Character && ((Character)unit).Role.IsStaff);
		}

		public override string ToString()
		{
			return m_entry.DefaultName + " (Template: " + m_template + ")";
		}

		public bool CanInteractWith(Character chr)
		{
			// Todo: Other checks?
			if (IsInRadius(chr.Position, 10.0f))
			{
				return true;
			}
			return false;
		}

		#region Handling

		/// <summary>
		/// Makes the given Unit use this GameObject.
		/// Skill-locked GameObjects cannot be used directly but must be interacted on with spells.
		/// </summary>
		public bool Use(Character chr)
		{
			if ((Lock == null || Lock.IsUnlocked || Lock.Keys.Length > 0) && Handler.TryUse(chr))
			{
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

		#region Decay
		void DecayNow(float dt)
		{
			Delete();
		}

		protected internal override void DeleteNow()
		{
			if (m_linkedTrap != null)
			{
				m_linkedTrap.DeleteNow();
			}

			if (Respawns)
			{
				// TODO: Finish respawning
				OnDeleted();
				m_respawnTimer = new TimerEntry();
			}
			else
			{
				base.DeleteNow();
			}
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
		/// </summary>
		public float RemainingDecayDelay
		{
			get
			{
				return m_decayTimer.RemainingInitialDelay;
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

		public override void Update(float dt)
		{
			base.Update(dt);
			if (m_decayTimer != null)
			{
				m_decayTimer.Update(dt);
			}
		}

		#endregion

		#region Update
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
	}
}