using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.AI.Actions.Movement
{
    internal class AIWanderMoveAction : AIWaypointMoveAction
    {
        public AIWanderMoveAction(NPC owner)
            : base(owner)
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
