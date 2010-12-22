using WCell.RealmServer.NPCs;
using WCell.Addons.Default.Lang;
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
        public static void LoadStormwindMenu()
        {
            CreateStormwindGossipMenu(68);
            CreateStormwindGossipMenu(1976);
        }
        public static void CreateStormwindGossipMenu(uint npcID)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcID);
            entry.DefaultGossip = new GossipMenu(
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8815.65f, 665.4402f, 7, 7, "Stormwind Auction House")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStormwind), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4891.9f, -991.47f, 7, 7, "Stormwind Bank")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHarbor), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8577.078f, 988.1891f, 7, 7, "Stormwind Harbor")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardTram), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8391.07f, 570.8477f, 7, 7, "The Deeprun Tram")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8861.762f, -664.0831f, 7, 7, "The Gilded Rose")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGryphon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8837.699f, -487.6247f, 7, 7, "Stormwind Gryphon Master")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8886.755f, -600.606f, 7, 7, "Stormwind Visitor's Center")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLock), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8424.989f, -627.3555f, 7, 7, "Stormwind Locksmith")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8432.932f, -554.4769f, 7, 7, "Jenova Stoneshield")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8796.248f, -613.0318f, 7, 7, "Woo Ping")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Tabard, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardOfficer), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8767.027f, -409.2762f, 7, 7, "Champions' Hall")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8393.95f,-270.5327f, 7, 7, "Battlemasters Stormwind")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBarber), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8746.796f, -658.101f, 7, 7, "Stormwind Barber")),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023f, -1253.68f, 7, 7, "Hall of Arms")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627.02f, -926.45f, 7, 7, "Hall of Mysteries")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4647.83f, -1124f, 7, 7, "Ironforge Rogue Trainer")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4605f, -1110.45f, 7, 7, "Ironforge Warlock Trainer")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023.08f, -1253.68f, 7, 7, "Hall of Arms"))
                                    )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4858.5f, -1241.83f, 7, 7, "Berryfizz's Potions and Mixed Drinks")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4796.97f, -1110.17f, 7, 7, "The Great Forge")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4767.83f, -1184.59f, 7, 7, "The Bronze Kettle")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4803.72f, -1196.53f, 7, 7, "Thistlefuzz Arcanery")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4799.56f, -1250.23f, 7, 7, "Springspindle's Gadgets")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.6f, -1153.13f, 7, 7, "Ironforge Physician")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4597.91f, -1091.93f, 7, 7, "Traveling Fisherman")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.9f, -1151.92f, 7, 7, "Ironforge Physician")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4705.06f, -1116.43f, 7, 7, "Deepmountain Mining Guild")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4719.60f, -1057f, 7, 7, "Stonebrow's Clothier"))
                                    )
                    ));           
        }
    }
}