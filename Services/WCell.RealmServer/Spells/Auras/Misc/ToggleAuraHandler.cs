using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Applies another aura while active and removes it when turning inactive
	/// </summary>
	public class ToggleAuraHandler : AuraEffectHandler
	{
		public Spell ToggleAuraSpell { get; set; }
		private Aura activeToggleAura;

		public ToggleAuraHandler(SpellId auraId)
		{
			ToggleAuraSpell = SpellHandler.Get(auraId);
		}

		protected override void Apply()
		{
			// add aura
			activeToggleAura = Owner.Auras.CreateAura(m_aura.CasterReference, ToggleAuraSpell, true);
		}

		protected override void Remove(bool cancelled)
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
