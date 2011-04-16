using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras
{
	public class DamagePctAmplifierHandler : AttackEventEffectHandler
	{
		public override void OnBeforeAttack(DamageAction action)
		{
		}

		public override void OnAttack(DamageAction action)
		{
		}

		public override void OnDefend(DamageAction action)
		{
			if (m_spellEffect.Spell.SchoolMask.HasAnyFlag(action.UsedSchool))
			{
				action.ModDamagePercent(EffectValue);
			}
		}
	}
}