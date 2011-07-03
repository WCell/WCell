using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.Simple.ADT;
using WCell.Terrain.Simple.M2;
using WCell.Terrain.Simple.WMO;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Serialization
{
    public static class SimpleADTReader
    {
		public static SimpleADT ReadTile(SimpleTerrain terrain, Point2D tileCoord)
        {
			var filePath = SimpleADTWriter.GetFileName(terrain.MapId, tileCoord.X, tileCoord.Y);
            if (!File.Exists(filePath))
            {
                return null;
            }

        	SimpleADT adt;

            using(var file = File.OpenRead(filePath))
            using(var reader = new BinaryReader(file))
            {
                var fileType = reader.ReadString();
				if (fileType != SimpleADTWriter.FileTypeId)
                {
                    throw new InvalidDataException(string.Format("ADT file not in valid format: {0}", filePath));
                }

            	var version = reader.ReadInt32();
				if (version != SimpleADTWriter.Version)
				{
					// invalid version -> File is outdated
					return null;
				}


				adt = new SimpleADT(tileCoord.X, tileCoord.Y, terrain);

				terrain.m_IsWmoOnly = reader.ReadBoolean();
                adt.TerrainVertices = reader.ReadVector3Array();
            	adt.TerrainIndices = reader.ReadInt32Array();
            }


            return adt;
        }
    }
}
