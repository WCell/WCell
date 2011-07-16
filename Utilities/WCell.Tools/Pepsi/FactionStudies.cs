using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.RealmServer.Factions;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Pepsi
{
	public static class FactionStudies
	{
		public static readonly string DumpFile = ToolConfig.OutputDir + "/FactionStudies.txt";

		[Tool]
		public static void WriteFactionTemplatesByFlag()
		{
			var templatesByFlag = new Dictionary<FactionTemplateFlags, List<FactionTemplateEntry>>();
			foreach (var faction in FactionMgr.ByTemplateId.Where(faction => faction != null))
			{
				foreach (FactionTemplateFlags member in Enum.GetValues(typeof(FactionTemplateFlags)))
				{
					if (faction.Template.Flags.HasAnyFlag(member))
					{
						if (!templatesByFlag.ContainsKey(member))
						{
							templatesByFlag.Add(member, new List<FactionTemplateEntry>());
							templatesByFlag[member].Add(faction.Template);
						}
						else
							templatesByFlag[member].Add(faction.Template);
					}
				}
			}

			using (var writer = new IndentTextWriter(DumpFile))
			{
				foreach (var flag in templatesByFlag.Keys)
				{
					writer.WriteLine();
					writer.WriteLine("##############################################");
					writer.WriteLine(flag);
					writer.WriteLine("##############################################");
					writer.WriteLine();

					foreach(var value in templatesByFlag[flag])
						DumpFactionTemplate(value, writer);
				}
				
			}

			
		}

		public static void DumpFactionTemplate(FactionTemplateEntry entry, IndentTextWriter writer)
		{
			writer.WriteLine("Id: " + entry.Id);
			writer.WriteLine("FactionId: " + entry.FactionId);
			writer.WriteLine("Flags: " + entry.Flags);
			/// <summary>
			/// The Faction-Group mask of this faction.
			/// </summary>
			writer.WriteLine("FactionGroup: " + entry.FactionGroup);
			/// <summary>
			/// Mask of Faction-Groups this faction is friendly towards
			/// </summary>
			writer.WriteLine("FriendGroup: " + entry.FriendGroup);
			/// <summary>
			/// Mask of Faction-Groups this faction is hostile towards
			/// </summary>
			writer.WriteLine("EnemyGroup: " + entry.EnemyGroup);

			writer.WriteLine("Explicit Enemies:");
			writer.IndentLevel++;
			for (var i = 0; i < entry.EnemyFactions.Length; i++)
			{
				writer.WriteLine(i + ": " + entry.EnemyFactions[i]);
			}
			writer.IndentLevel--;
			writer.WriteLine("Explicit Friends:");
			writer.IndentLevel++;
			for (var i = 0; i < entry.FriendlyFactions.Length; i++)
			{
				writer.WriteLine(i + ": " + entry.FriendlyFactions[i]);
			}
			writer.IndentLevel--;
			writer.WriteLine();
		}
	}
}
