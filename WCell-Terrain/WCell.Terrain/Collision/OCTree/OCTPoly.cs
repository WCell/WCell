using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision.OCTree
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OCTPoly
    {
        /// <summary>
        /// The vertices that make up this OctPoly, currently 3.
        /// </summary>
        public Vector3[] Verts;

        public OCTPoly(Vector3 a, Vector3 b, Vector3 c)
        {
            Verts = new[] { a, b, c };
        }

        public OCTPoly(OCTPoly poly)
        {
            Verts = new[] { poly.Verts[0], poly.Verts[1], poly.Verts[2] };
        }
    }
}
