using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
    public static class HunterMarkmanship
    {
        [Initialization(InitializationPass.Second)]
        public static void FixMarkmanshipSpells()
        {
            // Volley does incorrect damage ($RAP*0.083700 + $m1)
            SpellHandler.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.08370f;

                }, SpellId.EffectClassSkillVolleyRank1, SpellId.EffectClassSkillVolleyRank2,
                          SpellId.EffectClassSkillVolleyRank3, SpellId.EffectClassSkillVolleyRank4,
                          SpellId.EffectClassSkillVolleyRank5, SpellId.EffectClassSkillVolleyRank6
                          );

            // Arcane Shot does incorrect damage ($RAP*0.15 + $m1)
            SpellLineId.HunterArcaneShot.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.15f;
                });

            // Multi-Shot affects three targets
            SpellLineId.HunterMultiShot.Apply(spell =>
                {
                    spell.Effects[0].ImplicitTargetA = ImplicitTargetType.Chain;
                });

            // Readiness clears cooldown of every spells of the hunter except Beast Mastery
            SpellLineId.HunterMarksmanshipReadiness.Apply(spell =>
                {
                    spell.Effects[0].SpellEffectHandlerCreator =
                        (cast, effect) => new ReadinessHandler(cast, effect);
                });
        }
    }

    #region Readiness
    public class ReadinessHandler : DummyEffectHandler
    {
        public ReadinessHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var charSpells = ((Unit)target).Spells;
            foreach (Spell spell in charSpells)
            {
                if (spell.SpellLine.LineId != SpellLineId.HunterBeastMasteryBestialWrath && spell.SpellClassSet == SpellClassSet.Hunter)
                {
                    SpellHandler.SendClearCoolDown((Character)target, spell.SpellId);
                }
            }
        }
    }
    #endregion
}
