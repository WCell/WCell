using WCell.Constants.Factions;

namespace WCell.RealmServer.Spells.Auras
{
    public class ForceReactionHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            var factionId = (FactionId)SpellEffect.MiscValue;
        }
    }
}