using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{
	public enum AuraType
	{
		None = 0,
		BindSight = 1,
		ModPossess = 2,
		PeriodicDamage = 3,
		Dummy = 4,
		ModConfuse = 5,
		Charm = 6,
		Fear = 7,
		PeriodicHeal = 8,
		ModAttackSpeed = 9,

		/// <summary>
		/// Modifies the threat of attacks of a given school
		/// </summary>
		ModThreat = 10,
		ModTaunt = 11,
		ModStun = 12,
		ModDamageDone = 13,
		ModDamageTaken = 14,
		DamageShield = 15,
		ModStealth = 16,
		ModDetect = 17,
		ModInvisibility = 18,
		ModInvisibilityDetection = 19,
		/// <summary>
		/// Regen X% of hp per Y sec
		/// </summary>
		RegenPercentOfTotalHealth = 20,
		RegenPercentOfTotalMana = 21,
		ModResistance = 22,
		PeriodicTriggerSpell = 23,
		PeriodicEnergize = 24,
		ModPacify = 25,
		ModRoot = 26,
		ModSilence = 27,
		ReflectSpells = 28,
		ModStat = 29,
		ModSkill = 30,
		ModIncreaseSpeed = 31,
		ModIncreaseMountedSpeed = 32,
		ModDecreaseSpeed = 33,
		ModIncreaseHealth = 34,
		ModIncreaseEnergy = 35,
		ModShapeshift = 36,
		EffectImmunity = 37,
		StateImmunity = 38,
		SchoolImmunity = 39,
		DamageImmunity = 40,
		DispelImmunity = 41,
		ProcTriggerSpell = 42,
		ProcTriggerDamage = 43,
		TrackCreatures = 44,
		TrackResources = 45,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly ModParrySkill
		/// </summary>
		Aura_46 = 46,
		ModParryPercent = 47,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly ModDodgeSkill
		/// </summary>
		Aura_48 = 48,
		ModDodgePercent = 49,
		ModBlockSkill = 50,
		ModBlockPercent = 51,
		ModCritPercent = 52,
		PeriodicLeech = 53,
		ModHitChance = 54,
		ModSpellHitChance = 55,
		Transform = 56,
		ModSpellCritChance = 57,
		ModIncreaseSwimSpeed = 58,
		/// <summary>
		/// MiscValue: Mask of CreatureType.dbc
		/// </summary>
		ModDamageDoneToCreatureType = 59,
		ModPacifySilence = 60,
		ModScale = 61,
		PeriodicHealthFunnel = 62,
		Unused_PeriodicManaFunnel = 63,
		PeriodicManaLeech = 64,
		ModCastingSpeed = 65,
		FeignDeath = 66,
		ModDisarm = 67,
		ModStalked = 68,
		SchoolAbsorb = 69,
		ExtraAttacks = 70,
		ModSpellCritChanceForSchool = 71,
		ModPowerCost = 72,
		ModPowerCostForSchool = 73,
		/// <summary>
		/// MiscValue: Mask of Resistances.dbc
		/// </summary>
		ReflectSpellsFromSchool = 74,
		ModLanguage = 75,
		FarSight = 76,
		MechanicImmunity = 77,
		/// <summary>
		/// Sit on a ride
		/// </summary>
		Mounted = 78,
		ModDamageDonePercent = 79,//AuraModDamagePercentDone
		ModStatPercent = 80,
		SplitDamage = 81,
		WaterBreathing = 82,
		ModBaseResistance = 83,

		/// <summary>
		/// $sX * ($d/5sec) = total healed over $d
		/// </summary>
		ModHealthRegen = 84,
		ModPowerRegen = 85,
		/// <summary>
		/// If target dies from spell, add item ItemId to player
		/// </summary>
		CreateItemOnTargetDeath = 86,
		ModDamageTakenPercent = 87,
		ModHealthRegenPercent = 88,
		PeriodicDamagePercent = 89,
		Unused_ModResistChance = 90,
		ModDetectRange = 91,
		PreventFleeing = 92,
		/// <summary>
		/// Invul
		/// </summary>
		Unattackable = 93,
		InterruptRegen = 94,
		Ghost = 95,
		SpellMagnet = 96,
		ManaShield = 97,
		ModSkillTalent = 98,
		ModAttackPower = 99,
		/// <summary>
		/// Shows beneficial spells to all, like Detect Magic
		/// </summary>
		AurasVisible = 100,
		ModResistancePercent = 101,
		ModAttackPowerToCreatureType = 102,

		/// <summary>
		/// 
		/// </summary>
		ModTotalThreat = 103,
		WaterWalk = 104,
		FeatherFall = 105,
		Hover = 106,
		/// <summary>
		/// MiscValue: ref to <see cref="SpellModifierType"/> enum
		/// </summary>
		AddModifierFlat = 107,
		/// <summary>
		/// 
		/// </summary>
		AddModifierPercent = 108,
		AddTargetTrigger = 109,
		ModPowerRegenPercent = 110,
		AddCasterHitTrigger = 111,
		OverrideClassScripts = 112,
		ModRangedDamageTaken = 113,
		Unused_ModRangedDamageTakenPercent = 114,
		ModHealing = 115,
		ModRegenInCombat = 116,
		ModMechanicResistance = 117,
		ModHealingPercent = 118,
		Unused_SharePetTracking = 119,
		Untrackable = 120,
		Empathy = 121,
		ModOffhandDamagePercent = 122,
		ModTargetResistance = 123,
		ModRangedAttackPower = 124,
		ModMeleeDamageTaken = 125,
		ModMeleeDamageTakenPercent = 126,
		ModAttackerRangedAttackPowerBonus = 127,
		ModPossessPet = 128,
		ModIncreaseSpeedAlways = 129,
		ModMountedSpeedAlways = 130,
		ModRangedAttackPowerToCreatureType = 131,
		ModIncreaseEnergyPercent = 132,
		ModIncreaseHealthPercent = 133,
		ModManaRegenInterrupt = 134,
		ModHealingDone = 135,
		ModHealingTaken = 136,
		ModTotalStatPercent = 137,
		ModHaste = 138,
		ForceReaction = 139,
		ModRangedHaste = 140,
		ModRangedAmmoHaste = 141,
		ModBaseResistancePercent = 142,
		ModResistanceExclusive = 143,
		SafeFall = 144,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly Charisma
		/// Possibly now "Add 4 Talent points to all pets"
		/// </summary>
		ModTalentPoints = 145,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly Persuaded
		/// Possibly now "Can control Exotic pets"
		/// </summary>
		ControlExoticPet = 146,
		AddCreatureImmunity = 147,
		RetainComboPoints = 148,
		ModResistSpellInterruptionPercent = 149,                      //    Resist Pushback
		ModShieldBlockvaluePct = 150,
		TrackStealthed = 151,                      //    Track Stealthed
		/// <summary>
		/// 32926
		/// </summary>
		ModDetectedRange = 152,                    //    Mod Detected Range
		SplitDamageFlat = 153,                     //    Split Damage Flat
		ModStealthLevel = 154,                     //    Stealth Level Modifier
		ModWaterBreathing = 155,                   //    Mod Water Breathing
		ModReputationGain = 156,                   //    Mod Reputation Gain
		ModPetDamagePercent = 157,                      //    Mod Pet Damage
		ModShieldBlockvalue = 158,
		NoPvPCredit = 159,
		ModAoEAvoidancePercent = 160,
		ModHealthRegenInCombat = 161,
		/// <summary>
		/// Misc = powertype
		/// </summary>
		PowerBurn = 162,
		/// <summary>
		/// %
		/// </summary>
		ModMeleeCritDamageBonus = 163,
		Aura_164 = 164,
		ModAttackerMeleeAttackPowerBonus = 165,
		/// <summary>
		/// %
		/// </summary>
		ModAttackPowerPercent = 166,
		/// <summary>
		/// %
		/// </summary>
		ModRangedAttackPowerPercent = 167,
		/// <summary>
		/// Misc = mask of creaturetype
		/// </summary>
		ModDamageDoneVersusCreatureType = 168,
		/// <summary>
		/// Misc = mask of creaturetype
		/// </summary>
		ModCritPercentVersusCreatureType = 169,
		DetectAmore = 170,
		ModPartySpeed = 171,
		ModPartySpeedMounted = 172,
		Unused_AllowChampionSpells = 173,
		ModSpellDamageByPercentOfSpirit = 174,
		ModSpellHealingByPercentOfSpirit = 175,
		SpiritOfRedemption = 176,
		AoeCharm = 177,
		ModDebuffResistancePercent = 178,
		/// <summary>
		/// %
		/// </summary>
		ModAttackerSpellCritChance = 179,
		ModSpellDamageVsUndead = 180,
		/// <summary>
		/// Unused
		/// </summary>
		Unused_181 = 181,
		ModArmorByPercentOfIntellect = 182,
		ModCriticalHitThreatGenerationPercent = 183,
		/// <summary>
		/// %
		/// </summary>
		ModAttackerMeleeHitChance = 184,
		/// <summary>
		/// %
		/// </summary>
		ModAttackerRangedHitChance = 185,
		/// <summary>
		/// %
		/// </summary>
		ModAttackerSpellHitChance = 186,
		ModAttackerMeleeCritChance = 187,
		/// <summary>
		/// %
		/// </summary>
		ModAttackerRangedCritChance = 188,
		ModRating = 189,
		ModFactionReputationGainPercent = 190,
		UseNormalMovementSpeed = 191,
		ModMeleeHastePercent = 192,
		ModTimeBetweenAttacks = 193,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly Unused_ModSpellDamageOfIntellect
		/// </summary>
		Aura_194 = 194,
		/// <summary>
		/// No longer unused as of 3.0.2
		/// Function may have changed though
		/// formerly Unused_ModSpellHealingOfIntellect
		/// </summary>
		Aura_195 = 195,
		/// <summary>
		/// Misc = seconds to add
		/// </summary>
		ModAllCooldownDuration = 196,
		ModAttackerCritChancePercent = 197,
		Unused_ModAllWeaponSkills = 198,
		/// <summary>
		/// What is this?
		/// </summary>
		ModSpellHitChance2 = 199,
		ModXpPct = 200,
		Fly = 201,
		/// <summary>
		/// Misc = 2: Finishing moves
		/// Misc = 3: Devastate
		/// </summary>
		CannotBeDodged = 202,
		ModAttackerMeleeCritDamagePercent = 203,
		ModAttackerRangedCritDamagePercent = 204,
		Aura_205 = 205,
		ModSpeedMounted = 206,
		ModSpeedMountedFlight = 207,
		ModSpeedFlight = 208,
		Aura_209 = 209,
		Aura_210 = 210,
		/// <summary>
		/// Seems to be a copy of AuraModPartySpeedMounted
		/// </summary>
		Aura_211 = 211,
		ModRangedAttackPowerByPercentOfIntellect = 212,
		ModRageFromDamageDealtPercent = 213,
		Aura_214 = 214,
		/// <summary>
		/// 32727 Arena Preparation
		/// </summary>
		Aura_215 = 215,
		ModSpellHastePercent = 216,
		Unused_217 = 217,
		/// <summary>
		/// Time between ranged attacks
		/// </summary>
		ModTimeBetweenRangedAttacks = 218,
		ModManaRegen = 219,
		/// <summary>
		/// No longer unused as of 3.0.2, function may have changed
		/// formerly AuraModSpellHealingOfStrength
		/// </summary>
		ModSpecificCombatRating = 220,
		/// <summary>
		/// Ignores an enemy, forcing the caster to not attack it unless there is no other target nearby. When the effect wears off, the creature will attack the most threatening target.
		/// </summary>
		Ignored = 221,
		Aura_222 = 222,
		Aura_223 = 223,
		Unused_224 = 224,
		/// <summary>
		/// Heal hp on next hit (Prayer of Mending)
		/// </summary>
		ChainHeal = 225,
		/// <summary>
		/// Multi-purpose: Mana regen, damage absorption, also:
		/// RogueCombatPreyOnTheWeakRank5
		/// Your critical strike damage is increased by $s1% when the target has less health than you (as a percentage of total health).
		/// </summary>
		Aura_226 = 226,
		Aura_227 = 227,
		/// <summary>
		/// Stealth Detection:
		/// Shadow Sight (Id: 34709)
		/// </summary>
		Aura_228 = 228,
		/// <summary>
		/// MiscValue = mask of schools
		/// </summary>
		ModAOEDamagePercent = 229,
		ModMaxHealth = 230,
		Aura_231 = 231,
		ModSilenceDurationPercent = 232,
		/// <summary>
		/// makes all humanoids except self appear as npc MiscValue
		/// </summary>
		ModHumanoidDisplayId = 233,
		/// <summary>
		/// MiscValue = <see cref="SpellMechanic"/>
		/// </summary>
		ModMechanicDurationPercent = 234,
		ModDispelMechanicResistancePercent = 235,
		/// <summary>
		/// Marks Units as "in vehicle"
		/// </summary>
		Vehicle = 236,
		/// <summary>
		/// Increases your spell power by an amount equal to X% of your attack power.
		/// </summary>
		ModSpellPowerByAPPct = 237,
		/// <summary>
		/// Always in combo with Aura_237, unnecessary (adding this would add spell power twice)
		/// </summary>
		Aura_238 = 238,
		/// <summary>
		/// Noggenfogger Elixir (Id: 16595)
		/// </summary>
		Aura_239 = 239,
		/// <summary>
		/// Apparently adds boni to weapon skills, see http://www.wowwiki.com/Expertise
		/// </summary>
		Expertise = 240,
		/// <summary>
		/// Brewfest Racing Ram Aura [DND] (Id: 42146)
		/// </summary>
		Aura_241 = 241,
		/// <summary>
		/// Increases your spell damage and healing by % of your total Intellect.
		/// </summary>
		ModSpellDamageAndHealingByPercentOfIntellect = 242,
		/// <summary>
		/// HolidayRobot Faction Override (Id: 45056)
		/// </summary>
		Aura_243 = 243,
		/// <summary>
		/// MiscValue = LanguageId
		/// </summary>
		ComprehendLanguage = 244,
		ModMagicEffectDurationPercent = 245,
		Aura_246 = 246,
		Aura_247 = 247,
		ModChanceTargetDodgesAttackPercent = 248,
		Aura_249 = 249,
		Aura_250 = 250,
		Aura_251 = 251,
		Aura_252 = 252,
		Aura_253 = 253,
		Aura_254 = 254,
		Aura_255 = 255,
		Aura_256 = 256,
		Aura_257 = 257,
		Aura_258 = 258,
		Aura_259 = 259,
		Aura_260 = 260,
		Phase = 261, // Highest as of 2.4.3.8606
		Aura_262 = 262,
		Aura_263 = 263,
		Unused_264 = 264,
		Unused_265 = 265,
		Unused_266 = 266,
		Aura_267 = 267,
		Aura_268 = 268,
		Aura_269 = 269,
		Aura_270 = 270,
		/// <summary>
		/// Uses AffectMask for spells that are influenced
		/// </summary>
		DamagePctAmplifier = 271,
		Aura_272 = 272,
		Aura_273 = 273,
		Aura_274 = 274,
		Aura_275 = 275,
		Aura_276 = 276,
		Aura_277 = 277,
		Aura_278 = 278,
		Aura_279 = 279,
		ModArmorPenetration = 280,
		Aura_281 = 281,
		Aura_282 = 282,
		Aura_283 = 283, // Highest in 3.0.2.9056
		Aura_284 = 284,
		/// <summary>
		/// Increases your attack power by X for every Y armor value you have.
		/// </summary>
		ModAPByArmor = 285,
		Aura_286 = 286,
		/// <summary>
		/// "chance to deflect spells cast by targets in front of you"
		/// </summary>
		ModDeflectChance = 287,
		/// <summary>
		/// Only: Deterrence (Id: 67801) [Deterrence_3]
		/// </summary>
		Aura_288 = 288,
		/// <summary>
		/// Only: PaladinProtectionCombatExpertise (no apparent effect)
		/// </summary>
		Aura_290 = 290,
		/// <summary>
		/// Always with ModXpPct (no apparent effect)
		/// </summary>
		Aura_291 = 291,
		Aura_292 = 292,
		Aura_293 = 293,
        Aura_294 = 294,
        Aura_295 = 295,
        Aura_296 = 296,
        Aura_297 = 297,
        Aura_298 = 298,
        Aura_299 = 299,
        Aura_300 = 300,
        Aura_301 = 301,
        Aura_302 = 302,
        Aura_303 = 303,
        Aura_304 = 304,
        Aura_305 = 305,
        Aura_306 = 307,
		End = 500
	}
}
