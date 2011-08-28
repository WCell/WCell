namespace WCell.Constants.Spells
{
	public enum SummonGroup
	{
		Wild = 0,
		Friendly,
		Pets,
		Controllable,
		Vehicle
	}

	/// <summary>
	/// Mostly Unknown
	/// </summary>
	public enum SummonFlags
	{
		None,
		SummonFlags0x00000001 = 0x00000001,
		SummonFlags0x00000002 = 0x00000002,
		SummonFlags0x00000004 = 0x00000004,
		SummonFlags0x00000008 = 0x00000008,
		SummonFlags0x00000010 = 0x00000010,
		SummonFlags0x00000020 = 0x00000020,
		SummonFlags0x00000040 = 0x00000040,
		SummonFlags0x00000080 = 0x00000080,
		SummonFlags0x00000100 = 0x00000100,
		SummonFlags0x00000200 = 0x00000200,
		SummonFlags0x00000400 = 0x00000400,
		SummonFlags0x00000800 = 0x00000800,
		SummonFlags0x00001000 = 0x00001000,
		SummonFlags0x00002000 = 0x00002000,
		SummonFlags0x00004000 = 0x00004000,
		SummonFlags0x00008000 = 0x00008000,
		SummonFlags0x00010000 = 0x00010000,
		SummonFlags0x00020000 = 0x00020000,
		SummonFlags0x00040000 = 0x00040000,
		SummonFlags0x00080000 = 0x00080000,
		SummonFlags0x00100000 = 0x00100000,
		SummonFlags0x00200000 = 0x00200000,
		SummonFlags0x00400000 = 0x00400000,
		SummonFlags0x00800000 = 0x00800000,
	}

	public enum SummonPropertyType
	{
		None,
		Summon,
		Guardian,
		Army,
		Totem,
		Critter,
		DeathKnight,
		Construct,
		Quest,
		SiegeVehicle,
		DrakeVehicle,
		Lightwell,
		RepairBot
	}

	/// <summary>
	/// Used in MiscValueB of SpellEffect.Summon
	/// </summary>
	public enum SummonType
	{
		None = 0,
		Critter = 41,
		Guardian = 61,
		/// <summary>
		/// Fire Totems
		/// </summary>
		TotemSlot1 = 63,
		Wild = 64,
		Possessed = 65,
		Demon = 66,
		SummonPet = 67,
		TotemSlot2 = 81,
		TotemSlot3 = 82,
		/// <summary>
		/// Air Totems
		/// </summary>
		TotemSlot4 = 83,
		Totem = 121,
		Type_181 = 181,
		Type_187 = 187,
		Type_247 = 247,
		Critter2 = 307,
		Critter3 = 407,
		Type_409 = 409,
		Type_427 = 427,
		/// <summary>
		/// Only used in steam tonk summon
		/// </summary>
		SummonAndPossess = 428,
		Guardian2 = 713,
		/// <summary>
		/// Only used for the Priest's LightWell summon
		/// </summary>
		Lightwell = 1141,
		Guardian3 = 1161,

		/// <summary>
		/// Summons the NPC exactly as in the entry, only used for ClassSkillCurseOfDoomEffect
		/// </summary>
		DoomGuard = 1221,
		Elemental = 1561,
		ForceOfNature = 1562,
		End
	}
}
