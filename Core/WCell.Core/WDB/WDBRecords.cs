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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using WCell.Constants;
using WCell.Constants.NPCs;

namespace WCell.Core.WDB
{
    #region creaturecache.wdb

    public class CreatureCache
    {
        public uint Id;
        public string[] Names;//4
        public string Description;

        public string UnkString;

        public NPCEntryFlags Flags;
        /// <summary>
        /// CreatureType.dbc
        /// </summary>
        public NPCType Type;
        /// <summary>
        /// CreatureFamily.dbc
        /// </summary>
        public uint Family;
        public CreatureRank Rank;
        public uint UnkField;
        /// <summary>
        /// CreatureSpellData.dbc
        /// </summary>
        public uint SpellDataId;
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

        public float UnkFloat1;
        public float UnkFloat2;

        public byte UnkByte;
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

            cache.Type = (NPCType)binReader.ReadUInt32();
            cache.Family = binReader.ReadUInt32();
            cache.Rank = (CreatureRank)binReader.ReadUInt32();
            cache.UnkField = binReader.ReadUInt32();
            cache.SpellDataId = binReader.ReadUInt32();
            cache.MaleDisplayId = binReader.ReadUInt32();
            cache.FemaleDisplayId = binReader.ReadUInt32();
            cache.DisplayId3 = binReader.ReadUInt32();
            cache.DisplayId4 = binReader.ReadUInt32();

            cache.UnkFloat1 = binReader.ReadSingle();
            cache.UnkFloat2 = binReader.ReadSingle();

            cache.UnkByte = binReader.ReadByte();

            return cache;
        }
    }

    #endregion
}