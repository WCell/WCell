using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Updates
{
	/// <summary>
	/// Update Types used in SMSG_UPDATE_OBJECT inside realm server
	/// </summary>
	public enum UpdateType : byte
	{
		Values = 0,
		Movement = 1,
		Create = 2,
		CreateSelf = 3,
		OutOfRange = 4,
		Near = 5
	}

	[Flags]
	public enum UpdateFlags : uint
	{
		Self = 0x1,
		Transport = 0x2,
		/// <summary>
		/// Attack Target
		/// </summary>
		AttackingTarget = 0x4,
		/// <summary>
		/// Value depends on object type
		/// 
		/// Items, Containers, GameObjects, DynamicObjects, and Corpses its high 4 bytes of guid
		/// </summary>
		Flag_0x8 = 0x8,
		/// <summary>
		/// Value depends on object type
		/// 
		/// Items, Containers, GameObjects, DynamicObjects, and Corpses its low 4 bytes of guid
		/// </summary>
		Flag_0x10 = 0x10,
		/// <summary>
		/// Mobile Objects
		/// </summary>
		Living = 0x20,
		/// <summary>
		/// Stationary Objects
		/// </summary>
		StationaryObject = 0x40,
		/// <summary>
		/// For Vehicles
		/// Int: VehicleId (from Vehicle.dbc)
		/// Float: Aim Adjustment
		/// </summary>
		Vehicle = 0x80,
		/// <summary>
		/// Appears to be stationary objects on transports
		/// </summary>
		StationaryObjectOnTransport = 0x100,

		/// <summary>
		/// This is an ulong that used to be in the GAMEOBJECT_ROTATION updatefield
		/// It is 3 values packed together, but of unknown use.
		/// </summary>
		HasRotation = 0x200,
	}

	/// <summary>
	/// Object Type Ids are used in SMSG_UPDATE_OBJECT inside realm server
	/// </summary>
	public enum ObjectTypeId
	{
		Object = 0,
		Item = 1,
		Container = 2,
		Unit = 3,
		Player = 4,
		GameObject = 5,
		DynamicObject = 6,
		Corpse = 7,
		AIGroup = 8,
		AreaTrigger = 9,
		Count,
		None = 0xFF
	}

	[Flags]
	public enum ObjectTypes : uint
	{
		None = 0,
		Object = 0x1,
		Item = 0x2,
		Container = 0x4,
		/// <summary>
		/// Any unit
		/// </summary>
		Unit = 0x8,
		Player = 0x10,
		GameObject = 0x20,
		/// <summary>
		/// Any Unit or GameObject
		/// </summary>
		Attackable = 0x28,
		DynamicObject = 0x40,
		Corpse = 0x80,
		AIGroup = 0x100,
		AreaTrigger = 0x200,
		All = 0xFFFF,
	}

	/// <summary>
	/// Custom enum to enable further distinction between Units
	/// </summary>
	[Flags]
	public enum ObjectTypeCustom
	{
		None = 0,
		Object = 0x1,
		Item = 0x2,
		Container = 0x6,
		/// <summary>
		/// Any unit
		/// </summary>
		Unit = 0x8,
		Player = 0x10,
		GameObject = 0x20,
		/// <summary>
		/// Any Unit or GameObject
		/// </summary>
		Attackable = 0x28,
		DynamicObject = 0x40,
		Corpse = 0x80,
		AIGroup = 0x100,
		AreaTrigger = 0x200,
		NPC = 0x1000,
		Pet = 0x2000,
		All = 0xFFFF
	}
}