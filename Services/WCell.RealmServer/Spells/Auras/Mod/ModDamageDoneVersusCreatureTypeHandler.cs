using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModDamageDoneVersusCreatureTypeHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, EffectValue);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, -EffectValue);
		}
	}
}
