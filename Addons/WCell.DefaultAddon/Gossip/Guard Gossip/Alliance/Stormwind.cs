using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Alliance
{
    class Stormwind : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateStormwindGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.StormwindGuard);
            var entry2 = NPCMgr.GetEntry(Constants.NPCs.NPCId.StormwindCityPatroller);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Talk, "Auction House", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-8815.65f,665.4402f,7,7,"Stormwind Auction House")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Bank of Stormwind",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4891.9f, -991.47f, 7,7, "Stormwind Bank")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Stormwind Harbor",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-8577.078f,988.1891f, 7,7,"Stormwind Harbor")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Deeprun Tram",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8391.07f,570.8477f, 7,7, "The Deeprun Tram")),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Inn",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8861.762f, -664.0831f, 7,7, "The Gilded Rose")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Gryphon Master",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8837.699f, -487.6247f, 7,7, "Stormwind Gryphon Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Guild Master",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8886.755f, -600.606f, 7, 7,"Stormwind Visitor's Center")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Locksmith",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8424.989f, -627.3555f, 7,7, "Stormwind Locksmith")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Stable Master",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8432.932f, -554.4769f, 7, 7,"Jenova Stoneshield")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Weapons Trainer",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8796.248f, -613.0318f,7,7, "Woo Ping")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Officers' Lounge",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8767.027f, -409.2762f,7,7, "Champions' Hall")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Battlemaster",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8393.95f,-270.5327f, 7, 7, "Battlemasters Stormwind")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Barber",convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8746.796f,-658.101f, 7, 7, "Stormwind Barber")),
                new GossipMenuItem("A Class Trainer",
                    new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -5023f,-1253.68f, 7, 7, "Hall of Arms")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mage",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4627f,-926.45f, 7, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4627.02f,-926.45f, 7, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Priest",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4627f,-926.45f, 7, 7, "Hall of Mysteries")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Rogue",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4647.83f,-1124f, 7, 7,"Ironforge Rogue Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warlock",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4605f,-1110.45f, 7, 7,"Ironforge Warlock Trainer")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -5023.08f,-1253.68f, 7, 7, "Hall of Arms"))
                                  )
                    ),
                new GossipMenuItem("A Profession Trainer",
                    new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4858.5f, -1241.83f, 7, 7,"Berryfizz's Potions and Mixed Drinks")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4796.97f, -1110.17f, 7, 7,"The Great Forge")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4767.83f, -1184.59f, 7, 7,"The Bronze Kettle")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4803.72f, -1196.53f, 7, 7,"Thistlefuzz Arcanery")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4799.56f, -1250.23f, 7, 7,"Springspindle's Gadgets")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4881.6f, -1153.13f, 7, 7,"Ironforge Physician")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4597.91f, -1091.93f, 7, 7,"Traveling Fisherman")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4881.9f, -1151.92f, 7, 7,"Ironforge Physician")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4745f,-1027.57f, 7, 7,"Finespindle's Leather Goods")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Mining",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4705.06f, -1116.43f, 7, 7,"Deepmountain Mining Guild")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -4745f,-1027.57f, 7, 7,"Finespindle's Leather Goods")),
                                       new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",convo =>GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-4719.60f, -1057f, 7, 7,"Stonebrow's Clothier"))
                                 )
                    ));
            entry2.DefaultGossip = new GossipMenu(
                            new GossipMenuItem(GossipMenuIcon.Talk, "Auction House", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8815.65f, 665.4402f, 7, 7, "Stormwind Auction House")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Bank of Stormwind", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4891.9f, -991.47f, 7, 7, "Stormwind Bank")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Stormwind Harbor", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8577.078f, 988.1891f, 7, 7, "Stormwind Harbor")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Deeprun Tram", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8391.07f, 570.8477f, 7, 7, "The Deeprun Tram")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "The Inn", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8861.762f, -664.0831f, 7, 7, "The Gilded Rose")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Gryphon Master", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8837.699f, -487.6247f, 7, 7, "Stormwind Gryphon Master")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Guild Master", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8886.755f, -600.606f, 7, 7, "Stormwind Visitor's Center")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Locksmith", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8424.989f, -627.3555f, 7, 7, "Stormwind Locksmith")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Stable Master", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8432.932f, -554.4769f, 7, 7, "Jenova Stoneshield")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Weapons Trainer", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8796.248f, -613.0318f, 7, 7, "Woo Ping")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Officers' Lounge", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8767.027f, -409.2762f, 7, 7, "Champions' Hall")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Battlemaster", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8393.95f, -270.5327f, 7, 7, "Battlemasters Stormwind")),
                            new GossipMenuItem(GossipMenuIcon.Talk, "Barber", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8746.796f, -658.101f, 7, 7, "Stormwind Barber")),
                            new GossipMenuItem("A Class Trainer",
                                new GossipMenu(
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Hunter", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023f, -1253.68f, 7, 7, "Hall of Arms")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Mage", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Paladin", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627.02f, -926.45f, 7, 7, "Hall of Mysteries")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Priest", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Rogue", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4647.83f, -1124f, 7, 7, "Ironforge Rogue Trainer")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Warlock", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4605f, -1110.45f, 7, 7, "Ironforge Warlock Trainer")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Warrior", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023.08f, -1253.68f,7, 7, "Hall of Arms"))
                                              )
                                ),
                            new GossipMenuItem("A Profession Trainer",
                                new GossipMenu(
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4858.5f, -1241.83f, 7, 7, "Berryfizz's Potions and Mixed Drinks")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4796.97f, -1110.17f, 7, 7, "The Great Forge")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Cooking", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4767.83f, -1184.59f, 7, 7, "The Bronze Kettle")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4803.72f, -1196.53f, 7, 7, "Thistlefuzz Arcanery")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Engineering", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4799.56f, -1250.23f, 7, 7, "Springspindle's Gadgets")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "First Aid", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.6f, -1153.13f, 7, 7, "Ironforge Physician")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Fishing", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4597.91f, -1091.93f, 7, 7, "Traveling Fisherman")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.9f, -1151.92f, 7, 7, "Ironforge Physician")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Mining", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4705.06f, -1116.43f, 7, 7, "Deepmountain Mining Guild")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Skinning", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods")),
                                                   new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring", convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4719.60f, -1057f, 7, 7, "Stonebrow's Clothier"))
                                             )
                                ));
        }
    }
}