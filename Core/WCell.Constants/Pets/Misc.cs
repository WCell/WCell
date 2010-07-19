using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WCell.Constants.Pets
{

	public enum PetAction : ushort
	{
		Stay = 0,
		Follow = 1,
		Attack = 2,
		Abandon = 3
	}

	[Flags]
	public enum PetActionType : byte
	{
		CastSpell = 1,
		CastSpell2 = 8,
		CastSpell3 = 9,
		CastSpell4 = 10,
		CastSpell5 = 11,
		CastSpell6 = 12,
		CastSpell7 = 13,
		CastSpell8 = 14,
		CastSpell9 = 15,
		CastSpell10 = 16,
		CastSpell11 = 17,

		/// <summary>
		///  (actionId is PetAttackMode)
		/// </summary>
		SetMode = 6,
		/// <summary>
		/// (actionId is PetAction)
		/// </summary>
		SetAction = 7,

        IsAutoCastEnabled = 0x40,
        IsAutoCastAllowed = 0x80,
	}

	public enum PetAttackMode : byte
	{
		Passive = 0,
		Defensive = 1,
		Aggressive = 2
	}

	[Flags]
	public enum PetSpellState : ushort
	{
		None,
		Default = 0x100,

		Unk2 = 0xC100
	}

    [Flags]
	public enum PetFlags : ushort
	{
		None = 0x0,
        Stabled = 0x1
	}

	/// <summary>
	/// Used in UNIT_FIELD_BYTES_2, 3rd byte 
	/// </summary>
	[Flags]
	public enum PetState
	{
		CanBeRenamed = 0x1,
		CanBeAbandoned = 0x2,
	}

	public enum PetFoodType
	{
		None = 0,
		Meat = 1,
		Fish = 2,
		Cheese = 3,
		Bread = 4,
		Fungus = 5,
		Fruit = 6,
		RawMeat = 7,
		RawFish = 8
	}

	[Flags]
	public enum PetFoodMask : uint
	{
		Meat = 1,
		Fish = 2,
		Cheese = 4,
		Bread = 8,
		Fungus = 0x0010,
		Fruit = 0x0020,
		RawMeat = 0x0040,
		RawFish = 0x0080
	}

	public struct PetSpellInfo
	{
		public ushort Spell;
		public PetSpellState State;
	}

    [StructLayout(LayoutKind.Sequential)]
	public struct PetMode
	{
		public PetAttackMode AttackMode;

		/// <summary>
		/// (setting this also sets Flags | 0x8000, g_PetMode = (PetAction << 8) | g_PetMode & 0x80000FF)
		/// </summary>
		public PetAction PetAction;

		public PetFlags Flags;
	}
    
    public enum PetType
    {
        Minion,
        Guardian,
        Pet,
        None
    }
}