using System.IO;
using WCell.Constants.World;
using WCell.Terrain.Simple.M2;
using WCell.Util;

namespace WCell.Terrain.Serialization
{
    public static class SimpleM2Reader
    {
        public static SimpleM2 Process(string basePath, MapId mapId, string path)
        {
            basePath = Path.Combine(basePath, mapId.ToString());
            var filePath = Path.Combine(basePath, path);
            filePath = Path.ChangeExtension(filePath, ".m2x");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Extracted M2 file not found: {0}", filePath);
            }

            var m2 = new SimpleM2();

            using(var file = File.OpenRead(filePath))
            using(var reader = new BinaryReader(file))
            {
                var type = reader.ReadString();
                if (type != WorldObjectWriter.M2FileTypeId)
                {
                    throw new InvalidDataException(string.Format("M2x file in invalid format: {0}", filePath));
				}

				var version = reader.ReadInt32();
				if (version != ADTWriter.Version)
				{
					throw new InvalidDataException(string.Format(
						"WMO file \"{0}\" has wrong version: {1} - Expected: {2}",
						filePath, version, WorldObjectWriter.Version));
				}

                m2.Extents = reader.ReadBoundingBox();
                m2.BoundingVertices = reader.ReadVector3List();
                m2.BoundingTriangles = reader.ReadIndex3List();
            }

            return m2;
        }
    }
}
