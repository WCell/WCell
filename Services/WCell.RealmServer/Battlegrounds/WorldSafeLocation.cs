using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cell.Core;
using WCell.Core.DBC;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
	public class DBCWorldSafeLocationConverter : AdvancedDBCRecordConverter<WorldSafeLocation>
	{
		public override WorldSafeLocation ConvertTo(byte[] rawData, ref int id)
		{
			var location = new WorldSafeLocation();

			id = (int)(location.Id = rawData.GetUInt32(0));

			location.MapId = (MapId)rawData.GetUInt32(1);
			location.X = rawData.GetFloat(2);
			location.Y = rawData.GetFloat(3);
			location.Z = rawData.GetFloat(4);

			return location;

		}
	}

	public class WorldSafeLocation
	{
		public uint Id;

		public MapId MapId;

		public float X;
		public float Y;
		public float Z;

		//public string Location;

	}
}