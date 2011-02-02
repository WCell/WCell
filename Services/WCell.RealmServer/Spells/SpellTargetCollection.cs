/*************************************************************************
 *
 *   file		: SpellTargetCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 16:30:19 +0100 (lø, 30 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1235 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Extensions;
using WCell.Util;
using WCell.Util.Graphics;
using Cell.Core;

namespace WCell.RealmServer.Spells
{
	public delegate void TargetFilter(SpellCast cast, WorldObject target, ref SpellFailedReason failReason);

	/// <summary>
	/// A list of all targets for a Spell
	/// </summary>
	public class SpellTargetCollection : List<WorldObject>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		static readonly ObjectPool<SpellTargetCollection> SpellTargetCollectionPool = ObjectPoolMgr.CreatePool(() => new SpellTargetCollection());

		public static SpellTargetCollection Obtain()
		{
			// TODO: Recycle collections
			return new SpellTargetCollection();
		}

		/// <summary>
		/// Further SpellEffectHandlers that are sharing this TargetCollection with the original Handler
		/// </summary>
		internal List<SpellEffectHandler> m_handlers;
		public bool IsInitialized
		{
			get;
			private set;
		}

		internal SpellEffectHandler FirstHandler
		{
			get { return m_handlers[0]; }
		}

		internal SpellCast Cast
		{
			get { return m_handlers[0].Cast; }
		}

		/// <summary>
		/// Private ctor used by SpellTargetCollectionPool
		/// </summary>
		public SpellTargetCollection()
		{
			m_handlers = new List<SpellEffectHandler>(3);
		}

		#region Find & Validate Targets
		public SpellFailedReason FindAllTargets()
		{
			IsInitialized = true;

			var cast = Cast;
			var caster = cast.CasterObject;
			if (caster == null)
			{
				// TODO: This still needs to work without a caster?!
				log.Warn("Invalid SpellCast - Tried to find targets, without Caster set: {0}", caster);
				return SpellFailedReason.Error;
			}

			var firstEffect = FirstHandler.Effect;
			if (firstEffect.Spell.IsPreventionDebuff)
			{
				// need to call InitializeTarget nevertheless
				var err = SpellFailedReason.Ok;

				foreach (var handler in m_handlers)
				{
					err = handler.ValidateAndInitializeTarget(caster);
					if (err != SpellFailedReason.Ok)
					{
						return err;
					}
				}
				Add(caster);
				return err;
			}

			var failReason = SpellFailedReason.Ok;
			if (firstEffect.ImplicitTargetA != ImplicitSpellTargetType.None)
			{
				failReason = FindTargets(firstEffect.ImplicitTargetA);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}
			if (firstEffect.ImplicitTargetB != ImplicitSpellTargetType.None)
			{
				failReason = FindTargets(firstEffect.ImplicitTargetB);
			}
			return failReason;
		}

		public SpellFailedReason FindTargets(ImplicitSpellTargetType targetType)
		{
			var failedReason = SpellFailedReason.Ok;
			var def = targetHandlers[(int)targetType];
			if (def.Handler != null)
			{
				def.Handler(this, def.Filter, ref failedReason);
			}
			return failedReason;
		}

		/// <summary>
		/// Does default checks on whether the given Target is valid for the current SpellCast
		/// </summary>
		public SpellFailedReason ValidateTarget(WorldObject target, TargetFilter filter)
		{
			var cast = Cast;
			var spell = cast.Spell;

			var failReason = spell.CheckValidTarget(cast.CasterObject, target);
			if (failReason != SpellFailedReason.Ok)
			{
				return failReason;
			}

			if (filter != null)
			{
				filter(cast, target, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}

			return ValidateTargetForHandlers(target);
		}

		public SpellFailedReason ValidateTargetForHandlers(WorldObject target)
		{
			foreach (var handler in m_handlers)
			{
				if (!target.CheckObjType(handler.TargetType))
				{
					return SpellFailedReason.BadTargets;
				}
				var failReason = handler.ValidateAndInitializeTarget(target);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}
			return SpellFailedReason.Ok;
		}

		public void AddTargetsInArea(Vector3 pos, TargetFilter targetFilter, float radius)
		{
			var spell = FirstHandler.Effect.Spell;
			int limit;
			if (spell.MaxTargetEffect != null)
			{
				limit = spell.MaxTargetEffect.CalcEffectValue(Cast.CasterReference);
			}
			else
			{
				limit = (int)spell.MaxTargets;
			}

			if (limit < 1)
			{
				limit = int.MaxValue;
			}

			Cast.Map.IterateObjects(pos, radius > 0 ? radius : 5, Cast.Phase,
				obj =>
				{
					if (ValidateTarget(obj, targetFilter) == SpellFailedReason.Ok)
					{
						Add(obj);
						return Count < limit;
					}
					return true;
				});
		}

		/// <summary>
		/// Adds all chained units around the selected unit to the list
		/// </summary>
		public void FindChain(Unit first, TargetFilter filter, bool harmful, int limit)
		{
			var handler = FirstHandler;
			var caster = handler.Cast.CasterObject;
			var spell = handler.Cast.Spell;

			first.IterateEnvironment(handler.GetRadius(), target =>
			{
				if ((spell.FacingFlags & SpellFacingFlags.RequiresInFront) != 0 &&
					caster != null &&
					!target.IsInFrontOf(caster))
				{
					return true;
				}
				if (target != caster &&
					target != first)
				{
					if (caster == null)
					{
						// TODO: Add chain targets, even if there is no caster
						return true;
					}
					else if ((harmful && !caster.MayAttack(target)) ||
					(!harmful && !caster.IsInSameDivision(target)))
					{
						return true;
					}
					if (ValidateTarget(target, filter) == SpellFailedReason.Ok)
					{
						Add(target);
					}
					//return Count < limit;
				}
				return true;
			});

			// TODO: Consider better ways to do this?
			Sort((a, b) => a.GetDistanceSq(first).CompareTo(b.GetDistanceSq(first)));
			if (Count > limit)
			{
				RemoveRange(limit, Count - limit);
			}
		}
		#endregion

		#region Handlers
		public delegate void TargetAdder(SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason);
		public delegate void TargetHandler();

		private static readonly TargetDefinition[] targetHandlers =
			new TargetDefinition[(int)Utility.GetMaxEnum<ImplicitSpellTargetType>() + 1];

		/// <summary>
		/// Returns the handler and filter for the given target type
		/// </summary>
		public static TargetDefinition GetTargetDefinition(ImplicitSpellTargetType target)
		{
			return targetHandlers[(int)target];
		}

		public struct TargetDefinition
		{
			//public readonly bool CheckInRange;
			public readonly TargetAdder Handler;
			public readonly TargetFilter Filter;

			public TargetDefinition(TargetAdder handler, TargetFilter filter)
			{
				Handler = handler;
				Filter = filter;
			}
		}

		static SpellTargetCollection()
		{
			InitTargetHandlers();
		}

		private static void InitTargetHandlers()
		{
			targetHandlers[(int)ImplicitSpellTargetType.AllAroundLocation] = new TargetDefinition(
					TargetMethods.AddAreaDest,	// Is this right?
					null);

			targetHandlers[(int)ImplicitSpellTargetType.AllEnemiesAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInArea] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInAreaInstant] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.CanHarm);

			// ImplicitTargetType.AllFriendlyInAura

			targetHandlers[(int)ImplicitSpellTargetType.AllParty] = new TargetDefinition(
					TargetMethods.AddAllParty,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.AllPartyAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsAllied);

			// Seems to be bogus: Often used together with AllEnemiesAroundCaster
			targetHandlers[(int)ImplicitSpellTargetType.AllPartyInArea] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.IsAllied);

			targetHandlers[(int)ImplicitSpellTargetType.AllPartyInAreaChanneled] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.IsAllied);

			// Odd: Mostly in combination with LocationToSummon and TeleportLocation
			// The only spell that has this with a negative effect is: Goblin Mortar (Id: 13238, Target: Default)
			targetHandlers[(int)ImplicitSpellTargetType.AllTargetableAroundLocationInRadiusOverTime] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitSpellTargetType.AreaEffectPartyAndClass] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsSamePartyAndClass);

			//targetHandlers.Add(ImplicitTargetType.BehindTargetLocation,
			//    new TargetDefinition(
			//        TargetMethods.AddSelection,
			//        TargetMethods.IsBehind));

			// ImplicitTargetType.CaliriEggs

			// any kind of chain effect
			targetHandlers[(int)ImplicitSpellTargetType.Chain] = new TargetDefinition(
					TargetMethods.AddChain,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.CurrentSelection] = new TargetDefinition(
					TargetMethods.AddSelection,
					null);
			/*
			Spell: ClassSkill Dispel Magic, Rank1 (Id: 527, Target: Default)
	School: Holy
	DispelType: 1
	Attributes: Flag0x10000
	AttributesExB: Flag0x80000

			 * Spell: Charm (Possess) (Id: 530, Target: Default)
	School: Arcane
	Attributes: Flag0x10000
	AttributesExC: Flag0x10000000
			 * 
			 Spell: Periodic Mana Burn (Id: 812, Target: Default)
	Attributes: Flag0x100, Flag0x800, Flag0x800000
	AttributesExB: Flag0x4
			 */

			targetHandlers[(int)ImplicitSpellTargetType.Duel] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarmOrHeal);

			targetHandlers[(int)ImplicitSpellTargetType.DynamicObject] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// Some poision effects which always have other targets set
			// ImplicitTargetType.EnemiesInAreaChanneledWithExceptions

			targetHandlers[(int)ImplicitSpellTargetType.GameObject] = new TargetDefinition(
					TargetMethods.AddObject,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.GameObjectOrItem] = new TargetDefinition(
					TargetMethods.AddItemOrObject,
					null);

			// ImplicitTargetType.HeartstoneLocation

			targetHandlers[(int)ImplicitSpellTargetType.InFrontOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsInFrontEnemies);

			targetHandlers[(int)ImplicitSpellTargetType.ConeInFrontOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsInFrontEnemies);

			targetHandlers[(int)ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					null);

			//targetHandlers[(int)ImplicitTargetType.LocationInFrontCaster] = new TargetDefinition(
			//        TargetMethods.AddAreaCaster,
			//        TargetMethods.IsInFrontFriends);

			// ImplicitTargetType.LocationInFrontCasterAtRange

			targetHandlers[(int)ImplicitSpellTargetType.LocationNearCaster] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// If not Demon and ObjectWild, add all around?
			//targetHandlers.Add(ImplicitTargetType.LocationToSummon,
			//    new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null));

			targetHandlers[(int)ImplicitSpellTargetType.Minion] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.MultipleGuardianSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.MultipleSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			//targetHandlers[(int)ImplicitTargetType.MultipleSummonPetLocation] = new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null);

			targetHandlers[(int)ImplicitSpellTargetType.NatureSummonLocation] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.NetherDrakeSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.Pet] = new TargetDefinition(
					TargetMethods.AddPet,
					null);

			// default
			// TODO: What happens if items/gameobjects are involved?
			targetHandlers[(int)ImplicitSpellTargetType.None] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// odd:
			targetHandlers[(int)ImplicitSpellTargetType.PartyMember] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitSpellTargetType.PartyAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitSpellTargetType.ScriptedOrSingleTarget] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitSpellTargetType.SelectedEnemyChanneled] = new TargetDefinition(
					TargetMethods.AddChannelObject,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.Self] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.SingleEnemy] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.SingleFriend] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitSpellTargetType.SingleParty] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsAllied);

			targetHandlers[(int)ImplicitSpellTargetType.SpreadableDesease] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitSpellTargetType.SummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.TargetAtOrientationOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsInFrontEnemies);

			// ImplicitTargetType.TargetForVisualEffect

			// ImplicitTargetType.TeleportLocation, <- Used for Summon spells

			// Totem summoning
			targetHandlers[(int)ImplicitSpellTargetType.TotemAir] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.TotemEarth] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.TotemFire] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.TotemWater] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitSpellTargetType.Tranquility] = new TargetDefinition(
					TargetMethods.AddAreaSource,
					TargetMethods.IsAllied);
		}
		#endregion

		public delegate void TargetHandler<in T>(T target);

		public void ApplyToAllOf<TargetType>(TargetHandler<TargetType> handler)
			where TargetType : ObjectBase
		{
			foreach (var target in this)
			{
				if (target is TargetType)
					handler(target as TargetType);
			}
		}

		internal void Dispose()
		{
			if (!IsInitialized)
			{
				return;
			}

			IsInitialized = false;

			m_handlers.Clear();
			Clear();
			//SpellTargetCollectionPool.Recycle(this);
		}
	}
}


