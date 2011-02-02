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
using WCell.Util;
using WCell.Util.Graphics;
using Cell.Core;

namespace WCell.RealmServer.Spells
{

	/// <summary>
	/// A list of all targets for a Spell
	/// </summary>
	public class SpellTargetCollection : List<WorldObject>
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static readonly TargetDefinition[] DefaultTargetHandlers = new TargetDefinition[(int)Utility.GetMaxEnum<ImplicitSpellTargetType>() + 1];

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

		public SpellEffectHandler FirstHandler
		{
			get { return m_handlers[0]; }
		}

		public SpellCast Cast
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
				if (def.Adder != null)
				{
					def.Adder(this, def.Filter, ref failedReason);
				}
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
			var def = GetTargetDefinition(targetType);
			if (def.Adder != null)
			{
				def.Adder(this, def.Filter, ref failedReason);
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
				filter(cast, firstHandler, target, ref failReason);
				if (failReason != SpellFailedReason.Ok)
				{
					return failReason;
				}
			}

			if (cast.IsAICast)
			{
				
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
					if (ValidateTarget(target, GetTargetFilter(firstHandler.Effect.ImplicitTargetA)) == SpellFailedReason.Ok)
					{
						var filter = GetTargetFilter(firstHandler.Effect.ImplicitTargetB);
						if (filter != null)
						{
							var failedReason = SpellFailedReason.Ok;
							filter(cast, firstHandler, target, ref failedReason);
							if (failedReason == SpellFailedReason.Ok)
							{
								continue;
							}
						}
					}
				}
				RemoveAt(i);
			}
		}

		#endregion

		#region Add Targets
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
		#endregion

		#region Handlers

		/// <summary>
		/// Returns the handler and filter for the given target type
		/// </summary>
		public static TargetDefinition GetTargetDefinition(ImplicitSpellTargetType target)
		{
			return DefaultTargetHandlers[(int)target];
		}

		public static TargetFilter GetTargetFilter(ImplicitSpellTargetType target)
		{
			var def = DefaultTargetHandlers[(int)target];
			return def != null ? def.Filter : null;
		}

		static SpellTargetCollection()
		{
			InitTargetHandlers();
		}

		private static void InitTargetHandlers()
		{
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllAroundLocation] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,	// Is this right?
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInArea] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllEnemiesInAreaInstant] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			// ImplicitTargetType.AllFriendlyInAura

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllParty] = new TargetDefinition(
					DefaultTargetAdders.AddAllParty,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);

			// Seems to be bogus: Often used together with AllEnemiesAroundCaster
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyInArea] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllPartyInAreaChanneled] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsAllied);

			// Odd: Mostly in combination with LocationToSummon and TeleportLocation
			// The only spell that has this with a negative effect is: Goblin Mortar (Id: 13238, Target: Default)
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AllTargetableAroundLocationInRadiusOverTime] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.AreaEffectPartyAndClass] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsSamePartyAndClass);

			//targetHandlers.Add(ImplicitTargetType.BehindTargetLocation,
			//    new TargetDefinition(
			//        TargetMethods.AddSelection,
			//        DefaultTargetFilters.IsBehind));

			// ImplicitTargetType.CaliriEggs

			// any kind of chain effect
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Chain] = new TargetDefinition(
					DefaultTargetAdders.AddChain,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.CurrentSelection] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
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

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Duel] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostileOrHeal);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.DynamicObject] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// Some poision effects which always have other targets set
			// ImplicitTargetType.EnemiesInAreaChanneledWithExceptions

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.GameObject] = new TargetDefinition(
					DefaultTargetAdders.AddObject,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.GameObjectOrItem] = new TargetDefinition(
					DefaultTargetAdders.AddItemOrObject,
					null);

			// ImplicitTargetType.HeartstoneLocation

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.InFrontOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.ConeInFrontOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					null);

			//targetHandlers[(int)ImplicitTargetType.LocationInFrontCaster] = new TargetDefinition(
			//        TargetMethods.AddAreaCaster,
			//        DefaultTargetFilters.IsInFrontFriends);

			// ImplicitTargetType.LocationInFrontCasterAtRange

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.LocationNearCaster] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// If not Demon and ObjectWild, add all around?
			//targetHandlers.Add(ImplicitTargetType.LocationToSummon,
			//    new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null));

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Minion] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.MultipleGuardianSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.MultipleSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			//targetHandlers[(int)ImplicitTargetType.MultipleSummonPetLocation] = new TargetDefinition(
			//        TargetMethods.AddSelf,
			//        null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.NatureSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddAreaDest,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.NetherDrakeSummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Pet] = new TargetDefinition(
					DefaultTargetAdders.AddPet,
					null);

			// default
			// TODO: What happens if items/gameobjects are involved?
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.None] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			// odd:
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.PartyMember] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.PartyAroundCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.ScriptedOrSingleTarget] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SelectedEnemyChanneled] = new TargetDefinition(
					DefaultTargetAdders.AddChannelObject,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Self] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleEnemy] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleFriend] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsFriendly);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SingleParty] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsAllied);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SpreadableDesease] = new TargetDefinition(
					DefaultTargetAdders.AddSelection,
					DefaultTargetFilters.IsHostile);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.SummonLocation] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TargetAtOrientationOfCaster] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsInFrontAndHostile);

			// ImplicitTargetType.TargetForVisualEffect

			// ImplicitTargetType.TeleportLocation, <- Used for Summon spells

			// Totem summoning
			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemAir] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemEarth] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemFire] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.TotemWater] = new TargetDefinition(
					DefaultTargetAdders.AddSelf,
					null);

			DefaultTargetHandlers[(int)ImplicitSpellTargetType.Tranquility] = new TargetDefinition(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetFilters.IsAllied);
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