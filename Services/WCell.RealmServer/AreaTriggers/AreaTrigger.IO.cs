using Cell.Core;
using WCell.Constants.World;
using WCell.Core.ClientDB;

namespace WCell.RealmServer.AreaTriggers
{
	class ATConverter : AdvancedClientDBRecordConverter<AreaTrigger>
	{
		public override AreaTrigger ConvertTo(byte[] rawData, ref int id)
		{
			var trigger = new AreaTrigger((uint)(id = rawData.GetInt32(0)),
				(MapId)rawData.GetUInt32(1),
				rawData.GetFloat(2),
				rawData.GetFloat(3),
				rawData.GetFloat(4),
				rawData.GetFloat(5),
                rawData.GetFloat(6),
                rawData.GetFloat(7),
                rawData.GetFloat(8),
                rawData.GetFloat(9));

			return trigger;
		}
	}
}