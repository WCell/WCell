using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants;

namespace WCell.Addons.Default.Spells.Rogue
{
    public static class RogueSubtletyFixes
    {
        private static readonly SpellId[] EffectVanishLine = new[]
        {
            SpellId.EffectVanishRank1,
            SpellId.EffectVanishRank2,
            SpellId.EffectVanishRank3
        };

        [Initialization(InitializationPass.Second)]
        public static void FixRogue()
        {
            SpellHandler.Apply(spell =>
            {
                spell.AddTriggerSpellEffect(SpellId.ClassSkillStealth);
            }, EffectVanishLine);

            SpellLineId.RogueCloakOfShadows.Apply(spell =>
            {
                var effect = spell.GetEffect(SpellEffectType.TriggerSpell);
                effect.SpellEffectHandlerCreator = (cast, eff) => new CloakOfShadowsHandler(cast, eff);
            });

            SpellLineId.RogueSubtletyPreparation.Apply(Spell =>
            {
                var effect = Spell.GetEffect(SpellEffectType.Dummy);
                effect.SpellEffectHandlerCreator = (cast, eff) => new PreparationHandler(cast, eff);
            });
        }
    }

    class CloakOfShadowsHandler : SpellEffectHandler
    {
        public CloakOfShadowsHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

        public override void Apply()
        {
            var chr = m_cast.CasterChar;

            if(chr != null)
            {
                chr.Auras.RemoveWhere(aura => aura.Spell.HasHarmfulEffects);
            }
        }
    }

    class PreparationHandler : SpellEffectHandler
    {
        private SpellLineId[] spellsWithoutGlyph = new[]
        {
            SpellLineId.RogueVanish,
            SpellLineId.RogueEvasion,
            SpellLineId.RogueSprint,
            SpellLineId.RogueAssassinationColdBlood,
            SpellLineId.RogueSubtletyShadowstep
        };

        private SpellLineId[] spellsWithGlyph = new[]
        {
            SpellLineId.RogueCombatBladeFlurry,
            SpellLineId.RogueDismantle,
            SpellLineId.RogueKick
        };

        public PreparationHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
		{
		}

        protected override void Apply(RealmServer.Entities.WorldObject target)
        {
            var chr = target as Character;
            if(chr != null)
            {
                foreach(var line in spellsWithoutGlyph)
                {
                    line.Apply(spell =>
                    {
                        if(chr.Spells.Contains(spell.Id))
                        { 
                            chr.Spells.ClearCooldown(spell);
                        }
                    });
                }
                if (chr.Spells.Contains(SpellId.GlyphOfPreparation) || chr.Spells.Contains(SpellId.GlyphOfPreparation_2))
                {
                    foreach (var line in spellsWithGlyph)
                    {
                        line.Apply(spell =>
                        {
                            if (chr.Spells.Contains(spell.Id))
                            {
                                chr.Spells.ClearCooldown(spell);
                            }
                        });
                    }
                }
            }
        }
    }
}