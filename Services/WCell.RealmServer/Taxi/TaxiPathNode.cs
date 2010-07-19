using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Graphics;
using WCell.Core.DBC;
using WCell.RealmServer.AI;
using WCell.RealmServer.Global;
using Cell.Core;
using WCell.Constants.World;

namespace WCell.RealmServer.Taxi
{
    [Flags]
    public enum TaxiPathNodeFlags : byte
    {
        /// <summary>
        /// Show the teleport screen even if MapId doesnt change
        /// </summary>
        IsTeleport = 1,
        ArrivalOrDeparture = 2,
    }

	/// <summary>
	/// A bend or other kind of special location on a TaxiPath
	/// </summary>
	public class TaxiPathNode : IMovementNode
	{
        // DBC Layout
		public uint Id;
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
		public float TimeFromStart;
        public float DistFromPrevious;
        public float TimeFromPrevious;

        public bool HasMapChange;

        public bool IsStoppingPoint
        {
            get { return (Flags & TaxiPathNodeFlags.ArrivalOrDeparture) != 0; }
        }

	    public uint ID
	    {
	        get { return Id; }
            set { }
	    }

	    public float Orientation
	    {
            get { return 0.0f; }
            set { }
	    }

	    public Vector3 TargetPosition
	    {
	        get { return Pos; }
            set { }
	    }

	    public uint WaitTime
	    {
            get { return 0; }
            set { }
	    }

		internal LinkedListNode<TaxiPathNode> ListEntry;
	}

	#region DBC
	public class DBCTaxiPathNodeConverter : DBCRecordConverter<TaxiPathNode>
	{
		public override TaxiPathNode ConvertTo(byte[] rawData, ref int id)
		{
			TaxiPathNode node = new TaxiPathNode();

            uint i = 0;
			id = (int)(node.Id = rawData.GetUInt32(i++)); // 0
			node.PathId = rawData.GetUInt32(i++); // 1
			node.NodeIndex = rawData.GetUInt32(i++);// 2
			node.MapId = (MapId)rawData.GetUInt32(i++);// 3
			node.Pos = rawData.GetLocation(i);// 4, 5, 6,
            i += 3;
            node.Flags = (TaxiPathNodeFlags)rawData.GetUInt32(i++);// 7
            node.Delay = rawData.GetUInt32(i++);// 8
            node.ArrivalEventId = rawData.GetUInt32(i++);// 9
            node.DepartureEventId = rawData.GetUInt32(i++);// 10

			return node;
		}
	}
	#endregion
}