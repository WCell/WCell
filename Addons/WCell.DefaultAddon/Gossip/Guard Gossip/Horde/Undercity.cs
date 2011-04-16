using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
using WCell.Core.Initialization;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Horde
{
    class Undercity : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof (NPCMgr))]
        public static void CreateUndercityGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.KorKronOverseer);
            entry.DefaultGossip = new GossipMenu(3543,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1596.0f, 232.0f, 7, 7, "Undercity Bank"), new GossipMenu(3514)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBat),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1566.0f, 271.0f, 7, 7, "Undercity Bat Handler"),new GossipMenu(3515)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1594.0f, 205.0f, 7, 7, "Undercity Guild Master"), new GossipMenu(3516)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1639.0f, 221.0f, 7, 7, "Undercity Inn"), new GossipMenu(3517)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1632.0f, -219.0f, 7, 7, "Undercity Mailbox"), new GossipMenu(3518)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1647.0f, 258.0f, 7, 7, "Undercity Auction House"), new GossipMenu(3520)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardZepplin),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2059.0f, 274.0f, 7, 7, "Undercity Zeppelin"), new GossipMenu(3519)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1670.0f, 324.0f, 7, 7, "Archibald"), new GossipMenu(4521)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1634.0f, 226.76f, 7, 7, "Anya Maulray"), new GossipMenu(5979)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1329.0f, 333.92f, 7, 7, "Undercity's Battlemasters"), new GossipMenu(7527)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(3542,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1781.0f, 53.0f, 7, 7, "Undercity Mage Trainers"), new GossipMenu(3513)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1299.59f, 319.0f, 7, 7, "Undercity Paladin Trainers"), new GossipMenu(3521)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1758.33f, 401.5f, 7, 7, "Undercity Priest Trainers"), new GossipMenu(3521)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1418.56f, 65.0f, 7, 7, "Undercity Rogue Trainers"), new GossipMenu(3526)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1780.92f, 53.16f, 7, 7, "Undercity Warlock Trainers"), new GossipMenu(3526)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1775.59f, 418.19f, 7, 7, "Undercity Warrior Trainers"), new GossipMenu(3527))
                                    )
                            ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(3541,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1419.82f, 417.19f, 7, 7, "The Apothecarium"), new GossipMenu(3528)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1696.0f, 285.0f, 7, 7, "Undercity Blacksmithing Trainer"), new GossipMenu(3529)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1596.34f, 274.68f, 7, 7, "Undercity Cooking Trainer"), new GossipMenu(3530)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1488.54f, 280.19f, 7, 7, "Undercity Enchanting Trainer"), new GossipMenu(3531)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1408.58f, 143.43f, 7, 7, "Undercity Engineering Trainer"), new GossipMenu(3532)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1519.65f, 167.19f, 7, 7, "Undercity First Aid Trainer"), new GossipMenu(3533)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1679.9f, 89.0f, 7, 7, "Undercity Fishing Trainer"), new GossipMenu(3534)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1558.0f, 349.36f, 7, 7, "Undercity Herbalism Trainer"), new GossipMenu(3535)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1498.76f, 196.43f, 7, 7, "Undercity Leatherworking Trainer"), new GossipMenu(3536)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1642.88f, 335.58f, 7, 7, "Undercity Mining Trainer"), new GossipMenu(3537)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1498.6f, 196.46f, 7, 7, "Undercity Skinning Trainer"), new GossipMenu(3538)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1689.55f, 193.0f, 7, 7, "Undercity Tailoring Trainer"), new GossipMenu(3539))
                                    )
                            )
                    )
            {
                KeepOpen = true
            };
         }
    }
}