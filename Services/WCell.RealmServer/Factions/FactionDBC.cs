/*************************************************************************
 *
 *   file		: FactionDBC.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core.ClientDB;
using WCell.Util;

namespace WCell.RealmServer.Factions
{
	#region Faction.dbc
	public struct FactionEntry
	{
		public FactionId Id;
        /// <summary>
        /// m_reputationIndex
        /// </summary>
		public FactionReputationIndex FactionIndex;
		public RaceMask[] RaceMask;
	    public ClassMask[] ClassMask;
		public int[] BaseRepValue;
        public int[] BaseFlags;
		public FactionId ParentId;
		public string Name;
	}

    public class FactionConverter : AdvancedClientDBRecordConverter<FactionEntry>
    {
        public override FactionEntry ConvertTo(byte[] rawData, ref int id)
        {
            var entry = new FactionEntry();
            id = (int)(entry.Id = (FactionId)GetUInt32(rawData, 0));
            entry.FactionIndex = (FactionReputationIndex)GetInt32(rawData, 1);

            entry.RaceMask = new RaceMask[4];
            for (int i = 0; i < entry.RaceMask.Length; i++)
            {
                entry.RaceMask[i] = (RaceMask)GetUInt32(rawData, 2 + i);
            }

            entry.ClassMask = new ClassMask[4];
            for (int i = 0; i < entry.ClassMask.Length; i++)
            {
                entry.ClassMask[i] = (ClassMask)GetUInt32(rawData, 6 + i);
            }

            entry.BaseRepValue = new int[4];
            for (int i = 0; i < entry.BaseRepValue.Length; i++)
            {
                entry.BaseRepValue[i] = GetInt32(rawData, 10 + i);
            }

            entry.BaseFlags = new int[4];
            for (int i = 0; i < entry.BaseFlags.Length; i++)
            {
                entry.BaseFlags[i] = GetInt32(rawData, 14 + i);
            }

            entry.ParentId = (FactionId)GetUInt32(rawData, 18);
            entry.Name = GetString(rawData, 23);

            return entry;
        }
    }
	#endregion

	#region FactionTemplate.dbc
	public struct FactionTemplateEntry
	{
		public uint Id;
		public FactionId FactionId;
		public uint Flags;
        /// <summary>
        /// The Faction-Group mask of this faction.
        /// </summary>
		public FactionGroupMask FactionGroup;
        /// <summary>
        /// Mask of Faction-Groups this faction is friendly towards
        /// </summary>
		public FactionGroupMask FriendGroup;
        /// <summary>
        /// Mask of Faction-Groups this faction is hostile towards
        /// </summary>
		public FactionGroupMask EnemyGroup;
		public FactionId[] EnemyFactions;
		public FactionId[] FriendlyFactions;
	}

	public class FactionTemplateConverter : AdvancedClientDBRecordConverter<FactionTemplateEntry>
	{
		public override FactionTemplateEntry ConvertTo(byte[] rawData, ref int id)
		{
			var entry = new FactionTemplateEntry();
			int x = 0;
			id = (int)(entry.Id = GetUInt32(rawData, x++));
			entry.FactionId = (FactionId)GetUInt32(rawData, x++);
			entry.Flags = GetUInt32(rawData, x++);
            entry.FactionGroup = (FactionGroupMask)GetUInt32(rawData, x++);
			entry.FriendGroup = (FactionGroupMask)GetUInt32(rawData, x++);
			entry.EnemyGroup = (FactionGroupMask)GetUInt32(rawData, x++);

			entry.EnemyFactions = new FactionId[4];
			for (uint i = 0; i < entry.EnemyFactions.Length; i++)
			{
				entry.EnemyFactions[i] = (FactionId)GetUInt32(rawData, x++);
			}

			entry.FriendlyFactions = new FactionId[4];
			for (uint i = 0; i < entry.FriendlyFactions.Length; i++)
			{
				entry.FriendlyFactions[i] = (FactionId)GetUInt32(rawData, x++);
			}

			return entry;
		}
	}
	#endregion
}