using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AI.Actions.Combat;
using System;
using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///

namespace WCell.Addons.Default.Instances
{
	public class UtgardeKeep : DungeonInstance
	{
        #region Setup Content

        
        private static NPCEntry dragonflayerIronhelm;


        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void InitNPCs()
        {
            // Dragonflayer Ironhelm
            dragonflayerIronhelm = NPCMgr.GetEntry(NPCId.DragonflayerIronhelm);

            dragonflayerIronhelm.AddSpell(SpellId.HeroicStrike_9);
            SpellHandler.Apply(spell => { spell.CooldownTime = 5000; },
                SpellId.HeroicStrike_9);
        }


        #endregion

    }

}