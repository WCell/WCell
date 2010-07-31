using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// TODO: Replaces a spell with an alternate version of the spell that works differently
	/// </summary>
	public class ReplaceSpellHandler : AuraEffectHandler
	{
		public Spell SpellAlternateVersion { get; set; }

		public ReplaceSpellHandler(Spell spellAlternateVersion)
		{
			SpellAlternateVersion = spellAlternateVersion;
		}

		protected override void Apply()
		{
			
		}

		protected override void Remove(bool cancelled)
		{
			
		}
	}
}
