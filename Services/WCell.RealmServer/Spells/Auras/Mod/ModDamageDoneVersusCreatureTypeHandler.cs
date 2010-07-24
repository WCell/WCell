using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModDamageDoneVersusCreatureTypeHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, -EffectValue);
		}
	}
}
