using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	public enum GossipMenuIcon : byte
	{
		Talk = 0,
		Trade = 1,
		Taxi = 2,
		Train = 2,
		Resurrect = 3,
		Bind = 4,
		Bank = 5,
		Guild = 6,
		Tabard = 7,
		Battlefield = 8,
		End
	}

	[Flags]
	public enum GossipPOIFlags : uint
	{
		None = 0
	}
}