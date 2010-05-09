using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants;
using WCell.RealmServer.Content;

namespace WCell.RealmServer.RacesClasses
{
	[DataHolder]
	public class ClassLevelSetting : IDataHolder
	{
		public ClassId ClassId;

		public uint Level;

		public int Health;

		public int Mana;

		public void FinalizeDataHolder()
		{
			var clss = ArchetypeMgr.GetClass(ClassId);
			if (clss == null)
			{
				ContentHandler.OnInvalidDBData("Invalid ClassId in BaseClassSetting: {0} ({1})", ClassId, (int)ClassId);
			}
			else
			{
				ArrayUtil.Set(ref clss.Settings, Level, this);
			}
		}
	}
}
