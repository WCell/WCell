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
            entry.DefaultGossip = new GossipMenu(2760,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4597.39f, -911.6f, 7, 7, "Ironforge Auction House"), new GossipMenu(3014)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardIronforge), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4891.9f, -991.47f, 7, 7, "The Vault"),new GossipMenu(2761)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardTram), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4835.27f, -1294.69f, 7, 7, "The Deeprun Tram"), new GossipMenu(3814)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGryphon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4821.52f, -1152.3f, 7, 7, "Ironforge Gryphon Master"), new GossipMenu(2762)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5021f, -996.45f, 7, 7, "Ironforge Visitor's Center"), new GossipMenu(2764)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4850.47f, -872.57f, 7, 7, "Stonefire Tavern"), new GossipMenu(2768)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4845.7f, -880.55f, 7, 7, "Ironforge Mailbox"), new GossipMenu(2769)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5010f, -1262f, 7, 7, "Ulbrek Firehand"), new GossipMenu(5986)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5040f, -1201.88f, 7, 7, "Bixi and Buliwyf"), new GossipMenu(4518)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5047.87f, -1263.77f, 7, 7, "Battlemasters Ironforge"), new GossipMenu(10216)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(2766,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023f, -1253.68f, 7, 7, "Hall of Arms"), new GossipMenu(2770)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries"), new GossipMenu(2771)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627.02f, -926.45f, 7, 7, "Hall of Mysteries"), new GossipMenu(2773)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4627f, -926.45f, 7, 7, "Hall of Mysteries"), new GossipMenu(2772)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4647.83f, -1124f, 7, 7, "Ironforge Rogue Trainer"), new GossipMenu(2774)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainShaman), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4722.02f, -1150.66f, 7, 7, "Ironforge Shaman Trainer"), new GossipMenu(10842)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarlock), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4605f, -1110.45f, 7, 7, "Ironforge Warlock Trainer"), new GossipMenu(2775)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -5023.08f, -1253.68f, 7, 7, "Hall of Arms"), new GossipMenu(2776))
                                    )
                            ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(2793,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4858.5f, -1241.83f, 7, 7, "Berryfizz's Potions and Mixed Drinks"), new GossipMenu(2794)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4796.97f, -1110.17f, 7, 7, "The Great Forge"), new GossipMenu(2795)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4767.83f, -1184.59f, 7, 7, "The Bronze Kettle"), new GossipMenu(2796)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4803.72f, -1196.53f, 7, 7, "Thistlefuzz Arcanery"), new GossipMenu(2797)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4799.56f, -1250.23f, 7, 7, "Springspindle's Gadgets"), new GossipMenu(2798)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.6f, -1153.13f, 7, 7, "Ironforge Physician"), new GossipMenu(2799)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4597.91f, -1091.93f, 7, 7, "Traveling Fisherman"), new GossipMenu(2800)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4881.9f, -1151.92f, 7, 7, "Ironforge Physician"), new GossipMenu(2801)),
                            new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfInscription),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4801.72f, -1189.41f, 7, 7, "Ironforge Inscription"), new GossipMenu(13884)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods"), new GossipMenu(2802)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4705.06f, -1116.43f, 7, 7, "Deepmountain Mining Guild"), new GossipMenu(2804)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4745f, -1027.57f, 7, 7, "Finespindle's Leather Goods"), new GossipMenu(2805)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4719.60f, -1057f, 7, 7, "Stonebrow's Clothier"), new GossipMenu(2807))
                                 )),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLexicon),
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4801.72f, -1189.41f, 7, 7, "Ironforge Lexicon of Power"), new GossipMenu(14174))
                    );
        }
    }
}