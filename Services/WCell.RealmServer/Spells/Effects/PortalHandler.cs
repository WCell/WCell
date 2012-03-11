using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;

namespace WCell.RealmServer.Spells.Effects
{
    public class PortalHandler : SpellEffectHandler
    {
        public PortalHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            if ((m_cast.TargetLoc.X == 0 || m_cast.TargetLoc.Y == 0))
            {
                return SpellFailedReason.BadTargets;
            }

            return SpellFailedReason.Ok;
        }

        public override void Apply()
        {
            var portal = Portal.Create(new WorldLocation(m_cast.TargetMap, m_cast.CasterObject.Position), new WorldLocation(m_cast.TargetMap, m_cast.TargetLoc));
            //portal.IsWalkInPortal = false;
            portal.State = GameObjectState.Enabled;
            portal.Flags = 0;
            portal.Orientation = m_cast.TargetOrientation;
        }

        public override ObjectTypes TargetType
        {
            get { return ObjectTypes.Unit; }
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }
    }
}