using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class SummonAllTotemsHandler : SpellEffectHandler
    {
        public SummonAllTotemsHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override void Apply()
        {
            var target = Cast.CasterObject as Character;
            if (target == null) return;

            var buttonId = Effect.MiscValue + 132;
            for (var actionButtonCount = Effect.MiscValueB; actionButtonCount != 0; actionButtonCount--, buttonId++)
            {
                //type 0 is spell
                if (target.GetTypeFromActionButton(buttonId) != 0) continue;

                var spellId = target.GetActionFromActionButton(buttonId);
                var spell = SpellHandler.Get(spellId);
                if (spell == null) continue;

                var cast = target.SpellCast;
                if (cast == null) continue;

                cast.Trigger(spell);
            }
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Unit; }
        }
    }
}