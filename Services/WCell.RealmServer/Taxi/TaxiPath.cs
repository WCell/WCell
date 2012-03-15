using System.Collections.Generic;
using Cell.Core;
using WCell.Core.DBC;
using WCell.RealmServer.Global;
using WCell.RealmServer.Paths;

namespace WCell.RealmServer.Taxi
{
    public class TaxiPath
    {
        public uint Id;

        public uint StartNodeId;
        public uint EndNodeId;
        public uint Price;

        public PathNode From;
        public PathNode To;
        public readonly LinkedList<PathVertex> Nodes = new LinkedList<PathVertex>();

        public float PathLength;
        /// <summary>
        /// The total time that it takes to fly on this Path from start to end
        /// in millis (equals <see cref="PathLength"/>*<see cref="WCell.RealmServer.Taxi.TaxiMgr.AirSpeed"/>)
        /// </summary>
        public uint PathTime;

        /// <summary>
        /// Returns a partial list of TaxiPathNodes
        ///  that comprise the remainder of the Units Taxi flight.
        /// </summary>
        //public PathVertex[] GetPartialPath(Unit unit)
        //{
        //    var startNode = unit.LatestPathNode ?? Nodes.First;

        //    if (startNode == Nodes.Last)
        //        return new PathVertex[0];

        //    var nodeList = new List<PathVertex>(Nodes.Count);
        //    while (startNode != Nodes.Last)
        //    {
        //        nodeList.Add(startNode.Next.Value);
        //        startNode = startNode.Next;
        //    }

        //    return nodeList.ToArray();
        //}

        public override string ToString()
        {
            if (From != null && To != null)
            {
                return string.Format("Path from {0} to {1}", From.Name, To.Name);
            }
            return string.Empty;
        }
    }

    public class DBCTaxiPathConverter : AdvancedDBCRecordConverter<TaxiPath>
    {
        public override TaxiPath ConvertTo(byte[] rawData, ref int id)
        {
            var path = new TaxiPath();

            id = (int)(path.Id = rawData.GetUInt32(0));
            path.StartNodeId = rawData.GetUInt32(1);
            path.EndNodeId = rawData.GetUInt32(2);
            path.Price = rawData.GetUInt32(3);

            return path;
        }
    }
}