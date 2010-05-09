using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
    [Flags]
    public enum UnitFlags : uint
    {
        None = 0,
        //Unknown1 = 0x00000001,

        Flag_0_0x1 = 0x1,

        /// <summary>
        /// Can select. Cannot be attacked
        /// </summary>
        SelectableNotAttackable = 0x2,

        /// <summary>
        /// Applied in SPELL_AURA_MOD_CONFUSE (5)
        /// Applied in SPELL_AURA_MOD_FEAR (7)
        /// </summary>
        Influenced = 0x4, // Stops movement packets

        /// <summary>
        /// Enables Detailed Collision, Allows swimming
        /// When set for pets allows the popup menu to be shown (dismiss, rename, etc)
        /// </summary>
        PlayerControlled = 0x8, // 2.4.2

        /// <summary>
        /// Rename? Dead?
        /// </summary>
        Flag_0x10 = 0x10,

        /// <summary>
        /// Spells are free, and some have no reagent cost
        /// </summary>
        Preparation = 0x20, // 3.0.3

        /// <summary>
        /// Elite
        /// </summary>
        PlusMob = 0x40, // 3.0.2

        /// <summary>
        /// Can select. Cannot be attacked
        /// </summary>
        SelectableNotAttackable_2 = 0x80,

        NotAttackable = 0x100,

        Flag_0x200 = 0x200,

        /// <summary>
        /// Looting animation is shown
        /// </summary>
        Looting = 0x400,

        PetInCombat = 0x800, // 3.0.2

        Flag_12_0x1000 = 0x1000,

        Silenced = 0x2000,//3.0.3

        Flag_14_0x4000 = 0x4000,

        Flag_15_0x8000 = 0x8000,

        /// <summary>
        /// Can select. Cannot be attacked
        /// </summary>
        SelectableNotAttackable_3 = 0x10000,

        /// <summary>
        /// Pacifies the target, preventing spells from being cast that have PreventionType == Pacified
        /// </summary>
        Pacified = 0x20000,//3.0.3

        Stunned = 0x40000,

        CanPerformAction_Mask1 = 0x60000,

        /// <summary>
        /// Unit is in Combat
        /// </summary>
        Combat = 0x80000, // 3.1.1

        /// <summary>
        /// Unit is currently on a taxi
        /// </summary>
        TaxiFlight = 0x100000,// 3.1.1

        Disarmed = 0x200000, // 3.1.1

        Confused = 0x400000,//  3.0.3

        Feared = 0x800000,

        Possessed = 0x1000000, // 3.1.1

        NotSelectable = 0x2000000,

        Skinnable = 0x4000000,

        Mounted = 0x8000000,

        Flag_28_0x10000000 = 0x10000000,

        Flag_29_0x20000000 = 0x20000000,

        Flag_30_0x40000000 = 0x40000000,

        Flag_31_0x80000000 = 0x80000000,
    }

    [Flags]
    public enum UnitFlags2
    {
        FeignDeath = 0x1,
        NoModel = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8 = 0x8,
        Flag_0x10 = 0x10,
        Flag_0x20 = 0x20,
        ForceAutoRunForward = 0x40,

        /// <summary>
        /// Treat as disarmed?
        /// Treat main and off hand weapons as not being equipped?
        /// </summary>
        Flag_0x80 = 0x80,

        /// <summary>
        /// Skip checks on ranged weapon?
        /// Treat it as not being equipped?
        /// </summary>
        Flag_0x400 = 0x400,

        Flag_0x800 = 0x800,
        Flag_0x1000 = 0x1000,
    }

    [Flags]
    public enum UnitDynamicFlags
    {
        None = 0,
        Lootable = 0x1,
        TrackUnit = 0x2,
        TaggedByOther = 0x4,
        TaggedByMe = 0x8,
        SpecialInfo = 0x10,
        Dead = 0x20,
        ReferAFriendLinked = 0x40,
        IsTappedByAllThreatList = 0x80,
    }


    /// <summary>
    /// Used in UNIT_FIELD_BYTES_1, 1st byte
    /// </summary>
    public enum StandState : byte
    {
        Stand = 0,
        Sit = 1,
        SittingInChair = 2,
        /// <summary>
        /// AnimationData Id 99 or 100
        /// </summary>
        Sleeping = 3,
        /// <summary>
        /// AnimationData Id 102
        /// </summary>
        SittingInLowChair = 4,
        /// <summary>
        /// AnimationData Id 103
        /// </summary>
        SittingInMediumChair = 5,
        /// <summary>
        /// AnimationData Id 104
        /// </summary>
        SittingInHighChair = 6,
        Dead = 7,
        Kneeling = 8,
        /// <summary>
        /// 201 = submerge
        /// 
        /// </summary>
        Type9 = 9,
    }

    /// <summary>
    /// Used in UNIT_FIELD_BYTES_2, 2nd byte
    /// </summary>
    [Flags]
    public enum PvPState
    {
		None = 0,
        PVP = 0x1,
        FFAPVP = 0x4,
        InPvPSanctuary = 0x8,
    }

    /// <summary>
    /// Used in UNIT_FIELD_BYTES_1, 3rd byte
    /// </summary>
    [Flags]
    public enum StateFlag
    {
        None = 0,
        AlwaysStand = 0x1,
        Sneaking = 0x2,
        UnTrackable = 0x4,
    }

    /// <summary>
    /// Used in UNIT_FIELD_BYTES_2, 1st byte
    /// </summary>
    public enum SheathType : sbyte
    {
		Undetermined = -1,
        None = 0,
        Melee = 1,
        Ranged = 2,

        Shield = 4,
        Rod = 5,
        Light = 7
    }

    /// <summary>
    /// Used in UNIT_FIELD_BYTES_2, 4th byte
    /// <remarks>Values from the first column of SpellShapeshiftForm.dbc</remarks>
    /// </summary>
    public enum ShapeShiftForm
    {
        Normal = 0,
        Cat = 1,
        TreeOfLife = 2,
        Travel = 3,
        Aqua = 4,
        Bear = 5,
        Ambient = 6,
        Ghoul = 7,
        DireBear = 8,
        CreatureBear = 14,
        CreatureCat = 15,
        GhostWolf = 16,
        BattleStance = 17,
        DefensiveStance = 18,
        BerserkerStance = 19,
        EpicFlightForm = 27,
        Shadow = 28,
        Stealth = 30,
        Moonkin = 31,
        SpiritOfRedemption = 32
    }
}
