using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Tools.Maps.Constants;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Tools.Maps.Parsing.ADT.Components;
using WCell.Tools.Maps.Parsing.M2s;
using WCell.Tools.Maps.Parsing.WDT;
using WCell.Tools.Maps.Parsing.WMO;
using WCell.Tools.Maps.Parsing.WMO.Components;
using WCell.Tools.Maps.Utils;
using WCell.Util.Graphics;
using NLog;
using MathHelper = WCell.Util.Graphics.MathHelper;

namespace WCell.Tools.Maps
{
	public static class MapTileExtractor
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
        public static readonly HashSet<string> ModelsToIgnore = new HashSet<string>();
        public static readonly HashSet<uint> LoadedM2Ids = new HashSet<uint>();
        public static readonly HashSet<uint> LoadedWmoIds = new HashSet<uint>();
        public static ADT[,] TerrainInfo;
        
        private const string FileTypeId = "ter";

		public static void Prepare()
		{
			WDTParser.Parsed += ExportMapTiles;
		}

		/// <summary>
		/// Writes all height maps to the default MapDir
		/// </summary>
		public static void ExportMapTiles(WDTFile wdt)
		{
            // Map data should only be stored per map
		    ClearObjectData();
            var path = Path.Combine(ToolConfig.MapDir, wdt.Entry.Id.ToString());

			if (wdt.IsWMOOnly)
			{
				// This Map has no Tiles, but the MODF still needs to be written
                // These maps will be considered to have one tile at {0, 0} with only one MODF written therein
			    var adt = ExtractWMOOnly(wdt);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var file = File.Create(Path.Combine(path, TerrainConstants.GetMapFilename(0, 0))))
                {
                    WriteTileInfo(file, adt);
                }
				return;
			}

            // Read in the ADT data - this includes height and liquid maps, WMO information and M2 information
			TerrainInfo = ExtractMapTiles(wdt);

            // Write the processed data to files
            var count = 0;
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
			{
				for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
				{
					var adt = TerrainInfo[tileY, tileX];
					if (adt == null) continue;
					
					using (var file = File.Create(Path.Combine(path, TerrainConstants.GetMapFilename(tileX, tileY))))
					{
					    WriteTileInfo(file, adt);
                        file.Close();
					}
					count++;
				}
			}

