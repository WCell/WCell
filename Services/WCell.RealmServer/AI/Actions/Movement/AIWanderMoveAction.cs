using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Waypoints;

namespace WCell.RealmServer.AI.Actions.Movement
{
	class AIWanderMoveAction : AIWaypointMoveAction
	{
		public AIWanderMoveAction(NPC owner) : base(owner)
		{
		}

		public AIWanderMoveAction(NPC owner, AIMovementType waypointSequence)
			: base(owner, waypointSequence)
		{
		}

		public AIWanderMoveAction(NPC owner, AIMovementType waypointSequence, LinkedList<WaypointEntry> waypoints)
			: base(owner, waypointSequence, waypoints)
		{
			owner.SpawnEntry.CreateRandomWaypoints();
		}
	}
}
