using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.MovementActions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.AI.Actions
{
	public class SpellCastAction : AITargetedAction
	{
		protected Spell m_spell;

		public SpellCastAction(IBrain owner, Unit target, Spell spell) : base(owner)
		{
			m_ownerBrain = owner;
			m_target = target;
			m_spell = spell;

			if (m_target != null)
				m_range = new AIRange(m_spell.Range.Min, m_spell.Range.Max);

			IsPrimary = true;
		}
  
		public override AIActionResult Start()
		{
			var ownerUnit = m_ownerBrain.Owner;

			if (ownerUnit == null)
				return AIActionResult.Failure;

			var spellCast = ownerUnit.SpellCast;
			var result = spellCast.Start(m_spell, false, ownerUnit.Target);

			if (result == SpellFailedReason.Ok)
			{
			    if (spellCast.IsCasting || spellCast.IsChanneling)
					return AIActionResult.Executing;
			    return AIActionResult.Success;
			}
		    return AIActionResult.Failure;
		}

		public override AIActionResult Update()
		{
			var spellCast = Owner.Owner.SpellCast;

			if (spellCast.IsCasting || spellCast.IsChanneling)
				return AIActionResult.Executing;
		    return AIActionResult.Success;
		}

		public override void Stop()
		{
			var spellCast = Owner.Owner.SpellCast;

			spellCast.Cancel();
		}
	}
}