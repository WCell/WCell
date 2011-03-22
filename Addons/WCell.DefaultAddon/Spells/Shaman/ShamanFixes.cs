using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Spells.Shaman
{
	public static class ShamanFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixShaman()
		{
			// All elemental shields are mutually exclusive on one target:
			AuraHandler.AddAuraGroup(SpellLineId.ShamanLightningShield, SpellLineId.ShamanRestorationEarthShield, SpellLineId.ShamanWaterShield);

			// Lightning shield spells are missing the proc part
			SpellLineId.ShamanLightningShield.Apply(spell => spell.ProcChance = 100);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank1, SpellId.ClassSkillLightningShieldRank1_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank2, SpellId.ClassSkillLightningShieldRank2_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank3, SpellId.ClassSkillLightningShieldRank3_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank4, SpellId.ClassSkillLightningShieldRank4_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank5, SpellId.ClassSkillLightningShieldRank5_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank6, SpellId.ClassSkillLightningShieldRank6_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank7, SpellId.ClassSkillLightningShieldRank7_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank8, SpellId.ClassSkillLightningShieldRank8_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank9, SpellId.ClassSkillLightningShieldRank9_2);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank10_2, SpellId.ClassSkillLightningShieldRank10);
			AddProcTrigger(SpellId.ClassSkillLightningShieldRank11_2, SpellId.ClassSkillLightningShieldRank11);

			//if you target is affected by Flameshock, this spell will crit
			SpellLineId.ShamanLavaBurst.Apply(spell => 
				{
					var eff = spell.GetEffect(SpellEffectType.SchoolDamage);
					eff.SpellEffectHandlerCreator = (cast, effect) => new LavaBursthandler(cast, effect);
				});

            SpellLineId.ShamanAncestralSpirit.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.ResurrectFlat);
                effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
            });

            // fixes bloodlust being applied to every party/raid member but yourself
            SpellHandler.Apply(spell =>
            {
                var modMeleeHaste = spell.GetEffect(AuraType.ModMeleeHastePercent);
                modMeleeHaste.ImplicitTargetB = ImplicitSpellTargetType.AllPartyAroundCaster;

                var modScale = spell.GetEffect(AuraType.ModScale);
                modScale.ImplicitTargetB = ImplicitSpellTargetType.AllPartyAroundCaster;

                var modCastingSpeed = spell.GetEffect(AuraType.ModCastingSpeed);
                modCastingSpeed.ImplicitTargetA = ImplicitSpellTargetType.AllPartyAroundCaster;
            }, SpellLineId.ShamanBloodlust, SpellLineId.ShamanHeroism);
		}

		static void AddProcTrigger(SpellId id, SpellId triggerId)
		{
			SpellHandler.Apply(spell =>
			{
				var effect = spell.AddAuraEffect(AuraType.ProcTriggerSpell, ImplicitSpellTargetType.Self);
				effect.TriggerSpellId = triggerId;
			},id);
		}
		#region LavaBurst
		public class LavaBursthandler : SpellEffectHandler
		{
			public LavaBursthandler(SpellCast cast, SpellEffect effect)
				: base(cast, effect)
			{
			}

			protected override void Apply(WorldObject target)
			{
				var unit = (Unit)target;
				if (unit != null)
				{
					if (unit.Auras[SpellLineId.ShamanFlameShock] != null)
						unit.DealSpellDamage(m_cast.CasterUnit, Effect, CalcEffectValue(), true, true, true);
					else
						unit.DealSpellDamage(m_cast.CasterUnit, Effect, CalcEffectValue());
				}
			}
		}
		#endregion


	}
}