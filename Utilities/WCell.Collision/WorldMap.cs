using System;
using System.IO;
using WCell.Constants;
using WCell.Constants.World;
using System.Collections.Generic;
using WCell.Util.Graphics;
using WCell.RealmServer;

namespace WCell.Collision
{
	public class WorldMap
	{
		public static string HeightMapFolder
		{
			get { return RealmServerConfiguration.GetContentPath("Maps/"); }
		}

		private const float ignorableHeightDiff = 0.1f;

		private static readonly Dictionary<string, TileReference> m_tileCache = new Dictionary<string, TileReference>();
		private static readonly bool[] mapExists = new bool[(int)MapId.End];


		public static int NumLoadedTiles
		{
			get
			{
				lock (m_tileCache)
				{
					var toRemove = new List<string>();

					foreach (var entry in m_tileCache)
					{
						if (entry.Value.Expired)
						{
							toRemove.Add(entry.Key);
						}
					}

					foreach (var key in toRemove)
					{
						m_tileCache.Remove(key);
					}

					toRemove = null;

					return m_tileCache.Count;
				}
			}
		}

		public static bool HasLOS(MapId mapId, Vector3 startPos, Vector3 endPos)
		{
			float tMax;
			var ray = CollisionHelper.CreateRay(startPos, endPos, out tMax);
			var intTMax = (int)(tMax + 0.5f);

			for (var t = 0; t < intTMax; t++)
			{
				var currentPos = ray.Position + ray.Direction * t;

				TileCoord currentTileCoord;
				ChunkCoord currentChunkCoord;
				PointX2D currentPointX2D;
				var currentHeightMapFraction = LocationHelper.GetFullInfoForPos(currentPos,
																				out currentTileCoord,
																				out currentChunkCoord,
																				out currentPointX2D);

				var currentTile = GetTile(mapId, currentTileCoord);
				if (currentTile == null)
				{
					// Can't check non-existant tiles.
					return false;
				}

				var terrainHeight = currentTile.GetInterpolatedHeight(currentChunkCoord, currentPointX2D, currentHeightMapFraction);

				if (terrainHeight < currentPos.Z) continue;
				return false;
			}

			return true;
		}

		public static bool TerrainDataExists(MapId mapId)
		{
			return HasMap(mapId);
		}

		public static float GetHeightAtPoint(MapId mapId, Vector3 worldPos)
		{
			float liquidHeight;
			var terrainHeight = GetHeightAtPoint(mapId, worldPos, out liquidHeight, false);
			return terrainHeight;
		}

		public static float GetHeightAtPoint(MapId mapId, Vector3 worldPos, out bool submerged)
		{
			float liquidHeight;
			var terrainHeight = GetHeightAtPoint(mapId, worldPos, out liquidHeight, true);
			submerged = terrainHeight < liquidHeight;
			return terrainHeight;
		}

		public static float GetHeightAtPoint(MapId mapId, Vector3 worldPos, out float liquidHeight, bool seekLiquid)
		{
			liquidHeight = float.NaN;

			TileCoord tileCoord;
			ChunkCoord chunkCoord;
			PointX2D pointX2D;
			var heightMapFraction = LocationHelper.GetFullInfoForPos(worldPos, out tileCoord, out chunkCoord, out pointX2D);

			var tile = GetTile(mapId, tileCoord);
			if (tile == null) return float.NaN;

			if (seekLiquid)
			{
				liquidHeight = tile.GetLiquidHeight(chunkCoord, pointX2D);
			}

			return tile.GetInterpolatedHeight(chunkCoord, pointX2D, heightMapFraction);
		}

		public static Vector3[] GetDirectPath(MapId mapId, Vector3 from, Vector3 to)
		{
			var position = from;
			var direction = (to - from);
			var dir = direction.NormalizedCopy();
			var maxTime = (int)from.GetDistance(to);

			var wayPoints = new List<Vector3>();
			for (var t = 1; t < maxTime; t++)
			{
				var newPos = position + dir;
				var newHeight = GetHeightAtPoint(mapId, newPos);

				var heightDiff = position.Z - newHeight;
				if (heightDiff < ignorableHeightDiff)
				{
					position = newPos;
					continue;
				}

				newPos.Z = newHeight;
				wayPoints.Add(newPos);
				position = newPos;
			}

			return wayPoints.ToArray();
		}

		public static FluidType GetFluidTypeAtPoint(MapId mapId, Vector3 worldPos)
		{
			TileCoord tileCoord;
			var chunkCoord = LocationHelper.GetChunkXYForPos(worldPos, out tileCoord);

			var tile = GetTile(mapId, tileCoord);
			if (tile == null) return FluidType.None;

			return tile.GetLiquidType(chunkCoord);
		}

		public static void DumpMapTileChunk(MapId mapId, Vector3 worldPos)
		{
			TileCoord tileCoord;
			var chunkCoord = LocationHelper.GetChunkXYForPos(worldPos, out tileCoord);

			var tile = GetTile(mapId, tileCoord);
			if (tile == null) return;

			tile.DumpChunk(chunkCoord);
		}

		internal static WorldMapTile GetTile(MapId mapId, TileCoord tileCoord)
		{
			if (!HasMap(mapId))
				return null;

			var key = string.Format("{0}_{1}_{2}", (int)mapId, tileCoord.TileX, tileCoord.TileY);

			TileReference reference;

			lock (m_tileCache)
			{
				if (m_tileCache.TryGetValue(key, out reference))
				{
					reference.KeepAlive();
					if (reference.Tile != null)
						return reference.Tile;

					if (reference.Broken)
						return null;
				}
				else
				{
					m_tileCache[key] = reference = new TileReference();
				}
			}

			lock (reference.LoadLock)
			{
				// If this Tile was loaded already, return it.
				if (reference.Tile != null)
					return reference.Tile;

				if (reference.Broken)
					return null;

				// Else try to load in the tile.
				var tile = LoadTile(mapId, tileCoord);
				if (tile == null)
				{
					reference.Broken = true;
					return null;
				}

				return reference.Tile = tile;
			}
		}

		private static WorldMapTile LoadTile(MapId mapId, TileCoord tileCoord)
		{
			var dir = Path.Combine(HeightMapFolder, ((int)mapId).ToString());
			if (!Directory.Exists(dir)) return null;

			var fileName = TerrainConstants.GetHeightMapFile(tileCoord.TileX, tileCoord.TileY);
			var fullPath = Path.Combine(dir, fileName);

			return (File.Exists(fullPath)) ? new WorldMapTile(fullPath) : null;
		}

		private static bool HasMap(MapId mapId)
		{
			lock (mapExists)
			{
				if (mapExists.Length <= (int)mapId)
				{
					return false;
				}

				if (!IsLoaded(mapId))
				{
					var mapPath = Path.Combine(HeightMapFolder, ((uint)mapId).ToString());
					var exists = Directory.Exists(mapPath);

					mapExists[(int)mapId] = exists;
					return exists;
				}

				return mapExists[(int)mapId];
			}
		}

		private static bool IsLoaded(MapId mapId)
		{
			return mapExists[(int)mapId];
		}
	}
}
