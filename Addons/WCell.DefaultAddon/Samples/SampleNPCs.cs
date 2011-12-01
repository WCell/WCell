using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Samples
{
	public static class SampleNPCs
	{
		#region Teleport NPCs
		/// <summary>
		/// 
		/// </summary>
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void SetupTeleportGossips()
		{
			var loc = WorldLocationMgr.Stormwind;
			if (loc != null)
			{
				CreateTeleportNPC(NPCId.ZzarcVul, loc);
			}
		}

		public static void CreateTeleportNPC(NPCId id, IWorldLocation loc)
		{
			CreateTeleportNPC(id, loc.Position, loc.MapId, loc.Phase);
		}

		public static void CreateTeleportNPC(uint id, Vector3 location, MapId mapId, uint phase = WorldObject.DefaultPhase)
		{
			CreateTeleportNPC((NPCId)id, location, mapId, phase);
		}

		public static void CreateTeleportNPC(NPCId id, Vector3 location, MapId mapId, uint phase = WorldObject.DefaultPhase)
		{
			var spawn = new NPCSpawnEntry(id, mapId, location);
			spawn.FinalizeDataHolder();

			spawn.Spawned += npc =>
			{
				npc.Invulnerable++;
				npc.GossipMenu = WorldLocationMgr.CreateTeleMenu();
			};
		}
		#endregion
	}

	//public static class NPCThrall
	//{
	//    [Initialization]
	//    [DependentInitialization(typeof(NPCMgr))]
	//    public static void SetupIni()
	//    {
	//        var entry = NPCMgr.GetEntry(NPCId.Thrall);
	//        entry.BrainCreator = thrall =>
	//        {
	//            return new ThrallBrain(thrall);
	//        };

	//    }

	//    public class ThrallBrain : MobBrain
	//    {
	//        public ThrallBrain(NPC thrall)
	//            : base(thrall)
	//        {
	//            // ...
	//        }

	//        public override void Update(int dt)
	//        {

	//            base.Update(dt);
	//        }

	//        public override void OnActivate()
	//        {
	//            base.OnActivate();
	//            Actions[BrainState.Roam] = new ThrallRoamAction((NPC)m_owner);
	//        }
	//    }

	//    public class ThrallRoamAction : AIRoamAction
	//    {
	//        public ThrallRoamAction(NPC thrall)
	//            : base(thrall)
	//        {
	//        }

	//        public override void Update()
	//        {
	//            if (!m_owner.IsFighting)
	//            {
	//                m_owner.Say("Me Thrall!");
	//            }

	//            base.Update();

	//        }
	//    }
	//}

}