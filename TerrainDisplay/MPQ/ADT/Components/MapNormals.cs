using Microsoft.Xna.Framework;

namespace MPQNav.MPQ.ADT.Components
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
            for (var r = 0; r < 17; r++)
            {
                if (r % 2 != 0) continue;
                for (var c = 0; c < 9; c++)
                {
                    var count = ((r / 2) * 9) + ((r / 2) * 8) + c;
                    normals[r / 2, c] = Normals[count];
                }
            }
            return normals;
        }
    }
}
