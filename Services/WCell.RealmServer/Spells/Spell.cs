/*************************************************************************
 *
 *   file		: Spell.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util;
using WCell.Util.Data;
using System.Text.RegularExpressions;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents any spell action or aura
	/// </summary>
	[DataHolder(RequirePersistantAttr = true)]
	public partial class Spell : IDataHolder, ISpellGroup
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="spell">The special Spell being casted</param>
		/// <param name="caster">The caster casting the spell</param>
		/// <param name="target">The target that the Caster selected (or null)</param>
		/// <param name="targetPos">The targetPos that was selected (or 0,0,0)</param>
		public delegate void SpecialCastHandler(Spell spell, WorldObject caster, WorldObject target, ref Vector3 targetPos);

		/// <summary>
		/// This Range will be used for all Spells that have MaxRange = 0
		/// </summary>
		public static int DefaultSpellRange = 30;

		private static readonly Regex numberRegex = new Regex(@"\d+");

		public static readonly Spell[] EmptyArray = new Spell[0];

		public Spell()
		{
			AISettings = new AISpellSettings(this);
		}

		#region Harmful SpellEffects
		//public static readonly HashSet<SpellEffectType> HarmfulSpellEffects = new Func<HashSet<SpellEffectType>>(() => {
		//    var effects = new HashSet<SpellEffectType>();

		//    effects.Add(SpellEffectType.Attack);
		//    effects.Add(SpellEffectType.AttackMe);
		//    effects.Add(SpellEffectType.DestroyAllTotems);
		//    effects.Add(SpellEffectType.Dispel);
		//    effects.Add(SpellEffectType.Attack);

		//    return effects;
		//})();
		#endregion

		#region Trigger Spells
		/// <summary>
		/// Add Spells to be casted on the targets of this Spell
		/// </summary>
		public void AddTargetTriggerSpells(params SpellId[] spellIds)
		{
			var spells = new Spell[spellIds.Length];
			for (var i = 0; i < spellIds.Length; i++)
			{
				var id = spellIds[i];
				var spell = SpellHandler.Get(id);
				if (spell == null)
				{
					throw new InvalidSpellDataException("Invalid SpellId: " + id);
				}
				spells[i] = spell;
			}
			AddTargetTriggerSpells(spells);
		}

		/// <summary>
		/// Add Spells to be casted on the targets of this Spell
		/// </summary>
		public void AddTargetTriggerSpells(params Spell[] spells)
		{
			if (TargetTriggerSpells == null)
			{
				TargetTriggerSpells = spells;
			}
			else
			{
				var oldLen = TargetTriggerSpells.Length;
				Array.Resize(ref TargetTriggerSpells, oldLen + spells.Length);
				Array.Copy(spells, 0, TargetTriggerSpells, oldLen, spells.Length);
			}
		}

		/// <summary>
		/// Add Spells to be casted on the targets of this Spell
		/// </summary>
		public void AddCasterTriggerSpells(params SpellId[] spellIds)
		{
			var spells = new Spell[spellIds.Length];
			for (var i = 0; i < spellIds.Length; i++)
			{
				var id = spellIds[i];
				var spell = SpellHandler.Get(id);
				if (spell == null)
				{
					throw new InvalidSpellDataException("Invalid SpellId: " + id);
				}
				spells[i] = spell;
			}
			AddCasterTriggerSpells(spells);
		}

		/// <summary>
		/// Add Spells to be casted on the targets of this Spell
		/// </summary>
		public void AddCasterTriggerSpells(params Spell[] spells)
		{
			if (CasterTriggerSpells == null)
			{
				CasterTriggerSpells = spells;
			}
			else
			{
				var oldLen = CasterTriggerSpells.Length;
				Array.Resize(ref CasterTriggerSpells, oldLen + spells.Length);
				Array.Copy(spells, 0, CasterTriggerSpells, oldLen, spells.Length);
			}
		}
		#endregion

		#region Custom Proc Handlers
		/// <summary>
		/// Add Handler to be enabled when this aura spell is active
		/// </summary>
		public void AddProcHandler(ProcHandlerTemplate handler)
		{
			if (ProcHandlers == null)
			{
				ProcHandlers = new List<ProcHandlerTemplate>();
			}
			ProcHandlers.Add(handler);
			if (Effects.Length == 0)
			{
				// need at least one effect to make this work
				AddAuraEffect(AuraType.Dummy);
			}
		}
		#endregion

		/// <summary>
		/// List of Spells to be learnt when this Spell is learnt
		/// </summary>
		public readonly List<Spell> AdditionallyTaughtSpells = new List<Spell>(0);

		#region Field Generation (Generates the value of many fields, based on the 200+ original Spell properties)
		/// <summary>
		/// Sets all default variables
		/// </summary>
		internal void Initialize()
		{
			if (Id == 11986)
				ToString();

			init1 = true;
			var learnSpellEffect = GetEffect(SpellEffectType.LearnSpell);
			if (learnSpellEffect == null)
			{
				learnSpellEffect = GetEffect(SpellEffectType.LearnPetSpell);
			}
			if (learnSpellEffect != null && learnSpellEffect.TriggerSpellId != 0)
			{
				IsTeachSpell = true;
			}

			// figure out Trigger spells
			for (var i = 0; i < Effects.Length; i++)
			{
				var effect = Effects[i];
				if (effect.TriggerSpellId != SpellId.None || effect.AuraType == AuraType.PeriodicTriggerSpell)
				{
					var triggeredSpell = SpellHandler.Get((uint)effect.TriggerSpellId);
					if (triggeredSpell != null)
					{
						if (!IsTeachSpell)
						{
							triggeredSpell.IsTriggeredSpell = true;
						}
						else
						{
							LearnSpell = triggeredSpell;
						}
						effect.TriggerSpell = triggeredSpell;
					}
					else
					{
						if (IsTeachSpell)
						{
							IsTeachSpell = GetEffect(SpellEffectType.LearnSpell) != null;
						}
					}
				}
			}

			foreach (var effect in Effects)
			{
				if (effect.EffectType == SpellEffectType.PersistantAreaAura// || effect.HasTarget(ImplicitTargetType.DynamicObject)
					)
				{
					DOEffect = effect;
					break;
				}
			}

			//foreach (var effect in Effects)
			//{
			//    effect.Initialize();
			//}
		}

		/// <summary>
		/// For all things that depend on info of all spells from first Init-round and other things
		/// </summary>
		internal void Init2()
		{
			if (init2)
			{
				return;
			}

			init2 = true;

			IsPassive = (Attributes.HasFlag(SpellAttributes.Passive)) ||
			            // tracking spells are also passive		     
			            HasEffectWith(effect => effect.AuraType == AuraType.TrackCreatures) ||
			            HasEffectWith(effect => effect.AuraType == AuraType.TrackResources) ||
			            HasEffectWith(effect => effect.AuraType == AuraType.TrackStealthed);

			IsChanneled = !IsPassive && AttributesEx.HasAnyFlag(SpellAttributesEx.Channeled_1 | SpellAttributesEx.Channeled_2) ||
			              // don't use Enum.HasFlag!
			              ChannelInterruptFlags > 0;

			foreach (var effect in Effects)
			{
				effect.Init2();
				if (effect.IsHealEffect)
				{
					IsHealSpell = true;
				}
				if (effect.EffectType == SpellEffectType.NormalizedWeaponDamagePlus)
				{
					IsDualWieldAbility = true;
				}
			}

			InitAura();

			if (IsChanneled)
			{
				if (Durations.Min == 0)
				{
					Durations.Min = Durations.Max = 1000;
				}

				foreach (var effect in Effects)
				{
					if (effect.IsPeriodic)
					{
						ChannelAmplitude = effect.Amplitude;
						break;
					}
				}
			}

			IsOnNextStrike = Attributes.HasAnyFlag(SpellAttributes.OnNextMelee | SpellAttributes.OnNextMelee_2);
			// don't use Enum.HasFlag!

			IsRanged = (Attributes.HasAnyFlag(SpellAttributes.Ranged) ||
			            AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon));

			IsRangedAbility = IsRanged && !IsTriggeredSpell;

			IsStrikeSpell = HasEffectWith(effect => effect.IsStrikeEffect);

			IsPhysicalAbility = (IsRangedAbility || IsOnNextStrike || IsStrikeSpell) && !HasEffect(SpellEffectType.SchoolDamage);

			DamageIncreasedByAP = DamageIncreasedByAP || (PowerType == PowerType.Rage && SchoolMask == DamageSchoolMask.Physical);

			GeneratesComboPoints = HasEffectWith(effect => effect.EffectType == SpellEffectType.AddComboPoints);

			IsFinishingMove =
				AttributesEx.HasAnyFlag(SpellAttributesEx.FinishingMove) ||
				HasEffectWith(effect => effect.PointsPerComboPoint > 0 && effect.EffectType != SpellEffectType.Dummy);

			TotemEffect = GetFirstEffectWith(effect => effect.HasTarget(
				ImplicitSpellTargetType.TotemAir, ImplicitSpellTargetType.TotemEarth, ImplicitSpellTargetType.TotemFire,
				ImplicitSpellTargetType.TotemWater));

			IsEnchantment = HasEffectWith(effect => effect.IsEnchantmentEffect);

			if (!IsEnchantment && EquipmentSlot == EquipmentSlot.End)
			{
				// Required Item slot for weapon abilities
				if (RequiredItemClass == ItemClass.Armor && RequiredItemSubClassMask == ItemSubClassMask.Shield)
				{
					EquipmentSlot = EquipmentSlot.OffHand;
				}
				else if ((IsRangedAbility || AttributesExC.HasFlag(SpellAttributesExC.RequiresWand)))
				{
					EquipmentSlot = EquipmentSlot.ExtraWeapon;
				}
				else if (AttributesExC.HasFlag(SpellAttributesExC.RequiresOffHandWeapon))
				{
					EquipmentSlot = EquipmentSlot.OffHand;
				}
				else if (AttributesExC.HasFlag(SpellAttributesExC.RequiresMainHandWeapon))
				{
					EquipmentSlot = EquipmentSlot.MainHand;
				}
				else if (RequiredItemClass == ItemClass.Weapon)
				{
					if (RequiredItemSubClassMask == ItemSubClassMask.AnyMeleeWeapon)
					{
						EquipmentSlot = EquipmentSlot.MainHand;
					}
					else if (RequiredItemSubClassMask == ItemSubClassMask.AnyRangedWeapon)
					{
						EquipmentSlot = EquipmentSlot.ExtraWeapon;
					}
				}
				else if (IsPhysicalAbility)
				{
					// OnNextMelee is set but no equipment slot -> select main hand
					EquipmentSlot = EquipmentSlot.MainHand;
				}
			}

			HasIndividualCooldown = CooldownTime > 0 ||
			                        (IsPhysicalAbility && !IsOnNextStrike && EquipmentSlot != EquipmentSlot.End);

			HasCooldown = HasIndividualCooldown || CategoryCooldownTime > 0;

			//IsAoe = HasEffectWith((effect) => {
			//    if (effect.ImplicitTargetA == ImplicitTargetType.)
			//        effect.ImplicitTargetA = ImplicitTargetType.None;
			//    if (effect.ImplicitTargetB == ImplicitTargetType.Unused_EnemiesInAreaChanneledWithExceptions)
			//        effect.ImplicitTargetB = ImplicitTargetType.None;
			//    return false;
			//});

			var profEffect = GetEffect(SpellEffectType.SkillStep);
			if (profEffect != null)
			{
				TeachesApprenticeAbility = profEffect.BasePoints == 0;
			}

			IsProfession = !IsRangedAbility && Ability != null && Ability.Skill.Category == SkillCategory.Profession;
			IsEnhancer = HasEffectWith(effect => effect.IsEnhancer);
			IsFishing = HasEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.SelfFishing));
			IsSkinning = HasEffectWith(effect => effect.EffectType == SpellEffectType.Skinning);
			IsTameEffect = HasEffectWith(effect => effect.EffectType == SpellEffectType.TameCreature);

			if (IsPreventionDebuff || Mechanic.IsNegative())
			{
				HasHarmfulEffects = true;
				HasBeneficialEffects = false;
				HarmType = HarmType.Harmful;
			}
			else
			{
				HasHarmfulEffects = HasEffectWith(effect => effect.HarmType == HarmType.Harmful);
				HasBeneficialEffects = HasEffectWith(effect => effect.HarmType == HarmType.Beneficial);
				if (HasHarmfulEffects != HasBeneficialEffects && !HasEffectWith(effect => effect.HarmType == HarmType.Neutral))
				{
					HarmType = HasHarmfulEffects ? HarmType.Harmful : HarmType.Beneficial;
				}
				else
				{
					HarmType = HarmType.Neutral;
				}
			}

			RequiresDeadTarget = HasEffect(SpellEffectType.Resurrect) || HasEffect(SpellEffectType.ResurrectFlat) || HasEffect(SpellEffectType.SelfResurrect);
				// unreliable: TargetFlags.HasAnyFlag(SpellTargetFlags.Corpse | SpellTargetFlags.PvPCorpse | SpellTargetFlags.UnitCorpse);

			CostsPower = PowerCost > 0 || PowerCostPercentage > 0;

			CostsRunes = RuneCostEntry != null && RuneCostEntry.CostsRunes;

			HasTargets = HasEffectWith(effect => effect.HasTargets);

			CasterIsTarget = HasTargets && HasEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.Self));

			//HasSingleNotSelfTarget = 

			IsAreaSpell = HasEffectWith(effect => effect.IsAreaEffect);

			IsDamageSpell = HasHarmfulEffects && !HasBeneficialEffects && HasEffectWith(effect =>
			                                                                            effect.EffectType ==
			                                                                            SpellEffectType.Attack ||
			                                                                            effect.EffectType ==
			                                                                            SpellEffectType.EnvironmentalDamage ||
			                                                                            effect.EffectType ==
			                                                                            SpellEffectType.InstantKill ||
			                                                                            effect.EffectType ==
			                                                                            SpellEffectType.SchoolDamage ||
			                                                                            effect.IsStrikeEffect);

			if (DamageMultipliers[0] <= 0)
			{
				DamageMultipliers[0] = 1;
			}

			IsHearthStoneSpell = HasEffectWith(effect => effect.HasTarget(ImplicitSpellTargetType.HeartstoneLocation));

			// ResurrectFlat usually has no target type set
			ForeachEffect(effect =>
			              	{
			              		if (effect.ImplicitTargetA == ImplicitSpellTargetType.None &&
			              		    effect.EffectType == SpellEffectType.ResurrectFlat)
			              		{
			              			effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
			              		}
			              	});

			Schools = Utility.GetSetIndices<DamageSchool>((uint) SchoolMask);
			if (Schools.Length == 0)
			{
				Schools = new[] {DamageSchool.Physical};
			}

			RequiresCasterOutOfCombat = !HasHarmfulEffects && CastDelay > 0 &&
			                            (Attributes.HasFlag(SpellAttributes.CannotBeCastInCombat) ||
			                             AttributesEx.HasFlag(SpellAttributesEx.RemainOutOfCombat) ||
			                             AuraInterruptFlags.HasFlag(AuraInterruptFlags.OnStartAttack));

			if (RequiresCasterOutOfCombat)
			{
				// We fail if being attacked (among others)
				InterruptFlags |= InterruptFlags.OnTakeDamage;
			}

			IsThrow = AttributesExC.HasFlag(SpellAttributesExC.ShootRangedWeapon) &&
			          Attributes.HasFlag(SpellAttributes.Ranged) && Ability != null && Ability.Skill.Id == SkillId.Thrown;

			HasModifierEffects = HasModifierEffects ||
			                     HasEffectWith(
			                     	effect =>
			                     	effect.AuraType == AuraType.AddModifierFlat || effect.AuraType == AuraType.AddModifierPercent);

			// cannot taunt players
			CanCastOnPlayer = CanCastOnPlayer && !HasEffect(AuraType.ModTaunt);

			HasAuraDependentEffects = HasEffectWith(effect => effect.IsDependentOnOtherAuras);

			ForeachEffect(effect =>
			              	{
			              		for (var i = 0; i < 3; i++)
			              		{
			              			AllAffectingMasks[i] |= effect.AffectMask[i];
			              		}
			              	});

			if (Range.MaxDist == 0)
			{
				Range.MaxDist = 5;
			}

			if (RequiredToolIds == null)
			{
				RequiredToolIds = new uint[0];
			}
			else
			{
				if (RequiredToolIds.Length > 0 && (RequiredToolIds[0] > 0 || RequiredToolIds[1] > 0))
				{
					SpellHandler.SpellsRequiringTools.Add(this);
				}
				ArrayUtil.PruneVals(ref RequiredToolIds);
			}

			var skillEffect = GetFirstEffectWith(effect =>
			                                     effect.EffectType == SpellEffectType.SkillStep ||
			                                     effect.EffectType == SpellEffectType.Skill);
			if (skillEffect != null)
			{
				SkillTier = (SkillTierId) skillEffect.BasePoints;
			}
			else
			{
				SkillTier = SkillTierId.End;
			}

			ArrayUtil.PruneVals(ref RequiredToolCategories);

			ForeachEffect(effect =>
			              	{
			              		if (effect.SpellEffectHandlerCreator != null)
			              		{
			              			EffectHandlerCount++;
			              		}
			              	});
			//IsHealSpell = HasEffectWith((effect) => effect.IsHealEffect);

			if (GetEffect(SpellEffectType.QuestComplete) != null)
			{
				SpellHandler.QuestCompletors.Add(this);
			}

			AISettings.InitializeAfterLoad();
		}
		#endregion

		#region Targeting
		/// <summary>
		/// Sets the AITargetHandlerDefintion of all effects
		/// </summary>
		public void OverrideCustomTargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
		{
			OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters));
		}

		/// <summary>
		/// Sets the CustomTargetHandlerDefintion of all effects
		/// </summary>
		public void OverrideCustomTargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null, 
			params TargetFilter[] filters)
		{
			OverrideCustomTargetDefinitions(new TargetDefinition(adder, filters), evaluator);
		}

		public void OverrideCustomTargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
		{
			ForeachEffect(
				effect => effect.CustomTargetHandlerDefintion = def);
			if (evaluator != null)
			{
				OverrideCustomTargetEvaluators(evaluator);
			}
		}

		/// <summary>
		/// Sets the AITargetHandlerDefintion of all effects
		/// </summary>
		public void OverrideAITargetDefinitions(TargetAdder adder, params TargetFilter[] filters)
		{
			OverrideAITargetDefinitions(new TargetDefinition(adder, filters));
		}

		/// <summary>
		/// Sets the AITargetHandlerDefintion of all effects
		/// </summary>
		public void OverrideAITargetDefinitions(TargetAdder adder, TargetEvaluator evaluator = null,
			params TargetFilter[] filters)
		{
			OverrideAITargetDefinitions(new TargetDefinition(adder, filters), evaluator);
		}

		public void OverrideAITargetDefinitions(TargetDefinition def, TargetEvaluator evaluator = null)
		{
			ForeachEffect(
				effect => effect.AITargetHandlerDefintion = def);
			if (evaluator != null)
			{
				OverrideCustomTargetEvaluators(evaluator);
			}
		}

		/// <summary>
		/// Sets the CustomTargetEvaluator of all effects
		/// </summary>
		public void OverrideCustomTargetEvaluators(TargetEvaluator eval)
		{
			ForeachEffect(
				effect => effect.CustomTargetEvaluator = eval);
		}

		/// <summary>
		/// Sets the AITargetEvaluator of all effects
		/// </summary>
		public void OverrideAITargetEvaluators(TargetEvaluator eval)
		{
			ForeachEffect(
				effect => effect.AITargetEvaluator = eval);
		}

		#endregion

		#region Manage Effects
		public void ForeachEffect(Action<SpellEffect> callback)
		{
			for (int i = 0; i < Effects.Length; i++)
			{
				var effect = Effects[i];
				callback(effect);
			}
		}

		public bool HasEffectWith(Predicate<SpellEffect> predicate)
		{
			for (var i = 0; i < Effects.Length; i++)
			{
				var effect = Effects[i];
				if (predicate(effect))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasEffect(SpellEffectType type)
		{
			return GetEffect(type, false) != null;
		}

		public bool HasEffect(AuraType type)
		{
			return GetEffect(type, false) != null;
		}

		/// <summary>
		/// Returns the first SpellEffect of the given Type within this Spell
		/// </summary>
		public SpellEffect GetEffect(SpellEffectType type)
		{
			return GetEffect(type, true);
		}

		/// <summary>
		/// Returns the first SpellEffect of the given Type within this Spell
		/// </summary>
		public SpellEffect GetEffect(SpellEffectType type, bool force)
		{
			foreach (var effect in Effects)
			{
				if (effect.EffectType == type)
				{
					return effect;
				}
			}
			//ContentHandler.OnInvalidClientData("Spell {0} does not contain Effect of type {1}", this, type);
			//return null;
			if (!init1 && force)
			{
				throw new ContentException("Spell {0} does not contain Effect of type {1}", this, type);
			}
			return null;
		}

		/// <summary>
		/// Returns the first SpellEffect of the given Type within this Spell
		/// </summary>
		public SpellEffect GetEffect(AuraType type)
		{
			return GetEffect(type, ContentMgr.ForceDataPresence);
		}

		/// <summary>
		/// Returns the first SpellEffect of the given Type within this Spell
		/// </summary>
		public SpellEffect GetEffect(AuraType type, bool force)
		{
			foreach (var effect in Effects)
			{
				if (effect.AuraType == type)
				{
					return effect;
				}
			}
			//ContentHandler.OnInvalidClientData("Spell {0} does not contain Aura Effect of type {1}", this, type);
			//return null;
			if (!init1 && force)
			{
				throw new ContentException("Spell {0} does not contain Aura Effect of type {1}", this, type);
			}
			return null;
		}

		public SpellEffect GetFirstEffectWith(Predicate<SpellEffect> predicate)
		{
			foreach (var effect in Effects)
			{
				if (predicate(effect))
				{
					return effect;
				}
			}
			return null;
		}

		public SpellEffect[] GetEffectsWhere(Predicate<SpellEffect> predicate)
		{
			List<SpellEffect> effects = null;
			foreach (var effect in Effects)
			{
				if (predicate(effect))
				{
					if (effects == null)
					{
						effects = new List<SpellEffect>();
					}
					effects.Add(effect);
				}
			}
			return effects != null ? effects.ToArray() : null;
		}

		///// <summary>
		///// Removes the first Effect of the given Type and replace it with a new one which will be returned.
		///// Appends a new one if none of the given type was found.
		///// </summary>
		///// <param name="type"></param>
		///// <returns></returns>
		//public SpellEffect ReplaceEffect(SpellEffectType type, SpellEffectType newType, ImplicitTargetType target)
		//{
		//    for (var i = 0; i < Effects.Length; i++)
		//    {
		//        var effect = Effects[i];
		//        if (effect.EffectType == type)
		//        {
		//            return Effects[i] = new SpellEffect();
		//        }
		//    }
		//    return AddEffect(type, target);
		//}

		/// <summary>
		/// Adds a new Effect to this Spell
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public SpellEffect AddEffect(SpellEffectHandlerCreator creator, ImplicitSpellTargetType target)
		{
			var effect = AddEffect(SpellEffectType.Dummy, target);
			effect.SpellEffectHandlerCreator = creator;
			return effect;
		}

		/// <summary>
		/// Adds a new Effect to this Spell
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public SpellEffect AddEffect(SpellEffectType type, ImplicitSpellTargetType target)
		{
			var effect = new SpellEffect(this, -1) { EffectType = type };
			var effects = new SpellEffect[Effects.Length + 1];
			Array.Copy(Effects, effects, Effects.Length);
			Effects = effects;
			Effects[effects.Length - 1] = effect;

			effect.ImplicitTargetA = target;
			return effect;
		}

		/// <summary>
		/// Adds a SpellEffect that will trigger the given Spell on oneself
		/// </summary>
		public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell)
		{
			return AddTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
		}

		/// <summary>
		/// Adds a SpellEffect that will trigger the given Spell on the given type of target
		/// </summary>
		public SpellEffect AddTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
		{
			var effect = AddEffect(SpellEffectType.TriggerSpell, targetType);
			effect.TriggerSpellId = triggerSpell;
			return effect;
		}

		/// <summary>
		/// Adds a SpellEffect that will trigger the given Spell on oneself
		/// </summary>
		public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell)
		{
			return AddPeriodicTriggerSpellEffect(triggerSpell, ImplicitSpellTargetType.Self);
		}

		/// <summary>
		/// Adds a SpellEffect that will trigger the given Spell on the given type of target
		/// </summary>
		public SpellEffect AddPeriodicTriggerSpellEffect(SpellId triggerSpell, ImplicitSpellTargetType targetType)
		{
			var effect = AddAuraEffect(AuraType.PeriodicTriggerSpell);
			effect.TriggerSpellId = triggerSpell;
			effect.ImplicitTargetA = targetType;
			return effect;
		}

		/// <summary>
		/// Adds a SpellEffect that will be applied to an Aura to be casted on oneself
		/// </summary>
		public SpellEffect AddAuraEffect(AuraType type)
		{
			return AddAuraEffect(type, ImplicitSpellTargetType.Self);
		}

		/// <summary>
		/// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
		/// </summary>
		public SpellEffect AddAuraEffect(AuraType type, ImplicitSpellTargetType targetType)
		{
			var effect = AddEffect(SpellEffectType.ApplyAura, targetType);
			effect.AuraType = type;
			return effect;
		}

		/// <summary>
		/// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
		/// </summary>
		public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator)
		{
			return AddAuraEffect(creator, ImplicitSpellTargetType.Self);
		}

		/// <summary>
		/// Adds a SpellEffect that will be applied to an Aura to be casted on the given type of target
		/// </summary>
		public SpellEffect AddAuraEffect(AuraEffectHandlerCreator creator, ImplicitSpellTargetType targetType)
		{
			var effect = AddEffect(SpellEffectType.ApplyAura, targetType);
			effect.AuraType = AuraType.Dummy;
			effect.AuraEffectHandlerCreator = creator;
			return effect;
		}

		public void ClearEffects()
		{
			Effects = new SpellEffect[0];
		}

		public SpellEffect RemoveEffect(AuraType type)
		{
			var effect = GetEffect(type);
			RemoveEffect(effect);
			return effect;
		}

		public SpellEffect RemoveEffect(SpellEffectType type)
		{
			var effect = GetEffect(type);
			RemoveEffect(effect);
			return effect;
		}

		public void RemoveEffect(SpellEffect toRemove)
		{
			var effects = new SpellEffect[Effects.Length - 1];
			var e = 0;
			foreach (var effct in Effects)
			{
				if (effct != toRemove)
				{
					effects[e++] = effct;
				}
			}
			Effects = effects;
		}

		public void RemoveEffect(Func<SpellEffect, bool> predicate)
		{
			foreach (var effct in Effects.ToArray())
			{
				if (predicate(effct))
				{
					RemoveEffect(effct);
				}
			}
		}
		#endregion

		#region Misc Methods & Props
		public bool IsAffectedBy(Spell spell)
		{
			return MatchesMask(spell.AllAffectingMasks);
		}

		public bool MatchesMask(uint[] masks)
		{
			for (var i = 0; i < SpellClassMask.Length; i++)
			{
				if ((masks[i] & SpellClassMask[i]) != 0)
				{
					return true;
				}
			}
			return false;
		}

		public int GetMaxLevelDiff(int casterLevel)
		{
			if (MaxLevel >= BaseLevel && MaxLevel < casterLevel)
			{
				return MaxLevel - BaseLevel;
			}
			return Math.Abs(casterLevel - BaseLevel);
		}

		public int CalcBasePowerCost(Unit caster)
		{
			var cost = PowerCost + (PowerCostPerlevel * GetMaxLevelDiff(caster.Level));
			if (PowerCostPercentage > 0)
			{
				cost += (PowerCostPercentage *
					((PowerType == PowerType.Health ? caster.BaseHealth : caster.BasePower))) / 100;
			}
			return cost;
		}

		public int CalcPowerCost(Unit caster, DamageSchool school)
		{
			return caster.GetPowerCost(school, this, CalcBasePowerCost(caster));
		}

		public bool ShouldShowToClient()
		{
			return IsRangedAbility || Visual != 0 || Visual2 != 0 ||
				   IsChanneled || CastDelay > 0 || HasCooldown;
			// || (!IsPassive && IsAura)
			;
		}

		public void SetDuration(int duration)
		{
			Durations.Min = Durations.Max = duration;
		}

		/// <summary>
		/// Returns the max duration for this Spell in milliseconds, 
		/// including all modifiers.
		/// </summary>
		public int GetDuration(ObjectReference caster)
		{
			return GetDuration(caster, null);
		}

		/// <summary>
		/// Returns the max duration for this Spell in milliseconds, 
		/// including all modifiers.
		/// </summary>
		public int GetDuration(ObjectReference caster, Unit target)
		{
			var millis = Durations.Min;
			//if (Durations.LevelDelta > 0)
			//{
			//	millis += (int)caster.Level * Durations.LevelDelta;
			//	if (Durations.Max > 0 && millis > Durations.Max)
			//	{
			//		millis = Durations.Max;
			//	}
			//}

			if (Durations.Max > Durations.Min && IsFinishingMove && caster.UnitMaster != null)
			{
				// For some finishing moves, Duration depends on Combopoints
				millis += caster.UnitMaster.ComboPoints * ((Durations.Max - Durations.Min) / 5);
			}

			if (target != null && Mechanic != SpellMechanic.None)
			{
				var mod = target.GetMechanicDurationMod(Mechanic);
				if (mod != 0)
				{
					millis = UnitUpdates.GetMultiMod(mod / 100f, millis);
				}
			}

			var unit = caster.UnitMaster;
			if (unit != null)
			{
				millis = unit.Auras.GetModifiedInt(SpellModifierType.Duration, this, millis);
			}
			return millis;
		}
		#endregion

		#region Verbose / Debug

		/// <summary>
		/// Fully qualified name
		/// </summary>
		public string FullName
		{
			get
			{
				// TODO: Item-spell?
				string fullName;

				bool isTalent = Talent != null;
				bool isSkill = Ability != null;

				if (isTalent)
				{
					fullName = Talent.FullName;
				}
				else
				{
					fullName = Name;
				}

				if (isSkill && !isTalent && Ability.Skill.Category != SkillCategory.Language &&
					Ability.Skill.Category != SkillCategory.Invalid)
				{
					fullName = Ability.Skill.Category + " " + fullName;
				}

				if (IsTeachSpell &&
					!Name.StartsWith("Learn", StringComparison.InvariantCultureIgnoreCase))
				{
					fullName = "Learn " + fullName;
				}
				else if (IsTriggeredSpell)
				{
					fullName = "Effect: " + fullName;
				}

				if (isSkill)
				{
				}
				else if (IsDeprecated)
				{
					fullName = "Unused " + fullName;
				}
				else if (Description != null && Description.Length == 0)
				{
					//fullName = "No Learn " + fullName;
				}


				return fullName;
			}
		}
		/// <summary>
		/// Spells that contain "zzOld", "test", "unused"
		/// </summary>
		public bool IsDeprecated
		{
			get
			{
				return IsDeprecatedSpellName(Name);
			}
		}

		public static bool IsDeprecatedSpellName(string name)
		{
			return name.IndexOf("test", StringComparison.InvariantCultureIgnoreCase) > -1 ||
					   name.StartsWith("zzold", StringComparison.InvariantCultureIgnoreCase) ||
					   name.IndexOf("unused", StringComparison.InvariantCultureIgnoreCase) > -1;
		}

		public override string ToString()
		{
			return FullName + (RankDesc != "" ? " " + RankDesc : "") + " (Id: " + Id + ")";
		}

		#endregion

		#region Dump
		public void Dump(TextWriter writer, string indent)
		{
			writer.WriteLine("Spell: " + this + " [" + SpellId + "]");

			if (Category != 0)
			{
				writer.WriteLine(indent + "Category: " + Category);
			}
			if (Line != null)
			{
				writer.WriteLine(indent + "Line: " + Line);
			}
			if (PreviousRank != null)
			{
				writer.WriteLine(indent + "Previous Rank: " + PreviousRank);
			}
			if (NextRank != null)
			{
				writer.WriteLine(indent + "Next Rank: " + NextRank);
			}
			if (DispelType != 0)
			{
				writer.WriteLine(indent + "DispelType: " + DispelType);
			}
			if (Mechanic != SpellMechanic.None)
			{
				writer.WriteLine(indent + "Mechanic: " + Mechanic);
			}
			if (Attributes != SpellAttributes.None)
			{
				writer.WriteLine(indent + "Attributes: " + Attributes);
			}
			if (AttributesEx != SpellAttributesEx.None)
			{
				writer.WriteLine(indent + "AttributesEx: " + AttributesEx);
			}
			if (AttributesExB != SpellAttributesExB.None)
			{
				writer.WriteLine(indent + "AttributesExB: " + AttributesExB);
			}
			if (AttributesExC != SpellAttributesExC.None)
			{
				writer.WriteLine(indent + "AttributesExC: " + AttributesExC);
			}
			if (AttributesExD != SpellAttributesExD.None)
			{
				writer.WriteLine(indent + "AttributesExD: " + AttributesExD);
			}
			if ((int)RequiredShapeshiftMask != 0)
			{
				writer.WriteLine(indent + "ShapeshiftMask: " + RequiredShapeshiftMask);
			}
			if ((int)ExcludeShapeshiftMask != 0)
			{
				writer.WriteLine(indent + "ExcludeShapeshiftMask: " + ExcludeShapeshiftMask);
			}
			if ((int)TargetFlags != 0)
			{
				writer.WriteLine(indent + "TargetType: " + TargetFlags);
			}
			if ((int)CreatureMask != 0)
			{
				writer.WriteLine(indent + "TargetUnitTypes: " + CreatureMask);
			}
			if ((int)RequiredSpellFocus != 0)
			{
				writer.WriteLine(indent + "RequiredSpellFocus: " + RequiredSpellFocus);
			}
			if (FacingFlags != 0)
			{
				writer.WriteLine(indent + "FacingFlags: " + FacingFlags);
			}
			if ((int)RequiredCasterAuraState != 0)
			{
				writer.WriteLine(indent + "RequiredCasterAuraState: " + RequiredCasterAuraState);
			}
			if ((int)RequiredTargetAuraState != 0)
			{
				writer.WriteLine(indent + "RequiredTargetAuraState: " + RequiredTargetAuraState);
			}
			if ((int)ExcludeCasterAuraState != 0)
			{
				writer.WriteLine(indent + "ExcludeCasterAuraState: " + ExcludeCasterAuraState);
			}
			if ((int)ExcludeTargetAuraState != 0)
			{
				writer.WriteLine(indent + "ExcludeTargetAuraState: " + ExcludeTargetAuraState);
			}

			if (RequiredCasterAuraId != 0)
			{
				writer.WriteLine(indent + "RequiredCasterAuraId: " + RequiredCasterAuraId);
			}
			if (RequiredTargetAuraId != 0)
			{
				writer.WriteLine(indent + "RequiredTargetAuraId: " + RequiredTargetAuraId);
			}
			if (ExcludeCasterAuraId != 0)
			{
				writer.WriteLine(indent + "ExcludeCasterAuraSpellId: " + ExcludeCasterAuraId);
			}
			if (ExcludeTargetAuraId != 0)
			{
				writer.WriteLine(indent + "ExcludeTargetAuraSpellId: " + ExcludeTargetAuraId);
			}


			if ((int)CastDelay != 0)
			{
				writer.WriteLine(indent + "StartTime: " + CastDelay);
			}
			if (CooldownTime > 0)
			{
				writer.WriteLine(indent + "CooldownTime: " + CooldownTime);
			}
			if (categoryCooldownTime > 0)
			{
				writer.WriteLine(indent + "CategoryCooldownTime: " + categoryCooldownTime);
			}

			if ((int)InterruptFlags != 0)
			{
				writer.WriteLine(indent + "InterruptFlags: " + InterruptFlags);
			}
			if ((int)AuraInterruptFlags != 0)
			{
				writer.WriteLine(indent + "AuraInterruptFlags: " + AuraInterruptFlags);
			}
			if ((int)ChannelInterruptFlags != 0)
			{
				writer.WriteLine(indent + "ChannelInterruptFlags: " + ChannelInterruptFlags);
			}
			if ((int)ProcTriggerFlags != 0)
			{
				writer.WriteLine(indent + "ProcTriggerFlags: " + ProcTriggerFlags);
			}
			if ((int)ProcChance != 0)
			{
				writer.WriteLine(indent + "ProcChance: " + ProcChance);
			}


			if (ProcCharges != 0)
			{
				writer.WriteLine(indent + "ProcCharges: " + ProcCharges);
			}
			if (MaxLevel != 0)
			{
				writer.WriteLine(indent + "MaxLevel: " + MaxLevel);
			}
			if (BaseLevel != 0)
			{
				writer.WriteLine(indent + "BaseLevel: " + BaseLevel);
			}
			if (Level != 0)
			{
				writer.WriteLine(indent + "Level: " + Level);
			}
			if (Durations.Max > 0)
			{
				writer.WriteLine(indent + "Duration: " + Durations.Min + " - " + Durations.Max + " (" + Durations.LevelDelta + ")");
			}
			if (Visual != 0u)
			{
				writer.WriteLine(indent + "Visual: " + Visual);
			}

			if ((int)PowerType != 0)
			{
				writer.WriteLine(indent + "PowerType: " + PowerType);
			}
			if (PowerCost != 0)
			{
				writer.WriteLine(indent + "PowerCost: " + PowerCost);
			}
			if (PowerCostPerlevel != 0)
			{
				writer.WriteLine(indent + "PowerCostPerlevel: " + PowerCostPerlevel);
			}
			if (PowerPerSecond != 0)
			{
				writer.WriteLine(indent + "PowerPerSecond: " + PowerPerSecond);
			}
			if (PowerPerSecondPerLevel != 0)
			{
				writer.WriteLine(indent + "PowerPerSecondPerLevel: " + PowerPerSecondPerLevel);
			}
			if (PowerCostPercentage != 0)
			{
				writer.WriteLine(indent + "PowerCostPercentage: " + PowerCostPercentage);
			}

			if (Range.MinDist != 0 || Range.MaxDist != DefaultSpellRange)
			{
				writer.WriteLine(indent + "Range: " + Range.MinDist + " - " + Range.MaxDist);
			}
			if ((int)ProjectileSpeed != 0)
			{
				writer.WriteLine(indent + "ProjectileSpeed: " + ProjectileSpeed);
			}
			if ((int)ModalNextSpell != 0)
			{
				writer.WriteLine(indent + "ModalNextSpell: " + ModalNextSpell);
			}
			if (MaxStackCount != 0)
			{
				writer.WriteLine(indent + "MaxStackCount: " + MaxStackCount);
			}

			if (RequiredTools != null)
			{
				writer.WriteLine(indent + "RequiredTools:");
				foreach (var tool in RequiredTools)
				{
					writer.WriteLine(indent + "\t" + tool);
				}
			}
			if (RequiredItemClass != ItemClass.None)
			{
				writer.WriteLine(indent + "RequiredItemClass: " + RequiredItemClass);
			}
			if ((int)RequiredItemInventorySlotMask != 0)
			{
				writer.WriteLine(indent + "RequiredItemInventorySlotMask: " + RequiredItemInventorySlotMask);
			}
			if ((int)RequiredItemSubClassMask != -1 && (int)RequiredItemSubClassMask != 0)
			{
				writer.WriteLine(indent + "RequiredItemSubClassMask: " + RequiredItemSubClassMask);
			}


			if ((int)Visual2 != 0)
			{
				writer.WriteLine(indent + "Visual2: " + Visual2);
			}
			if (Priority != 0)
			{
				writer.WriteLine(indent + "Priority: " + Priority);
			}

			if (StartRecoveryCategory != 0)
			{
				writer.WriteLine(indent + "StartRecoveryCategory: " + StartRecoveryCategory);
			}
			if (StartRecoveryTime != 0)
			{
				writer.WriteLine(indent + "StartRecoveryTime: " + StartRecoveryTime);
			}
			if (MaxTargetLevel != 0)
			{
				writer.WriteLine(indent + "MaxTargetLevel: " + MaxTargetLevel);
			}
			if ((int)SpellClassSet != 0)
			{
				writer.WriteLine(indent + "SpellClassSet: " + SpellClassSet);
			}

			if (SpellClassMask[0] != 0 || SpellClassMask[1] != 0 || SpellClassMask[2] != 0)
			{
				writer.WriteLine(indent + "SpellClassMask: {0}{1}{2}", SpellClassMask[0].ToString("X8"), SpellClassMask[1].ToString("X8"), SpellClassMask[2].ToString("X8"));
			}

			/*if ((int)FamilyFlags != 0)
			{
				writer.WriteLine(indent + "FamilyFlags: " + FamilyFlags);
			}*/
			if ((int)MaxTargets != 0)
			{
				writer.WriteLine(indent + "MaxTargets: " + MaxTargets);
			}

			if (StanceBarOrder != 0)
			{
				writer.WriteLine(indent + "StanceBarOrder: " + StanceBarOrder);
			}

			if ((int)DefenseType != 0)
			{
				writer.WriteLine(indent + "DefenseType: " + DefenseType);
			}

			if ((int)PreventionType != 0)
			{
				writer.WriteLine(indent + "PreventionType: " + PreventionType);
			}

			if (DamageMultipliers.Any(mult => mult != 1))
			{
				writer.WriteLine(indent + "DamageMultipliers: " + DamageMultipliers.ToString(", "));
			}

			for (int i = 0; i < RequiredToolCategories.Length; i++)
			{
				if (RequiredToolCategories[i] != 0)
					writer.WriteLine(indent + "RequiredTotemCategoryId[" + i + "]: " + RequiredToolCategories[i]);
			}

			if ((int)AreaGroupId != 0)
			{
				writer.WriteLine(indent + "AreaGroupId: " + AreaGroupId);
			}

			if ((int)SchoolMask != 0)
			{
				writer.WriteLine(indent + "SchoolMask: " + SchoolMask);
			}

			if (RuneCostEntry != null)
			{
				writer.WriteLine(indent + "RuneCostId: " + RuneCostEntry.Id);
				var ind = indent + "\t";
				var rcosts = new List<String>(3);
				if (RuneCostEntry.CostPerType[(int)RuneType.Blood] != 0)
					rcosts.Add(string.Format("Blood: {0}", RuneCostEntry.CostPerType[(int)RuneType.Blood]));
				if (RuneCostEntry.CostPerType[(int)RuneType.Unholy] != 0)
					rcosts.Add(string.Format("Unholy: {0}", RuneCostEntry.CostPerType[(int)RuneType.Unholy]));
				if (RuneCostEntry.CostPerType[(int)RuneType.Frost] != 0)
					rcosts.Add(string.Format("Frost: {0}", RuneCostEntry.CostPerType[(int)RuneType.Frost]));
				writer.WriteLine(ind + "Runes - {0}", rcosts.Count == 0 ? "<None>" : rcosts.ToString(", "));
				writer.WriteLine(ind + "RunicPowerGain: {0}", RuneCostEntry.RunicPowerGain);
			}
			if (MissileId != 0)
			{
				writer.WriteLine(indent + "MissileId: " + MissileId);
			}


			if (Description.Length > 0)
			{
				writer.WriteLine(indent + "Desc: " + Description);
			}

			if (Reagents.Length > 0)
			{
				writer.WriteLine(indent + "Reagents: " + Reagents.ToString(", "));
			}

			if (Ability != null)
			{
				writer.WriteLine(indent + string.Format("Skill: {0}", Ability.SkillInfo));
			}

			if (Talent != null)
			{
				writer.WriteLine(indent + string.Format("TalentTree: {0}", Talent.Tree));
			}

			writer.WriteLine();
			foreach (var effect in Effects)
			{
				effect.DumpInfo(writer, "\t\t");
			}
		}
		#endregion

		public bool IsBeneficialFor(ObjectReference casterReference, WorldObject target)
		{
			return HarmType == HarmType.Beneficial || (HarmType == HarmType.Neutral && (casterReference.Object == null || !casterReference.Object.MayAttack(target)));
		}

		public bool IsHarmfulFor(ObjectReference casterReference, WorldObject target)
		{
			return HarmType == HarmType.Harmful || (HarmType == HarmType.Neutral && casterReference.Object != null && casterReference.Object.MayAttack(target));
		}

		public override bool Equals(object obj)
		{
			return obj is Spell && ((Spell)obj).Id == Id;
		}

		public override int GetHashCode()
		{
			return (int)Id;
		}

		#region ISpellGroup
		public IEnumerator<Spell> GetEnumerator()
		{
			return new SingleEnumerator<Spell>(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region Spell Alternatives

		#endregion

		protected Spell Clone()
		{
			return (Spell)MemberwiseClone();
		}
	}
}