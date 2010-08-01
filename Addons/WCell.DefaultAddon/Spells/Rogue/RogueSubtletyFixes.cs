using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

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
                var effect = spell.GetEffect(AuraType.ModStealth);
                effect.AuraEffectHandlerCreator = () => new RogueVanishHandler();
            }, EffectVanishLine);
        }
    }
    
    class RogueVanishHandler : AuraEffectHandler
    {
        protected override void Remove(bool cancelled)
        {
            //hacky fix for now until we get a log of vanish
            if (!cancelled)
            {
                m_aura.Owner.SpellCast.Trigger(SpellId.ClassSkillStealth);
            }
        }
    }
}