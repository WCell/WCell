using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
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
            entry.DefaultGossip = new GossipMenu(9316,
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    new GossipMenu(9317,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9649.429f, -7134.027f, 7, 7, "Silvermoon City Auction House"), new GossipMenu(9318)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardEast),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9682.864f, -7515.786f, 7, 7, "Silvermoon City Auction House"), new GossipMenu(9319))
                                    )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    new GossipMenu(9320,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9522.104f, -7208.878f, 7, 7, "Silvermoon City West Bank"), new GossipMenu(9321)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardEast),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9791.07f, -7488.041f, 7, 7, "Silvermoon City East Bank"), new GossipMenu(9322))
                                    )
                    ),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardDragonhawk),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9378.45f, -7163.94f, 7, 7, "Silvermoon City Dragonhawk Master"), new GossipMenu(9323)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9480.75f, -7345.587f, 7, 7, "Silvermoon City Guild Master"), new GossipMenu(9324)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    new GossipMenu(9325,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSilvermoonInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9677.113f, -7367.575f, 7, 7, "Silvermoon City Inn"), new GossipMenu(9325)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWayfarer),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9562.813f, -7218.63f, 7, 7, "The Wayfarer's Rest Tavern"), new GossipMenu(9603))
                                    )
                    ),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9743.078f, -7466.4f, 7, 7, "Silvermoon City Mailbox"), new GossipMenu(9326)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9904.95f, -7404.31f, 7, 7, "Silvermoon City Stable Master"), new GossipMenu(9327)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9841.17f, -7505.13f, 7, 7, "Silvermoon City Weapon Master"), new GossipMenu(9328)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9850.74f, -7563.84f, 7, 7, "Silvermoon City Battlemasters"), new GossipMenu(9329)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(9331,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9700.55f, -7262.57f, 7, 7, "Silvermoon City Druid Trainer"), new GossipMenu(9330)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9930.568f, -7412.115f, 7, 7, "Silvermoon City Hunter Trainer"), new GossipMenu(9332)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9996.914f, -7104.803f, 7, 7, "Silvermoon City Mage Trainer"), new GossipMenu(9333)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9850.22f, -7516.93f, 7, 7, "Silvermoon City Paladin Trainer"), new GossipMenu(9334)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9935.37f, -7131.14f, 7, 7, "Silvermoon City Priest Trainer"), new GossipMenu(9335)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9739.88f, -7374.33f, 7, 7, "Silvermoon City Rogue Trainer"), new GossipMenu(9336)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9803.052f, -7316.967f, 7, 7, "Silvermoon City, Warlock Trainer"), new GossipMenu(9337))
                                    )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(9338,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10000.9f, -7216.63f, 7, 7, "Silvermoon City Alchemy"), new GossipMenu(9339)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9841.43f, -7361.53f, 7, 7, "Silvermoon City Blacksmithing"), new GossipMenu(9340)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9577.26f, -7243.6f, 7, 7, "Silvermoon City Cooking"), new GossipMenu(9624)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9962.57f, -7246.18f, 7, 7, "Silvermoon City Enchanting"), new GossipMenu(9341)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9808.85f, -7287.31f, 7, 7, "Silvermoon City Engineering"), new GossipMenu(9342)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9588.61f, -7337.526f, 7, 7, "Silvermoon City First Aid"), new GossipMenu(9343)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9601.97f, -7332.34f, 7, 7, "Silvermoon City Fishing"), new GossipMenu(9344)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9996.96f, -7208.39f, 7, 7, "Silvermoon City Herbalism"), new GossipMenu(9345)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfInscription),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9957.12f, -7242.85f, 7, 7, "Silvermoon City Inscription"), new GossipMenu(13893)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfJewelcrafting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1498.76f, 196.43f, 7, 7, "Silvermoon City Jewelcrafting"), new GossipMenu(9346)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9502.486f, -7425.51f, 7, 7, "Silvermoon City Leatherworking"), new GossipMenu(9347)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9813.73f, -7360.19f, 7, 7, "Silvermoon City Mining"), new GossipMenu(9348)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9513.37f, -7429.4f, 7, 7, "Silvermoon City Skinning"), new GossipMenu(9349)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9727.56f, -7086.65f, 7, 7, "Silvermoon City Tailoring"), new GossipMenu(9350))
                                    )
                        ),
                    new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardManaLoom),
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9751.013f, -7074.85f, 7, 7, "Silvermoon City Mana Loom"), new GossipMenu(10502)),
                    new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLexicon),
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9957.12f, -7242.85f, 7, 7, "Silvermoon City Lexicon of Power"), new GossipMenu(14174))
                        )
            {
                KeepOpen = true
            };
        }
    }
}