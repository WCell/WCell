using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Constants.NPCs;

namespace WCell.RealmServer.Spells.Effects
{
	public class ChargeEffectHandler : SpellEffectHandler
	{
		public ChargeEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (!m_cast.Selected.IsInFrontOf(m_cast.CasterObject))
			{
				failReason = SpellFailedReason.NotInfront;
			}
			else if (m_cast.CasterUnit.IsInCombat)
			{
				failReason = SpellFailedReason.AffectingCombat;
			}
			else
			{
				failReason = SpellFailedReason.Ok;
			}
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