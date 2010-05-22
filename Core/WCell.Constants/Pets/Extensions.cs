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

namespace WCell.Constants.Pets
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this PetFoodMask flags, PetFoodMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this PetFoodMask flags, PetFoodType foodType)
		{
			return (flags & (PetFoodMask)(1 << ((int)foodType-1))) != 0;
		}
		#endregion
	}
}
