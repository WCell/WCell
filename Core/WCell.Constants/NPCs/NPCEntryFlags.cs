using System;

namespace WCell.Constants.NPCs
{
	[Flags]
    public enum NPCEntryFlags : uint
    {
		Tamable = 0x1,
		SpiritHealer = 0x2,

        /// <summary>
        /// Can gather herbs from mobs corpse
        /// </summary>
        CanGatherHerbs = 0x100,
        /// <summary>
        /// Can mine this mob's corpse
		/// </summary>
		CanMine = 0x200,

		NPCFlag0x400 = 0x400,

        /// <summary>
        /// To tame this creature, you must be able to tame exotic creatures
        /// </summary>
        ExoticCreature = 0x10000,

        /// <summary>
        /// This makes the client strip away the lowGuid before sending it to the combat log
        /// </summary>
        Flag_0x4000,
        
        /// <summary>
        /// Can gather engineering materials from this mob's corpse
        /// </summary>
        CanSalvage = 0x8000,

        CanWalk = 0x00040000,
        CanSwim = 0x10000000,
        
    }
}