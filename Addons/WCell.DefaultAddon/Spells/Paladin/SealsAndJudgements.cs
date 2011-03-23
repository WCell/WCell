using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Logging;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells;
using WCell.Constants.Updates;
using WCell.Constants;
using WCell.Util;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class SealsAndJudgements
	{
		public static uint SealAuraId, JudgementAuraId;

		public static readonly SpellLineId[] AllSeals = new[] {
				SpellLineId.PaladinSealOfJustice,
				SpellLineId.PaladinSealOfLight,
				SpellLineId.PaladinSealOfRighteousness,
				SpellLineId.PaladinSealOfVengeance,
				SpellLineId.PaladinSealOfWisdom,
				SpellLineId.PaladinRetributionSealOfCommand
			};

		public static readonly SpellLineId[] AllJudgements = new[] {
				SpellLineId.PaladinJudgementOfJustice,
				SpellLineId.PaladinJudgementOfLight,
				SpellLineId.PaladinJudgementOfWisdom
			};

		public static readonly Dictionary<SpellLineId, Func<SpellCast, Unit, int>> SealDamageCalculators = new Dictionary<SpellLineId, Func<SpellCast, Unit, int>>();

		[Initialization(InitializationPass.Second)]
		public static void FixSealsAndJudgements()
		{
			CreateSealCalculators();

			// Only one Judgement per Paladin can be active at any one time
			AuraHandler.AddAuraCasterGroup(AllJudgements);

			// only one seal can be active at a time
			SealAuraId = AuraHandler.AddAuraGroup(AllSeals);

			// applying of seals allows the carrier to use judgements
			AllowJudgementEffectHandler.AddTo(AllSeals);

			// Judgements trigger this spell to "unleash" the seal
			SpellHandler.Apply(spell =>
			{
				spell.AddTargetTriggerSpells(SpellId.Judgement_5);
				spell.AttributesExB &= ~SpellAttributesExB.RequiresBehindTarget;
			}, AllJudgements);

			// these 2 seals' 3rd effect should trigger the debuff but have wrong settings
			SpellHandler.Apply(spell =>
			{
				var triggerEffect = spell.Effects[2];
				triggerEffect.AuraType = AuraType.ProcTriggerSpell;
				triggerEffect.TriggerSpellId = (SpellId)triggerEffect.BasePoints;
			},
			SpellLineId.PaladinSealOfVengeance, SpellLineId.PaladinSealOfRighteousness);

			/*
			 * Most Judgements' ProcTriggerSpells need to be changed to use customized values, depending on spellpower, weapon damage etc...

					- SpellLineId.PaladinSealOfJustice
						-> Should work
					- SpellLineId.PaladinSealOfWisdom
						-> Should work
					
					- SpellLineId.PaladinSealOfRighteousness:
						-> ClassSkillJudgementOfRighteousnessRank1: Dmg + ${$cond($eq($HND,1),0.85*($m1*1.2*1.03*$MWS/100)+0.03*($MW+$mw)/2-1,1.2*($m1*1.2*1.03*$MWS/100)+0.03*($MW+$mw)/2+1)}
					
					- SpellLineId.PaladinSealOfLight:
						-> EffectSealOfLight heals for ${0.15*$AP+0.15*$SPH}
						
					- SpellLineId.PaladinSealOfVengeance:
						-> ClassSkillJudgementOfVengeanceRank1: 
							- Dmg + ${(0.013*$SPH+0.025*$AP)*6} + 10% for each application of Blood Corruption on the target
							- Once stacked to $31803u times, each of the Paladin's attacks also deals $42463s1% weapon damage as additional Holy damage.
							
					- SpellLineId.PaladinRetributionSealOfCommand
						-> All melee attacks deal ${0.36*$mw} to ${0.36*$MW} additional Holy damage.  When used with attacks or abilities that strike a single target, this additional Holy damage will strike up to 2 additional targets.
			 */
		}

		private static void CreateSealCalculators()
		{
			SealDamageCalculators[SpellLineId.PaladinSealOfJustice] = CalcJusticeDamage;
			SealDamageCalculators[SpellLineId.PaladinSealOfLight] = CalcLightDamage;
			SealDamageCalculators[SpellLineId.PaladinSealOfRighteousness] = CalcRighteousnessDamage;
			SealDamageCalculators[SpellLineId.PaladinSealOfVengeance] = CalcVengeanceDamage;
			SealDamageCalculators[SpellLineId.PaladinSealOfWisdom] = CalcWisdomDamage;
			SealDamageCalculators[SpellLineId.PaladinRetributionSealOfCommand] = CalcCommandDamage;
		}

		/// <summary>
		/// Justice: ${1+0.25*$SPH+0.16*$AP}
		/// </summary>
		private static int CalcJusticeDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			return 1 + (caster.GetDamageDoneMod(DamageSchool.Holy) >> 2) + (caster.TotalMeleeAP / 6);
		}

		/// <summary>
		/// Light: ${1+0.25*$SPH+0.16*$AP}
		/// </summary>
		private static int CalcLightDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			return 1 + (caster.GetDamageDoneMod(DamageSchool.Holy) >> 2) + (caster.TotalMeleeAP / 6);
		}

		/// <summary>
		/// Righteousness: ${1+0.2*$AP+0.32*$SPH}
		/// </summary>
		private static int CalcRighteousnessDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			return 1 + (caster.GetDamageDoneMod(DamageSchool.Holy) / 3) + (caster.TotalMeleeAP / 5);
		}

		/// <summary>
		/// Vengeance: ${1+0.22*$SPH+0.14*$AP} + 10% for each application
		/// </summary>
		private static int CalcVengeanceDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			var val = 1 + (int)((0.22f * caster.GetDamageDoneMod(DamageSchool.Holy)) + (caster.TotalMeleeAP / 0.14f));
			var aura = caster.Auras[SpellId.ClassSkillHolyVengeanceRank1, false];
			if (aura != null)
			{
				val = (int)(val * (1f + (aura.StackCount / 10f)));
			}
			return val;
		}

		/// <summary>
		/// Wisdom: ${1+0.25*$SPH+0.16*$AP}
		/// </summary>
		private static int CalcWisdomDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			return 1 + (caster.GetDamageDoneMod(DamageSchool.Holy) >> 2) + (caster.TotalMeleeAP / 6);
		}

		/// <summary>
		/// Command: ${0.19*$mw+0.08*$AP+0.13*$SPH} to ${0.19*$MW+0.08*$AP+0.13*$SPH}
		/// </summary>
		private static int CalcCommandDamage(SpellCast cast, Unit target)
		{
			var caster = cast.CasterChar;
			var value = 0.19f * Utility.Random(target.MinDamage, target.MaxDamage + 1);
			value += (0.13f * caster.GetDamageDoneMod(DamageSchool.Holy)) + (0.08f * caster.TotalMeleeAP);
			return (int)value;
		}
	}

	/// <summary>
	/// Allows judgement spells to be casted
	/// </summary>
	public class AllowJudgementEffectHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Holder.AuraState |= AuraStateMask.Judgement;
		}

		protected override void Remove(bool cancelled)
		{
			Holder.AuraState &= ~AuraStateMask.Judgement;
		}


		/// <summary>
		/// Adds this handler to all Spells in the given lines.
		/// </summary>
		public static void AddTo(params SpellLineId[] lineIds)
		{
			foreach (var lineId in lineIds)
			{
				lineId.Apply(spell =>
				{
					spell.AddAuraEffect(() => new AllowJudgementEffectHandler());
				});
			}
		}
	}

	/// <summary>
	/// Every judgement effect applies damage, depending on the active Seal
	/// </summary>
	public class ApplyJudgementEffectHandler : SpellEffectHandler
	{
		Func<SpellCast, Unit, int> calc;

		public ApplyJudgementEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			var seal = m_cast.CasterUnit.Auras[new AuraIndexId(SealsAndJudgements.SealAuraId, true)];
			if (seal == null)
			{
				failReason = SpellFailedReason.CasterAurastate;
			}
			else
			{
				if (!SealsAndJudgements.SealDamageCalculators.TryGetValue(seal.Spell.Line.LineId, out calc))
				{
					LogManager.GetCurrentClassLogger().Warn("Seal {0} has no Damage Calculator.", seal.Spell);
				}
			}
		}

		protected override void Apply(WorldObject target)
		{
			if (calc != null)
			{
				var dmg = calc(m_cast, (Unit)target);
				((Unit)target).DealSpellDamage(m_cast.CasterUnit, Effect, dmg);
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Player; }
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}