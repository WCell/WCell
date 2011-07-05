using System.IO;
using WCell.Terrain.Simple.ADT;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Serialization
{
    public static class SimpleADTReader
    {
		public static SimpleADT ReadTile(SimpleTerrain terrain, int x, int y)
        {
			var filePath = SimpleADTWriter.GetFileName(terrain.MapId, x, y);
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


				adt = new SimpleADT(x, y, terrain);

				terrain.m_IsWmoOnly = reader.ReadBoolean();
                adt.TerrainVertices = reader.ReadVector3Array();
            	adt.TerrainIndices = reader.ReadInt32Array();
            }


            return adt;
        }
    }
}
