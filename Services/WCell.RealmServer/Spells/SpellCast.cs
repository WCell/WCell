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
using Cell.Core;
using NLog;
using WCell.Constants;
using WCell.Constants.Pets;
using WCell.Constants.Spells;
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
	public partial class SpellCast : IUpdatable
	{
		public static int PushbackDelay = 500;
		public static int ChannelPushbackFraction = 4;

		public static readonly ObjectPool<SpellCast> SpellCastPool = ObjectPoolMgr.CreatePool(() => new SpellCast(), true);
		public static readonly ObjectPool<List<IAura>> AuraListPool = ObjectPoolMgr.CreatePool(() => new List<IAura>(), true);
		public static readonly ObjectPool<List<CastMiss>> CastMissListPool = ObjectPoolMgr.CreatePool(() => new List<CastMiss>(3), true);
		public static readonly ObjectPool<List<SpellEffectHandler>> SpellEffectHandlerListPool = ObjectPoolMgr.CreatePool(() => new List<SpellEffectHandler>(3), true);
		//internal static readonly ObjectPool<List<AuraApplicationInfo>> AuraAppListPool = ObjectPoolMgr.CreatePool(() => new List<AuraApplicationInfo>());

		public static void Trigger(WorldObject caster, SpellEffect triggerEffect, Spell spell)
		{
			var cast = SpellCastPool.Obtain();
			cast.Caster = caster;
			cast.m_triggerEffect = triggerEffect;

			caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true);
				cast.Dispose();
			});
		}

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
			var cast = SpellCastPool.Obtain();
			cast.Caster = caster;
			cast.TargetLoc = targetLoc;
			cast.Selected = selected;
			cast.CasterItem = casterItem;

			caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true);
				cast.Dispose();
			});
		}

		public static SpellCast ObtainPooledCast(WorldObject caster)
		{
			var cast = SpellCastPool.Obtain();
			cast.SetCaster(caster);
			return cast;
		}

		#region Fields
		private readonly bool CanCollect;
		Spell m_spell;

		/// <summary>
		/// The SpellEffect that triggered this cast (or null if not triggered)
		/// </summary>
		private SpellEffect m_triggerEffect;

		private int m_castDelay;

		private int m_startTime;
		/// <summary>
		/// The Unit or GameObject (traps), triggering this spell
		/// </summary>
		public WorldObject Caster
		{
			get;
			private set;
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
		/// An Item that this Spell is being used on
		/// </summary>
		public Item UsedItem;

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

		public Region TargetRegion
		{
			get
			{
				Region rgn;
				if (m_spell.TargetLocation != null)
				{
					rgn = m_spell.TargetLocation.Region ?? Caster.Region;
				}
				else
				{
					rgn = Caster.Region;
				}
				return rgn;
			}
		}

		public float TargetOrientation
		{
			get { return m_spell.TargetLocation != null ? m_spell.TargetOrientation : Caster.Orientation; }
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
			CanCollect = true;
			m_castTimer = new TimerEntry(0f, 0f, Perform);
		}

		/// <summary>
		/// Creates a new SpellCast for the given caster
		/// </summary>
		/// <param name="caster">The GameObject (in case of traps etc) or Unit casting</param>
		public SpellCast(WorldObject caster)
		{
			SetCaster(caster);

			m_castTimer = new TimerEntry(0f, 0f, Perform);
		}

		void SetCaster(WorldObject caster)
		{
			Caster = caster;
			if (Caster is IOwned)
			{
				CasterUnit = ((IOwned)Caster).Owner;
			}
			else
			{
				CasterUnit = Caster as Unit;
			}
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

		public CastFlags StartFlags
		{
			get
			{
				var flags = CastFlags.None;
				if (m_spell != null && m_spell.IsRangedAbility)
				{
					flags |= CastFlags.Ranged;
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
			get { return (CasterUnit != null) ? CasterUnit.Level : 0; }
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
				if (CasterUnit != null)
				{
					var uCaster = Caster as Unit;
					Character chr;
					if (uCaster != null && uCaster.Master != uCaster)
					{
						chr = uCaster.Master as Character;
					}
					else
					{
						chr = Caster as Character;
					}

					if (chr != null)
					{
						return chr.Client;
					}
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
				m_castTimer.RemainingInitialDelay = value / 1000f;

				SpellHandler.SendCastDelayed(Caster, delta);
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
			get { return m_triggerEffect != null ? m_triggerEffect.IsAreaEffect : m_spell.IsAreaSpell; }
			//get { return m_spell.IsAreaSpell; }
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

		public void Start(SpellId spellId, bool passiveCast, params WorldObject[] targets)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell == null)
			{
				LogManager.GetCurrentClassLogger().Error("{0} tried to cast non-existant Spell: {1}", Caster, spellId);
				return;
			}
			Start(spell, passiveCast, targets);
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

			var region = Caster.Region;

			m_casting = true;


			m_spell = spell;
			Id = castId;

			//byte unkFlag = packet.ReadByte();

			// TODO: Make more use of these information to optimize SpellTargetCollection
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
				TargetLoc = Caster.Position;
				selected = Caster;
			}
			// 0x18A02
			if (TargetFlags.Has(
				SpellTargetFlags.SpellTargetFlag_Dynamic_0x10000 |
				SpellTargetFlags.Corpse |
				SpellTargetFlags.Object |
				SpellTargetFlags.PvPCorpse |
				SpellTargetFlags.Unit))
			{
				// The user selected an Object
				var uid = packet.ReadPackedEntityId();
				selected = region.GetObject(uid);

				if (selected == null || !Caster.CanSee(selected))
				{
					Cancel(SpellFailedReason.BadTargets);
					return SpellFailedReason.BadTargets;
				}

				targetFound = true;
				TargetLoc = selected.Position;
			}
			// 0x1010
			if (Caster is Character && TargetFlags.Has(SpellTargetFlags.TradeItem | SpellTargetFlags.Item))
			{
				var uid = packet.ReadPackedEntityId();
				UsedItem = ((Character)Caster).Inventory.GetItem(uid);
				if (UsedItem == null || !UsedItem.CanBeUsed)
				{
					Cancel(SpellFailedReason.BadTargets);
					return SpellFailedReason.BadTargets;
				}
			}
			// 0x20
			if (TargetFlags.Has(SpellTargetFlags.SourceLocation))
			{
				region.GetObject(packet.ReadPackedEntityId());		// since 3.2.0
				SourceLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
			}
			// 0x40
			if (TargetFlags.Has(SpellTargetFlags.DestinationLocation))
			{
				selected = region.GetObject(packet.ReadPackedEntityId());
				TargetLoc = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
				//Console.WriteLine("SpellCast.Start - DestLoc {0}", TargetLoc);
				targetFound = true;
			}
			// 0x2000
			if (TargetFlags.Has(SpellTargetFlags.String))
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
				spell.SpecialCast(spell, Caster, selected, ref TargetLoc);
				Cancel(SpellFailedReason.DontReport);
				return SpellFailedReason.DontReport;
			}

			if (targetFound)
			{
				// default checks
				if (selected != Caster)
				{
					if (Caster is Character)
					{
						// check range
						var chr = Caster as Character;
						var sqDistance = Caster.GetDistanceSq(ref TargetLoc);
						if (!Utility.IsInRange(sqDistance, chr.GetSpellMaxRange(spell, selected)) ||
							(selected != null && selected.Region != Caster.Region))
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
		public SpellFailedReason Start(Spell spell, SpellEffect triggerEffect, bool passiveCast)
		{
			m_triggerEffect = triggerEffect;
			return Start(spell, passiveCast, (WorldObject[])null);
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
			m_initialTargets = initialTargets;

			var reason = Prepare();
			if (reason == SpellFailedReason.Ok)
			{
				return FinishPrepare();
			}
			return reason;
		}

		/// <summary>
		/// Use this method to change the SpellCast object
		/// after it has been prepared.
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
			//var stopwatch = Stopwatch.StartNew();
			if (Selected == null && Caster is Unit)
			{
				if (m_initialTargets != null)
				{
					Selected = m_initialTargets[0];
				}
				else
				{
					Selected = ((Unit)Caster).Target;
				}
			}

			if (!m_passiveCast && CasterUnit != null)
			{
				// don't sit on a ride (even if you try to, the Client will show you dismounted - maybe add auto-remount for GodMode)
				var spell = m_spell;

				if (!spell.Attributes.Has(SpellAttributes.CastableWhileMounted))
				{
					CasterUnit.Dismount();
				}

				// make sure, the Caster is standing
				if (!spell.Attributes.Has(SpellAttributes.CastableWhileSitting))
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

					// remove certain Auras
					CasterUnit.Auras.RemoveByFlag(AuraInterruptFlags.OnCast);
				}
			}

			m_startTime = Environment.TickCount;
			m_castDelay = (int)m_spell.CastDelay;

			if (!IsInstant)
			{
				// calc exact cast delay
				if (CasterUnit != null)
				{
					m_castDelay = (CasterUnit.CastSpeedFactor * m_castDelay).RoundInt();
					if (CasterChar != null)
					{
						m_castDelay = CasterChar.PlayerSpells.GetModifiedInt(SpellModifierType.CastTime, m_spell, m_castDelay);
					}
				}
			}

			if (m_spell.TargetLocation != null)
			{
				TargetLoc = m_spell.TargetLocation.Position;
			}

			// Notify that we are about to cast
			return NotifyCasting();
		}

		SpellFailedReason FinishPrepare()
		{
			if (!IsInstant)
			{
				// send Start packet
				SendCastStart();
				m_castTimer.Start(m_castDelay);
				return SpellFailedReason.Ok;
			}

			return Perform();
		}

		/// <summary>
		/// Triggers the Casting event
		/// </summary>
		/// <returns></returns>
		SpellFailedReason NotifyCasting()
		{
			var evt = Casting;
			if (evt != null)
			{
				var err = evt(this);
				if (err != SpellFailedReason.Ok)
				{
					Cancel(err);
					return err;
				}
			}
			return SpellFailedReason.Ok;
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

		internal void CheckHitAndSendSpellGo(bool revalidateTargets)
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

			if (m_spell.ShouldShowToClient())
			{
				// send the packet (so client sees the actual cast) if its not a passive spell
				var caster2 = CasterItem ?? (ObjectBase)Caster;
				SpellHandler.SendSpellGo(caster2, this, m_targets, missedTargets);
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
		/// <param name="selected"></param>
		/// <returns></returns>
		protected SpellFailedReason CheckPlayerCast(WorldObject selected)
		{
			var caster = (Character)Caster;
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

			var err = m_spell.CheckCasterConstraints(CasterUnit);
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

			if (!IsPassive)
			{
				if (!caster.HasEnoughPowerToCast(m_spell, null))
				{
					return SpellFailedReason.NoPower;
				}
			}

			// Item restrictions			
			return m_spell.CheckItemRestrictions(UsedItem, caster.Inventory);
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
							handler.Targets.Remove(target);
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
								handler.Targets.Remove(target);
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
			else
			{
				if (!target.Entry.IsTamable)
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
				//else if (caster != null && caster.StabledPetRecords)
				//{
				//    failReason = TameFailReason.TooManyPets;
				//}
			}

			if (failReason != TameFailReason.Ok && caster != null)
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
			if (!(Caster is Unit))
			{
				LogManager.GetCurrentClassLogger().Warn("{0} is not a Unit and casted Weapon Ability {1}", Caster, m_spell);
			}
			else
			{
				var weapon = ((Unit)Caster).GetWeapon(m_spell.EquipmentSlot);
				if (weapon == null)
				{
					LogManager.GetCurrentClassLogger().Warn("{0} casted {1} without required Weapon: {2}", Caster, m_spell, m_spell.EquipmentSlot);
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
			if (reagents != null && Caster is Character)
			{
				if (!((Character)Caster).Inventory.Consume(false, reagents))
				{
					Cancel(SpellFailedReason.Reagents);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Indicates whether a spell hit a target or not
		/// </summary>
		public CastMissReason CheckCastHit(Unit target, Spell spell)
		{
			if (Caster.MayAttack(target))
			{
				if (target.IsEvading)
				{
					return CastMissReason.Evade;
				}

				if (!spell.Attributes.Has(SpellAttributes.UnaffectedByInvulnerability) ||
					(target is Character && ((Character)target).Role.IsStaff))
				{
					if (target.IsInvulnerable)
					{
						return CastMissReason.Immune_2;
					}

					var immune = true;
					for (var i = 0; i < spell.Schools.Length; i++)
					{
						var school = spell.Schools[i];
						if (!target.IsImmune(school))
						{
							immune = false;
							break;
						}
					}
					if (immune)
					{
						return CastMissReason.Immune;
					}
				}

				if (target.CheckResist(CasterUnit, target.GetLeastResistant(spell), spell.Mechanic) && !spell.AttributesExB.Has(SpellAttributesExB.CannotBeResisted))
				{
					return CastMissReason.Resist;
				}
			}

			return CastMissReason.None;
		}

		private void OnException(Exception e)
		{
			LogUtil.ErrorException(e, "{0} failed to cast Spell {1} (Targets: {2})", Caster, Spell, Targets.ToString(", "));
			if (IsCasting)
			{
				Cleanup(true);
			}
		}
		#endregion

		#region Pushback
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
			if (m_spell.InterruptFlags.Has(InterruptFlags.OnTakeDamage))
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
			if (Caster is Unit)
			{
				var pct = ((Unit)Caster).GetSpellInterruptProt(m_spell);
				if (pct > 100)
				{
					return 0;
				}

				time -= (pct * time) / 100; // reduce by protection %
				if (Caster is Character)
				{
					time = ((Character)Caster).PlayerSpells.GetModifiedInt(SpellModifierType.Pushback, m_spell, time);
				}
			}
			return time;
		}
		#endregion

		#region Passive SpellCasts
		public void Trigger(SpellId spell)
		{
			Trigger(SpellHandler.Get(spell));
		}

		public void TriggerSelf(SpellId spell)
		{
			TriggerSingle(SpellHandler.Get(spell), Caster);
		}

		public void TriggerSelf(Spell spell)
		{
			TriggerSingle(spell, Caster);
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

			Caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true, singleTarget);
				cast.Dispose();
			});
		}

		/// <summary>
		/// Triggers all given spells instantly on the given single target
		/// </summary>
		public void TriggerAll(WorldObject singleTarget, params Spell[] spells)
		{
			var passiveCast = InheritSpellCast();

			if (Caster is Character && CasterChar.Region == null)
			{
				CasterChar.MessageQueue.Enqueue(new Message(() =>
				{
					foreach (var spell in spells)
					{
						passiveCast.Start(spell, true, singleTarget);
					}
					passiveCast.Dispose();
				}));
			}
			else
			{
				foreach (var spell in spells)
				{
					passiveCast.Start(spell, true, singleTarget);
				}
				passiveCast.Dispose();
			}
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(SpellId spell, params WorldObject[] targets)
		{
			Trigger(SpellHandler.Get(spell), targets);
		}

		///// <summary>
		///// Casts the given spell on the given targets within this SpellCast's context.
		///// Determines targets and hostility, based on the given triggerEffect.
		///// </summary>
		//public void Trigger(Spell spell, SpellEffect triggerEffect)
		//{
		//    Trigger(spell, triggerEffect, null);
		//}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Determines targets and hostility, based on the given triggerEffect.
		/// </summary>
		public void Trigger(Spell spell, SpellEffect triggerEffect, WorldObject selected)
		{
			var cast = InheritSpellCast();
			if (selected != null)
			{
				cast.Selected = selected;
			}
			cast.m_triggerEffect = triggerEffect;

			Caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true, (WorldObject[])null);
				cast.Dispose();
			});
		}

		/// <summary>
		/// Casts the given spell on the given targets within this SpellCast's context.
		/// Finds targets automatically if the given targets are null.
		/// </summary>
		public void Trigger(Spell spell, params WorldObject[] targets)
		{
			var cast = InheritSpellCast();

			Caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true, targets != null && targets.Length > 0 ? targets : null);
				cast.Dispose();
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

			Caster.ExecuteInContext(() =>
			{
				cast.Start(spell, true);
				cast.Dispose();
			});
		}

		SpellCast InheritSpellCast()
		{
			var cast = SpellCastPool.Obtain();
			cast.SetCaster(Caster);
			cast.TargetLoc = TargetLoc;
			cast.Selected = Selected;
			cast.CasterItem = CasterItem;
			return cast;
		}

		/// <summary>
		/// Validates whether the given target is the correct target
		/// or if we have to look for the actual targets ourselves.
		/// Revalidate targets, if it is:
		/// - an area spell 
		/// - a harmful spell and currently targeting friends
		/// - not harmful and targeting an enemy
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="target"></param>
		public void ValidateAndTrigger(Spell spell, WorldObject target)
		{
			var passiveCast = InheritSpellCast();

			WorldObject[] targets;
			// TODO: Consider triggering effect
			if (spell.IsAreaSpell || (Caster.MayAttack(target) != spell.HasHarmfulEffects))
			{
				targets = null;
			}
			else
			{
				targets = new[] { target };
			}

			passiveCast.Start(spell, true, targets);
			passiveCast.Dispose();
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

			var evt = Cancelling;
			if (evt != null)
			{
				evt(this, reason);
			}

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
					SpellHandler.SendSpellGo(Caster, this, null, null);
				}
			}
			else if (Caster.IsUsingSpell && reason != SpellFailedReason.Ok && reason != SpellFailedReason.DontReport)
			{
				// cancel original spellcast (for multiple trigger nesting, we might need all SpellCasts in the sequence (if there are non-instant ones))
				var cast = Caster.SpellCast;
				if (this != Caster.SpellCast)
				{
					cast.Cancel(reason);
				}
			}

			Cleanup(true);
		}
		#endregion

		#region IUpdatable
		public void Update(float dt)
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
		internal protected void Cleanup(bool disposeHandlers)
		{
			isPlayerCast = false;

			Id = 0;
			m_casting = false;
			if (disposeHandlers)
			{
				DisposeHandlers(m_handlers);
			}
			if (m_spell.IsTame && Selected is NPC)
			{
				((NPC)Selected).CurrentTamer = null;
			}

			UsedItem = null;
			CasterItem = null;
			m_castTimer.Stop();
			m_initialTargets = null;
			m_handlers = null;
			m_passiveCast = false;
			m_pushbacks = 0;
			m_spell = null;
			m_triggerEffect = null;
			TargetFlags = 0;

			if (m_targets != null)
			{
				m_targets.Clear();
			}
		}

		private void DisposeHandlers(SpellEffectHandler[] handlers)
		{
			if (handlers == null)
				return;

			foreach (var handler in handlers)
			{
				// can be null if spell is cancelled during initialization
				if (handler != null)
				{
					handler.Cleanup();
				}
			}
		}

		internal void Dispose()
		{
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

			if (CanCollect)
			{
				SourceLoc = Vector3.Zero;
				Caster = CasterUnit = null;
				SpellCastPool.Recycle(this);
			}
		}
		#endregion

		public override string ToString()
		{
			return Spell + " casted by " + Caster;
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