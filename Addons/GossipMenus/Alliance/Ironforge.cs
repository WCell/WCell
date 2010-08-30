using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons
{
    class Ironforge : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateIronforgeGossipMenu()
        {
            var entry = NPCMgr.GetEntry(5595);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Trade, "Auction House",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4597.39f, -911.6f,
                                                               7, "Ironforge Auction House")),
                new GossipMenuItem(GossipMenuIcon.Guild, "Bank of Ironforge",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4891.9f, -991.47f,
                                                               7, "The Vault")),
                new GossipMenuItem(GossipMenuIcon.Bind, "Deeprun Tram",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4835.27f,
                                                               -1294.69f, 7, "Deeprun Tram")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "Gryphon Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4821.52f, -1152.3f,
                                                               7, "Ironforge Gryphon Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Guild Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -5021f, -996.45f, 7,
                                                               "Ironforge Visitor's Center")),
                new GossipMenuItem(GossipMenuIcon.Trade, "The Inn",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4850.47f, -872.57f,
                                                               7, "Stonefire Tavern")),
                new GossipMenuItem(GossipMenuIcon.Bind, "The Mailbox",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -4845.7f, -880.55f,
                                                               7, "Ironforge Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Bank, "Stable Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -5010f, -1262f, 7,
                                                               "Ulbrek Firehand")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Weapons Trainer",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -5040f, -1201.88f,
                                                               7, "Bixi and Buliwyf")),
                new GossipMenuItem(GossipMenuIcon.Battlefield, "The Battlemaster",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, -5047.87f,
                                                               -1263.77f, 7, "Battlemasters Ironforge")),
                new GossipMenuItem("A Class Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -5023f,
                                                                                      -1253.68f, 7, "Hall of Arms")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4627f,
                                                                                      -926.45f, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4627.02f,
                                                                                      -926.45f, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4627f,
                                                                                      -926.45f, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Rogue",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4647.83f,
                                                                                      -1124f, 7,
                                                                                      "Ironforge Rogue Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warlock",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4605f,
                                                                                      -1110.45f, 7,
                                                                                      "Ironforge Warlock Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -5023.08f,
                                                                                      -1253.68f, 7, "Hall of Arms"))
                                       )
                    ),
                new GossipMenuItem("A Profession Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4858.5f, -1241.83f, 7,
                                                                                      "Berryfizz's Potions and Mixed Drinks")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4796.97f, -1110.17f, 7,
                                                                                      "The Great Forge")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4767.83f, -1184.59f, 7,
                                                                                      "The Bronze Kettle")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4803.72f, -1196.53f, 7,
                                                                                      "Thistlefuzz Arcanery")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4799.56f, -1250.23f, 7,
                                                                                      "Springspindle's Gadgets")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4881.6f, -1153.13f, 7,
                                                                                      "Ironforge Physician")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4597.91f, -1091.93f, 7,
                                                                                      "Traveling Fisherman")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4881.9f, -1151.92f, 7,
                                                                                      "Ironforge Physician")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4745f,
                                                                                      -1027.57f, 7,
                                                                                      "Finespindle's Leather Goods")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4705.06f, -1116.43f, 7,
                                                                                      "Deepmountain Mining Guild")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None, -4745f,
                                                                                      -1027.57f, 7,
                                                                                      "Finespindle's Leather Goods")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                                                          convo =>
                                                          GossipHandler.SendGossipPOI(convo.Character,
                                                                                      GossipPOIFlags.None,
                                                                                      -4719.60f, -1057f, 7,
                                                                                      "Stonebrow's Clothier"))
                                       )
                    )
                )
            {

                KeepOpen = true
            };
        }
    }
}