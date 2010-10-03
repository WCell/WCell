using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.GameObjects;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Handlers;

namespace WCell.Addons.Default.GameObjects
{
    public static class GameObjectFixes
    {
        [Initialization]
        [DependentInitialization(typeof(GOMgr))]
        public static void InitGOs()
        {
           //Add your GO fixes here
           //View the GO API for more info
        }

    }
}
