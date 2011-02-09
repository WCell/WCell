using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Events
{
    class DarkmoonFaire
    {
        public const GOEntryId Blastenheimer5000ElwynnId = GOEntryId.Blastenheimer5000UltraCannon;
        public const GOEntryId Blastenheimer5000TerokkarId = GOEntryId.Blastenheimer5000UltraCannon_2;
        public const GOEntryId Blastenheimer5000MulgoreId = GOEntryId.Blastenheimer5000UltraCannon_3;
        public static SpellId MagicWingsId = SpellId.MagicWings_2;
        public static SpellId CannonPrepId = SpellId.CannonPrep_3;

        public static Vector3 ElwynnTelePosition = new Vector3(-9569.15f, -14.75f, 68.05f);
        public const float ElwynnTeleOrientation = 4.87f;
        public static Vector3 TerokkarTelePosition = new Vector3(-1742.64f, 5454.71f, -7.92f);
        public const float TerokkarTeleOrientation = 4.60f;
        public static Vector3 MulgoreTelePosition = new Vector3(-1326.71f, 86.30f, 133.09f);
        public const float MulgoreTeleOrientation = 3.51f;

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
            go.PlaySound(8476);
            

            //Everything beyond this should really happen after around two seconds
            //see FireCannon method

            switch(go.EntryId)
            {
                case (uint)Blastenheimer5000ElwynnId:
                    {
                        user.TeleportTo(ElwynnTelePosition, ElwynnTeleOrientation);
                    }break;
                case (uint)Blastenheimer5000TerokkarId:
                    {
                        user.TeleportTo(TerokkarTelePosition, TerokkarTeleOrientation);
                    }break;
                case (uint)Blastenheimer5000MulgoreId:
                    {
                        user.TeleportTo(MulgoreTelePosition, MulgoreTeleOrientation);
                    }break;
                default:
                    {
                        user.TeleportTo(go);
                    }break;
            }

            user.DecMechanicCount(SpellMechanic.Rooted);
            cast = user.SpellCast;
            cast.Start(MagicWingsId);
            return true;
        }

        public static void FireCannon(GameObject go, Character user)
        {
            /*
            switch(go.EntryId)
            {
                case (uint)Blastenheimer5000ElwynnId:
                    {
                        user.TeleportTo(ElwynnTelePosition, ElwynnTeleOrientation);
                    }break;
                case (uint)Blastenheimer5000TerokkarId:
                    {
                        user.TeleportTo(TerokkarTelePosition, TerokkarTeleOrientation);
                    }break;
                case (uint)Blastenheimer5000MulgoreId:
                    {
                        user.TeleportTo(MulgoreTelePosition, MulgoreTeleOrientation);
                    }break;
                default:
                    {
                        user.TeleportTo(go);
                    }break;
            }

            user.DecMechanicCount(SpellMechanic.Rooted);
            cast = user.SpellCast;
            cast.Start(MagicWingsId);
            */
        }
    }
}
