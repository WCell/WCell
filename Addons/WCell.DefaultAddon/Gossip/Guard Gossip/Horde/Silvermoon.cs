using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Horde
{
    class Silvermoon : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof (NPCMgr))]
        public static void CreateSilvermoonGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.SilvermoonCityGuardian);
            entry.DefaultGossip = new GossipMenu(
                new GossipMenuItem("Auction House",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Trade, "To the west.",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9649.429f, -7134.027f, 7, 7, "Silvermoon City Auction House")),
                        new GossipMenuItem(GossipMenuIcon.Trade, "To the east.",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9682.864f, -7515.786f, 7, 7, "Silvermoon City Auction House"))
                                    )
                    ),
                new GossipMenuItem("The Bank",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Trade, "The west.",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9522.104f, -7208.878f, 7, 7, "Silvermoon City West Bank")),
                        new GossipMenuItem(GossipMenuIcon.Trade, "The east.",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9791.07f, -7488.041f, 7, 7, "Silvermoon City East Bank"))
                                    )
                    ),
                new GossipMenuItem(GossipMenuIcon.Taxi, "Dragonhawk Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9378.45f, -7163.94f, 7, 7, "Silvermoon City Dragonhawk Master")),
                new GossipMenuItem(GossipMenuIcon.Guild, "The Guild Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9480.75f, -7345.587f, 7, 7, "Silvermoon City Guild Master")),
                new GossipMenuItem("The Inn",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Bind, "The Silvermoon City Inn.",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9677.113f, -7367.575f, 7, 7, "Silvermoon City Inn")),
                        new GossipMenuItem(GossipMenuIcon.Bind, "The Wayfarer's Rest tavern",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9562.813f, -7218.63f, 7, 7, "The Wayfarer's Rest tavern"))
                                    )
                    ),
                new GossipMenuItem(GossipMenuIcon.Taxi, "Mailbox",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9743.078f, -7466.4f, 7, 7, "Silvermoon City Mailbox")),
                new GossipMenuItem(GossipMenuIcon.Taxi, "The Stable Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9904.95f, -7404.31f, 7, 7, "Silvermoon City Stable Master")),
                new GossipMenuItem(GossipMenuIcon.Resurrect, "The Weapon Master",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9841.17f, -7505.13f, 7, 7, "Silvermoon City Weapon Master")),
                new GossipMenuItem(GossipMenuIcon.Tabard, "The Battlemaster",
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9850.74f, -7563.84f, 7, 7, "Silvermoon City Battlemasters")),
                new GossipMenuItem("A Class Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Druid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9700.55f, -7262.57f, 7, 7, "Silvermoon City Druid Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Hunter",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9930.568f, -7412.115f, 7, 7, "Silvermoon City Hunter Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mage",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9996.914f, -7104.803f, 7, 7, "Silvermoon City Mage Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Paladin",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9850.22f, -7516.93f, 7, 7, "Silvermoon City Paladin Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Priest",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9935.37f, -7131.14f, 7, 7, "Silvermoon City Priest Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Rogue",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9739.88f, -7374.33f, 7, 7, "Silvermoon City Rogue Trainer")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Warlock",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9803.052f, -7316.967f, 7, 7, "Silvermoon City, Warlock Trainer"))
                                    )
                    ),
                new GossipMenuItem("A Profession Trainer",
                    new GossipMenu(
                        new GossipMenuItem(GossipMenuIcon.Talk, "Alchemy",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10000.9f, -7216.63f, 7, 7, "Silvermoon City Alchemy")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Blacksmithing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9841.43f, -7361.53f, 7, 7, "Silvermoon City Blacksmithing")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Cooking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9577.26f, -7243.6f, 7, 7, "Silvermoon City Cooking")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Enchanting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9962.57f, -7246.18f, 7, 7, "Silvermoon City Enchanting")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Engineering",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9808.85f, -7287.31f, 7, 7, "Silvermoon City Engineering")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "First Aid",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9588.61f, -7337.526f, 7, 7, "Silvermoon City First Aid")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Fishing",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9601.97f, -7332.34f, 7, 7, "Silvermoon City Fishing")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Herbalism",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9996.96f, -7208.39f, 7, 7, "Silvermoon City Herbalism")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Inscription",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9957.12f, -7242.85f, 7, 7, "Silvermoon City Inscription")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Jewelcrafting",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1498.76f, 196.43f, 7, 7, "Silvermoon City Jewelcrafting")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Leatherworking",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9502.486f, -7425.51f, 7, 7, "Silvermoon City Leatherworking")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Mining",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9813.73f, -7360.19f, 7, 7, "Silvermoon City Mining")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Skinning",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9513.37f, -7429.4f, 7, 7, "Silvermoon City Skinning")),
                        new GossipMenuItem(GossipMenuIcon.Talk, "Tailoring",
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9727.56f, -7086.65f, 7, 7, "Silvermoon City Tailoring"))
                                    )
                        ),
                    new GossipMenuItem(GossipMenuIcon.Talk, "Mana Loom",
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9751.013f, -7074.85f, 7, 7, "Silvermoon City Mana Loom")),
                    new GossipMenuItem(GossipMenuIcon.Talk, "Lexicon of Power",
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9957.12f, -7242.85f, 7, 7, "Silvermoon City Lexicon of Power"))
                        );
        }
    }
}