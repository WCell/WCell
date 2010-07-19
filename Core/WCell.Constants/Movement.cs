using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	public enum MovementState
	{
		Root = 1,
		Unroot = 2,
		WalkOnWater = 3,
		WalkOnLand = 4,
	}

	public enum MovementType
	{
		Walk,
		WalkBack,
		Run,
		Swim,
		SwimBack,
		Fly,
		FlyBack,
		Turn
	}


	[Flags]
	public enum MovementFlags : uint
	{
		None = 0,
		Forward = 0x1,
		Backward = 0x2,
		StrafeLeft = 0x4,
		StrafeRight = 0x8,
		Left = 0x10,
		Right = 0x20,
		PitchUp = 0x40,
		PitchDown = 0x80,
		Walk = 0x100,
		OnTransport = 0x200,
		Levitating = 0x400,

		/// <summary>
		/// Used in Movement block
		/// </summary>
		UnitFlying = 0x800,//FlyUnk1 = 0x00000800,

		/// <summary>
		/// 0x1000
		/// </summary>
		Falling = 0x1000, // 2.4.3 confirmed. Lua_IsFalling

		Flag_13_0x2000 = 0x2000,
		Flag_14_0x4000 = 0x4000,
		Flag_15_0x8000 = 0x8000,
		Flag_16_0x10000 = 0x10000,
		Flag_17_0x20000 = 0x20000,
		Flag_18_0x40000 = 0x40000,
		Flag_19_0x80000 = 0x80000,
		Flag_20_0x100000 = 0x100000,

		/// <summary>
		/// 0x200000
		/// </summary>
		Swimming = 0x200000, // 2.4.3 confirmed. Lua_IsSwimming

		Ascend = 0x400000,
		Descend = 0x800000,
		Flag_24_0x1000000 = 0x1000000,

		/// <summary>
		/// 0x2000000
		/// </summary>
		Flying = 0x2000000, // 2.4.3 confirmed for players. Lua_IsFlying

		/// <summary>
		/// 0x4000000
		/// </summary>
		Spline = 0x4000000, // ?
		SplinePath = 0x8000000,

		Waterwalking = 0x10000000,
		SafeFall = 0x20000000,
		Leviate = 0x40000000,
		Flag_31_0x80000000 = 0x80000000,

		//IsTranslating = 0xC0100F,
		IsTranslating = Forward | Backward | StrafeLeft | StrafeRight | Falling | Ascend | Descend
	}

	[Flags]
	public enum MovementFlags2 : uint
	{
		None = 0,
		/// <summary>        
		/// 0x20        
		/// </summary>        
		AlwaysAllowPitching = 0x20,
		InterpMask = MoveFlag2_10_0x400 | MoveFlag2_11_0x800 | MoveFlag2_12_0x1000,

		MoveFlag2_0_0x1 = 0x1,//0
		MoveFlag2_1_0x2 = 0x2,//1
		MoveFlag2_2_0x4 = 0x4,//2
		MoveFlag2_3_0x8 = 0x8,//3
		MoveFlag2_4_0x10 = 0x10,//4
		MoveFlag2_6_0x40 = 0x40,//6
		MoveFlag2_7_0x80 = 0x80,//7
		MoveFlag2_8_0x100 = 0x100,//8
		MoveFlag2_9_0x200 = 0x200,//9
		MoveFlag2_10_0x400 = 0x400,//10
		MoveFlag2_11_0x800 = 0x800,//11
		MoveFlag2_12_0x1000 = 0x1000,//12
		MoveFlag2_13_0x2000 = 0x2000,//13
		MoveFlag2_14_0x4000 = 0x4000,//14
		MoveFlag2_15_0x8000 = 0x8000,//15
		MoveFlag2_16_0x10000 = 0x10000,//16
		MoveFlag2_17_0x20000 = 0x20000,//17
		MoveFlag2_18_0x40000 = 0x40000,//18
		MoveFlag2_19_0x80000 = 0x80000,//19
		MoveFlag2_20_0x100000 = 0x100000,//20
		MoveFlag2_21_0x200000 = 0x200000,//21
		MoveFlag2_22_0x400000 = 0x400000,//22
		MoveFlag2_23_0x800000 = 0x800000,//23
		MoveFlag2_24_0x1000000 = 0x1000000,//24
		MoveFlag2_25_0x2000000 = 0x2000000,//25
		MoveFlag2_26_0x4000000 = 0x4000000,//26
		MoveFlag2_27_0x8000000 = 0x8000000,//27
		MoveFlag2_28_0x10000000 = 0x10000000,//28
		MoveFlag2_29_0x20000000 = 0x20000000,//29
		MoveFlag2_30_0x40000000 = 0x40000000,//30
		MoveFlag2_31_0x80000000 = 0x80000000,//31
	}

	[Flags]
	public enum SplineFlags : uint
	{
		None = 0,

		XYZ = 0x8000,
		GUID = 0x10000,
		Orientation = 0x20000
	}
}