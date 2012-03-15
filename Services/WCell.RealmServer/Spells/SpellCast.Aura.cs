using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Effects;

namespace WCell.RealmServer.Spells
{
    public partial class SpellCast
    {
        /// <summary>
        /// Checks whether the given target resisted the debuff, represented through the given spell
        /// </summary>
        public static CastMissReason CheckDebuffResist(Unit target, Spell spell, int casterLevel, bool hostile)
        {
            var missReason = CastMissReason.None;
            if (hostile && target.CheckDebuffResist(casterLevel, target.GetLeastResistantSchool(spell)))
            {
                missReason = CastMissReason.Resist;
            }
            return missReason;
        }

        private SpellFailedReason PrepAuras()
        {
            // create Auras
            //m_auraApplicationInfos = AuraAppListPool.Obtain();
            m_auraApplicationInfos = new List<AuraApplicationInfo>(4);

            // check stacking
            SpellEffectHandler lastHandler = null;
            for (var i = 0; i < Handlers.Length; i++)
            {
                var spellHandler = Handlers[i];
                if (spellHandler.Effect.IsAuraEffect)
                {
                    if (lastHandler != null && lastHandler.Effect.SharesTargetsWith(spellHandler.Effect, IsAICast))
                    {
                        // same aura
                        continue;
                    }
                    lastHandler = spellHandler;

                    if (spellHandler.m_targets != null)
                    {
                        foreach (var target in spellHandler.m_targets)
                        {
                            if (target is Unit)
                            {
                                if (m_auraApplicationInfos.Any(info => info.Target == target))
                                {
                                    // target was already added
                                    continue;
                                }

                                var id = Spell.GetAuraUID(CasterReference, target);
                                var failReason = SpellFailedReason.Ok;
                                if (((Unit)target).Auras.PrepareStackOrOverride(CasterReference, id, Spell, ref failReason, this))
                                {
                                    m_auraApplicationInfos.Add(new AuraApplicationInfo((Unit)target));
                                }
                                else if (failReason != SpellFailedReason.Ok && !IsAoE)
                                {
                                    // spell fails
                                    // m_auraApplicationInfos.Clear();
                                    // AuraAppListPool.Recycle(m_auraApplicationInfos);
                                    return failReason;
                                }
                            }
                        }
                    }
                }
            }
            return SpellFailedReason.Ok;
        }

        private void CreateAuras(ref List<MissedTarget> missedTargets, ref List<IAura> auras, DynamicObject dynObj)
        {
            auras = AuraListPool.Obtain();

            var allowDead = Spell.PersistsThroughDeath;

            // create AreaAura
            if (Spell.IsAreaAura)
            {
                if (dynObj != null || (CasterObject != null && (allowDead || !(CasterObject is Unit) || ((Unit)CasterObject).IsAlive)))
                {
                    // AreaAura is created at the target location if it is a DynamicObject, else its applied to the caster
                    var aaura = new AreaAura(dynObj ?? CasterObject, Spell);
                    if (dynObj != null)
                    {
                        // also start the area aura
                        auras.Add(aaura);
                    }
                    // else: Is coupled to an Aura instance
                }
                else
                {
                    LogManager.GetCurrentClassLogger().Warn(
                        "Tried to cast Spell {0} with invalid dynObj or Caster - dynObj: {1}, CasterObject: {2}, CasterUnit: {3}",
                        Spell, dynObj, CasterObject, CasterUnit);
                }
            }

            // remove missed targets
            for (var i = m_auraApplicationInfos.Count - 1; i >= 0; i--)
            {
                var app = m_auraApplicationInfos[i];
                if (!Targets.Contains(app.Target))
                {
                    m_auraApplicationInfos.RemoveAt(i);
                }
            }
            if (m_auraApplicationInfos.Count == 0)
            {
                return;
            }

            // create Aura-Handlers
            for (var i = 0; i < Handlers.Length; i++)
            {
                var spellHandler = Handlers[i];
                if (spellHandler is ApplyAuraEffectHandler)
                {
                    ((ApplyAuraEffectHandler)spellHandler).AddAuraHandlers(m_auraApplicationInfos);
                }
            }
            if (missedTargets == null)
            {
                missedTargets = CastMissListPool.Obtain();
            }

            // apply all new Auras
            for (var i = 0; i < m_auraApplicationInfos.Count; i++)
            {
                var info = m_auraApplicationInfos[i];
                var target = info.Target;

                if (!target.IsInContext)
                {
                    continue;
                }

                if (info.Handlers == null || (!allowDead && !target.IsAlive))
                {
                    continue;
                }

                // check for immunities and resistances
                CastMissReason missReason;
                var hostile = Spell.IsHarmfulFor(CasterReference, target);

                if (!IsPassive && !Spell.IsPreventionDebuff &&
                    (missReason = CheckDebuffResist(target, Spell, CasterReference.Level, hostile)) != CastMissReason.None)
                {
                    // debuff miss
                    missedTargets.Add(new MissedTarget(target, missReason));
                }
                else
                {
                    // create aura
                    var newAura = target.Auras.CreateAura(CasterReference, Spell, info.Handlers, TargetItem, !Spell.IsPreventionDebuff && !hostile);
                    if (newAura != null)
                    {
                        // check for debuff and if the spell causes no threat we aren't put in combat
                        if (!Spell.IsPreventionDebuff && !((Spell.AttributesExC & SpellAttributesExC.NoInitialAggro) != 0) && hostile && target.IsInWorld && target.IsAlive)
                        {
                            // force combat mode
                            target.IsInCombat = true;
                            if (target is NPC && CasterUnit != null)
                            {
                                ((NPC)target).ThreatCollection.AddNewIfNotExisted(CasterUnit);
                            }
                        }
                        // add Aura now
                        auras.Add(newAura);
                    }
                }
            }

            //m_auraApplicationInfos.Clear();
            //AuraAppListPool.Recycle(m_auraApplicationInfos);
            m_auraApplicationInfos = null;
        }
    }
}