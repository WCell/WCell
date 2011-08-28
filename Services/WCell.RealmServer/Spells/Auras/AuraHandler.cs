/*************************************************************************
 *
 *   file		: AuraHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-31 03:46:31 +0100 (s�, 31 jan 2010) $

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
using WCell.Constants.Spells;
using WCell.RealmServer.Auras.Effects;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Auras.Mod;
using WCell.RealmServer.Spells.Auras.Passive;
using WCell.Util;

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
			EffectHandlers[(int)AuraType.None] = () => new AuraVoidHandler();														// 0
			EffectHandlers[(int)AuraType.ModPossess] = () => new ModPossessAuraHandler();											// 2
			EffectHandlers[(int)AuraType.PeriodicDamage] = () => new PeriodicDamageHandler();										// 3  
			EffectHandlers[(int)AuraType.Dummy] = () => new DummyHandler();															// 4
			EffectHandlers[(int)AuraType.ModConfuse] = () => new ModConfuseHandler();												// 5
			EffectHandlers[(int)AuraType.Charm] = () => new CharmAuraHandler();														// 6
			EffectHandlers[(int)AuraType.Fear] = () => new FearHandler();															// 7
			EffectHandlers[(int)AuraType.PeriodicHeal] = () => new PeriodicHealHandler();											// 8
			EffectHandlers[(int)AuraType.ModAttackSpeed] = () => new ModAttackSpeedHandler();										// 9
			EffectHandlers[(int)AuraType.ModThreat] = () => new ModThreatHandler();													// 10
			EffectHandlers[(int)AuraType.ModTaunt] = () => new ModTauntAuraHandler();												// 11
			EffectHandlers[(int)AuraType.ModStun] = () => new StunHandler();														// 12
			EffectHandlers[(int)AuraType.ModDamageDone] = () => new ModDamageDoneHandler();											// 13
			EffectHandlers[(int)AuraType.ModDamageTaken] = () => new ModDamageTakenHandler();										// 14
			EffectHandlers[(int)AuraType.DamageShield] = () => new DamageShieldEffectHandler();										// 15
			EffectHandlers[(int)AuraType.ModStealth] = () => new ModStealthHandler();												// 16
			EffectHandlers[(int)AuraType.ModInvisibility] = () => new ModInvisibilityHandler();										// 18
			EffectHandlers[(int)AuraType.RegenPercentOfTotalHealth] = () => new RegenPercentOfTotalHealthHandler();					// 20
			EffectHandlers[(int)AuraType.RegenPercentOfTotalMana] = () => new RegenPercentOfTotalManaHandler();						// 21
			EffectHandlers[(int)AuraType.ModResistance] = () => new ModResistanceHandler();											// 22
			EffectHandlers[(int)AuraType.PeriodicTriggerSpell] = () => new PeriodicTriggerSpellHandler();							// 23		
			EffectHandlers[(int)AuraType.PeriodicEnergize] = () => new PeriodicEnergizeHandler();									// 24
			EffectHandlers[(int)AuraType.ModPacify] = () => new ModPacifyHandler();													// 25
			EffectHandlers[(int)AuraType.ModRoot] = () => new RootHandler();														// 26
			EffectHandlers[(int)AuraType.ModSilence] = () => new ModSilenceHandler();												// 27
			EffectHandlers[(int)AuraType.ModStat] = () => new ModStatHandler();														// 29
			EffectHandlers[(int)AuraType.ModIncreaseSpeed] = () => new ModIncreaseSpeedHandler();									// 31
			EffectHandlers[(int)AuraType.ModIncreaseMountedSpeed] = () => new ModIncreaseMountedSpeedHandler();						// 32
			EffectHandlers[(int)AuraType.ModDecreaseSpeed] = () => new ModDecreaseSpeedHandler();									// 33
			EffectHandlers[(int)AuraType.ModIncreaseHealth] = () => new ModIncreaseHealthHandler();									// 34
			EffectHandlers[(int)AuraType.ModIncreaseEnergy] = () => new ModIncreaseEnergyHandler();									// 35
			EffectHandlers[(int)AuraType.ModShapeshift] = () => new ShapeshiftHandler();											// 36
			EffectHandlers[(int)AuraType.SchoolImmunity] = () => new SchoolImmunityHandler();										// 39
			EffectHandlers[(int)AuraType.DamageImmunity] = () => new DamageImmunityHandler();										// 40
			EffectHandlers[(int)AuraType.DispelImmunity] = () => new DispelImmunityHandler();										// 41
			EffectHandlers[(int)AuraType.ProcTriggerSpell] = () => new ProcTriggerSpellHandler();									// 42
			EffectHandlers[(int)AuraType.ProcTriggerDamage] = () => new ProcTriggerDamageHandler();									// 43
			EffectHandlers[(int)AuraType.TrackCreatures] = () => new TrackCreaturesHandler();										// 44
			EffectHandlers[(int)AuraType.TrackResources] = () => new TrackResourcesHandler();										// 45
			EffectHandlers[(int)AuraType.ModParryPercent] = () => new ModParryPercentHandler();										// 47
			EffectHandlers[(int)AuraType.ModDodgePercent] = () => new ModDodgePercentHandler();										// 49
			EffectHandlers[(int)AuraType.ModCritHealValuePct] = () => new ModCritHealValuePctHandler();								// 50
			EffectHandlers[(int)AuraType.ModBlockPercent] = () => new ModBlockPercentHandler();										// 51
			EffectHandlers[(int)AuraType.ModCritPercent] = () => new ModCritPercentHandler();										// 52
			EffectHandlers[(int)AuraType.PeriodicLeech] = () => new PeriodicLeechHandler();											// 53
			EffectHandlers[(int)AuraType.ModHitChance] = () => new ModHitChanceHandler();											// 54
			EffectHandlers[(int)AuraType.ModSpellHitChance] = () => new ModSpellHitChanceHandler();									// 55
			EffectHandlers[(int)AuraType.Transform] = () => new TransformHandler();													// 56
			EffectHandlers[(int)AuraType.ModSpellCritChance] = () => new ModSpellCritChanceHandler();								// 57
			EffectHandlers[(int)AuraType.ModIncreaseSwimSpeed] = () => new ModIncreaseSwimSpeedHandler();							// 58
			EffectHandlers[(int)AuraType.ModPacifySilence] = () => new ModPacifySilenceHandler();									// 60
			EffectHandlers[(int)AuraType.ModScale] = () => new ModScaleHandler();													// 61
			EffectHandlers[(int)AuraType.PeriodicHealthFunnel] = () => new PeriodicHealthFunnelHandler();							// 62
			EffectHandlers[(int)AuraType.PeriodicManaLeech] = () => new PeriodicManaLeechHandler();									// 64
			EffectHandlers[(int)AuraType.ModCastingSpeed] = () => new ModCastingSpeedHandler();										// 65
			EffectHandlers[(int)AuraType.DisarmMainHand] = () => new DisarmMainHandHandler();										// 67
			EffectHandlers[(int)AuraType.SchoolAbsorb] = () => new SchoolAbsorbHandler();											// 69
			EffectHandlers[(int)AuraType.ModSpellCritChanceForSchool] = () => new ModSpellCritChanceForSchoolHandler();				// 71
			EffectHandlers[(int)AuraType.ModPowerCost] = () => new ModPowerCostHandler();											// 72
			EffectHandlers[(int)AuraType.ModPowerCostForSchool] = () => new ModPowerCostForSchoolHandler();							// 73
			EffectHandlers[(int)AuraType.ModLanguage] = () => new ModLanguageHandler();												// 75
			EffectHandlers[(int)AuraType.MechanicImmunity] = () => new MechanicImmunityHandler();									// 77
			EffectHandlers[(int)AuraType.Mounted] = () => new MountedHandler();														// 78
			EffectHandlers[(int)AuraType.ModDamageDonePercent] = () => new ModDamageDonePercentHandler();							// 79
			EffectHandlers[(int)AuraType.ModStatPercent] = () => new ModStatPercentHandler();										// 80
			EffectHandlers[(int)AuraType.SplitDamage] = () => new SplitDamageHandler();												// 81
			EffectHandlers[(int)AuraType.ModPowerRegen] = () => new ModPowerRegenHandler();											// 85
			EffectHandlers[(int)AuraType.CreateItemOnTargetDeath] = () => new CreateItemOnTargetDeathHandler();						// 86
			EffectHandlers[(int)AuraType.ModDamageTakenPercent] = () => new ModDamageTakenPercentHandler();							// 87
			EffectHandlers[(int)AuraType.PeriodicDamagePercent] = () => new PeriodicDamagePercentHandler();							// 89
			EffectHandlers[(int)AuraType.ModDetectRange] = () => new ModDetectRangeHandler();										// 91
			EffectHandlers[(int)AuraType.Unattackable] = () => new UnattackableHandler();											// 93
			EffectHandlers[(int)AuraType.InterruptRegen] = () => new InterruptRegenHandler();										// 94
			EffectHandlers[(int)AuraType.Ghost] = () => new GhostHandler();															// 95
			EffectHandlers[(int)AuraType.ManaShield] = () => new ManaShieldHandler();												// 97
			EffectHandlers[(int)AuraType.ModSkillTalent] = () => new ModSkillTalentHandler();										// 98
			EffectHandlers[(int)AuraType.ModMeleeAttackPower] = () => new ModMeleeAttackPowerHandler();								// 99
			EffectHandlers[(int)AuraType.ModResistancePercent] = () => new ModResistancePctHandler();								// 101
			EffectHandlers[(int)AuraType.WaterWalk] = () => new WaterWalkHandler();													// 104
			EffectHandlers[(int)AuraType.FeatherFall] = () => new FeatherFallHandler();												// 105
			EffectHandlers[(int)AuraType.Hover] = () => new HoverHandler();															// 106
			EffectHandlers[(int)AuraType.AddModifierFlat] = () => new AddModifierFlatHandler();										// 107
			EffectHandlers[(int)AuraType.AddModifierPercent] = () => new AddModifierPercentHandler();								// 108
			EffectHandlers[(int)AuraType.AddTargetTrigger] = () => new AddTargetTriggerHandler();									// 109
			EffectHandlers[(int)AuraType.ModPowerRegenPercent] = () => new ModPowerRegenPercentHandler();							// 110
			EffectHandlers[(int)AuraType.AddCasterHitTrigger] = () => new AddCasterHitTriggerHandler();								// 111
			EffectHandlers[(int)AuraType.ModMechanicResistance] = () => new ModMechanicResistanceHandler();							// 117
			EffectHandlers[(int)AuraType.ModHealingTakenPercent] = () => new ModHealingTakenPctHandler();							// 118
			EffectHandlers[(int)AuraType.Untrackable] = () => new UntrackableHandler();												// 120
			EffectHandlers[(int)AuraType.ModOffhandDamagePercent] = () => new ModOffhandDamagePercentHandler();						// 122
			EffectHandlers[(int)AuraType.ModTargetResistance] = () => new ModTargetResistanceHandler();								// 123
			EffectHandlers[(int)AuraType.ModRangedAttackPower] = () => new ModRangedAttackPowerHandler();							// 124
			EffectHandlers[(int)AuraType.ModIncreaseSpeedAlways] = () => new ModIncreaseSpeedAlwaysHandler();						// 129
			EffectHandlers[(int)AuraType.ModMountedSpeedAlways] = () => new ModMountedSpeedAlwaysHandler();							// 130
			EffectHandlers[(int)AuraType.ModIncreaseEnergyPercent] = () => new ModIncreaseEnergyPercentHandler();					// 132
			EffectHandlers[(int)AuraType.ModIncreaseHealthPercent] = () => new ModIncreaseHealthPercentHandler();					// 133
			EffectHandlers[(int)AuraType.ModManaRegenInterrupt] = () => new ModManaRegenInterruptHandler();							// 134
			EffectHandlers[(int)AuraType.ModHealingDone] = () => new ModHealingDoneHandler();										// 135
			EffectHandlers[(int)AuraType.ModHealingDonePct] = () => new ModHealingDonePctHandler();									// 136
			EffectHandlers[(int)AuraType.ModTotalStatPercent] = () => new ModTotalStatPercentHandler();								// 137
			EffectHandlers[(int)AuraType.ModHaste] = () => new ModHasteHandler();													// 138
			EffectHandlers[(int)AuraType.ForceReaction] = () => new ForceReactionHandler();											// 139
			EffectHandlers[(int)AuraType.ModBaseResistancePercent] = () => new ModBaseResistancePercentHandler();					// 142
			EffectHandlers[(int)AuraType.ModResistanceExclusive] = () => new ModResistanceExclusiveHandler();						// 143
			EffectHandlers[(int)AuraType.SafeFall] = () => new SafeFallHandler();													// 144
			EffectHandlers[(int)AuraType.ModTalentPoints] = () => new ModPetTalentPointsHandler();									// 145
			EffectHandlers[(int)AuraType.ControlExoticPet] = () => new ControlExoticPetsHandler();									// 146
			EffectHandlers[(int)AuraType.RetainComboPoints] = () => new RetainComboPointsHandler();									// 148
			EffectHandlers[(int)AuraType.ModResistSpellInterruptionPercent] = () => new ModResistSpellInterruptionPercentHandler();	// 149
			EffectHandlers[(int)AuraType.ModShieldBlockValuePct] = () => new ModShieldBlockValuePercentHandler();					// 150
			EffectHandlers[(int)AuraType.ModReputationGain] = () => new ModReputationGainHandler();									// 156
			EffectHandlers[(int)AuraType.NoPvPCredit] = () => new NoPvPCreditHandler();												// 159
			EffectHandlers[(int)AuraType.ModHealthRegenInCombat] = () => new ModHealthRegenInCombatHandler();						// 161
			EffectHandlers[(int)AuraType.PowerBurn] = () => new PowerBurnHandler();													// 162
			EffectHandlers[(int)AuraType.ModMeleeCritDamageBonus] = () => new ModMeleeCritDamageBonusHandler();						// 163
			EffectHandlers[(int)AuraType.ModAttackPowerPercent] = () => new ModMeleeAttackPowerPercentHandler();					// 166
			EffectHandlers[(int)AuraType.ModRangedAttackPowerPercent] = () => new ModRangedAttackPowerPercentHandler();				// 167
			EffectHandlers[(int)AuraType.ModDamageDoneVersusCreatureType] = () => new ModDamageDoneVersusCreatureTypeHandler();		// 168
			EffectHandlers[(int)AuraType.ModSpellDamageByPercentOfStat] = () => new ModSpellDamageByPercentOfStatHandler();			// 174
			EffectHandlers[(int)AuraType.ModSpellHealingByPercentOfStat] = () => new ModHealingByPercentOfStatHandler();			// 175
			EffectHandlers[(int)AuraType.ModDebuffResistancePercent] = () => new ModDebuffResistancePercentHandler();				// 178
			EffectHandlers[(int)AuraType.ModAttackerSpellCritChance] = () => new ModAttackerSpellCritChanceHandler();				// 179
			EffectHandlers[(int)AuraType.ModArmorByPercentOfIntellect] = () => new ModArmorByPercentOfIntellectHandler();			// 182
			EffectHandlers[(int)AuraType.ModAttackerMeleeHitChance] = () => new ModAttackerMeleeHitChanceHandler();					// 184
			EffectHandlers[(int)AuraType.ModAttackerRangedHitChance] = () => new ModAttackerRangedHitChanceHandler();				// 185
			EffectHandlers[(int)AuraType.ModAttackerSpellHitChance] = () => new ModAttackerSpellHitChanceHandler();					// 186
			EffectHandlers[(int)AuraType.ModRating] = () => new ModRatingHandler();													// 189
			EffectHandlers[(int)AuraType.ModMeleeHastePercent] = () => new ModMeleeHastePercentHandler();							// 192
			EffectHandlers[(int)AuraType.ModHastePercent] = () => new ModHastePercentHandler();										// 193
			EffectHandlers[(int)AuraType.ModAllCooldownDuration] = () => new ModAllCooldownDurationHandler();						// 196
			EffectHandlers[(int)AuraType.ModAttackerCritChancePercent] = () => new ModAttackerCritChancePercentHandler();			// 197
			EffectHandlers[(int)AuraType.ModSpellHitChance2] = () => new ModSpellHitChanceHandler();								// 199
			EffectHandlers[(int)AuraType.ModKillXpPct] = () => new ModKillXpPctHandler();											// 200
			EffectHandlers[(int)AuraType.Fly] = () => new FlyHandler();																// 201
			EffectHandlers[(int)AuraType.ModSpeedMountedFlight] = () => new ModSpeedMountedFlightHandler();							// 207
			EffectHandlers[(int)AuraType.ModRangedAttackPowerByPercentOfStat] =														// 212
				() => new ModRangedAttackPowerByPercentOfStatHandler();
			EffectHandlers[(int)AuraType.ModRageFromDamageDealtPercent] = () => new ModRageFromDamageDealtPercentHandler();			// 213
			EffectHandlers[(int)AuraType.ArenaPreparation] = () => new ArenaPreparationHandler();									// 215
			EffectHandlers[(int)AuraType.ModSpellHastePercent] = () => new ModSpellHastePercentHandler();							// 216
			EffectHandlers[(int)AuraType.ModManaRegen] = () => new ModManaRegenHandler();											// 219
			EffectHandlers[(int)AuraType.ModSpecificCombatRating] = () => new ModCombatRatingStat();								// 220
			EffectHandlers[(int)AuraType.PeriodicTriggerSpell2] = () => new PeriodicTriggerSpellHandler();							// 227
			EffectHandlers[(int)AuraType.ModAOEDamagePercent] = () => new ModAOEDamagePercentHandler();								// 229
			EffectHandlers[(int)AuraType.ModMaxHealth] = () => new ModMaxHealthHandler();											// 230
			EffectHandlers[(int)AuraType.ProcTriggerSpellWithOverride] = 
				() => new ProcTriggerSpellHandler();								//TODO: Might need some tweaks					// 231 
			EffectHandlers[(int)AuraType.ModSilenceDurationPercent] = () => new ModSilenceDurationPercentHandler();					// 232
			EffectHandlers[(int)AuraType.ModMechanicDurationPercent] = () => new ModMechanicDurationPercentHandler();				// 234
			EffectHandlers[(int)AuraType.Vehicle] = () => new VehicleAuraHandler();													// 236
			EffectHandlers[(int)AuraType.ModSpellPowerByAPPct] = () => new ModSpellPowerByAPPctHandler();							// 237
			EffectHandlers[(int)AuraType.ModScale2] = () => new ModScaleHandler();													// 239
			EffectHandlers[(int)AuraType.Expertise] = () => new ModExpertiseHandler();												// 240
			EffectHandlers[(int)AuraType.ForceAutoRunForward] = () => new ForceAutoRunForwardHandler();								// 241
			EffectHandlers[(int)AuraType.MirrorImage] = () => new MirrorImageHandler();												// 247
			EffectHandlers[(int)AuraType.ModChanceTargetDodgesAttackPercent] = 
				() => new ModChanceTargetDodgesAttackPercentHandler();																// 248
			EffectHandlers[(int)AuraType.CriticalBlockPct] = () => new CriticalBlockPctHandler();									// 253
			EffectHandlers[(int)AuraType.DisarmOffhandAndShield] = () => new DisarmOffHandHandler();								// 254
			EffectHandlers[(int)AuraType.IncreaseBleedEffectPct] = () => new AuraVoidHandler();										// 255
			EffectHandlers[(int)AuraType.Phase] = () => new PhaseAuraHandler();														// 261
			EffectHandlers[(int)AuraType.ModMeleeAttackPowerByPercentOfStat] =														// 268
				() => new ModMeleeAttackPowerByPercentOfStatHandler();
			EffectHandlers[(int)AuraType.DamagePctAmplifier] = () => new DamagePctAmplifierHandler();								// 271
			EffectHandlers[(int)AuraType.DisarmRanged] = () => new DisarmRangedHandler();											// 278
			EffectHandlers[(int)AuraType.ModArmorPenetration] = () => new ModArmorPenetrationHandler();								// 280
			EffectHandlers[(int)AuraType.ToggleAura] = () => new ToggleAuraHandler();												// 284
			EffectHandlers[(int)AuraType.ModAPByArmor] = () => new ModAPByArmorHandler();											// 285
			EffectHandlers[(int)AuraType.EnableCritical] = () => new EnableCriticalHandler();										// 286
			EffectHandlers[(int)AuraType.ModQuestXpPct] = () => new ModQuestXpPctHandler();											// 291
			EffectHandlers[(int)AuraType.CallStabledPet] = () => new CallStabledPetHandler();										// 292

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
			AddAuraGroupEvaluator(IsTracker);
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

		static bool IsTracker(Spell spell)
		{
			return spell.HasEffect(AuraType.TrackCreatures) || spell.HasEffect(AuraType.TrackResources) ||
				   spell.HasEffect(AuraType.TrackStealthed);
		}

		public static void AddAuraGroupEvaluator(AuraIdEvaluator eval)
		{
			if (RealmServer.Instance.IsRunning && RealmServer.Instance.ClientCount > 0)
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
			var line = auraLine.GetLine();
			line.AuraUID = uid;
			foreach (var spell in line)
			{
				spell.AuraUID = uid;
			}
			line = auraLine2.GetLine();
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