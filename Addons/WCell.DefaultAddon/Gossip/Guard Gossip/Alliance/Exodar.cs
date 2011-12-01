using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Gossip.Guard_Gossip.Alliance
{
    class Exodar : GossipMenu
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void LoadExodarMenu()
        {
            CreateExodarGossipMenu(16733);
            CreateExodarGossipMenu(20674);
        }
        public static void CreateExodarGossipMenu(uint npcID)
        {
            NPCEntry entry = NPCMgr.GetEntry(npcID);
            entry.DefaultGossip = new GossipMenu(9551,
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardAuction),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4013.82f, -11729.64f, 7, 7, "Exodar Auctioneer"), new GossipMenu(9528)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBank),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3923.89f, -11544.5f, 7, 7, "Exodar Bank"), new GossipMenu(9529)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardHippogryph),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4058.45f, -11789.7f, 7, 7, "Exodar Hippogryph Master"), new GossipMenu(9530)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Guild, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardGuild),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4093.38f, -11630.39f, 7, 7, "Exodar Guild Master"), new GossipMenu(9539)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Bind, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardInn),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3765.34f, -11695.8f, 7, 7, "Exodar Inn"), new GossipMenu(9545)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardMail),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3913.75f, -11606.83f, 7, 7, "Exodar Mailbox"), new GossipMenu(10254)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Taxi, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardStable),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3787.01f, -11702.7f, 7, 7, "Exodar Stable Master"), new GossipMenu(9558)),
                new MultiStringGossipMenuItem(GossipMenuIcon.Resurrect, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardWeapon),
                    convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4215.68f, -11628.9f, 7, 7, "Exodar Weapon Master"), new GossipMenu(9565)),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                    new GossipMenu(9531,
                                       new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardBattle),
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3999.82f, -11368.33f, 7, 7, "Exodar Battlemasters")),
                                       new MultiStringGossipMenuItem(GossipMenuIcon.Battlefield, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardArena),
                                           convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3725.25f, -11688.3f, 7, 7, "Arena Battlemaster"))
                                   )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardClass),
                    new GossipMenu(9533,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainDruid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4274.81f, -11495.3f, 7, 7, "Exodar Druid Trainer"),  new GossipMenu(9534)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainHunter),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4229.36f, -11563.36f, 7, 7, "Exodar Hunter Trainers"), new GossipMenu(9544)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainMage),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4048.8f, -11559.02f, 7, 7, "Exodar Mage Trainers"), new GossipMenu(9550)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPaladin),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4176.57f, -11476.46f, 7, 7, "Exodar Paladin Trainers"), new GossipMenu(9553)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainPriest),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3972.38f, -11483.2f, 7, 7, "Exodar Priest Trainers"), new GossipMenu(9554)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainShaman),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3843.8f, -11390.75f, 7, 7, "Exodar Shaman Trainer"), new GossipMenu(9556)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.TrainWarrior),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4191.11f, -11650.45f, 7, 7, "Exodar Warrior Trainers"), new GossipMenu(9562))
                                  )
                    ),
                new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardProfession),
                    new GossipMenu(9555,
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfAlchemy),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4042.37f, -11366.3f, 7, 7, "Exodar Alchemist Trainers"), new GossipMenu(9527)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfBlacksmithing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4232.4f, -11705.23f, 7, 7, "Exodar Blacksmithing Trainers"), new GossipMenu(9532)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfCooking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3799.69f, -11650.51f, 7, 7, "Exodar Cook"), new GossipMenu(9559)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEnchanting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Enchanters"), new GossipMenu(9535)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfEngineering),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4257.93f, -11636.53f, 7, 7, "Exodar Engineering"), new GossipMenu(9536)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFirstAid),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3766.05f, -11481.8f, 7, 7, "Exodar First Aid Trainer"), new GossipMenu(9537)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfFishing),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3726.64f, -11384.43f, 7, 7, "Exodar Fishing Trainer"), new GossipMenu(9538)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfHerbalism),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4052.5f, -11356.6f, 7, 7, "Exodar Herbalism Trainer"), new GossipMenu(9543)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfInscription),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Exodar Inscription"), new GossipMenu(13887)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfJewelcrafting),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3786.27f, -11541.33f, 7, 7, "Exodar Jewelcrafters"), new GossipMenu(9547)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfLeatherworking),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.42f, -11772.93f, 7, 7, "Exodar Leatherworking"), new GossipMenu(9549)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfMining),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4220.31f, -11694.29f, 7, 7, "Exodar Mining Trainers"), new GossipMenu(9552)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfSkinning),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4134.97f, -11760.5f, 7, 7, "Exodar Skinning Trainer"), new GossipMenu(9557)),
                        new MultiStringGossipMenuItem(GossipMenuIcon.Train, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.ProfTailoring),
                            convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -4095.78f, -11746.9f, 7, 7, "Exodar Tailors"), new GossipMenu(9350))
                                 )
                    ),
                    new MultiStringGossipMenuItem(GossipMenuIcon.Talk, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.GuardLexicon),
                        convo => GossipHandler.SendGossipPOI(convo.Character, GossipPOIFlags.Six, -3889.3f, -11495, 7, 7, "Lexicon of Power"), new GossipMenu(14174))
                    );
        }
    }
}
