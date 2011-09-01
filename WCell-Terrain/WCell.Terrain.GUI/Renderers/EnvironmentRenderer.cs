using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WCell.Terrain.GUI.Util;
using WVector3 = WCell.Util.Graphics.Vector3;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace WCell.Terrain.GUI.Renderers
{
	class EnvironmentRenderer : RendererBase
	{
	    private static Color TerrainColor = Color.DarkSlateGray;

	    private TerrainTile tile;

		public EnvironmentRenderer(Game game, TerrainTile tile)
			: base(game)
		{
		    this.tile = tile;
		}

		#region Build polygons
		protected override void BuildVerticiesAndIndicies()
		{
		    var tempIndices = new List<int>();
		    var tempVertices = new List<VertexPositionNormalColored>();
		    
            var offset = tempVertices.Count;
		    foreach (var vertex in tile.TerrainVertices)
		    {
		        var vertexPosNmlCol1 = new VertexPositionNormalColored(vertex.ToXna(),
		                                                               TerrainColor,
		                                                               Vector3.Zero);
		        tempVertices.Add(vertexPosNmlCol1);
		    }

		    foreach (var index in tile.TerrainIndices)
		    {
		        tempIndices.Add(offset + index);
		    }

		    _cachedVertices = tempVertices.ToArray();
		    _cachedIndices = tempIndices.ToArray();

		    _renderCached = true;
		}

	    #endregion
	}
}
