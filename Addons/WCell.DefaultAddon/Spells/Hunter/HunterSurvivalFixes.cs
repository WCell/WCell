using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Hunter
{
    public static class HunterSurvival
    {
        [Initialization(InitializationPass.Second)]
        public static void FixIt()
        {
            // Moongonse Bite does incorrect damage ($AP*0.2 + $m1)
            SpellLineId.HunterMongooseBite.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.2f;
                });

            // Black Arrow does incorrect damage ($RAP*0.1 + $m1)
            SpellLineId.HunterSurvivalBlackArrow.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.1f;
                });
        }
    }
}
