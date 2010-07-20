using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
    #region Arcane Shot
    public class ArcaneShotHandler : SchoolDamageEffectHandler
    {
        public ArcaneShotHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var caster = (Unit)m_cast.Caster;
            var value = ((caster.TotalRangedAP * 0.15) + CalcEffectValue());

            ((Unit)target).DoSpellDamage(caster, Effect, (int)value);
        }
    }
    #endregion

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
