using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
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
}