/*************************************************************************
 *
 *   file		: SpellTargetCollection.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 16:30:19 +0100 (lø, 30 jan 2010) $
 
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
using WCell.RealmServer.Spells.Targeting;
using WCell.Util;
using WCell.Util.Graphics;
using Cell.Core;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Spells
{

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

		/// <summary>
		/// Private ctor used by SpellTargetCollectionPool
		/// </summary>
		public SpellTargetCollection()
		{
			m_handlers = new List<SpellEffectHandler>(3);
		}

		public bool IsInitialized
		{
			get;
			private set;
		}

		public SpellEffectHandler FirstHandler
		{
			get { return m_handlers[0]; }
		}

		public SpellCast Cast
		{
			get { return m_handlers[0].Cast; }
		}

		/// <summary>
		/// Compares targets if the effect is to select a limited amount of targets from an arbitrary set.
		/// Mostly used by AI casters.
		/// For example, AI casters should select the most wounded as target for healing spells.
		/// </summary>
		public TargetEvaluator TargetEvaluator
		{
			get
			{
				var handler = FirstHandler;
				return handler.Effect.GetTargetEvaluator(handler.Cast.IsAICast);
			}
		}

		#region Add
		public SpellFailedReason AddAll(WorldObject[] forcedTargets)
		{
			IsInitialized = true;
			for (var j = 0; j < forcedTargets.Length; j++)
			{
				var target = forcedTargets[j];
				if (target.IsInContext)
				{
					var err = ValidateTargetForHandlers(target);
					if (err != SpellFailedReason.Ok)
					{
						LogManager.GetCurrentClassLogger().Warn(
							"{0} tried to cast spell \"{1}\" with forced target {2} which is not valid: {3}",
							Cast.CasterObject, Cast.Spell, target, err);
						if (!Cast.IsAoE)
						{
							return err;
						}
					}
					else
					{
						Add(target);
					}
				}
				else if (target.IsInWorld)
				{
					LogManager.GetCurrentClassLogger().Warn(
						"{0} tried to cast spell \"{1}\" with forced target {2} which is not in context",
						Cast.CasterObject, Cast.Spell, target);
				}
			}
			return SpellFailedReason.Ok;
		}

		int EvaluateTarget(WorldObject target)
		{
			var handler = FirstHandler;
			var cast = handler.Cast;
			var value = TargetEvaluator(handler, target);

			if (cast.IsAICast)
			{
				// TODO: Should AI prefer targets that do not already have an Aura?
				if (cast.Spell.IsAura && target is Unit)
				{
					var unit = (Unit)target;
					if (unit.Auras.Contains(cast.Spell))
					{
						// reduce target's value
						// number is arbitrarily chosen -> Can lead to issues!
						value += 1000000;
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Adds the given target.
		/// If an evaluator is set and the limit is reached, replace the worst existing target, if its worse than the new target.
		/// A *bigger* value returned by the evaluator is worse than a *smaller* one.
		/// </summary>
		public bool AddOrReplace(WorldObject target, int limit)
		{
			if (TargetEvaluator == null)
			{
				// no decision to be made
				Add(target);
				return Count < limit;
			}
			else
			{
				if (Count < limit)
				{
					// limit not reached -> Add without condition
					Add(target);
				}
				else
				{
					// add new target, only if its a better choice than what we had before
					var replacementValue = EvaluateTarget(target);
					var replacementIndex = -1;

					// Find target with greatest value and replace it
					for (var i = Count - 1; i >= 0; i--)
					{
						var existingTarget = this[i];
						var existingValue = EvaluateTarget(existingTarget);
						if (existingValue > replacementValue)
						{
							// possible replacement found
							replacementValue = existingValue;
							replacementIndex = i;
						}
					}

					if (replacementIndex > -1)
					{
						// replacement found
						this[replacementIndex] = target;
					}
				}
				// keep looking at all targets, since there might still be better choices out there
				return true;
			}
		}

		public void AddSingleTarget(WorldObject target)
		{
			if (TargetEvaluator != null)
			{
				log.Warn("Target Evaluator is not null, but only a single target adder was used - " +
					"Consider using an \"AddArea*\" adder to add the best choice from any possible nearby target for spell: " + FirstHandler.Effect.Spell);
			}
			Add(target);
		}
		#endregion

		#region Find Targets
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
			var def = firstEffect.GetTargetDefinition(cast.IsAICast);
			if (def != null)
			{
				var failedReason = SpellFailedReason.Ok;
				def.Collect(this, ref failedReason);
			}
			else
			{
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
			}
			return failReason;
		}

		public SpellFailedReason FindTargets(ImplicitSpellTargetType targetType)
		{
			var failedReason = SpellFailedReason.Ok;
			var def = DefaultTargetDefinitions.GetTargetDefinition(targetType);
			if (def != null)
			{
				def.Collect(this, ref failedReason);
			}
			return failedReason;
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
						AddOrReplace(target, limit);
					}
					//return Count < limit;
				}
				return true;
			});

			if (TargetEvaluator == null)
			{
				Sort((a, b) => a.GetDistanceSq(first).CompareTo(b.GetDistanceSq(first)));
				if (Count > limit)
				{
					RemoveRange(limit, Count - limit);
				}
			}
		}
		#endregion

		#region Validate targets
		/// <summary>
		/// Does default checks on whether the given Target is valid for the current SpellCast
		/// </summary>
		public SpellFailedReason ValidateTarget(WorldObject target, TargetFilter filter)
		{
			var firstHandler = FirstHandler;
			var cast = firstHandler.Cast;
			var spell = cast.Spell;

			var failReason = spell.CheckValidTarget(cast.CasterObject, target);
			if (failReason != SpellFailedReason.Ok)
			{
				return failReason;
			}

			if (filter != null)
			{
				filter(firstHandler, target, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}

			//if (cast.IsAICast && spell.IsAura && !spell.IsAreaSpell)
			//{
			//    // non-AoE aura -> Don't select target if target does already have the aura?
			//}

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

		/// <summary>
		/// Removes all targets that don't satisfy the effects' constraints
		/// </summary>
		public void RevalidateAll()
		{
			var firstHandler = FirstHandler;
			var cast = firstHandler.Cast;

			// remove all targets that are not valid anymore
			for (var i = Count - 1; i >= 0; i--)
			{
				var target = this[i];
				if (target.IsInWorld)
				{
					var def = firstHandler.Effect.GetTargetDefinition(cast.IsAICast);
					if (def != null)
					{
						// custom filter
						if (ValidateTarget(target, def.Filter) == SpellFailedReason.Ok)
						{
							continue;
						}
					}
					else
					{
						// two default filters
						if (ValidateTarget(target, DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetA)) == SpellFailedReason.Ok)
						{
							var filter = DefaultTargetDefinitions.GetTargetFilter(firstHandler.Effect.ImplicitTargetB);
							if (filter != null)
							{
								var failedReason = SpellFailedReason.Ok;
								filter(firstHandler, target, ref failedReason);
								if (failedReason == SpellFailedReason.Ok)
								{
									continue;
								}
							}
						}
					}
				}
				RemoveAt(i);
			}
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