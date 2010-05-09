using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;
using WCell.MPQTool;
using WCell.RealmServer;

namespace WCell.Tools.Commands
{
	public class DBCCommand : ToolCommand
	{
		protected override void Initialize()
		{
			Init("DBC");
			EnglishDescription = "Provides commands to work with DBC files.";
		}

		public class DBCDumpCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Dump");
				EnglishDescription = "Dumps all DBC files.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				DBCTool.DBCOutputDir = RealmServerConfiguration.Instance.DBCFolder;
				DBCTool.Dump();
			}
		}
	}
}
