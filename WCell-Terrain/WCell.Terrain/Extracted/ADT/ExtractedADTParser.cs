using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TerrainDisplay.Collision;
using WCell.Terrain;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADT.Components;
using TerrainDisplay.Util;
using TerrainDisplay.Extracted.ADT;
using TerrainDisplay.Extracted.M2;
using TerrainDisplay.Extracted.WMO;
using WCell.Constants.World;
using WCell.Util;

namespace TerrainDisplay.Extracted
{
    public static class ExtractedADTParser
    {
        private const string FileTypeId = "ter";

        public static ExtractedADT Process(string filePath, MapId mapId, int tileX, int tileY)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("ADT file does not exist: {0}", filePath);
            }

            var adt = new ExtractedADT(mapId, tileX, tileY);

            using(var file = File.OpenRead(filePath))
            using(var br = new BinaryReader(file))
            {
                var fileType = br.ReadString();
                if (fileType != FileTypeId)
                {
                    br.Close();
                    throw new InvalidDataException(string.Format("ADT file not in valid format: {0}", filePath));
                }

                adt.IsWMOOnly = br.ReadBoolean();
                ReadWMODefs(br, adt);

                if (adt.IsWMOOnly)
                {
                    br.Close();
                    return adt;
                }

                ReadMapM2Defs(br, adt);
                ReadQuadTree(br, adt);

                adt.TerrainVertices = br.ReadVector3List();

                ReadADTChunks(br, adt);

                br.Close();
            }

            LoadQuadTree(adt);

            return adt;
        }

        private static void ReadWMODefs(BinaryReader br, ExtractedADT adt)
        {
            var count = br.ReadInt32();
            var wmoDefList = new List<ExtractedWMODefinition>(count);
            for (var i = 0; i < count; i++)
            {
                var def = new ExtractedWMODefinition {
                                                         UniqueId = br.ReadUInt32(),
                                                         FilePath = br.ReadString(),
                                                         Extents = br.ReadBoundingBox(),
                                                         Position = br.ReadVector3(),
                                                         DoodadSetId = br.ReadUInt16(),
                                                         WorldToWMO = br.ReadMatrix(),
                                                         WMOToWorld = br.ReadMatrix()
                                                     };
                wmoDefList.Add(def);
            }
            adt.WMODefs = wmoDefList;
        }

        private static void ReadMapM2Defs(BinaryReader br, ExtractedADT adt)
        {
            var count = br.ReadInt32();
            var m2DefList = new List<ExtractedMapM2Definition>(count);
            for (var i = 0; i < count; i++)
            {
                var def = new ExtractedMapM2Definition {
                                                           UniqueId = br.ReadUInt32(),
                                                           FilePath = br.ReadString(),
                                                           Extents = br.ReadBoundingBox(),
                                                           Position = br.ReadVector3(),
                                                           WorldToModel = br.ReadMatrix(),
                                                           ModelToWorld = br.ReadMatrix()
                                                       };
                m2DefList.Add(def);
            }
            adt.M2Defs = m2DefList;
        }

        private static void ReadQuadTree(BinaryReader br, ExtractedADT adt)
        {
            adt.QuadTree = QuadTree<ExtractedADTChunk>.LoadFromFile(br);
        }

        private static void ReadADTChunks(BinaryReader br, ExtractedADT adt)
        {
            var chunks = new ExtractedADTChunk[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
            for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
            {
                for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
                {
                    chunks[y, x] = ReadADTChunk(br);
                }
            }
            adt.Chunks = chunks;
        }

        private static ExtractedADTChunk ReadADTChunk(BinaryReader br)
        {
            var chunk = new ExtractedADTChunk
            {
                NodeId = br.ReadInt32(),
                IsFlat = br.ReadBoolean(),
                MedianHeight = br.ReadSingle(),
                M2References = br.ReadInt32List(),
                WMOReferences = br.ReadInt32List(),
                TerrainTris = br.ReadIndex3List(),
                HasLiquid = br.ReadBoolean()
                //HasHoles = br.ReadBoolean()
            };

            //if (chunk.HasHoles)
            //{
            //    ReadChunkHolesMap(br, chunk);
            //}

            //if (!chunk.IsFlat)
            //{
            //    ReadChunkHeightMap(br, chunk);
            //}

            if (!chunk.HasLiquid) return chunk;

            chunk.LiquidFlags = (MH2OFlags) br.ReadUInt16();
            chunk.LiquidType = (FluidType) br.ReadUInt16();
            chunk.IsLiquidFlat = br.ReadBoolean();
            chunk.MedianLiquidHeight1 = br.ReadSingle();
            chunk.MedianLiquidHeight2 = br.ReadSingle();

            if (chunk.LiquidFlags.HasFlag(MH2OFlags.Ocean)) return chunk;
            ReadChunkLiquidMap(br, chunk);

            if (chunk.IsLiquidFlat) return chunk;
            ReadChunkLiquidHeights(br, chunk);

            return chunk;
        }

        //private static void ReadChunkHolesMap(BinaryReader br, ExtractedADTChunk chunk)
        //{
        //    chunk.HolesMap = new bool[4,4];
        //    for (var x = 0; x < 4; x++)
        //    {
        //        for (var y = 0; y < 4; y++)
        //        {
        //            chunk.HolesMap[y, x] = br.ReadBoolean();
        //        }
        //    }
        //}

        //private static void ReadChunkHeightMap(BinaryReader br, ExtractedADTChunk chunk)
        //{
        //    var heightMap = new float[TerrainConstants.UnitsPerChunkSide + 1,TerrainConstants.UnitsPerChunkSide + 1];
        //    for (var x = 0; x < TerrainConstants.UnitsPerChunkSide + 1; x++)
        //    {
        //        for (var y = 0; y < TerrainConstants.UnitsPerChunkSide + 1; y++)
        //        {
        //            heightMap[y, x] = br.ReadSingle();
        //        }
        //    }
        //    chunk.HeightMap = heightMap;
        //}

        private static void ReadChunkLiquidMap(BinaryReader br, ExtractedADTChunk chunk)
        {
            var liquidMap = new bool[TerrainConstants.UnitsPerChunkSide, TerrainConstants.UnitsPerChunkSide];
            for (var x = 0; x < TerrainConstants.UnitsPerChunkSide; x++)
            {
                for (var y = 0; y < TerrainConstants.UnitsPerChunkSide; y++)
                {
                    liquidMap[y, x] = br.ReadBoolean();
                }
            }
            chunk.LiquidMap = liquidMap;
        }

        private static void ReadChunkLiquidHeights(BinaryReader br, ExtractedADTChunk chunk)
        {
            var liquidHeights = new float[TerrainConstants.UnitsPerChunkSide + 1, TerrainConstants.UnitsPerChunkSide + 1];
            for (var x = 0; x <= TerrainConstants.UnitsPerChunkSide + 1; x++)
            {
                for (var y = 0; y <= TerrainConstants.UnitsPerChunkSide + 1; y++)
                {
                    liquidHeights[y, x] = br.ReadSingle();
                }
            }
            chunk.LiquidHeights = liquidHeights;
        }

        private static void LoadQuadTree(ExtractedADT adt)
        {
            foreach (var chunk in adt.Chunks)
            {
                adt.QuadTree.Insert(chunk);
                chunk.Bounds = adt.QuadTree.Nodes[chunk.NodeId].Bounds;
            }
        }
    }
}
