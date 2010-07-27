using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.RealmServer.Handlers;
using WCell.Constants;
using WCell.Constants.Updates;
using NLog;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class CharmAuraHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			var caster = casterReference.Object as Unit;
			if (caster == null)
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				if (!(target is NPC))
				{
					LogManager.GetCurrentClassLogger().Warn("{0} tried to Charm {1} which is not an NPC, but Player charming is not yet supported.");
					failReason = SpellFailedReason.BadTargets;
				}
				if (caster.Charm != null)
				{
					failReason = SpellFailedReason.AlreadyHaveCharm;
				}
				else if (target.HasMaster)
				{
					failReason = SpellFailedReason.CantBeCharmed;
				}
				else if (caster.Level > EffectValue)
				{
					failReason = SpellFailedReason.Highlevel;
				}
				else if (caster.HasMaster)
				{
					failReason = SpellFailedReason.Charmed;
				}
				else if (caster is Character)
				{
					if (((Character)caster).ActivePet != null)
					{
						failReason = SpellFailedReason.AlreadyHaveSummon;
					}
				}
			}
		}

		protected override void Apply()
		{
			var caster = m_aura.Caster as Unit;
			if (caster == null)
			{
				return;
			}

			var target = m_aura.Auras.Owner;
			caster.Charm = target;
			target.Charmer = caster;

			var duration = m_aura.Duration;

			if (caster is Character)
			{
				((Character)caster).MakePet((NPC)target, duration);
			}
			else
			{
				caster.Enslave((NPC)target, duration);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var caster = (Unit)m_aura.Caster;
			var target = m_aura.Auras.Owner;
			caster.Charm = null;
			target.Charmer = null;
			if (caster is Character)
			{
				if (((Character)caster).ActivePet == m_aura.Auras.Owner)
				{
					((Character)caster).ActivePet = null;
				}
			}
		}
	}
}