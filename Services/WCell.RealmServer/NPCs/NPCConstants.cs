namespace WCell.RealmServer.NPCs
{
	/// <summary>
	/// </summary>
	public static class NPCConstants
	{
		//public static readonly uint[] DefaultHealth = new uint[] {
		//    42, 55, 71, 86, 102, 120, 137, 156, 176, 198, 222,
		//    247, 273, 300, 328, 356, 386, 417, 449, 484, 521,
		//    562, 605, 651, 699, 750, 800, 853, 905, 955, 1006,
		//    1057, 1110, 1163, 1220, 1277, 1336, 1395, 1459, 1524, 1585,
		//    1651, 1716, 1782, 1848, 1919, 1990, 2062, 2138, 2215, 2292,
		//    2371, 2453, 2533, 2614, 2699, 2784, 2871, 2961, 3052, 3144,
		//    3237, 3331, 3425, 3524, 3624, 3728, 3834, 3941, 4050
		//};

		//public static readonly uint[] DefaultArmor = new uint[] {
		//    20, 21, 46, 82, 126, 180, 245, 322, 412, 518, 545,
		//    580, 615, 650, 685, 721, 756, 791, 826, 861, 897,
		//    932, 967, 1002, 1037, 1072, 1108, 1142, 1177, 1212, 1247,
		//    1283, 1317, 1353, 1387, 1494, 1607, 1724, 1849, 1980, 2117,
		//    2262, 2414, 2574, 2742, 2798, 2853, 2907, 2963, 3018, 3072,
		//    3128, 3183, 3237, 3292, 3348, 3402, 3457, 3512, 3814, 4113,
		//    4410, 4708, 5006, 5303, 5601, 5900, 6197, 6495, 6807
		//};

		///// <summary>
		///// Base Str, Agi, Int, Spi etc
		///// </summary>
		//public static readonly uint[] DefaultStats = new uint[] { 
		//    20, 20, 21, 21, 21, 21, 22, 22, 22, 23, 23, 23, 24, 24, 24, 25, 25, 
		//    25, 26, 26, 26, 27, 27, 28, 28, 28, 29, 29, 30, 30, 30, 31, 31, 32, 
		//    32, 33, 33, 34, 34, 35, 35, 35, 36, 36, 37, 37, 38, 38, 39, 39, 40,
		//    40, 41, 41, 42, 42, 43, 43, 44, 44, 45, 46, 46, 47, 47, 48, 49, 49, 50, 50, 51 };

		///// <summary>
		///// 
		///// </summary>
		//public static uint GetValue(uint level, uint[] values)
		//{
		//    if (level > values.Length)
		//    {
		//        level = (uint)values.Length;
		//    }
		//    return values[level - 1];
		//}

		//public static uint GetHealth(uint level)
		//{
		//    if (level > DefaultHealth.Length)
		//    {
		//        uint delta = level - (uint)DefaultHealth.Length;
		//        return DefaultHealth[DefaultHealth.Length - 1] + (delta * 120);
		//    }
		//    return DefaultHealth[level - 1];
		//}

		//public static uint GetArmor(uint level)
		//{
		//    if (level > DefaultArmor.Length)
		//    {
		//        var delta = level - (uint)DefaultArmor.Length;
		//        return DefaultArmor[DefaultArmor.Length - 1] + (delta * 350);
		//    }
		//    return DefaultArmor[level - 1];
		//}

		//public static uint GetStats(uint level)
		//{
		//    if (level > DefaultStats.Length)
		//    {
		//        var delta = level - (uint)DefaultStats.Length;
		//        return DefaultStats[DefaultStats.Length - 1] + delta;
		//    }
		//    return DefaultStats[level - 1];
		//}

		/*
		 * 
// Two Handed Weapons: (Speed 3.2)
{2,4} // 1
{3,5} // 2
{4,6} // 3
{5,8} // 4
{6,10} // 5
{8,12} // 6
{9,15} // 7
{11,17} // 8
{12,19} // 9
{14,22} // 10
{16,25} // 11
{18,28} // 12
{19,30} // 13
{21,32} // 14
{23,35} // 15
{25,38} // 16
{26,40} // 17
{28,43} // 18
{30,45} // 19
{31,47} // 20
{33,49} // 21
{34,51} // 22
{36,53} // 23
{38,55} // 24
{40,58} // 25
{41,61} // 26
{43,63} // 27
{44,65} // 28
{45,68} // 29
{47,71} // 30
{49,74} // 31
{51,77} // 32
{53,80} // 33
{55,83} // 34
{57,86} // 35
{59,89} // 36
{61,92} // 37
{63,95} // 38
{65,99} // 39
{67,102} // 40
{70,106} // 41
{73,110} // 42
{76,114} // 44
{79,119} // 44
{82,124} // 45
{85,129} // 46
{88,134} // 47
{91,139} // 48
{94,145} // 49
{97,149} // 50
{100,153} // 51
{103,157} // 52
{106,161} // 55
{109,165} // 54
{112,169} // 55
{115,173} // 56
{118,177} // 57
{122,181} // 58
{126,185} // 59
{129,189} // 60
{133,193} // 61
{137,197} // 62
{141,201} // 66
{145,205} // 64
{149,210} // 66
{153,215} // 66
{157,220} // 67
{161,225} // 68
{166,230} // 69
{170,236} // 70
		 */


		/*
		 * /*
---------------------------------------
----------- 1 Hand Weapons ------------
---------------------------------------
// ---------------------
// --- Daggers/Knife ---
	13639426
	33492738
	218173186
 
// ---------------------
// -- One-Handed Axes --
	218169346
 
// ---------------------
// - One-Handed Swords -
	33490690
	218170882
	218171138
	285280258
	352388866
	369166082
 
// ---------------------
// -- One-Handed Maces -
	352453634
 
// ---------------------
// Engeneer Tools (Those are 1 hand weapon)
	50267138
	218235906
 
// ---------------------
// ---- Fist Weapons ---
	218172674
 
// ---------------------
// -- Off-hand Frills --
	33488900
	33492482
	385941508
 
// ---------------------
// ------ Shields ------
	33490436
	234948100
 
---------------------------------------
----------- 2 Hand Weapons ------------
---------------------------------------
// ---------------------
// -- Two-Handed Axes --
	33488898
	33489154
	33490178
	285278466
 
// ---------------------
// - Two-Handed Swords -
	33490946
	285280002
 
// ---------------------
// -- Two-Handed Maces -
	33490434
	50267394
	285345026
	285279746
 
// ---------------------
// ------ Staves -------
	285280770
	285346306
	50268674
 
*/
	}

	public enum TaxiActivateResponse : uint
	{
		Ok = 0,
		InvalidChoice = 1,
		NotAvailable = 2,
		InsufficientFunds = 3,
		NoPathNearby = 4,
		NoVendorNearby = 5,
		NodeNotActive = 6,
		PlayerBusy = 7,
		PlayerAlreadyMounted = 8,
		PlayerShapeShifted = 9,
		PlayerMoving = 10,
		SameNode = 11,
		PlayerNotStanding = 12
	}

	public enum BuyBankBagResponse
	{
		LimitReached = 0,
		CantAfford = 1,
		NotABanker = 2,
        Ok = 3
	}

    public enum TrainerBuyError
    {
        /// <summary>
        /// "Trainer service %d unavailable"
        /// </summary>
        TrainerServiceUnavailable = 0,
        /// <summary>
        /// "Not enough money for trainer service %d"
        /// </summary>
        InsufficientFunds = 1,
        /// <summary>
        /// "Not enough skill points for trainer service %d"
        /// </summary>
        NotEnoughSkillPoints = 2,
    }

	public enum TrainerType : byte
	{
		Class = 0x00,
		Mounts = 0x01,
		Professions = 0x02,
		Pets = 0x03,
		NotATrainer = 0xFF
	}

	public enum TrainerSubType : byte
	{
		Weapons = 0x00,

		Fishing = 0x02,

		// Why are these the same?
		Herbalism = 0x04,
		Shaman = 0x04,
		Skinning = 0x04,

		// Why are these the same?
		Druid = 0x05,
		Rogue = 0x05,
		Priest = 0x05,
		Warlock = 0x05,
		Warrior = 0x05,

		// Why are these the same?
		Paladin = 0x06,
		Mage = 0x06,

		FirstAid = 0x07,

		Cooking = 0x0A,

		Mining = 0x0D,

		Alchemy = 0x2C,
		Pet = 0x38,
		Engineering = 0x44,
		Enchanting = 0x48,

		Leatherworking = 0x4C,

		Blacksmithing = 0x4E,

		Tailoring = 0x65,

		Hunter = 0x90,
		
		NotATrainer = 0xFF
	}

	public enum TrainerSpellState
	{
		/// <summary>
		/// Green in the client Spell List
		/// </summary>
		Available = 0,
		/// <summary>
		/// Red in the client Spell List
		/// </summary>
		Unavailable = 1,
		/// <summary>
		/// Gray in the client Spell List
		/// </summary>
		AlreadyLearned = 2
	}

	public enum AuctionHouseFaction : uint
	{
		Alliance = 2,
		Horde = 6,
		Neutral = 7
	}

	public enum AuctionError : uint
	{
		Ok = 0,
		InternalError = 2,
		NotEnoughMoney = 3,
		ItemNotFound = 4,
		CannotBidOnOwnAuction = 10
	}

	public enum AuctionAction : uint
	{
		SellItem = 0,
		CancelAuction = 1,
		PlaceBid = 2
	}
}