using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Spells.Effects
{
    public class ChargeEffectHandler : SpellEffectHandler
    {
        public ChargeEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if (!m_cast.SelectedTarget.IsInFrontOf(m_cast.CasterObject))
            {
                return SpellFailedReason.NotInfront;
            }

            if (m_cast.CasterUnit.IsInCombat)
            {
                return SpellFailedReason.AffectingCombat;
            }

            return SpellFailedReason.Ok;
        }

        protected override void Apply(WorldObject target)
        {
            var distance = m_cast.CasterObject.Position.GetDistance(target.Position) - ((Unit)target).BoundingRadius - 2;
            var direction = target.Position - m_cast.CasterObject.Position;

            direction.Normalize();
            direction = m_cast.CasterObject.Position + direction * distance;

            MovementHandler.SendMoveToPacket(m_cast.CasterUnit, ref direction, m_cast.CasterUnit.Orientation, 3, MonsterMoveFlags.Walk);

            // TODO: Need to put caster and target in combat
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