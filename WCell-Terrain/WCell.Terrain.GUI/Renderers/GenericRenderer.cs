using System;
using Microsoft.Xna.Framework;
using WCell.Terrain.GUI.Util;
using WCell.Util.Graphics;
using Color = Microsoft.Xna.Framework.Graphics.Color;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	public class GenericRenderer : RendererBase
	{
		public static Color SelectedTriangleColor = Color.Yellow;

		public GenericRenderer(Game game)
			: base(game)
		{
			_cachedVertices = new VertexPositionNormalColored[0];
			_cachedIndices = new int[0];
		}

		public void SelectLine(WCell.Util.Graphics.Vector3 p1, WCell.Util.Graphics.Vector3 p2, Color color, bool doNotReplace = true)
		{
			var v = _cachedVertices.Length;
			var i = _cachedIndices.Length;
			if (doNotReplace)
			{
				Array.Resize(ref _cachedVertices, _cachedVertices.Length + 2);
				Array.Resize(ref _cachedIndices, _cachedIndices.Length + 3);
			}
			else
			{
				_cachedVertices = new VertexPositionNormalColored[2];
				_cachedIndices = new int[3];
			}

			XNAUtil.TransformWoWCoordsToXNACoords(ref p1);
			XNAUtil.TransformWoWCoordsToXNACoords(ref p2);

			_cachedVertices[v] = new VertexPositionNormalColored(p1.ToXna(),
																	color,
																	Vector3.Up);
			_cachedVertices[v+1] = new VertexPositionNormalColored(p2.ToXna(),
																	color,
																	Vector3.Up);

			// add indices
			_cachedIndices[i] = v;
			_cachedIndices[i+1] = v+1;
			_cachedIndices[i+2] = v;
		}

		public void Select(ref Triangle tri, bool doNotReplace =  true)
		{
			Select(ref tri, doNotReplace, SelectedTriangleColor);
		}

		public void Select(ref Triangle tri, bool doNotReplace, Color color)
		{
			var v = _cachedVertices.Length;
			var i = _cachedIndices.Length;

			if (doNotReplace)
			{
				Array.Resize(ref _cachedVertices, _cachedVertices.Length + 3);
				Array.Resize(ref _cachedIndices, _cachedIndices.Length + 3);
			}
			else
			{
				_cachedVertices = new VertexPositionNormalColored[3];
				_cachedIndices = new int[3];
			}

			// add vertices
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point1);
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point2);
			XNAUtil.TransformWoWCoordsToXNACoords(ref tri.Point3);
			_cachedVertices[v] = new VertexPositionNormalColored(tri.Point1.ToXna(),
																	color,
																	Vector3.Up);
			_cachedVertices[v+1] = new VertexPositionNormalColored(tri.Point2.ToXna(),
																	color,
																	Vector3.Up);
			_cachedVertices[v+2] = new VertexPositionNormalColored(tri.Point3.ToXna(),
																	color,
																	Vector3.Up);


			// add indices
			_cachedIndices[i] = v;
			_cachedIndices[i+1] = v+1;
			_cachedIndices[i+2] = v+2;
		}

		public void Clear()
		{
			Array.Resize(ref _cachedVertices, 0);
			Array.Resize(ref _cachedIndices, 0);
		}

		protected override void BuildVerticiesAndIndicies()
		{
			// do nothing
		}
	}
}
