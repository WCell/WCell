using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Debugging;
using WCell.Util.Commands;
using WCell.Tools.PATools;
using System.IO;
using WCell.PacketAnalysis.Logs;
using WCell.Util;

namespace WCell.Tools.Commands
{
	public class PacketAnalyzerCommand : ToolCommand
	{
		public PATool PATool
		{
			get { return Tools.PATool; }
		}

		protected override void Initialize()
		{
			Init("PacketAnalyzer", "PA");
			EnglishDescription = "Provides commands to use the PacketAnalyzer for converting log-files.";
		}

		public abstract class PASubCommand : SubCommand
		{
			public PATool PATool
			{
				get { return ((PacketAnalyzerCommand)RootCmd).PATool; }
			}

			public PacketAnalyzerCommand PACommand
			{
				get { return (PacketAnalyzerCommand)RootCmd; }
			}

			public void DisplaySelectedFiles(CmdTrigger<ToolCmdArgs> trigger, string indent)
			{
				var tool = PATool;

				for (var i = 0; i < tool.SelectedFiles.Count; i++)
				{
					var file = tool.SelectedFiles[i];
					trigger.Reply(indent + "{0}. {1}", i, file.FullName);
				}
			}

			public void AddSelectedFiles(CmdTrigger<ToolCmdArgs> trigger, IEnumerable<string> parts)
			{
				List<int> indices = null;
				int index;
				foreach (var part in parts)
				{
					if (int.TryParse(part, out index))
					{
						if (indices == null)
						{
							indices = new List<int>();
						}
						indices.Add(index);
					}
					else
					{
						if (!PATool.AddSelectedFile(part.Trim()))
						{
							trigger.Reply(" File was invalid or already selected: " + part.Trim());
						}
					}
				}

				if (indices != null)
				{
					if (!PATool.AddSelectedFiles(indices.ToArray()))
					{
						trigger.Reply(" One or more indices were invalid: " + indices.ToString(", "));
					}
				}
				trigger.Reply("Done - Selected files:");
				DisplaySelectedFiles(trigger, " ");
			}

			public void RemoveSelectedFiles(CmdTrigger<ToolCmdArgs> trigger, IEnumerable<string> parts)
			{
				List<int> indices = null;
				int index;
				foreach (var part in parts)
				{
					if (int.TryParse(part, out index))
					{
						if (indices == null)
						{
							indices = new List<int>();
						}
						indices.Add(index);
					}
					else
					{
						if (!PATool.RemoveSelectedFile(part.Trim()))
						{
							trigger.Reply(" File was not selected: " + part.Trim());
						}
					}
				}
				if (indices != null)
				{
					if (!PATool.RemoveSelectedFiles(indices.ToArray()))
					{
						trigger.Reply(" One or more indices were invalid: " + indices.ToString(", "));
					}
				}
				trigger.Reply("Done - Selected files:");
				DisplaySelectedFiles(trigger, " ");
			}

			public void Remove(List<string> list, CmdTrigger<ToolCmdArgs> trigger)
			{
				if (!trigger.Text.HasNext)
				{
					trigger.Reply("No arguments specified - " + CreateInfo());
				}
				else
				{
					if (trigger.Text.NextModifiers() == "a")
					{
						list.Clear();
						trigger.Reply("Filters removed.");
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						var indices = new List<int>(3);
						var strings = new List<string>(3);
						foreach (var part in parts)
						{
							int index;
							if (int.TryParse(part, out index) && index >= 0 && index < list.Count)
							{
								indices.Add(index);
							}
							else
							{
								strings.Add(part);
							}
						}

						var removed = new List<string>(3);
						indices.Sort();
						for (var i = indices.Count - 1; i >= 0; i--)
						{
							var index = indices[i];
							removed.Add(list[i]);
							list.RemoveAt(index);
						}

						foreach (var part in strings)
						{
							if (list.Remove(part))
							{
								removed.Add(part);
							}
						}
						trigger.Reply("Removed {0} filters: " + removed.ToString(", "), removed.Count());
					}
				}
			}
		}

		#region Parse

