using WCell.Util.Data;

namespace WCell.RealmServer.NPCs
{
	public interface INPCDataHolder : IDataHolder
	{
		NPCEntry Entry
		{
			get;
		}

		NPCAddonData AddonData
		{
			get;
		}
	}
}
