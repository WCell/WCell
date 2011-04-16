using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Data;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.Battlegrounds
{
	[DataHolder]
	public class BattlemasterRelation : IDataHolder
	{
		public BattlegroundId BattlegroundId;

		public NPCId BattleMasterId;


		public uint GetId()
		{
			return (uint)BattlegroundId;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			var bm = NPCMgr.GetEntry(BattleMasterId);
			if (bm == null)
			{
				ContentMgr.OnInvalidDBData("Invalid BattleMaster in: " + this);
			}
			else
			{
				var bg = BattlegroundMgr.GetTemplate(BattlegroundId);
				if (bg == null)
				{
					ContentMgr.OnInvalidDBData("Invalid Battleground in: " + this);
				}
				else
				{
					bm.BattlegroundTemplate = bg;
				}
			}
		}

		public override string ToString()
		{
			return GetType().Name +
				string.Format(" (BG: {0} (#{1}), BattleMaster: {2} (#{3})) ",
				BattlegroundId, (int)BattlegroundId, BattleMasterId, (int)BattleMasterId);
		}
	}
}