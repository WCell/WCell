using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Util;
using WCell.Core.Initialization;
using System.Xml.Serialization;
using WCell.Constants.World;

namespace WCell.RealmServer.Instances
{
	[XmlRoot("Instances")]
	public class InstanceConfig : InstanceConfigBase<InstanceConfig, MapId>
	{
		public static InstanceConfig Instance;

		public static void LoadSettings()
		{
			Instance = LoadSettings("Instances.xml");
		}

		protected override void InitSetting(InstanceSetting<MapId> setting)
		{
			InstanceMgr.SetCreator(setting.Name, setting.TypeName.Trim());
		}

		public override IEnumerable<MapId> SortedIds
		{
			get { return InstanceMgr.InstanceInfos.TransformList(info => info.Id); }
		}
	}
}
