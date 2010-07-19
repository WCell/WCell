using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModSpellDamageByPercentOfStatHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var stat = (StatType)SpellEffect.MiscValueB;

			// TODO: Update when stat changes
			if (m_aura.Auras.Owner is Character)
			{
				//((Character)m_aura.Auras.Owner).ModDamageBonusPct(m_spellEffect.MiscBitSet, EffectValue);
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (m_aura.Auras.Owner is Character)
			{
				//((Character)m_aura.Auras.Owner).RemoveDamageMod(m_spellEffect.MiscBitSet, EffectValue);
			}
		}
	}
}