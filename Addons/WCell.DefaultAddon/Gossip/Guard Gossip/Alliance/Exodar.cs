using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.Guard_Gossip.Alliance
{
    class Exodar : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateExodarGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.ExodarPeacekeeper);
            var entry2 = NPCMgr.GetEntry(Constants.NPCs.NPCId.ShieldOfVelen);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Talk, "Auction House",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4013.82f, -11729.64f, 7, 7, "Exodar Auctioneer")),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Bank",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3923.89f, -11544.5f, 7, 7, "Exodar Bank")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Hippogryph Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4058.45f, -11789.7f, 7, 7, "Exodar Hippogryph Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Guild Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4093.38f, -11630.39f, 7, 7, "Exodar Guild Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Inn",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3765.34f, -11695.8f, 7, 7, "Exodar Inn")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Mailbox",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3913.75f, -11606.83f, 7, 7, "Exodar Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Stable Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3787.01f, -11702.7f, 7, 7, "Exodar Stable Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Weapon Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4215.68f, -11628.9f, 7, 7, "Exodar Weapon Master")),
                new GossipMenuItem("Battlemasters",
                    new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Tabard, "Battlemasters",
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3999.82f, -11368.33f, 7, 7, "Exodar Battlemasters")),
                                       new GossipMenuItem(GossipMenuIcon.Tabard, "Arena Battlemaster",
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3725.25f, -11688.3f, 7, 7, "Arena Battlemaster"))
                                   )
                    ),
                new GossipMenuItem("A Class Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Druid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4274.81f, -11495.3f, 7, 7, "Exodar Druid Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4229.36f, -11563.36f, 7, 7, "Exodar Hunter Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4048.8f, -11559.02f, 7, 7, "Exodar Mage Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4176.57f, -11476.46f, 7, 7, "Exodar Paladin Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3972.38f, -11483.2f, 7, 7, "Exodar Priest Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Shaman",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3843.8f, -11390.75f, 7, 7, "Exodar Shaman Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4191.11f, -11650.45f, 7, 7, "Exodar Warrior Trainers"))
                                  )
                    ),
                new GossipMenuItem("A Profession Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4042.37f, -11366.3f, 7, 7, "Exodar Alchemist Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4232.4f, -11705.23f, 7, 7, "Exodar Blacksmithing Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3799.69f, -11650.51f, 7, 7, "Exodar Cook")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Enchanters")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4257.93f, -11636.53f, 7, 7, "Exodar Engineering")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3766.05f, -11481.8f, 7, 7, "Exodar First Aid Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3726.64f, -11384.43f, 7, 7, "Exodar Fishing Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4052.5f, -11356.6f, 7, 7, "Exodar Herbalism Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Inscription",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Inscription")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Jewelcrafting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3786.27f, -11541.33f, 7, 7, "Exodar Jewelcrafters")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.42f, -11772.93f, 7, 7, "Exodar Leatherworking")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4220.31f, -11694.29f, 7, 7, "Exodar Mining Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.97f, -11760.5f, 7, 7, "Exodar Skinning Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4095.78f, -11746.9f, 7, 7, "Exodar Tailors"))
                                 )
                    ),
                    new GossipMenuItem(GossipMenuIcon.Talk, "Lexicon of Power",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Lexicon of Power"))
                    );
            entry2.DefaultGossip = new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Talk, "Auction House",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4013.82f, -11729.64f, 7, 7, "Exodar Auctioneer")),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Bank",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3923.89f, -11544.5f, 7, 7, "Exodar Bank")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Hippogryph Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4058.45f, -11789.7f, 7, 7, "Exodar Hippogryph Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Guild Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4093.38f, -11630.39f, 7, 7, "Exodar Guild Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "The Inn",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3765.34f, -11695.8f, 7, 7, "Exodar Inn")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Mailbox",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3913.75f, -11606.83f, 7, 7, "Exodar Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Stable Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3787.01f, -11702.7f, 7, 7, "Exodar Stable Master")),
                new GossipMenuItem(GossipMenuIcon.Talk, "Weapon Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4215.68f, -11628.9f, 7, 7, "Exodar Weapon Master")),
                new GossipMenuItem("Battlemasters",
                    new GossipMenu(
                                       new GossipMenuItem(GossipMenuIcon.Tabard, "Battlemasters",
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3999.82f, -11368.33f, 7, 7, "Exodar Battlemasters")),
                                       new GossipMenuItem(GossipMenuIcon.Tabard, "Arena Battlemaster",
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3725.25f, -11688.3f,7, 7, "Arena Battlemaster"))
                                   )
                    ),
                new GossipMenuItem("A Class Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Druid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4274.81f, -11495.3f, 7, 7, "Exodar Druid Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4229.36f, -11563.36f, 7, 7, "Exodar Hunter Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4048.8f, -11559.02f, 7, 7, "Exodar Mage Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4176.57f, -11476.46f, 7, 7, "Exodar Paladin Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3972.38f, -11483.2f, 7, 7, "Exodar Priest Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Shaman",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3843.8f, -11390.75f, 7, 7, "Exodar Shaman Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Warrior",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4191.11f, -11650.45f, 7, 7, "Exodar Warrior Trainers"))
                                  )
                    ),
                new GossipMenuItem("A Profession Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4042.37f, -11366.3f, 7, 7, "Exodar Alchemist Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4232.4f, -11705.23f, 7, 7, "Exodar Blacksmithing Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3799.69f, -11650.51f, 7, 7, "Exodar Cook")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Enchanters")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4257.93f, -11636.53f, 7, 7, "Exodar Engineering")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3766.05f, -11481.8f, 7, 7, "Exodar First Aid Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3726.64f, -11384.43f, 7, 7, "Exodar Fishing Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4052.5f, -11356.6f, 7, 7, "Exodar Herbalism Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Inscription",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Inscription")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Jewelcrafting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3786.27f, -11541.33f, 7, 7, "Exodar Jewelcrafters")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.42f, -11772.93f, 7, 7, "Exodar Leatherworking")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4220.31f, -11694.29f, 7, 7, "Exodar Mining Trainers")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.97f, -11760.5f, 7, 7, "Exodar Skinning Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4095.78f, -11746.9f, 7, 7, "Exodar Tailors"))
                                 )
                    ),
                    new GossipMenuItem(GossipMenuIcon.Talk, "Lexicon of Power",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Lexicon of Power"))
                    );
        }
    }
}
