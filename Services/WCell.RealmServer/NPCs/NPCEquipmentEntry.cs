using WCell.Constants.Items;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
	[DataHolder]
	public class NPCEquipmentEntry : IDataHolder
	{
		public uint EquipmentId;

		[Persistent(3)]
		public ItemId[] ItemIds = new ItemId[3];

		public void FinalizeDataHolder()
		{
			if (EquipmentId > 100000)
			{
				ContentMgr.OnInvalidDBData("NPCEquipmentEntry had invalid Id: " + EquipmentId);
				return;
			}
			ArrayUtil.Set(ref NPCMgr.EquipmentEntries, EquipmentId, this);
		}
	}
}