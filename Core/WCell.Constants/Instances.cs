using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	public enum DungeonDifficulty
	{
		Normal,
		Heroic
	}

	public enum RaidDifficulty
	{
		Normal10,
		Normal25,
		Heroic10,
		Heroic25,
		End
	}

	public enum InstanceResetFailed
	{
		PlayersInside,
		PlayersOffline,
		PlayersZoning
	}
}
