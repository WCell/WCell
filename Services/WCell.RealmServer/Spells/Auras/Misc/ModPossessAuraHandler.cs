using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.RealmServer.Handlers;
using WCell.Constants;
using WCell.Constants.Updates;
using NLog;

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
                if (target is NPC)
                {
                    chr.MakePet((NPC) target, duration);
                }
                chr.FarSight = target.EntityId;
                chr.SetMover(target, true);
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
                chr.SetMover(null, true);
                chr.FarSight = EntityId.Zero;
                if (chr.ActivePet == m_aura.Auras.Owner)
                {
                    chr.ActivePet = null;
                }
            }
        }
    }
}
