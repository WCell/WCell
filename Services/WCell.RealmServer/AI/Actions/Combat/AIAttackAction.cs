using System;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI.Actions.Combat
{
	/// <summary>
	/// Attack with the main weapon
	/// </summary>
	public class AIAttackAction : AITargetMoveAction
	{
		protected float minDist, maxDist, desiredDist;

		public AIAttackAction(NPC owner)
			: base(owner)
		{
			minDist = owner.BoundingRadius;
		}

		public override float DistanceMin
		{
			get { return minDist; }
		}

		public override float DistanceMax
		{
			get { return maxDist; }
		}

		public override float DesiredDistance
		{
			get { return desiredDist; }
		}

		/// <summary>
		/// Called when starting to attack a new Target
		/// </summary>
		public override void Start()
		{
			m_owner.IsFighting = true;
			if (UsesSpells)
			{
				((NPC)m_owner).NPCSpells.ShuffleReadySpells();
			}

			m_target = m_owner.Target;
			if (m_target != null)
			{
				maxDist = m_owner.GetBaseAttackRange(m_target) - 1;
				if (maxDist < 0.5f)
				{
					maxDist = 0.5f;
				}
				desiredDist = maxDist / 2;
			}
			if (m_owner.CanMelee)
			{
				base.Start();
			}
		}

		/// <summary>
		/// Called during every Brain tick
		/// </summary>
		public override void Update()
		{
			// Check for spells that we can cast
			if (UsesSpells && HasSpellReady && m_owner.CanCastSpells)
			{
				if (TryCastSpell())
				{
					m_owner.Movement.Stop();
					return;
				}
			}

			// Move in on the target
			if (m_owner.CanMelee)
			{
				base.Update();
			}
		}

		/// <summary>
		/// Called when we stop attacking a Target
		/// </summary>
		public override void Stop()
		{
			m_owner.IsFighting = false;
			base.Stop();
		}

		/// <summary>
		/// Tries to cast a Spell that is ready and allowed in the current context.
		/// </summary>
		/// <returns></returns>
		protected bool TryCastSpell()
		{
			var owner = (NPC)m_owner;

			foreach (var spell in owner.NPCSpells.ReadySpells)
			{
				var err = spell.CheckCasterConstraints(owner);
				if (err == SpellFailedReason.Ok)
				{
					return m_owner.SpellCast.Start(spell) == SpellFailedReason.Ok;
				}
				else if (err == SpellFailedReason.NoPower)
				{
					// add this for now -> need to think of a smarter way to handle this
					owner.Say("Not enough " + owner.PowerType);
				}
			}
			return false;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Active; }
		}
	}
}