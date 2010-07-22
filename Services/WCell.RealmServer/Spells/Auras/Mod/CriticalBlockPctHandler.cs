using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class CriticalBlockPctHandler : AttackEventEffectHandler
	{

		public override void OnBeforeAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnDefend(DamageAction action)
		{
			// if damage was blocked and we are lucky, we double the block amount
			if (action.Blocked > 0 && EffectValue > Utility.Random(1, 101))
			{
				// crit block
				action.Blocked *= 2;
			}
		}
	}
}
