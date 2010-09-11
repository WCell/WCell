using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Horde
{
    class Orgrimmar : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateOrgrimmarGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.OrgrimmarGrunt);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Trade, "The Bank",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1631.0f, -4375.0f,
                                                               7,
                                                               "Bank of Orgrimmar")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Wind Rider Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1676.0f, -4332.0f,
                                                               7,
                                                               "The Sky Tower")),
                new GossipMenuItem(GossipMenuIcon.Guild, "The Guild Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1576.0f, -4294.0f,
                                                               7,
                                                               "Horde Embassy")),
                new GossipMenuItem(GossipMenuIcon.Bind, "The Inn",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1644.0f, -4447.0f,
                                                               7,
                                                               "Orgrimmar Inn")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Mailbox",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1622.0f, -4388.0f,
                                                               7,
                                                               "Orgrimmar Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Trade, "The Auction House",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1679.0f, -4450.0f,
                                                               7,
                                                               "Orgrimmar Auction House")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Zeppelin Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1337.0f, -4632.0f,
                                                               7,
                                                               "Orgrimmar Zeppelin Tower")),
                new GossipMenuItem(GossipMenuIcon.Resurrect, "The Weapon Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 2092.0f, -4823.0f,
                                                               7,
                                                               "Sayoc & Hanashi")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Stable Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 2133.0f, -4663.0f,
                                                               7,
                                                               "Xon'cha")),
                new GossipMenuItem(GossipMenuIcon.Tabard, "The Officers' Lounge",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1633.0f, -4249.0f,
                                                               7,
                                                               "Hall of Legends")),
                new GossipMenuItem(GossipMenuIcon.Tabard, "The Battlemaster",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1983.0f, -4794.0f,
                                                               7,
                                                               "Hall of the Brave")),
                new GossipMenuItem("A Class Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 2114.0f,
                                                                                      -4625.0f,
                                                                                      7, "Orgrimmar Hunter's Hall")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1451.0f,
                                                                                      -4223.0f,
                                                                                      7, "Darkbriar Lodge")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1442.0f,
                                                                                      -4183.0f,
                                                                                      7, "Spirit Lodge")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Shaman",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1925.0f,
                                                                                      -4181.0f,
                                                                                      7, "Thrall's Fortress")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Rogue",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1773.0f,
                                                                                      -4278.0f,
                                                                                      7, "Shadowswift Brotherhood")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warlock",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1849.0f,
                                                                                      -4359.0f,
                                                                                      7, "Darkfire Enclave")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1983.0f,
                                                                                      -4794.0f,
                                                                                      7, "Hall of the Brave")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1906.0f,
                                                                                      -4134.0f,
                                                                                      7, "Valley of Wisdom"))
                                       )
                    ),
                new GossipMenuItem("A Profession Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1955.0f,
                                                                                      -4475.0f,
                                                                                      7, "Yelmak's Alchemy and Potions")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 2054.0f,
                                                                                      -4831.0f,
                                                                                      7, "The Burning Anvil")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1780.0f,
                                                                                      -4481.0f,
                                                                                      7, "Borstan's Firepit")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1917.0f,
                                                                                      -4434.0f,
                                                                                      7, "Godan's Runeworks")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 2038.0f,
                                                                                      -4744.0f,
                                                                                      7, "Nogg's Machine Shop")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1485.0f,
                                                                                      -4160.0f,
                                                                                      7, "Survival of the Fittest")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1994.0f,
                                                                                      -4655.0f,
                                                                                      7, "Lumak's Fishing")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1898.0f,
                                                                                      -4454.0f,
                                                                                      7, "Jandi's Arboretum")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1852.0f,
                                                                                      -4562.0f,
                                                                                      7, "Kodohide Leatherworkers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 2029.0f,
                                                                                      -4704.0f,
                                                                                      7, "Red Canyon Mining")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1852.0f,
                                                                                      -4562.0f,
                                                                                      7, "Kodohide Leatherworkers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, 1802.0f,
                                                                                      -4560.0f,
                                                                                      7, "Magar's Cloth Goods"))
                                       )
                    )
                )
            {

                KeepOpen = true
            };
        }
    }
}
