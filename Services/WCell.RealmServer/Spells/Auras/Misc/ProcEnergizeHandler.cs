using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Energizes EffectValue on proc
	/// </summary>
	public class ProcEnergizeHandler : AuraEffectHandler
	{
		public override void OnProc(Unit triggerer, IUnitAction action)
		{
			Owner.Energize(EffectValue, m_aura.Caster, m_spellEffect);
		}
	}
}
