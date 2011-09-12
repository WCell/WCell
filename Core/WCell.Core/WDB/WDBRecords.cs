/*************************************************************************
 *
 *   file		: WDBRecords.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-04 09:19:17 +0100 (on, 04 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 779 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.IO;
using WCell.Constants.NPCs;

namespace WCell.Core.WDB
{
    #region creaturecache.wdb

    public class CreatureCache
    {
        public uint Id;
        public string[] Names;          //4
        public string Description;

        public string UnkString;

        public NPCEntryFlags Flags;
        /// <summary>
        /// CreatureType.dbc
        /// </summary>
        public CreatureType Type;
        /// <summary>
        /// CreatureFamily.dbc
        /// </summary>
        public uint Family;
        public CreatureRank Rank;
        
        /// <summary>
        /// Link creature to another creature that is required for a quest.
        /// </summary>
        public uint CreatureRelation1;
        public uint CreatureRelation2;
        /// <summary>
        /// CreatureDisplayInfo.dbc
        /// </summary>
        public uint MaleDisplayId;
        /// <summary>
        /// CreatureDisplayInfo.dbc
        /// </summary>
        public uint FemaleDisplayId;
        /// <summary>
        /// CreatureDisplayInfo.dbc
        /// </summary>
        public uint DisplayId3;
        /// <summary>
        /// CreatureDisplayInfo.dbc
        /// </summary>
        public uint DisplayId4;

        public float HpModifier;
        public float ManaModifier;

        public byte RacialLeader;

        public uint[] QuestItem;        // 6

        /// <summary>
        /// CreatureMovementInfo.dbc
        /// </summary>
        public uint MovementInfo;
    }

    public class CreatureCacheConverter : WDBRecordConverter<CreatureCache>
    {
        public override CreatureCache Convert(BinaryReader binReader)
        {
            CreatureCache cache = new CreatureCache();

            cache.Id = binReader.ReadUInt32();

            int entryLength = binReader.ReadInt32();

            cache.Names = new string[4];
            for (int i = 0; i < cache.Names.Length; i++)
            {
                cache.Names[i] = binReader.ReadCString();
            }

            cache.Description = binReader.ReadCString();

            cache.UnkString = binReader.ReadCString();

            cache.Flags = (NPCEntryFlags)binReader.ReadUInt32();

            cache.Type = (CreatureType)binReader.ReadUInt32();
            cache.Family = binReader.ReadUInt32();
            cache.Rank = (CreatureRank)binReader.ReadUInt32();
            cache.CreatureRelation1 = binReader.ReadUInt32();
            cache.CreatureRelation2 = binReader.ReadUInt32();
            cache.MaleDisplayId = binReader.ReadUInt32();
            cache.FemaleDisplayId = binReader.ReadUInt32();
            cache.DisplayId3 = binReader.ReadUInt32();
            cache.DisplayId4 = binReader.ReadUInt32();

            cache.HpModifier = binReader.ReadSingle();
            cache.ManaModifier = binReader.ReadSingle();

            cache.RacialLeader = binReader.ReadByte();

            cache.QuestItem = new uint[6];
            for (int i = 0; i < cache.QuestItem.Length; i++)
            {
                cache.QuestItem[i] = binReader.ReadUInt32();
            }

            cache.MovementInfo = binReader.ReadUInt32();

            return cache;
        }
    }

    #endregion
}