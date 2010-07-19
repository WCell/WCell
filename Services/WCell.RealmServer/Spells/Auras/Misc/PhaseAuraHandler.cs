using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class PhaseAuraHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var phase = (uint) m_spellEffect.MiscValue;
			m_aura.Auras.Owner.Phase = phase;
		}

		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.Phase = 1;
		}
	}
}