using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Util.DynamicAccess;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.RealmServer.Instances
{
    /* TODO: Add in instance, heroic, and raid checks before entering
     *  - Quests, Items, Raid
     */

    public class InstanceMgr
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The max amount of different instances that a Character may enter within <see cref="InstanceCounterTime"/>
        /// </summary>
        public static int MaxInstancesPerHour = 5;

        /// <summary>
        /// Amount of time until a normal empty Instance expires by default
        /// </summary>
        public static int DungeonExpiryMinutes = 10;

        /// <summary>
        /// Players may only enter MaxInstancesPerCharPerHour instances within this cooldown
        /// </summary>
        public static TimeSpan InstanceCounterTime = TimeSpan.FromHours(1);

        public const int MaxInstanceDifficulties = (int)RaidDifficulty.End;

        public static readonly Dictionary<uint, InstanceCollection> OfflineLogs = new Dictionary<uint, InstanceCollection>();

        public static readonly List<MapTemplate> InstanceInfos = new List<MapTemplate>();

        [NotVariable]
        public static GlobalInstanceTimer[] GlobalResetTimers;

        private static readonly ReaderWriterLockWrapper syncLock = new ReaderWriterLockWrapper();

        public static readonly WorldInstanceCollection<MapId, BaseInstance> Instances = new WorldInstanceCollection<MapId, BaseInstance>(MapId.End);

        #region Properties

        #endregion Properties

        #region Enter & Creation

        public static void SetCreator(MapId id, string typeName)
        {
            var type = RealmServer.GetType(typeName);
            if (type == null)
            {
                log.Warn("Invalid Creator for Instance \"" + id + "\": " + typeName + "  - " +
                                            "Please correct it in the Instance-config file: " + InstanceConfig.Filename);
                return;
            }
            var producer = AccessorMgr.GetOrCreateDefaultProducer(type);
            SetCreator(id, () => (BaseInstance)producer.Produce());
        }

        public static void SetCreator(MapId id, InstanceCreator creator)
        {
            var info = World.GetMapTemplate(id);
            if (info != null && info.InstanceTemplate != null)
            {
                info.InstanceTemplate.Creator = creator;
            }
            else
            {
                throw new ArgumentException("Given Map is not an Instance:" + id);
            }
        }

        /// <param name="creator">Can be null</param>
        public static I CreateInstance<I>(Character creator, InstanceTemplate template, uint difficultyIndex)
            where I : BaseInstance, new()
        {
            return (I)SetupInstance(creator, new I(), template, difficultyIndex);
        }

        /// <param name="creator">Can be null</param>
        public static BaseInstance CreateInstance(Character creator, InstanceTemplate template, uint difficultyIndex)
        {
            var instance = template.Create();
            return SetupInstance(creator, instance, template, difficultyIndex);
        }

        /// <summary>
        /// Convinience method for development
        /// </summary>
        /// <param name="creator">Can be null</param>
        public static BaseInstance CreateInstance(Character creator, MapId mapId)
        {
            var mapTemplate = World.GetMapTemplate(mapId);
            if (mapTemplate == null || !mapTemplate.IsInstance) return null;

            uint diffIndex;
            if (creator != null)
            {
                creator.EnsurePureStaffGroup(); // for development purposes only, don't drag players into the instance
                diffIndex = creator.GetInstanceDifficulty(mapTemplate.IsRaid);
            }
            else
            {
                diffIndex = 0;
            }
            var instance = mapTemplate.InstanceTemplate.Create();
            return SetupInstance(creator, instance, mapTemplate.InstanceTemplate, diffIndex);
        }

        /// <param name="creator">Can be null</param>
        private static BaseInstance SetupInstance(Character creator, BaseInstance instance, InstanceTemplate template, uint difficultyIndex = 0u)
        {
            if (instance != null)
            {
                instance.difficulty = template.MapTemplate.GetDifficulty(difficultyIndex) ?? template.MapTemplate.Difficulties[0];

                if (creator != null)
                {
                    // set faction & leader
                    instance.m_OwningFaction = creator.FactionGroup;
                    instance.Owner = creator.InstanceLeader;
                    instance.IsActive = true;
                }
                instance.InitMap(template.MapTemplate);
                Instances.AddInstance(instance.MapId, instance);
            }

            return instance;
        }

        /// <summary>
        /// This is called when an area trigger causes entering an instance
        /// </summary>
        public static bool EnterInstance(Character chr, MapTemplate mapTemplate, Vector3 targetPos)
        {
            if (!mapTemplate.IsInstance)
            {
                log.Error("Character {0} tried to enter \"{1}\" as Instance.", chr, mapTemplate);
                return false;
            }

            var isRaid = (mapTemplate.Type == MapType.Raid);
            var group = chr.Group;
            if (isRaid && !chr.Role.IsStaff && !group.Flags.HasFlag(GroupFlags.Raid))
            {
                InstanceHandler.SendRequiresRaid(chr.Client, 0);
                return false;
            }

            if (!mapTemplate.MayEnter(chr))
            {
                return false;
            }

            chr.SendSystemMessage("Entering instance...");

            // Find out if we've been here before
            var instances = chr.Instances;
            var instance = instances.GetActiveInstance(mapTemplate);

            if (instance == null)
            {
                var difficulty = mapTemplate.GetDifficulty(chr.GetInstanceDifficulty(isRaid));

                // Check whether we were in too many normal dungeons recently
                if (difficulty.BindingType == BindingType.Soft && !instances.HasFreeInstanceSlot && !chr.GodMode)
                {
                    MovementHandler.SendTransferFailure(chr.Client, mapTemplate.Id, MapTransferError.TRANSFER_ABORT_TOO_MANY_INSTANCES);
                    return false;
                }

                // Check whether we can join a group-owned instance
                if (group != null)
                {
                    instance = group.GetActiveInstance(mapTemplate);
                    if (instance != null)
                    {
                        if (!CheckFull(instance, chr))
                        {
                            return false;
                        }
                    }
                }

                if (instance == null)
                {
                    // create new instance
                    instance = CreateInstance(chr, mapTemplate.InstanceTemplate, chr.GetInstanceDifficulty(isRaid));
                    if (instance == null)
                    {
                        log.Warn("Could not create Instance \"{0}\" for: {1}", mapTemplate, chr);
                        return false;
                    }
                }
            }
            else if (!chr.GodMode)
            {
                if (!CheckFull(instance, chr))
                {
                    return false;
                }

                // Check that the Raid member has the same instance as the leader
                if (isRaid)
                {
                    if (group == null)
                    {
                        MovementHandler.SendTransferFailure(chr.Client, instance.Id, MapTransferError.TRANSFER_ABORT_NEED_GROUP);
                        return false;
                    }

                    var leaderRaid = group.InstanceLeaderCollection.GetBinding(mapTemplate.Id, BindingType.Hard);
                    var playerRaid = instances.GetBinding(mapTemplate.Id, BindingType.Hard);

                    if (playerRaid != null && leaderRaid != playerRaid)
                    {
                        // Player has a different instance than the leader
                        MovementHandler.SendTransferFailure(chr.Client, instance.Id, MapTransferError.TRANSFER_ABORT_NOT_FOUND);
                        return false;
                    }
                }
            }

            instance.TeleportInside(chr, targetPos);
            return true;
        }

        private static bool CheckFull(BaseInstance instance, Character chr)
        {
            if (instance.MaxPlayerCount != 0 && instance.PlayerCount >= instance.MaxPlayerCount && !chr.GodMode)
            {
                //chr.SendSystemMessage("Instance full: {0}/{1}", instance.PlayerCount, instance.MaxPlayerCount);
                MovementHandler.SendTransferFailure(chr.Client, instance.Id, MapTransferError.TRANSFER_ABORT_MAX_PLAYERS);
                return false;
            }
            return true;
        }

        /// <summary>
        /// This is called when an area trigger causes leaving an instance
        /// TODO: Add associations to raid individuals
        /// TODO: Implement the 5 instances per hour limit, simple but needs the right spot
        /// </summary>
        public static void LeaveInstance(Character player, MapTemplate mapTemplate, Vector3 entryInfo)
        {
            var map = World.GetNonInstancedMap(mapTemplate.Id);
            player.TeleportTo(map, entryInfo);
        }

        #endregion Enter & Creation

        #region InstanceLogs

        /// <summary>
        ///
        /// </summary>
        /// <param name="lowId"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        public static InstanceCollection GetOfflineInstances(uint lowId, bool autoCreate)
        {
            return GetOfflineInstances(lowId, autoCreate, false);
        }

        public static void RemoveLog(uint lowId)
        {
            var log = GetOfflineInstances(lowId, false, true);
            if (log != null)
            {
                // TODO: Remove from active instances
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lowId"></param>
        /// <param name="autoCreate"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public static InstanceCollection GetOfflineInstances(uint lowId, bool autoCreate, bool remove)
        {
            using (syncLock.EnterReadLock())
            {
                return GetOfflineInstancesUnlocked(lowId, autoCreate, remove);
            }
        }

        private static InstanceCollection GetOfflineInstancesUnlocked(uint lowId, bool autoCreate, bool remove)
        {
            InstanceCollection instanceCollection = null;

            if (OfflineLogs.ContainsKey(lowId))
            {
                instanceCollection = OfflineLogs[lowId];
                if (remove)
                    OfflineLogs.Remove(lowId);
            }

            if (autoCreate)
            {
                instanceCollection = new InstanceCollection(lowId);
                OfflineLogs.Add(lowId, instanceCollection);
            }

            return instanceCollection;
        }

        /// <summary>
        /// Gets and removes the InstanceLog for the given Character
        /// </summary>
        /// <param name="character"></param>
        internal static void RetrieveInstances(Character character)
        {
            using (syncLock.EnterReadLock())
            {
                var instances = GetOfflineInstancesUnlocked(character.EntityId.Low, false, true);
                if (instances != null)
                {
                    instances.Character = character;
                    character.Instances = instances;
                }
            }
        }

        //We have to remove all refences to this character
        internal static void OnCharacterLogout(Character character)
        {
            if (!character.HasInstanceCollection)
            {
                return;
            }

            using (syncLock.EnterWriteLock())
            {
                character.Instances.Character = null;
                OfflineLogs[character.EntityId.Low] = character.Instances;
            }
        }

        #endregion InstanceLogs

        #region Init

        [Initialization(InitializationPass.Fifth, "Initialize Instances")]
        public static void Initialize()
        {
            InstanceInfos.Sort();
            InstanceConfig.LoadSettings();
            try
            {
                GlobalResetTimers = GlobalInstanceTimer.LoadTimers();
            }
            catch (Exception e)
            {
                RealmDBMgr.OnDBError(e);
                GlobalResetTimers = GlobalInstanceTimer.LoadTimers();
            }
        }

        #endregion Init

        #region Resets

        public static DateTime GetNextResetTime(MapId id, uint difficultyIndex)
        {
            var rgn = World.GetMapTemplate(id);
            if (rgn != null)
            {
                return GetNextResetTime(rgn.GetDifficulty(difficultyIndex));
            }
            return default(DateTime);
        }

        public static DateTime GetNextResetTime(MapDifficultyEntry difficulty)
        {
            var timer = GlobalResetTimers[(int)difficulty.Map.Id];
            if (timer != null)
            {
                var time = timer.LastResets.Get(difficulty.Index);
                return time.AddSeconds(difficulty.ResetTime);
            }
            return default(DateTime);
        }

        #endregion Resets
    }
}