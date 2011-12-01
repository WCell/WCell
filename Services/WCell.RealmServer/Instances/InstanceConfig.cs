using System.Collections.Generic;
using System.Xml.Serialization;
using WCell.Constants.World;
using WCell.Util;

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

		protected override void InitSetting(InstanceConfigEntry<MapId> configEntry)
		{
			InstanceMgr.SetCreator(configEntry.Name, configEntry.TypeName.Trim());
		}

		public override IEnumerable<MapId> SortedIds
		{
			get { return InstanceMgr.InstanceInfos.TransformList(info => info.Id); }
		}
	}
}