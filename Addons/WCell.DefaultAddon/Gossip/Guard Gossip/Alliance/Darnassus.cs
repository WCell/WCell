using WCell.RealmServer.Gossips;
using WCell.Addons.Default.Lang;
using WCell.Core.Initialization;
using WCell.RealmServer.NPCs;
using WCell.Constants;
using WCell.RealmServer.Handlers;

namespace WCell.Addons.Default.Gossip.GuardGossip.Alliance
{
    class Darnassus : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void CreateDarnassusGossipMenu()
        {
            var entry = NPCMgr.GetEntry(Constants.NPCs.NPCId.DarnassusSentinel);
             entry.DefaultGossip = new GossipMenu(3016,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9858.326f,2340.811f, 7, 7, "Darnassus Auction House"), new GossipMenu(3833)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9936.607f,2516.145f, 7, 7,"Darnassus Bank"), new GossipMenu(3017)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHippogryph),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9945.578f,2618.262f, 7, 7,"Rut'theran Village"), new GossipMenu(3018)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10076.19f,2199.77f, 7, 7,"Darnassus Guild Master"), new GossipMenu(3019)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10127.67f,2224.849f, 7, 7,"Darnassus Inn"), new GossipMenu(3020)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9941.858f,2497.169f, 7, 7,"Darnassus Mailbox"), new GossipMenu(3021)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10167.46f, 2522.8f, 7, 7,"Alassin"), new GossipMenu(5980)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9908.618f,2329.354f, 7, 7,"Ilyenia Moonfire"), new GossipMenu(4517)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 9975.605f,2324.583f, 7, 7,"Battlemasters Darnassus"), new GossipMenu(7519)),

                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(4264,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10186.02f, 2570.437f, 7, 7,"Darnassus Druid Trainer"), new GossipMenu(3024)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 10178.57f,2510.957f, 7, 7,"Darnassus Hunter Trainer"), new GossipMenu(3023)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 9664.587f,2528.172f, 7, 7,"Darnassus Mage Trainer")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 9964.599f,2529.992f, 7, 7,"Darnassus Paladin Trainer")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 9667.163f,2532.749f, 7, 7,"The Temple The Moon"), new GossipMenu(3025)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainRogue),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 10124f,2592.819f, 7, 7,"Darnassus Rogue Trainer"), new GossipMenu(3026)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, 9940.102f,2284.522f, 7, 7,"Warrior's Terrace"), new GossipMenu(3033))
                                    )
                            ),

                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(4273,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10068.18f, 2357.79f, 7, 7,"Darnassus Alchemy Trainer"), new GossipMenu(3035)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10088.59f, 2419.885f, 7, 7,"Darnassus Cooking Trainer"), new GossipMenu(3036)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10145.06f, 2321.341f, 7, 7,"Darnassus Enchanting Trainer"), new GossipMenu(3337)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10151.97f, 2391.253f, 7, 7,"Darnassus First Aid Trainer"), new GossipMenu(3037)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,9838.131f, 2430.753f, 7, 7,"Darnassus Fishing Trainer"), new GossipMenu(3038)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,9758.695f, 2431.348f, 7, 7,"Darnassus Herbalism Trainer"), new GossipMenu(3039)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfInscription),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10139.04f, 2312.588f, 7, 7,"Darnassus Inscription"), new GossipMenu(13886)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10085.3f, 2257.551f, 7, 7,"Darnassus Leatherworking Trainer"), new GossipMenu(3040)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10080.58f, 2260.167f, 7, 7,"Darnassus Skinning Trainer"), new GossipMenu(3042)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,10080.7f, 2267.028f, 7, 7,"Darnassus Tailor"), new GossipMenu(3044))
                                    )
                            ),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLexicon),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 10146.09f, 2313.42f, 7, 7, "Lexicon of Power"), new GossipMenu(14174))
                    );
        }
    }
}
