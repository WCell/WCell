using Cell.Core;
using WCell.Constants.Items;
using WCell.Core.ClientDB;

namespace WCell.RealmServer.NPCs.Armorer
{
	public class DurabilityCost
	{
		public uint ItemLvl;
		public uint[] Multipliers;

		public uint GetModifierBySubClassId( ItemClass itemClass, ItemSubClass itemSubClass )
		{
			switch( itemClass )
			{
				case ItemClass.Weapon:
					return Multipliers[ (int)itemSubClass ];
				case ItemClass.Armor:
					return Multipliers[ (int)itemSubClass + 21 ];
				default: 
					return 0;
			}
		}

		public DurabilityCost()
		{
			Multipliers = new uint[29];
		}
	}

	#region DBC

	public class DBCDurabilityCostsConverter : AdvancedClientDBRecordConverter<DurabilityCost>
	{
		public override DurabilityCost ConvertTo( byte[] rawData, ref int id )
		{
			var cost = new DurabilityCost();

			id = (int)( cost.ItemLvl = rawData.GetUInt32( 0 ) );
			for( uint i = 1; i < 30; ++i )
			{
				cost.Multipliers[ i - 1 ] = rawData.GetUInt32( i );
			}

            return cost;
		}
	}

	#endregion
}