using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Terrain.GUI.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.GUI
{
	public static class TerrainDisplayExtensions
	{
		public static Microsoft.Xna.Framework.Vector3 ToXna(this Vector3 vec)
		{
			XNAUtil.TransformWoWCoordsToXNACoords(ref vec);
			return new Microsoft.Xna.Framework.Vector3()
			{
				X = vec.X,
				Y = vec.Y,
				Z = vec.Z
			};
		}

		public static Vector3 ToWCell(this Microsoft.Xna.Framework.Vector3 vec)
		{
			XNAUtil.TransformXnaCoordsToWoWCoords(ref vec);
			return new Vector3(vec.X, vec.Y, vec.Z);
		}
	}

	public static class MatrixExtensions
	{
		public static Microsoft.Xna.Framework.Matrix ToXna(this Matrix mat)
		{
			return new Microsoft.Xna.Framework.Matrix
			{
				M11 = mat.M11,
				M12 = mat.M12,
				M13 = mat.M13,
				M14 = mat.M14,
				M21 = mat.M21,
				M22 = mat.M22,
				M23 = mat.M23,
				M24 = mat.M24,
				M31 = mat.M31,
				M32 = mat.M32,
				M33 = mat.M33,
				M34 = mat.M34,
				M41 = mat.M41,
				M42 = mat.M42,
				M43 = mat.M43,
				M44 = mat.M44
			};
		}
	}
}