namespace WCell.RealmServer.Spells.Extensions
{

	/// <summary>
	/// A srcCont for public extension methods for TargetCollection.
	/// 
	/// This is required in order to define default target-definitions 
	/// in a static context AND be able to invoke these methods as usual
	/// upon TargetCollection instances without reflection
	/// </summary>
	static class TargetMethods
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Add Methods
		public static void AddSelf(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var self = targets.Cast.CasterObject;
			if (self == null)
			{
				// invalid trigger proc
				log.Warn("Invalid SpellCast tried to target self, but no Caster given: {0}", targets.Cast);
				failReason = SpellFailedReason.Error;
				return;
			}

			if ((failReason = targets.ValidateTargetForHandlers(self)) == SpellFailedReason.Ok)
			{
				targets.Add(self);
			}
		}

		public static void AddPet(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterObject;
			if (!(caster is Character))
			{
				log.Warn("Non-Player {0} tried to cast Pet - spell {1}", caster, targets.Cast.Spell);
				failReason = SpellFailedReason.TargetNotPlayer;
				return;
			}

			var pet = ((Character)caster).ActivePet;
			if (pet == null)
			{
				failReason = SpellFailedReason.NoPet;
				return;
			}


			targets.Add(pet);
		}

		//public static void AddOwnPet(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		//{
		//    var chr = targets.Cast.Caster as Character;
		//    if (chr != null)
		//    {
		//        var pet = chr.ActivePet;
		//        if (pet != null)
		//        {
		//            foreach (var handler in targets.m_handlers)
		//            {
		//                if ((failReason = handler.CheckValidTarget(pet)) != SpellFailedReason.Ok)
		//                {
		//                    return;
		//                }
		//            }
		//            targets.Add(pet);
		//        }
		//        else
		//        {
		//            failReason = SpellFailedReason.BadTargets;
		//        }
		//    }
		//    else
		//    {
		//        log.Warn("NPC \"{0}\" used Spell with Pet-Target.", targets.Cast.Caster);
		//        failReason = SpellFailedReason.BadTargets;
		//    }
		//}

