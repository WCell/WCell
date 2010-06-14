using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TerrainDisplay.Recast
{
	public class NavOffMeshConnection
	{
		public Vector3[] Positions;							// Both end point locations.
		public float Radius;								// Link connection radius.
		public ushort Polygon;								// Poly Id
		public byte Flags;									// Link flags
		public byte Side;									// End point side.

		//public dtOffMeshConnection()
		//{
		//    pos = new Vector3[2];
		//}
	}
}
