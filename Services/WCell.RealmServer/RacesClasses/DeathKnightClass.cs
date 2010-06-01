using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.RacesClasses
{
	// TODO: Correct these formulas
	public class DeathKnightClass : BaseClass
	{
		public override ClassId Id
		{
			get { return ClassId.DeathKnight; }
		}

		public override int StartLevel
		{
			get
			{
				return 55;
			}
		}

		public override PowerType PowerType
		{
			get
			{
				return PowerType.RunicPower;
			}
		}

		/// <summary>
		/// Calculates attack power for the class at a specific level, Strength and Agility.
		/// </summary>
		/// <param name="level">the player's level</param>
		/// <param name="strength">the player's Strength</param>
		/// <param name="agility">the player's Agility</param>
		/// <returns>the total attack power</returns>
		public override int CalculateMeleeAP(int level, int strength, int agility)
		{
			return (level * 3 + strength * 2 - 20);
		}

		public override int CalculateRangedAP(int level, int strength, int agility)
		{
			return 0;
		}

        public override float CalculateMagicCritChance(int level, int intellect)
		{
			return (intellect / 80f) + /*(Spell Critical Strike Rating/22.08)*/ +2.2f;
		}

        /// <summary>
        /// Deathknights get 25% of their str added as parry rating.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="parryRating"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public override float CalculateParry(int level, int parryRating, int str)
        {
            return base.CalculateParry(level, (int)(parryRating + str*0.25), str);
        }
		/// <summary>
		/// Calculates the amount of power regeneration for the class at a specific level and Spirit.
		/// </summary>
		/// <param name="level">the player's level</param>
		/// <param name="spirit">the player's Spirit</param>
		/// <returns>the total power regeneration amount</returns>
        public override int CalculatePowerRegen(Character chr)
		{
			return 4 + (chr.Spirit / 5);
		}
	}
}
