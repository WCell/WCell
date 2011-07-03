/*************************************************************************
 *
 *   file		: WorldObject.cs
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
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Paths;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.NPCs;
using WCell.Util.Collections;
using NLog;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.Updates;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Constants.World;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Constants.NPCs;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Handlers;
using WCell.Constants.Spells;
using WCell.Core.Terrain.Paths;
using Cell.Core;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Used to override visibility of objects
	/// </summary>
	public enum VisibilityStatus
	{
		Default,
		Visible,
		Invisible
	}

	/// <summary>
	/// 
	/// </summary>
	public abstract partial class WorldObject : ObjectBase, IFactionMember, IWorldLocation, INamedEntity, IContextHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public static readonly ObjectPool<HashSet<WorldObject>> WorldObjectSetPool = ObjectPoolMgr.CreatePool(() => new HashSet<WorldObject>());
		public static readonly ObjectPool<List<WorldObject>> WorldObjectListPool = ObjectPoolMgr.CreatePool(() => new List<WorldObject>());

		/// <summary>
		/// Default phase (the one that existed long before phasing was added)
		/// </summary>
		public const uint DefaultPhase = 1;

		/// <summary>
		/// All phases
		/// </summary>
		public const uint AllPhases = uint.MaxValue;

		#region Variables
		/// <summary>
		/// Default vision range. Characters will only receive packets of what happens within this range (unit: Yards)
		/// </summary>
		public static float BroadcastRange = 120.0f;

		public const float InFrontAngleMin = 5f * MathUtil.PI / 3f;

		public const float InFrontAngleMax = MathUtil.PI / 3f;

		public const float BehindAngleMin = 2 * MathUtil.PI / 3f;

		public const float BehindAngleMax = 4f * MathUtil.PI / 3f;

		public static readonly WorldObject[] EmptyArray = new WorldObject[0];
		public static readonly List<WorldObject> EmptyList = new List<WorldObject>();

		public static UpdatePriority DefaultObjectUpdatePriority = UpdatePriority.LowPriority;

		public static float HighlightScale = 5f;

		public static int HighlightDelayMillis = 1500;
		#endregion

		protected Vector3 m_position;

		/// <summary>
		/// never null
		/// </summary>
		protected Map m_Map;
		internal ZoneSpacePartitionNode Node;
		protected bool HasNode { get { return Node != null; } }
		protected Zone m_zone;
		protected float m_orientation;
		protected SpellCast m_spellCast;
		protected List<AreaAura> m_areaAuras;
		protected ObjectReference m_CasterReference;
		protected Unit m_master;
		protected int m_areaCharCount;
		protected uint m_Phase = 1;
		public readonly uint CreationTime;

		/// <summary>
		/// Messages to be processed during the next Update (which ensures that it will be within the Object's context)
		/// </summary>
		internal readonly LockfreeQueue<IMessage> m_messageQueue = new LockfreeQueue<IMessage>();

		protected WorldObject()
		{
			CreationTime = Utility.GetSystemTime();
			LastUpdateTime = DateTime.Now;
		}

		#region Misc Properties
		public virtual ObjectTemplate Template
		{
			get { return null; }
		}

		/// <summary>
		/// Time in seconds since creation
		/// </summary>
		public uint Age
		{
			get { return Utility.GetSystemTime() - CreationTime; }
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual uint Phase
		{
			get { return m_Phase; }
			set { m_Phase = value; }
		}

		/// <summary>
		/// The current position of the object
		/// </summary>
		public virtual Vector3 Position
		{
			get { return m_position; }
			internal set { m_position = value; }
		}

		/// <summary>
		/// The current zone of the object
		/// </summary>
		public virtual Zone Zone
		{
			get { return m_zone; }
			internal set { m_zone = value; }
		}

		public virtual void SetZone(Zone zone)
		{
			Zone = zone;
		}

		public ZoneTemplate ZoneTemplate
		{
			get { return m_zone != null ? m_zone.Template : null; }
		}

		public ZoneId ZoneId
		{
			get { return m_zone != null ? m_zone.Id : ZoneId.None; }
		}

		/// <summary>
		/// The current map of the object.
		/// Map must (and will) never be null.
		/// </summary>
		public virtual Map Map
		{
			get { return m_Map; }
			internal set { m_Map = value; }
		}

		public MapId MapId
		{
			get { return m_Map != null ? m_Map.Id : MapId.End; }
		}

		/// <summary>
		/// The heading of the object (direction it is facing)
		/// </summary>
		public float Orientation
		{
			get { return m_orientation; }
			set { m_orientation = value; }
		}

		public override bool IsInWorld
		{
			get { return m_Map != null; }
		}

		public abstract string Name
		{
			get;
			set;
		}

		/// <summary>
		/// TODO: Find correct caster-level for non-units
		/// </summary>
		public virtual int CasterLevel
		{
			get { return 0; }
		}

		public ObjectReference SharedReference
		{
			get
			{
				if (m_CasterReference == null)
				{
					m_CasterReference = CreateCasterInfo();
				}

				m_CasterReference.Level = CasterLevel;
				return m_CasterReference;
			}
		}

		/// <summary>
		/// Whether there are active Characters in the Area
		/// </summary>
		public bool IsAreaActive
		{
			get { return m_areaCharCount > 0; }
		}

		/// <summary>
		/// The amount of Characters nearby.
		/// </summary>
		public int AreaCharCount
		{
			get { return m_areaCharCount; }
			internal set { m_areaCharCount = value; }
		}

		protected internal virtual void OnEncounteredBy(Character chr)
		{
			++AreaCharCount;
		}

		public virtual bool IsTrap
		{
			get { return false; }
		}

		public bool IsCorpse
		{
			get { return this is Corpse || IsNPCCorpse; }
		}

		public bool IsNPCCorpse
		{
			get { return this is NPC && !((NPC)this).IsAlive; }
		}
		#endregion

		#region Spells
		/// <summary>
		/// whether this Object is currently casting or channeling a Spell
		/// </summary>
		public bool IsUsingSpell
		{
			get
			{
				return m_spellCast != null && (m_spellCast.IsCasting || m_spellCast.IsChanneling);
			}
		}

		public void SetSpellCast(SpellCast cast)
		{
			if (m_spellCast != null && m_spellCast != cast)
			{
				m_spellCast.Dispose();
			}
			m_spellCast = cast;
		}

		/// <summary>
		/// Set to the SpellCast-object of this Object.
		/// If the Object is not in the world, will return null
		/// </summary>
		public SpellCast SpellCast
		{
			get
			{
				if (m_spellCast == null)
				{
					m_spellCast = SpellCast.ObtainPooledCast(this);
					InitSpellCast();
				}

				return m_spellCast;
			}
			internal set
			{
				if (value == m_spellCast)
				{
					return;
				}

				m_spellCast = value;
			}
		}

		public float GetSpellMaxRange(Spell spell)
		{
			return GetSpellMaxRange(spell, spell.Range.MaxDist);
		}

		public float GetSpellMaxRange(Spell spell, float range)
		{
			return GetSpellMaxRange(spell, null, range);
		}

		public float GetSpellMaxRange(Spell spell, WorldObject target)
		{
			return GetSpellMaxRange(spell, target, spell.Range.MaxDist);
		}

		public float GetSpellMaxRange(Spell spell, WorldObject target, float range)
		{
			if (target is Unit)
			{
				range += ((Unit)target).CombatReach;
			}
			if (this is Unit)
			{
				range += ((Unit)this).CombatReach;
				range = ((Unit)this).Auras.GetModifiedFloat(SpellModifierType.Range, spell, range);
			}
			return range;
		}

		public float GetSpellMinRange(float range, WorldObject target)
		{
			if (target is Unit)
			{
				range += ((Unit)target).CombatReach;
			}
			if (this is Unit)
			{
				range += ((Unit)this).CombatReach;
			}
			return range;
		}
		#endregion

		/// <summary>
		/// Can be used to slow down execution of methods that:
		///		1. Should not be executed too often
		///		2. Don't need to be timed precisely
		/// For example: AI updates
		/// </summary>
		public bool CheckTicks(int ticks)
		{
			return ticks == 0 || ((Map.TickCount + EntityId.Low) % ticks) == 0;
		}

		/// <summary>
		/// Creates a new CasterInfo object to represent this WorldObject
		/// </summary>
		protected ObjectReference CreateCasterInfo()
		{
			return new ObjectReference(this);
		}

		protected virtual void InitSpellCast()
		{
		}

		public bool IsInPhase(uint phase)
		{
			return (Phase & phase) != 0;
		}

		public bool IsInPhase(WorldObject obj)
		{
			return (Phase & obj.Phase) != 0;
		}

		public void SetOrientationTowards(ref Vector3 pos)
		{
			m_orientation = GetAngleTowards(ref pos);
		}

		public void SetOrientationTowards(IHasPosition pos)
		{
			m_orientation = GetAngleTowards(pos);
		}

		public bool SetPosition(Vector3 pt)
		{
			return m_Map.MoveObject(this, ref pt);
		}

		public bool SetPosition(Vector3 pt, float orientation)
		{
			if (m_Map.MoveObject(this, ref pt))
			{
				m_orientation = orientation;
				return true;
			}

			return false;
		}

		#region Master
		/// <summary>
		/// The Master of this Object (Units are their own Masters if not controlled, Objects might have masters that they belong to)
		/// </summary>
		public Unit Master
		{
			get { return m_master != null && m_master.IsInWorld ? m_master : this as Unit; }
			protected internal set
			{
				if (value != m_master)
				{
					if (value != null)
					{
						Faction = value.Faction;
						if (value is Character)
						{
							if (this is Unit)
							{
								((Unit)this).UnitFlags |= UnitFlags.PlayerControlled;
								if (this is NPC)
								{
									// detatch from SpawnPoint
									((NPC)this).m_spawnPoint = null;
								}
							}
						}
					}
					else
					{
						if (this is Unit)
						{
							Faction = ((Unit)this).DefaultFaction;
							((Unit)this).UnitFlags &= ~(UnitFlags.PlayerControlled | UnitFlags.Possessed);
						}
					}

					//var oldMaster = m_master;
					m_master = value;
				}
			}
		}

		public bool HasMaster
		{
			get { return m_master != null && m_master != this; }
		}

		/// <summary>
		/// Either this or the master as Character. 
		/// Returns null if neither is Character.
		/// </summary>
		public Character CharacterMaster
		{
			get
			{
				if (this is Character)
				{
					return (Character)this;
				}
				return m_master as Character;
			}
		}

		/// <summary>
		/// Either this or the master as Unit. 
		/// Returns null if neither is Unit.
		/// </summary>
		public Unit UnitMaster
		{
			get
			{
				if (this is Unit)
				{
					return (Unit)this;
				}
				return m_master;
			}
		}
		#endregion

		#region Positions & Distances
		/// <summary>
		/// The Terrain height at this object's current location
		/// </summary>
		public float TerrainHeight
		{
			get { return Map.Terrain.QueryTerrainHeight(m_position); }
		}

		/// <summary>
		/// Indicates whether the given radius is within the given distance to this object
		/// </summary>
		public bool IsInRadius(ref Vector3 pt, float distance)
		{
			return GetDistanceSq(ref pt) <= distance * distance;
		}

		/// <summary>
		/// Indicates whether the given radius is within the given distance to this object
		/// </summary>
		public bool IsInRadius(Vector3 pt, float distance)
		{
			return GetDistanceSq(ref pt) <= distance * distance;
		}

		public bool IsInRadius(WorldObject obj, float distance)
		{
			return GetDistanceSq(obj) <= distance * distance;
		}

		public bool IsInRadius(ref Vector3 pt, SimpleRange range)
		{
			var distSq = GetDistanceSq(ref pt);
			return distSq <= range.MaxDist * range.MaxDist &&
				(range.MinDist < 1 || distSq >= range.MinDist * range.MinDist);
		}

		public bool IsInRadius(WorldObject obj, SimpleRange range)
		{
			var distSq = GetDistanceSq(obj);
			return distSq <= range.MaxDist * range.MaxDist &&
				(range.MinDist < 1 || distSq >= range.MinDist * range.MinDist);
		}

		/// <summary>
		/// Indicates whether the given obj is in update range
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsInUpdateRange(WorldObject obj)
		{
			return GetDistanceSq(obj) <= BroadcastRange * BroadcastRange;
		}

		/// <summary>
		/// Indicates whether the given radius is within the given distance to this object
		/// </summary>
		public bool IsInRadiusSq(ref Vector3 pt, float sqDistance)
		{
			return GetDistanceSq(ref pt) <= sqDistance;
		}

		public bool IsInRadiusSq(Vector3 pos, float sqDistance)
		{
			return GetDistanceSq(ref pos) <= sqDistance;
		}

		public bool IsInRadiusSq(IHasPosition pos, float sqDistance)
		{
			return GetDistanceSq(pos) <= sqDistance;
		}

		public float GetDistance(ref Vector3 pt)
		{
			float dx = (pt.X - m_position.X);
			float dy = (pt.Y - m_position.Y);
			float dz = (pt.Z - m_position.Z);

			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
		}

		public float GetDistanceSq(ref Vector3 pt)
		{
			float dx = (pt.X - m_position.X);
			float dy = (pt.Y - m_position.Y);
			float dz = (pt.Z - m_position.Z);

			return (dx * dx) + (dy * dy) + (dz * dz);
		}

		public float GetDistance(Vector3 pt)
		{
			float dx = (pt.X - m_position.X);
			float dy = (pt.Y - m_position.Y);
			float dz = (pt.Z - m_position.Z);

			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
		}

		public float GetDistanceSq(Vector3 pt)
		{
			float dx = (pt.X - m_position.X);
			float dy = (pt.Y - m_position.Y);
			float dz = (pt.Z - m_position.Z);

			return (dx * dx) + (dy * dy) + (dz * dz);
		}

		/// <summary>
		/// Less precise method to get the square distance to a point
		/// </summary>
		public int GetDistanceSqInt(ref Vector3 pt)
		{
			var dx = (pt.X - m_position.X);
			var dy = (pt.Y - m_position.Y);
			var dz = (pt.Z - m_position.Z);

			return (int)((dx * dx) + (dy * dy) + (dz * dz));
		}

		public float GetDistance(WorldObject obj)
		{
			var dx = (obj.Position.X - Position.X);
			var dy = (obj.Position.Y - Position.Y);
			var dz = (obj.Position.Z - Position.Z);

			return (float)Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
		}

		public float GetDistanceSq(WorldObject obj)
		{
			var dx = (obj.Position.X - Position.X);
			var dy = (obj.Position.Y - Position.Y);
			var dz = (obj.Position.Z - Position.Z);

			return (dx * dx) + (dy * dy) + (dz * dz);
		}

		public float GetDistanceSq(IHasPosition pos)
		{
			var dx = (pos.Position.X - Position.X);
			var dy = (pos.Position.Y - Position.Y);
			var dz = (pos.Position.Z - Position.Z);

			return (dx * dx) + (dy * dy) + (dz * dz);
		}

		public float GetDistanceXY(ref Vector3 pt)
		{
			var dx = (pt.X - m_position.X);
			var dy = (pt.Y - m_position.Y);

			return (float)Math.Sqrt((dx * dx) + (dy * dy));
		}

		public float GetDistanceXY(WorldObject obj)
		{
			float dx = (obj.Position.X - Position.X);
			float dy = (obj.Position.Y - Position.Y);

			return (float)Math.Sqrt((dx * dx) + (dy * dy));
		}

		public bool IsTeleporting
		{
			get;
			internal set;
		}

		/// <summary>
		/// Adds the given object ontop of this one.
		/// The object will not be added immediately after the method-call.
		/// </summary>
		/// <remarks>This object must be in the world for this method call.</remarks>
		public void PlaceOnTop(WorldObject obj)
		{
			var pos = m_position;
			pos.Z += 2;
			m_Map.TransferObjectLater(obj, pos);
		}

		public void PlaceInFront(WorldObject obj)
		{
			var pos = m_position;
			pos.Z += 1;
			//m_position.GetPointYX(m_orientation, 5, out pos);
			m_Map.TransferObjectLater(obj, pos);
			obj.Orientation = obj.GetAngleTowards(this);
		}
		#endregion

		#region Angles
		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(Vector3 v)
		{
			var angle = (float)Math.Atan2((v.Y - m_position.Y), (v.X - m_position.X));
			if (angle < 0)
			{
				angle += (2 * MathUtil.PI);
			}
			return angle;
		}

		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(ref Vector3 v)
		{
			var angle = (float)Math.Atan2((v.Y - m_position.Y), (v.X - m_position.X));
			if (angle < 0)
			{
				angle += (2 * MathUtil.PI);
			}
			return angle;
		}

		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(IHasPosition obj)
		{
			var angle = (float)Math.Atan2((obj.Position.Y - m_position.Y), (obj.Position.X - m_position.X));
			if (angle < 0)
			{
				// wrap angle from 0 to 2*PI (instead of -PI to PI) to correspond with orientation
				angle += (2 * MathUtil.PI);
			}
			return angle;
		}

		/// <summary>
		/// Returns whether this Object is behind the given obj
		/// </summary>
		public bool IsBehind(WorldObject obj)
		{
			if (obj == this)
			{
				return false;
			}

			var angle = Math.Abs(obj.m_orientation - obj.GetAngleTowards(m_position));
			return angle >= BehindAngleMin &&
				angle <= BehindAngleMax;
		}

		/// <summary>
		/// Returns whether this Object is in front of the given obj
		/// </summary>
		public bool IsInFrontOf(WorldObject obj)
		{
			if (obj == this)
			{
				return false;
			}

			var angle = Math.Abs(obj.m_orientation - obj.GetAngleTowards(m_position));
			return angle <= InFrontAngleMax ||
				angle >= InFrontAngleMin;		// difference is close to 0 (or 2 pi) if obj is in view field of the other
		}


		/// <summary>
		/// Returns whether the given pos is in front of this Object
		/// </summary>
		public bool IsInFrontOfThis(Vector3 pos)
		{
			var angle = Math.Abs(m_orientation - GetAngleTowards(pos));
			return angle <= InFrontAngleMax ||
				angle >= InFrontAngleMin;
		}

		/// <summary>
		/// 
		/// </summary>
		public void GetPointXY(float angle, float dist, out Vector3 pos)
		{
			pos = m_position;
			m_position.GetPointYX(angle + m_orientation, dist, out pos);
		}

		/// <summary>
		/// 
		/// </summary>
		public void GetPointInFront(float dist, out Vector3 pos)
		{
			GetPointXY(0, dist, out pos);
		}

		/// <summary>
		/// 
		/// </summary>
		public void GetPointBehind(float dist, out Vector3 pos)
		{
			GetPointXY(MathUtil.PI, dist, out pos);
		}
		#endregion

		public virtual ClientLocale Locale
		{
			get { return RealmServerConfiguration.DefaultLocale; }
			set { log.Warn("Tried to illegaly set WorldObject.Locale for: {0}", this); }
		}

		#region Say
		public void Say(string message)
		{
			Say(ChatMgr.ListeningRadius, message);
		}

		public virtual void Say(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, ChatLanguage.Universal, message, radius);
		}

		public void Say(RealmLangKey key, params object[] args)
		{
			Say(RealmLocalizer.Instance.Translate(Locale, key, args));
		}

		public void Say(string message, params object[] args)
		{
			Say(string.Format(message, args));
		}

		public void Say(string[] localizedMsgs)
		{
			Say(ChatMgr.ListeningRadius, localizedMsgs);
		}

		public virtual void Say(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterSay, ChatLanguage.Universal, localizedMsgs, radius);
		}
		#endregion

		#region Yell
		public virtual void Yell(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, ChatLanguage.Universal, message, radius);
		}

		public void Yell(string message)
		{
			Yell(ChatMgr.YellRadius, message);
		}

		public void Yell(RealmLangKey key, params object[] args)
		{
			Yell(RealmLocalizer.Instance.Translate(Locale, key, args));
		}

		public void Yell(string message, params object[] args)
		{
			Yell(string.Format(message, args));
		}

		public void Yell(string[] localizedMsgs)
		{
			Yell(ChatMgr.YellRadius, localizedMsgs);
		}

		public virtual void Yell(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterYell, ChatLanguage.Universal, localizedMsgs, radius);
		}
		#endregion

		#region Emote
		public virtual void Emote(float radius, string message)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, ChatLanguage.Universal, message, radius);
		}

		public void Emote(string message)
		{
			Emote(ChatMgr.ListeningRadius, message);
		}

		public void Emote(RealmLangKey key, params object[] args)
		{
			Emote(RealmLocalizer.Instance.Translate(Locale, key, args));
		}

		public void Emote(string message, params object[] args)
		{
			Emote(string.Format(message, args));
		}

		public void Emote(string[] localizedMsgs)
		{
			Emote(ChatMgr.ListeningRadius, localizedMsgs);
		}

		public virtual void Emote(float radius, string[] localizedMsgs)
		{
			ChatMgr.SendMonsterMessage(this, ChatMsgType.MonsterEmote, ChatLanguage.Universal, localizedMsgs, radius);
		}
		#endregion

		#region Packets
		/// <summary>
		/// Sends a packet to all nearby characters.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		public virtual void SendPacketToArea(RealmPacketOut packet)
		{
			if (IsAreaActive)
			{
				this.IterateEnvironment(BroadcastRange, obj =>
				{
					if (obj is Character)
					{
						((Character)obj).Send(packet.GetFinalizedPacket());
					}
					return true;
				});
			}
		}

		/// <summary>
		/// Sends a packet to all nearby characters.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		/// <param name="radius">the radius of the area to send to</param>
		/// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
		public virtual void SendPacketToArea(RealmPacketOut packet, float radius, bool includeSelf)
		{
			if (radius > 0)
			{
				this.IterateEnvironment(radius, obj =>
				{
					if (obj is Character)
					{
						((Character)obj).Send(packet.GetFinalizedPacket());
					}
					return true;
				});
			}
			else
			{
				for (var i = m_Map.Characters.Count - 1; i >= 0; i--)
				{
					m_Map.Characters[i].Send(packet.GetFinalizedPacket());
				}
			}
		}

		/// <summary>
		/// Sends a packet to all nearby characters.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		/// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
		public virtual void SendPacketToArea(RealmPacketOut packet, bool includeSelf)
		{
			if (IsAreaActive)
			{
				this.IterateEnvironment(BroadcastRange, obj =>
				{
					if (obj is Character)
					{
						((Character)obj).Send(packet.GetFinalizedPacket());
					}
					return true;
				});
			}
		}

		/// <summary>
		/// Sends a manual update field change to all nearby characters.
		/// </summary>
		/// <param name="field">the field to update</param>
		/// <param name="value">the value to update it to</param>
		public void SendFieldUpdateToArea(UpdateFieldId field, uint value)
		{
			if (IsAreaActive)
			{
				using (var packet = GetFieldUpdatePacket(field, value))
				{
					SendPacketToArea(packet);
				}
			}
		}

		/// <summary>
		/// Sends a manual update field refresh to all nearby characters.
		/// </summary>
		/// <param name="field">the field to refresh</param>
		public void SendFieldUpdateToArea(UpdateFieldId field)
		{
			if (IsAreaActive)
			{
				var value = GetUInt32(field.RawId);
				using (var packet = GetFieldUpdatePacket(field, value))
				{
					SendPacketToArea(packet);
				}
			}
		}

		/// <summary>
		/// Sends a manual update field refresh to all nearby characters.
		/// </summary>
		/// <param name="field">the field to refresh</param>
		public void SendFieldUpdateTo(IPacketReceiver rcv, UpdateFieldId field)
		{
			if (IsAreaActive)
			{
				var value = GetUInt32(field.RawId);
				using (var packet = GetFieldUpdatePacket(field, value))
				{
					rcv.Send(packet);
				}
			}
		}
		#endregion

		/// <summary>
		/// Ensures that the given action is always executed in map context of this Character - which
		/// might be right now or after the Character is added to a map or during the next Map update.
		/// </summary>
		/// <returns>Whether the Action has been executed immediately (or enqueued)</returns>
		public bool ExecuteInContext(Action action)
		{
			if (!IsInContext)
			{
				AddMessage(() => action());	// use lambda for now to generate the full stacktrace
				return false;
			}

			action();
			return true;
		}

		public void AddMessage(IMessage msg)
		{
			//var handler = ContextHandler;
			//if (m_map != null)
			//{
			//    handler.AddMessage(msg);
			//}
			//else
			//{
			//    log.Warn("Trying to add message for action \"{0}\"when no ContextHandler is set: " + this, msg);
			//    msg.Execute();
			//}
			m_messageQueue.Enqueue(msg);
		}

		public void AddMessage(Action action)
		{
			m_messageQueue.Enqueue(new Message(action));
		}

		#region Factions/Hostility
		public abstract Faction Faction
		{
			get;
			set;
		}

		public abstract FactionId FactionId
		{
			get;
			set;
		}

		/// <summary>
		/// Checks whether this Unit can currently do any harm (must be alive and not in a sanctuary)
		/// </summary>
		public virtual bool CanDoHarm
		{
			get
			{
				if ((this is Unit) && !((Unit)this).IsAlive)
				{
					return false;
				}

				return true;
				//var zone = m_zone;
				//return zone != null && (zone.Flags & ZoneFlag.Sanctuary) == 0;
			}
		}

		/// <summary>
		/// Checks whether this Object can currently be harmed (must be alive and not in sanctuary)
		/// </summary>
		public bool CanBeHarmed
		{
			get
			{
				return true;
				//return (!(this is Character) || !((Character)this).GodMode) && IsAlive &&
				//    m_zone != null && (m_zone.Flags & ZoneFlag.Sanctuary) == 0;
			}
		}

		/// <summary>
		/// whether this Unit is in a no-combat zone
		/// </summary>
		public bool IsInSanctuary
		{
			get
			{
				return false;
				//return m_zone == null || (m_zone.Flags & ZoneFlag.Sanctuary) != 0;
			}
		}

		/// <summary>
		/// Indicates whether the 2 units are friendly towards each other.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsFriendlyWith(IFactionMember opponent)
		{
			if (HasMaster)
			{
				if (ReferenceEquals(m_master, opponent)) return true;
				return Master.IsFriendlyWith(opponent);
			}
			if ((opponent is WorldObject && ((WorldObject)opponent).HasMaster))
			{
				opponent = ((WorldObject)opponent).Master;
			}
			if (ReferenceEquals(opponent, this))
			{
				return true;
			}
			if (opponent is Character)
			{
				return ((Character)opponent).IsFriendlyWith(this);
			}

			var faction = Faction;
			var opFaction = opponent.Faction;
			if (faction == opponent.Faction)
			{
				return true;
			}

			if (faction != null && opFaction != null)
				return faction.IsFriendlyTowards(opFaction);
			return false;
		}

		/// <summary>
		/// Indicates whether the 2 units are neutral towards each other.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsAtLeastNeutralWith(IFactionMember opponent)
		{
			if (IsFriendlyWith(opponent))
			{
				return true;
			}

			var faction = Faction;
			var opFaction = opponent.Faction;
			if (faction != null && opFaction != null && faction.Neutrals.Contains(opFaction))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Indicates whether the 2 units are hostile towards each other.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsHostileWith(IFactionMember opponent)
		{
			if (HasMaster)
			{
				if (ReferenceEquals(m_master, opponent)) return false;
				return Master.IsHostileWith(opponent);
			}
			if ((opponent is WorldObject && ((WorldObject)opponent).HasMaster))
			{
				opponent = ((WorldObject)opponent).Master;
			}
			if (ReferenceEquals(opponent, this))
			{
				return false;
			}
			if (opponent is Character)
			{
				return ((Character)opponent).IsHostileWith(this);
			}

			var faction = Faction;
			var opFaction = opponent.Faction;
			if (faction == opFaction)
			{
				return false;
			}

			if (faction != null && opFaction != null && faction.Enemies.Contains(opFaction))
			{
				return true;
			}
			return false;
		}

		public virtual bool MayAttack(IFactionMember opponent)
		{
			if (HasMaster)
			{
				if (ReferenceEquals(m_master, opponent)) return false;
				return Master.MayAttack(opponent);
			}
			if ((opponent is WorldObject && ((WorldObject)opponent).HasMaster))
			{
				opponent = ((WorldObject)opponent).Master;
			}
			if (opponent == null)
			{
				return false;
			}
			if (ReferenceEquals(opponent, this))
			{
				return false;
			}
			if (opponent is Character)
			{
				return ((Character)opponent).MayAttack(this);
			}

			var faction = Faction;
			var opFaction = opponent.Faction;
			if (faction == opFaction)
			{
				return false;
			}

			if (faction != null && opFaction != null && faction.Enemies.Contains(opFaction))
			{
				return true;
			}
			return false;
		}

		public virtual bool IsAlliedWith(IFactionMember opponent)
		{
			if (HasMaster)
			{
				if (ReferenceEquals(m_master, opponent)) return true;
				return Master.IsAlliedWith(opponent);
			}
			if ((opponent is WorldObject && ((WorldObject)opponent).HasMaster))
			{
				opponent = ((WorldObject)opponent).Master;
			}
			if (ReferenceEquals(opponent, this))
			{
				return true;
			}

			if (opponent is Character)
			{
				return ((Character)opponent).IsAlliedWith(this);
			}

			var faction = Faction;
			var opFaction = opponent.Faction;
			if (faction == opponent.Faction)
			{
				return true;
			}
			if (faction != null && opponent.Faction != null)
				return faction.Friends.Contains(opFaction);
			return false;
		}

		public virtual bool IsInSameDivision(IFactionMember opponent)
		{
			if (opponent is Character)
			{
				return ((Character)opponent).IsInSameDivision(this);
			}
			return IsAlliedWith(opponent);
		}

		/// <summary>
		/// Indicates whether we can currently do any harm and are allowed to attack
		/// the given opponent (hostile or neutral factions, duel partners etc)
		/// </summary>
		public virtual bool CanHarm(WorldObject opponent)
		{
			return CanDoHarm && MayAttack(opponent);
		}
		#endregion

		#region AreaAuras
		/// <summary>
		/// The set of currently active AreaAuras or null.
		/// Do not modify the list.
		/// </summary>
		public List<AreaAura> AreaAuras
		{
			get { return m_areaAuras; }
		}

		public bool HasAreaAuras
		{
			get { return m_areaAuras != null && m_areaAuras.Count > 0; }
		}

		/// <summary>
		/// Called when AreaAura is created
		/// </summary>
		internal void AddAreaAura(AreaAura aura)
		{
			if (m_areaAuras == null)
			{
				m_areaAuras = new List<AreaAura>(2);
			}
			else if (aura.Spell.AttributesExB.HasFlag(SpellAttributesExB.ExclusiveAreaAura))
			{
				// cannot be applied with other AreaAuras of that type
				foreach (var aaura in m_areaAuras)
				{
					if (aura.Spell.AttributesExB.HasFlag(SpellAttributesExB.ExclusiveAreaAura))
					{
						aaura.Remove(true);
						break;
					}
				}
			}
			m_areaAuras.Add(aura);
		}

		/// <summary>
		/// Returns the first AreaAura of the given spell
		/// </summary>
		public AreaAura GetAreaAura(Spell spell)
		{
			if (m_areaAuras == null)
			{
				return null;
			}
			return m_areaAuras.FirstOrDefault(aura => aura.Spell == spell);
		}

		/// <summary>
		/// Cancels the first <see cref="AreaAura"/> of the given spell
		/// </summary>
		/// <returns>Whether it found & removed one</returns>
		public bool CancelAreaAura(Spell spell)
		{
			var aura = GetAreaAura(spell);
			if (aura != null)
			{
				return CancelAreaAura(aura);
			}
			return false;
		}

		/// <summary>
		/// Called by AreaAura.Remove
		/// </summary>
		internal bool CancelAreaAura(AreaAura aura)
		{
			if (m_areaAuras == null)
			{
				return false;
			}

			if (m_areaAuras.Remove(aura))
			{
				if (this is Unit)
				{
					((Unit)this).Auras.Remove(aura.Spell);
				}
				else if (m_areaAuras.Count == 0 && (IsTrap || this is DynamicObject))
				{
					// remove trap & dynamic object when aura gets removed
					Delete();
				}
				return true;
			}
			return false;
		}

		#endregion

		/// <summary>
		/// Indicates whether this Object can see the other object
		/// </summary>
		public virtual bool CanSee(WorldObject obj)
		{
			return IsInPhase(obj);
		}

		/// <summary>
		/// Visibility of this object in the eyes of the given observer.
		/// Can be used to override default visibility checks
		/// </summary>
		public virtual VisibilityStatus DetermineVisibilityFor(Unit observer)
		{
			return VisibilityStatus.Default;
		}

		public virtual bool IsPlayer
		{
			get { return false; }
		}

		/// <summary>
		/// Whether this or it's master is a player
		/// </summary>
		public bool IsPlayerOwned
		{
			get { return IsPlayer || (m_master != null && m_master.IsPlayer); }
		}

		/// <summary>
		/// Whether this object's master is a player
		/// </summary>
		public bool HasPlayerMaster
		{
			get { return m_master != null && m_master.IsPlayer; }
		}

		public Character PlayerOwner
		{
			get { return this is Character ? (Character)this : m_master as Character; }
		}

		/// <summary>
		/// Whether this is actively controlled by a player. 
		/// Not to be confused with IsOwnedByPlayer.
		/// </summary>
		public virtual bool IsPlayerControlled
		{
			get { return false; }
		}

		/// <summary>
		/// Grow a bit and then become small again
		/// </summary>
		public void Highlight()
		{
			var diff = ((HighlightScale - 1) * ScaleX);
			ScaleX = HighlightScale * ScaleX;
			CallDelayed(HighlightDelayMillis, obj => obj.ScaleX -= diff);
		}

		public void PlaySound(uint sound)
		{
			MiscHandler.SendPlayObjectSound(this, sound);
		}

		/// <summary>
		/// TODO: Find a better way to identify texts (i.e. by entry/object-type ids and sequence number)
		/// </summary>
		public void PlayTextAndSoundByEnglishPrefix(string englishPrefix)
		{
			var text = NPCAiTextMgr.GetFirstTextByEnglishPrefix(englishPrefix);
			if (text != null)
			{
				PlayTextAndSound(text);
			}
		}

		/// <summary>
		/// Play a text and sound identify by the id
		/// </summary>
		/// <param name="id">Id of the text in creature_ai_texts</param>
		public void PlayTextAndSoundById(int id)
		{
			var text = NPCAiTextMgr.GetFirstTextById(id);
			if (text != null)
			{
				PlayTextAndSound(text);
			}
		}

		public void PlayTextAndSound(NPCAiText text)
		{
			PlaySound((uint)text.Sound);
			Yell(text.Texts);
		}

		#region Deletion & Disposal
		protected bool m_Deleted;

		/// <summary>
		/// Deleted objects must never be used again!
		/// </summary>
		public bool IsDeleted
		{
			get { return m_Deleted; }
		}

		/// <summary>
		/// Enqueues a message to remove this WorldObject from it's Map
		/// </summary>
		public void RemoveFromMap()
		{
			if (IsInWorld)
			{
				m_Map.RemoveObjectLater(this);
			}
		}

		/// <summary>
		/// Enqueues a message to remove this Object from the World and dispose it.
		/// </summary>
		public virtual void Delete()
		{
			if (m_Deleted)
			{
				return;
			}

			m_Deleted = true;
			if (m_Map != null)
			{
				m_Map.AddMessage(() => DeleteNow()); // use lambda for now to generate the full stacktrace
			}
			else
			{
				DeleteNow();
			}
		}

		/// <summary>
		/// Removes this Object from the World and disposes it.
		/// </summary>
		/// <see cref="Delete"/>
		/// <remarks>Requires map context</remarks>
		protected internal virtual void DeleteNow()
		{
			try
			{
				m_Deleted = true;
				OnDeleted();

				Dispose();
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to correctly delete object \"{0}\"", this), e);
			}
		}

		protected void OnDeleted()
		{
			m_updateActions = null;

			if (m_Map != null)
			{
				if (m_areaAuras != null)
				{
					foreach (var aura in m_areaAuras.ToArray())
					{
						aura.Remove(true);
					}
				}
				m_Map.RemoveObjectNow(this);
			}
		}

		public override void Dispose(bool disposing)
		{
			SpellCast = null;
			m_Map = null;
		}
		#endregion

		public override string ToString()
		{
			return Name + " (" + EntityId + ")";
		}
	}
}