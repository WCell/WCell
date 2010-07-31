using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageArcaneFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixMe()
		{
			// conjure water and food don't have any per level bonus
			SpellLineId.MageConjureFood.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));
			SpellLineId.MageConjureWater.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));
		}
	}
}
