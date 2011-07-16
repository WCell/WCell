using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.Constants;
using WCell.Util.Code;
using WCell.Util.Toolshed;
using System.IO;

namespace WCell.Tools.Domi
{
	public static class Instances
	{
		[Tool]
		public static void WriteInstanceStubs()
		{
			var dir = ToolConfig.DefaultAddonSourceDir + "Instances/";

			RealmDBMgr.Initialize();
			ContentMgr.Initialize();
			World.InitializeWorld();
			InstanceMgr.Initialize();

			foreach (var instance in InstanceMgr.InstanceInfos)
			{
				var className = instance.Id.ToString();
				var baseClass = typeof(BaseInstance).Name;
				var file = dir + className + ".cs";
				if (!File.Exists(file))
				{
					using (var writer = new CodeFileWriter(file, "WCell.Addons.Default.Instances",
					                                       className,
					                                       "class",
					                                       ": " + baseClass,
					                                       "WCell.RealmServer.Instances"))
					{

					}
				}
			}
		}
	}
}