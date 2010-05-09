using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Tools.Maps;
using WCell.Util.Commands;

namespace WCell.Tools.Commands
{
	public class TerrainCommand : ToolCommand
	{
		protected override void Initialize()
		{
			Init("Terrain", "Terr");
		}

		public class ExportHeightMapsCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Export", "ExportMapFiles", "Exp");
				EnglishDescription = "Exports the Heightmaps to the Content/ folder.";
			}

			public override void Process(CmdTrigger<ToolCmdArgs> trigger)
			{
				//HeightMapExtractor.WriteHeightMaps();
				WDTParser.ExportAll();
			}
		}
	}
}
