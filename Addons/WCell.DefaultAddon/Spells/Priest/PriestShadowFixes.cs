using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Priest
{
    class PriestShadowFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixPriest()
        {
            /*
            // only proc on kill that rewards xp or honor
            SpellLineId.PriestShadowMagicSpiritTap.Apply(spell => spell.ProcTriggerFlags = ProcTriggerFlags.GainExperience);

            // Shadow Weaving applies to caster and can also be proc'ed by Mind Flay
            SpellLineId.PriestShadowMagicShadowWeaving.Apply(spell =>
            {
                var effect = spell.GetEffect(AuraType.AddTargetTrigger);
                effect.ImplicitTargetA = ImplicitSpellTargetType.Self;
                effect.AddAffectingSpells(SpellLineId.PriestShadowMagicMindFlay);
            });

            // Mind Flay: Assault the target's mind with Shadow energy, causing ${$m3*3} Shadow damage over $d and slowing their movement speed by $s2%.
			SpellLineId.PriestShadowMagicMindFlay.Apply(spell =>
            {
                var effect = spell.AddAuraEffect(AuraType.PeriodicDamage, ImplicitSpellTargetType.SingleEnemy);
                effect.Amplitude = spell.Effects[2].Amplitude;
            });

            // Dispersion also regenerates Mana
            SpellLineId.PriestShadowMagicDispersion.Apply(spell =>
            {
                var effect = spell.AddPeriodicTriggerSpellEffect(SpellId.Dispersion_2, ImplicitSpellTargetType.Self);
                effect.Amplitude = 1000;
            });

            // Vampiric Embrace can be proc'ed by a certain set of spells, and has a custom healing AuraEffectHandler
			SpellLineId.PriestShadowMagicVampiricEmbrace.Apply(spell =>
            {
                // only proc on damaging SpellCast
                spell.ProcTriggerFlags = ProcTriggerFlags.SpellCast;

                // change Dummy to proc effect
                var effect = spell.Effects[0];
                effect.IsProc = true;
                effect.AuraEffectHandlerCreator = () => new AuraVampiricEmbracerHandler();

                // Set correct flags and set of spells to trigger the proc
                effect.AddAffectingSpells(
					SpellLineId.PriestShadowMagicMindFlay,
                    SpellLineId.PriestShadowWordPain,
                    SpellLineId.PriestShadowWordDeath,
                    SpellLineId.PriestMindBlast,
                    SpellLineId.PriestManaBurn,
                    SpellLineId.PriestDevouringPlague,
					SpellLineId.PriestShadowMagicVampiricTouch,
                    SpellLineId.PriestMindSear);
            });
             */
        }
              
    }

    #region AuraVampiricEmbracerHandler
    public class AuraVampiricEmbracerHandler : AuraEffectHandler
    {
        public override void OnProc(Unit triggerer, IUnitAction action)
        {
            if (action is IDamageAction)
            {
                var owner = Owner;
                var dmgAction = ((IDamageAction)action);

                var healSelfAmount = ((dmgAction.Damage * EffectValue) + 50) / 100;	// don't forget rounding
                var healPartyAmount = (healSelfAmount + 3) / 5; // don't forget rounding

                owner.Heal(healSelfAmount, owner, SpellEffect);
                if (owner is Character)
                {
                    var chr = (Character)owner;
                    var group = chr.Group;
                    if (group != null)
                    {
                        // heal all group members in same context (ie same Map in current implementation)
                        group.CallOnAllInSameContext(chr.ContextHandler, (member) =>
                        {
                            member.Heal(healPartyAmount, owner, SpellEffect);
                        });
                    }
                }
            }
        }
    }
    #endregion
}
