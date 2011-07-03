using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.Simple.ADT;
using WCell.Terrain.Simple.M2;
using WCell.Terrain.Simple.WMO;
using WCell.Util;

namespace WCell.Terrain.Serialization
{
    public static class SimpleADTReader
    {
        public static SimpleADT ReadTile(string filePath, Terrain terrain, int tileX, int tileY)
        {
			// TODO: Fix SimpleADTReader

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("ADT file does not exist: {0}", filePath);
            }

        	SimpleADT adt;

            using(var file = File.OpenRead(filePath))
            using(var br = new BinaryReader(file))
            {
                var fileType = br.ReadString();
				if (fileType != ADTWriter.FileTypeId)
                {
                    throw new InvalidDataException(string.Format("ADT file not in valid format: {0}", filePath));
                }

            	var version = br.ReadInt32();
				if (version != ADTWriter.Version)
				{
					throw new InvalidDataException(string.Format(
						"ADT file \"{0}\" has wrong version: {1} - Expected: {2}",
						filePath, version, ADTWriter.Version));
				}


				adt = new SimpleADT(tileX, tileY, terrain);

                adt.IsWMOOnly = br.ReadBoolean();
                ReadWMODefs(br, adt);

                if (adt.IsWMOOnly)
                {
                    br.Close();
                    return adt;
                }

                ReadMapM2Defs(br, adt);
                ReadQuadTree(br, adt);

                adt.TerrainVertices = br.ReadVector3Array();

                br.Close();
            }


            return adt;
        }

        private static void ReadWMODefs(BinaryReader br, SimpleADT adt)
        {
            var count = br.ReadInt32();
            var wmoDefList = new List<SimpleWMODefinition>(count);
            for (var i = 0; i < count; i++)
            {
                var def = new SimpleWMODefinition {
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

        private static void ReadMapM2Defs(BinaryReader br, SimpleADT adt)
        {
            var count = br.ReadInt32();
            var m2DefList = new List<SimpleMapM2Definition>(count);
            for (var i = 0; i < count; i++)
            {
                var def = new SimpleMapM2Definition {
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

        private static void ReadQuadTree(BinaryReader br, SimpleADT adt)
        {
            adt.QuadTree = QuadTree<SimpleTerrainChunk>.LoadFromFile(br);
        }

    }
}
