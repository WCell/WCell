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
		/// <summary>
		/// Every x Map-Ticks shuffle Spells
		/// </summary>
		public static int SpellShuffleTicks = 50;

		/// <summary>
		/// Every x Map-Ticks try to cast a random active spell
		/// </summary>
		public static int SpellCastTicks = 1;

		protected float maxDist, desiredDist;

		public AIAttackAction(NPC owner)
			: base(owner)
		{
		}

		public bool UsesSpells
		{
			get { return m_owner.HasSpells; }
		}

		public bool HasSpellReady
		{
			get { return ((NPC)m_owner).NPCSpells.ReadyCount > 0; }
		}

		public override float DistanceMin
		{
			get { return m_owner.BoundingRadius; }
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
				if (!m_owner.CanMelee || m_owner.CheckTicks(SpellCastTicks))
				{
					if (TryCastSpell())
					{
						m_owner.Movement.Stop();
						return;
					}
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

			// no need to shuffle - spells are on cooldowns anyway
			//var spells = owner.NPCSpells;
			//if (owner.CheckTicks(SpellShuffleTicks))
			//{
			//    spells.ShuffleReadySpells();
			//}

			foreach (var spell in owner.NPCSpells.ReadySpells)
			{
				if (spell.CanCast(owner))
				{
					return m_owner.SpellCast.Start(spell) == SpellFailedReason.Ok;
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