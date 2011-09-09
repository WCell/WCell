using WCell.RealmServer.Factions;
using WCell.Util.Commands;

namespace WCell.Tools.Commands
{
	public class PepsiFactionStudiesCommand : ToolCommand
	{
		protected override void Initialize()
		{
			Init("FactionStudies", "FS");
			EnglishDescription = "Provides commands to study factions.";
		}

		public class StudySpellCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("WriteFactionTemplatesByFlag", "WFT");
				EnglishDescription = "Writes all faction templates to file grouped by flag.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				FactionMgr.Initialize();
				Pepsi.FactionStudies.WriteFactionTemplatesByFlag();
			}
		}
	}
}
