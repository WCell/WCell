using System.Collections.Generic;
using Cell.Core;
using WCell.Constants.Pathing;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Core.Paths;
using WCell.RealmServer.Taxi;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Paths
{
	/// <summary>
	/// A bend or other kind of special location on a <see cref="TaxiPath"/>
	/// </summary>
	public class PathVertex : IPathVertex
	{
		// DBC Layout
		public uint Id { get; set; }
		public uint PathId;
		public uint NodeIndex;
		public MapId MapId;
		public Vector3 Pos;
		public TaxiPathNodeFlags Flags;
		/// <summary>
		/// Delay in seconds of how long to stay at this node
		/// </summary>
		public uint Delay;
		public uint ArrivalEventId;
		public uint DepartureEventId;

		// Custom
		public float DistFromStart;
		public float DistFromPrevious;

		/// <summary>
		/// Normalized vector in direction from the last node
		/// </summary>
		public Vector3 FromLastNode;

		/// <summary>
		/// Time from start in millis
		/// </summary>
		public int TimeFromStart;
		/// <summary>
		/// Time from previous node in millis
		/// </summary>
		public int TimeFromPrevious;

		public bool HasMapChange;

		public bool IsStoppingPoint
		{
			get { return (Flags & TaxiPathNodeFlags.ArrivalOrDeparture) != 0; }
		}
        
		public float Orientation
		{
			get { return 0.0f; }
		}

		public Vector3 Position
		{
			get { return Pos; }
		}

		public uint WaitTime
		{
			get { return Delay; }
		}

		public float GetDistanceToNext()
		{
			if (ListEntry.Next == null)
			{
				return 0;
			}
			return ListEntry.Next.Value.DistFromPrevious;
		}

		public TaxiPath Path;

		internal LinkedListNode<PathVertex> ListEntry;
	}

	public class DBCTaxiPathNodeConverter : AdvancedDBCRecordConverter<PathVertex>
	{
		public override PathVertex ConvertTo(byte[] rawData, ref int id)
		{
			uint i = 0;
			var node = new PathVertex();
			id = (int)(node.Id = rawData.GetUInt32(i++)); // 0
			node.PathId = rawData.GetUInt32(i++); // 1
			node.NodeIndex = rawData.GetUInt32(i++);// 2
			node.MapId = (MapId)rawData.GetUInt32(i++);// 3
			node.Pos = rawData.GetLocation(i);// 4, 5, 6,
			i += 3;
			node.Flags = (TaxiPathNodeFlags)rawData.GetUInt32(i++);// 7
			node.Delay = rawData.GetUInt32(i++);// 8
			node.ArrivalEventId = rawData.GetUInt32(i++);// 9
			node.DepartureEventId = rawData.GetUInt32(i);// 10

			return node;
		}
	}
}