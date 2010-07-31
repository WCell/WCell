using Cell.Core;
using WCell.Core.DBC;

namespace WCell.RealmServer.NPCs.Armorer
{
	public class DurabilityQuality
	{
		public uint Id;
		public uint CostModifierPct;
	}

	#region DBC

	public class DBCDurabilityQualityConverter : AdvancedDBCRecordConverter<DurabilityQuality>
	{
		public override DurabilityQuality ConvertTo( byte[] rawData, ref int id )
		{
			var quality = new DurabilityQuality();

			id = (int)( quality.Id = rawData.GetUInt32( 0 ) );
			quality.CostModifierPct = (uint)(rawData.GetFloat( 1 ) * 100);

			return quality;
		}
	}

	#endregion
}