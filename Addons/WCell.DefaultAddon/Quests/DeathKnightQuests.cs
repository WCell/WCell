using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core.Initialization;
using WCell.RealmServer.Quests;

namespace WCell.Addons.Default.Quests
{
    public static class DeathKnightQuests
    {
        [Initialization]
        [DependentInitialization(typeof(QuestMgr))]
        public static void FixIt()
        {
        }
    }
}
