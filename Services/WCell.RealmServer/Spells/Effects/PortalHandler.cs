using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.Constants.GameObjects;

namespace WCell.RealmServer.Spells.Effects
{
	public class PortalHandler : SpellEffectHandler
	{
		public PortalHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if ((m_cast.TargetLoc.X == 0 || m_cast.TargetLoc.Y == 0))
			{
				failReason = SpellFailedReason.BadTargets;
			}

			//if (zone != null && !zone.Flags.And(ZoneFlags.CanHearthAndResurrectFromArea))
			//{
			//    failReason = SpellFailedReason.NotHere;
			//}
		}

		public override void Apply()
		{
			var portal = Portal.Create(new WorldLocation(m_cast.TargetRegion, m_cast.TargetLoc));
			//portal.IsWalkInPortal = false;
			portal.State = GameObjectState.Enabled;
			portal.Flags = 0;
			portal.Orientation = m_cast.TargetOrientation;
			portal.Position = m_cast.Caster.Position;

			m_cast.TargetRegion.AddObjectLater(portal);
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