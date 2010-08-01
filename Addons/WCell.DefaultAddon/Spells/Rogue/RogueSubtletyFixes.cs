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
        }
    }
}