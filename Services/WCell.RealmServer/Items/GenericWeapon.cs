using WCell.Constants;
using WCell.Constants.Skills;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// A weapon that can be completely customized 
	/// </summary>
	public class GenericWeapon : IWeapon
	{
		public static readonly DamageInfo[] FistDamage = new[] { new DamageInfo(DamageSchoolMask.Physical, 1f, 2f) };

		public static readonly DamageInfo[] RangedDamage = new[] { new DamageInfo(DamageSchoolMask.Physical, 1f, 2f) };

		/// <summary>
		/// Default Fists
		/// </summary>
		public static GenericWeapon Fists = new GenericWeapon(false, FistDamage, SkillId.Unarmed, 0f, Unit.DefaultMeleeDistance, 2000);

		/// <summary>
		/// Default Ranged Weapon
		/// </summary>
		public static GenericWeapon Ranged = new GenericWeapon(true, RangedDamage, SkillId.Bows, Unit.DefaultMeleeDistance, Unit.DefaultRangedDistance, 2000);

		/// <summary>
		/// No damage weapon
		/// </summary>
		public static GenericWeapon Peace = new GenericWeapon(false, FistDamage, SkillId.None, 0, 0, 10000);

		public static readonly DamageInfo[] DefaultDamage = new[] { new DamageInfo(DamageSchoolMask.Physical, 1f, 3f) };

		public GenericWeapon(bool isRanged, int damageCount)
		{
			IsRanged = isRanged;
			IsMelee = !isRanged;
			Damages = new DamageInfo[damageCount];
		}

		public GenericWeapon(float minDmg, float maxDmg)
			: this(false, minDmg, maxDmg)
		{
		}

		public GenericWeapon(bool isRanged, float minDmg, float maxDmg)
			: this(isRanged, minDmg, maxDmg, DamageSchoolMask.Physical)
		{
		}

		public GenericWeapon(bool isRanged, float minDmg, float maxDmg, DamageSchoolMask dmgType)
			: this(isRanged, minDmg, maxDmg, Fists.AttackTime, dmgType)
		{
		}

		public GenericWeapon(float minDmg, float maxDmg, int attackTime)
			: this(false, minDmg, maxDmg, attackTime)
		{
		}

		public GenericWeapon(bool isRanged, float minDmg, float maxDmg, int attackTime)
			: this(isRanged, minDmg, maxDmg, attackTime, DamageSchoolMask.Physical)
		{
		}

		public GenericWeapon(bool isRanged, float minDmg, float maxDmg, int attackTime, DamageSchoolMask dmgType)
		{
			IsRanged = isRanged;
			IsMelee = !isRanged;
			AttackTime = attackTime;
			Damages = new[] { new DamageInfo(dmgType, minDmg, maxDmg) };
		}

		public GenericWeapon(bool isRanged, DamageInfo[] damages, SkillId skill, float minRange, float maxRange, int attackTime)
		{
			IsRanged = isRanged;
			IsMelee = !isRanged;
			Damages = damages;
			Skill = skill;
			MinRange = minRange;
			MaxRange = maxRange;
			AttackTime = attackTime;
		}

		public DamageInfo[] Damages
		{
			get;
			set;
		}

		public SkillId Skill
		{
			get;
			set;
		}

		public bool IsRanged
		{
			get;
			protected set;
		}

		public bool IsMelee
		{
			get;
			protected set;
		}

		/// <summary>
		/// The minimum Range of this weapon
		/// </summary>
		public float MinRange
		{
			get;
			set;
		}

		/// <summary>
		/// The maximum Range of this Weapon
		/// </summary>
		public float MaxRange
		{
			get;
			set;
		}

		/// <summary>
		/// The time in milliseconds between 2 attacks
		/// </summary>
		public int AttackTime
		{
			get;
			set;
		}
	}
}
