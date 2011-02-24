using System;
using System.Linq;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Events
{
    class DarkmoonFaireBlastenheimer
    {
        public const GOEntryId Blastenheimer5000ElwynnId = GOEntryId.Blastenheimer5000UltraCannon;
        public const GOEntryId Blastenheimer5000TerokkarId = GOEntryId.Blastenheimer5000UltraCannon_2;
        public const GOEntryId Blastenheimer5000MulgoreId = GOEntryId.Blastenheimer5000UltraCannon_3;
        public static SpellId MagicWingsId = SpellId.MagicWings_2;
        public static SpellId CannonPrepId = SpellId.CannonPrep_3;

        public static Vector3 ElwynnTelePosition = new Vector3(-9571.3f, -18.8353f, 70.05f);
        public static float ElwynnTeleOrientation = 4.90124f;
        public static Vector3 TerokkarTelePosition = new Vector3(-1742.18f, 5457.85f, -7.92f);
        public static float TerokkarTeleOrientation = -1.71042f;
        public static Vector3 MulgoreTelePosition = new Vector3(-1325.26f, 86.7761f, 133.09f);
        public static float MulgoreTeleOrientation = 3.48324f;

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void RegisterEvents()
        {
            var blastenheimer5000Elwynn = GOMgr.GetEntry(Blastenheimer5000ElwynnId) as GOGooberEntry;
            var blastenheimer5000Terokkar = GOMgr.GetEntry(Blastenheimer5000TerokkarId) as GOGooberEntry;
            var blastenheimer5000Mulgore = GOMgr.GetEntry(Blastenheimer5000MulgoreId) as GOGooberEntry;

            if (blastenheimer5000Elwynn != null) blastenheimer5000Elwynn.Used += Blastenheimer5000Used;
            if (blastenheimer5000Terokkar != null) blastenheimer5000Terokkar.Used += Blastenheimer5000Used;
            if (blastenheimer5000Mulgore != null) blastenheimer5000Mulgore.Used += Blastenheimer5000Used;
        }

        private static bool Blastenheimer5000Used(GameObject go, Character user)
        {
            var cast = user.SpellCast;
            cast.Start(CannonPrepId);
            user.IncMechanicCount(SpellMechanic.Rooted);
            switch (go.EntryId)
            {
                case (uint)Blastenheimer5000ElwynnId:
                    {
                        user.TeleportTo(ElwynnTelePosition, ElwynnTeleOrientation);
                    } break;
                case (uint)Blastenheimer5000TerokkarId:
                    {
                        user.TeleportTo(TerokkarTelePosition, TerokkarTeleOrientation);
                    } break;
                case (uint)Blastenheimer5000MulgoreId:
                    {
                        user.TeleportTo(MulgoreTelePosition, MulgoreTeleOrientation);
                    } break;
                default:
                    {
                        user.DecMechanicCount(SpellMechanic.Rooted);
                        return false;
                    } break;
            }
            go.PlaySound(8476);
            
            user.CallDelayed(2000, obj => FireCannon(user));
            return true;
        }

        public static void FireCannon(Character user)
        {
            user.DecMechanicCount(SpellMechanic.Rooted);
            var cast = user.SpellCast;
            if (cast != null)
                cast.TriggerSelf(MagicWingsId);
        }
    }

    class DarkmoonFaireSteamTonks
    {
        public static GOEntryId TonkControlConsoleGOEntryId = GOEntryId.TonkControlConsole;
        public static NPCId SteamTonkNPCId = NPCId.DarkmoonSteamTonk;
        public static SpellId SummonTonkSpellId = SpellId.SummonRCTonk;
        public static SpellId UseTonkSpellId = SpellId.UsingSteamTonkController;
        public static SpellId[] NormalTonkSpells = { SpellId.Cannon, SpellId.Mortar, SpellId.NitrousBoost };
        public static SpellId[] SpecialTonkSpells = {SpellId.DropMine, SpellId.ActivateMGTurret, SpellId.Flamethrower, SpellId.ShieldGenerator};

        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void RegisterEvents()
        {
            var consoleEntry = GOMgr.GetEntry(TonkControlConsoleGOEntryId);
            if (consoleEntry == null) return;
            consoleEntry.Used += TonkConsoleUsed;
            consoleEntry.Activated += TonkConsoleActivated;
        }

        private static void TonkConsoleActivated(GameObject obj)
        {
            obj.State = GameObjectState.Enabled;
        }

        private static bool TonkConsoleUsed(GameObject go, Character user)
        {
            go.State = GameObjectState.Disabled;

            var tonkEntry = NPCMgr.GetEntry(SteamTonkNPCId);
            var tonk = tonkEntry.SpawnAt(user);
            tonk.Summoner = user;
            
            //Remove any of the special abilities from the tonk entry
            //and add a random one
            foreach (var tonkSpell in SpecialTonkSpells.Where(tonkSpell => tonkEntry.Spells.ContainsKey(tonkSpell)))
            {
                tonkEntry.Spells.Remove(tonkSpell);
            }
            var rand = new Random();
            tonkEntry.AddSpell(SpecialTonkSpells[rand.Next(0,3)]);

            var cast = user.SpellCast;
            if (cast == null)
            {
                go.State = GameObjectState.Enabled;
                return false;
            }

            if(cast.Start(UseTonkSpellId, false, tonk, user) != SpellFailedReason.Ok)
            {
                go.State = GameObjectState.Enabled;
                return false;
            }
            return true;
        }

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            var tonkEntry = NPCMgr.GetEntry(SteamTonkNPCId);
            if (tonkEntry == null) return;
            tonkEntry.Died += ResetTonk;
            tonkEntry.AddSpells(NormalTonkSpells);
        }

        private static void ResetTonk(NPC npc)
        {
            if (npc.IsAlive)
            {
                //kill it and return since this should
                //fire the event again
                npc.Kill();
                return;
            }

            var user = npc.Summoner;
            if (user != null && user.IsInWorld)
            {
                var cast = user.SpellCast;
                if(cast != null)
                    cast.TriggerSelf(SpellId.Stun_2);

                var go = user.Map.GetNearestGameObject(user.Position, TonkControlConsoleGOEntryId);
                if (go != null)
                {
                    go.State = GameObjectState.Enabled;
                }
            }

            
            npc.RemoveFromMap();

        }
    }
}
