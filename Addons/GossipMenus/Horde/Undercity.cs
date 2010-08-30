using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons
{
    internal class Undercity : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof (NPCMgr))]
        public static void CreateUndercityGossipMenu()
        {
            var entry = NPCMgr.GetEntry(5624);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Trade, "The Bank",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1596.0f, 232.0f, 7,
                                                               "Undercity Bank")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Bat Handler",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1566.0f, 271.0f, 7,
                                                               "Undercity Bat Handler")),
                new GossipMenuItem(GossipMenuIcon.Guild, "The Guild Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1594.0f, 205.0f, 7,
                                                               "Undercity Guild Master")),
                new GossipMenuItem(GossipMenuIcon.Bind, "The Inn",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1639.0f, 221.0f, 7,
                                                               "Undercity Inn")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Mailbox",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1632.0f, -219.0f, 7,
                                                               "Undercity Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Trade, "The Auction House",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1647.0f, 258.0f, 7,
                                                               "Undercity Auction House")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Zeppelin Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 2059.0f, 274.0f, 7,
                                                               "Undercity Zeppelin")),
                new GossipMenuItem(GossipMenuIcon.Resurrect, "The Weapon Master",
                                   convo =>
                                   GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1670.0f, 324.0f, 7,
                                                               "Archibald")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Stable Master",
                                   convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1634.0f,
                                                                        226.76f, 7, "Anya Maulray")),
                new GossipMenuItem(GossipMenuIcon.Tabard, "The Battlemaster",
                                   convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.None, 1329.0f,
                                                                        333.92f, 7, "Undercity's Battlemasters")),
                new GossipMenuItem("A Class Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1781.0f, 53.0f,
                                                                                               7,
                                                                                               "Undercity Mage Trainers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1758.33f, 401.5f,
                                                                                               7,
                                                                                               "Undercity Priest Trainers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Rogue",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1418.56f, 65.0f, 7,
                                                                                               "Undercity Rogue Trainers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warlock",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1780.92f, 53.16f,
                                                                                               7,
                                                                                               "Undercity Warlock Trainers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1775.59f,
                                                                                               418.19f, 7,
                                                                                               "Undercity Warrior Trainers")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1299.59f, 319.0f, 7,
                                                                                               "Undercity Paladin Trainers"))
                                       )
                    ),
                new GossipMenuItem("A Profession Trainer",
                                   new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1419.82f,
                                                                                               417.19f, 7,
                                                                                               "The Apothecarium")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1696.0f, 285.0f,
                                                                                               7,
                                                                                               "Undercity Blacksmithing Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1596.34f,
                                                                                               274.68f, 7,
                                                                                               "Undercity Cooking Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1488.54f,
                                                                                               280.19f, 7,
                                                                                               "Undercity Enchanting Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1408.58f,
                                                                                               143.43f, 7,
                                                                                               "Undercity Engineering Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1519.65f,
                                                                                               167.19f, 7,
                                                                                               "Undercity First Aid Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1679.9f, 89.0f, 7,
                                                                                               "Undercity Fishing Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1558.0f,
                                                                                               349.36f, 7,
                                                                                               "Undercity Herbalism Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1498.76f,
                                                                                               196.43f, 7,
                                                                                               "Undercity Leatherworking Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1642.88f,
                                                                                               335.58f, 7,
                                                                                               "Undercity Mining Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1498.6f, 196.46f,
                                                                                               7,
                                                                                               "Undercity Skinning Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                                                          convo => GossipHandler.SendGossipPOI(convo.Character,
                                                                                               GossipPOIFlags.None,
                                                                                               1689.55f, 193.0f, 7,
                                                                                               "Undercity Tailoring Trainer"))
                                       )
                    )
                )
            {
                KeepOpen = true
            };
        }
    }
}