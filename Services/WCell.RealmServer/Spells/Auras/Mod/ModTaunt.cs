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
		protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterRef, Unit target, ref SpellFailedReason failReason)
		{
			if (!(target is NPC))
			{
				failReason = SpellFailedReason.BadTargets;
			}

			if (casterRef != null && casterRef.Object is Unit)
			{
				var caster = (Unit)casterRef.Object;
				//if (target.Target == caster)
				//{
				//    failReason = SpellFailedReason.NoValidTargets;
				//}
				//else
				{
					var spell = m_spellEffect.Spell;
					var hasSingleFriendTarget = spell.HasBeneficialEffects && !spell.IsAreaSpell && spell.HasTargets;
					if (hasSingleFriendTarget && caster.Target != null && caster.IsFriendlyWith(caster.Target))
					{
						// taunting a friend, means we want to taunt his attackers
						// needed for Righteous defense, amongst others
						if (target.Target != caster.Target)
						{
							failReason = SpellFailedReason.NoValidTargets;
						}
					}
				}
			}
		}

		protected override void Apply()
		{
			var npc = (NPC)Owner;
			var caster = m_aura.Caster as Unit;
			if (caster != null)
			{
				npc.ThreatCollection.Taunter = caster;
			}
		}

		protected override void Remove(bool cancelled)
		{
			var npc = (NPC)Owner;
			if (npc.ThreatCollection.Taunter == m_aura.Caster)
			{
				npc.ThreatCollection.Taunter = null;
			}
		}
	}
}