using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Gossip.GuardGossip.Neutral
{
    class Shattrath : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void LoadShattrathMenu()
        {
            CreateShattrathGossipMenu(19687);
            CreateShattrathGossipMenu(18568);
            CreateShattrathGossipMenu(18549);
        }
        public static void CreateShattrathGossipMenu(uint npcID)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcID);

            entry.DefaultGossip = new GossipMenu(
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardTavern),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1760.4f, 5166.9f, 7, 7, "World's End Tavern"), new GossipMenu(10394)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    new GossipMenu(10395,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABank),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1730.8f, 5496.2f, 7, 7, "Aldor Bank"), new GossipMenu(10396)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSBank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1999.6f, 5362.0f, 7, 7,"Scryers Bank"), new GossipMenu(10397))
                                    )
                                ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    new GossipMenu(10398,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1897.5f, 5767.5f, 7, 7,"Aldor Inn"), new GossipMenu(10399)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -2178.0f,5405.0f, 7, 7, "Scryers Inn"), new GossipMenu(10401))
                                    )
                                ),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardFMaster), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1832.0f, 5299.0f, 7, 7,"Flight Master"), new GossipMenu(10402)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    new GossipMenu(10403,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1730.5f, 5496.0f, 7, 7,"Aldor Bank"), new GossipMenu(10396)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1897.5f, 5767.5f, 7, 7,"Aldor Inn"), new GossipMenu(10399)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSBank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1997.7f, 5363.0f, 7, 7,"Scryers Bank"), new GossipMenu(10397)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -2178.0f, 5405.0f, 7, 7, "Scryers Inn"), new GossipMenu(10401))
                                    )
                                ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    new GossipMenu(10404,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAStable),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1888.5f,5761.0f, 7, 7,"Aldor Stable"), new GossipMenu(10399)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSStable),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2170.0f,5404.0f, 7, 7,"Scryers Stable"), new GossipMenu(10401))
                                )),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    new GossipMenu(10405,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHBattle),
                            convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1774.0f,5251.0f, 7, 7,"Alliance Battlemasters"), new GossipMenu(10406)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABattle),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1963.0f,5263.0f, 7, 7,"Horde Battlemasters"), new GossipMenu(10407)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardArena),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1960.0f, 5175.0f, 7, 7,"Arena Battlemasters"), new GossipMenu(10407))
                                )),
                new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardManaLoom),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -2070.0f, 5265.5f, 7, 7, "Mana Loom"), new GossipMenu(10408)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardALab),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1648.5f, 5540.0f, 7, 7, "Alchemy Lab"), new GossipMenu(10409)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGem),
                    new GossipMenu(10410,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAGem),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1645.0f, 5669.5f, 7, 7,"Aldor Gem Merchant"), new GossipMenu(10411)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSGem),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-2193.0f, 5424.5f, 7, 7,"Scryers Gem Merchant"), new GossipMenu(10412))
                                )),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(10391,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1648.5f, 5534.0f, 7, 7,"Lorokeem"), new GossipMenu(10413)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1847.0f,5222.0f, 7, 7,"Kradu Grimblade and Zula Slagfury"), new GossipMenu(10400)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-2067.4f, 5316.5f, 7, 7,"Jack Trapper"), new GossipMenu(10414)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2263.5f, 5563.5f, 7, 7,"High Enchanter Bardolan"), new GossipMenu(10415)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1591.0f, 5265.5f, 7, 7,"Mildred Fletcher"), new GossipMenu(10416)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfJewelcrafting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1654.0f, 5667.5f, 7, 7, "Hamanar"), new GossipMenu(10417)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2060.5f, 5256.5f, 7, 7,"Darmari"), new GossipMenu(10418)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2048.0f, 5300.0f, 7, 7, "Seymour"), new GossipMenu(10419))
                                    )
                                )
                            )
            {
                KeepOpen = true
            };
        }
    }
}