using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells.Warlock
{
    public class RecallToGOHandler : SpellEffectHandler
    {
        public RecallToGOHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }
        public override void Apply()
        {
            var goId = (GOEntryId)Effect.MiscValue;
            var caster = m_cast.CasterUnit as Character;

            if (caster != null)
            {
                var go = caster.GetOwnedGO(goId);
                if (go != null)
                {
                    caster.TeleportTo(go);
                }
            }
        }
    }
}