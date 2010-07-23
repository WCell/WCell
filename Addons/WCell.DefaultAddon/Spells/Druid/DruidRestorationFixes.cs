using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidRestorationFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Omen of Clarity: "has a chance"
			SpellLineId.DruidRestorationOmenOfClarity.Apply(spell =>
			{
				// TODO: Fix proc chance (100 by default)
				spell.ProcChance = 5;
				spell.ProcDelay = 5000;

				// Add all "of the Druid's damage, healing spells and auto attacks" to the affect mask
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.AddToAffectMask(
					SpellLineId.DruidBalanceInsectSwarm, SpellLineId.DruidFeralCombatFeralChargeBear,
					SpellLineId.DruidRestorationSwiftmend, SpellLineId.DruidBalanceForceOfNature,
					SpellLineId.DruidFeralCombatMangleBear, SpellLineId.DruidRestorationWildGrowth,
					SpellLineId.DruidBalanceStarfall, SpellLineId.DruidBalanceTyphoon, SpellLineId.DruidWrath,
					SpellLineId.DruidHealingTouch, SpellLineId.DruidRip, SpellLineId.DruidClaw, SpellLineId.DruidRegrowth,
					SpellLineId.DruidStarfire, SpellLineId.DruidDemoralizingRoar, SpellLineId.DruidTranquility,
					SpellLineId.DruidRavage, SpellLineId.DruidSwipeBear, SpellLineId.DruidMoonfire,
					SpellLineId.DruidEntanglingRoots, SpellLineId.DruidRake, SpellLineId.DruidRejuvenation,
					SpellLineId.DruidMaul, SpellLineId.DruidPounce, SpellLineId.DruidShred, SpellLineId.DruidBash,
					SpellLineId.DruidFerociousBite, SpellLineId.DruidLacerate, SpellLineId.DruidHurricane,
					SpellLineId.DruidSwipeCat, SpellLineId.DruidNourish, SpellLineId.DruidThorns,
					SpellLineId.DruidLifebloom, SpellLineId.DruidMaim, SpellLineId.DruidMangleCat);
			});

			// Nature's Grace needs to set the correct set of affecting spells
			SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
			{
				// copy AffectMask from proc effect, which has it all set correctly
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				var triggerSpellEffect = effect.GetTriggerSpell().GetEffect(AuraType.ModCastingSpeed);
				effect.AffectMask = triggerSpellEffect.AffectMask;
			});

			// Intensity only procs for Enrage
			SpellLineId.DruidRestorationIntensity.Apply(spell =>
			{
				var proc = spell.GetEffect(AuraType.ProcTriggerSpell);
				proc.AddToAffectMask(SpellLineId.DruidEnrage);
			});
		}
	}
}
