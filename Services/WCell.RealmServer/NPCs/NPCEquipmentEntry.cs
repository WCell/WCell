using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants.Items;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.NPCs
{
	[DataHolder]
	public class NPCEquipmentEntry : IDataHolder
	{
		public uint EquipmentId;

		[Persistent(3)]
		public ItemId[] ItemIds;

		public void FinalizeDataHolder()
		{
			if (EquipmentId > 100000)
			{
				ContentHandler.OnInvalidDBData("NPCEquipmentEntry had invalid Id: " + EquipmentId);
				return;
			}
			ArrayUtil.Set(ref NPCMgr.EquipmentEntries, EquipmentId, this);
		}
	}
}