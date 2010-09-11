using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Gossip.GuardGossip.Neutral
{
    class Shattrath : GossipMenu
    {
        public static void CreateShattrathGossipMenu(uint npcID)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcID);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Talk, "World's End Tavern",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -1760.4f,
                                                               5166.9f, 7, "World's End Tavern")),
                new GossipMenuItem("The Bank",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Bank",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1730.8f, 5496.2f, 7,
                                                                                      "Aldor Bank")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Bank",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1999.6f, 5362.0f, 7,
                                                                                      "Scryers Bank"))
                                       )
                    ),
                new GossipMenuItem("The Inn",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Inn",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1897.5f, 5767.5f, 7,
                                                                                      "Aldor Inn")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Inn",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -2178.0f,
                                                                                      5405.0f,
                                                                                      7, "Scryers Inn"))
                                       )
                    ),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Flight Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -1832.0f, 5299.0f,
                                                               7,
                                                               "Flight Master")),
                new GossipMenuItem("A Mailbox",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Bank",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1730.5f, 5496.0f, 7,
                                                                                      "Aldor Bank")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Inn",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1897.5f, 5767.5f, 7,
                                                                                      "Aldor Inn")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Bank",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1997.7f, 5363.0f, 7,
                                                                                      "Scryers Bank")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Inn",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -2178.0f,
                                                                                      5405.0f,
                                                                                      7, "Scryers Inn"))
                                       )
                    ),
                new GossipMenuItem("Stable Master", new GossipMenu(
                                                        new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Stable",
                                                                           convo =>
                                                                           GossipHandler.SendGossipPOI(convo.Character,
                                                                                                       GossipPOIFlags.
                                                                                                           None,
                                                                                                       -1888.5f,
                                                                                                       5761.0f, 7,
                                                                                                       "Aldor Stable")),
                                                        new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Stable",
                                                                           convo =>
                                                                           GossipHandler.SendGossipPOI(convo.Character,
                                                                                                       GossipPOIFlags.
                                                                                                           None,
                                                                                                       -2170.0f,
                                                                                                       5404.0f, 7,
                                                                                                       "Scryers Stable"))
                                                        )),
                new GossipMenuItem("Battlemasters", new GossipMenu(
                                                        new GossipMenuItem(GossipMenuIcon.Talk, "Alliance Battlemasters",
                                                                           convo =>
                                                                           GossipHandler.SendGossipPOI(convo.Character,
                                                                                                       GossipPOIFlags.
                                                                                                           None,
                                                                                                       -1774.0f,
                                                                                                       5251.0f, 7,
                                                                                                       "Alliance Battlemasters")),
                                                        new GossipMenuItem(GossipMenuIcon.Talk, "Horde Battlemasters",
                                                                           convo =>
                                                                           GossipHandler.SendGossipPOI(convo.Character,
                                                                                                       GossipPOIFlags.
                                                                                                           None,
                                                                                                       -1963.0f,
                                                                                                       5263.0f, 7,
                                                                                                       "Horde Battlemasters")),
                                                        new GossipMenuItem(GossipMenuIcon.Talk, "Arena Battlemasters",
                                                                           convo =>
                                                                           GossipHandler.SendGossipPOI(convo.Character,
                                                                                                       GossipPOIFlags.
                                                                                                           None,
                                                                                                       -1960.0f,
                                                                                                       5175.0f, 7,
                                                                                                       "Arena Battlemasters"))
                                                        )),
                new GossipMenuItem(GossipMenuIcon.Talk, "Mana Loom",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -2070.0f,
                                                               5265.5f, 7, "Mana Loom")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy Lab",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -1648.5f,
                                                               5540.0f, 7, "Alchemy Lab")),
                new GossipMenuItem("Gem Merchant", new GossipMenu(
                                                       new GossipMenuItem(GossipMenuIcon.Talk, "Aldor Gem Merchant",
                                                                          convo =>
                                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                                      GossipPOIFlags.
                                                                                                          None,
                                                                                                      -1645.0f,
                                                                                                      5669.5f, 7,
                                                                                                      "Aldor Gem Merchant")),
                                                       new GossipMenuItem(GossipMenuIcon.Talk, "Scryers Gem Merchant",
                                                                          convo =>
                                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                                      GossipPOIFlags.
                                                                                                          None,
                                                                                                      -2193.0f,
                                                                                                      5424.5f, 7,
                                                                                                      "Scryers Gem Merchant"))
                                                       )),
                new GossipMenuItem("A Profession Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -1648.5f, 5534.0f, 7,
                                                                                      "Lorokeem")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -1847.0f,
                                                                                      5222.0f, 7,
                                                                                      "Kradu Grimblade and Zula Slagfury")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -2067.4f, 5316.5f, 7,
                                                                                      "Jack Trapper")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -2263.5f, 5563.5f, 7,
                                                                                      "High Enchanter Bardolan")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -1591.0f,
                                                                                      5265.5f, 7,
                                                                                      "Mildred Fletcher")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Jewelcrafting",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -1654.0f,
                                                                                      5667.5f, 7, "Hamanar")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -2060.5f, 5256.5f, 7,
                                                                                      "Darmari")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 2048.0f,
                                                                                      5300.0f, 7, "Seymour"))
                                       )
                    )
                )
                                      {
                                          KeepOpen = true
                                      };
        }

        [Initialization]
        [DependentInitialization(typeof (NPCMgr))]
        public static void LoadShattrathMenu()
        {
            CreateShattrathGossipMenu(19687);
            CreateShattrathGossipMenu(18568);
            CreateShattrathGossipMenu(18549);
        }
    }
}