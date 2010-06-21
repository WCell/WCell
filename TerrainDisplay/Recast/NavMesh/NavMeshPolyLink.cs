using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainDisplay.Recast
{
	public class NavMeshPolyLink
	{
	    public const uint NullLink = 0xFFFFFFFF;

		public uint Reference;					// Neighbour reference.
		public uint Next;						// Index to next link.
		public byte Edge;						// Index to polygon edge which owns this link. 
		public byte Side;						// If boundary link, defines on which side the link is.
		public byte Bmin, Bmax;					// If boundary link, defines the sub edge area.
	}
}
