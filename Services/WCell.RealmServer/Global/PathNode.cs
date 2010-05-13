using System.Collections.Generic;
using Cell.Core;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Taxi;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Represents a Node in the Taxi-network (can also be considered a "station")
	/// </summary>
	public class PathNode : IWorldLocation
	{
		public uint Id;
		public MapId MapId;

		public Vector3 Position
		{
			get;
			set;
		}
		public string Name;

		public NPCId HordeMountId;
		public NPCId AllianceMountId;

		/// <summary>
		/// All Paths from this Node to its neighbour Nodes
		/// </summary>
		public readonly List<TaxiPath> Paths = new List<TaxiPath>();

		public MapId RegionId
		{
			get { return Region != null ? Region.Id : MapId.End; }
		}

		public Region Region
		{
			get { return World.GetRegion(MapId); }
		}

		public void AddPath(TaxiPath path)
		{
			Paths.Add(path);
		}

		public TaxiPath GetPathTo(PathNode toNode)
		{
			foreach (var toPath in Paths)
			{
				if (toPath.To == toNode)
					return toPath;
			}
			return null;
		}

		public override string ToString()
		{
			return Name + " in " + MapId + " (" + Id + ")";
		}
	}

	public class DBCTaxiNodeConverter : AdvancedDBCRecordConverter<PathNode>
	{
		public override PathNode ConvertTo(byte[] rawData, ref int id)
		{
			var node = new PathNode();

			int currentIndex = 0;
            id = (int)(node.Id = GetUInt32(rawData, currentIndex++));// col 0
            node.MapId = (MapId)GetUInt32(rawData, currentIndex++);// col 1
            node.Position = rawData.GetLocation((uint)currentIndex);// col 2, 3, 4
			currentIndex += 3;// 3 floats for location
			node.Name = GetString(rawData, ref currentIndex); // col 5 - 21
            node.HordeMountId = (NPCId)GetUInt32(rawData, currentIndex++);// col 22
            node.AllianceMountId = (NPCId)GetUInt32(rawData, currentIndex);// col 23

			return node;
		}
	}
}