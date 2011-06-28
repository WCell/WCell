using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TerrainDisplay.Util;
using WCell.Constants.World;

namespace TerrainDisplay.Extracted.M2
{
    public static class ExtractedM2Parser
    {
        private const string fileType = "m2x";
        
        public static ExtractedM2 Process(string basePath, MapId mapId, string path)
        {
            basePath = Path.Combine(basePath, mapId.ToString());
            var filePath = Path.Combine(basePath, path);
            filePath = Path.ChangeExtension(filePath, ".m2x");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Extracted M2 file not found: {0}", filePath);
            }

            var m2 = new ExtractedM2();

            using(var file = File.OpenRead(filePath))
            using(var br = new BinaryReader(file))
            {
                var type = br.ReadString();
                if (type != fileType)
                {
                    br.Close();
                    throw new InvalidDataException(string.Format("M2x file in invalid format: {0}", filePath));
                }

                m2.Extents = br.ReadBoundingBox();
                m2.BoundingVertices = br.ReadVector3List();
                m2.BoundingTriangles = br.ReadIndex3List();

                br.Close();
            }

            return m2;
        }
    }
}
