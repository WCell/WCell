/*************************************************************************
 *
 *   file		: DBCRecords.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-24 22:25:40 +0100 (to, 24 dec 2009) $
 *   last author	: $LastChangedBy: mokrago $
 *   revision		: $Rev: 1156 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Items;

namespace WCell.Core.DBC
{

	#region ChrClasses.dbc

	//public struct ChrClass
	//{
	//    public uint Id;
	//    public uint DamageBonusStat; // 0 = strength, 1 = agility
	//    public PowerType PowerType;
	//    public uint PetType;
	//    public string Name;
	//}

	//public class ChrClassConverter : DBCRecordConverter<ChrClass>
	//{
	//    public override ChrClass ConvertTo(byte[] rawData, ref int id)
	//    {
	//        id = GetInt32(rawData, 0);

	//        ChrClass chrClass = new ChrClass();

	//        chrClass.Id = GetUInt32(rawData, 0);
	//        chrClass.DamageBonusStat = GetUInt32(rawData, 1);
	//        chrClass.PowerType = (PowerType)GetUInt32(rawData, 2);
	//        chrClass.PetType = GetUInt32(rawData, 3);
	//        chrClass.Name = GetString(rawData, 4);

	//        return chrClass;
	//    }
	//}

	#endregion

	#region ChrRaces.dbc

	//public struct ChrRace
	//{
	//    public uint Id;
	//    public uint FactionTemplateId;
	//    public uint MaleModelId;
	//    public uint FemaleModelId;
	//    public uint TeamId;
	//    public uint CinematicId;
	//    public string Name;
	//    public ExpansionId ExpansionRequired;
	//}

	//public class ChrRaceConverter : DBCRecordConverter<ChrRace>
	//{
	//    public override ChrRace ConvertTo(byte[] rawData, ref int id)
	//    {
	//        id = GetInt32(rawData, 0);

	//        ChrRace chrRace = new ChrRace();

	//        chrRace.Id = GetUInt32(rawData, 0);
	//        chrRace.FactionTemplateId = GetUInt32(rawData, 2);
	//        chrRace.MaleModelId = GetUInt32(rawData, 4);
	//        chrRace.FemaleModelId = GetUInt32(rawData, 5);
	//        chrRace.TeamId = GetUInt32(rawData, 8);
	//        chrRace.CinematicId = GetUInt32(rawData, 13);
	//        chrRace.Name = GetString(rawData, 14);
	//        chrRace.ExpansionRequired = (ExpansionId)GetUInt32(rawData, 34);

	//        return chrRace;
	//    }
	//}

	#endregion

	#region Map.dbc
	/// <summary>
	/// Represents an entry in Map.dbc
	/// </summary>
	public class MapInfo
	{
		public uint Id;
		public string InternalName;
		public MapType MapType;
		public bool HasTwoSides;
		public string Name;
		public uint MinimumLevel;
		public uint MaximumLevel;
		public uint MaximumPlayers;
		public int Field_24;
		public float Field_25;
		public float Field_26;
		public uint AreaTableId;
		public string HordeText;
		public string AllianceText;
		public int LoadingScreen;
		public int BattlegroundLevelIncrement;
		public int Field_64;
		public float Field_65;
		public string HeroicDescription;
		public int ParentMap;//117
		public float Field_118;
		public float Field_119;
		/// <summary>
		/// In seconds
		/// </summary>
		public uint RaidResetTimer;
		/// <summary>
		/// In seconds
		/// </summary>
		public uint HeroicResetTimer;
	}
	#endregion

	#region Gt*.dbc

	public class GameTableConverter : AdvancedDBCRecordConverter<float>
	{
		public override float ConvertTo(byte[] rawData, ref int id)
		{
            return GetFloat(rawData, 0);
		}
	}

	#endregion

	#region ChatChannels.dbc

	public struct ChatChannelEntry
	{
		public uint Id;
		public ChatChannelFlags ChannelFlags;
	}

	public class ChatChannelConverter : AdvancedDBCRecordConverter<ChatChannelEntry>
	{
		public override ChatChannelEntry ConvertTo(byte[] rawData, ref int id)
		{
			id = GetInt32(rawData, 0);

			var entry = new ChatChannelEntry {
				Id = GetUInt32(rawData, 0),
				ChannelFlags = ((ChatChannelFlags)GetUInt32(rawData, 1))
			};
			return entry;
		}
	}

	#endregion

	#region CharStartOutfit.dbc

	public struct CharStartOutfit
	{
		public uint Id;
		public ClassId Class;
		public RaceId Race;
		public GenderType Gender;
		// byte unk always 0
		public uint[] ItemIds;//12
		//public uint[] StartingItemDisplayIds;//12
		public InventorySlotType[] ItemSlots;//12
	}

	public class CharStartOutfitConverter : AdvancedDBCRecordConverter<CharStartOutfit>
	{
		public override CharStartOutfit ConvertTo(byte[] rawData, ref int id)
		{
			id = GetInt32(rawData, 0);

			int currIndex = 0;

			CharStartOutfit outfit = new CharStartOutfit();
			outfit.Id = GetUInt32(rawData, currIndex++);
			uint temp = GetUInt32(rawData, currIndex++);

			outfit.Race = (RaceId)(temp & 0xFF);
			outfit.Class = (ClassId)((temp & 0xFF00) >> 8);
			outfit.Gender = (GenderType)((temp & 0xFF0000) >> 16);

			for (int i = 0; i < 12; i++)
			{
				outfit.ItemIds[i] = GetUInt32(rawData, currIndex++);
			}

			// Skip display ids
			currIndex += 12;

			for (int i = 0; i < 12; i++)
			{
				outfit.ItemSlots[i] = (InventorySlotType)GetUInt32(rawData, currIndex++);
			}

			return outfit;
		}
	}

	#endregion

	#region CharBaseInfo.dbc

	public struct CharBaseInfo
	{
		public RaceId Race;
		public ClassId Class;
	}

	public class CharBaseInfoConverter : AdvancedDBCRecordConverter<CharBaseInfo>
	{
		public override CharBaseInfo ConvertTo(byte[] rawData, ref int id)
		{
			id = 0;

			CharBaseInfo cbi = new CharBaseInfo();

			cbi.Race = (RaceId)rawData[0];
			cbi.Class = (ClassId)rawData[1];

			return cbi;
		}
	}

	#endregion
}