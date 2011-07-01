using System;
using Microsoft.Xna.Framework;
using WCell.Terrain.GUI.Util;
using WCell.Util.Graphics;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	public class SelectedTriangleRender : RendererBase
	{
		public static Color SelectedTriangleColor = Color.Yellow;

		public SelectedTriangleRender(Game game)
			: base(game)
		{
		}

		public void Select(ref Triangle tri, bool add)
		{
			if (add)
			{
				Array.Resize(ref _cachedVertices, _cachedVertices.Length + 3);
				Array.Resize(ref _cachedIndices, _cachedIndices.Length + 3);
			}
			else
			{
				_cachedVertices = new VertexPositionNormalColored[3];
				_cachedIndices = new int[3];
			}

			var i = _cachedVertices.Length - 3;

			// add vertices
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point1);
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point2);
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point3);
			_cachedVertices[i++] = new VertexPositionNormalColored(tri.Point1.ToXna(),
																	SelectedTriangleColor,
																	Vector3.Up);
			_cachedVertices[i++] = new VertexPositionNormalColored(tri.Point2.ToXna(),
																	SelectedTriangleColor,
																	Vector3.Up);
			_cachedVertices[i++] = new VertexPositionNormalColored(tri.Point3.ToXna(),
																	SelectedTriangleColor,
																	Vector3.Up);

			i -= 3;

			// add indices
			_cachedIndices[i] = i++;
			_cachedIndices[i] = i++;
			_cachedIndices[i] = i++;
		}

		protected override void BuildVerticiesAndIndicies()
		{
			// do nothing
		}
	}
}
