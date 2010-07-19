using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

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
		}

		static void AddProcTrigger(SpellId id, SpellId triggerId)
		{
			SpellHandler.Apply(spell =>
			{
				var effect = spell.AddAuraEffect(AuraType.ProcTriggerSpell, ImplicitTargetType.Self);
				effect.TriggerSpellId = triggerId;
			},id);
		}
	}
}