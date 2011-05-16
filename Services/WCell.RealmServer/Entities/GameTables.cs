/*
 * Written by Mokrago from the WCell team.
 * Please see the wiki article on my findings and more documentation.
 * 
 */

using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core;
using WCell.Core.ClientDB;
using WCell.RealmServer.Res;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
    public class GameTables
    {
        

        private static float[] s_baseMeleeCritChance;
        private static float[] s_baseSpellCritChance;

        private static float[] s_classMeleeCritChance;
        private static float[] s_classSpellCritChance;

        private static float[] s_barberShopCosts;
        private static float[] s_octManaRegen;
        private static float[] s_octHealthRegen;
        private static float[] s_octManaRegenPerSpirit;
        private static float[] s_octHealthPerStamina;

        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
        private static Dictionary<CombatRating, float[]> s_combatRatings;

        public static bool Loaded;

        /// <summary>
        /// All combat ratings from WCell.Constants.Misc.CombatRating
        /// NOTE: Fields are indexed by level starting from level 1 = index0 (use level-1)
        /// </summary>
        public static Dictionary<CombatRating, float[]> CombatRatings
        {
            get { return s_combatRatings; }
        }

        /// <summary>
        /// The base spell crit chance modifier indexed by class - 1
        /// </summary>
        public static float[] BaseSpellCritChance
        {
            get { return s_baseSpellCritChance; }
        }

        /// <summary>
        /// The base melee crit chance modifier indexed by class - 1
        /// </summary>
        public static float[] BaseMeleeCritChance
        {
            get { return s_baseMeleeCritChance; }
        }

        /// <summary>
        /// Spell crit modifier by level and class
        /// Used for crit per intellect and crit per crit rating
        /// </summary>
        private static float[] ClassSpellCritChance
        {
            get { return s_classSpellCritChance; }
        }

        /// <summary>
        /// Melee crit modifier by level and class
        /// Used for crit per agility and crit per crit rating
        /// </summary>
        private static float[] ClassMeleeCritChance
        {
            get { return s_classMeleeCritChance; }
        }

        /// <summary>
        /// Barber shop cost per level (?)
        /// </summary>
        public static float[] BarberShopCosts
        {
            get { return s_barberShopCosts; }
        }

        /// <summary>
        /// Mana regeneration per class per level (in combat?)
        /// </summary>
        public static float[] OCTRegenMP
        {
            get { return s_octManaRegen; }
        }

        /// <summary>
        /// Mana regeneration per class per level (how much spirit it takes for MP5)
        /// </summary>
        public static float[] RegenMPPerSpirit
        {
            get { return s_octManaRegenPerSpirit; }
        }

        /// <summary>
        /// Health regeneration per class per level (how much stamina it takes for what?)
        /// </summary>
        public static float[] OCTHpPerStamina
        {
            get { return s_octHealthPerStamina; }
        }

        /// <summary>
        /// Get's the table from the CombatRating property.
        /// </summary>
        /// <param name="rating"></param>
        /// <returns>The combat rating table with 100 values indexed by level - 1</returns>
        public static float[] GetCRTable(CombatRating rating)
        {
            float[] table;
            CombatRatings.TryGetValue(rating, out table);
            return table;
        }

        /// <summary>
        /// Gets the modified value from the table ClassSpellCritChance from the correct index.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns>The modified value matching the format "XX stat for 1% chance"</returns>
        public static float GetClassSpellCritChanceValue(int level, ClassId id)
        {
            var value = GetValuePerRating(ClassSpellCritChance, level, id);
            return ModifyValue(value);
        }

        /// <summary>
        /// Gets the modified value from the table ClassMeleeCritChance from the correct index.
        /// Returns the modified value matching the format "XX stat for 1% chance"
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns>The modified value matching the format "XX rating for 1% chance"</returns>
        public static float GetClassMeleeCritChanceValue(int level, ClassId id)
        {
            var value = GetValuePerRating(ClassMeleeCritChance, level, id);
            return ModifyValue(value);
			//return value;
        }

        public static float GetUnmodifiedClassSpellCritChanceValue(int level, ClassId id)
        {
            return GetValuePerRating(ClassSpellCritChance, level, id);
        }

        public static float GetUnModifiedClassMeleeCritChanceValue(int level, ClassId id)
        {
            return GetValuePerRating(ClassMeleeCritChance, level, id);
        }

        /// <summary>
        /// Modifies the values from ClassSpellCritChance, ClassMeleeCritChance
        /// to match the format of "XX stat for 1% chance"
        /// TODO: Phase out use for optimization (use the unmodified value)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static float ModifyValue(float value)
        {
            return 1f/(value*100);
        }

        /// <summary>
        /// Gets the real value from the table (unmodified)
        /// NOTE: Only for ClassSpellCritChance, ClassMeleeCritChance
        /// Will return the wrong values if used incorrectly.
        /// </summary>
        /// <param name="table">The table (ClassSpellCritChance, ClassMeleeCritChance)</param>
        /// <param name="level">Level of the character</param>
        /// <param name="classId">ClassId of the character</param>
        /// <returns></returns>
        private static float GetValuePerRating(float[] table, int level, ClassId classId)
        {
            if (level > 100)
            {
                level = 100;
            }
            if (level < 1)
            {
                level = 1;
            }
            return table[100*(int) classId + level - 101];
        }
        
        #region DBCs loading

        public class GameTableConverter : AdvancedClientDBRecordConverter<float>
        {
            public override float ConvertTo(byte[] rawData, ref int id)
            {
                return GetFloat(rawData, 0);
            }
        }

        private static bool LoadRatingChanceDBC(string file, out float[] vals)
        {
            vals = new float[0];

            string ccDbcPath = RealmServerConfiguration.GetDBCFile(file);

            if (!File.Exists(ccDbcPath))
            {
                s_log.Error(string.Format(Resources.DBCFileDoesntExist, file));

                return false;
            }

            var dbcRdr = new ListDBCReader<float, GameTableConverter>(ccDbcPath);

            vals = new float[dbcRdr.EntryList.Count];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = dbcRdr.EntryList[i];
            }

            return true;
        }

        private static bool LoadGtBaseSpellCritChanceDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTCHANCETOSPELLCRITBASE, out s_baseSpellCritChance);
        }

        private static bool LoadGtClassSpellCritChanceDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTCHANCETOSPELLCRIT, out s_classSpellCritChance);
        }

        private static bool LoadGtBaseMeleeCritChanceDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTCHANCETOMELEECRITBASE, out s_baseMeleeCritChance);
        }

        private static bool LoadGtClassMeleeCritChanceDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTCHANCETOMELEECRIT, out s_classMeleeCritChance);
        }

        private static bool LoadGtClassHealthRegenPerStaminaDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTOCTHPPERSTAMINA, out s_octHealthPerStamina);
        }

        private static bool LoadGtClassManaRegenPerSpiritDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTREGENMPPERSPT, out s_octManaRegenPerSpirit);
        }

        private static bool LoadGtClassOCTManaRegenDBC()
        {
            return LoadRatingChanceDBC(ClientDBConstants.DBC_GTOCTREGENMP, out s_octManaRegen);
        }

        private static bool LoadGtBarberShopCostDBC(out float[] vals)
        {
            string gtDbcPath = RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_GTBARBERSHOPCOSTBASE);

            var dbcRdr = new ListDBCReader<float, GameTableConverter>(gtDbcPath);

            vals = new float[dbcRdr.EntryList.Count];
            for (int i = 0; i < vals.Length; i++)
            {
                vals[i] = dbcRdr.EntryList[i];
            }

            return true;
        }

        private static bool LoadGtCombatRatingsDBC(out Dictionary<CombatRating, float[]> combatRatings)
        {
            combatRatings = new Dictionary<CombatRating, float[]>();

            string gtDbcPath = RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_GTCOMBATRATINGS);

            if (!File.Exists(gtDbcPath))
            {
                s_log.Error(string.Format(Resources.DBCFileDoesntExist, gtDbcPath));

                return false;
            }
            var dbcRdr = new ListDBCReader<float, GameTableConverter>(gtDbcPath);

            for (int rating = (int) CombatRating.WeaponSkill; rating < ((int) CombatRating.Expertise + 1); rating++)
            {
                combatRatings[(CombatRating) rating] = new float[100];

                for (int i = (rating - 1)*100; i < rating*100; i++)
                {
                    combatRatings[(CombatRating) rating][i - (rating - 1)*100] = dbcRdr.EntryList[i];
                }
            }

            return true;
        }

        /// <summary>
        /// Loads the DBC file starting with gtXXXX.dbc
        /// </summary>
        /// <returns>Wether all gametables were loaded</returns>
        public static bool LoadGtDBCs()
        {
            if (!LoadGtBaseSpellCritChanceDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTCHANCETOSPELLCRITBASE));

                return false;
            }

            if (!LoadGtClassSpellCritChanceDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTCHANCETOSPELLCRIT));

                return false;
            }

            if (!LoadGtBaseMeleeCritChanceDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTCHANCETOMELEECRITBASE));

                return false;
            }

            if (!LoadGtClassMeleeCritChanceDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTCHANCETOMELEECRIT));

                return false;
            }

            if (!LoadGtBarberShopCostDBC(out s_barberShopCosts))
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTBARBERSHOPCOSTBASE));

                return false;
            }

            if (!LoadGtCombatRatingsDBC(out s_combatRatings))
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTCOMBATRATINGS));

                return false;
            }

            if(!LoadGtClassHealthRegenPerStaminaDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTOCTHPPERSTAMINA));

                return false;
            }

            if (!LoadGtClassManaRegenPerSpiritDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTREGENMPPERSPT));

                return false;
            }

            if (!LoadGtClassOCTManaRegenDBC())
            {
                s_log.Info(string.Format(Resources.DBCLoadFailed, ClientDBConstants.DBC_GTOCTREGENMP));

                return false;
            }

            Loaded = true;
            return true;
        }

        #endregion

        #region Hardcoded values

        //TODO: Try and find these from DBCs
        /// <summary>
        /// The base mana regeneration modifier per level
        /// TODO: Get it from the DBCs (GtOCTRegenMP.dbc?)
        /// </summary>
        [NotVariable] public static readonly float[] BaseRegen = new float[81]
                                                                     {
                                                                         0f,
                                                                         0.034965f, 0.034191f, 0.033465f, 0.032526f,
                                                                         0.031661f, 0.031076f, 0.030523f,
                                                                         0.029994f, 0.029307f, 0.028661f, 0.027584f,
                                                                         0.026215f, 0.025381f, 0.0243f,
                                                                         0.023345f, 0.022748f, 0.021958f, 0.021386f,
                                                                         0.02079f, 0.020121f, 0.019733f,
                                                                         0.019155f, 0.018819f, 0.018316f, 0.017936f,
                                                                         0.017576f, 0.017201f, 0.016919f,
                                                                         0.016581f, 0.016233f, 0.015994f, 0.015707f,
                                                                         0.015464f, 0.015204f, 0.014956f,
                                                                         0.014744f, 0.014495f, 0.014302f, 0.014094f,
                                                                         0.013895f, 0.013724f, 0.013522f,
                                                                         0.013363f, 0.013175f, 0.012996f, 0.012853f,
                                                                         0.012687f, 0.012539f, 0.012384f,
                                                                         0.012233f, 0.012113f, 0.011973f, 0.011859f,
                                                                         0.011714f, 0.011575f, 0.011473f,
                                                                         0.011342f, 0.011245f, 0.01111f, 0.010999f,
                                                                         0.0107f, 0.010522f, 0.01029f,
                                                                         0.010119f, 0.009968f, 0.009808f, 0.009651f,
                                                                         0.009553f, 0.009445f, 0.009327f,
                                                                         0.008859f, 0.008415f, 0.007993f, 0.007592f,
                                                                         0.007211f, 0.006849f, 0.006506f,
                                                                         0.006179f, 0.005869f, 0.005575f,
                                                                     };

        // Table for base dodge values
        public static readonly float[] BaseDodge = new float[12]
                                                       {
                                                           0.0f,        // None
                                                           0.0075f,     // Warrior
                                                           0.00652f,    // Paladin
                                                           -0.0545f,    // Hunter
                                                           -0.0059f,    // Rogue
                                                           0.03183f,    // Priest
                                                           0.0114f,     // DK
                                                           0.0167f,     // Shaman
                                                           0.034575f,   // Mage
                                                           0.02011f,    // Warlock
                                                           0.0f,        // ??
                                                           -0.0187f     // Druid
                                                       };

        /// <summary>
        /// Agi/1% crit (ClassMeleeCritChance) to agility/1%dodge coefficient multipliers
        /// Divide the value from GetClassMeleeCritChanceValue(level, Id) by this.
        /// Latest intel says this is wrong.
        /// </summary>
        public static readonly float[] CritAgiMod = new float[12]
                                                        {
                                                            0.0f,   // None
                                                            0.85f,   // Warrior
                                                            1.0f,   // Paladin
                                                            1.1f,   // Hunter
                                                            2.0f,   // Rogue
                                                            1.0f,   // Priest
                                                            0.85f,   // DK
                                                            1.6f,   // Shaman
                                                            1.0f,   // Mage
                                                            1.0f,   // Warlock
                                                            0.0f,   // ??
                                                            2.0f    // Druid
                                                        };

        /// <summary>
        /// Constant used for diminishing returns indexed per class.
        /// </summary>
        public static readonly float[] DiminisherConstant = new float[12]
                                                                {
                                                                    0f, // None
                                                                    0.9560f, // Warrior
                                                                    0.9560f, // Paladin
                                                                    0.9880f, // Hunter
                                                                    0.9880f, // Rogue
                                                                    0.9830f, // Priest
                                                                    0.9560f, // DK
                                                                    0.9880f, // Shaman
                                                                    0.9830f, // Mage
                                                                    0.9830f, // Warlock
                                                                    0.0f, // ??
                                                                    0.9720f // Druid
                                                                };

        /// <summary>
        /// Stat cap constant per class
        /// </summary>
        public static readonly float[] StatCap = new float[12]
                                                                {
                                                                    0f, // None
                                                                    88.129021f, // Warrior
                                                                    88.129021f, // Paladin
                                                                    145.560408f, // Hunter
                                                                    145.560408f, // Rogue
                                                                    150.375940f, // Priest
                                                                    88.129021f, // DK
                                                                    145.560408f, // Shaman
                                                                    150.375940f, // Mage
                                                                    150.375940f, // Warlock
                                                                    0.0f, // ??
                                                                    116.890707f // Druid
                                                                };

        #endregion
    }
}