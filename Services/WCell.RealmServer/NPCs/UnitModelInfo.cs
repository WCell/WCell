using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Util.Data;
using WCell.Util;
using WCell.RealmServer.Content;
using WCell.Constants;

namespace WCell.RealmServer.NPCs
{
	[DataHolder]
	public class UnitModelInfo : IDataHolder
	{
		public uint DisplayId;

		public float BoundingRadius, CombatReach;

		public GenderType Gender;


		public void FinalizeDataHolder()
		{
			if (DisplayId > 100000)
			{
				ContentHandler.OnInvalidDBData("ModelInfo has invalid Id: " + this);
				return;
			}
			if (CombatReach < 1.5f)
			{
				CombatReach = 1.5f;
			}
			ArrayUtil.Set(ref UnitMgr.ModelInfos, DisplayId, this);
		}

		public override string ToString()
		{
			return string.Format("{0} (Id: {1})", GetType(), DisplayId);
		}
	}
}