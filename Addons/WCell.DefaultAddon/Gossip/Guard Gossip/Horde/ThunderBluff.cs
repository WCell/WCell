using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.Guard_Gossip.Horde
{
    class ThunderBluff : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateThunderBluffGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.Bluffwatcher);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Trade, "The Bank",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1257.8f, 24.14f, 7, 7, "Thunder Bluff Bank")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Wind Rider Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1196.43f, 28.26f, 7, 7, "Wind Rider Roost")),
                new GossipMenuItem(GossipMenuIcon.Guild, "The Guild Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1296.5f, 127.57f, 7 , 7, "Thunder Bluff Civic Information")),
                new GossipMenuItem(GossipMenuIcon.Bind, "The Inn",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1296, 39.7f, 7, 7, "Thunder Bluff Inn")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Mailbox",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1263.59f, 44.36f, 7, 7, "Thunder Bluff Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Trade, "The Auction House",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1205.51f, 105.74f, 7 , 7, "Thunder Bluff Auction House")),
                new GossipMenuItem(GossipMenuIcon.Resurrect, "The Weapon Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1282.31f, 89.56f, 7, 7, "Ansekhwa")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Stable Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1270.19f, 48.84f, 7, 7, "Bulrug")),
                new GossipMenuItem(GossipMenuIcon.Tabard, "The Battlemaster",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1391.22f, -81.33f, 7, 7, "Thunder Bluff's Battlemasters")),
                new GossipMenuItem("A Class Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Druid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1054.47f, -285, 7, 7, "Hall of Elders")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1416.32f, -114.28f, 7, 7, "Hunter's Hall")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1061.2f, 195.5f, 7, 7, "Pools of Vision")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1061.2f, 195.5f, 7, 7, "Pools of Vision")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Shaman",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -989.54f, 278.25f, 7, 7, "Hall of Spirits")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1416.32f, -114.28f, 7, 7, "Hunter's Hall"))
                                    )
                            ),
                new GossipMenuItem("A Profession Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1085.56f, 27.29f, 7, 7, "Bena's Alchemy")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1239.75f, 104.88f, 7, 7, "Karn's Smithy")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1214.5f, -21.23f, 7, 7, "Aska's Kitchen")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1112.65f, 48.26f, 7, 7, "Dawnstrider Enchanters")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -996.58f, 200.5f, 7, 7, "Spiritual Healing")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1169.35f, -68.87f, 7, 7, "Mountaintop Bait & Tackle")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1137.7f, -1.51f, 7, 7, "Holistic Herbalism")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1156.22f, 66.86f, 7, 7, "Thunder Bluff Armorers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1249.17f, 155, 7, 7, "Stonehoof Geology")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1148.56f, 51.18f, 7, 7, "Mooranta")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1156.22f, 66.86f, 7, 7, "Thunder Bluff Armorers"))
                                    )
                            )
                    );
        }
    }
}