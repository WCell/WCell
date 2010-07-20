using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
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
