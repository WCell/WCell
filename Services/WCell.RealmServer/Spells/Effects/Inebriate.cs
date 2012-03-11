using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class Inebriate : SpellEffectHandler
    {
        public Inebriate(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            var target = Cast.CasterObject as Character;

            if (target == null)
                return;

            var state = target.DrunkState;
            state += (byte)Effect.BasePoints;
            target.DrunkState = state;
            if (state > 100)
                target.SpellCast.TriggerSelf(SpellId.DrunkenVomit);

            // 1 drunk point is removed every 2173ms
            var delay = 2173 * (byte)Effect.BasePoints;
            //target.CallPeriodicallyUntil(2173, delay, obj => target.DrunkState -= 1);
        }
    }
}