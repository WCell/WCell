using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class CharmAuraHandler : AuraEffectHandler
	{
		protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterRef, Unit target, ref SpellFailedReason failReason)
		{
			var caster = creatingCast.CasterReference.Object as Unit;
			if (caster == null)
			{
				failReason = SpellFailedReason.BadTargets;
			}
			else
			{
				if (!(target is NPC))
				{
					LogManager.GetCurrentClassLogger().Warn("{0} tried to Charm {1} which is not an NPC, but Player charming is not yet supported.", caster, target);
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
				//else if (caster.Level < EffectValue)
				//{
				//    failReason = SpellFailedReason.Highlevel;
				//}
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
			var caster = m_aura.CasterUnit;
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
			var caster = (Unit)m_aura.CasterUnit;
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