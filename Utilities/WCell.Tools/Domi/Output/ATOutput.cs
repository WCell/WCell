using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.RealmServer.Database;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Spells;
using WCell.RealmServer.AreaTriggers;
using WCell.Util;

namespace WCell.Tools.Domi.Output
{
	public static class ATOutput
	{
		public static readonly string DumpFile = ToolConfig.OutputDir + "/AreaTriggers.txt";

		private static void Init()
		{
			RealmDBMgr.Initialize();
			SpellHandler.LoadSpells();
			AreaTriggerMgr.Initialize();
		}

		public static void DumpToFile()
		{
			Init();
			using (var writer = new IndentTextWriter(DumpFile))
			{
				foreach (var at in AreaTriggerMgr.AreaTriggers)
				{
					if (at != null)
					{
						at.Write(writer);

						writer.WriteLine();
						writer.WriteLine("##############################################");
						writer.WriteLine();
					}
				}
			}
		}
	}
}