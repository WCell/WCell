using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Items;
using System.IO;
using WCell.Util;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
	[Tool]
	public static class ItemOutput
	{
		public static readonly string DefaultDumpFile = ToolConfig.OutputDir + "Items.txt";
		static readonly string ItemDir = ToolConfig.OutputDir + "Items/";
		static readonly string TierFile = ItemDir + "Tiers.txt";

		public static void WriteAllItemInfo()
		{
			Tools.StartRealm();
			ItemMgr.LoadAll();

			using (var writer = new StreamWriter(DefaultDumpFile))
			{
				foreach (var item in ItemMgr.Templates)
				{
					if (item != null)
					{
						item.Dump(writer, "");

						writer.WriteLine();
						writer.WriteLine("########################################");
						writer.WriteLine();
					}
				}
			}

			WriteTiers();
		}

		private static void WriteTiers()
		{
			using (var writer = new IndentTextWriter(TierFile))
			{
				
			}
		}
	}
}