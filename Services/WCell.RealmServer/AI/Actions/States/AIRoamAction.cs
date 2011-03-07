using System;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
	/// <summary>
	/// AI movemement action for roaming
	/// </summary>
	public class AIRoamAction : AIWaypointMoveAction, IAIStateAction
	{
		public static int MinRoamSpellCastDelay = 60000;

		private DateTime lastSpellCast;

		public AIRoamAction(Unit owner) :
			base(owner, AIMovementType.ForwardThenBack, owner.Waypoints)
		{
			var spawn = owner.SpawnPoint;
			if (spawn != null)
			{
				var spawnEntry = spawn.SpawnEntry;

				if (spawnEntry != null)
				{
					m_WPmovementType = spawnEntry.MoveType;
				}
			}
		}

		public override void Start()
		{
			// make sure we don't have Target nor Attacker
			m_owner.FirstAttacker = null;
			m_owner.Target = null;
			base.Start();
		}

		public override void Update()
		{
			if (!m_owner.Brain.CheckCombat())
			{
				if (UsesSpells && HasSpellReady && m_owner.CanCastSpells && lastSpellCast + TimeSpan.FromMilliseconds(MinRoamSpellCastDelay) < DateTime.Now)
				{
					if (TryCastSpell())
					{
						lastSpellCast = DateTime.Now;
						m_owner.Movement.Stop();
						return;
					}
				}

				base.Update();
			}
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
				if (!spell.HasHarmfulEffects && spell.CanCast(owner))
				{
					return m_owner.SpellCast.Start(spell) == SpellFailedReason.Ok;
				}
			}
			return false;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.VeryLowPriority; }
		}
	}
}