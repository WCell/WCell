using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Forces the wearer to only attack the caster while the Aura is applied
	/// </summary>
	public class ModTauntAuraHandler : AuraEffectHandler
	{
		private Unit caster;
		private NPC npc;

		protected internal override void CheckInitialize(CasterInfo casterInfo, Unit target, ref SpellFailedReason failReason)
		{
			caster = casterInfo.Caster as Unit;
			
			if (!(target is NPC) || caster == null)
			{
				failReason = SpellFailedReason.BadTargets;
			}
		}

		protected internal override void Apply()
		{
			npc = (NPC)m_aura.Auras.Owner;
			if (caster != null && caster.IsInWorld)
			{
				npc.ThreatCollection.Taunter = caster;
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (npc.ThreatCollection.Taunter == caster)
			{
				npc.ThreatCollection.Taunter = null;
			}
		}
	}
}