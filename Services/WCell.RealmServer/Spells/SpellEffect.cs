/*************************************************************************
 *
 *   file		: SpellEffect.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-16 10:32:12 +0200 (fr, 16 apr 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1278 $
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
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells
{
    public partial class SpellEffect
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static readonly ImplicitSpellTargetType[] NoTargetTypes = new[] {
			ImplicitSpellTargetType.None,
			ImplicitSpellTargetType.CaliriEggs,
			ImplicitSpellTargetType.DynamicObject,
			ImplicitSpellTargetType.GameObject,
			ImplicitSpellTargetType.GameObjectOrItem,
			ImplicitSpellTargetType.HeartstoneLocation,
			ImplicitSpellTargetType.ScriptedGameObject,
			ImplicitSpellTargetType.ScriptedLocation,
			ImplicitSpellTargetType.ScriptedObjectLocation
		};

		public static HashSet<AuraType> ProcAuraTypes = new HashSet<AuraType> {
		                                                		AuraType.ProcTriggerSpell,
		                                                		AuraType.ProcTriggerDamage,
		                                                		AuraType.ProcTriggerSpellWithOverride
		                                                	};

		/// <summary>
		/// Only valid for SpellEffects of type Summon
		/// </summary>
		public SpellSummonEntry SummonEntry
		{
			get
			{
				if (EffectType != SpellEffectType.Summon || (SummonType)MiscValueB == SummonType.None)
				{
					return null;
				}
				return SpellHandler.GetSummonEntry((SummonType)MiscValueB);
			}
		}

		/// <summary>
		/// All specific SpellLines that are affected by this SpellEffect
		/// </summary>
		public IEnumerable<SpellLine> AffectedLines
		{
			get
			{
                if (Spell.SpellClassOptions.ClassId != 0)
				{
                    return SpellHandler.GetAffectedSpellLines(Spell.SpellClassOptions.ClassId, AffectMask);
				}
				return new SpellLine[0];
			}
		}

		public SpellEffect() { }

		public SpellEffect(Spell spell, int index)
		{
			Spell = spell;
			EffectIndex = index;
		}

		#region Init & Auto Generation of fields
		internal void Init2()
		{
			// see http://www.wowhead.com/spell=25269 for comparison
			ValueMin = BasePoints + 1;
			ValueMax = BasePoints + DiceSides; // TODO: check this!

			IsTargetAreaEffect = TargetAreaEffects.Contains(ImplicitTargetA) || TargetAreaEffects.Contains(ImplicitTargetB);

			// prevent aoe and non-aoe effects from mixing together
			if (AreaEffects.Contains(ImplicitTargetA))
			{
				IsAreaEffect = true;
				if (ImplicitTargetB != ImplicitSpellTargetType.None && AreaEffects.Contains(ImplicitTargetB))
				{
					ImplicitTargetB = ImplicitSpellTargetType.None;
				}
			}
			else if (ImplicitTargetB != ImplicitSpellTargetType.None && AreaEffects.Contains(ImplicitTargetB))
			{
				IsAreaEffect = true;
				ImplicitTargetA = ImplicitSpellTargetType.None;
			}

			if (IsPeriodic = AuraPeriod > 0)
			{
				_IsPeriodicAura = (AuraType == AuraType.PeriodicDamage ||
								   AuraType == AuraType.PeriodicDamagePercent ||
								   AuraType == AuraType.PeriodicEnergize ||
								   AuraType == AuraType.PeriodicHeal ||
								   AuraType == AuraType.PeriodicHealthFunnel ||
								   AuraType == AuraType.PeriodicLeech ||
								   AuraType == AuraType.PeriodicManaLeech ||
								   AuraType == AuraType.PeriodicTriggerSpell);
			}

			if (Spell.IsPassive)
			{
				// proc effect etc
				HarmType = HarmType.Beneficial;
			}
			else if ((HasTarget(ImplicitSpellTargetType.AllEnemiesAroundCaster,
								ImplicitSpellTargetType.AllEnemiesInArea,
								ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
								ImplicitSpellTargetType.AllEnemiesInAreaInstant,
								ImplicitSpellTargetType.CurrentSelection) ||
					  HasTarget(ImplicitSpellTargetType.InFrontOfCaster,
								ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius,
								ImplicitSpellTargetType.LocationInFrontCaster,
								ImplicitSpellTargetType.NetherDrakeSummonLocation,
								ImplicitSpellTargetType.SelectedEnemyChanneled,
								ImplicitSpellTargetType.SelectedEnemyDeadlyPoison,
								ImplicitSpellTargetType.SingleEnemy,
								ImplicitSpellTargetType.SpreadableDesease,
								ImplicitSpellTargetType.TargetAtOrientationOfCaster)) &&
					 (!HasTarget(
						ImplicitSpellTargetType.Self,
						ImplicitSpellTargetType.AllFriendlyInAura,
						ImplicitSpellTargetType.AllParty,
						ImplicitSpellTargetType.AllPartyAroundCaster,
						ImplicitSpellTargetType.AllPartyInArea,
						ImplicitSpellTargetType.PartyAroundCaster,
						ImplicitSpellTargetType.AllPartyInAreaChanneled) ||
					  Spell.SpellCategories.Mechanic.IsNegative()))
			{
				HarmType = HarmType.Harmful;
			}
			else if (!HasTarget(ImplicitSpellTargetType.Duel) &&
					 (ImplicitTargetA != ImplicitSpellTargetType.None || ImplicitTargetB != ImplicitSpellTargetType.None))
			{
				HarmType = HarmType.Beneficial;
			}

			// do some correction for ModManaRegen
			if (AuraType == AuraType.ModManaRegen && AuraPeriod == 0)
			{
				// 5000 ms if not specified otherwise
                AuraPeriod = ModManaRegenHandler.DefaultAuraPeriod;
			}

			if (HasTarget(ImplicitSpellTargetType.AllFriendlyInAura))
			{
				// whenever it's used, its used together with AllEnemiesAroundCaster in a beneficial spell)
				ImplicitTargetA = ImplicitSpellTargetType.AllFriendlyInAura;
				ImplicitTargetB = ImplicitSpellTargetType.None;
			}

			HasTargets = !NoTargetTypes.Contains(ImplicitTargetA) || !NoTargetTypes.Contains(ImplicitTargetB);

			HasSingleTarget = HasTargets && !IsAreaEffect;

			IsAreaAuraEffect = (EffectType == SpellEffectType.PersistantAreaAura ||
								EffectType == SpellEffectType.ApplyAreaAura ||
								EffectType == SpellEffectType.ApplyGroupAura);

			if (EffectType == SpellEffectType.ApplyGroupAura)
			{
				if (Radius > 0)
				{
					ImplicitTargetA = ImplicitSpellTargetType.AllPartyInArea;
				}
				else
				{
					ImplicitTargetA = ImplicitSpellTargetType.AllParty;
				}
			}

			IsAuraEffect = IsAreaAuraEffect ||
						   EffectType == SpellEffectType.ApplyAura ||
						   EffectType == SpellEffectType.ApplyAuraToMaster ||
						   EffectType == SpellEffectType.ApplyPetAura ||
						   EffectType == SpellEffectType.ApplyStatAura ||
						   EffectType == SpellEffectType.ApplyStatAuraPercent;

			IsEnhancer = IsAuraEffect && (AuraType == AuraType.AddModifierFlat || AuraType == AuraType.AddModifierPercent);

			if (MiscValueType == typeof(DamageSchoolMask))
			{
				// make sure that only valid schools are used
				MiscValue = MiscValue & (int)DamageSchoolMask.AllSchools;
			}

			MiscBitSet = MiscValue > 0 ? Utility.GetSetIndices((uint)MiscValue) : new uint[0];

			MinValue = BasePoints; // + DiceCount; TODO: check this!

			IsStrikeEffectFlat = EffectType == SpellEffectType.WeaponDamage ||
								 EffectType == SpellEffectType.WeaponDamageNoSchool ||
								 EffectType == SpellEffectType.NormalizedWeaponDamagePlus;

			IsStrikeEffectPct = EffectType == SpellEffectType.WeaponPercentDamage;

			IsTotem = HasTarget(ImplicitSpellTargetType.TotemAir) ||
					  HasTarget(ImplicitSpellTargetType.TotemEarth) ||
					  HasTarget(ImplicitSpellTargetType.TotemFire) ||
					  HasTarget(ImplicitSpellTargetType.TotemWater);

			IsProc = IsProc || ProcAuraTypes.Contains(AuraType);

			OverrideEffectValue = OverrideEffectValue ||
				AuraType == AuraType.ProcTriggerSpellWithOverride;

			IsHealEffect = EffectType == SpellEffectType.Heal ||
						   EffectType == SpellEffectType.HealMaxHealth ||
						   AuraType == AuraType.PeriodicHeal ||
						   (TriggerSpell != null && TriggerSpell.IsHealSpell);

			IsDamageEffect = EffectType == SpellEffectType.SchoolDamage || IsStrikeEffect;

			IsModifierEffect = AuraType == AuraType.AddModifierFlat || AuraType == AuraType.AddModifierPercent;

			HasAffectMask = AffectMask.Any(mask => mask != 0);

			if (HasAffectMask)
			{
				AffectMaskBitSet = Utility.GetSetIndices(AffectMask);
			}

			if (SpellEffectHandlerCreator == null)
			{
				SpellEffectHandlerCreator = SpellHandler.SpellEffectCreators[(int)EffectType];
			}
			if (IsAuraEffect && AuraEffectHandlerCreator == null)
			{
				AuraEffectHandlerCreator = AuraHandler.EffectHandlers[(int)AuraType];
				if (AuraEffectHandlerCreator == null)
				{
					AuraEffectHandlerCreator = AuraHandler.EffectHandlers[0];
				}
			}

			RepairBrokenTargetPairs();

			IsEnchantmentEffect = EffectType == SpellEffectType.EnchantHeldItem ||
				EffectType == SpellEffectType.EnchantItem ||
				EffectType == SpellEffectType.EnchantItemTemporary;

			AISpellUtil.DecideDefaultTargetHandlerDefintion(this);
		}

		/// <summary>
		/// More or less frequently occuring target pairs that make no sense
		/// </summary>
		private void RepairBrokenTargetPairs()
		{
			// Used on some beam visuals
			if (ImplicitTargetA == ImplicitSpellTargetType.Self && ImplicitTargetB == ImplicitSpellTargetType.Duel)
			{
				// Duel and Self doesn't make sense -> Remove Self
				ImplicitTargetA = ImplicitSpellTargetType.None;
			}
		}

		#endregion

		/// <summary>
		/// Whether this effect can share targets with the given effect
		/// </summary>
		public bool SharesTargetsWith(SpellEffect b, bool aiCast)
		{
			// if a TargetDefinition is set, it overrides the default implicit targets
			var targetDef = GetTargetDefinition(aiCast);
			return (targetDef != null && targetDef.Equals(b.GetTargetDefinition(aiCast))) ||
				(ImplicitTargetA == b.ImplicitTargetA && ImplicitTargetB == b.ImplicitTargetB);
		}

		public bool HasTarget(ImplicitSpellTargetType target)
		{
			return ImplicitTargetA == target || ImplicitTargetB == target;
		}

		public bool HasTarget(params ImplicitSpellTargetType[] targets)
		{
			return targets.FirstOrDefault(HasTarget) != 0;
		}

		public void CopyValuesTo(SpellEffect effect)
		{
			effect.BasePoints = BasePoints;
			effect.DiceSides = DiceSides;
		}

		#region Auras
		/// <summary>
		/// Adds a set of Auras of which at least one need to be active for this SpellEffect to activate
		/// </summary>
		public void AddRequiredActivationAuras(params SpellLineId[] lines)
		{
			foreach (var id in lines)
			{
				AddRequiredActivationAuras(id.GetLine().ToArray());
			}
		}

		public void AddRequiredActivationAuras(params SpellId[] ids)
		{
			var spells = new Spell[ids.Length];
			for (var i = 0; i < ids.Length; i++)
			{
				var spellId = ids[i];
				var spell = SpellHandler.Get(spellId);
				if (spell == null)
				{
					throw new ArgumentException("Invalid spell in AddRequiredActivationAuras: " + spellId);
				}
				spells[i] = spell;
			}
			AddRequiredActivationAuras(spells);
		}

		public void AddRequiredActivationAuras(params Spell[] spells)
		{
			if (RequiredActivationAuras == null)
			{
				RequiredActivationAuras = spells;
			}
			else
			{
				ArrayUtil.Concat(ref RequiredActivationAuras, spells);
			}
		}

		public AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster,
															  Unit target, ref SpellFailedReason failedReason)
		{
			return CreateAuraEffectHandler(caster, target, ref failedReason, null);
		}

		internal AuraEffectHandler CreateAuraEffectHandler(ObjectReference caster,
			Unit target, ref SpellFailedReason failedReason, SpellCast triggeringCast)
		{
			var handler = AuraEffectHandlerCreator();

			if (triggeringCast != null &&
				triggeringCast.TriggerEffect != null &&
				triggeringCast.TriggerEffect.OverrideEffectValue)
			{
				if (Spell.Effects.Length > 1)
				{
					// it does not make sense to override multiple effects with a single effect...
					log.Warn("Spell {0} had overriding SpellEffect although the spell that was triggered had {2} (> 1) effects",
						Spell, Spell.Effects.Length);
				}
				handler.m_spellEffect = triggeringCast.TriggerEffect;
			}
			else
			{
				handler.m_spellEffect = this;
			}

			handler.BaseEffectValue = CalcEffectValue(caster);
			handler.CheckInitialize(triggeringCast, caster, target, ref failedReason);
			return handler;
		}
		#endregion

		#region Formulars
		public int CalcEffectValue(ObjectReference casterReference)
		{
			var caster = casterReference.UnitMaster;
			if (caster != null)
			{
				return CalcEffectValue(caster);
			}
			else
			{
				return CalcEffectValue(casterReference.Level, 0, false);
			}
		}

		public int CalcEffectValue(Unit caster)
		{
			int value;
			if (caster != null)
			{
				value = CalcEffectValue(caster.Level, caster.ComboPoints, true);
			}
			else
			{
				value = CalcEffectValue(1, 0, false);
			}
			return CalcEffectValue(caster, value);
		}

		public int CalcEffectValue(Unit caster, int value)
		{
			if (EffectValueOverrideEffect != null && caster.Spells.Contains(EffectValueOverrideEffect.Spell))
			{
				return EffectValueOverrideEffect.CalcEffectValue(caster, value);
			}

			if (caster == null)
			{
				return value;
			}

			if (APValueFactor != 0 || APPerComboPointValueFactor != 0)
			{
				var apFactor = APValueFactor + (APPerComboPointValueFactor * caster.ComboPoints);
				var ap = Spell.IsRanged ? caster.TotalRangedAP : caster.TotalMeleeAP;

				value += (int)(ap * apFactor + 0.5f); // implicit rounding
			}
			if (caster is Character)
			{
				if (SpellPowerValuePct != 0)
				{
					value += (SpellPowerValuePct * caster.GetDamageDoneMod(Spell.Schools[0]) + 50) / 100;
				}
			}
			if (EffectIndex <= 2)
			{
				SpellModifierType type;
				switch (EffectIndex)
				{
					case 0:
						type = SpellModifierType.EffectValue1;
						break;
					case 1:
						type = SpellModifierType.EffectValue2;
						break;
					case 3:
						type = SpellModifierType.EffectValue3;
						break;
					default:
						type = SpellModifierType.EffectValue4AndBeyond;
						break;
				}
				value = caster.Auras.GetModifiedInt(type, Spell, value);
			}
			value = caster.Auras.GetModifiedInt(SpellModifierType.AllEffectValues, Spell, value);

			return value;
		}

		public int CalcEffectValue()
		{
			return CalcEffectValue(0, 0, false);
		}

		public int CalcEffectValue(int level, int comboPoints, bool useOverride)
		{
			if (EffectValueOverrideEffect != null && useOverride)
			{
				return EffectValueOverrideEffect.CalcEffectValue(level, comboPoints, false);
			}

			// calculate effect value
			var value = BasePoints;

			// apply Unit boni
			value += (int)Math.Round(RealPointsPerLevel * Spell.GetMaxLevelDiff(level));
			value += (int)Math.Round(PointsPerComboPoint * comboPoints);

			// die += (uint)Math.Round(Effect.DicePerLevel * caster.Level);

			// dice bonus
			// see http://www.wowhead.com/spell=25269 for comparison
			if (DiceSides > 0)
			{
				value += Utility.Random(1, DiceSides);
			}

			return value;
		}

		public int GetMultipliedValue(Unit caster, int val, int currentTargetNo)
		{
			if (EffectIndex >= Spell.Effects.Length || EffectIndex < 0 || currentTargetNo == 0)
			{
				return val;
			}

			var dmgMod = Spell.Effects[EffectIndex].ChainAmplitude;
			if (caster != null)
			{
				dmgMod = caster.Auras.GetModifiedFloat(SpellModifierType.ChainValueFactor, Spell, dmgMod);
			}
			if (dmgMod != 1)
			{
				return val = MathUtil.RoundInt((float)(Math.Pow(dmgMod, currentTargetNo) * val));
			}
			return val;
		}

		public float GetRadius(ObjectReference caster)
		{
			var radius = Radius;
			var chr = caster.UnitMaster;
			if (chr != null)
			{
				radius = chr.Auras.GetModifiedFloat(SpellModifierType.Radius, Spell, radius);
			}
			if (radius < 5)
			{
				return 5;
			}
			return radius;
		}
		#endregion

		#region Modify Effects
		public void ClearAffectMask()
		{
			AffectMask = new uint[3];
		}

		public void SetAffectMask(params SpellLineId[] abilities)
		{
			ClearAffectMask();
			AddToAffectMask(abilities);
		}

		/// <summary>
		/// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
		/// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
		/// Only adds the spells, will not work on the spells' trigger spells.
		/// </summary>
		/// <param name="abilities"></param>
		public void AddAffectingSpells(params SpellLineId[] abilities)
		{
			if (AffectSpellSet == null)
			{
				AffectSpellSet = new HashSet<Spell>();
			}
			foreach (var ability in abilities)
			{
				AffectSpellSet.AddRange(SpellLines.GetLine(ability));
			}
		}

		/// <summary>
		/// Adds a set of spells to the explicite relationship set of this effect, which is used to determine whether
		/// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
		/// Only adds the spells, will not work on the spells' trigger spells.
		/// </summary>
		/// <param name="abilities"></param>
		public void AddAffectingSpells(params SpellId[] spells)
		{
			if (AffectSpellSet == null)
			{
				AffectSpellSet = new HashSet<Spell>();
			}
			foreach (var spellId in spells)
			{
				AffectSpellSet.Add(SpellHandler.Get(spellId));
			}
		}

		/// <summary>
		/// Adds a set of spells to this Effect's AffectMask, which is used to determine whether
		/// a certain Spell and this effect have some kind of influence on one another (for procs, talent modifiers etc).
		/// Usually the mask also contains any spell that is triggered by the original spell.
		/// 
		/// If you get a warning that the wrong set is affected, use AddAffectingSpells instead.
		/// </summary>
		public void AddToAffectMask(params SpellLineId[] abilities)
		{
			var newMask = new uint[SpellConstants.SpellClassMaskSize];

			// build new mask from abilities
			if (abilities.Length != 1)
			{
				foreach (var ability in abilities)
				{
					var spell = SpellLines.GetLine(ability).FirstRank;
					for (int i = 0; i < SpellConstants.SpellClassMaskSize; i++)
					{
                        newMask[i] |= spell.SpellClassOptions.SpellClassMask[i];
					}
				}
			}
			else
			{
                SpellLines.GetLine(abilities[0]).FirstRank.SpellClassOptions.SpellClassMask.CopyTo(newMask, 0);
			}

			// verification
            var affectedLines = SpellHandler.GetAffectedSpellLines(Spell.SpellClassOptions.ClassId, newMask);
			if (affectedLines.Count() != abilities.Length)
			{
				LogManager.GetCurrentClassLogger().Warn("[SPELL Inconsistency for {0}] " +
					"Invalid affect mask affects a different set than the one intended: {1} (intended: {2}) - " +
					"You might want to use AddAffectingSpells instead!",
					Spell, affectedLines.ToString(", "), abilities.ToString(", "));
			}

			for (int i = 0; i < SpellConstants.SpellClassMaskSize; i++)
			{
				AffectMask[i] |= newMask[i];
			}
		}

		public void CopyAffectMaskTo(uint[] mask)
		{
			for (var i = 0; i < AffectMask.Length; i++)
			{
				mask[i] |= AffectMask[i];
			}
		}

		public void RemoveAffectMaskFrom(uint[] mask)
		{
			for (var i = 0; i < AffectMask.Length; i++)
			{
				mask[i] ^= AffectMask[i];
			}
		}

		public bool MatchesSpell(Spell spell)
		{
			return (spell.SpellClassOptions.SpellClassSet == Spell.SpellClassOptions.SpellClassSet && spell.MatchesMask(AffectMask)) ||
				(AffectSpellSet != null && AffectSpellSet.Contains(spell));
		}

		public void MakeProc(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
		{
            Spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

			IsProc = true;
			ClearAffectMask();
			AddAffectingSpells(exclusiveTriggers);
			AuraEffectHandlerCreator = creator;
		}

		/// <summary>
		/// Uses the AffectMask, rather than exclusive trigger spells. This is important if also spells
		/// that are triggerd by the triggered spells are allowed to trigger this proc.
		/// </summary>
		public void MakeProcWithMask(AuraEffectHandlerCreator creator, params SpellLineId[] exclusiveTriggers)
		{
            Spell.SpellAuraOptions.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

			IsProc = true;
			SetAffectMask(exclusiveTriggers);
			AuraEffectHandlerCreator = creator;
		}

		public bool CanProcBeTriggeredBy(Spell spell)
		{
			return spell == null ||
					!HasAffectingSpells ||
					MatchesSpell(spell);
		}
		#endregion

		#region Dump
		public void DumpInfo(TextWriter writer, string indent)
		{
			writer.WriteLine(indent + "Effect: " + this);

			indent += "\t";

			//writer.WriteLine("Effect {0}", EffectIndex);
			if (ImplicitTargetA != ImplicitSpellTargetType.None)
			{
				writer.WriteLine(indent + "ImplicitTargetA: {0}", ImplicitTargetA);
			}
			if (ImplicitTargetB != ImplicitSpellTargetType.None)
			{
				writer.WriteLine(indent + "ImplicitTargetB: {0}", ImplicitTargetB);
			}
			if (MiscValue != 0 || MiscValueType != null)
			{
				writer.WriteLine(indent + "MiscValue: " + GetMiscStr(MiscValueType, MiscValue));
			}
			if (MiscValueB != 0)
			{
				writer.WriteLine(indent + "MiscValueB: " + GetMiscStr(MiscValueBType, MiscValueB));
			}

			if (AffectMask[0] != 0 || AffectMask[1] != 0 || AffectMask[2] != 0)
			{
				var lines = AffectedLines;
				writer.WriteLine(indent + "Affects: {0} ({1}{2}{3})", lines.Count() > 0 ? lines.ToString(", ") : "<Nothing>",
					AffectMask[0].ToString("X8"), AffectMask[1].ToString("X8"), AffectMask[2].ToString("X8"));
			}

			if (BasePoints != 0)
			{
				writer.WriteLine(indent + "BasePoints: {0}", BasePoints);
			}
			//if (DiceCount != 0)
			//{
			//    writer.WriteLine(indent + "DiceCount: {0}", DiceCount);
			//}
			if (DiceSides != 0)
			{
				writer.WriteLine(indent + "DiceSides: {0}", DiceSides);
			}
			//if (DiePerLevel != 0f)
			//{
			//	writer.WriteLine(indent + "DiePerLevel: {0}", DiePerLevel);
			//}
            if (AuraPeriod != 0)
            {
                writer.WriteLine(indent + "AuraPeriod: {0}", AuraPeriod);
            }
			if (Amplitude != 0)
			{
				writer.WriteLine(indent + "Amplitude: {0}", Amplitude);
			}
			if (ChainTargets != 0)
			{
				writer.WriteLine(indent + "ChainTarget: {0}", ChainTargets);
			}
			if (ItemId != 0)
			{
				writer.WriteLine(indent + "ItemId: {0} ({1})", (ItemId)ItemId, ItemId);
			}
			if (PointsPerComboPoint != 0f)
			{
				writer.WriteLine(indent + "PointsPerComboPoint: {0}", PointsPerComboPoint);
			}
			if (ChainAmplitude != 0f)
			{
				writer.WriteLine(indent + "ChainAmplitude: {0}", ChainAmplitude);
			}
			if (Radius != 0)
			{
				writer.WriteLine(indent + "Radius: {0}", Radius);
			}
			if (RealPointsPerLevel != 0.0f)
			{
				writer.WriteLine(indent + "RealPointsPerLevel: {0}", RealPointsPerLevel);
			}
			if (Mechanic != SpellMechanic.None)
			{
				writer.WriteLine(indent + "Mechanic: {0}", Mechanic);
			}
			if (TriggerSpellId != SpellId.None)
			{
				writer.WriteLine(indent + "Triggers: {0} ({1})", TriggerSpellId, (uint)TriggerSpellId);
			}

			var summonEntry = SummonEntry;
			if (summonEntry != null)
			{
				writer.WriteLine(indent + "Summon information:");
				indent += "\t";
				writer.WriteLine(indent + "Summon ID: {0}", summonEntry.Id);
				if (summonEntry.Group != 0)
				{
					writer.WriteLine(indent + "Summon Group: {0}", summonEntry.Group);
				}
				if (summonEntry.FactionTemplateId != 0)
				{
					writer.WriteLine(indent + "Summon Faction: {0}", summonEntry.FactionTemplateId);
				}
				if (summonEntry.Type != 0)
				{
					writer.WriteLine(indent + "Summon Type: {0}", summonEntry.Type);
				}
				if (summonEntry.Flags != 0)
				{
					writer.WriteLine(indent + "Summon Flags: {0}", summonEntry.Flags);
				}
				if (summonEntry.Slot != 0)
				{
					writer.WriteLine(indent + "Summon Slot: {0}", summonEntry.Slot);
				}

			}
		}
		#endregion

		#region Verbose
		public string GetTargetString()
		{
			int targetCount = 0;
			var targets = new List<string>(2);

			if (ImplicitTargetA != ImplicitSpellTargetType.None)
			{
				targetCount++;
				targets.Add("A: " + ImplicitTargetA);
			}
			if (ImplicitTargetB != ImplicitSpellTargetType.None)
			{
				targetCount++;
				targets.Add("B: " + ImplicitTargetB);
			}
			if (targets.Count > 0)
			{
				return "Targets (" + targetCount + ") - " + targets.ToArray().ToString(", ");
			}
			return "Targets: None";
		}

		string GetMiscStr(Type type, int val)
		{
			object obj = null;
			if (type != null && Utility.Parse(val.ToString(), type, ref obj))
			{
				return string.Format("{0} ({1})", obj, val);
			}
			else
			{
				return val.ToString();
			}
		}
		#endregion

		#region Spell Misc Data Representation
		private static readonly Type[] SpellEffectMiscValueTypes = new Type[(int)SpellEffectType.End];
		private static readonly Type[] AuraEffectMiscValueTypes = new Type[(int)AuraType.End];

		public static Type GetSpellEffectEffectMiscValueType(SpellEffectType type)
		{
			if (type >= SpellEffectType.End)
			{
				log.Warn("Found invalid SpellEffectType {0}.", type);
				return null;
			}
			return SpellEffectMiscValueTypes[(int)type];
		}

		public static void SetSpellEffectEffectMiscValueType(SpellEffectType effectType, Type type)
		{
			SpellEffectMiscValueTypes[(int)effectType] = type;
		}

		public static Type GetAuraEffectMiscValueType(AuraType type)
		{
			if (type >= AuraType.End)
			{
				log.Warn("Found invalid AuraType {0}.", type);
				return null;
			}
			return AuraEffectMiscValueTypes[(int)type];
		}

		public static void SetAuraEffectMiscValueType(AuraType auraType, Type type)
		{
			AuraEffectMiscValueTypes[(int)auraType] = type;
		}

		private static readonly Type[] SpellEffectMiscValueBTypes = new Type[(int)SpellEffectType.End];
		private static readonly Type[] AuraEffectMiscValueBTypes = new Type[(int)AuraType.End];
		private static readonly HashSet<ImplicitSpellTargetType> TargetAreaEffects = new HashSet<ImplicitSpellTargetType>(),
			AreaEffects = new HashSet<ImplicitSpellTargetType>();

		public static Type GetSpellEffectEffectMiscValueBType(SpellEffectType type)
		{
			if (type >= SpellEffectType.End)
			{
				log.Warn("Found invalid SpellEffectType {0}.", type);
				return null;
			}
			return SpellEffectMiscValueBTypes[(int)type];
		}

		public static void SetSpellEffectEffectMiscValueBType(SpellEffectType effectType, Type type)
		{
			SpellEffectMiscValueBTypes[(int)effectType] = type;
		}

		public static Type GetAuraEffectMiscValueBType(AuraType type)
		{
			if (type >= AuraType.End)
			{
				log.Warn("Found invalid AuraType {0}.", type);
				return null;
			}
			return AuraEffectMiscValueBTypes[(int)type];
		}

		public static void SetAuraEffectMiscValueBType(AuraType auraType, Type type)
		{
			AuraEffectMiscValueBTypes[(int)auraType] = type;
		}

		internal static void InitMiscValueTypes()
		{
			AuraEffectMiscValueTypes[(int)AuraType.AddModifierPercent] = typeof(SpellModifierType);
			AuraEffectMiscValueTypes[(int)AuraType.AddModifierFlat] = typeof(SpellModifierType);

			SetAuraEffectMiscValueType(AuraType.ModDamageDone, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModDamageDonePercent, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModDamageDoneToCreatureType, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModDamageDoneVersusCreatureType, typeof(CreatureMask));
			SetAuraEffectMiscValueType(AuraType.ModDamageTaken, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModDamageTakenPercent, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModPowerCost, typeof(PowerType));
			SetAuraEffectMiscValueType(AuraType.ModPowerCostForSchool, typeof(PowerType));
			SetAuraEffectMiscValueType(AuraType.ModPowerRegen, typeof(PowerType));
			SetAuraEffectMiscValueType(AuraType.ModPowerRegenPercent, typeof(PowerType));
			SetAuraEffectMiscValueType(AuraType.ModRating, typeof(CombatRatingMask));
			SetAuraEffectMiscValueType(AuraType.ModSkill, typeof(SkillId));
			SetAuraEffectMiscValueType(AuraType.ModSkillTalent, typeof(SkillId));
			SetAuraEffectMiscValueType(AuraType.ModStat, typeof(StatType));
			SetAuraEffectMiscValueType(AuraType.ModStatPercent, typeof(StatType));
			SetAuraEffectMiscValueType(AuraType.ModTotalStatPercent, typeof(StatType));
			SetAuraEffectMiscValueType(AuraType.DispelImmunity, typeof(DispelType));
			SetAuraEffectMiscValueType(AuraType.MechanicImmunity, typeof(SpellMechanic));
			SetAuraEffectMiscValueType(AuraType.Mounted, typeof(NPCId));
			SetAuraEffectMiscValueType(AuraType.ModShapeshift, typeof(ShapeshiftForm));
			SetAuraEffectMiscValueType(AuraType.Transform, typeof(NPCId));
			SetAuraEffectMiscValueType(AuraType.ModSpellDamageByPercentOfStat, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModSpellHealingByPercentOfStat, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.DamagePctAmplifier, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModSilenceDurationPercent, typeof(SpellMechanic));
			SetAuraEffectMiscValueType(AuraType.ModMechanicDurationPercent, typeof(SpellMechanic));
			SetAuraEffectMiscValueType(AuraType.TrackCreatures, typeof(CreatureType));
			SetAuraEffectMiscValueType(AuraType.ModSpellHitChance, typeof(DamageSchoolMask));
			SetAuraEffectMiscValueType(AuraType.ModSpellHitChance2, typeof(DamageSchoolMask));

			SetAuraEffectMiscValueBType(AuraType.ModSpellDamageByPercentOfStat, typeof(StatType));
			SetAuraEffectMiscValueBType(AuraType.ModSpellHealingByPercentOfStat, typeof(StatType));


			SetSpellEffectEffectMiscValueType(SpellEffectType.Dispel, typeof(DispelType));
			SetSpellEffectEffectMiscValueType(SpellEffectType.DispelMechanic, typeof(SpellMechanic));
			SetSpellEffectEffectMiscValueType(SpellEffectType.Skill, typeof(SkillId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.SkillStep, typeof(SkillId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.Skinning, typeof(SkinningType));
			SetSpellEffectEffectMiscValueType(SpellEffectType.Summon, typeof(NPCId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObject, typeof(GOEntryId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot1, typeof(GOEntryId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectSlot2, typeof(GOEntryId));
			SetSpellEffectEffectMiscValueType(SpellEffectType.SummonObjectWild, typeof(GOEntryId));

			SetSpellEffectEffectMiscValueBType(SpellEffectType.Summon, typeof(SummonType));

			TargetAreaEffects.AddRange(new[] {ImplicitSpellTargetType.AllAroundLocation,
			          ImplicitSpellTargetType.AllEnemiesInArea,
			          ImplicitSpellTargetType.AllEnemiesInAreaChanneled,
			          ImplicitSpellTargetType.AllEnemiesInAreaInstant,
			          ImplicitSpellTargetType.AllPartyInArea,
			          ImplicitSpellTargetType.AllPartyInAreaChanneled,
			          ImplicitSpellTargetType.InvisibleOrHiddenEnemiesAtLocationRadius});

			AreaEffects.AddRange(TargetAreaEffects);
			AreaEffects.AddRange(new[] {
						ImplicitSpellTargetType.AllEnemiesAroundCaster,
						ImplicitSpellTargetType.AllPartyAroundCaster,
						ImplicitSpellTargetType.AllTargetableAroundLocationInRadiusOverTime,
						ImplicitSpellTargetType.BehindTargetLocation,
						ImplicitSpellTargetType.LocationInFrontCaster,
						ImplicitSpellTargetType.LocationInFrontCasterAtRange,
						ImplicitSpellTargetType.ConeInFrontOfCaster,
						ImplicitSpellTargetType.AreaEffectPartyAndClass,
						ImplicitSpellTargetType.NatureSummonLocation,
						ImplicitSpellTargetType.TargetAtOrientationOfCaster,
						ImplicitSpellTargetType.Tranquility});
		}
		#endregion

		#region MiscValue Types
		public Type MiscValueType
		{
			get
			{
				if (IsAuraEffect)
				{
					return GetAuraEffectMiscValueType(AuraType);
				}
				return GetSpellEffectEffectMiscValueType(EffectType);
			}
		}

		public Type MiscValueBType
		{
			get
			{
				if (IsAuraEffect)
				{
					return GetAuraEffectMiscValueBType(AuraType);
				}
				return GetSpellEffectEffectMiscValueBType(EffectType);
			}
		}
		#endregion

		public override bool Equals(object obj)
		{
			return obj is SpellEffect && ((SpellEffect)obj).EffectType == EffectType;
		}

		public override int GetHashCode()
		{
			return EffectType.GetHashCode();
		}

		public override string ToString()
		{
			string triggerSpell;
			if (TriggerSpell != null)
			{
				triggerSpell = " (" + TriggerSpell + ")";
			}
			else
			{
				triggerSpell = "";
			}

			string aura;
			if (AuraType != AuraType.None)
			{
				aura = " (" + AuraType + ")";
			}
			else
			{
				aura = "";
			}

			return EffectType + triggerSpell + aura;
		}
	}
}