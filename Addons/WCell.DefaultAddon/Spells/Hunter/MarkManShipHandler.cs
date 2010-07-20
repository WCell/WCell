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

    #region Volley
    public class VolleyHandler : SchoolDamageEffectHandler
    {
        public VolleyHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var caster = (Unit)m_cast.Caster;
            var value = ((caster.TotalRangedAP * 0.08370) + CalcEffectValue()); // Magic constant used for a single hunter spell

            ((Unit)target).DoSpellDamage(caster, Effect, (int)value);
        }
    }
    #endregion
}
