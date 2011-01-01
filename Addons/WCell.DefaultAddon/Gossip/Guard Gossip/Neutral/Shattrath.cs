using WCell.Constants;
using WCell.Addons.Default.Lang;
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
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1760.4f, 5166.9f, 7, 7, "World's End Tavern")),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABank),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1730.8f, 5496.2f, 7, 7, "Aldor Bank")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSBank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1999.6f, 5362.0f, 7, 7,"Scryers Bank"))
                                    )
                                ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1897.5f, 5767.5f, 7, 7,"Aldor Inn")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six, -2178.0f,5405.0f, 7, 7, "Scryers Inn"))
                                    )
                                ),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardFMaster), 
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1832.0f, 5299.0f, 7, 7,"Flight Master")),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1730.5f, 5496.0f, 7, 7,"Aldor Bank")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1897.5f, 5767.5f, 7, 7,"Aldor Inn")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSBank),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1997.7f, 5363.0f, 7, 7,"Scryers Bank")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSInn),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -2178.0f, 5405.0f, 7, 7, "Scryers Inn"))
                                    )
                                ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAStable),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1888.5f,5761.0f, 7, 7,"Aldor Stable")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSStable),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2170.0f,5404.0f, 7, 7,"Scryers Stable"))
                                )),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHBattle),
                            convo =>GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1774.0f,5251.0f, 7, 7,"Alliance Battlemasters")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardABattle),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1963.0f,5263.0f, 7, 7,"Horde Battlemasters")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardArena),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1960.0f, 5175.0f, 7, 7,"Arena Battlemasters"))
                                )),
                new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardManaLoom),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -2070.0f, 5265.5f, 7, 7, "Mana Loom")),
                new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardALab),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1648.5f, 5540.0f, 7, 7, "Alchemy Lab")),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGem),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAGem),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-1645.0f, 5669.5f, 7, 7,"Aldor Gem Merchant")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardSGem),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-2193.0f, 5424.5f, 7, 7,"Scryers Gem Merchant"))
                                )),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-1648.5f, 5534.0f, 7, 7,"Lorokeem")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1847.0f,5222.0f, 7, 7,"Kradu Grimblade and Zula Slagfury")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character,GossipPOIFlags.Six,-2067.4f, 5316.5f, 7, 7,"Jack Trapper")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2263.5f, 5563.5f, 7, 7,"High Enchanter Bardolan")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1591.0f, 5265.5f, 7, 7,"Mildred Fletcher")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfJewelcrafting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -1654.0f, 5667.5f, 7, 7, "Hamanar")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking), 
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six,-2060.5f, 5256.5f, 7, 7,"Darmari")),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, 2048.0f, 5300.0f, 7, 7, "Seymour"))
                                    )
                                )
                            );
        }
    }
}