/*************************************************************************
 *
 *   file		: SpellEffect.DBC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-15 13:15:29 +0800 (Tue, 15 Sep 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1100 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
	public partial class SpellEffect
	{
		/* Summary of $ values
		 * $sX = MinValue to MaxValue
		 * $oX = Amplitude
		 * $eX = ProcValue
		 * $bX = PointsPerComboPoint
		 * */
		#region DBC Fields

		/// <summary>
		/// SpellEffectNames.dbc - no longer included in mpqs
		/// </summary>
		public SpellEffectType EffectType;

		/// <summary>
		/// Random value max (BaseDice to DiceSides)
		/// </summary>
		public int DiceSides;

		public float RealPointsPerLevel;

		/// <summary>
		/// Base value
		/// Value = BasePoints + rand(BaseDice, DiceSides)
		/// </summary>
		public int BasePoints;

		public SpellMechanic Mechanic;

		public ImplicitSpellTargetType ImplicitTargetA;
		public ImplicitSpellTargetType ImplicitTargetB;

		/// <summary>
		/// SpellRadius.dbc
		/// Is always at least 5y. 
		/// If area-related spells dont have a radius we just look for very close targets
		/// </summary>
		public float Radius;

		/// <summary>
		/// SpellAuraNames.dbc - no longer included in mpqs
		/// </summary>
		public AuraType AuraType;

		/// <summary>
		/// Interval-delay in milliseconds
		/// </summary>
		public int Amplitude;

		/// <summary>
		/// Returns the max amount of ticks of this Effect
		/// </summary>
		public int GetMaxTicks()
		{
			if (Amplitude == 0) return 0;
			return Spell.Durations.Max/Amplitude;
		}

		/// <summary>
		/// $e1/2/3 in Description
		/// </summary>
		public float ProcValue;

		public int ChainTargets;

		/// <summary>
		/// 
		/// </summary>
		public uint ItemId;

		public int MiscValue;

		public int MiscValueB;

		/// <summary>
		/// Not set during InitializationPass 2, so 
		/// for fixing things, use GetTriggerSpell() instead.
		/// </summary>
		[NotPersistent]
		public Spell TriggerSpell;
		public SpellId TriggerSpellId;

		public Spell GetTriggerSpell()
		{
			var spell = SpellHandler.Get(TriggerSpellId);
			if (spell == null && ContentMgr.ForceDataPresence)
			{
				throw new ContentException("Spell {0} does not have a valid TriggerSpellId: {1}", this, TriggerSpellId);
			}
			return spell;
		}

		/// <summary>
		/// 
		/// </summary>
		public float PointsPerComboPoint;

		/// <summary>
		/// Multi purpose.
		/// 1. If it is a proc effect, determines set of spells that can proc this proc (use <see cref="AddToAffectMask"/>)
		/// 2. If it is a modifier effect, determines set of spells to be affected by this effect
		/// 3. Ignored in some cases
		/// 4. Special applications in some cases
		/// </summary>
		[Persistent(3)]
		public uint[] AffectMask = new uint[3];
		#endregion

		#region Variables
		/// <summary>
		/// Factor of the amount of AP to be added to the EffectValue (1.0f = +100%)
		/// </summary>
		public float APValueFactor;

		/// <summary>
		/// Amount of Spell Power to be added to the EffectValue in % (1 = +1%)
		/// </summary>
		public int SpellPowerValuePct;

		/// <summary>
		/// Factor of the amount of AP to be added to the EffectValue per combo point
		/// </summary>
		public float APPerComboPointValueFactor;

		/// <summary>
		/// Only use this effect if the caster is in the given form (if given)
		/// </summary>
		public ShapeshiftMask RequiredShapeshiftMask;

		/// <summary>
		/// If set, it will use the SpellEffect that triggered or proc'ed this SpellEffect (if any)
		/// instead of this one.
		/// </summary>
		public bool OverrideEffectValue;

		[NotPersistent]
		public SpellEffectHandlerCreator SpellEffectHandlerCreator;

		[NotPersistent]
		public AuraEffectHandlerCreator AuraEffectHandlerCreator;

		/// <summary>
		/// Explicitely defined spells that are somehow related to this effect.
		/// Is used for procs, talent-modifiers and AddTargetTrigger-relations mostly. 
		/// Can be used for other things.
		/// </summary>
		[NotPersistent]
		public HashSet<Spell> AffectSpellSet;

		/// <summary>
		/// Set of Auras that need to be active for this effect to activate
		/// </summary>
		public Spell[] RequiredActivationAuras;

		public bool IsDependentOnOtherAuras
		{
			get { return RequiredActivationAuras != null; }
		}

		/// <summary>
		/// If the caster has the spell of the EffectValueOverrideEffect it uses it for EffectValue calculation.
		/// If not it uses this Effect's original value.
		/// </summary>
		public SpellEffect EffectValueOverrideEffect;

		/// <summary>
		/// Used to determine the targets for this effect when casted by an AI caster
		/// </summary>
		public AISpellCastTargetType AISpellCastTargetType = AISpellCastTargetType.Default;
		#endregion

		#region Auto generated Fields
		/// <summary>
		/// The spell to which this effect belongs
		/// </summary>
		[NotPersistent]
		public Spell Spell;

		public int EffectIndex;

		[NotPersistent]
		public int ValueMin, ValueMax;

		[NotPersistent]
		public bool IsAuraEffect;

		/// <summary>
		/// Applies to targets in a specific area
		/// </summary>
		[NotPersistent]
		public bool IsAreaEffect;

		/// <summary>
		/// Whether this requires the caster to target the area
		/// </summary>
		[NotPersistent]
		public bool IsTargetAreaEffect;

		[NotPersistent]
		public bool HasSingleTarget;

		/// <summary>
		/// Applies to targets in a specific area
		/// </summary>
		[NotPersistent]
		public bool IsAreaAuraEffect;

		/// <summary>
		/// Summons something
		/// </summary>
		[NotPersistent]
		public bool IsSummon;

		/// <summary>
		/// Whether it happens multiple times (certain Auras or channeled effects)
		/// </summary>
		[NotPersistent]
		public bool IsPeriodic;

		/// <summary>
		/// Probably useless
		/// </summary>
		[NotPersistent]
		public bool _IsPeriodicAura;

		/// <summary>
		/// Whether this effect has actual Objects as targets
		/// </summary>
		[NotPersistent]
		public bool HasTargets;

		/// <summary>
		/// Whether this is a heal-effect
		/// </summary>
		[NotPersistent]
		public bool IsHealEffect;

		/// <summary>
		/// Whether this is a damage effect
		/// </summary>
		[NotPersistent]
		public bool IsDamageEffect;

		/// <summary>
		/// Whether this Effect is triggered by Procs
		/// </summary>
		[NotPersistent]
		public bool IsProc;

		/// <summary>
		/// Harmful, neutral or beneficial
		/// </summary>
		[NotPersistent]
		public HarmType HarmType;

		/// <summary>
		/// Whether this effect gives a flat bonus to your strike's damage
		/// </summary>
		[NotPersistent]
		public bool IsStrikeEffectFlat;

		/// <summary>
		/// Whether this effect gives a percent bonus to your strike's damage
		/// </summary>
		[NotPersistent]
		public bool IsStrikeEffectPct;

		/// <summary>
		/// Whether this is an effect that applies damage on strike
		/// </summary>
		public bool IsStrikeEffect
		{
			get { return IsStrikeEffectFlat || IsStrikeEffectPct; }
		}

		/// <summary>
		/// Wheter this Effect enchants an Item
		/// </summary>
		public bool IsEnchantmentEffect;

		/// <summary>
		/// All set bits of the MiscValue field. 
		/// This is useful for all SpellEffects whose MiscValue is a flag field.
		/// </summary>
		[NotPersistent]
		public uint[] MiscBitSet;

		/// <summary>
		/// Set to the actual (min) EffectValue
		/// </summary>
		[NotPersistent]
		public int MinValue;

		/// <summary>
		/// Whether this effect boosts other Spells
		/// </summary>
		[NotPersistent]
		public bool IsEnhancer;

		/// <summary>
		/// Whether this Effect summons a Totem
		/// </summary>
		[NotPersistent]
		public bool IsTotem;

		public bool HasAffectMask;

		public bool HasAffectingSpells
		{
			get { return HasAffectMask || AffectSpellSet != null; }
		}

		public bool IsModifierEffect;

		/// <summary>
		/// 
		/// </summary>
		public uint[] AffectMaskBitSet;

		/// <summary>
		/// Whether this spell effect (probably needs special handling)
		/// </summary>
		[NotPersistent]
		public bool IsScripted
		{
			get { return EffectType == SpellEffectType.Dummy || EffectType == SpellEffectType.ScriptEffect; }
		}
		#endregion

		#region IDataHolder Members

		//void IDataHolder.FinalizeAfterLoad()
		//{
		//    Initialize();
		//    Init2();
		//    if (Spell != null)
		//    {
		//        ArrayUtil.EnsureSize(ref Spell.Effects, EffectIndex);
		//        Spell.Effects[EffectIndex] = this;
		//        return;
		//    }
		//}

		#endregion
	}
}