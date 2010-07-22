using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Druid
{
	public static class DruidBalanceFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Nature's Grace needs to set the correct set of affecting spells
			SpellLineId.DruidBalanceNaturesGrace.Apply(spell =>
			{
				// copy AffectMask from proc effect, which has it all set correctly
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				var triggerSpellEffect = effect.TriggerSpell.GetEffect(AuraType.ModCastingSpeed);
				effect.AffectMask = triggerSpellEffect.AffectMask;
			});
		}
	}
}
