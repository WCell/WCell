using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.MPQTool;
using WCell.Util.Commands;

namespace WCell.Tools.Commands
{
    public class RalekSpellStudiesCommand : ToolCommand
    {
        protected override void Initialize()
        {
            Init("SpellStudies", "ST");
			EnglishDescription = "Provides commands to study spells.";
        }

        public class StudySpellCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("MatchSpellClassMask", "MSCM");
				EnglishDescription = "Matches spells with the given spell class mask.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
                Ralek.SpellStudies.FindFubecasFrickinSpells();
            }
		}
    }
}
