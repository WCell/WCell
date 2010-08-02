using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants;

namespace WCell.Addons.Default.Spells.Rogue
{
    public static class RogueCombatFixes
    {
        [Initialization(InitializationPass.Second)]
        public static void FixRogue()
        {
        }
    }
}
