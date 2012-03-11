using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
    public class TriggerRitualOfSummoningEffectHandler : SpellEffectHandler
    {
        public TriggerRitualOfSummoningEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (m_cast.InitialTargets == null || m_cast.InitialTargets.Length == 0 || !(m_cast.InitialTargets[0] is Unit))
            {
                return SpellFailedReason.NoValidTargets;
            }
            return SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            m_cast.Trigger(Effect.TriggerSpell, m_cast.InitialTargets);
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }
    }
}