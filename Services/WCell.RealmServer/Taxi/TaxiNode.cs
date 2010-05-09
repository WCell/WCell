using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Global;
using WCell.Core.Graphics;
using WCell.Core.DBC;
using Cell.Core;
using WCell.Constants.World;

namespace WCell.RealmServer.Taxi
{
	/// <summary>
	/// Represents a Node in the Taxi-network (can also be considered a "station")
	/// </summary>
	public class TaxiNode
	{
		public uint Id;
		public MapId MapId;
		public Vector3 Position;
		public string Name;

		public uint HordeDisplayId;
		public uint AllianceDisplayId;
		
        /// <summary>
		/// All Paths from this Node to its neighbour Nodes
		/// </summary>
		public readonly List<TaxiPath> Paths = new List<TaxiPath>();

		public void AddPath( TaxiPath path )
		{
			Paths.Add( path );
		}

		public TaxiPath GetPathTo( TaxiNode toNode )
		{
			foreach( TaxiPath toPath in Paths )
			{
				if( toPath.To == toNode )
					return toPath;
			}
			return null;
		}
	}

	#region DBC
	public class DBCTaxiNodeConverter : DBCRecordConverter<TaxiNode>
	{
		public override TaxiNode ConvertTo(byte[] rawData, ref int id)
	    {
	        TaxiNode node = new TaxiNode();

            uint currentIndex = 0;
			id = (int)(node.Id = rawData.GetUInt32(currentIndex++));// col 0
			node.MapId = (MapId)rawData.GetUInt32(currentIndex++);// col 1
			node.Position = rawData.GetLocation(currentIndex);// col 2, 3, 4
            currentIndex += 3;// 3 floats for location
			node.Name = GetLocalizedString(rawData, ref currentIndex); // col 5 - 21
			node.HordeDisplayId = rawData.GetUInt32(currentIndex++);// col 22
			node.AllianceDisplayId = rawData.GetUInt32(currentIndex++);// col 23

	        return node;
	    }
	}
	#endregion
}
