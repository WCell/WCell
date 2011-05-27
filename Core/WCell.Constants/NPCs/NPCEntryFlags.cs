using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.NPCs
{
	[Flags]
    public enum NPCEntryFlags : uint
    {
		Tamable = 0x1,

		SpiritHealer = 0x2,

        BossMob = 0x4,

        DoNotPlayWoundAnimation = 0x8,

        HideFactionTooltip = 0x10,

        Flag_0x20 = 0x20,
        Flag_0x40 = 0x40,

        CanInteractWhileDead = 0x80,

        /// <summary>
        /// Can gather herbs from mobs corpse
        /// </summary>
        CanGatherHerbs = 0x100,
        /// <summary>
        /// Can mine this mob's corpse
		/// </summary>
		CanMine = 0x200,

		DoNotLogDeath = 0x400,

        MountCombatAllowed = 0x800,

        CanAssist = 0x1000,

        IsPetBarUsed = 0x2000,

        /// <summary>
        /// This makes the client strip away the lowGuid before sending it to the combat log
        /// </summary>
        MaskUID = 0x4000,

        /// <summary>
        /// Can gather engineering materials from this mob's corpse
        /// </summary>
        CanSalvage = 0x8000,

        /// <summary>
        /// To tame this creature, you must be able to tame exotic creatures
        /// </summary>
        ExoticCreature = 0x10000,

        /// <summary>
        /// Width 0.6666666865348816, Height 2.027777910232544
        /// </summary>
        UseDefaultCollisionBox = 0x20000,
        
        CanWalk = 0x40000,

        DoesNotCollideWithMissiles = 0x00080000,

        HideNamePlate = 0x00100000,

        DoNotPlayMountedAnimations = 0x00200000,

        IsLinkAll = 0x00400000,

        InteractOnlyWithCreator = 0x00800000,

        DoNotPlayUnitEventSounds = 0x01000000,

        HasNoShadowBlob = 0x02000000,

        TreatAsRaidUnit = 0x04000000,

        ForceGossip = 0x08000000,

        /// <summary>
        /// DoNotSheathe
        /// </summary>
        CanSwim = 0x10000000,

        DoNotTargetOnInteraction = 0x20000000,

        DoNotRenderObjectName = 0x40000000,

        UnitIsQuestBoss = 0x80000000, 
    }
}