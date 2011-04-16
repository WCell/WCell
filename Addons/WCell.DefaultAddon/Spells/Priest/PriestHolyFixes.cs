using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Priest
{
    class PriestHolyFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixPriest()
        {
            // Holy Inspiration can be proced when priest crits with the given spells
            SpellLineId.PriestHolyInspiration.Apply(spell =>
            {
                spell.ProcTriggerFlags = ProcTriggerFlags.HealOther;

                var effect = spell.GetEffect(AuraType.ProcTriggerSpell);

                // only on crit
                effect.AuraEffectHandlerCreator = () => new ProcTriggerSpellOnCritHandler();

                // only when any of these spells are used
                effect.AddAffectingSpells(
                    SpellLineId.PriestFlashHeal,
                    SpellLineId.PriestHeal,
                    SpellLineId.PriestBindingHeal,
                    SpellLineId.PriestDisciplinePenance,
                    SpellLineId.PriestPrayerOfMending,
                    SpellLineId.PriestPrayerOfHealing,
                    SpellLineId.PriestHolyCircleOfHealing);
            });

            SpellLineId.PriestResurrection.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.ResurrectFlat);
                effect.ImplicitTargetA = ImplicitSpellTargetType.SingleFriend;
            });
        }
    }
}
