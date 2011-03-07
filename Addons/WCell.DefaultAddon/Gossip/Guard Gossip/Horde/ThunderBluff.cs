using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
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
            entry.DefaultGossip = new GossipMenu(3543,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1257.8f, 24.14f, 7, 7, "Thunder Bluff Bank"), new GossipMenu(1292)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWindRider),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1196.43f, 28.26f, 7, 7, "Wind Rider Roost"), new GossipMenu(1293)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1296.5f, 127.57f, 7 , 7, "Thunder Bluff Civic Information"), new GossipMenu(1291)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1296, 39.7f, 7, 7, "Thunder Bluff Inn"), new GossipMenu(3153)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1263.59f, 44.36f, 7, 7, "Thunder Bluff Mailbox"), new GossipMenu(3154)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1205.51f, 105.74f, 7 , 7, "Thunder Bluff Auction House"), new GossipMenu(3155)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1282.31f, 89.56f, 7, 7, "Ansekhwa"), new GossipMenu(4520)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1270.19f, 48.84f, 7, 7, "Bulrug"), new GossipMenu(5977)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1391.22f, -81.33f, 7, 7, "Thunder Bluff's Battlemasters"), new GossipMenu(7527)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(3542,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1054.47f, -285, 7, 7, "Hall of Elders"), new GossipMenu(1294)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1416.32f, -114.28f, 7, 7, "Hunter's Hall"), new GossipMenu(1295)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1061.2f, 195.5f, 7, 7, "Pools of Vision"), new GossipMenu(1296)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1061.2f, 195.5f, 7, 7, "Pools of Vision"), new GossipMenu(1297)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainShaman),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -989.54f, 278.25f, 7, 7, "Hall of Spirits"), new GossipMenu(1298)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1416.32f, -114.28f, 7, 7, "Hunter's Hall"), new GossipMenu(1299))
                                    )
                            ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(3541,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1085.56f, 27.29f, 7, 7, "Bena's Alchemy"), new GossipMenu(1332)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1239.75f, 104.88f, 7, 7, "Karn's Smithy"), new GossipMenu(1333)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1214.5f, -21.23f, 7, 7, "Aska's Kitchen"), new GossipMenu(1334)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1112.65f, 48.26f, 7, 7, "Dawnstrider Enchanters"), new GossipMenu(1335)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -996.58f, 200.5f, 7, 7, "Spiritual Healing"), new GossipMenu(1336)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1169.35f, -68.87f, 7, 7, "Mountaintop Bait & Tackle"), new GossipMenu(1337)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1137.7f, -1.51f, 7, 7, "Holistic Herbalism"), new GossipMenu(1338)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1156.22f, 66.86f, 7, 7, "Thunder Bluff Armorers"), new GossipMenu(1339)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1249.17f, 155, 7, 7, "Stonehoof Geology"), new GossipMenu(1340)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1148.56f, 51.18f, 7, 7, "Mooranta"), new GossipMenu(1343)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1156.22f, 66.86f, 7, 7, "Thunder Bluff Armorers"), new GossipMenu(1341))
                                    )
                            )
                    )
            {
                KeepOpen = true
            };
        }
    }
}