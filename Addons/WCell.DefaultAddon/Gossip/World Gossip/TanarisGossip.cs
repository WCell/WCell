using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

namespace WCell.Addons.Default.Gossip.World_Gossip
{
    class TanarisGossips : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateTanarisGossipMenus()
        {
            var Curgle = NPCMgr.GetEntry(Constants.NPCs.NPCId.CurgleCranklehop);
            var Trenton = NPCMgr.GetEntry(Constants.NPCs.NPCId.TrentonLighthammer);
            Curgle.DefaultGossip = new GossipMenu(1519,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCCurgle1), new GossipMenu(1521)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCCurgle2), new GossipMenu(1646)));
            Trenton.DefaultGossip = new GossipMenu(1758,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTrenton), new GossipMenu(1759)));
        }
    }
}
