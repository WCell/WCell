using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells
{
	public delegate void TargetAdder(SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason);
	public delegate void TargetFilter(SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failReason);
	public delegate void TargetComparer(WorldObject target1, WorldObject target2);

	#region TargetDefinition
	/// <summary>
	/// 
	/// </summary>
	public class TargetDefinition
	{
		//public readonly bool CheckInRange;
		public readonly TargetAdder Adder;
		public readonly TargetFilter Filter;
		public readonly TargetComparer Comparer;

		public TargetDefinition(TargetAdder adder, TargetFilter filter, TargetComparer comparer = null)
		{
			Adder = adder;
			Filter = filter;
			Comparer = comparer;
		}
	}
	#endregion

	#region Adders
	/// <summary>
	/// A srcCont for public extension methods for TargetCollection.
	/// 
	/// This is required in order to define default target-definitions 
	/// in a static context AND be able to invoke these methods as usual
	/// upon TargetCollection instances without reflection
	/// </summary>
	public static class DefaultTargetAdders
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Self
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
		#endregion

		#region Pet
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
		#endregion

		#region Selection
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
		#endregion

		#region AddChannelObject
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
		#endregion

		#region AoE
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
		#endregion

		#region Party
		public static void AddAllParty(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var cast = targets.Cast;
			if (cast.CasterChar != null)
			{
				// For Characters: Add the whole party
				if (targets.Cast.CasterChar.Group != null)
				{
					foreach (var member in cast.CasterChar.Group)
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
				targets.AddAreaSource(cast.Spell.HasHarmfulEffects ? (TargetFilter)DefaultTargetFilters.IsFriendly : DefaultTargetFilters.IsHostile, ref failReason, radius);
			}
		}
		#endregion

		#region Items & Objects
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
		#endregion

		/// <summary>
		/// Your current summon or self
		/// </summary>
		public static void AddSummon(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			// if (Handler.SpellCast.Caster.Summon == null)
			targets.AddSelf(filter, ref failReason);
		}

		/// <summary>
		/// 
		/// </summary>
		public static void AddNearestUnit(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.CasterObject == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = targets.Cast.CasterObject.GetNearestUnit(targets.FirstHandler.GetRadius(), unit =>
				targets.ValidateTarget(unit, filter) == SpellFailedReason.Ok);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public static void AddNearestPlayer(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.CasterObject == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = targets.Cast.CasterObject.GetNearestUnit(targets.FirstHandler.GetRadius(), unit =>
				unit is Character && targets.ValidateTarget(unit, filter) == SpellFailedReason.Ok);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void AddRandomUnit(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.CasterObject == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = targets.Cast.CasterObject.GetRandomVisibleUnit(targets.FirstHandler.GetRadius(), unit =>
				targets.ValidateTarget(unit, filter) == SpellFailedReason.Ok);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void AddRandomPlayer(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			if (targets.Cast.CasterObject == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = targets.Cast.CasterObject.GetRandomVisibleUnit(targets.FirstHandler.GetRadius(), unit =>
				unit is Character && targets.ValidateTarget(unit, filter) == SpellFailedReason.Ok);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void AddSecondHighestThreatTarget(this SpellTargetCollection targets, TargetFilter filter, ref SpellFailedReason failReason)
		{
			var caster = targets.Cast.CasterUnit as NPC;
			if (caster == null)
			{
				failReason = SpellFailedReason.NoValidTargets;
				return;
			}

			var nearest = caster.ThreatCollection.GetAggressorByThreatRank(2);

			if (nearest != null)
			{
				targets.Add(nearest);
			}
			else
			{
				failReason = SpellFailedReason.NoValidTargets;
			}
		}
	}
	#endregion

	#region Filters
	public static class DefaultTargetFilters
	{
		public static void IsFriendly(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (cast.CasterObject.MayAttack(target))
			{
				failedReason = SpellFailedReason.TargetEnemy;
			}
		}

		public static void IsHostile(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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
		public static void IsHostileOrHeal(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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

		public static void IsAllied(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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

		public static void IsSameClass(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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

		public static void IsSamePartyAndClass(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsAllied(effectHandler, target, ref failedReason);
			if (failedReason != SpellFailedReason.Ok)
				cast.IsSameClass(effectHandler, target, ref failedReason);
		}

		public static void IsInFrontAndHostile(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsInFrontOfCaster(effectHandler, target, ref failedReason, IsHostile);
		}

		public static void IsInFrontAndFriendly(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			cast.IsInFrontOfCaster(effectHandler, target, ref failedReason, IsFriendly);
		}

		public static void IsInFrontOfCaster(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason, TargetFilter filter)
		{
			var caster = cast.CasterObject;
			if (caster.IsPlayer && !target.IsInFrontOf(caster))
			{
				failedReason = SpellFailedReason.NotInfront;
			}
			else
			{
				filter(cast, effectHandler, target, ref failedReason);
			}
		}

		/// <summary>
		/// Is caster behind target?
		/// </summary>
		public static void IsCasterBehind(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
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
	}
	#endregion

	/// <summary>
	/// Set of methods to evaluate whether a target is "good" for an AI caster
	/// </summary>
	public static class DefaultAITargetComparers
	{
		/// <summary>
		/// Heal spells
		/// </summary>
		public static bool IsWounded(this SpellCast cast, SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			if (target is Unit)
			{
				// useful heal spell: Amount to be healed is greater or equal half of what the spell can heal
				var unit = (Unit)target;
				var missingHealth = unit.MaxHealth - unit.Health;
				return missingHealth >= effectHandler.CalcEffectValue() / 2;
			}
			return false;
		}
	}
}