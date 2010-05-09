using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Factions
{

	[Flags]
	public enum ReputationFlags
	{
		None = 0,
		Visible = 1,
		AtWar = 0x2,
        Hidden = 0x4,
        /// <summary>
        /// Always overrides the <value>Visible</value> flag
        /// </summary>
        ForcedInvisible = 0x8,
        /// <summary>
        /// Always overrides the <value>AtWar</value> flag
        /// </summary>
        ForcedPeace = 0x10,
        Inactive = 0x20,
        /// <summary>
        /// Factions from The Burning Crusade
        /// </summary>
        Flag_0x40 = 0x40,
        /// <summary>
        /// The main 2 opposing factions from Wrath of the Lich King
        /// </summary>
        Expansion_2 = 0x80,
	}
}
