using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainDisplay.Recast
{
    public class PolyMesh
    {
        public ushort[] verts;	// Vertices of the mesh, 3 elements per vertex.
        public ushort[] polys;	// Polygons of the mesh, nvp*2 elements per polygon.
        public ushort[] regs;	// Region ID of the polygons.
        public ushort[] flags;	// Per polygon flags.
        public byte[] areas;	// Area ID of polygons.
        public int nverts;				// Number of vertices.
        public int npolys;				// Number of polygons.
        public int maxpolys;			// Number of allocated polygons.
        public int nvp;				// Max number of vertices per polygon.
        public float[] bmin;
        public float[] bmax;	// Bounding box of the mesh.
        public float cs;
        public float ch;			// Cell size and height.
    }
}
