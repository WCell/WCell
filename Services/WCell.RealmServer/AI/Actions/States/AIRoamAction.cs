using WCell.Constants.NPCs;
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
				base.Update();
			}
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.VeryLowPriority; }
		}
	}
}