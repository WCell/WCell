using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.AI.MovementActions;

namespace WCell.RealmServer.AI.Actions
{
	public class MeleeAttackAction : AITargetedAction
	{
		public MeleeAttackAction(IBrain owner)
			: base(owner)
		{
			var ownerUnit = owner.Owner;
			var mainWep = ownerUnit.MainWeapon;

			m_range = new AIRange(mainWep.MinRange, mainWep.MaxRange);

			m_target = ownerUnit.Target;

			IsPrimary = false;
		}

		public override AIActionResult Start()
		{
			var owner = m_ownerBrain.Owner;

			owner.IsFighting = true;

			return InternalUpdate();
		}

		public override AIActionResult Update()
		{
			return InternalUpdate();
		}

		private AIActionResult InternalUpdate()
		{
			var owner = m_ownerBrain.Owner;
			var target = owner.Target;

			if (target == null)
			{
				return AIActionResult.Failure;
			}

			var distance = owner.GetDistanceToTarget(target);

			if (distance >= m_range.Min && distance <= m_range.Max)
			{
				return AIActionResult.Executing;
			}

		    m_ownerBrain.OnCombatTargetOutOfRange();
		    return AIActionResult.Failure;
		}

		public override void Stop()
		{
			var owner = m_ownerBrain.Owner;

			owner.IsFighting = false;
		}
	}
}