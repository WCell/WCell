using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain;
using WCell.Util.Graphics;
using Path = System.IO.Path;

namespace WCell.Terrain
{
    ///<summary>
    /// TODO: Read GO collision data, using GameobjectDisplayInfo.dbc
	///</summary>
	public abstract class Terrain : ITerrain
	{
		private const float ignorableHeightDiff = 0.1f;

		public static bool DoesMapExist(MapId mapId)
		{
			var mapPath = Path.Combine(WCellTerrainSettings.MapDir, ((uint)mapId).ToString());
			return Directory.Exists(mapPath);
		}


		///<summary>
		/// A profile of the tiles in this map
		/// Contains true if Tile [y, x] exists
		///</summary>
		public readonly bool[,] TileProfile = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

		public readonly TerrainTile[,] Tiles = new TerrainTile[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

		protected Terrain(MapId mapId)
		{
			// TODO: Read TileProfile from WDT file
			MapId = mapId;
		}

		public abstract bool IsWMOOnly { get; }

		public MapId MapId
		{
			get; 
			private set;
		}

		#region Terrain Queries
		public bool HasTerrainLOS(Vector3 startPos, Vector3 endPos)
		{
			float tMax;
			var ray = CollisionUtil.CreateRay(startPos, endPos, out tMax);
			var intTMax = (int)(tMax + 0.5f);

			for (var t = 0; t < intTMax; t++)
			{
				var currentPos = ray.Position + ray.Direction * t;

				Point2D tileCoord;
				Point2D chunkCoord;
				Point2D unitCoord;
				var currentHeightMapFraction = PositionUtil.GetHeightMapFraction(currentPos,
																				out tileCoord,
																				out chunkCoord,
																				out unitCoord);

				var currentTile = GetTile(tileCoord);
				if (currentTile == null)
				{
					// Can't check non-existant tiles.
					return false;
				}

				var terrainHeight = currentTile.GetInterpolatedHeight(chunkCoord, unitCoord, currentHeightMapFraction);

				if (terrainHeight < currentPos.Z) continue;
				return false;
			}

			return true;
		}

		public float QueryTerrainHeight(Vector3 worldPos)
		{
			float liquidHeight;
			var terrainHeight = QueryTerrainHeight(worldPos, out liquidHeight, false);
			return terrainHeight;
		}

		public float QueryTerrainHeight(Vector3 worldPos, out bool submerged)
		{
			float liquidHeight;
			var terrainHeight = QueryTerrainHeight(worldPos, out liquidHeight, true);
			submerged = terrainHeight < liquidHeight;
			return terrainHeight;
		}

		public float QueryTerrainHeight(Vector3 worldPos, out float liquidHeight, bool seekLiquid)
		{
			liquidHeight = float.NaN;

			Point2D tileCoord;
			Point2D chunkCoord;
			Point2D point2D;
			var heightMapFraction = PositionUtil.GetHeightMapFraction(worldPos, out tileCoord, out chunkCoord, out point2D);

			var tile = GetTile(tileCoord);
			if (tile == null) return float.NaN;

			if (seekLiquid)
			{
				liquidHeight = tile.GetLiquidHeight(chunkCoord, point2D);	// TODO: Interpolate?
			}

			return tile.GetInterpolatedHeight(chunkCoord, point2D, heightMapFraction);
		}

		public Vector3[] GetDirectPath(Vector3 from, Vector3 to)
		{
			var position = from;
			var direction = (to - from);
			var dir = direction.NormalizedCopy();
			var maxTime = (int)from.GetDistance(to);

			var wayPoints = new List<Vector3>();
			for (var t = 1; t < maxTime; t++)
			{
				var newPos = position + dir;
				var newHeight = QueryTerrainHeight(newPos);

				var heightDiff = position.Z - newHeight;
				if (heightDiff >= ignorableHeightDiff)
				{
					newPos.Z = newHeight;
					wayPoints.Add(newPos);
				}
				
				position = newPos;
			}

			return wayPoints.ToArray();
		}

		public FluidType GetFluidTypeAtPoint(Vector3 worldPos)
		{
			Point2D tileCoord;
			var chunkCoord = PositionUtil.GetXYForPos(worldPos, out tileCoord);

			var tile = GetTile(tileCoord);
			if (tile == null) return FluidType.None;

			return tile.GetFluidType(chunkCoord);
		}

        public bool HasLOS(Vector3 startPos, Vector3 endPos)
        {
            return HasModelLOS(startPos, endPos) ||
                   HasWMOLOS(startPos, endPos) ||
                   HasTerrainLOS(startPos, endPos);
		}

		public float GetEvironmentHeight(float x, float y)
		{
			// TODO: Improve
			return QueryWorldHeight(new Vector3(x, y));
		}

        public float QueryWorldHeight(Vector3 worldPos)
        {
            var modelHeight = QueryModelHeight(worldPos);
            var wmoHeight = QueryWMOHeight(worldPos);
            var terrainHeight = QueryTerrainHeight(worldPos);
            if (modelHeight == null)
            {
                return wmoHeight == null ? terrainHeight : Math.Max(wmoHeight.Value, terrainHeight);
            }
			if (wmoHeight == null)
			{
				return Math.Max(modelHeight.Value, terrainHeight);
			}
			return Math.Max(Math.Max(modelHeight.Value, wmoHeight.Value), terrainHeight);
        }

        public void QueryDirectPathAsync(PathQuery query)
        {
            var wayPoints = GetDirectPath(query.From, query.To);
            query.Reply(new Core.Paths.Path(wayPoints));
        }

        public bool HasWMOLOS(Vector3 startPos, Vector3 endPos)
        {
			return false;
			// TODO: MapObjectCollection.HasLOS(startPos, endPos);
        }

        public float? QueryWMOHeight(Vector3 worldPos)
		{
			return null;
			// TODO: return MapObjectCollection.GetWMOHeight(MapId, worldPos);
        }

		public float? QueryWMOCollision(Vector3 startPos, Vector3 endPos)
		{
			return null;
			// TODO: return MapObjectCollection.CheckCollision(MapId, startPos, endPos);
		}

        public float? QueryModelCollision(Vector3 startPos, Vector3 endPos)
        {
			return null;
			// TODO: Models != null ? Models.GetCollisionDistance(startPos, endPos) : null;
        }

        public bool HasModelLOS(Vector3 startPos, Vector3 endPos)
        {
			return false;
			// TODO: Models != null ? Models.HasLOS(startPos, endPos) : true;
        }

        public float? QueryModelHeight(Vector3 worldPos)
        {
			return null;
			// TODO:return Models != null ? Models.GetModelHeight(worldPos) : null;
        }
		#endregion

		public TerrainTile GetTile(Point2D tileCoord)
		{
			// get loaded tile
			var tile = Tiles[tileCoord.X, tileCoord.Y];
			if (tile != null) return tile;

			// check whether the tile exists
			if (!TileProfile[tileCoord.X, tileCoord.Y]) return null;

			// if it exists, try to load it from disk
			tile = LoadTile(tileCoord);
			TileProfile[tileCoord.X, tileCoord.Y] = tile != null;
			
			return tile;
		}

    	protected abstract TerrainTile LoadTile(Point2D tileCoord);

    	//public void DumpMapTileChunk(Vector3 worldPos)
    	//{
    	//    Point2D tileCoord;
    	//    var chunkCoord = PositionUtil.GetXYForPos(worldPos, out tileCoord);

    	//    var tile = GetTile(tileCoord);
    	//    if (tile == null) return;

    	//    tile.DumpChunk(chunkCoord);
    	//}

		#region Recast
		// TODO: Change it to become generic and add recast-specific changes in a recast-related class
		//public void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices)
		//{
		//    int vecCount;
		//    int idxCount;
		//    CalcArraySizes(out vecCount, out idxCount);

		//    vertices = new Vector3[vecCount];
		//    indices = new int[idxCount];

		//    var vecOffset = 0;
		//    var idxOffset = 0;

		//    // Get the ADT triangles
		//    foreach (var tile in m_TileLoader.MapTiles)
		//    {
		//        if (tile == null) continue;

		//        // The heightmap information
		//        idxOffset = CopyIndicesToRecastArray(tile.Indices, indices, idxOffset, vecOffset);
		//        vecOffset = CopyVectorsToRecastArray(tile.TerrainVertices, vertices, vecOffset);

		//        // The liquid information
		//        //idxOffset = CopyIndicesToRecastArray(tile.LiquidIndices, indices, idxOffset, vecOffset);
		//        //vecOffset = CopyVectorsToRecastArray(tile.LiquidVertices, vertices, vecOffset);
		//    }

		//    // Get the WMO triangles
		//    idxOffset = CopyIndicesToRecastArray(m_WMOLoader.WmoIndices, indices, idxOffset, vecOffset);
		//    vecOffset = CopyVectorsToRecastArray(m_WMOLoader.WmoVertices, vertices, vecOffset);

		//    // Get the M2 triangles
		//    idxOffset = CopyIndicesToRecastArray(m_M2Loader.RenderIndices, indices, idxOffset, vecOffset);
		//    vecOffset = CopyVectorsToRecastArray(m_M2Loader.RenderVertices, vertices, vecOffset);
		//}

		//private void CalcArraySizes(out int vecCount, out int idxCount)
		//{
		//    vecCount = CalcVecArraySize(true, false, true, true);
		//    idxCount = CalcIntArraySize(true, false, true, true);
		//}

		//private static int CopyIndicesToRecastArray(IList<int> renderIndices, IList<int> indices, int idxOffset, int vecOffset)
		//{
		//    if (renderIndices != null)
		//    {
		//        const int second = 2;
		//        const int third = 1;

		//        var length = renderIndices.Count / 3;
		//        for (var i = 0; i < length; i++)
		//        {
		//            var idx = i * 3;
		//            var index = renderIndices[idx + 0];
		//            indices[idxOffset + idx + 0] = (index + vecOffset);

		//            // reverse winding for recast
		//            index = renderIndices[idx + second];
		//            indices[idxOffset + idx + 1] = (index + vecOffset);

		//            index = renderIndices[idx + third];
		//            indices[idxOffset + idx + 2] = (index + vecOffset);
		//        }
		//        idxOffset += renderIndices.Count;
		//    }
		//    return idxOffset;
		//}

		//private static int CopyVectorsToRecastArray(IList<Vector3> renderVertices, IList<Vector3> vertices, int vecOffset)
		//{
		//    if (renderVertices != null)
		//    {
		//        var length = renderVertices.Count;
		//        for (var i = 0; i < length; i++)
		//        {
		//            var vertex = renderVertices[i];
		//            PositionUtil.TransformWoWCoordsToRecastCoords(ref vertex);
		//            vertices[vecOffset + i] = vertex;
		//        }
		//        vecOffset += renderVertices.Count;
		//    }
		//    return vecOffset;
		//}

		//private int CalcIntArraySize(bool includeTerrain, bool includeLiquid, bool includeWMO, bool includeM2)
		//{
		//    var count = 0;
		//    List<int> renderIndices;
		//    for (var t = 0; t < m_TileLoader.MapTiles.Count; t++)
		//    {
		//        var tile = m_TileLoader.MapTiles[t];
		//        if (includeTerrain)
		//        {
		//            renderIndices = tile.Indices;
		//            if (renderIndices != null)
		//            {
		//                count += renderIndices.Count;
		//            }
		//        }

		//        if (includeLiquid)
		//        {
		//            renderIndices = tile.LiquidIndices;
		//            if (renderIndices == null) continue;
		//            count += renderIndices.Count;
		//        }
		//    }

		//    // Get the WMO triangles
		//    if (includeWMO)
		//    {

		//        renderIndices = m_WMOLoader.WmoIndices;
		//        if (renderIndices != null)
		//        {
		//            count += renderIndices.Count;
		//        }
		//    }

		//    // Get the M2 triangles
		//    if (includeM2)
		//    {
		//        renderIndices = m_M2Loader.RenderIndices;
		//        if (renderIndices != null)
		//        {
		//            count += renderIndices.Count;
		//        }
		//    }

		//    return count;
		//}

		//private int CalcVecArraySize(bool includeTerrain, bool includeLiquid, bool includeWMO, bool includeM2)
		//{
		//    var count = 0;
		//    List<Vector3> renderVertices;
		//    foreach (var tile in m_TileLoader.MapTiles)
		//    {
		//        if (tile == null) continue;

		//        if (includeTerrain)
		//        {
		//            renderVertices = tile.TerrainVertices;
		//            if (renderVertices != null)
		//            {
		//                count += renderVertices.Count;
		//            }
		//        }

		//        if (includeLiquid)
		//        {
		//            renderVertices = tile.LiquidVertices;
		//            if (renderVertices == null) continue;
		//            count += renderVertices.Count;
		//        }
		//    }

		//    // Get the WMO triangles
		//    if (includeWMO)
		//    {
		//        renderVertices = m_WMOLoader.WmoVertices;
		//        if (renderVertices != null)
		//        {
		//            count += renderVertices.Count;
		//        }
		//    }

		//    // Get the M2 triangles
		//    if (includeM2)
		//    {
		//        renderVertices = m_M2Loader.RenderVertices;
		//        if (renderVertices != null)
		//        {
		//            count += renderVertices.Count;
		//        }
		//    }

		//    return count;
		//}
		#endregion
	}
}
