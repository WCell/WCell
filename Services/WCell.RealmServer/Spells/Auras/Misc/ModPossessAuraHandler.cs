using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Pets;
using WCell.Core;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.RealmServer.Handlers;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Util.Logging;

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
                    chr.Enslave((NPC) target, duration);
                    target.Brain.State = BrainState.Idle;
                    PetHandler.SendSpells(chr, (NPC)target, PetAction.Stay);
                    chr.SetMover(target, true);
                    target.UnitFlags |= UnitFlags.Possessed;
                }
                else if(target is Character)
                {
                    PetHandler.SendPlayerPossessedPetSpells(chr, (Character)target);
                    chr.SetMover(target, true);
                }
                
                chr.FarSight = target.EntityId;
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
                chr.SetMover(chr, true);
                chr.ResetMover();
                chr.FarSight = EntityId.Zero;
                PetHandler.SendEmptySpells(chr);
                target.UnitFlags &= ~UnitFlags.Possessed;
                if (target is NPC)
                {
                    target.Brain.EnterDefaultState();
                    ((NPC) target).RemainingDecayDelayMillis = 1;
                }
            }
        }
    }
}