		/// <summary>
		/// Adds the object or unit that has been chosen by a player when the spell was casted
		/// </summary>
		/// <param name="targets"></param>
		public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddSelection(targets, filter, ref failReason, false);
		}

		/// <summary>
		/// Adds the selected targets and chain-targets (if any)
		/// </summary>
		public static void AddSelection(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, bool harmful)
		{
			var cast = targets.Cast;
			if (cast == null)
				return;

			var caster = cast.CasterObject as Unit;
			var selected = cast.Selected;
			if (selected == null)
			{
				if (caster == null)
				{
					log.Warn("Invalid SpellCast, tried to add Selection but nothing selected and no Caster present: {0}", targets.Cast);
					failReason = SpellFailedReason.Error;
					return;
				}
				selected = caster.Target;
				if (selected == null)
				{
					failReason = SpellFailedReason.BadTargets;
					return;
				}
			}

			var effect = targets.FirstHandler.Effect;
			var spell = effect.Spell;
			if (selected != caster && caster != null)
			{
				if (!caster.IsInMaxRange(spell, selected))
				{
					failReason = SpellFailedReason.OutOfRange;
				}
				else if (caster.IsPlayer && !selected.IsInFrontOf(caster))
				{
					failReason = SpellFailedReason.UnitNotInfront;
				}
			}

			if (failReason == SpellFailedReason.Ok)
			{
				// standard checks
				failReason = targets.ValidateTarget(selected, filter);

				if (failReason == SpellFailedReason.Ok)
				{
					// add target and look for more if we have a chain effect
					targets.Add(selected);
					var chainCount = effect.ChainTargets;
					if (caster != null)
					{
						chainCount = caster.Auras.GetModifiedInt(SpellModifierType.ChainTargets, spell, chainCount);
					}
					if (chainCount > 1 && selected is Unit)
					{
						targets.FindChain((Unit)selected, filter, true, chainCount);
					}
				}
			}
		}

		public static void AddChannelObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterUnit;
			if (caster != null)
			{
				if (caster.ChannelObject != null)
				{
					if ((failReason = targets.ValidateTarget(caster.ChannelObject, filter)) == SpellFailedReason.Ok)
					{
						targets.Add(caster.ChannelObject);
					}
				}
				else
				{
					failReason = SpellFailedReason.BadTargets;
				}
			}
		}


		/// <summary>
		/// Adds targets around the caster
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="filter"></param>
		/// <param name="failReason"></param>
		public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaSource(targets, filter, ref failReason, targets.FirstHandler.GetRadius());
		}

		public static void AddAreaSource(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, float radius)
		{
			targets.AddTargetsInArea(targets.Cast.SourceLoc, filter, radius);
		}

		public static void AddAreaSrc(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaSource(targets, filter, ref failReason);
		}

		public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaDest(targets, filter, ref failReason, targets.FirstHandler.GetRadius());
		}

		public static void AddAreaDest(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, float radius)
		{
			targets.AddTargetsInArea(targets.Cast.TargetLoc, filter, radius);
		}

		public static void AddChain(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddSelection(targets, filter, ref failReason, false);
		}

		public static void AddAllParty(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.CasterChar != null)
			{
				// For Characters: Add the whole party
				if (targets.Cast.CasterChar.Group != null)
				{
					foreach (var member in targets.Cast.CasterChar.Group)
					{
						var chr = member.Character;
						if (chr != null)
						{
							targets.Add(chr);
						}
					}
				}
				else
				{
					failReason = SpellFailedReason.TargetNotInParty;
				}
			}
			else
			{
				var radius = targets.FirstHandler.GetRadius();
				if (radius == 0)
				{
					// For NPCs: Add all friendly minions around (radius 30 if no radius is set?)
					radius = 30;
				}
				targets.AddAreaSource(IsFriendly, ref failReason, radius);
			}
		}

		/// <summary>
		/// Used for Lock picking and opening (with or without keys)
		/// </summary>
		public static void AddItemOrObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.TargetItem == null && !(targets.Cast.Selected is GameObject))
			{
				failReason = SpellFailedReason.BadTargets;
			}
		}

		public static void AddObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (!(targets.Cast.Selected is GameObject))
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				targets.Add(targets.Cast.Selected);
			}
		}

		/// <summary>
		/// Your current summon or self
		/// </summary>
		public static void AddSummon(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			// if (Handler.SpellCast.Caster.Summon == null)
			targets.AddSelf(filter, ref failReason);
		}
		#endregion


		#region Filters
		public static void IsFriendly(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (cast.CasterObject.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetEnemy;
			}
		}

		public static void CanHarm(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = cast.CasterObject;
			if (!caster.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetFriendly;
			}
			else if (!target.CanBeHarmed)
			{
				failedReason = SpellFailedReason.NotHere;
			}
		}

		/// <summary>
		/// Can be used on either side
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="target"></param>
		/// <param name="failedReason"></param>
		public static void CanHarmOrHeal(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = cast.CasterObject;
			var isHarmful = cast.Spell.HasHarmfulEffects;
			var isHarmfulAndBeneficial = cast.Spell.HasHarmfulEffects == cast.Spell.HasBeneficialEffects;

			if (!isHarmfulAndBeneficial)
			{
				if (isHarmful != caster.MayAttack(target))
				{
					if (isHarmful)
					{
						failedReason = SpellFailedReason.TargetFriendly;
					}
					else
					{
						failedReason = SpellFailedReason.TargetEnemy;
					}
				}
				else if (isHarmful && !target.CanBeHarmed)
				{
					failedReason = SpellFailedReason.NotHere;
				}
			}
		}

		public static void IsAllied(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			//if (targets.Cast.Caster is Character && target is Character)
			//{
			//    var caster = (Character)targets.Cast.Caster;
			//    if (!caster.IsAlliedWith(target))
			//    {
			//        failedReason = SpellFailedReason.TargetNotInParty;
			//    }
			//}
			if (!cast.CasterObject.IsAlliedWith(target))
			{
				failedReason = SpellFailedReason.TargetNotInParty;
			}
		}

		public static void IsSameClass(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (cast.CasterUnit != null && target is Unit)
			{
				if (cast.CasterUnit.Class == ((Unit)target).Class)
				{
					return;
				}
			}
			failedReason = SpellFailedReason.BadTargets;
		}

		public static void IsSamePartyAndClass(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsAllied(target, ref failedReason);
			if (failedReason != SpellFailedReason.Ok)
				cast.IsSameClass(target, ref failedReason);
		}

		public static void IsInFrontEnemies(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsInFrontOfCaster(target, ref failedReason, true);
		}

		public static void IsInFrontFriends(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsInFrontOfCaster(target, ref failedReason, false);
		}

		public static void IsInFrontOfCaster(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason, bool harmful)
		{
			var caster = cast.CasterObject;
			if (caster.IsPlayer && !target.IsInFrontOf(caster))
			{
				failedReason = SpellFailedReason.NotInfront;
			}
			else
			{
				if (harmful != caster.MayAttack(target))
				{
					failedReason = harmful ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
				}
			}
		}

		/// <summary>
		/// Is caster behind target?
		/// </summary>
		public static void IsCasterBehind(this SpellCast cast, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (!cast.CasterObject.IsBehind(target))
			{
				failedReason = SpellFailedReason.NotBehind;
			}
			else
			{
				var harmful = cast.Spell.HasHarmfulEffects;
				if (harmful != cast.CasterObject.MayAttack(target))
				{
					failedReason = harmful ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
				}
			}
		}
		#endregion
	}
}