			log.Info("Extracted {0} tiles for {1}.", count, (MapId)wdt.Entry.Id);
		}

	    private static void ClearObjectData()
	    {
	        LoadedM2Ids.Clear();
	        LoadedWmoIds.Clear();
            ModelsToIgnore.Clear();
            WorldObjectExtractor.LoadedM2Models.Clear();
	        WorldObjectExtractor.LoadedWMORoots.Clear();
	    }

	    #region Extract Terrain
        public static ADT ExtractWMOOnly(WDTFile wdt)
        {
            var adt = new ADT("");
            adt.ObjectDefinitions.Capacity = wdt.WmoDefinitions.Count;
            foreach (var wmoDefinition in wdt.WmoDefinitions)
            {
                adt.ObjectDefinitions.Add(wmoDefinition);
            }

            adt.ObjectFiles.Capacity = wdt.WmoFiles.Count;
            foreach (var wmoFile in wdt.WmoFiles)
            {
                adt.ObjectFiles.Add(wmoFile);
            }

            foreach (var def in adt.ObjectDefinitions)
            {
                LoadWMO(wdt.Manager, def);
            }
            return adt;
        }

		public static ADT[,] ExtractMapTiles(WDTFile wdt)
		{
			var mapTiles = new ADT[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

			for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
			{
				for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
				{
					if (!wdt.TileProfile[y, x]) continue;

                    var adt = ADTParser.Process(WDTParser.MpqManager, wdt.Entry, x, y);
                    if (adt == null) continue;

				    adt.IsWMOOnly = false;
                    
                    // Load in the WMORoots and their DoodadDefinitions
                    // Load in the ADTs referenced M2Models
				    PrepareChunkInfo(wdt.Manager, adt);

					mapTiles[y, x] = adt;
				}
			}

			return mapTiles;
		}

	    private static void PrepareChunkInfo(MpqManager manager, ADT adt)
	    {
            for (var chunkX = 0; chunkX < TerrainConstants.ChunksPerTileSide; chunkX++)
            {
                for (var chunkY = 0; chunkY < TerrainConstants.ChunksPerTileSide; chunkY++)
                {
                    var chunk = adt.MapChunks[chunkY, chunkX];
                    foreach (var dRefId in chunk.DoodadRefs)
                    {
                        var dRef = adt.DoodadDefinitions[dRefId];
                        if (dRef == null) continue;
                        if (ModelsToIgnore.Contains(dRef.FilePath)) continue;
                        if (LoadedM2Ids.Contains(dRef.UniqueId)) continue;

                        M2Model model;
                        if (!WorldObjectExtractor.LoadedM2Models.TryGetValue(dRef.FilePath, out model))
                        {
                            model = M2ModelParser.Process(manager, dRef.FilePath);
                            WorldObjectExtractor.LoadedM2Models.Add(dRef.FilePath, model);
                        }

                        // This model may have no collision information. If so there is no point saving the data.
                        if (model.BoundingVertices.IsNullOrEmpty())
                        {
                            // This model has no collision information. We'll ignore it.
                            ModelsToIgnore.Add(dRef.FilePath);
                            WorldObjectExtractor.LoadedM2Models.Remove(dRef.FilePath);
                        }

                        // At this point, we're using the model for collision stuff
                        PrepareM2Info(dRef, model);
                        LoadedM2Ids.Add(dRef.UniqueId);
                    }

                    foreach (var oRefId in chunk.ObjectRefs)
                    {
                        var oRef = adt.ObjectDefinitions[oRefId];
                        if (oRef == null) continue;
                        LoadWMO(manager, oRef);
                    }

                    PrepareChunk(adt, chunkX, chunkY);
                }
            }
	    }

        private static void LoadWMO(MpqManager manager, MapObjectDefinition def)
	    {
	        if (LoadedWmoIds.Contains(def.UniqueId)) return;
            LoadedWmoIds.Add(def.UniqueId);

	        if (!WorldObjectExtractor.LoadedWMORoots.ContainsKey(def.FilePath))
	        {
	            var root = WMORootParser.Process(manager, def.FilePath);
	            WorldObjectExtractor.LoadedWMORoots.Add(def.FilePath, root);
	        }

	        PrepareWMOInfo(def);
	    }

	    private static void PrepareWMOInfo(MapObjectDefinition def)
        {
            TerrainConstants.TilePositionToWorldPosition(ref def.Position);
            TerrainConstants.TileExtentsToWorldExtents(ref def.Extents);

            Matrix modelToWorld;
            Matrix.CreateRotationZ(WCell.Util.Graphics.MathHelper.ToRadians(def.OrientationB + 180), out modelToWorld);
            Matrix worldToModel;
            Matrix.Invert(ref modelToWorld, out worldToModel);
            def.WMOToWorld = modelToWorld;
            def.WorldToWMO = worldToModel;

            // Reposition the m2s contained within the wmos
            //WMORoot root;
            //if (!LoadedWmoRoots.TryGetValue(def.FilePath, out root))
            //{
            //    log.Error(String.Format("WMORoot file: {0} missing from the Dictionary!", def.FilePath));
            //    continue;
            //}

            //var setIndices = new List<int> { 0 };
            //if (def.DoodadSet > 0) setIndices.Add(def.DoodadSet);

            //foreach (var index in setIndices)
            //{
            //    var doodadSet = root.DoodadSets[index];
            //    for (var i = 0; i < doodadSet.InstanceCount; i++)
            //    {
            //        var dDef = root.DoodadDefinitions[doodadSet.FirstInstanceIndex + i];
            //        if (string.IsNullOrEmpty(dDef.FilePath)) continue;

            //        M2Model model;
            //        if (!LoadedM2Models.TryGetValue(dDef.FilePath, out model))
            //        {
            //            log.Error(String.Format("M2Model file: {0} missing from the Dictionary!", dDef.FilePath));
            //            continue;
            //        }

            //        // Calculate and store the models' transform matrices
            //        Matrix scaleMatrix;
            //        Matrix.CreateScale(dDef.Scale, out scaleMatrix);

            //        Matrix rotMatrix;
            //        Matrix.CreateFromQuaternion(ref dDef.Rotation, out rotMatrix);

            //        Matrix modelToWMO;
            //        Matrix.Multiply(ref scaleMatrix, ref rotMatrix, out modelToWMO);

            //        Matrix wmoToModel;
            //        Matrix.Invert(ref modelToWMO, out wmoToModel);
            //        dDef.ModelToWMO = modelToWMO;
            //        dDef.WMOToModel = wmoToModel;

            //        // Calculate the wmoSpace bounding box for this model
            //        var wmoSpaceVecs = new List<Vector3>(model.BoundingVertices.Length);
            //        for (var j = 0; j < model.BoundingVertices.Length; j++)
            //        {
            //            Vector3 rotated;
            //            Vector3.Transform(ref model.BoundingVertices[i], ref modelToWMO, out rotated);

            //            Vector3 final;
            //            Vector3.Add(ref rotated, ref dDef.Position, out final);
            //            wmoSpaceVecs.Add(final);
            //        }

            //        dDef.Extents = new BoundingBox(wmoSpaceVecs.ToArray());
            //        def.M2Refs.Add(dDef);
            //    }
            //}
        }

	    private static void PrepareM2Info(MapDoodadDefinition def, M2Model model)
        {
            TerrainConstants.TilePositionToWorldPosition(ref def.Position);

            Matrix scaleMatrix;
            Matrix.CreateScale(def.Scale, out scaleMatrix);

            var rotateZ = Matrix.CreateRotationZ(MathHelper.ToRadians(def.OrientationB + 180));
            var rotateY = Matrix.CreateRotationY(MathHelper.ToRadians(def.OrientationA));
            var rotateX = Matrix.CreateRotationX(MathHelper.ToRadians(def.OrientationC));

            var modelToWorld = Matrix.Multiply(scaleMatrix, rotateZ);
            modelToWorld = Matrix.Multiply(modelToWorld, rotateX);
            modelToWorld = Matrix.Multiply(modelToWorld, rotateY);
            def.ModelToWorld = modelToWorld;

            Matrix worldToModel;
            Matrix.Invert(ref modelToWorld, out worldToModel);
            def.WorldToModel = worldToModel;

            CalculateModelBounds(def, model);

        }

        private static void CalculateModelBounds(MapDoodadDefinition def, M2Model model)
        {
            var vecs = new List<Vector3>(model.BoundingVertices);
            for (var i = 0; i < model.BoundingVertices.Length; i++)
            {
                var vec = model.BoundingVertices[i];
                
                Vector3 rotatedVec;
                Vector3.Transform(ref vec, ref def.ModelToWorld, out rotatedVec);

                Vector3 finalVec;
                Vector3.Add(ref rotatedVec, ref def.Position, out finalVec);

                vecs.Add(finalVec);
            }

            def.Extents = new BoundingBox(vecs.ToArray());
        }

	    private static void PrepareChunk(ADT adt, int chunkX, int chunkY)
	    {
	        var chunk = adt.MapChunks[chunkY, chunkX];
            if (chunk == null) return;

	        var min = float.MaxValue;
	        var max = float.MinValue;
	        foreach (var height in chunk.Heights.Heights)
	        {
	            min = Math.Min(height, min);
	            max = Math.Max(height, max);
	        }
	        chunk.IsFlat = (Math.Abs(max - min) < TerrainConstants.MinAllowedHeightDiff);

	        chunk.WaterInfo = adt.LiquidInfo[chunkY, chunkX];
	        PrepareWaterInfo(chunk.WaterInfo);

	        // Replace the model and wmo refs with their respective UniqueIds
            for (var i = 0; i < chunk.DoodadRefs.Count; i++)
            {
                var doodadRef = chunk.DoodadRefs[i];
                var def = adt.DoodadDefinitions[doodadRef];
                if (def == null || ModelsToIgnore.Contains(def.FilePath))
                {
                    chunk.DoodadRefs[i] = int.MinValue;
                    continue;
                }

                var uniqueId = def.UniqueId;
                chunk.DoodadRefs[i] = (int)uniqueId;
            }
            for (var i = 0; i < chunk.ObjectRefs.Count; i++)
            {
                var objectRef = chunk.ObjectRefs[i];
                var def = adt.ObjectDefinitions[objectRef];
                if (def == null)
                {
                    chunk.ObjectRefs[i] = int.MinValue;
                    continue;
                }

                var uniqueId = def.UniqueId;
                chunk.ObjectRefs[i] = (int)uniqueId;
            }
	    }

	    private static void PrepareWaterInfo(MH2O water)
        {
            if (water == null) return;
            if (!water.Header.Used) return;

            var min = float.MaxValue;
            var max = float.MinValue;
            foreach (var height in water.Heights)
            {
                min = Math.Min(height, min);
                max = Math.Max(height, max);
            }
            water.IsFlat = (Math.Abs(max - min) < TerrainConstants.MinAllowedHeightDiff);
        }
        #endregion

		#region Write
		public static void WriteTileInfo(FileStream file, ADT adt)
		{
			var writer = new BinaryWriter(file);
			writer.Write(FileTypeId);

            writer.Write(adt.IsWMOOnly);
            WriteWMODefs(writer, adt.ObjectDefinitions);

            if (adt.IsWMOOnly) return;
            WriteM2Defs(writer, adt.DoodadDefinitions);

			for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
			{
				for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
				{
				    var chunk = adt.MapChunks[y, x];
                    // Whether this chunk has a height map
				    WriteChunkInfo(writer, chunk);
				}
			}
		}

        private static void WriteWMODefs(BinaryWriter writer, ICollection<MapObjectDefinition> defs)
        {
            writer.Write(defs.Count);
            foreach (var def in defs)
            {
                writer.Write(def.UniqueId);
                writer.Write(def.FilePath);
                writer.Write(def.Extents);
                writer.Write(def.Position);
                writer.Write(def.DoodadSetId);
                writer.Write(def.WorldToWMO);
                writer.Write(def.WMOToWorld);
            }
        }

	    private static void WriteChunkInfo(BinaryWriter writer, ADTChunk chunk)
	    {
	        writer.Write(chunk.IsFlat);
	        // The base height for this chunk
	        writer.Write(chunk.Header.Z);
	        // The wmos and m2s (UniqueIds) that overlap this chunk
	        WriteChunkModelRefs(writer, chunk.DoodadRefs);
	        WriteChunkObjRefs(writer, chunk.ObjectRefs);
            
            writer.Write(chunk.Header.Holes > 0);
            if (chunk.Header.Holes > 0)
            {
                WriteChunkHolesMap(writer, chunk.Header.GetHolesMap());
            }

	        // The height map
	        if (!chunk.IsFlat)
	        {
	            WriteChunkHeightMap(writer, chunk);
	        }

	        // The liquid information
            if (chunk.WaterInfo == null)
            {
                writer.Write(false);
                return;
            }

	        writer.Write(chunk.WaterInfo.Header.Used);
	        if (!chunk.WaterInfo.Header.Used) return;

	        writer.Write((ushort)chunk.WaterInfo.Header.Flags);
	        writer.Write((ushort)chunk.WaterInfo.Header.Type);
            writer.Write(chunk.WaterInfo.IsFlat);
	        writer.Write(chunk.WaterInfo.Header.HeightLevel1);
	        writer.Write(chunk.WaterInfo.Header.HeightLevel2);

	        if (chunk.WaterInfo.Header.Flags.HasFlag(MH2OFlags.Ocean)) return;
	        WriteWaterRenderBits(writer, chunk.WaterInfo.GetRenderBitMapMatrix());

	        if (chunk.WaterInfo.IsFlat) return;
	        WriteWaterHeights(writer, chunk.WaterInfo.GetMapHeightsMatrix());
	    }

        private static void WriteChunkHolesMap(BinaryWriter writer, bool[,] holes)
        {
            for(var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    writer.Write(holes[y, x]);
                }
            }
        }

        private static void WriteChunkModelRefs(BinaryWriter writer, ICollection<int> doodadIds)
	    {
	        var sparseList = new List<int>(doodadIds.Count);
	        foreach (var doodadId in doodadIds)
	        {
                if (doodadId == int.MinValue) continue;
                sparseList.Add(doodadId);
	        }

            writer.Write(sparseList);
	    }

        private static void WriteChunkObjRefs(BinaryWriter writer, List<int> objectIds)
        {
            var sparseList = new List<int>(objectIds.Count);
            foreach (var doodadId in objectIds)
            {
                if (doodadId == int.MinValue) continue;
                sparseList.Add(doodadId);
            }

            writer.Write(sparseList);
        }

	    private static void WriteWaterHeights(BinaryWriter writer, float[,] heights)
        {
            for (var x = 0; x < TerrainConstants.UnitsPerChunkSide + 1; x++)
            {
                for (var y = 0; y < TerrainConstants.UnitsPerChunkSide + 1; y++)
                {
                    writer.Write(heights[y, x]);
                }
            }
        }

        private static void WriteWaterRenderBits(BinaryWriter writer, bool[,] render)
        {
            for (var x = 0; x < TerrainConstants.UnitsPerChunkSide; x++)
            {
                for (var y = 0; y < TerrainConstants.UnitsPerChunkSide; y++)
                {
                    writer.Write(render[y, x]);
                }
            }
        }

        private static void WriteChunkHeightMap(BinaryWriter writer, ADTChunk chunk)
        {
            var heightMap = chunk.Heights.GetLowResMapMatrix();
            for (var x = 0; x < TerrainConstants.UnitsPerChunkSide + 1; x++)
            {
                for (var y = 0; y < TerrainConstants.UnitsPerChunkSide + 1; y++)
                {
                    writer.Write(heightMap[y, x]);
                }
            }
        }

        private static void WriteM2Defs(BinaryWriter writer, List<MapDoodadDefinition> defs)
        {
            var defList = new List<MapDoodadDefinition>();
            foreach (var def in defs)
            {
                if (ModelsToIgnore.Contains(def.FilePath)) continue;
                defList.Add(def);
            }
            
            writer.Write(defList.Count);
            foreach (var def in defList)
            {
                writer.Write(def.UniqueId);
                writer.Write(def.FilePath);
                writer.Write(def.Extents);
                writer.Write(def.Position);
                writer.Write(def.WorldToModel);
                writer.Write(def.ModelToWorld);
            }
        }
        #endregion

		#region Positions
		public static TileCoord GetTileXYForPos(Vector3 worldPos)
		{
			return new TileCoord
			{
				TileX = (int)((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize),
				TileY = (int)((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize)
			};
		}

		public static ChunkCoord GetChunkXYForPos(Vector3 worldPos)
		{
			var tileXFraction = ((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
			var tileYFraction = ((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);

			return new ChunkCoord
			{
				ChunkX = (int)((tileXFraction - ((int)tileXFraction)) * TerrainConstants.ChunkSize),
				ChunkY = (int)((tileYFraction - ((int)tileYFraction)) * TerrainConstants.ChunkSize)
			};
		}

		public static HeightMapCoord GetHeightMapXYForPos(Vector3 worldPos)
		{
			var tileXFraction = ((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
			var tileYFraction = ((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);

			var chunkXFraction = ((tileXFraction - ((int)tileXFraction)) * TerrainConstants.ChunkSize);
			var chunkYFraction = ((tileYFraction - ((int)tileYFraction)) * TerrainConstants.ChunkSize);

			return new HeightMapCoord
			{
				HeightMapX = (int)((chunkXFraction - ((int)chunkXFraction)) * TerrainConstants.UnitSize),
				HeightMapY = (int)((chunkYFraction - ((int)chunkYFraction)) * TerrainConstants.UnitSize)
			};
		}
		#endregion
	}

	public class TileCoord
	{
		public int TileX;
		public int TileY;
	}

	public class ChunkCoord
	{
		public int ChunkX;
		public int ChunkY;
	}

	public class HeightMapCoord
	{
		public int HeightMapX;
		public int HeightMapY;
	}
}