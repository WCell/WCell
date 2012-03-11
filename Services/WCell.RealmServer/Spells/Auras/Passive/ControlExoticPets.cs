using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Passive
{
    public class ControlExoticPetsHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
            if (!(target is Character)) return;
            var chr = (Character)target;
            if (chr.Class != ClassId.Hunter)
            {
                failReason = SpellFailedReason.BadTargets;
            }
        }

        protected override void Apply()
        {
            var chr = m_aura.Auras.Owner as Character;
            if (chr != null)
            {
                chr.CanControlExoticPets = true;
            }
        }

        protected override void Remove(bool cancelled)
        {
            var chr = m_aura.Auras.Owner as Character;
            if (chr != null)
            {
                chr.CanControlExoticPets = false;
            }
        }
    }
}