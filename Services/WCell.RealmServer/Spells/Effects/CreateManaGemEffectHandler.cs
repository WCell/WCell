using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class CreateManaGemEffectHandler : CreateItemEffectHandler
    {
        public CreateManaGemEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (Effect.BasePoints < 0)
            {
                Effect.BasePoints = 0;
            }

            return base.Initialize();
        }

        public override SpellFailedReason InitializeTarget(WorldObject target)
        {
            var itemId = Effect.ItemId;
            if (((Character)target).Inventory.Contains(itemId))
            {
                return SpellFailedReason.TooManyOfItem;
            }
            return base.InitializeTarget(target);
        }
    }
}