		public class PAParseCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("Parse", "P");
				ParamInfo = "";
				EnglishDescription = "Parses all selected log-files with the current settings - Use Info for more information.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				trigger.Reply("Parsing...");
				PATool.Parse();
				trigger.Reply("Done.");
			}
		}

		#endregion

		#region Save

		public class PASaveCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("Save", "S");
				ParamInfo = "[<newlocation>]";
				EnglishDescription =
					"Saves the current settings of the PATool. You can optionally add a new location where it should be saved to.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				var location = trigger.Text.NextWord().Trim();
				if (location.Length > 0)
				{
					PATool.SaveAs(location);
				}
				else
				{
					PATool.Save();
				}
				trigger.Reply("Done. - Saved to: " + PATool.ActualFile);
			}
		}

		#endregion

		#region Load

		public class PALoadCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("Load");
				ParamInfo = "";
				EnglishDescription = "Reloads the packet definitions.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				DebugUtil.LoadDefinitions();
			}
		}

		#endregion

		#region Info

		public class PAInfoCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("Info", "I");
				ParamInfo = "";
				EnglishDescription = "Displays information about the current PATool settings.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				var tool = PATool;
				trigger.Reply("Selected dir: " + tool.SelectedDir.FullName);
				trigger.Reply("LogParser: " + tool.LogParserType);
				trigger.Reply("Output file: " + tool.Output.Name);
				trigger.Reply("Filters:");
				trigger.Reply(" Include:");
				trigger.Reply("	 IncAnd: " + tool.OpCodeIncAndFilters.ToString(", "));
				trigger.Reply("	 IncOr: " + tool.OpCodeIncOrFilters.ToString(", "));
				trigger.Reply(" Exclude:");
				trigger.Reply("	 ExcAnd: " + tool.OpCodeExcAndFilters.ToString(", "));
				trigger.Reply("	 ExcOr: " + tool.OpCodeExcOrFilters.ToString(", "));
				trigger.Reply(" ");
				trigger.Reply("Selected files:");
				DisplaySelectedFiles(trigger, " ");
			}
		}

		#endregion

		#region Selected Directory

		public class PASelectDirCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("SelectDir", "SelDir", "SD");
				ParamInfo = "[<directory>]";
				EnglishDescription = "Selects the given directory or -if directory is ommited- shows the currently selected directory.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				var tool = PATool;
				if (trigger.Text.HasNext)
				{
					var path = trigger.Text.Remainder;
					if (tool.SetSelectedDirPath(path))
					{
						trigger.Reply("Selected new directory: " + tool.SelectedDir.FullName);
					}
					else
					{
						trigger.Reply("Invalid directory: " + path);
						return;
					}
				}
				else
				{
					trigger.Reply("Selected directory: " + tool.SelectedDir.FullName);
				}

				trigger.Reply("Selected files: ");
				var files = tool.SelectedFiles;
				for (var i = 0; i < files.Count; i++)
				{
					var file = files[i];
					trigger.Reply(" {0}: {1}", i, file.Name);
				}
			}
		}

		#endregion

		#region Output File

		public class PASelectOutputFileCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("SelectOut", "SelOut", "SO");
				ParamInfo = "<file>";
				EnglishDescription = "Selects the given file to write the output to.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				if (!trigger.Text.HasNext)
				{
					trigger.Reply("You did not specify an Output file.");
				}
				else
				{
					PATool.OutputFilePath = trigger.Text.NextWord();
					trigger.Reply("Output File: " + PATool.OutputFilePath);
				}
			}
		}

		#endregion

		#region Parsers

		public class PASelectParserCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("SelectParser", "SetParser", "Parser", "SP");
				ParamInfo = "<parsername>";
				EnglishDescription = "Selects the given log-parser. Make sure to choose the right one for the format of your log files.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				PATool.LogParserType = trigger.Text.NextEnum(LogParserType.KSniffer);
				trigger.Reply("Selected Parser: " + PATool.LogParserType);
			}
		}

		public class PAListParsersCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("ListParsers", "LP");
				ParamInfo = "<parsername>";
				EnglishDescription = "Lists all available parsers. Use SelectParser to select the right one for your log files.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				trigger.Reply("Available LogParsers:");
				foreach (var type in Enum.GetValues(typeof(LogParserType)))
				{
					trigger.Reply(" {0}. {1}", (int)type, type);
				}
			}
		}

		#endregion

		#region Selected Files

		public class PAAddSelectedFilesCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("SelectFiles", "SF");
				ParamInfo = "<file>[,<file2>[,<file3> ...]]|[-l <dir>]";
				EnglishDescription =
					"Adds the given log-file(s) to the list of selected log-files. All selected log-files will be parsed. " +
					"Parameters can either be fully qualified file-names (relative or absolute) or numbers that correspond to the index within the currently selected Directory. The -l switch optionally lists all files (with numbers) of the given directory.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				if (!trigger.Text.HasNext)
				{
					trigger.Reply("No arguments specified - " + CreateInfo());
				}
				else
				{
                    var mod = trigger.Text.NextModifiers();
                    if (mod == "l")
                    {
                        var files = Directory.GetFiles(trigger.Text.NextWord());
                        PATool.ClearSelectedFiles();
                        trigger.Reply("Deleting selected files");
                        for (var i = 0; i < files.Length; i++)
                        {
                            var file = files[i];
                            if (PATool.AddSelectedFile(file))
                            {
                                trigger.Reply("{0}. {1}", i, file);
                            }
                        }
                    }
                    else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						AddSelectedFiles(trigger, parts);
					}
				}
			}
		}

		public class PARemoveSelectedFilesCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("DeselectFiles", "DeselFiles", "UnselFiles", "DF", "UF");
				ParamInfo = "[-a]<file>[,<file2>[,<file3> ...]]";
				EnglishDescription =
					"Removes either all (-a) or the given log-file(s) to the list of selected log-files. Only selected log-files will be parsed. " +
					"Parameters can either be fully qualified file-names (relative or absolute) or numbers that correspond to the index of the currently selected files.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				if (!trigger.Text.HasNext)
				{
					trigger.Reply("No arguments specified - " + CreateInfo());
				}
				else
				{
					if (trigger.Text.NextModifiers() == "a")
					{
						PATool.ClearSelectedFiles();
						trigger.Reply("Done - Selected files:");
						DisplaySelectedFiles(trigger, " ");
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						RemoveSelectedFiles(trigger, parts);
					}
				}
			}
		}

		#endregion

		#region Add Filters

		public class PAAddFilterCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("AddFilter", "AF");
				EnglishDescription = "Adds to the different kinds of filters.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				TriggerSubCommand(trigger);
			}

			public class PAAddIncAndFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("IncAnd", "IA");
					ParamInfo = "<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Adds inclusive And-filters: A packet is only parsed if its opcode contains *all* of these.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					if (!trigger.Text.HasNext)
					{
						trigger.Reply("No arguments specified - " + CreateInfo());
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						foreach (var part in parts)
						{
							PATool.OpCodeIncAndFilters.Add(part.Trim());
						}
						trigger.Reply("Added {0} new IncAnd-filters: " + parts.ToString(", "), parts.Count());
					}
				}
			}

			public class PAAddIncOrFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("IncOr", "IO");
					ParamInfo = "<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Adds inclusive Or-filters: A packet is only parsed if its opcode contains *any* of these.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					if (!trigger.Text.HasNext)
					{
						trigger.Reply("No arguments specified - " + CreateInfo());
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						foreach (var part in parts)
						{
							PATool.OpCodeIncOrFilters.Add(part.Trim());
						}
						trigger.Reply("Added {0} new IncOr-filters: " + parts.ToString(", "), parts.Count());
					}
				}
			}

			public class PAAddExcAndFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("ExcAnd", "EA");
					ParamInfo = "<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Adds exclusive And-filters: A packet is only parsed if *no* opcode contains *all* of these.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					if (!trigger.Text.HasNext)
					{
						trigger.Reply("No arguments specified - " + CreateInfo());
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						foreach (var part in parts)
						{
							PATool.OpCodeExcAndFilters.Add(part.Trim());
						}
						trigger.Reply("Added {0} new ExcAnd-filters: " + parts.ToString(", "), parts.Count());
					}
				}
			}

			public class PAAddExcOrFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("ExcOr", "EO");
					ParamInfo = "<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Adds exclusive Or-filters: A packet is only parsed if *no* opcode contains *any* of these.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					if (!trigger.Text.HasNext)
					{
						trigger.Reply("No arguments specified - " + CreateInfo());
					}
					else
					{
						var parts = trigger.Text.Remainder.Split(',').TransformArray(part => part.Trim());
						foreach (var part in parts)
						{
							PATool.OpCodeExcOrFilters.Add(part.Trim());
						}
						trigger.Reply("Added {0} new ExcOr-filters: " + parts.ToString(", "), parts.Count());
					}
				}
			}
		}

		#endregion

		#region Remove Filters

		public class PARemoveFilterCommand : PASubCommand
		{
			protected override void Initialize()
			{
				Init("RemoveFilter", "RF");
				EnglishDescription = "Removes the different kinds of filters.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				TriggerSubCommand(trigger);
			}

			public class PARemoveIncAndFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("IncAnd", "IA");
					ParamInfo = "[-a]|<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Removes inclusive And-filters. - A packet is only parsed if its opcode contains all of these. " +
						"-a removes all filters. Parts can also be indices.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					Remove(PATool.OpCodeIncAndFilters, trigger);
				}
			}

			public class PARemoveIncOrFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("IncOr", "IO");
					ParamInfo = "[-a]|<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Removes inclusive Or-filters. - A packet is only parsed if its opcode contains at least one these." +
						"-a removes all filters. Parts can also be indices.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					Remove(PATool.OpCodeIncOrFilters, trigger);
				}
			}

			public class PARemoveExcAndFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("ExcAnd", "EA");
					ParamInfo = "[-a]|<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Removes exclusive And-filters. - A packet is only parsed if *no* opcode contains *all* of these." +
						"-a removes all filters. Parts can also be indices.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					Remove(PATool.OpCodeExcAndFilters, trigger);
				}
			}

			public class PARemoveExcOrFilterCommand : PASubCommand
			{
				protected override void Initialize()
				{
					Init("ExcOr", "EO");
					ParamInfo = "[-a]|<part of opcode-name>[,<another part>[,<yet another part> ...]]";
					EnglishDescription = "Removes exclusive Or-filters. - A packet is only parsed if *no* opcode contains *any* of these." +
						"-a removes all filters. Parts can also be indices.";
				}

				public override void Process(CmdTrigger<ToolCmdArgs> trigger)
				{
					Remove(PATool.OpCodeExcOrFilters, trigger);
				}
			}
		}

		#endregion
	}
}