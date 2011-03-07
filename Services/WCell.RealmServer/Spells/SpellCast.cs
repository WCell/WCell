/*************************************************************************
 *
 *   file		    : SpellCast.cs
 *   copyright		: (C) The WCell Team
 *   email		    : info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-25 12:57:15 +0100 (to, 25 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1260 $
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
using Cell.Core;
using NLog;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.NLog;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents the progress of any Spell-casting
	/// </summary>
	public partial class SpellCast : IUpdatable, IWorldLocation
	{
		public static int PushbackDelay = 500;
		public static int ChannelPushbackFraction = 4;

		internal static readonly ObjectPool<SpellCast> SpellCastPool = ObjectPoolMgr.CreatePool(() => new SpellCast(), true);
		public static readonly ObjectPool<List<IAura>> AuraListPool = ObjectPoolMgr.CreatePool(() => new List<IAura>(), true);
		public static readonly ObjectPool<List<CastMiss>> CastMissListPool = ObjectPoolMgr.CreatePool(() => new List<CastMiss>(3), true);
		public static readonly ObjectPool<List<SpellEffectHandler>> SpellEffectHandlerListPool = ObjectPoolMgr.CreatePool(() => new List<SpellEffectHandler>(3), true);
		//internal static readonly ObjectPool<List<AuraApplicationInfo>> AuraAppListPool = ObjectPoolMgr.CreatePool(() => new List<AuraApplicationInfo>());

		public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc)
		{
			Trigger(caster, spell, ref targetLoc, null);
		}

		public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected)
		{
			Trigger(caster, spell, ref targetLoc, selected, null);
		}

		public static void Trigger(WorldObject caster, Spell spell, ref Vector3 targetLoc, WorldObject selected, Item casterItem)
		{
			var cast = ObtainPooledCast(caster);
			cast.TargetLoc = targetLoc;
			cast.Selected = selected;
			cast.CasterItem = casterItem;

			cast.ExecuteInContext(() =>
			{
				cast.Start(spell, true);
				//cast.Dispose();
			});
		}

		public static SpellCast ObtainPooledCast(WorldObject caster)
		{
			var cast = SpellCastPool.Obtain();
			cast.SetCaster(caster);
			return cast;
		}

		public static SpellCast ObtainPooledCast(ObjectReference caster, Map map, uint phase, ref Vector3 sourceLoc)
		{
			var cast = SpellCastPool.Obtain();
			cast.SetCaster(caster, map, phase, sourceLoc);
			return cast;
		}

		#region Fields
		Spell m_spell;
		private int m_castDelay;
		private int m_startTime;

		public ObjectReference CasterReference
		{
			get;
			internal set;
		}

		/// <summary>
		/// The caster himself or owner of the casting Item or GameObject
		/// </summary>
		public Unit CasterUnit
		{
			get;
			private set;
		}

		/// <summary>
		/// The Unit or GameObject (traps etc), triggering this spell
		/// </summary>
		public WorldObject CasterObject
		{
			get;
			private set;
		}

		/// <summary>
		/// The map where the SpellCast happens
		/// </summary>
		public Map Map
		{
			get;
			internal set;
		}

		public uint Phase
		{
			get;
			internal set;
		}

		/// <summary>
		/// Needed for IWorldLocation interface
		/// </summary>
		public Vector3 Position
		{
			get { return SourceLoc; }
		}

		/// <summary>
		/// Needed for IWorldLocation interface
		/// </summary>
		public MapId MapId
		{
			get { return Map.MapId; }
		}

		/// <summary>
		/// The context to which the SpellCast belongs
		/// </summary>
		public IContextHandler Context
		{
			get { return Map; }
		}

		/// <summary>
		/// An Item that this Spell is being used on
		/// </summary>
		public Item TargetItem;

		/// <summary>
		/// Any kind of item that was used to trigger this cast
		/// (trinkets, potions, food etc.)
		/// </summary>
		public Item CasterItem;

		/// <summary>
		/// Something that has been selected by the Caster for this Spell
		/// </summary>
		public WorldObject Selected;

		/// <summary>
		/// The target location for a spell which has been sent by the player
		/// </summary>
		public Vector3 TargetLoc;

		public Map TargetMap
		{
			get
			{
				Map rgn;
				if (m_spell.TargetLocation != null)
				{
					rgn = m_spell.TargetLocation.Map ?? Map;
				}
				else
				{
					rgn = Map;
				}
				return rgn;
			}
		}

		public float TargetOrientation
		{
			get
			{
				if (m_spell.TargetLocation != null || CasterObject == null)
					return m_spell.TargetOrientation;
				return CasterObject.Orientation;
			}
		}

		/// <summary>
		/// The source location for a spell which has been sent by the player
		/// </summary>
		public Vector3 SourceLoc;


		public string StringTarget;

		/// <summary>
		/// All SpellEffectHandlers that are applied during this spell-cast
		/// </summary>
		SpellEffectHandler[] m_handlers;

		// targets
		WorldObject[] m_initialTargets;

		private HashSet<WorldObject> m_targets;
		private List<AuraApplicationInfo> m_auraApplicationInfos;

		public int TargetCount
		{
			get;
			private set;
		}

		/// <summary>
		/// whether this is a passive cast (probably triggered by another one)
		/// </summary>
		bool m_passiveCast;

		internal TimerEntry m_castTimer;

		/// <summary>
		/// A SpellChannel that might or might not be open(ed by this SpellCast)
		/// </summary>
		SpellChannel m_channel;

		/// <summary>
		/// The amount of Pushbacks (the more Pushbacks, the less effective they are)
		/// </summary>
		int m_pushbacks;

		bool m_casting;

		/// <summary>
		/// Sent by the caster when initializing a spell.
		/// Can be ignored for spells not casted by players.
		/// </summary>
		public byte Id;

		private bool isPlayerCast;

		public SpellTargetFlags TargetFlags;
		#endregion

		/// <summary>
		/// Creates a recyclable SpellCast.
		/// </summary>
		/// <param name="caster">The GameObject (in case of traps etc) or Unit casting</param>
		private SpellCast()
		{
			m_castTimer = new TimerEntry(Perform);
		}

		/// <summary>
		/// Creates a new SpellCast for the given caster
		/// </summary>
		/// <param name="caster">The GameObject (in case of traps etc) or Unit casting</param>
		internal SpellCast(WorldObject caster)
		{
			SetCaster(caster);

			m_castTimer = new TimerEntry(Perform);
		}

		void SetCaster(ObjectReference caster, Map map, uint phase, Vector3 sourceLoc)
		{
			CasterReference = caster;
			if (caster == null)
			{
				throw new ArgumentNullException("caster");
			}
			CasterObject = caster.Object;
			CasterUnit = caster.UnitMaster;
			Map = map;
			Phase = phase;
			SourceLoc = sourceLoc;
		}

		void SetCaster(WorldObject caster)
		{
			CasterReference = caster.SharedReference;
			CasterObject = caster;
			CasterUnit = caster.UnitMaster;
			Map = caster.Map;
			Phase = caster.Phase;
		}

		#region Properties
		/// <summary>
		/// 
		/// </summary>
		public Spell Spell
		{
			get { return m_spell; }
		}

		/// <summary>
		/// The caster himself or owner of the casting Item or GameObject
		/// </summary>
		public Character CasterChar
		{
			get { return CasterUnit as Character; }
		}

		/// <summary>
		/// Whether the SpellCast was started by a Player
		/// </summary>
		public bool IsPlayerCast
		{
			get { return isPlayerCast; }
		}

		/// <summary>
		/// Whether the SpellCast was started by an AI-controlled Unit
		/// </summary>
		public bool IsAICast
		{
			get { return !isPlayerCast && !IsPassive && (CasterUnit == null || !CasterUnit.IsPlayer); }
		}

		public bool UsesRunes
		{
			get { return m_spell.RuneCostEntry != null && CasterChar != null && CasterChar.PlayerSpells.Runes != null; }
		}

		public CastFlags StartFlags
		{
			get
			{
				var flags = CastFlags.None;
				if (m_spell != null)
				{
					if (m_spell.IsRangedAbility)
					{
						flags |= CastFlags.Ranged;
					}
					if (UsesRunes)
					{
						flags |= CastFlags.RuneAbility;
					}
				}
				return flags;
			}
		}

		/// <summary>
		/// This corresponds to the actual level of Units
		/// and for GOs returns the level of the owner.
		/// </summary>
		public int CasterLevel
		{
			get { return CasterReference.Level; }
		}

		public CastFlags GoFlags
		{
			get
			{
				var flags = CastFlags.Flag_0x2;
				if (m_spell.IsRangedAbility)
				{
					flags |= CastFlags.Ranged;
				}
				if (UsesRunes)
				{
					flags |= CastFlags.RuneAbility;
					if (m_spell.RuneCostEntry.RunicPowerGain > 0)
					{
						flags |= CastFlags.RunicPowerGain;
					}
					if (m_spell.RuneCostEntry.CostsRunes)
					{
						flags |= CastFlags.RuneCooldownList;
					}
				}

				// TODO: If Ghost - Aura gets applied, add more flags?
				//if (missedTargets) {
				//    flags |= CastGoFlags.Notify;
				//}
				return flags;
			}
		}

		/// <summary>
		/// The Caster's or Caster's Master's Client (or null)
		/// </summary>
		public IRealmClient Client
		{
			get
			{
				var chr = CasterUnit as Character;

				if (chr != null)
				{
					return chr.Client;
				}
				return null;
			}
		}

		/// <summary>
		/// An object representing the channeling of a spell (any spell that is performed over a period of time)
		/// </summary>
		public SpellChannel Channel
		{
			get { return m_channel; }
		}

		/// <summary>
		/// The time in milliseconds between now and the actual casting (meaningless if smaller equal 0).
		/// Can be changed. Might return bogus numbers if not casting.
		/// </summary>
		public int RemainingCastTime
		{
			get
			{
				return CastDelay + m_startTime - Environment.TickCount;
			}
			set
			{
				var delta = Math.Max(0, value - RemainingCastTime);

				m_startTime = Environment.TickCount + delta;
				m_castTimer.RemainingInitialDelayMillis = value;

				SpellHandler.SendCastDelayed(this, delta);
			}
		}

		/// <summary>
		/// Time in milliseconds that it takes until the spell will start (0 if GodMode)
		/// </summary>
		public int CastDelay
		{
			get { return GodMode ? 1 : m_castDelay; }
		}

		/// <summary>
		/// The time at which the cast started (in millis since system start)
		/// </summary>
		public int StartTime
		{
			get { return m_startTime; }
		}

		/// <summary>
		/// whether the cast is currently being performed
		/// </summary>
		public bool IsCasting
		{
			get { return m_casting; }
		}

		/// <summary>
		/// whether the caster is currently channeling a spell
		/// </summary>
		public bool IsChanneling
		{
			get { return m_channel != null && m_channel.IsChanneling; }
		}

		/// <summary>
		/// Whether this SpellCast is waiting to be casted on next strike
		/// </summary>
		public bool IsPending
		{
			get { return m_casting && m_spell.IsOnNextStrike; }
		}

		/// <summary>
		/// Returns all targets that this SpellCast initially had
		/// </summary>
		public WorldObject[] InitialTargets
		{
			get { return m_initialTargets; }
		}

		/// <summary>
		/// All SpellEffectHandlers
		/// </summary>
		public SpellEffectHandler[] Handlers
		{
			get { return m_handlers; }
		}

		/// <summary>
		/// All SpellEffectHandlers
		/// </summary>
		public HashSet<WorldObject> Targets
		{
			get { return m_targets; }
		}

		/// <summary>
		/// Returns false if Player actively casted the spell, else true.
		/// Passive SpellCasts wont do any of the requirement checks.
		/// </summary>
		public bool IsPassive
		{
			get { return m_passiveCast; }
		}

		/// <summary>
		/// Ignore most limitations
		/// </summary>
		public bool GodMode
		{
			get;
			set;
		}

		public bool IsInstant
		{
			get { return m_passiveCast || GodMode || m_castDelay < 100; }
		}

		public bool IsAoE
		{
			get { return TriggerEffect != null ? TriggerEffect.IsAreaEffect : m_spell.IsAreaSpell; }
			//get { return m_spell.IsAreaSpell; }
		}

		/// <summary>
		/// The SpellEffect that triggered this cast (or null if not triggered)
		/// </summary>
		public SpellEffect TriggerEffect
		{
			get;
			private set;
		}

		/// <summary>
		/// The action that triggered this SpellCast, if any.
		/// If you want to save the Action for a point later in time, you need to
		/// increment the ReferenceCount, and decrement it when you are done with it.
		/// </summary>
		public IUnitAction TriggerAction
		{
			get;
			private set;
		}
		#endregion

		public SpellEffectHandler GetHandler(SpellEffectType type)
		{
			if (m_handlers == null)
			{
				throw new InvalidOperationException("Tried to get Handler from unintialized SpellCast");
			}

			foreach (var handler in m_handlers)
			{
				if (handler.Effect.EffectType == type)
				{
					return handler;
				}
			}
			return null;
		}

		#region Start & Prepare
		public SpellFailedReason Start(SpellId spell, bool passiveCast)
		{
			return Start(SpellHandler.Get(spell), passiveCast);
		}

		public SpellFailedReason Start(Spell spell, bool passiveCast)
		{
			return Start(spell, passiveCast, (WorldObject[])null);
		}

		public SpellFailedReason Start(SpellId spellId, bool passiveCast, params WorldObject[] targets)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell == null)
			{
				LogManager.GetCurrentClassLogger().Warn("{0} tried to cast non-existant Spell: {1}", CasterObject, spellId);
				return SpellFailedReason.DontReport;
			}
			return Start(spell, passiveCast, targets);
		}

		public SpellFailedReason Start(Spell spell, bool passiveCast, WorldObject singleTarget)
		{
			var targets = new[] { singleTarget };
			return Start(spell, passiveCast, targets);
		}

		/// <summary>
		/// This starts a spell-cast, requested by the client.
		/// The client submits where or what the user selected in the packet.
		/// </summary>
		internal SpellFailedReason Start(Spell spell, RealmPacketIn packet, byte castId, byte unkFlags)
		{
			isPlayerCast = true;

			//var stopwatch = Stopwatch.StartNew();

			if (IsCasting)
			{
				if (!IsChanneling)
				{
					SpellHandler.SendCastFailed(Client, castId, spell, SpellFailedReason.SpellInProgress);
					return SpellFailedReason.SpellInProgress;
				}
				else
				{
					Cancel(SpellFailedReason.DontReport);
				}
			}

			Map = CasterObject.Map;
			Phase = CasterObject.Phase;

			m_casting = true;
			m_spell = spell;
			Id = castId;

			//byte unkFlag = packet.ReadByte();

			// TODO: Corpse flags
			//(targetFlags & SpellCastTargetFlags.Corpse) != 0 || 
			//    (targetFlags & SpellCastTargetFlags.ReleasedCorpse) != 0) {

			// read the target-information, sent with the Spell
			WorldObject selected = null;
			bool targetFound = false;
			TargetFlags = (SpellTargetFlags)packet.ReadUInt32();

			// 0x0
			if (TargetFlags == SpellTargetFlags.Self)
			{
				targetFound = true;
				TargetLoc = CasterObject.Position;
				selected = CasterObject;
			}
			// 0x18A02
			if (TargetFlags.HasAnyFlag(
				SpellTargetFlags.SpellTargetFlag_Dynamic_0x10000 |
				SpellTargetFlags.Corpse |
				SpellTargetFlags.Object |
				SpellTargetFlags.PvPCorpse |
				SpellTargetFlags.Unit))
			{
				// The user selected an Object
				var uid = packet.ReadPackedEntityId();
				selected = Map.GetObject(uid);

				if (selected == null || !CasterObject.CanSee(selected))
				{
					Cancel(SpellFailedReason.BadTargets);
					return SpellFailedReason.BadTargets;
				}

				targetFound = true;
				TargetLoc = selected.Position;
			}
			// 0x1010
			if (CasterObject is Character && TargetFlags.HasAnyFlag(SpellTargetFlags.TradeItem | SpellTargetFlags.Item))
			{
				var uid = packet.ReadPackedEntityId();
				TargetItem = ((Character)CasterObject).Inventory.GetItem(uid);
				if (TargetItem == null || !TargetItem.CanBeUsed)
				{
					Cancel(SpellFailedReason.ItemGone);
					return SpellFailedReason.ItemGone;
				}
			}
			// 0x20
			if (TargetFlags.HasAnyFlag(SpellTargetFlags.SourceLocation))
			{
				Map.GetObject(packet.ReadPackedEntityId());		// since 3.2.0
				//SourceLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
			}

			SourceLoc = CasterObject.Position;

			// 0x40
			if (TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
			{
				selected = Map.GetObject(packet.ReadPackedEntityId());
				TargetLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
				//Console.WriteLine("SpellCast.Start - DestLoc {0}", TargetLoc);
				targetFound = true;
			}
			// 0x2000
			if (TargetFlags.HasAnyFlag(SpellTargetFlags.String))
			{
				StringTarget = packet.ReadCString();
			}

			if ((unkFlags & 2) != 0)
			{
				float f1 = packet.ReadFloat();
				float f2 = packet.ReadFloat();
				byte b1 = packet.ReadByte();
				// here the client appends a MSG_MOVE_STOP movement packet
			}

			// special cast handler: Interrupt casting and call the handler instead
			// for Spell-overrides through Addons
			if (spell.SpecialCast != null)
			{
				spell.SpecialCast(spell, CasterObject, selected, ref TargetLoc);
				Cancel(SpellFailedReason.DontReport);
				return SpellFailedReason.DontReport;
			}

			if (targetFound)
			{
				// default checks
				if (selected != CasterObject)
				{
					if (CasterObject is Character)
					{
						// check range
						var chr = CasterObject as Character;
						var sqDistance = CasterObject.GetDistanceSq(ref TargetLoc);
						if (!Utility.IsInRange(sqDistance, chr.GetSpellMaxRange(spell, selected)) ||
							(selected != null && selected.Map != CasterObject.Map))
						{
							Cancel(SpellFailedReason.OutOfRange);
							return SpellFailedReason.OutOfRange;
						}
						if (Utility.IsInRange(sqDistance, spell.Range.MinDist))
						{
							Cancel(SpellFailedReason.TooClose);
							return SpellFailedReason.TooClose;
						}
					}

					if (selected != null && !selected.IsInWorld)
					{
						Cancel(SpellFailedReason.OutOfRange);
						return SpellFailedReason.OutOfRange;
					}
				}

				Selected = selected;
			}

			if (spell.RequiredTargetId != 0)
			{
				// check for custom Targets
				if (Selected == null || Selected.EntryId != spell.RequiredTargetId || !spell.MatchesRequiredTargetType(Selected))
				{
					Cancel(SpellFailedReason.BadTargets);
					return SpellFailedReason.BadTargets;
				}
			}

			//if (selected == null && Caster is Unit)
			//{
			//    Selected = ((Unit)Caster).Target;
			//}

			var reason = Prepare();

			if (reason == SpellFailedReason.Ok)
			{
				return FinishPrepare();
			}
			//if (CasterChar != null)
			//{
			//    CasterChar.SendSystemMessage("SpellCast (Total): {0} ms", stopwatch.ElapsedTicks / 10000d);
			//}
			return reason;
		}

		/// <summary>
		/// Starts casting the given spell.
		/// if <code>GodMode</code> is set or the spell is not delayed.
		/// Returns whether, under the given circumstances, this spell may be casted.
		/// </summary>
		/// <param name="passiveCast">whether the Spell is a simple spell-application or an actual spell-cast</param>
		/// <param name="initialTargets">A collection of initial targets or null.</param>
		public SpellFailedReason Start(Spell spell, SpellEffect triggerEffect, bool passiveCast, params WorldObject[] initialTargets)
		{
			TriggerEffect = triggerEffect;
			return Start(spell, passiveCast, initialTargets);
		}

		/// <summary>
		/// Starts casting the given spell.
		/// if <code>GodMode</code> is set or the spell is not delayed.
		/// Returns whether, under the given circumstances, this spell may be casted.
		/// </summary>
		public SpellFailedReason Start(Spell spell)
		{
			return Start(spell, false, WorldObject.EmptyArray);
		}

		/// <summary>
		/// Starts casting the given spell.
		/// if <code>GodMode</code> is set or the spell is not delayed.
		/// Returns whether, under the given circumstances, this spell may be casted.
		/// </summary>
		public SpellFailedReason Start(SpellId spell)
		{
			return Start(spell, false, WorldObject.EmptyArray);
		}

		/// <summary>
		/// Starts casting the given spell.
		/// if <code>GodMode</code> is set or the spell is not delayed.
		/// Returns whether, under the given circumstances, this spell may be casted.
		/// </summary>
		/// <param name="passiveCast">whether the Spell is a simple spell-application or an actual spell-cast</param>
		/// <param name="initialTargets">A collection of initial targets or null.</param>
		public SpellFailedReason Start(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
		{
			if (m_casting || IsChanneling)
			{
				// Make sure that we are not still busy
				Cancel();
			}
			m_casting = true;

			m_spell = spell;
			m_passiveCast = passiveCast;
			if (initialTargets == null || initialTargets.Length == 0)
			{
				m_initialTargets = null;
			}
			else
			{
				m_initialTargets = initialTargets;
			}

			var reason = Prepare();
			if (reason == SpellFailedReason.Ok)
			{
				return FinishPrepare();
			}
			return reason;
		}

		/// <summary>
		/// Use this method to change the SpellCast object after it has been prepared.
		/// If no changes are necessary, simply use <see cref="Start(Spell, bool, WorldObject[])"/>
		/// </summary>
		public SpellFailedReason Prepare(Spell spell, bool passiveCast, params WorldObject[] initialTargets)
		{
			if (m_casting || IsChanneling)
			{
				// Make sure that we are not still busy
				Cancel();
			}
			m_casting = true;

			m_spell = spell;
			m_passiveCast = passiveCast;
			m_initialTargets = initialTargets;

			var failReason = Prepare();

			if (failReason == SpellFailedReason.Ok)
			{
				failReason = InitHandlers();
				if (failReason != SpellFailedReason.Ok)
				{
					Cancel(failReason);
				}
			}
			return failReason;
		}

		private SpellFailedReason Prepare()
		{
			if (m_spell == null)
			{
				LogManager.GetCurrentClassLogger().Warn("{0} tried to cast without selecting a Spell.", CasterObject);
				return SpellFailedReason.Error;
			}

			try
			{
				// Let AI caster prepare targets and only cast, if valid targets were found
				if (IsAICast)
				{
					var err = PrepareAI();
					if (err != SpellFailedReason.Ok)
					{
						Cancel(err);
						return err;
					}
				}

				//var stopwatch = Stopwatch.StartNew();
				if (Selected == null && CasterUnit != null)
				{
					if (m_initialTargets != null)
					{
						Selected = m_initialTargets[0];
					}
					else
					{
						Selected = CasterUnit.Target;
					}
				}

				if (!m_passiveCast && !m_spell.IsPassive && CasterUnit != null)
				{
					var spell = m_spell;

					if (!spell.Attributes.HasFlag(SpellAttributes.CastableWhileMounted))
					{
						// don't sit on a ride (even if you try to, the Client will show you dismounted - maybe add auto-remount for GodMode)
						CasterUnit.Dismount();
					}

					// make sure, the Caster is standing
					if (!spell.Attributes.HasFlag(SpellAttributes.CastableWhileSitting))
					{
						CasterUnit.StandState = StandState.Stand;
					}

					if (!GodMode && !m_passiveCast && CasterUnit.IsPlayer)
					{
						// check whether we may cast at all for Characters (NPC check before casting)
						var failReason = CheckPlayerCast(Selected);
						if (failReason != SpellFailedReason.Ok)
						{
							Cancel(failReason);
							return failReason;
						}
					}

					// remove certain Auras
					CasterUnit.Auras.RemoveByFlag(AuraInterruptFlags.OnCast);
				}

				m_startTime = Environment.TickCount;
				m_castDelay = (int)m_spell.CastDelay;

				if (!IsInstant)
				{
					// calc exact cast delay
					if (CasterUnit != null)
					{
						m_castDelay = MathUtil.RoundInt(CasterUnit.CastSpeedFactor * m_castDelay);
						m_castDelay = CasterUnit.Auras.GetModifiedInt(SpellModifierType.CastTime, m_spell, m_castDelay);
					}
				}

				if (m_spell.TargetLocation != null)
				{
					TargetLoc = m_spell.TargetLocation.Position;
				}

				// Notify that we are about to cast
				return m_spell.NotifyCasting(this);
			}
			catch (Exception e)
			{
				OnException(e);
				return SpellFailedReason.Error;
			}
		}

		SpellFailedReason FinishPrepare()
		{
			try
			{
				if (!IsInstant)
				{
					// put away weapon and send Start packet
					if (CasterObject is Unit)
					{
						((Unit)CasterObject).SheathType = SheathType.None;
					}
					SendCastStart();
					m_castTimer.Start(m_castDelay);
					return SpellFailedReason.Ok;
				}

				if (m_spell.IsOnNextStrike)
				{
					// perform on next strike
					if (!(CasterObject is Unit))
					{
						Cancel();
						return SpellFailedReason.Error;
					}

					CasterUnit.SetSpellCast(this);

					// send Start packet
					//SendCastStart();
					return SpellFailedReason.Ok;

				}
				else
				{
					return Perform();
				}
			}
			catch (Exception e)
			{
				OnException(e);
				return SpellFailedReason.Error;
			}
		}

		/// <summary>
		/// Is sent in either of 3 cases:
		/// 1. At the beginning of a Cast of a normal Spell that is not instant
		/// 2. After the last check if its instant and not a weapon ability
		/// 3. On Strike if its a weapon ability
		/// </summary>
		internal void SendCastStart()
		{
			if (m_spell.ShouldShowToClient())
			{
				SpellHandler.SendCastStart(this);
			}
		}

		internal void CheckHitAndSendSpellGo(bool revalidateTargets, byte previousRuneMask)
		{
			List<CastMiss> missedTargets;
			if (revalidateTargets)
			{
				// check whether targets were hit
				missedTargets = CheckHit(m_spell);
			}
			else
			{
				missedTargets = null;
			}

			if (!m_spell.IsPassive && !m_spell.Attributes.HasAnyFlag(SpellAttributes.InvisibleAura) &&
				!m_spell.HasEffectWith(effect => effect.EffectType == SpellEffectType.OpenLock) &&
				m_spell.ShouldShowToClient())
			{
				// send the packet (so client sees the actual cast) if its not a passive spell
				var caster2 = CasterItem ?? (IEntity)CasterReference;
				SpellHandler.SendSpellGo(caster2, this, m_targets, missedTargets, previousRuneMask);
			}

			if (missedTargets != null)
			{
				missedTargets.Clear();
				CastMissListPool.Recycle(missedTargets);
			}
		}
		#endregion

		#region Constraints Checks
		/// <summary>
		/// Checks the current Cast when Players are using it
		/// </summary>
		protected SpellFailedReason CheckPlayerCast(WorldObject selected)
		{
			var caster = (Character)CasterUnit;
			if (m_spell.TargetFlags != 0 && !IsAoE && selected == caster)
			{
				// Caster is selected by default
				return SpellFailedReason.NoValidTargets;
			}

			// target specific settings			
			if (!IsAoE && caster != selected && selected != null)
			{
				// Target must be selected and in front of us
				if (caster.IsPlayer && !selected.IsInFrontOf(caster))
				{
					// NPCs always face the Target
					return SpellFailedReason.UnitNotInfront;
				}

				if (m_spell.HasHarmfulEffects && selected is Unit)
				{
					if (((Unit)selected).IsEvading || ((Unit)selected).IsInvulnerable)
					{
						return SpellFailedReason.TargetAurastate;
					}
				}
				//else if (Caster is Unit && !Caster.IsInFrontOfThis(TargetLoc))				
				//{				
				//    ((Unit)Caster).Face(TargetLoc);				
				//}			
			}
			// we can't cast if we are dead
			else if (!caster.IsAlive)
			{
				return SpellFailedReason.CasterDead;
			}

			var err = m_spell.CheckCasterConstraints(caster);
			if (err != SpellFailedReason.Ok)
			{
				return err;
			}

			// cancel looting
			caster.CancelLooting();

			// check required skill
			if (m_spell.Ability != null && m_spell.Ability.RedValue > 0 &&
				caster.Skills.GetValue(m_spell.Ability.Skill.Id) < m_spell.Ability.RedValue)
			{
				return SpellFailedReason.MinSkill;
			}

			// Taming needs some extra checks
			if (m_spell.IsTame)
			{
				var target = selected as NPC;
				if (target == null)
				{
					return SpellFailedReason.BadTargets;
				}

				if (target.CurrentTamer != null)
				{
					return SpellFailedReason.AlreadyBeingTamed;
				}

				if (CheckTame(caster, target) != TameFailReason.Ok)
				{
					return SpellFailedReason.DontReport;
				}
			}

			if (!caster.HasEnoughPowerToCast(m_spell, null) ||
				(UsesRunes && !caster.PlayerSpells.Runes.HasEnoughRunes(Spell)))
			{
				return SpellFailedReason.NoPower;
			}

			// Item restrictions			
			return m_spell.CheckItemRestrictions(TargetItem, caster.Inventory);
		}

		List<CastMiss> CheckHit(Spell spell)
		{
			if (spell.HasHarmfulEffects && !m_passiveCast && !GodMode)
			{
				var missedTargets = CastMissListPool.Obtain();
				m_targets.RemoveWhere(target =>
				{
					if (!target.IsInWorld)
					{
						for (var i = 0; i < m_handlers.Length; i++)
						{
							var handler = m_handlers[i];
							handler.m_targets.Remove(target);
						}
						return true;
					}

					if (target is Unit)
					{
						var missReason = CheckCastHit((Unit)target, spell);
						if (missReason != CastMissReason.None)
						{
							// missed
							missedTargets.Add(new CastMiss(target, missReason));

							// remove missed target from SpellEffectHandlers' target lists
							for (var i = 0; i < m_handlers.Length; i++)
							{
								var handler = m_handlers[i];
								handler.m_targets.Remove(target);
							}
							return true;
						}
					}
					return false;
				});
				return missedTargets;
			}
			return null;
		}

		/// <summary>
		/// Checks the current SpellCast parameters for whether taming the Selected NPC is legal.
		/// Sends the TameFailure packet if it didn't work
		/// </summary>
		public static TameFailReason CheckTame(Character caster, NPC target)
		{
			var failReason = TameFailReason.Ok;
			if (!target.IsAlive)
			{
				failReason = TameFailReason.TargetDead;
			}
			else if (!target.Entry.IsTamable)
			{
				failReason = TameFailReason.NotTamable;
			}
			else if (target.Entry.IsExoticPet && !caster.CanControlExoticPets)
			{
				failReason = TameFailReason.CantControlExotic;
			}
			else if (target.HasMaster)
			{
				failReason = TameFailReason.CreatureAlreadyOwned;
			}
			else if (target.Level > caster.Level)
			{
				failReason = TameFailReason.TooHighLevel;
			}
			//else if (caster != null && caster.StabledPetRecords)
			//{
			//    failReason = TameFailReason.TooManyPets;
			//}
			else
			{
				return TameFailReason.Ok;
			}

			if (caster != null)
			{
				PetHandler.SendTameFailure(caster, failReason);
			}
			return failReason;
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Tries to consume the given amount of power
		/// </summary>
		public SpellFailedReason ConsumePower(int amount)
		{
			var caster = CasterUnit;
			if (caster != null)
			{
				if (m_spell.PowerType != PowerType.Health)
				{
					if (!caster.ConsumePower(m_spell.Schools[0], m_spell, amount))
					{
						return SpellFailedReason.NoPower;
					}
				}
				else
				{
					var health = caster.Health;
					caster.Health = health - amount;
					if (amount >= health)
					{
						// woops, we died
						return SpellFailedReason.CasterDead;
					}
				}
			}
			return SpellFailedReason.Ok;
		}

		public IWeapon GetWeapon()
		{
			if (!(CasterObject is Unit))
			{
				LogManager.GetCurrentClassLogger().Warn("{0} is not a Unit and casted Weapon Ability {1}", CasterObject, m_spell);
			}
			else
			{
				var weapon = ((Unit)CasterObject).GetWeapon(m_spell.EquipmentSlot);
				if (weapon == null)
				{
					LogManager.GetCurrentClassLogger().Warn("{0} casted {1} without required Weapon: {2}", CasterObject, m_spell, m_spell.EquipmentSlot);
				}
				return weapon;
			}
			return null;
		}

		/// <summary>
		/// Tries to consume all reagents or cancels the cast if it failed
		/// </summary>
		public bool ConsumeReagents()
		{
			var reagents = m_spell.Reagents;
			if (reagents != null && CasterUnit is Character)
			{
				if (!((Character)CasterUnit).Inventory.Consume(reagents, false))
				{
					Cancel(SpellFailedReason.Reagents);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Indicates whether a spell hit a target or not
		/// TODO: Actually check whether a spell 'Misses' (CastMissReason.Miss)
		/// </summary>
		public CastMissReason CheckCastHit(Unit target, Spell spell)
		{
			if (CasterObject != null && CasterObject.MayAttack(target))
			{
				var school = target.GetLeastResistantSchool(spell);
				// evasion
				if (target.IsEvading)
				{
					return CastMissReason.Evade;
				}
				// immune & invul
				if (!spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability) ||
					(target is Character && ((Character)target).Role.IsStaff))
				{
					if (target.IsInvulnerable)
					{
						return CastMissReason.Immune_2;
					}

					if (spell.Schools.All(target.IsImmune))
					{
						return CastMissReason.Immune;
					}
				}

				//// avoid/miss
				//var avoidance = target.GetSpellAvoidancePct(school);
				//if (avoidance > 0 && Utility.Random(1, 101) < avoidance)
				//{
				//    return CastMissReason.Miss;
				//}


				// resist
				if (target.CheckResist(CasterUnit, school, spell.Mechanic) && !spell.AttributesExB.HasFlag(SpellAttributesExB.CannotBeResisted))
				{
					return CastMissReason.Resist;
				}
			}

			return CastMissReason.None;
		}

		private void OnException(Exception e)
		{
			LogUtil.ErrorException(e, "{0} failed to cast Spell {1} (Targets: {2})", CasterObject, Spell, Targets.ToString(", "));
			if (CasterObject != null && !CasterObject.IsPlayer)
			{
				CasterObject.Delete();
			}
			else if (Client != null)
			{
				Client.Disconnect();
			}
			if (IsCasting)
			{
				Cleanup(true);
			}
		}
		#endregion

		#region Pushback
		public void Pushback(int millis)
		{
			if (IsChanneling)
			{
				m_channel.Pushback(millis);
			}
			else
			{
				RemainingCastTime += millis;
			}
		}

		/// <summary>
		/// Caused by damage.
		/// Delays the cast and might result in interruption (only if not DoT).
		/// See: http://www.wowwiki.com/Interrupt
		/// </summary>
		public void Pushback()
		{
			if (GodMode || !m_casting)
			{
				return;
			}

			// check for interruption
			if (m_spell.InterruptFlags.HasFlag(InterruptFlags.OnTakeDamage))
			{
				Cancel();
			}
			else
			{
				if (m_pushbacks < 2 && RemainingCastTime > 0)
				{
					if (IsChanneling)
					{
						// reduce 25% channeling time
						m_channel.Pushback(GetPushBackTime(m_channel.Duration / ChannelPushbackFraction));
					}
					else
					{
						// add 0.5 seconds
						RemainingCastTime += GetPushBackTime(PushbackDelay);
					}
					m_pushbacks++;
				}
			}
		}

		int GetPushBackTime(int time)
		{
			if (CasterObject is Unit)
			{
				var pct = ((Unit)CasterObject).GetSpellInterruptProt(m_spell);
				if (pct >= 100)
				{
					return 0;
				}

				time -= (pct * time) / 100; // reduce by protection %

				// pushback reduction is a positive value, but we want it to be reduced, so we need to use GetModifiedIntNegative
				time = ((Unit)CasterObject).Auras.GetModifiedIntNegative(SpellModifierType.PushbackReduction, m_spell, time);
			}
			return Math.Max(0, time);
		}
		#endregion

		#region Passive SpellCasts
		public void Trigger(SpellId spell)
		{
			Trigger(SpellHandler.Get(spell));
		}

		public void TriggerSelf(SpellId spell)
		{
			TriggerSingle(SpellHandler.Get(spell), CasterObject);
		}

		public void TriggerSelf(Spell spell)
		{
			TriggerSingle(spell, CasterObject);
		}

		public void TriggerSingle(SpellId spell, WorldObject singleTarget)
		{
			TriggerSingle(SpellHandler.Get(spell), singleTarget);
		}

		/// <summary>
		/// Casts the given spell on the given target, inheriting this SpellCast's information.
		/// SpellCast will automatically be enqueued if the Character is currently not in the world.
		/// </summary>
		public void TriggerSingle(Spell spell, WorldObject singleTarget)
		{
			var cast = InheritSpellCast();

			ExecuteInContext(() =>
			{
				cast.Start(spell, true, singleTarget);
				//cast.Dispose();
			});
		}

		/// <summary>
		/// Triggers all given spells instantly on the given single target
		/// </summary>
		public void TriggerAll(WorldObject singleTarget, params Spell[] spells)
		{
			if (CasterObject is Character && !CasterObject.IsInWorld)
			{
				CasterChar.AddMessage(new Message(() =>
				{
					TriggerAllSpells(singleTarget, spells);
				}));
			}
			else
			{
				TriggerAllSpells(singleTarget, spells);
			}
		}

		private void TriggerAllSpells(WorldObject singleTarget, params Spell[] spells)
		{
			var passiveCast = SpellCastPool.Obtain();
			foreach (var spell in spells)
			{
				SetupInheritedCast(passiveCast);
				passiveCast.Start(spell, true, singleTarget);
			}
			//passiveCast.Dispose();
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(SpellId spell, params WorldObject[] targets)
		{
			Trigger(spell, null, targets);
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(SpellId spell, SpellEffect triggerEffect, params WorldObject[] targets)
		{
			Trigger(spell, triggerEffect, null, targets);
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(SpellId spell, SpellEffect triggerEffect, IUnitAction triggerAction = null, params WorldObject[] targets)
		{
			Trigger(SpellHandler.Get(spell), triggerEffect, triggerAction, targets);
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Determines targets and hostility, based on the given triggerEffect.
		/// </summary>
		public void Trigger(Spell spell, SpellEffect triggerEffect, params WorldObject[] initialTargets)
		{
			Trigger(spell, triggerEffect, null, initialTargets);
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Determines targets and hostility, based on the given triggerEffect.
		/// </summary>
		public void Trigger(Spell spell, SpellEffect triggerEffect, IUnitAction triggerAction, params WorldObject[] initialTargets)
		{
			var cast = InheritSpellCast();
			//cast.TriggerEffect = triggerEffect;
			cast.TriggerAction = triggerAction;

			ExecuteInContext(() =>
			{
				cast.Start(spell, triggerEffect, true, initialTargets);
				//cast.Dispose();
			});
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(Spell spell, params WorldObject[] targets)
		{
			var cast = InheritSpellCast();

			ExecuteInContext(() =>
			{
				cast.Start(spell, true, targets != null && targets.Length > 0 ? targets : null);
				//cast.Dispose();
			});
		}

		/// <summary>
		/// Casts the given spell on targets determined by the given Spell.
		/// The given selected object will be the target, if the spell is a single target spell.
		/// </summary>
		public void TriggerSelected(Spell spell, WorldObject selected)
		{
			var cast = InheritSpellCast();
			cast.Selected = selected;

			ExecuteInContext(() =>
			{
				cast.Start(spell, true);
				//cast.Dispose();
			});
		}

		SpellCast InheritSpellCast()
		{
			var cast = SpellCastPool.Obtain();
			SetupInheritedCast(cast);
			return cast;
		}

		void SetupInheritedCast(SpellCast cast)
		{
			cast.SetCaster(CasterReference, Map, Phase, SourceLoc);
			cast.TargetLoc = TargetLoc;
			cast.Selected = Selected;
			cast.CasterItem = CasterItem;
		}

		/// <summary>
		/// 
		/// </summary>
		public static void ValidateAndTriggerNew(Spell spell, Unit caster, WorldObject target, SpellChannel usedChannel = null,
			Item usedItem = null, IUnitAction action = null, SpellEffect triggerEffect = null)
		{
			ValidateAndTriggerNew(spell, caster.SharedReference, caster, target, usedChannel, usedItem, action, triggerEffect);
		}

		/// <summary>
		/// 
		/// </summary>
		public static void ValidateAndTriggerNew(Spell spell, ObjectReference caster, Unit triggerOwner, WorldObject target,
			SpellChannel usedChannel = null, Item usedItem = null, IUnitAction action = null, SpellEffect triggerEffect = null)
		{
			var cast = SpellCastPool.Obtain();
			cast.SetCaster(caster, target.Map, target.Phase, triggerOwner.Position);
			cast.Selected = target;
			if (usedChannel != null && usedChannel.Cast.CasterUnit == triggerOwner)
			{
				cast.TargetLoc = triggerOwner.ChannelObject.Position;
			}
			else
			{
				cast.TargetLoc = target.Position;
			}
			cast.TargetItem = cast.CasterItem = usedItem;

			cast.ValidateAndTrigger(spell, triggerOwner, target, action, triggerEffect);
		}

		/// <summary>
		/// Creates a new SpellCast object to trigger the given spell.
		/// Validates whether the given target is the correct target
		/// or if we have to look for the actual targets ourselves.
		/// Revalidate targets, if it is:
		/// - an area spell 
		/// - a harmful spell and currently targeting friends
		/// - not harmful and targeting an enemy
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="target"></param>
		public void ValidateAndTriggerNew(Spell spell, Unit triggerOwner, WorldObject target, IUnitAction action = null, SpellEffect triggerEffect = null)
		{
			var passiveCast = InheritSpellCast();

			passiveCast.ValidateAndTrigger(spell, triggerOwner, target, action, triggerEffect);
		}

		public void ValidateAndTrigger(Spell spell, Unit triggerOwner, IUnitAction action = null)
		{
			if (action != null)
			{
				action.ReferenceCount++;
				TriggerAction = action;
			}

			ValidateAndTrigger(spell, triggerOwner, null, action);
		}

		public void ValidateAndTrigger(Spell spell, Unit triggerOwner, WorldObject target, IUnitAction action = null, SpellEffect triggerEffect = null)
		{
			WorldObject[] targets;

			if (triggerOwner == null)
			{
				LogManager.GetCurrentClassLogger().Warn("triggerOwner is null when trying to proc spell: {0} (target: {1})", spell, target);
				return;
			}

			if (spell.CasterIsTarget || !spell.HasTargets)
			{
				targets = new[] { triggerOwner };
			}
			else if (target != null)
			{
				if (spell.IsAreaSpell ||
					(spell.IsHarmfulFor(CasterReference, target)))
				{
					targets = null;
				}
				else
				{
					targets = new[] { target };
				}
			}
			else
			{
				targets = null;
			}

			if (action != null)
			{
				action.ReferenceCount++;
				TriggerAction = action;
			}

			Start(spell, triggerEffect, true, targets);
			//Dispose();
		}

		#endregion

		#region Cancel
		/// <summary>
		/// Cancels the spell cast
		/// </summary>
		public void Cancel()
		{
			Cancel(SpellFailedReason.Interrupted);
		}

		/// <summary>
		/// Cancels the spell cast
		/// </summary>
		public void Cancel(SpellFailedReason reason)
		{
			if (!m_casting)
			{
				return;
			}
			m_casting = false;

			if (m_channel != null)
			{
				m_channel.Close(true);
			}

			m_spell.NotifyCancelled(this, reason);

			// Client already disconnected?
			if (reason != SpellFailedReason.Ok && !m_passiveCast && m_spell.ShouldShowToClient())
			{
				//if (reason != SpellFailedReason.DontReport)
				{
					SpellHandler.SendCastFailPackets(this, reason);
				}
				//else
				//if (!m_spell.IsModalAura)
				{
					SpellHandler.SendSpellGo(CasterReference, this, null, null, 0);
				}
			}
			else if (CasterObject != null && CasterObject.IsUsingSpell && reason != SpellFailedReason.Ok && reason != SpellFailedReason.DontReport)
			{
				// cancel original spellcast (for multiple trigger nesting, we might need all SpellCasts in the sequence (if there are non-instant ones))
				var cast = CasterObject.SpellCast;
				if (this != CasterObject.SpellCast)
				{
					cast.Cancel(reason);
				}
			}

			Cleanup(true);
		}
		#endregion

		#region IUpdatable
		public void Update(int dt)
		{
			// gotta update the cast timer.
			m_castTimer.Update(dt);

			if (IsChanneling)
			{
				m_channel.Update(dt);
			}
		}
		#endregion

		#region Finalization
		/// <summary>
		/// Close the timer and get rid of circular references; will be called automatically
		/// </summary>
		internal protected void Cleanup(bool finalCleanup)
		{
			isPlayerCast = false;

			Id = 0;
			m_casting = false;
			if (m_spell.IsTame && Selected is NPC)
			{
				((NPC)Selected).CurrentTamer = null;
			}

			TargetItem = null;
			CasterItem = null;
			m_castTimer.Stop();
			m_initialTargets = null;
			m_handlers = null;
			m_passiveCast = false;
			m_pushbacks = 0;
			m_spell = null;
			TriggerEffect = null;
			TargetFlags = 0;

			if (m_targets != null)
			{
				m_targets.Clear();
			}
			if (finalCleanup)
			{
				DoFinalCleanup(m_handlers);
			}
		}

		private void DoFinalCleanup(SpellEffectHandler[] handlers)
		{
			if (TriggerAction != null)
			{
				TriggerAction.ReferenceCount--;
				TriggerAction = null;
			}

			if (handlers != null)
			{
				foreach (var handler in handlers)
				{
					// can be null if spell is cancelled during initialization
					if (handler != null)
					{
						handler.Cleanup();
					}
				}
			}

			if (CasterObject == null || CasterObject.SpellCast != this)
			{
				// TODO: Improve dispose strategy
				Dispose();
			}
		}

		internal void Dispose()
		{
			if (CasterReference == null)
			{
				LogManager.GetCurrentClassLogger().Warn("Tried to dispose SpellCast twice: " + this);
				return;
			}
			Cancel();
			if (m_channel != null)
			{
				m_channel.Dispose();
				m_channel = null;
			}

			if (m_targets != null)
			{
				m_targets.Clear();
			}
			//WorldObject.WorldObjectSetPool.Recycle(m_targets);

			SourceLoc = Vector3.Zero;
			CasterObject = CasterUnit = null;
			CasterReference = null;
			Map = null;
			Selected = null;
			GodMode = false;
			SpellCastPool.Recycle(this);
			//Context = null;
		}

		#endregion

		public void ExecuteInContext(Action action)
		{
			var obj = CasterObject;
			if (obj != null)
			{
				obj.ExecuteInContext(action);
			}
			else
			{
				Map.ExecuteInContext(action);
			}
		}

		public void SendPacketToArea(RealmPacketOut packet)
		{
			if (CasterObject != null)
			{
				CasterObject.SendPacketToArea(packet, true);
			}
			else
			{
				Map.SendPacketToArea(packet, ref TargetLoc, Phase);
			}
		}

		/// <summary>
		/// Is called whenever the validy of the caster might have changed
		/// </summary>
		private void CheckCasterValidity()
		{
			if (CasterObject != null && (!CasterObject.IsInWorld || !CasterObject.IsInContext))
			{
				CasterObject = null;
				CasterUnit = null;
			}
		}

		public override string ToString()
		{
			return Spell + " casted by " + CasterObject;
		}
	}

	#region CastMiss
	/// <summary>
	/// Represents a target which didn't get hit by a SpellCast
	/// </summary>
	public struct CastMiss
	{
		public readonly WorldObject Target;
		public readonly CastMissReason Reason;

		public CastMiss(WorldObject target, CastMissReason reason)
		{
			Target = target;
			Reason = reason;
		}

		public override bool Equals(object obj)
		{
			return obj is CastMiss && (((CastMiss)obj).Target == Target);
		}

		public override int GetHashCode()
		{
			return Target.GetHashCode();
		}
	}
	#endregion
}