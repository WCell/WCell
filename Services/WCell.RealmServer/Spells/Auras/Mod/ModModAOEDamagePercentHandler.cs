using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModModAOEDamagePercentHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.AoEDamageModifierPct += EffectValue;
		}

		protected override void Remove(bool cancelled)
		{
			Owner.AoEDamageModifierPct -= EffectValue;
		}
	}
}
