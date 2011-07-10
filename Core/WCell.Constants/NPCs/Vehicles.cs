using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.NPCs
{
	[Flags]
	public enum VehicleFlags
	{
		PreventStrafe = 0x00000001,						// Sets MOVEFLAG2_NO_STRAFE
		PreventJumping = 0x00000002,						// Sets MOVEFLAG2_NO_JUMPING
		FullSpeedTurning = 0x00000004,				// Sets MOVEFLAG2_FULLSPEEDTURNING
		Flagx8 = 0x8,
		AlwaysAllowPitching = 0x00000010,					// Sets MOVEFLAG2_ALLOW_PITCHING
		FullSpeedPitching = 0x00000020,				// Sets MOVEFLAG2_FULLSPEEDPITCHING
		CustomPitch = 0x00000040,					// If set use pitchMin and pitchMax from DBC, otherwise pitchMin = -pi/2, pitchMax = pi/2    
		Flagx80 = 0x80,
		Flagx100 = 0x100,
		Flagx200 = 0200,
		AimAngleAdjustable = 0x00000400,			// Lua_IsVehicleAimAngleAdjustable
		AimPowerAdjustable = 0x00000800,			// Lua_IsVehicleAimPowerAdjustable
	};

	[Flags]
	public enum VehicleSeatFlags : uint
	{
		None = 0x0,
		HasLowerAnimForEnter = 0x1,
        HasLowerAnimForRide = 0x2,
		Flagx4 = 0x4,
		ShouldUseVehicleSeatExitAnimationOnVoluntaryExit = 0x8,
		Flagx10 = 0x10,
		Flagx20 = 0x20,
		Flagx40 = 0x40,
		Flagx80 = 0x80,
		Flagx100 = 0x100,
		HidePassenger = 0x200,						// Passenger is hidden
		Flagx400 = 0x400,
		VehicleControlSeat = 0x800,					// Lua_UnitInVehicleControlSeat
		Flagx1000 = 0x1000,
		Uncontrolled = 0x2000,						// can override !& VEHICLE_SEAT_FLAG_CAN_ENTER_OR_EXIT
		CanAttack = 0x4000,							// Can attack, cast spells and use items from vehicle?
		ShouldUseVehicleSeatExitAnimationOnForcedExit = 0x8000,
		Flagx10000 = 0x10000,
		Flagx20000 = 0x20000,
        /// <summary>
        /// The next two may be switched
        /// </summary>
        HasVehicleExitAnimForVoluntaryExit = 0x40000,
        HasVehicleExitAnimForForcedExit = 0x80000,
		Flagx100000 = 0x100000,
        Flagx200000 = 0x200000,
        RecHasVehicleEnterAnim = 0x400000,
		Flagx800000 = 0x800000,
        EnableVehicleZoom = 0x1000000,
		CanEnterorExit = 0x2000000,				// Lua_CanExitVehicle
		CanSwitchSeats = 0x4000000,				// Lua_CanSwitchVehicleSeats
		HasStartWaitingForVehicleTransitionAnim_Enter = 0x8000000,
		HasStartWaitingForVehicleTransitionAnim_Exit = 0x10000000,
		CanCast = 0x20000000,						// Lua_UnitHasVehicleUI
		Flagx40000000 = 0x40000000,
		AllowsInteraction = 0x80000000
	};

	[Flags]
	public enum VehicleSeatFlagsB : uint
	{
		None = 0x00000000,
		Flagx1 = 0x1,
		UsableForced = 0x00000002,
		Flagx4 = 0x4,
		TargetsInRaidUI = 0x00000008,           // Lua_UnitTargetsVehicleInRaidUI
		Flagx10 = 0x10,
		Ejectable = 0x00000020,           // ejectable
		UsableForced2 = 0x00000040,
		Flagx80 = 0x80,
		UsableForced3 = 0x00000100,
		CanSwitchSeats = 0x04000000,           // can switch seats
		VehiclePlayerFrameUI = 0x80000000,           // Lua_UnitHasVehiclePlayerFrameUI - actually checked for flagsb &~ 0x80000000
	};

	public enum VehiclePowerType
	{
		None = 0,
		Unknown = 1,
		Pyrite = 41,
		Steam = 61,
		Heat = 101,
		Ooze = 121,
		Blood = 141,
		Wrath = 142
	};
}