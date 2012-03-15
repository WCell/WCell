using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class ModPossessAuraHandler : AuraEffectHandler
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
                if (caster.Charm != null)
                {
                    failReason = SpellFailedReason.AlreadyHaveCharm;
                }
                else if (target.HasMaster)
                {
                    failReason = SpellFailedReason.CantDoThatRightNow;
                }
                else if (caster.HasMaster)
                {
                    failReason = SpellFailedReason.Possessed;
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

            var chr = caster as Character;
            if (chr != null)
            {
                chr.Possess(duration, target);
            }
        }

        protected override void Remove(bool cancelled)
        {
            var caster = (Unit)m_aura.CasterUnit;
            var target = m_aura.Auras.Owner;
            caster.Charm = null;
            target.Charmer = null;
            var chr = caster as Character;
            if (chr != null)
            {
                chr.UnPossess(target);
            }
        }
    }
}
