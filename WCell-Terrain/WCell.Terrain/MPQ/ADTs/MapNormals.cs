using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.ADTs
{
    /// <summary>
    /// MCNR Chunk - Normal information for the MCNK
    /// </summary>
    public class MapNormals
    {
        // x, y, x format. One byte per integer. Each int is a signed int and between -127 and 127
        // with -127 being -1 and 127 being 1. This is for our normal vectors.

        /// <summary>
        /// Normal information, 3 integers per normal, one normal per vertex, 145 vertices
        /// </summary>
        public Vector3[] Normals = new Vector3[145];

        /// <summary>
        /// 145 Floats for the 9 x 9 and 8 x 8 grid of Normal data.
        /// </summary>
        public Vector3[,] GetLowResNormalMatrix()
        {
            var normals = new Vector3[9, 9];
            for (var x = 0; x < 17; x++)
            {
                if (x % 2 != 0) continue;
                for (var y = 0; y < 9; y++)
                {
                    var count = ((x / 2) * 9) + ((x / 2) * 8) + y;
                    normals[y, x / 2] = Normals[count];
                }
            }
            return normals;
        }
    }
}
