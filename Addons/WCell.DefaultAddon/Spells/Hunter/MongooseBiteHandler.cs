using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
    public class MongooseBiteHandler : SchoolDamageEffectHandler
    {
        public MongooseBiteHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        protected override void Apply(WorldObject target)
        {
            var caster = (Unit)m_cast.Caster;
            var damage = ((caster.TotalMeleeAP * 0.2) + CalcDamageValue());

            ((Unit)target).DoSpellDamage(caster, Effect, (int)damage);
        }
    }
}