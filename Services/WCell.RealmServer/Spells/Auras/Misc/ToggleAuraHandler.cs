using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Toggles an aura while active
	/// </summary>
	public class ToggleAuraHandler : AuraEffectHandler
	{
		public Spell ToggleAuraSpell { get; set; }
		private Aura activeToggleAura;

		public ToggleAuraHandler(SpellId auraId)
		{
			ToggleAuraSpell = SpellHandler.Get(auraId);
		}

		protected internal override void Apply()
		{
			// add aura
			activeToggleAura = Owner.Auras.AddAura(m_aura.CasterInfo, ToggleAuraSpell, true);
		}

		protected internal override void Remove(bool cancelled)
		{
			if (activeToggleAura != null)
			{
				// remove aura
				activeToggleAura.Cancel();
				activeToggleAura = null;
			}
		}
	}
}
