using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;
using WCell.Constants.Factions;
using WCell.Core.Initialization;
using WCell.RealmServer.Items;
using WCell.Constants.Items;
using WCell.Constants;
using WCell.Util.Graphics;
using WCell.Util.Variables;
using WCell.Constants.Spells;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;

namespace WCell.Addons.Default.Teleport
{
    class ZeppelinMaster
    {
        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void SetupCustomGossips()
        {
            var SnurkBucksquickEntry = NPCMgr.GetEntry(NPCId.SnurkBucksquick);
            SnurkBucksquickEntry.DefaultGossip = StranglethornGossipMenu();
            
            var FrezzaEntry = NPCMgr.GetEntry(NPCId.Frezza);
            FrezzaEntry.DefaultGossip = BrillGossipMenu();
            
            var SquibbyOverspeckEntry = NPCMgr.GetEntry(NPCId.SquibbyOverspeck);
            SquibbyOverspeckEntry.DefaultGossip = BrillGossipMenu();

            var NezRazEntry = NPCMgr.GetEntry(NPCId.NezRaz);
            NezRazEntry.DefaultGossip = OrgrimmarGossipMenu();

            var HinDenburgEntry = NPCMgr.GetEntry(NPCId.HinDenburg);
            HinDenburgEntry.DefaultGossip = StranglethornGossipMenu();

            var ZapettaEntry = NPCMgr.GetEntry(NPCId.Zapetta);
            ZapettaEntry.DefaultGossip = StranglethornGossipMenu();

            var GrizzloweEntry = NPCMgr.GetEntry(NPCId.Grizzlowe);
            GrizzloweEntry.DefaultGossip = RatchetGossipMenu();

            var GrimbleEntry = NPCMgr.GetEntry(NPCId.Grimble);
            GrimbleEntry.DefaultGossip = BootyBayGossipMenu();
        }

        public static GossipMenu StranglethornGossipMenu()
        {
            return new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Taxi, "Do you want fly to Stranglethorn Vale?", convo =>
                {
                    convo.Character.TeleportTo(World.EasternKingdoms, new Vector3(-12407.52f, 208.3989f, 31.64571f));
                    convo.StayOpen = false;	// convo is over
                }),
                new QuitGossipMenuItem("Goodbye!")	// convo is over
				)
                {
                      // Don't close the menu, unless the user selected a final option
                      KeepOpen = true
                };
        }

        public static GossipMenu BrillGossipMenu()
        {
            return new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Taxi, "Do you want fly to Brill?", convo =>
                {
                    convo.Character.TeleportTo(World.EasternKingdoms, new Vector3(2056.241f, 236.5589f, 99.76692f));
                    convo.StayOpen = false;	// convo is over
                }),
                new QuitGossipMenuItem("Goodbye!")	// convo is over
                )
                {
                // Don't close the menu, unless the user selected a final option
                KeepOpen = true
                };
        }

        public static GossipMenu OrgrimmarGossipMenu()
        {
            return new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Taxi, "Do you want fly to Orgrimmar?", convo =>
                {
                    convo.Character.TeleportTo(World.Kalimdor, new Vector3(1360.738f, -4638.002f, 53.85339f));
                    convo.StayOpen = false;	// convo is over
                }),
                new QuitGossipMenuItem("Goodbye!")	// convo is over
                )
            {
                // Don't close the menu, unless the user selected a final option
                KeepOpen = true
            };
        }

        public static GossipMenu BootyBayGossipMenu()
        {
            return new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Taxi, "Do you want swim to Booty Bay?", convo =>
                {
                    convo.Character.TeleportTo(World.EasternKingdoms, new Vector3(-14285.23f, 557.6923f, 8.872749f));
                    convo.StayOpen = false;	// convo is over
                }),
                new QuitGossipMenuItem("Goodbye!")	// convo is over
                )
            {
                // Don't close the menu, unless the user selected a final option
                KeepOpen = true
            };
        }

        public static GossipMenu RatchetGossipMenu()
        {
            return new GossipMenu(
                new GossipMenuItem(GossipMenuIcon.Taxi, "Do you want swim to Ratchet?", convo =>
                {
                    convo.Character.TeleportTo(World.Kalimdor, new Vector3(-996.545f, -3829.761f, 5.601001f));
                    convo.StayOpen = false;	// convo is over
                }),
                new QuitGossipMenuItem("Goodbye!")	// convo is over
                )
            {
                // Don't close the menu, unless the user selected a final option
                KeepOpen = true
            };
        }
    }
}