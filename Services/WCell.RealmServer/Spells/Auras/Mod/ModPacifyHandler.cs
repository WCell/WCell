using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Prevents carrier from attacking or using "physical abilities"
	/// </summary>
	public class ModPacifyHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Fleeing);
		}

		protected internal override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Fleeing);
		}
	}
}
