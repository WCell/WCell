using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Formulas
{
    public static class FallDamageGenerator
    {
        /// <summary>
        /// The coefficient the distance fallen is multiplied by
        /// </summary>
        public static float DefaultFallDamageCoefficient = 0.018f;

        /// <summary>
        /// The amount substracted from the fall damage
        /// </summary>
        public static float DefaultFallDamageReduceAmount = 0.2426f;

        /// <summary>
        /// The coefficient the final fall damage is multiplied by
        /// </summary>
        public static float DefaultFallDamageRate = 1.0f;

        /// <summary>
        /// The amount of damage inflicted to a character for fall
        /// </summary>
        public static Func<Character, float, int> GetFallDmg = (Character chr, float fallenDistance) =>
            {
                float distanceMinusSafeFall = fallenDistance - chr.SafeFall;
                float damageInHPPercents = DefaultFallDamageCoefficient * distanceMinusSafeFall - DefaultFallDamageReduceAmount;

                if (damageInHPPercents < 0)
                    return 0;

                damageInHPPercents *= DefaultFallDamageRate;

                return (int)(chr.MaxHealth * damageInHPPercents);
            };
    }
}