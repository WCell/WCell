using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
