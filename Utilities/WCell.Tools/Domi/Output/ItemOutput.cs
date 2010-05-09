using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Items;
using System.IO;
using WCell.Util.Toolshed;

namespace WCell.Tools.Domi.Output
{
	[Tool]
	public static class ItemOutput
	{
	    public static readonly string DumpFile = ToolConfig.OutputDir + "Items.txt";

		public static void WriteAllItemInfo()
		{
			Tools.StartRealm();
			ItemMgr.LoadAll();
			using (var writer = new StreamWriter(DumpFile))
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
		}
	}
}