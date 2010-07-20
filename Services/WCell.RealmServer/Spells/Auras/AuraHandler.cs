/*************************************************************************
 *
 *   file		: AuraHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-31 03:46:31 +0100 (sø, 31 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1238 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants.Spells;
using WCell.RealmServer.Auras.Effects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Mod;
using WCell.RealmServer.Spells.Auras.Passive;
using WCell.Util;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells.Auras
{
	public delegate AuraEffectHandler AuraEffectHandlerCreator();
	public delegate bool AuraIdEvaluator(Spell spell);

	/// <summary>
	/// Static Aura helper class
	/// 
	/// TODO: CMSG_PET_CANCEL_AURA
	/// TODO: CMSG_CANCEL_GROWTH_AURA unused?
	/// </summary>
	public static partial class AuraHandler
	{
		/// <summary>
		///  The maximum amount of positive Auras
		/// </summary>
		public const int MaxPositiveAuras = 28;

		/// <summary>
		///  The maximum amount of any kind of Auras
		/// </summary>
		public const int MaxAuras = 56;

		#region AuraEffectHandler Creation
		public static readonly AuraEffectHandlerCreator[] EffectHandlers =
			new AuraEffectHandlerCreator[(int)Convert.ChangeType(Utility.GetMaxEnum<AuraType>(), typeof(int)) + 1];

		static AuraHandler()
		{
			EffectHandlers[(int)AuraType.None] = () => new AuraVoidHandler();
			EffectHandlers[(int)AuraType.PeriodicDamage] = () => new PeriodicDamageHandler();
			EffectHandlers[(int)AuraType.Dummy] = () => new DummyHandler();
			EffectHandlers[(int)AuraType.PeriodicHeal] = () => new PeriodicHealHandler();
			EffectHandlers[(int)AuraType.ModAttackSpeed] = () => new ModAttackSpeedHandler();
			EffectHandlers[(int)AuraType.ModThreat] = () => new ModThreatHandler();
			EffectHandlers[(int)AuraType.ModStun] = () => new StunHandler();
			EffectHandlers[(int)AuraType.ModDamageDone] = () => new ModDamageDoneHandler();
			EffectHandlers[(int)AuraType.ModStealth] = () => new StealthHandler();
			EffectHandlers[(int)AuraType.ModInvisibility] = () => new ModInvisibilityHandler();
			EffectHandlers[(int)AuraType.RegenPercentOfTotalHealth] = () => new RegenPercentOfTotalHealthHandler();
			EffectHandlers[(int)AuraType.RegenPercentOfTotalMana] = () => new RegenPercentOfTotalManaHandler();
			EffectHandlers[(int)AuraType.ModResistance] = () => new ModResistanceHandler();
			EffectHandlers[(int)AuraType.ModResistancePercent] = () => new ModResistancePctHandler();
			EffectHandlers[(int)AuraType.PeriodicTriggerSpell] = () => new PeriodicTriggerSpellHandler();
			EffectHandlers[(int)AuraType.PeriodicEnergize] = () => new PeriodicEnergizeHandler();
			EffectHandlers[(int)AuraType.ModRoot] = () => new RootHandler();
			EffectHandlers[(int)AuraType.ModStat] = () => new ModStatHandler();
			EffectHandlers[(int)AuraType.ModStatPercent] = () => new ModStatPercentHandler();
			EffectHandlers[(int)AuraType.ModTotalStatPercent] = () => new ModTotalStatPercentHandler();
			EffectHandlers[(int)AuraType.ModIncreaseSpeed] = () => new ModIncreaseSpeedHandler();
			EffectHandlers[(int)AuraType.ModIncreaseMountedSpeed] = () => new ModIncreaseMountedSpeedHandler();
			EffectHandlers[(int)AuraType.ModDecreaseSpeed] = () => new ModDecreaseSpeedHandler();
			EffectHandlers[(int)AuraType.ModIncreaseEnergy] = () => new ModIncreaseEnergyHandler();
			EffectHandlers[(int)AuraType.ModShapeshift] = () => new ShapeshiftHandler();
			EffectHandlers[(int)AuraType.ModHealingPercent] = () => new ModHealingPercentHandler();
			EffectHandlers[(int)AuraType.ModHealingDone] = () => new ModHealingDoneHandler();
			EffectHandlers[(int)AuraType.SchoolImmunity] = () => new SchoolImmunityHandler();
			EffectHandlers[(int)AuraType.DamageImmunity] = () => new DamageImmunityHandler();
			EffectHandlers[(int)AuraType.DispelImmunity] = () => new DispelImmunityHandler();
			EffectHandlers[(int)AuraType.ProcTriggerSpell] = () => new ProcTriggerSpellHandler();
			EffectHandlers[(int)AuraType.ProcTriggerDamage] = () => new ProcTriggerDamageHandler();
			EffectHandlers[(int)AuraType.TrackCreatures] = () => new TrackCreaturesHandler();
			EffectHandlers[(int)AuraType.TrackResources] = () => new TrackResourcesHandler();
			EffectHandlers[(int)AuraType.ModDodgePercent] = () => new ModDodgePercentHandler();
			EffectHandlers[(int)AuraType.ModBlockSkill] = () => new ModBlockSkillHandler();
			EffectHandlers[(int)AuraType.ModBlockPercent] = () => new ModBlockPercentHandler();
			EffectHandlers[(int)AuraType.ModParryPercent] = () => new ModParryPercentHandler();
			EffectHandlers[(int)AuraType.ModShieldBlockvaluePct] = () => new ModShieldBlockValuePercentHandler();
			EffectHandlers[(int)AuraType.ModCritPercent] = () => new ModCritPercentHandler();
			EffectHandlers[(int)AuraType.PeriodicLeech] = () => new PeriodicLeechHandler();
			EffectHandlers[(int)AuraType.Transform] = () => new TransformHandler();
			EffectHandlers[(int)AuraType.ModSpellCritChance] = () => new ModSpellCritChanceHandler();
			EffectHandlers[(int)AuraType.ModIncreaseSwimSpeed] = () => new ModIncreaseSwimSpeedHandler();
			EffectHandlers[(int)AuraType.ModScale] = () => new ModScaleHandler();
			EffectHandlers[(int)AuraType.PeriodicHealthFunnel] = () => new PeriodicHealthFunnelHandler();
			EffectHandlers[(int)AuraType.PeriodicManaLeech] = () => new PeriodicManaLeechHandler();
			EffectHandlers[(int)AuraType.ModCastingSpeed] = () => new ModCastingSpeedHandler();
			EffectHandlers[(int)AuraType.ModDisarm] = () => new DisarmHandler();
			EffectHandlers[(int)AuraType.SchoolAbsorb] = () => new SchoolAbsorbHandler();
			EffectHandlers[(int)AuraType.ModSpellCritChanceForSchool] = () => new ModSpellCritChanceForSchoolHandler();
			EffectHandlers[(int)AuraType.ModPowerCost] = () => new ModPowerCostHandler();
			EffectHandlers[(int)AuraType.ModPowerCostForSchool] = () => new ModPowerCostForSchoolHandler();
			EffectHandlers[(int)AuraType.ModLanguage] = () => new ModLanguageHandler();
			EffectHandlers[(int)AuraType.MechanicImmunity] = () => new MechanicImmunityHandler();
			EffectHandlers[(int)AuraType.Mounted] = () => new MountedHandler();
			EffectHandlers[(int)AuraType.ModDamageDonePercent] = () => new ModDamageDonePercentHandler();
			EffectHandlers[(int)AuraType.SplitDamage] = () => new SplitDamageHandler();
			EffectHandlers[(int)AuraType.ModPowerRegen] = () => new ModPowerRegenHandler();
			EffectHandlers[(int)AuraType.CreateItemOnTargetDeath] = () => new CreateItemOnTargetDeathHandler();
			EffectHandlers[(int)AuraType.PeriodicDamagePercent] = () => new PeriodicDamagePercentHandler();
			EffectHandlers[(int)AuraType.InterruptRegen] = () => new InterruptRegenHandler();
			EffectHandlers[(int)AuraType.Ghost] = () => new GhostHandler();
			EffectHandlers[(int)AuraType.ManaShield] = () => new ManaShieldHandler();
			EffectHandlers[(int)AuraType.ModSkillTalent] = () => new ModSkillTalentHandler();
			EffectHandlers[(int)AuraType.ModAttackPower] = () => new ModMeleeAttackPowerHandler();
			EffectHandlers[(int)AuraType.ModAttackPowerPercent] = () => new ModMeleeAttackPowerPercentHandler();
			EffectHandlers[(int)AuraType.ModRangedAttackPower] = () => new ModRangedAttackPowerHandler();
			EffectHandlers[(int)AuraType.ModRangedAttackPowerPercent] = () => new ModRangedAttackPowerPercentHandler();
			EffectHandlers[(int)AuraType.WaterWalk] = () => new WaterWalkHandler();
			EffectHandlers[(int)AuraType.Hover] = () => new HoverHandler();
			EffectHandlers[(int)AuraType.AddModifierFlat] = () => new AddModifierFlatHandler();
			EffectHandlers[(int)AuraType.AddModifierPercent] = () => new AddModifierPercentHandler();
			EffectHandlers[(int)AuraType.AddTargetTrigger] = () => new AddTargetTriggerHandler();
			EffectHandlers[(int)AuraType.ModPowerRegenPercent] = () => new ModPowerRegenPercentHandler();
			EffectHandlers[(int)AuraType.AddCasterHitTrigger] = () => new AddCasterHitTriggerHandler();
			EffectHandlers[(int)AuraType.ModMechanicResistance] = () => new ModMechanicResistanceHandler();
			EffectHandlers[(int)AuraType.Untrackable] = () => new UntrackableHandler();
			EffectHandlers[(int)AuraType.ModTargetResistance] = () => new ModTargetResistanceHandler();
			EffectHandlers[(int)AuraType.ModIncreaseSpeedAlways] = () => new ModIncreaseSpeedAlwaysHandler();
			EffectHandlers[(int)AuraType.ModMountedSpeedAlways] = () => new ModMountedSpeedAlwaysHandler();
			EffectHandlers[(int)AuraType.ModIncreaseEnergyPercent] = () => new ModIncreaseEnergyPercentHandler();
			EffectHandlers[(int)AuraType.ModIncreaseHealthPercent] = () => new ModIncreaseHealthPercentHandler();
			EffectHandlers[(int)AuraType.ModManaRegenInterrupt] = () => new ModManaRegenInterruptHandler();
			EffectHandlers[(int)AuraType.ModHealingTaken] = () => new ModHealingTakenHandler();
			EffectHandlers[(int)AuraType.ModHaste] = () => new ModHasteHandler();
			EffectHandlers[(int)AuraType.ModSpecificCombatRating] = () => new ModCombatRatingStat();
			EffectHandlers[(int)AuraType.ModBaseResistancePercent] = () => new ModBaseResistancePercentHandler();
			EffectHandlers[(int)AuraType.ModResistanceExclusive] = () => new ModResistanceExclusiveHandler();
			EffectHandlers[(int)AuraType.SafeFall] = () => new SafeFallHandler();
			EffectHandlers[(int)AuraType.RetainComboPoints] = () => new RetainComboPointsHandler();
			EffectHandlers[(int)AuraType.ModResistSpellInterruptionPercent] = () => new ModResistSpellInterruptionPercentHandler();
			EffectHandlers[(int)AuraType.ModHealthRegenInCombat] = () => new ModHealthRegenInCombatHandler();
			EffectHandlers[(int)AuraType.PowerBurn] = () => new PowerBurnHandler();
			EffectHandlers[(int)AuraType.ModDebuffResistancePercent] = () => new ModDebuffResistancePercentHandler();
			EffectHandlers[(int)AuraType.ModRating] = () => new ModRatingHandler();
			EffectHandlers[(int)AuraType.ModTimeBetweenAttacks] = () => new ModTimeBetweenAttacksHandler();
			EffectHandlers[(int)AuraType.ModAllCooldownDuration] = () => new ModAllCooldownDurationHandler();
			EffectHandlers[(int)AuraType.ModAttackerCritChancePercent] = () => new ModAttackerCritChancePercentHandler();
			EffectHandlers[(int)AuraType.Fly] = () => new FlyHandler();
			EffectHandlers[(int)AuraType.ModRangedAttackPowerByPercentOfIntellect] =
				() => new ModRangedAttackPowerByPercentOfIntellectHandler();
			EffectHandlers[(int)AuraType.ModSpellHastePercent] = () => new ModSpellHastePercentHandler();
			EffectHandlers[(int)AuraType.ModManaRegen] = () => new ModManaRegenHandler();
			EffectHandlers[(int)AuraType.ModMaxHealth] = () => new ModMaxHealthHandler();
			EffectHandlers[(int)AuraType.ModSilenceDurationPercent] = () => new ModSilenceDurationPercentHandler();
			EffectHandlers[(int)AuraType.ModMechanicDurationPercent] = () => new ModMechanicDurationPercentHandler();
			EffectHandlers[(int)AuraType.NoPvPCredit] = () => new NoPvPCreditHandler();
			EffectHandlers[(int)AuraType.ModTalentPoints] = () => new ModPetTalentPointsHandler();
			EffectHandlers[(int)AuraType.ControlExoticPet] = () => new ControlExoticPetsHandler();
			EffectHandlers[(int)AuraType.ForceReaction] = () => new ForceReactionHandler();
			EffectHandlers[(int)AuraType.Vehicle] = () => new VehicleAuraHandler();
			EffectHandlers[(int)AuraType.Phase] = () => new PhaseAuraHandler();
			EffectHandlers[(int)AuraType.FeatherFall] = () => new FeatherFallHandler();
			EffectHandlers[(int)AuraType.Charm] = () => new CharmAuraHandler();
			EffectHandlers[(int)AuraType.ModTaunt] = () => new ModTauntAuraHandler();
			EffectHandlers[(int)AuraType.ModPacify] = () => new ModPacifyHandler();
			EffectHandlers[(int)AuraType.ModPacifySilence] = () => new ModPacifyHandler();
			EffectHandlers[(int)AuraType.ModSpellDamageByPercentOfSpirit] = () => new ModSpellDamageByPercentOfStatHandler();
			EffectHandlers[(int)AuraType.DamagePctAmplifier] = () => new DamagePctAmplifierHandler();
			EffectHandlers[(int)AuraType.ModArmorPenetration] = () => new ModArmorPenetrationHandler();
			EffectHandlers[(int)AuraType.PeriodicTriggerSpell2] = () => new PeriodicTriggerSpellHandler();
			EffectHandlers[(int)AuraType.ModMeleeCritDamageBonus] = () => new ModMeleeCritDamageBonusHandler();
			EffectHandlers[(int)AuraType.ModChanceTargetDodgesAttackPercent] = () => new ModChanceTargetDodgesAttackPercentHandler();
			EffectHandlers[(int)AuraType.ModOffhandDamagePercent] = () => new ModOffhandDamagePercentHandler();
			EffectHandlers[(int)AuraType.Expertise] = () => new ModExpertiseHandler();
			EffectHandlers[(int)AuraType.ModHitChance] = () => new ModHitChanceHandler();
			EffectHandlers[(int)AuraType.ModRageFromDamageDealtPercent] = () => new ModRageFromDamageDealtPercentHandler();
			EffectHandlers[(int)AuraType.CriticalBlockPct] = () => new CriticalBlockPctHandler();
			EffectHandlers[(int)AuraType.ModAPByArmor] = () => new ModAPByArmorHandler();
			

			// make sure, there are no missing handlers
			for (var i = 0; i < (int)AuraType.End; i++)
			{
				if (EffectHandlers[i] == null)
				{
					EffectHandlers[i] = () => new AuraVoidHandler();
				}
			}
		}
		#endregion

		#region EffectHandlers

		public static List<AuraEffectHandler> CreateEffectHandlers(Spell spell,
			CasterInfo caster,
			Unit target,
			bool beneficial)
		{
			return CreateEffectHandlers(spell.AuraEffects, caster, target, beneficial);
		}

		public static List<AuraEffectHandler> CreateEffectHandlers(SpellEffect[] effects, CasterInfo caster,
			Unit target, bool beneficial)
		{
			if (effects == null)
				return null;

			try
			{
				List<AuraEffectHandler> effectHandlers = null;
				var failReason = SpellFailedReason.Ok;

				for (var i = 0; i < effects.Length; i++)
				{
					var effect = effects[i];
					if (effect.HarmType == HarmType.Beneficial || !beneficial)
					{
						var effectValue = effect.CalcEffectValue(caster);

						var effectHandler = CreateEffectHandler(effect, caster, target, effectValue, ref failReason);
						if (failReason != SpellFailedReason.Ok)
						{
							return null;
						}

						if (effectHandlers == null)
						{
							effectHandlers = new List<AuraEffectHandler>(3);
						}
						effectHandlers.Add(effectHandler);
					}
				}
				return effectHandlers;
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "Failed to create AuraEffectHandlers for: " + effects.GetWhere(effect => effect != null).Spell);
				return null;
			}
		}

		public static AuraEffectHandler CreateEffectHandler(
			SpellEffect spellEffect, CasterInfo caster, Unit target,
			int effectValue, ref SpellFailedReason failedReason)
		{
			var handler = spellEffect.AuraEffectHandlerCreator();

			handler.m_spellEffect = spellEffect;
			handler.BaseEffectValue = effectValue;

			handler.CheckInitialize(caster, target, ref failedReason);
			return handler;
		}
		#endregion

		#region Unique Auras and Aura Groups
		internal static uint lastAuraUid;

		/// <summary>
		/// Used to make sure that certain auras can be applied multiple times
		/// </summary>
		internal static uint randomAuraId;

		internal static uint GetNextAuraUID()
		{
			if (lastAuraUid == 0)
			{
				lastAuraUid = (uint)SpellLineId.End + 10000;
			}
			return ++lastAuraUid;
		}

		/// <summary>
		/// Every Aura that is evaluated to true is only stackable with Auras that are also evaluated to true by the same evaluator.
		/// Spells that are not covered by any evaluator have no restrictions.
		/// </summary>
		public static readonly List<AuraIdEvaluator> AuraIdEvaluators = new List<AuraIdEvaluator>();

		internal static void RegisterAuraUIDEvaluators()
		{
			AddAuraGroupEvaluator(IsTransform);
			AddAuraGroupEvaluator(IsStealth);
		}

		/// <summary>
		/// All transform and visually supported shapeshift spells are in one group
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		static bool IsTransform(Spell spell)
		{
			return spell.IsShapeshift;
		}

		static bool IsStealth(Spell spell)
		{
			return spell.HasEffectWith(effect =>
									   effect.AuraType == AuraType.ModStealth);
		}

		public static void AddAuraGroupEvaluator(AuraIdEvaluator eval)
		{
			if (RealmServer.Instance.IsRunning && RealmServer.Instance.ClientCount > 0 )
			{
				throw new InvalidOperationException("Cannot set an Aura Group Evaluator at runtime because Aura Group IDs cannot be re-evaluated at this time. " +
					"Please register the evaluator during startup.");
			}
			AuraIdEvaluators.Add(eval);
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(IEnumerable<Spell> auras)
		{
			var uid = GetNextAuraUID();
			foreach (var spell in auras)
			{
				spell.AuraUID = uid;
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(params SpellId[] auras)
		{
			var uid = GetNextAuraUID();
			foreach (var id in auras)
			{
				var spell = SpellHandler.Get(id);
				if (spell == null)
				{
					throw new ArgumentException("Invalid SpellId: " + id);
				}
				spell.AuraUID = uid;
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(SpellId auraId, params SpellLineId[] auraLines)
		{
			var uid = GetNextAuraUID();
			var singleSpell = SpellHandler.Get(auraId);
			singleSpell.AuraUID = uid;
			foreach (var lineId in auraLines)
			{
				var line = SpellLines.GetLine(lineId);
				line.AuraUID = uid;
				foreach (var spell in line)
				{
					spell.AuraUID = uid;
				}
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(SpellId auraId, SpellId auraId2, params SpellLineId[] auraLines)
		{
			var uid = GetNextAuraUID();
			var singleSpell = SpellHandler.Get(auraId);
			singleSpell.AuraUID = uid;
			singleSpell = SpellHandler.Get(auraId2);
			singleSpell.AuraUID = uid;
			foreach (var lineId in auraLines)
			{
				var line = SpellLines.GetLine(lineId);
				line.AuraUID = uid;
				foreach (var spell in line)
				{
					spell.AuraUID = uid;
				}
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(SpellLineId auraLine, params SpellId[] auras)
		{
			var uid = GetNextAuraUID();
			var line = SpellLines.GetLine(auraLine);
			line.AuraUID = uid;
			foreach (var spell in line)
			{
				spell.AuraUID = uid;
			}
			foreach (var id in auras)
			{
				var spell = SpellHandler.Get(id);
				if (spell == null)
				{
					throw new ArgumentException("Invalid SpellId: " + id);
				}
				spell.AuraUID = uid;
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(SpellLineId auraLine, SpellLineId auraLine2, params SpellId[] auras)
		{
			var uid = GetNextAuraUID();
			var line = SpellLines.GetLine(auraLine);
			line.AuraUID = uid;
			foreach (var spell in line)
			{
				spell.AuraUID = uid;
			}
			line = SpellLines.GetLine(auraLine2);
			line.AuraUID = uid;
			foreach (var spell in line)
			{
				spell.AuraUID = uid;
			}
			foreach (var id in auras)
			{
				var spell = SpellHandler.Get(id);
				if (spell == null)
				{
					throw new ArgumentException("Invalid SpellId: " + id);
				}
				spell.AuraUID = uid;
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras that are mutually exclusive
		/// </summary>
		public static uint AddAuraGroup(params SpellLineId[] auraLines)
		{
			var uid = GetNextAuraUID();
			foreach (var lineId in auraLines)
			{
				var line = SpellLines.GetLine(lineId);
				line.AuraUID = uid;
				foreach (var spell in line)
				{
					spell.AuraUID = uid;
				}
			}
			return uid;
		}

		/// <summary>
		/// Defines a set of Auras of which one Unit can only have 1 per caster
		/// </summary>
		public static AuraCasterGroup AddAuraCasterGroup(params SpellLineId[] ids)
		{
			var group = new AuraCasterGroup { ids };
			foreach (var spell in group)
			{
				spell.AuraCasterGroup = group;
			}
			return group;
		}

		/// <summary>
		/// Defines a set of Auras of which one Unit can only have the given amount per caster
		/// </summary>
		public static AuraCasterGroup AddAuraCasterGroup(int maxPerCaster, params SpellLineId[] ids)
		{
			var group = new AuraCasterGroup(maxPerCaster) { ids };
			foreach (var spell in group)
			{
				spell.AuraCasterGroup = group;
			}
			return group;
		}
		#endregion
	}
}