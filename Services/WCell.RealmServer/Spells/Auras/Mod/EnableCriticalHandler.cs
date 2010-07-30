using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class EnableCriticalHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				m_spellEffect.CopyAffectMaskTo(chr.PlayerSpells.CriticalStrikeEnabledMask);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				m_spellEffect.RemoveAffectMaskFrom(chr.PlayerSpells.CriticalStrikeEnabledMask);
			}
		}
	}
}
