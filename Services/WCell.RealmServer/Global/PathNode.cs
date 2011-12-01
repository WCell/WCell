using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.ClientDB;
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
		public MapId mapId;

		public Vector3 Position
		{
			get;
			set;
		}

		public uint Phase
		{
			get { return WorldObject.AllPhases; }
		}

		public string Name;

		public NPCId HordeMountId;
		public NPCId AllianceMountId;

		/// <summary>
		/// All Paths from this Node to its neighbour Nodes
		/// </summary>
		public readonly List<TaxiPath> Paths = new List<TaxiPath>();

		public MapId MapId
		{
			get { return mapId; }
		}

		public Map Map
		{
			get { return World.GetNonInstancedMap(mapId); }
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

	public class DBCTaxiNodeConverter : AdvancedClientDBRecordConverter<PathNode>
	{
		public override PathNode ConvertTo(byte[] rawData, ref int id)
		{
			var node = new PathNode();

			int currentIndex = 0;
			id = (int)(node.Id = GetUInt32(rawData, currentIndex++));// col 0
			node.mapId = (MapId)GetUInt32(rawData, currentIndex++);// col 1
			node.Position = rawData.GetLocation((uint)currentIndex);// col 2, 3, 4
			currentIndex += 3;// 3 floats for location
			node.Name = GetString(rawData, ref currentIndex); // col 5 - 21
			node.HordeMountId = (NPCId)GetUInt32(rawData, currentIndex++);// col 22
			node.AllianceMountId = (NPCId)GetUInt32(rawData, currentIndex);// col 23

			return node;
		}
	}
}