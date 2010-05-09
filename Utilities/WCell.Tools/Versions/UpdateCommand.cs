using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Commands;
using WCell.Tools.Commands;
using WCell.MPQTool;
using System.IO;
using WCell.Constants;

namespace WCell.Tools.Versions
{
	public class UpdateCommand : ToolCommand
	{
		protected override void Initialize()
		{
			Init("Updater", "Update", "Upd");
			EnglishDescription = "Provides commands to maintain and check compatibility with the WoW client.";
		}

		public class VersionInfoCommand : VersionSubCmd
		{
			protected override void Initialize()
			{
				Init("Info", "I", "?");
				EnglishDescription = "Displays Client information.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				trigger.Reply("Selected client: {0}", VersionUpdater.WoWFile);
			}
		}

		public class DBCDumpCommand : VersionSubCmd
		{
			protected override void Initialize()
			{
				Init("DumpDBCs", "DBC");
				EnglishDescription = "Dumps the DBC files.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				trigger.Reply("Dumping DBC files...");
				DBCTool.Dump(ToolConfig.WoWDir, true, false);
			}
		}

		public class VersionUpdateCommand : VersionSubCmd
		{
			protected override void Initialize()
			{
				Init("Update", "U");
				ParamInfo = "[-f ]0/1|[-e]";
				EnglishDescription =
					"Updates WCell with the information from the selected client. Also dumps the DBC files again, unless specified otherwise. " +
					" -e only extracts enums.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				if (mod == "e")
				{
					WCellEnumWriter.WriteAllEnums();
				}
				else if (!mod.Contains("f") && VersionUpdater.WoWFile.Version <= WCellInfo.RequiredVersion)
				{
					trigger.Reply("WCell does already have the same or higher version as the given client: " +
					              WCellInfo.RequiredVersion);
					trigger.Reply("Use the -f switch (force) to update again.");
				}
				else
				{
					var dumpDBCs = !trigger.Text.HasNext || trigger.Text.NextBool();
					trigger.Reply("Updating changes for client: {0} ...", VersionUpdater.WoWFile);
					if (dumpDBCs)
					{
						trigger.Reply("Dumping DBC files...");
						DBCTool.Dump(ToolConfig.WoWDir, true, false);
					}
					VersionUpdater.DoUpdate();
					trigger.Reply("Done.");
				}
			}
		}

		public class SelectClientCommand : VersionSubCmd
		{
			public override bool RequiresClient
			{
				get { return false; }
			}

			protected override void Initialize()
			{
				Init("SelectClient", "Client", "C");
				ParamInfo = "-a|<client-path>";
				EnglishDescription = "Selects the given client or searches for it automatically when using switch '-a'.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				var mod = trigger.Text.NextModifiers();
				string dir;
				if (mod == "a")
				{
					dir = DBCTool.FindWowDir(null);
				}
				else
				{
					dir = Path.GetFullPath(trigger.Text.Remainder);
					if (trigger.Text.Remainder.EndsWith(".exe"))
					{
						dir = Path.GetDirectoryName(dir);
					}
				}

                if (!Directory.Exists(dir))
				{
					trigger.Reply("Directory does not exist: " + dir);
				}
				else
				{
					ToolConfig.WoWDir = dir;
					ToolConfig.Instance.Save();
					VersionUpdater.SetWowDir(ToolConfig.WoWFileLocation);
					trigger.Reply("Selected client: {0}", VersionUpdater.WoWFile);
				}
			}
		}

		public override bool MayTrigger(CmdTrigger<ToolCmdArgs> trigger, BaseCommand<ToolCmdArgs> cmd, bool silent)
		{
			if (cmd is VersionSubCmd)
			{
				var versionCmd = (VersionSubCmd) cmd;
				if (versionCmd.RequiresClient)
				{
					if (!Directory.Exists(ToolConfig.WoWDir))
					{
						if (!silent)
						{
							trigger.Reply("Invalid Client directory. Please use the Client-command to change it: {0}", ToolConfig.WoWDir);
						}
						return false;
					}
				}
			}
			return base.MayTrigger(trigger, cmd, silent);
		}

		public abstract class VersionSubCmd : SubCommand
		{
			public virtual bool RequiresClient
			{
				get { return true; }
			}
		}
	}
}