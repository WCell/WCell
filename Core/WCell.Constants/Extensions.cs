using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Login;
using WCell.Constants.NPCs;
using WCell.Constants.Items;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Pets;
using WCell.Constants.Updates;
using WCell.Util;

namespace WCell.Constants
{
	public static class Extensions
	{
		public static string ToString(this ObjectTypeId type, uint id)
		{
			string entryStr;
			if (type == ObjectTypeId.Item)
			{
				entryStr = (ItemId)id + " (" + id + ")";
			}
			else if (type == ObjectTypeId.Unit)
			{
				entryStr = (NPCId)id + " (" + id + ")";
			}
			else if (type == ObjectTypeId.GameObject)
			{
				entryStr = (GOEntryId)id + " (" + id + ")";
			}
			else
			{
				entryStr = id + " (" + id + ")";
			}
			return entryStr;
		}

		public static string ToString(this GOEntryId id)
		{
			return id + "(Id: " + (int)id + ")";
		}

		public static ClassMask ToMask(this ClassId clss)
		{
			return (ClassMask)(1 << ((int)clss - 1));
		}

		public static ClassId[] GetIds(this ClassMask mask)
		{
			var ids = Utility.GetSetIndices((uint) mask);
			var classIds = new ClassId[ids.Length];
			for (var i = 0; i < ids.Length; i++)
			{
				var id = ids[i];
				classIds[i] = (ClassId) (id+1);
			}
			return classIds;
		}
	}
}
