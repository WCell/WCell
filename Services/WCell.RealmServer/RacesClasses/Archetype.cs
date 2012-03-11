using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.RacesClasses
{
    /// <summary>
    /// An Archetype is the combination of a Race and a Class to define a distinct persona, eg. Orc Warrior, Human Paladin etc
    /// </summary>
    [DataHolder]
    public class Archetype : IDataHolder
    {
        public ClassId ClassId;
        public RaceId RaceId;

        /// <summary>
        /// The starting position for the given race.
        /// </summary>
        public Vector3 StartPosition;

        public float StartOrientation;

        /// <summary>
        /// The starting map for the given race.
        /// </summary>
        public MapId StartMapId;

        /// <summary>
        /// The starting zone for the given race.
        /// </summary>
        public ZoneId StartZoneId;

        [NotPersistent]
        public IWorldZoneLocation StartLocation;

        [NotPersistent]
        public BaseClass Class;

        [NotPersistent]
        public BaseRace Race;

        [NotPersistent]
        public readonly LevelStatInfo[] LevelStats = new LevelStatInfo[RealmServerConfiguration.MaxCharacterLevel];

        /// <summary>
        /// All initial spells of this Archetype
        /// </summary>
        [NotPersistent]
        public readonly List<Spell> Spells = new List<Spell>();

        /// <summary>
        /// All initial items for males of this Archetype
        /// </summary>
        [NotPersistent]
        public readonly List<ItemStack> MaleItems = new List<ItemStack>();

        /// <summary>
        /// All initial items for females of this Archetype
        /// </summary>
        [NotPersistent]
        public readonly List<ItemStack> FemaleItems = new List<ItemStack>();

        [NotPersistent]
        public byte[] ActionButtons = ActionButton.CreateEmptyActionButtonBar();

        [NotPersistent]
        public ChatLanguage[] SpokenLanguages;

        public LevelStatInfo FirstLevelStats
        {
            get;
            internal set;
        }

        public void FinalizeDataHolder()
        {
            Race = ArchetypeMgr.GetRace(RaceId);
            Class = ArchetypeMgr.GetClass(ClassId);

            if (Class == null || Race == null)
            {
                throw new ContentException("Could not load Archetype \"{0}\" - Invalid Class or race.", this);
            }

            var races = ArchetypeMgr.Archetypes[(uint)ClassId];
            if (races == null)
            {
                ArchetypeMgr.Archetypes[(uint)ClassId] = races = new Archetype[WCellConstants.RaceTypeLength];
            }

            StartLocation = new WorldZoneLocation(StartMapId, StartPosition, StartZoneId);
            if (StartLocation.Map == null)
            {
                LogManager.GetCurrentClassLogger().Warn("Failed to initialize Archetype \"" + this + "\" - StartMap does not exist: " + StartMapId);
                //ArrayUtil.Set(ref RaceClassMgr.BaseRaces, (uint)Id, null);
            }
            else
            {
                if (StartLocation.ZoneTemplate == null)
                {
                    LogManager.GetCurrentClassLogger().Warn("Failed to initialize Archetype \"" + this +
                                                 "\" - StartZone \"" + StartZoneId + "\" does not exist in StartMap \"" +
                                                 StartMapId + "\"");
                    //ArrayUtil.Set(ref RaceClassMgr.BaseRaces, (uint)Id, null);
                }
                else
                {
                    races[(uint)RaceId] = this;
                }
            }

            //get levelstats
        }

        public LevelStatInfo GetLevelStats(uint level)
        {
            if (level >= LevelStats.Length)
            {
                level = (uint)LevelStats.Length - 1;
            }
            return LevelStats[level - 1];
        }

        public List<ItemStack> GetInitialItems(GenderType gender)
        {
            if (gender == GenderType.Female)
            {
                return FemaleItems;
            }
            return MaleItems;
        }

        /// <summary>
        /// Gets the BaseStrength at a specific level.
        /// </summary>
        /// <param name="level">the level to get the BaseStrength for</param>
        /// <returns>BaseStrength amount</returns>
        public int GetStrength(int level)
        {
            var levelStat = LevelStats[level - 1];
            return levelStat != null ? levelStat.Strength : 0;
        }

        /// <summary>
        /// Gets the BaseAgility at a specific level.
        /// </summary>
        /// <param name="level">the level to get the BaseAgility for</param>
        /// <returns>BaseAgility amount</returns>
        public int GetAgility(int level)
        {
            var levelState = LevelStats[level - 1];
            return levelState != null ? levelState.Agility : 0;
        }

        /// <summary>
        /// Gets the BaseStamina at a specific level.
        /// </summary>
        /// <param name="level">the level to get the BaseStamina for</param>
        /// <returns>BaseStamina amount</returns>
        public int GetStamina(int level)
        {
            var levelStat = LevelStats[level - 1];
            return levelStat != null ? levelStat.Stamina : 0;
        }

        /// <summary>
        /// Gets the BaseIntellect at a specific level.
        /// </summary>
        /// <param name="level">the level to get the BaseIntellect for</param>
        /// <returns>the BaseIntellect amount</returns>
        public int GetIntellect(int level)
        {
            var levelStat = LevelStats[level - 1];
            return levelStat != null ? levelStat.Intellect : 0;
        }

        /// <summary>
        /// Gets the BaseSpirit at a specific level.
        /// </summary>
        /// <param name="level">the level to get the BaseSpirit for</param>
        /// <returns>the BaseSpirit amount</returns>
        public int GetSpirit(int level)
        {
            var levelStat = LevelStats[level - 1];
            return levelStat != null ? levelStat.Spirit : 0;
        }

        public override string ToString()
        {
            return Race + " " + Class;
        }
    }
}