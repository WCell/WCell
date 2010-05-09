using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util.Graphics
{
	[Flags]
	public enum IntersectionType
	{
		NoIntersection = 0,
		Intersects = 1,
		Contained = 2,
		Touches = Intersects | Contained
	}
}