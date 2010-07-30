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
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras.Handlers;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
	public partial class SpellEffect
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private static readonly ImplicitTargetType[] NoTargetTypes = new[] {
			ImplicitTargetType.None,
			ImplicitTargetType.CaliriEggs,
			ImplicitTargetType.DynamicObject,
			ImplicitTargetType.GameObject,
			ImplicitTargetType.GameObjectOrItem,
			ImplicitTargetType.HeartstoneLocation,
			ImplicitTargetType.ScriptedGameObject,
			ImplicitTargetType.ScriptedLocation,
			ImplicitTargetType.ScriptedObjectLocation
		};

		#region Variables
		/// <summary>
		/// Factor of the amount of AP to be added to the EffectValue
		/// TODO: Change to int (%)
		/// </summary>
		public float APValueFactor;

		/// <summary>
		/// Amount of Spell Power to be added to the EffectValue in %
		/// </summary>
		public int SpellPowerValuePct;

		/// <summary>
		/// Factor of the amount of AP to be added to the EffectValue per combo point
		/// </summary>
		public float APPerComboPointValueFactor;

		public bool IsUsed;

		/// <summary>
		/// Only use this effect if the caster is in the given form (if given)
		/// </summary>
		public ShapeshiftMask RequiredShapeshiftMask;

		[NotPersistent]
		public SpellEffectHandlerCreator SpellEffectHandlerCreator;

		[NotPersistent]
		public AuraEffectHandlerCreator AuraEffectHandlerCreator;
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
				if (Spell.ClassId != 0)
				{
					return SpellHandler.GetAffectedSpellLines(Spell.ClassId, AffectMask);
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
				if (ImplicitTargetB != ImplicitTargetType.None && AreaEffects.Contains(ImplicitTargetB))
				{
					ImplicitTargetB = ImplicitTargetType.None;
				}
			}
			else if (ImplicitTargetB != ImplicitTargetType.None && AreaEffects.Contains(ImplicitTargetB))
			{
				IsAreaEffect = true;
				ImplicitTargetA = ImplicitTargetType.None;
			}

			if (IsPeriodic = Amplitude > 0)
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
			else if ((HasTarget(ImplicitTargetType.AllEnemiesAroundCaster,
			                    ImplicitTargetType.AllEnemiesInArea,
			                    ImplicitTargetType.AllEnemiesInAreaChanneled,
			                    ImplicitTargetType.AllEnemiesInAreaInstant,
			                    ImplicitTargetType.CurrentSelection) ||
			          HasTarget(ImplicitTargetType.InFrontOfCaster,
			                    ImplicitTargetType.InvisibleOrHiddenEnemiesAtLocationRadius,
			                    ImplicitTargetType.LocationInFrontCaster,
			                    ImplicitTargetType.NetherDrakeSummonLocation,
			                    ImplicitTargetType.SelectedEnemyChanneled,
			                    ImplicitTargetType.SelectedEnemyDeadlyPoison,
			                    ImplicitTargetType.SingleEnemy,
			                    ImplicitTargetType.SpreadableDesease,
			                    ImplicitTargetType.TargetAtOrientationOfCaster)) &&
			         (!HasTarget(
			         	ImplicitTargetType.Self,
			         	ImplicitTargetType.AllFriendlyInAura,
			         	ImplicitTargetType.AllParty,
			         	ImplicitTargetType.AllPartyAroundCaster,
			         	ImplicitTargetType.AllPartyInArea,
			         	ImplicitTargetType.PartyAroundCaster,
			         	ImplicitTargetType.AllPartyInAreaChanneled) ||
			          Spell.Mechanic.IsNegative()))
			{
				HarmType = HarmType.Harmful;
			}
			else if (!HasTarget(ImplicitTargetType.Duel) &&
			         (ImplicitTargetA != ImplicitTargetType.None || ImplicitTargetB != ImplicitTargetType.None))
			{
				HarmType = HarmType.Beneficial;
			}
			
			// do some correction for ModManaRegen
			if (AuraType == AuraType.ModManaRegen && Amplitude == 0)
			{
				// 5000 ms if not specified otherwise
				Amplitude = ModManaRegenHandler.DefaultAmplitude;
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
					ImplicitTargetA = ImplicitTargetType.AllPartyInArea;
				}
				else
				{
					ImplicitTargetA = ImplicitTargetType.AllParty;
				}
			}

			IsAuraEffect = IsAreaAuraEffect ||
			               EffectType == SpellEffectType.ApplyAura ||
			               EffectType == SpellEffectType.ApplyAuraToMaster ||
			               EffectType == SpellEffectType.ApplyPetAura ||
			               EffectType == SpellEffectType.ApplyStatAura ||
			               EffectType == SpellEffectType.ApplyStatAuraPercent;

			IsEnhancer = IsAuraEffect && (AuraType == AuraType.AddModifierFlat || AuraType == AuraType.AddModifierPercent);

			MiscBitSet = MiscValue > 0 ? Utility.GetSetIndices((uint) MiscValue) : new uint[0];

			MinValue = BasePoints; // + DiceCount; TODO: check this!

			IsStrikeEffectFlat = EffectType == SpellEffectType.WeaponDamage ||
			                     EffectType == SpellEffectType.WeaponDamageNoSchool ||
			                     EffectType == SpellEffectType.NormalizedWeaponDamagePlus;

			IsStrikeEffectPct = EffectType == SpellEffectType.WeaponPercentDamage;

			IsTotem = HasTarget(ImplicitTargetType.TotemAir) ||
			          HasTarget(ImplicitTargetType.TotemEarth) ||
			          HasTarget(ImplicitTargetType.TotemFire) ||
			          HasTarget(ImplicitTargetType.TotemWater);

			IsProc = IsProc || (AuraType == AuraType.ProcTriggerSpell && TriggerSpell != null) ||
			         AuraType == AuraType.ProcTriggerDamage;

			IsHealEffect = EffectType == SpellEffectType.Heal ||
			               EffectType == SpellEffectType.HealMaxHealth ||
			               AuraType == AuraType.PeriodicHeal ||
			               (TriggerSpell != null && TriggerSpell.IsHealSpell);

			IsModifierEffect = AuraType == AuraType.AddModifierFlat || AuraType == AuraType.AddModifierPercent;

			foreach (var mask in AffectMask)
			{
				if (mask != 0)
				{
					HasAffectMask = true;
					break;
				}
			}

			if (HasAffectMask)
			{
				AffectMaskBitSet = Utility.GetSetIndices(AffectMask);
			}

			if (SpellEffectHandlerCreator == null && !IsUsed)
			{
				SpellEffectHandlerCreator = SpellHandler.SpellEffectCreators[(int) EffectType];
			}
			if (IsAuraEffect && AuraEffectHandlerCreator == null)
			{
				AuraEffectHandlerCreator = AuraHandler.EffectHandlers[(int) AuraType];
				if (AuraEffectHandlerCreator == null)
				{
					AuraEffectHandlerCreator = AuraHandler.EffectHandlers[0];
				}
			}

			RepairBrokenTargetPairs();

			IsEnchantmentEffect = EffectType == SpellEffectType.EnchantHeldItem ||
				EffectType == SpellEffectType.EnchantItem ||
				EffectType == SpellEffectType.EnchantItemTemporary;
		}

		/// <summary>
		/// More or less frequently occuring target pairs that make no sense
		/// </summary>
		private void RepairBrokenTargetPairs()
		{
			// Used on some beam visuals
			if (ImplicitTargetA == ImplicitTargetType.Self && ImplicitTargetB == ImplicitTargetType.Duel)
			{
				// Duel and Self doesn't make sense -> Remove Self
				ImplicitTargetA = ImplicitTargetType.None;
			}
		}

		#endregion

		/// <summary>
		/// Whether b has the same targets as this effect
		/// </summary>
		public bool TargetsEqual(SpellEffect b)
		{
			return ImplicitTargetA == b.ImplicitTargetA && ImplicitTargetB == b.ImplicitTargetB;
		}

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

		public bool HasTarget(ImplicitTargetType target)
		{
			return ImplicitTargetA == target || ImplicitTargetB == target;
		}

		public bool HasTarget(params ImplicitTargetType[] targets)
		{
			return targets.FirstOrDefault(HasTarget) != 0;
		}

		public void CopyValuesTo(SpellEffect effect)
		{
			effect.BasePoints = BasePoints;
			effect.DiceSides = DiceSides;
		}

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
				return CalcEffectValue(casterReference.Level, 0);
			}
		}

		public int CalcEffectValue(Unit caster)
		{
			var value = CalcEffectValue(caster != null ? caster.Level : 1, caster != null ? caster.ComboPoints : 0);
			return CalcEffectValue(caster, value);
		}

		public int CalcEffectValue(Unit caster, int value)
		{
			if (caster != null)
			{
				if (APValueFactor != 0 || APPerComboPointValueFactor != 0)
				{
					var apFactor = APValueFactor + (APPerComboPointValueFactor * caster.ComboPoints);
					var ap = Spell.IsRanged ? caster.TotalRangedAP : caster.TotalMeleeAP;

					value += (int)(ap * apFactor + 0.5f);	// implicit rounding
				}
			}

			if (caster is Character)
			{
				if (SpellPowerValuePct != 0)
				{
					value += (SpellPowerValuePct * ((Character)caster).GetDamageDoneMod(Spell.Schools[0]) + 50) / 100;
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
					value = ((Character)caster).PlayerSpells.GetModifiedInt(type, Spell, value);
				}
				value = ((Character)caster).PlayerSpells.GetModifiedInt(SpellModifierType.AllEffectValues, Spell, value);
			}
			return value;
		}

		public int CalcEffectValue()
		{
			return CalcEffectValue(0, 0);
		}

		public int CalcEffectValue(int level, int comboPoints)
		{
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

		public float GetRadius(ObjectReference caster)
		{
			var radius = Radius;
			var chr = caster.UnitMaster as Character;
			if (chr != null)
			{
				radius = chr.PlayerSpells.GetModifiedFloat(SpellModifierType.Radius, Spell, radius);
			}
			if (radius < 5)
			{
				return 5;
			}
			return radius;
		}
		#endregion

		#region Dump
		public void DumpInfo(TextWriter writer, string indent)
		{
			writer.WriteLine(indent + "Effect: " + this);

			indent += "\t";

			//writer.WriteLine("Effect {0}", EffectIndex);
			if (ImplicitTargetA != ImplicitTargetType.None)
			{
				writer.WriteLine(indent + "ImplicitTargetA: {0}", ImplicitTargetA);
			}
			if (ImplicitTargetB != ImplicitTargetType.None)
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
			if (ProcValue != 0f)
			{
				writer.WriteLine(indent + "ProcValue: {0}", ProcValue);
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

			if (ImplicitTargetA != ImplicitTargetType.None)
			{
				targetCount++;
				targets.Add("A: " + ImplicitTargetA);
			}
			if (ImplicitTargetB != ImplicitTargetType.None)
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
		private static readonly HashSet<ImplicitTargetType> TargetAreaEffects = new HashSet<ImplicitTargetType>(),
			AreaEffects = new HashSet<ImplicitTargetType>();

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
		#endregion

		#region MiscValue Types
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
			SetAuraEffectMiscValueType(AuraType.ModSpellHitChance, typeof(DamageSchool));
			SetAuraEffectMiscValueType(AuraType.ModSpellHitChance2, typeof(DamageSchool));

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



			TargetAreaEffects.AddRange(new[] {ImplicitTargetType.AllAroundLocation,
			          ImplicitTargetType.AllEnemiesInArea,
			          ImplicitTargetType.AllEnemiesInAreaChanneled,
			          ImplicitTargetType.AllEnemiesInAreaInstant,
			          ImplicitTargetType.AllPartyInArea,
			          ImplicitTargetType.AllPartyInAreaChanneled,
			          ImplicitTargetType.InvisibleOrHiddenEnemiesAtLocationRadius});

			AreaEffects.AddRange(TargetAreaEffects);
			AreaEffects.AddRange(new[] {
						ImplicitTargetType.AllEnemiesAroundCaster,
						ImplicitTargetType.AllPartyAroundCaster,
						ImplicitTargetType.AllTargetableAroundLocationInRadiusOverTime,
						ImplicitTargetType.BehindTargetLocation,
						ImplicitTargetType.LocationInFrontCaster,
						ImplicitTargetType.LocationInFrontCasterAtRange,
						ImplicitTargetType.ConeInFrontOfCaster,
						ImplicitTargetType.AreaEffectPartyAndClass,
						ImplicitTargetType.NatureSummonLocation,
						ImplicitTargetType.TargetAtOrientationOfCaster,
						ImplicitTargetType.Tranquility});
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

		public void AddToAffectMask(params SpellLineId[] abilities)
		{
			foreach (var ability in abilities)
			{
				var spell = SpellLines.GetLine(ability).FirstRank;
				for (int i = 0; i < AffectMask.Length; i++)
				{
					AffectMask[i] |= spell.SpellClassMask[i];
				}
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

		#endregion

		public bool MatchesSpell(Spell spell)
		{
			return spell.SpellClassSet == Spell.SpellClassSet && spell.MatchesMask(AffectMask);
		}

		public int GetMultipliedValue(Character charCaster, int val, int currentTargetNo)
		{
			if (EffectIndex >= Spell.DamageMultipliers.Length || currentTargetNo == 0)
			{
				return val;
			}

			var dmgMod = Spell.DamageMultipliers[EffectIndex];
			if (charCaster != null)
			{
				dmgMod = charCaster.PlayerSpells.GetModifiedFloat(SpellModifierType.ChainValueFactor, Spell, dmgMod);
			}
			if (dmgMod != 1)
			{
				return val = ((float) (Math.Pow(dmgMod, currentTargetNo)*val)).RoundInt();
			}
			return val;
		}
	}
}