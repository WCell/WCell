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

		IEnumerable<ZoneId> GetAllZoneIds();
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

		public ZoneId GetZoneId(int col, int row)
		{
			return (ZoneId)ZoneIds[col, row];
		}

		public IEnumerable<ZoneId> GetAllZoneIds()
		{
			var set = new HashSet<ZoneId>();
			foreach (ZoneId id in ZoneIds)
			{
				if (id != 0 && !set.Contains(id))
				{
					set.Add(id);
					yield return id;
				}
			}
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

		public IEnumerable<ZoneId> GetAllZoneIds()
		{
			yield return (ZoneId) Id;
		}
	}
}