using WCell.Constants.Skills;

namespace WCell.RealmServer.Items
{
	/// <summary>
	/// Any Item and Fists
	/// </summary>
	public interface IWeapon
	{
		/// <summary>
		/// A set of damages that will be applied on hit
		/// </summary>
		DamageInfo[] Damages
		{
			get;
		}

		/// <summary>
		/// The Skill that this weapon requires
		/// </summary>
		SkillId Skill
		{
			get;
		}

		/// <summary>
		/// Indicates whether this is a Melee weapon
		/// </summary>
		bool IsMelee
		{
			get;
		}

		/// <summary>
		/// Indicates whether this is a ranged weapon
		/// </summary>
		bool IsRanged
		{
			get;
		}

		/// <summary>
		/// The minimum Range of this weapon
		/// </summary>
		float MinRange
		{
			get;
		}

		/// <summary>
		/// The maximum Range of this Weapon
		/// </summary>
		float MaxRange
		{
			get;
		}

		/// <summary>
		/// The time in millis between 2 attacks
		/// </summary>
		int AttackTime
		{
			get;
		}
	}
}
