using WCell.Addons.Default.Lang;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.NPCs;

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
