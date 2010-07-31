using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;

namespace WCell.Constants.World
{
	public interface IZoneGrid
	{
		ZoneId GetZoneId(int x, int y);
	}

	public struct ZoneGrid : IZoneGrid
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly uint[,] ZoneIds;

		public ZoneGrid(uint[,] ids)
		{
			ZoneIds = ids;
		}

		public ZoneId GetZoneId(int x, int y)
		{
			return (ZoneId)ZoneIds[x, y];
		}
	}

	public struct SimpleZoneGrid : IZoneGrid
	{
		public readonly uint Id;

		public SimpleZoneGrid(uint id)
		{
			Id = id;
		}

		public ZoneId GetZoneId(int x, int y)
		{
			return (ZoneId)Id;
		}
	}
}