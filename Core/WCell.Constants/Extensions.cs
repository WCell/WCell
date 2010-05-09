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

		public static bool And(this NPCFlags flags, NPCFlags flag)
		{
			return (flags & flag) != 0;
		}

		public static bool And(this ZoneFlags mask, ZoneFlags matchMask)
		{
			return (mask & matchMask) != ZoneFlags.None;
		}

		public static bool And(this ProcTriggerFlags mask, ProcTriggerFlags matchMask)
		{
			return (mask & matchMask) != ProcTriggerFlags.None;
		}

		public static bool Has(this AuraStateMask mask, AuraState state)
		{
			return (mask & (AuraStateMask)(1 << ((int)state - 1))) != 0u;
		}

		public static bool Has(this DamageSchoolMask mask, DamageSchool value)
		{
			return (mask & (DamageSchoolMask)(1 << ((int)value - 1))) != 0u;
		}

		public static bool Has(this PetFoodMask mask, PetFoodType foodType)
		{
			return (mask & (PetFoodMask)(1 << ((int)foodType - 1))) != 0u;
		}

        public static bool Has(this MonsterMoveFlags flags, MonsterMoveFlags toCheck)
        {
            return (flags & toCheck) != 0;
        }

        public static bool Has(this NPCFlags flags, NPCFlags flag)
        {
            return (flags & flag) != 0;
        }

		public static bool Has(this PetFlags flags, PetFlags flag)
        {
            return (flags & flag) != 0;
        }
		
        /// <summary>
        /// Whether the given flags has any of the specified flags
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        public static bool HasAny(this HitFlags flags, HitFlags toCheck)
        {
            return (flags & toCheck) != 0;
		}

		public static bool IsPvP(this RealmServerType type)
		{
			return (type & (RealmServerType.PVP | RealmServerType.RPPVP)) != 0;
		}
		public static bool Has(this MovementFlags flags, MovementFlags toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool Has(this MovementFlags2 flags, MovementFlags2 toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool Has(this SplineFlags flags, SplineFlags toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool HasFlag(this PlayerFlags flags, PlayerFlags toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool HasFlag(this UnitFlags flags, UnitFlags toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool Has(this UpdateFlags flag, UpdateFlags toCheck)
		{
			return (flag & toCheck) != 0;
		}

		public static bool HasFlag(this NPCFlags flags, NPCFlags toCheck)
		{
			return (flags & toCheck) != 0;
		}

		public static bool HasFlag(this UnitDynamicFlags flags, UnitDynamicFlags toCheck)
		{
			return (flags & toCheck) != 0;
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

		public static bool Has(this ClassMask mask, ClassMask toCheck)
		{
			return (mask & toCheck) != 0;
		}

		public static bool Has(this ClassMask mask, ClassId toCheck)
		{
			var mask2 = toCheck.ToMask();
			return (mask & mask2) != 0;
		}

		public static bool Has(this RaceMask mask, RaceMask toCheck)
		{
			return (mask & toCheck) != 0;
		}

		public static bool HasFlag(this ItemFlags flags, ItemFlags toCheck)
		{
			return (flags & toCheck) != ItemFlags.None;
		}

		public static bool HasFlag(this PvPState flags, PvPState toCheck)
		{
			return (flags & toCheck) != PvPState.None;
		}

		public static bool Has(this GroupFlags mask, GroupFlags toCheck)
		{
			return (mask & toCheck) != 0;
		}
	}
}
