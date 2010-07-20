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
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Extensions;
using WCell.Util;
using WCell.Util.Graphics;
using Cell.Core;

namespace WCell.RealmServer.Spells
{
	public delegate void TargetFilter(SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failReason);

	/// <summary>
	/// A list of all targets for a spell.
	/// 
	/// TODO: Add only subgroups for party targeting?
	/// </summary>
	public class SpellTargetCollection : List<WorldObject>
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		public static readonly ObjectPool<SpellTargetCollection> SpellTargetCollectionPool = ObjectPoolMgr.CreatePool(() => new SpellTargetCollection());

		/// <summary>
		/// Further SpellEffectHandlers that are sharing this TargetCollection with the original Handler
		/// </summary>
		internal List<SpellEffectHandler> m_handlers;
		bool m_initialized;

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
			m_handlers = new List<SpellEffectHandler>(5);
		}

		public SpellFailedReason FindAllTargets()
		{
			if (m_initialized)
			{
				return SpellFailedReason.Ok;
			}
			m_initialized = true;

			var firstEffect = FirstHandler.Effect;
			if (firstEffect.Spell.IsPreventionDebuff)
			{
				// need to call CheckValidTarget nevertheless
				var caster = Cast.Caster;
				var err = SpellFailedReason.Ok;
				foreach (var handler in m_handlers)
				{
					err = handler.CheckValidTarget(caster);
					if (err != SpellFailedReason.Ok)
					{
						return err;
					}
				}
				Add(caster);
				return err;
			}

			var failReason = SpellFailedReason.Ok;
			if (firstEffect.ImplicitTargetA != ImplicitTargetType.None)
			{
				failReason = FindTargets(firstEffect.ImplicitTargetA);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}
			if (firstEffect.ImplicitTargetB != ImplicitTargetType.None)
			{
				failReason = FindTargets(firstEffect.ImplicitTargetB);
			}
			return failReason;
		}

		public SpellFailedReason FindTargets(ImplicitTargetType targetType)
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
			var handler = FirstHandler;
			var caster = handler.Cast.Caster;
			var spell = handler.Effect.Spell;

			if (!target.CheckObjType(handler.TargetType))
			{
				return SpellFailedReason.BadTargets;
			}

			if ((spell.FacingFlags & SpellFacingFlags.RequiresInFront) != 0 && !target.IsInFrontOf(caster))
			{
				return SpellFailedReason.NotInfront;
			}

			var failReason = spell.CheckValidTarget(caster, target);
			if (failReason != SpellFailedReason.Ok)
			{
				return failReason;
			}

			if (filter != null)
			{
				filter(this, target, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}

			return m_handlers.ValidateTargets(target);
		}

		public static SpellFailedReason ValidateTarget(WorldObject target, SpellEffectHandler handler)
		{
			var spell = handler.Effect.Spell;

			if (!target.CheckObjType(handler.TargetType))
			{
				return SpellFailedReason.BadTargets;
			}

			var caster = handler.Cast.Caster;
			var failReason = spell.CheckValidTarget(caster, target);

			if (failReason != SpellFailedReason.Ok)
			{
				return failReason;
			}

			return handler.CheckValidTarget(target);
		}

		public void AddTargetsInArea(Vector3 pos, TargetFilter targetFilter, float radius)
		{
			var limit = (int)FirstHandler.Effect.Spell.MaxTargets;
			if (limit < 1)
			{
				limit = int.MaxValue;
			}

			Cast.Caster.Region.IterateObjects(ref pos, radius > 0 ? radius : 5,
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
			var caster = handler.Cast.Caster;

			first.IterateEnvironment(handler.GetRadius(), target =>
			{
				if (target != caster &&
					target != first &&
					((harmful && caster.MayAttack(target)) ||
					(!harmful && caster.IsInSameDivision(target))) &&
					ValidateTarget(target, filter) == SpellFailedReason.Ok)
				{
					Add(target);
					//return Count < limit;
				}
				return true;
			});

			// TODO: Consider better ways to do this
			Sort((a, b) => a.GetDistanceSq(first).CompareTo(b.GetDistanceSq(first)));
			if (Count > limit)
			{
				RemoveRange(limit, Count - limit);
			}
		}

		#region Handlers
		internal delegate void TargetAdder(SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason);
		internal delegate void TargetHandler();

		private static readonly TargetDefinition[] targetHandlers =
			new TargetDefinition[(int)Utility.GetMaxEnum<ImplicitTargetType>() + 1];

		private struct TargetDefinition
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
			targetHandlers[(int)ImplicitTargetType.AllAroundLocation] = new TargetDefinition(
					TargetMethods.AddAreaDest,	// Is this right?
					null);

			targetHandlers[(int)ImplicitTargetType.AllEnemiesAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitTargetType.AllEnemiesInArea] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitTargetType.AllEnemiesInAreaInstant] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.CanHarm);

			// ImplicitTargetType.AllFriendlyInAura

			targetHandlers[(int)ImplicitTargetType.AllParty] = new TargetDefinition(
					TargetMethods.AddAllParty,
					null);

			targetHandlers[(int)ImplicitTargetType.AllPartyAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsAllied);

			// Seems to be bogus: Often used together with AllEnemiesAroundCaster
			targetHandlers[(int)ImplicitTargetType.AllPartyInArea] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.IsAllied);

			targetHandlers[(int)ImplicitTargetType.AllPartyInAreaChanneled] = new TargetDefinition(
					TargetMethods.AddAreaDest,
					TargetMethods.IsAllied);

			// Odd: Mostly in combination with LocationToSummon and TeleportLocation
			// The only spell that has this with a negative effect is: Goblin Mortar (Id: 13238, Target: Default)
			targetHandlers[(int)ImplicitTargetType.AllTargetableAroundLocationInRadiusOverTime] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitTargetType.AreaEffectPartyAndClass] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsSamePartyAndClass);

			//targetHandlers.Add(ImplicitTargetType.BehindTargetLocation,
			//    new TargetDefinition(
			//        TargetMethods.AddSelection,
			//        TargetMethods.IsBehind));

			// ImplicitTargetType.CaliriEggs

			// any kind of chain effect
			targetHandlers[(int)ImplicitTargetType.Chain] = new TargetDefinition(
					TargetMethods.AddChain,
					null);

			targetHandlers[(int)ImplicitTargetType.CurrentSelection] = new TargetDefinition(
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

			// TODO: Can be beneficial and harmful at the same time
			targetHandlers[(int)ImplicitTargetType.Duel] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarmOrHeal);

			targetHandlers[(int)ImplicitTargetType.DynamicObject] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// Some poision effects which always have other targets set
			// ImplicitTargetType.EnemiesInAreaChanneledWithExceptions

			targetHandlers[(int)ImplicitTargetType.GameObject] = new TargetDefinition(
					TargetMethods.AddObject,
					null);

			targetHandlers[(int)ImplicitTargetType.GameObjectOrItem] = new TargetDefinition(
					TargetMethods.AddItemOrObject,
					null);

			// ImplicitTargetType.HeartstoneLocation

			targetHandlers[(int)ImplicitTargetType.InFrontOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsInFrontEnemies);

			targetHandlers[(int)ImplicitTargetType.ConeInFrontOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsInFrontEnemies);

			targetHandlers[(int)ImplicitTargetType.InvisibleOrHiddenEnemiesAtLocationRadius] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					null);

			//targetHandlers[(int)ImplicitTargetType.LocationInFrontCaster] = new TargetDefinition(
			//        TargetMethods.AddAreaCaster,
			//        TargetMethods.IsInFrontFriends);

			// ImplicitTargetType.LocationInFrontCasterAtRange

			targetHandlers[(int)ImplicitTargetType.LocationNearCaster] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// If not Demon and ObjectWild, add all around?
			//targetHandlers.Add(ImplicitTargetType.LocationToSummon,
			//    new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null));

			targetHandlers[(int)ImplicitTargetType.Minion] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.MultipleGuardianSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.MultipleSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			//targetHandlers[(int)ImplicitTargetType.MultipleSummonPetLocation] = new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null);

			targetHandlers[(int)ImplicitTargetType.NatureSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.NetherDrakeSummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.Pet] = new TargetDefinition(
					TargetMethods.AddPet,
					null);

			// default
			// TODO: What happens if items/gameobjects are involved?
			targetHandlers[(int)ImplicitTargetType.None] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			// odd:
			targetHandlers[(int)ImplicitTargetType.PartyMember] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitTargetType.PartyAroundCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitTargetType.ScriptedOrSingleTarget] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitTargetType.SelectedEnemyChanneled] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitTargetType.Self] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.SingleEnemy] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitTargetType.SingleFriend] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsFriendly);

			targetHandlers[(int)ImplicitTargetType.SingleParty] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.IsAllied);

			targetHandlers[(int)ImplicitTargetType.SpreadableDesease] = new TargetDefinition(
					TargetMethods.AddSelection,
					TargetMethods.CanHarm);

			targetHandlers[(int)ImplicitTargetType.SummonLocation] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.TargetAtOrientationOfCaster] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsInFrontEnemies);

			// ImplicitTargetType.TargetForVisualEffect

			// ImplicitTargetType.TeleportLocation, <- Used for Summon spells

			// Totem summoning
			targetHandlers[(int)ImplicitTargetType.TotemAir] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.TotemEarth] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.TotemFire] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.TotemWater] = new TargetDefinition(
					TargetMethods.AddSelf,
					null);

			targetHandlers[(int)ImplicitTargetType.Tranquility] = new TargetDefinition(
					TargetMethods.AddAreaCaster,
					TargetMethods.IsAllied);
		}
		#endregion

		public delegate void TargetHandler<T>(T target);

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
			if (!m_initialized)
			{
				return;
			}

			m_initialized = false;

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
		private static Logger log = LogManager.GetCurrentClassLogger();

		#region Add Methods
		public static void AddSelf(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var self = targets.Cast.Caster;
			foreach (var handler in targets.m_handlers)
			{
				if ((failReason = handler.CheckValidTarget(self)) != SpellFailedReason.Ok)
				{
					return;
				}
			}
			targets.Add(self);
		}

		public static void AddPet(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.Caster;
			if (!(caster is Character))
			{
				log.Warn("Non-Player {0} tried to cast Pet - spell {1}", caster, targets.Cast.Spell);
				failReason = SpellFailedReason.TargetNotPlayer;
			}

			var pet = ((Character)caster).ActivePet;
			if (pet == null)
			{
				failReason = SpellFailedReason.NoPet;
				return;
			}

			foreach (var handler in targets.m_handlers)
			{
				if ((failReason = handler.CheckValidTarget(pet)) != SpellFailedReason.Ok)
				{
					return;
				}
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

			var caster = cast.Caster as Unit;
			if (caster != null)
			{
				var selected = cast.Selected;
				if (selected == null)
				{
					selected = caster.Target;
					if (selected == null)
					{
						failReason = SpellFailedReason.BadTargets;
						return;
					}
				}

				var effect = targets.FirstHandler.Effect;
				var spell = effect.Spell;
				if (selected != caster)
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
						if (caster is Character)
						{
							chainCount = ((Character) caster).PlayerSpells.GetModifiedInt(SpellModifierType.ChainTargets, spell, chainCount);
						}
						if (chainCount > 1 && selected is Unit)
						{
							targets.FindChain((Unit)selected, filter, true, chainCount);
						}
					}
				}
			}
			else
			{
				failReason = SpellFailedReason.Error;
			}
		}

		/// <summary>
		/// Adds targets around the caster
		/// </summary>
		/// <param name="targets"></param>
		/// <param name="filter"></param>
		/// <param name="failReason"></param>
		public static void AddAreaCaster(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaCaster(targets, filter, ref failReason, targets.FirstHandler.GetRadius());
		}

		public static void AddAreaCaster(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason, float radius)
		{
			var caster = targets.Cast.Caster;
			targets.AddTargetsInArea(caster.Position, filter, radius);
		}

		public static void AddAreaSrc(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			AddAreaCaster(targets, filter, ref failReason);
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
				targets.AddAreaCaster(IsFriendly, ref failReason, radius);
			}
		}

		/// <summary>
		/// Used for Lock picking and opening (with or without keys)
		/// </summary>
		public static void AddItemOrObject(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.UsedItem == null && !(targets.Cast.Selected is GameObject))
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
		public static void IsFriendly(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (targets.Cast.Caster.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetEnemy;
			}
		}

		public static void CanHarm(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			var caster = targets.Cast.Caster;
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
		public static void CanHarmOrHeal(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			var cast = targets.Cast;
			var caster = cast.Caster;
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

		public static void IsAllied(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			//if (targets.Cast.Caster is Character && target is Character)
			//{
			//    var caster = (Character)targets.Cast.Caster;
			//    if (!caster.IsAlliedWith(target))
			//    {
			//        failedReason = SpellFailedReason.TargetNotInParty;
			//    }
			//}
			if (!targets.Cast.Caster.IsAlliedWith(target))
			{
				failedReason = SpellFailedReason.TargetNotInParty;
			}
		}

		public static void IsSameClass(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (targets.Cast.CasterUnit != null && target is Unit)
			{
				if (targets.Cast.CasterUnit.Class == ((Unit)target).Class)
				{
					return;
				}
			}
			failedReason = SpellFailedReason.BadTargets;
		}

		public static void IsSamePartyAndClass(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			targets.IsAllied(target, ref failedReason);
			if (failedReason != SpellFailedReason.Ok)
				targets.IsSameClass(target, ref failedReason);
		}

		public static void IsInFrontEnemies(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			targets.IsInFront(target, ref failedReason, true);
		}

		public static void IsInFrontFriends(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			targets.IsInFront(target, ref failedReason, false);
		}

		public static void IsInFront(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason, bool harmful)
		{
			var caster = targets.Cast.Caster;
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
		public static void IsBehind(this SpellTargetCollection targets, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (!targets.Cast.Caster.IsBehind(target))
			{
				failedReason = SpellFailedReason.NotBehind;
			}
			else
			{
				var harmful = targets.FirstHandler.Effect.Spell.HasHarmfulEffects;
				if (harmful != targets.Cast.Caster.MayAttack(target))
				{
					failedReason = harmful ? SpellFailedReason.TargetFriendly : SpellFailedReason.TargetEnemy;
				}
			}
		}

		public static SpellFailedReason ValidateTargets(this List<SpellEffectHandler> handlers, WorldObject target)
		{
			for (var i = 0; i < handlers.Count; i++)
			{
				var hdlr = handlers[i];
				var failReason = hdlr.CheckValidTarget(target);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}
			return SpellFailedReason.Ok;
		}
		#endregion
	}
}