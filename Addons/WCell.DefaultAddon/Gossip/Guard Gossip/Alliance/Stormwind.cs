using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Gossip.GuardGossip.Alliance
{
    class Stormwind : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void LoadStormwindMenu()
        {
            CreateStormwindGossipMenu(68);
            CreateStormwindGossipMenu(1976);
        }
        public static void CreateStormwindGossipMenu(uint npcID)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcID);
            entry.DefaultGossip = new GossipMenu(933,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8815.65f, 665.4402f, 7, 7, "Stormwind Auction House"), new GossipMenu(3834)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStormwind), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8811.46f, 622.87f, 7, 7, "Stormwind Bank"), new GossipMenu(764)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHarbor), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8577.078f, 988.1891f, 7, 7, "Stormwind Harbor"), new GossipMenu(13439)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardTram), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8391.07f, 570.8477f, 7, 7, "The Deeprun Tram"), new GossipMenu(3813)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8861.762f, 664.0831f, 7, 7, "The Gilded Rose"), new GossipMenu(3860)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGryphon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8837.699f, 487.6247f, 7, 7, "Stormwind Gryphon Master"), new GossipMenu(879)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8886.755f, 600.606f, 7, 7, "Stormwind Visitor's Center"), new GossipMenu(882)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLock), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8424.989f, 627.3555f, 7, 7, "Stormwind Locksmith")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8432.932f, 554.4769f, 7, 7, "Jenova Stoneshield"), new GossipMenu(5984)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8796.248f, 613.0318f, 7, 7, "Woo Ping"), new GossipMenu(5984)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Tabard, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardOfficer), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8767.027f, 409.2762f, 7, 7, "Champions' Hall"), new GossipMenu(7047)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8393.95f,270.5327f, 7, 7, "Battlemasters Stormwind"), new GossipMenu(10218)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBarber), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8746.796f, 658.101f, 7, 7, "Stormwind Barber"), new GossipMenu(13882)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(898,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8751.0f, 1124.5f, 7, 7, "The Park"), new GossipMenu(902)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8413.0f, 541.5f, 7, 7, "Hunter Lodge"), new GossipMenu(905)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -9012.0f, 867.6f, 7, 7, "Wizard`s Sanctum"), new GossipMenu(899)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8577.0f, 881.7f, 7, 7, "Cathedral Of Light"), new GossipMenu(904)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8512.0f, 862.4f, 7, 7, "Cathedral Of Light"), new GossipMenu(903)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8753.0f, 367.8f, 7, 7, "Stormwind Rogue House"), new GossipMenu(900)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainShaman),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -9031.54f, 549.87f, 7, 7, "Farseer Umbrua"), new GossipMenu(10106)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8948.91f, 998.35f, 7, 7, "The Slaughtered Lamb"), new GossipMenu(906)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8714.14f, 334.96f, 7, 7, "Stormwind Barracks"), new GossipMenu(901))
                                    )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(918,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8988.0f, 759.60f, 7, 7, "Alchemy Needs"), new GossipMenu(919)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8424.0f, 616.9f, 7, 7, "Therum Deepforge"), new GossipMenu(920)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8611.0f, 364.6f, 7, 7, "Pig and Whistle Tavern"), new GossipMenu(921)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8858.0f, 803.7f, 7, 7, "Lucan Cordell"), new GossipMenu(941)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8347.0f, 644.1f, 7, 7, "Lilliam Sparkspindle"), new GossipMenu(922)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8513.0f, 801.8f, 7, 7, "Shaina Fuller"), new GossipMenu(923)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8803.0f, 767.5f, 7, 7, "Arnold Leland"), new GossipMenu(940)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8967.0f, 779.5f, 7, 7, "Ironforge Physician"), new GossipMenu(924)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfInscription),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8853.33f, 857.66f, 7, 7, "Stormwind Inscription"), new GossipMenu(13881)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8726.0f, 477.4f, 7, 7, "The Protective Hide"), new GossipMenu(925)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8434.0f, 692.8f, 7, 7, "Gelman Stonehand"), new GossipMenu(927)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8716.0f, 469.4f, 7, 7, "The Protective Hide"), new GossipMenu(928)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8938.0f, 800.7f, 7, 7, "Duncan`s Textiles"), new GossipMenu(929))
                                    )
                    ));           
        }
    }
}