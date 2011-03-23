using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Logging;
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

		public ToggleAuraHandler()
		{
		}

		public ToggleAuraHandler(SpellId auraId)
		{
			ToggleAuraSpell = SpellHandler.Get(auraId);
		}

		protected override void Apply()
		{
			// add aura
			// first check, if Aura already exists (eg. because it was loaded from DB)
			if (ToggleAuraSpell == null)
			{
				ToggleAuraSpell = m_spellEffect.TriggerSpell;
			}

			activeToggleAura = Owner.Auras[ToggleAuraSpell];
			if (activeToggleAura == null)
			{
				activeToggleAura = Owner.Auras.CreateAndStartAura(m_aura.CasterReference, ToggleAuraSpell, true);
				activeToggleAura.CanBeSaved = false;
			}
			else
			{
				LogManager.GetCurrentClassLogger().Warn("Tried to toggle on already created Aura \"{0}\" on {1}", activeToggleAura, Owner);
				activeToggleAura.IsActivated = true;
			}
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
