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
    class Tiare : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateTiareGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.LibrarianTiare);
            entry.DefaultGossip = new GossipMenu(1,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCTiare), 
                    convo => convo.Speaker.SpellCast.Trigger(SpellId.TeleportColdarraTransitusShieldToAmberLedge, convo.Character))
                );

        }
    }
}
