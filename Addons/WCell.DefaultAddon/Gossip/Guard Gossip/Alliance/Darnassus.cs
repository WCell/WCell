using WCell.RealmServer.Gossips;
using WCell.Core.Initialization;
using WCell.RealmServer.NPCs;
using WCell.Constants;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Alliance
{
    class Darnassus : GossipMenu
    {
      /*  [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateDarnassusGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.DarnassusSentinel);
            entry.DefaultGossip = new GossipMenu(new GossipMenuItem(GossipMenuIcon.Trade,"Auction House", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 0f,0f,7,"Auction House")));
        }*/
    }
}
