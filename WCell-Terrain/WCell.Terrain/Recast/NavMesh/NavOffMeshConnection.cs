using WCell.Util.Graphics;

namespace WCell.Terrain.Recast.NavMesh
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
