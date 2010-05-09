using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Skills;

namespace WCell.Constants.Items
{
	public static class ItemProfessions
	{
		public static readonly ItemSubClassMask[] ArmorProfessionSubClasses = new ItemSubClassMask[(int)SkillId.End];
		public static readonly ItemSubClassMask[] WeaponProfessionSubClasses = new ItemSubClassMask[(int)SkillId.End];

		static ItemProfessions()
		{
			WeaponProfessionSubClasses[(int)SkillId.Axes] = ItemSubClassMask.WeaponAxe;
			WeaponProfessionSubClasses[(int)SkillId.Bows] = ItemSubClassMask.WeaponBow;
			WeaponProfessionSubClasses[(int)SkillId.Crossbows] = ItemSubClassMask.WeaponCrossbow;
			WeaponProfessionSubClasses[(int)SkillId.Daggers] = ItemSubClassMask.WeaponDagger;
			WeaponProfessionSubClasses[(int)SkillId.Fishing] = ItemSubClassMask.WeaponFishingPole;
			WeaponProfessionSubClasses[(int)SkillId.Unarmed] = ItemSubClassMask.WeaponFist;
			WeaponProfessionSubClasses[(int)SkillId.Guns] = ItemSubClassMask.WeaponGun;
			WeaponProfessionSubClasses[(int)SkillId.Swords] = ItemSubClassMask.WeaponOneHandSword;
			WeaponProfessionSubClasses[(int)SkillId.Polearms] = ItemSubClassMask.WeaponPolearm;
			WeaponProfessionSubClasses[(int)SkillId.Staves] = ItemSubClassMask.WeaponStaff;
			WeaponProfessionSubClasses[(int)SkillId.Thrown] = ItemSubClassMask.WeaponThrown;
			WeaponProfessionSubClasses[(int)SkillId.TwoHandedAxes] = ItemSubClassMask.WeaponTwoHandAxe;
			WeaponProfessionSubClasses[(int)SkillId.TwoHandedMaces] = ItemSubClassMask.WeaponTwoHandMace;
			WeaponProfessionSubClasses[(int)SkillId.TwoHandedSwords] = ItemSubClassMask.WeaponTwoHandSword;
			WeaponProfessionSubClasses[(int)SkillId.Wands] = ItemSubClassMask.WeaponWand;

			ArmorProfessionSubClasses[(int)SkillId.Cloth] = ItemSubClassMask.ArmorCloth;
			ArmorProfessionSubClasses[(int)SkillId.Cloth] = ItemSubClassMask.ArmorCloth;
			ArmorProfessionSubClasses[(int)SkillId.Leather] = ItemSubClassMask.ArmorLeather;
			ArmorProfessionSubClasses[(int)SkillId.Mail] = ItemSubClassMask.ArmorMail;
			ArmorProfessionSubClasses[(int)SkillId.PlateMail] = ItemSubClassMask.ArmorPlate;
			ArmorProfessionSubClasses[(int)SkillId.Shield] = ItemSubClassMask.ArmorShield;
		}

		public static readonly SkillId[] WeaponSubClassProfessions = new[] {
			SkillId.Axes,
			SkillId.TwoHandedAxes,
			SkillId.Bows,
			SkillId.Guns,
			SkillId.Maces,
			SkillId.TwoHandedMaces,
			SkillId.Polearms,
			SkillId.Swords,
			SkillId.TwoHandedSwords,
			SkillId.None,
			SkillId.Staves,
			SkillId.None,
			SkillId.None,
			SkillId.FistWeapons,
			SkillId.None,
			SkillId.Daggers,
			SkillId.Thrown,
			SkillId.None,
			SkillId.Crossbows,
			SkillId.Wands,
			SkillId.Fishing
		};

		public static readonly SkillId[] ArmorSubClassProfessions = new[] {
			SkillId.None,
			SkillId.Cloth,
			SkillId.Leather,
			SkillId.Mail,
			SkillId.PlateMail,
			SkillId.None,
			SkillId.Shield
		};
	}
}
