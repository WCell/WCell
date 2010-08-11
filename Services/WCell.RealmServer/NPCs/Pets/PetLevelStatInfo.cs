using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Pets
{
	[DataHolder]
	public class PetLevelStatInfo : IDataHolder
	{
		public NPCId EntryId;
		public int Level;
		public int Health, Mana;
		public int Armor;

		[Persistent((int)StatType.End)]
		public int[] BaseStats = new int[(int)StatType.End];

		public void FinalizeDataHolder()
		{
			var entry = NPCMgr.GetEntry(EntryId);
			if (entry != null)
			{
				if (entry.PetLevelStatInfos == null)
				{
					entry.PetLevelStatInfos = new PetLevelStatInfo[100];
				}
				ArrayUtil.Set(ref entry.PetLevelStatInfos, (uint)Level, this);
			}
			else
			{
				LogManager.GetCurrentClassLogger().Warn("Invalid Pet entry id for PetLevelStatInfo: " + EntryId);
			}
		}
	}
}
