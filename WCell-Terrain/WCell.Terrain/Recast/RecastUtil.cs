using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Recast
{
	public static class RecastUtil
	{
		public static void TransformWoWCoordsToRecastCoords(ref Vector3 vertex)
		{
			var temp = vertex.X;
			vertex.X = vertex.Y;
			vertex.Y = vertex.Z;
			vertex.Z = temp;
		}

		public static void TransformRecastCoordsToWoWCoords(ref Vector3 vertex)
		{
			var temp = vertex.X;
			vertex.X = vertex.Z;
			vertex.Z = vertex.Y;
			vertex.Y = temp;
		}
	}
}
