using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
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
            entry.DefaultGossip = new GossipMenu(2593,		// "What are you looking for"?
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1631.0f, -4375.0f, 7, 7, "Bank of Orgrimmar"), new GossipMenu(2554)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWindRider),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1676.0f, -4332.0f, 7, 7, "The Sky Tower"), new GossipMenu(2555)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1576.0f, -4294.0f, 7, 7, "Horde Embassy"), new GossipMenu(2556)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1576.0f, -4294.0f, 7, 7, "Horde Embassy"), new GossipMenu(2557)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1622.0f, -4388.0f, 7, 7, "Orgrimmar Mailbox"), new GossipMenu(2558)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1679.0f, -4450.0f, 7, 7, "Orgrimmar Auction House"), new GossipMenu(3075)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardZepplin),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1337.0f, -4632.0f, 7, 7, "Orgrimmar Zeppelin Tower"), new GossipMenu(3173)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2092.0f, -4823.0f, 7, 7, "Sayoc & Hanashi"), new GossipMenu(4519)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2133.0f, -4663.0f, 7, 7, "Xon'cha"), new GossipMenu(5974)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Tabard, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardOfficer),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1633.0f, -4249.0f, 7, 7, "Hall of Legends"), new GossipMenu(7046)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1983.0f, -4794.0f, 7, 7, "Hall of the Brave"), new GossipMenu(7521)),

                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
					new GossipMenu(2599,		// "Which trainer do you seek?"
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2114.0f, -4625.0f, 7, 7, "Orgrimmar Hunter's Hall"), new GossipMenu(2559)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1451.0f, -4223.0f, 7, 7, "Darkbriar Lodge"), new GossipMenu(2560)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1906.0f, -4134.0f, 7, 7, "Valley of Wisdom"), new GossipMenu(2566)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1442.0f, -4183.0f, 7, 7, "Spirit Lodge"), new GossipMenu(2561)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1773.0f, -4278.0f, 7, 7, "Shadowswift Brotherhood"), new GossipMenu(2563)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainShaman),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1925.0f, -4181.0f, 7, 7, "Thrall's Fortress"), new GossipMenu(2562)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1849.0f, -4359.0f, 7, 7, "Darkfire Enclave"), new GossipMenu(2564)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1983.0f, -4794.0f, 7, 7, "Hall of the Brave"), new GossipMenu(2565))
                                    )
                            ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(2594,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1955.0f, -4475.0f, 7, 7, "Yelmak's Alchemy and Potions"), new GossipMenu(2497)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2054.0f, -4831.0f, 7, 7, "The Burning Anvil"), new GossipMenu(2499)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1780.0f, -4481.0f, 7, 7, "Borstan's Firepit"), new GossipMenu(2500)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1917.0f, -4434.0f, 7, 7, "Godan's Runeworks"), new GossipMenu(2501)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2038.0f, -4744.0f, 7, 7, "Nogg's Machine Shop"), new GossipMenu(2653)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1485.0f, -4160.0f, 7, 7, "Survival of the Fittest"), new GossipMenu(2502)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1994.0f, -4655.0f, 7, 7, "Lumak's Fishing"), new GossipMenu(2503)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1898.0f, -4454.0f, 7, 7, "Jandi's Arboretum"), new GossipMenu(2504)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1852.0f, -4562.0f, 7, 7, "Kodohide Leatherworkers"), new GossipMenu(2513)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2029.0f, -4704.0f, 7, 7, "Red Canyon Mining"), new GossipMenu(2515)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1852.0f, -4562.0f, 7, 7, "Kodohide Leatherworkers"), new GossipMenu(2516)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 1802.0f, -4560.0f, 7, 7, "Magar's Cloth Goods"), new GossipMenu(2518))
                                    )
                            )
                    )
            {
                KeepOpen = true
            };
        }
    }
}
