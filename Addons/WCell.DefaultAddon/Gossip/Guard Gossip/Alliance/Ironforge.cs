using WCell.RealmServer.NPCs;
using WCell.Core.Initialization;
using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Alliance
{
    class Ironforge : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateIronforgeGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.IronforgeGuard);
            entry.DefaultGossip = new GossipMenu(
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4597.39f, -911.6f, 7, 7, "Ironforge Auction House")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardIronforge), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4891.9f, -991.47f, 7, 7, "The Vault")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardTram), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4835.27f, -1294.69f, 7, 7, "The Deeprun Tram")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGryphon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4821.52f, -1152.3f, 7, 7, "Ironforge Gryphon Master")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5021f, -996.45f, 7, 7, "Ironforge Visitor's Center")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4850.47f, -872.57f, 7, 7, "Stonefire Tavern")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4845.7f, -880.55f, 7, 7, "Ironforge Mailbox")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5010f, -1262f, 7, 7, "Ulbrek Firehand")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5040f, -1201.88f, 7, 7, "Bixi and Buliwyf")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5047.87f, -1263.77f, 7, 7, "Battlemasters Ironforge")),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8737.481f, -1058.529f, 7, 7, "The Park")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -8423.877f, -552.1997f, 7, 7, "Hunter Lodge")),
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