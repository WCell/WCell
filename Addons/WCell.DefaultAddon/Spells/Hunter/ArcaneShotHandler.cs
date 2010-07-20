using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
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
}