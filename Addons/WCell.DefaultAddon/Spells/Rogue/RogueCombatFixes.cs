using System.Linq;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Rogue
{
    public static class RogueCombatFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixRogue()
        {
            SpellHandler.Apply(spell =>
            {
                spell.RemoveEffect(SpellEffectType.ScriptEffect);
                var effect = spell.GetEffect(SpellEffectType.Dummy);
                effect.SpellEffectHandlerCreator = (cast, eff) => new ImprovedSprintHandler(cast, eff);
            }, SpellId.EffectImprovedSprint);
        }
    }

    #region ImprovedSprintHandler
    class ImprovedSprintHandler : SpellEffectHandler
    {
        public ImprovedSprintHandler(SpellCast cast, SpellEffect effect) : base(cast, effect)
        {}

        protected override void Apply(RealmServer.Entities.WorldObject target)
        {
            var chr = target as Character;
            if(chr != null)
            {
                chr.Auras.RemoveWhere(aura => SpellConstants.MoveMechanics[(int)aura.Spell.Mechanic] || aura.Handlers.Any(handler => SpellConstants.MoveMechanics[(int)handler.SpellEffect.Mechanic]) && !aura.IsBeneficial);
            }
            base.Apply(target);
        }
    }
    #endregion
}
