using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Tools.Ralek;

namespace WCell.Tools.Commands
{
    public class GODumpCommand : ToolCommand
    {
        protected override void Initialize()
        {
            Init("GODump");
            EnglishDescription = "Dumps the gameobject entry structures to gotypes.txt";
        }

        public override void Process(Util.Commands.CmdTrigger<ToolCmdArgs> trigger)
        {
            using (var wowFile = new WoWFile(trigger.Text.NextWord()))
            {
                GameObjectTypeExtractor.Extract(wowFile);
            }
            //base.Process(trigger);
        }
    }
}