using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.RealmServer.Instances;
using WCell.Util;

namespace WCell.RealmServer.Battlegrounds
{
	[XmlRoot("Battlegrounds")]
	public class BattlegroundConfig : InstanceConfigBase<BattlegroundConfig, BattlegroundId>
	{
		public static BattlegroundConfig Instance;

		public override IEnumerable<BattlegroundId> SortedIds
		{
			get
			{
				IEnumerable<BattlegroundTemplate> templates 
					= BattlegroundMgr.Templates.Where(info => 
							info != null && info.Id > BattlegroundId.None &&
					        info.Id < BattlegroundId.End && info.Id != BattlegroundId.AllArenas
						);

				return templates.TransformList(info => info.Id);
			}
		}

		public static void LoadSettings()
		{
			Instance = LoadSettings("Battlegrounds.xml");
		}

		protected override void InitSetting(InstanceConfigEntry<BattlegroundId> configEntry)
		{
			BattlegroundMgr.SetCreator(configEntry.Name, configEntry.TypeName.Trim());
		}
	}
}