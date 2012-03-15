using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Entities
{
    public static class Targeting
    {
        #region Nearby objects/clients

        /// <summary>
        /// Iterates over all objects within the given radius around this object.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="predicate">Returns whether to continue iteration.</param>
        /// <returns>True, if iteration should continue (usually indicating that we did not find what we were looking for).</returns>
        public static bool IterateEnvironment(this IWorldLocation location, float radius, Func<WorldObject, bool> predicate)
        {
            return location.Map.IterateObjects(location.Position, radius, location.Phase, predicate);
        }

        /// <summary>
        /// Iterates over all objects of the given Type within the given radius around this object.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="predicate">Returns whether to continue iteration.</param>
        /// <returns>True, if iteration should continue (usually indicating that we did not find what we were looking for).</returns>
        public static bool IterateEnvironment<O>(this IWorldLocation location, float radius, Func<O, bool> predicate)
            where O : WorldObject
        {
            return location.Map.IterateObjects(location.Position, radius, location.Phase, obj => !(obj is O) || predicate((O)obj));
        }

        /// <summary>
        /// Returns all objects in radius
        /// </summary>
        public static IList<WorldObject> GetObjectsInRadius<O>(this O wObj, float radius, ObjectTypes filter, bool checkVisible, int limit = int.MaxValue) where O : WorldObject
        {
            if (wObj.Map != null)
            {
                IList<WorldObject> objects;

                if (checkVisible)
                {
                    Func<WorldObject, bool> visCheck = obj => obj.CheckObjType(filter) && wObj.CanSee(obj);

                    objects = wObj.Map.GetObjectsInRadius(wObj.Position, radius, visCheck, wObj.Phase, limit);
                }
                else
                {
                    objects = wObj.Map.GetObjectsInRadius(wObj.Position, radius, filter, wObj.Phase, limit);
                }

                return objects;
            }

            return WorldObject.EmptyArray;
        }

        public static IList<WorldObject> GetVisibleObjectsInRadius<O>(this O obj, float radius, ObjectTypes filter, int limit = int.MaxValue) where O : WorldObject
        {
            return obj.GetObjectsInRadius(radius, filter, true, limit);
        }

        public static IList<WorldObject> GetVisibleObjectsInRadius<O>(this O obj, float radius, Func<WorldObject, bool> filter, int limit) where O : WorldObject
        {
            if (obj.Map != null)
            {
                Func<WorldObject, bool> visCheck = otherObj => filter(otherObj) && obj.CanSee(otherObj);

                return obj.Map.GetObjectsInRadius(obj.Position, radius, visCheck, obj.Phase, limit);
            }

            return WorldObject.EmptyArray;
        }

        public static IList<WorldObject> GetVisibleObjectsInUpdateRadius<O>(this O obj, ObjectTypes filter) where O : WorldObject
        {
            return obj.GetVisibleObjectsInRadius(WorldObject.BroadcastRange, filter, 0);
        }

        public static IList<WorldObject> GetVisibleObjectsInUpdateRadius<O>(this O obj, Func<WorldObject, bool> filter) where O : WorldObject
        {
            return obj.GetVisibleObjectsInRadius(WorldObject.BroadcastRange, filter, 0);
        }

        /// <summary>
        /// Gets all clients in update-radius that can see this object
        /// </summary>
        public static ICollection<IRealmClient> GetNearbyClients<O>(this O obj, bool includeSelf) where O : WorldObject
        {
            return obj.GetNearbyClients(WorldObject.BroadcastRange, includeSelf);
        }

        /// <summary>
        /// Gets all clients that can see this object
        /// </summary>
        public static ICollection<IRealmClient> GetNearbyClients<O>(this O obj, float radius, bool includeSelf) where O : WorldObject
        {
            if (obj.Map != null && obj.IsAreaActive)
            {
                Func<Character, bool> visCheck = otherObj => otherObj.CanSee(obj) && (includeSelf || obj != otherObj);

                var entities = obj.Map.GetObjectsInRadius<Character>(obj.Position, radius, visCheck, obj.Phase);

                return entities.TransformList(chr => chr.Client);
            }

            return RealmClient.EmptyArray;
        }

        /// <summary>
        /// Gets all characters that can see this object
        /// </summary>
        public static ICollection<Character> GetNearbyCharacters<O>(this O obj) where O : WorldObject
        {
            return obj.GetNearbyCharacters(WorldObject.BroadcastRange);
        }

        /// <summary>
        /// Gets all characters that can see this object
        /// </summary>
        public static ICollection<Character> GetNearbyCharacters<O>(this O obj, bool includeSelf) where O : WorldObject
        {
            return obj.GetNearbyCharacters(WorldObject.BroadcastRange, includeSelf);
        }

        /// <summary>
        /// Gets all characters that can see this object
        /// </summary>
        public static ICollection<Character> GetNearbyCharacters<O>(this O obj, float radius, bool includeSelf = true) where O : WorldObject
        {
            if (obj.Map != null && obj.AreaCharCount > 0)
            {
                Func<Character, bool> visCheck =
                    otherObj => otherObj.CanSee(obj) && (obj != otherObj || includeSelf);

                return obj.Map.GetObjectsInRadius(obj.Position, radius, visCheck, obj.Phase);
            }

            return Character.EmptyArray;
        }

        /// <summary>
        /// Gets all Horde players in the given radius.
        /// </summary>
        public static ICollection<Character> GetNearbyHordeCharacters<O>(this O obj, float radius) where O : WorldObject
        {
            if (obj.Map != null)
            {
                return obj.Map.GetObjectsInRadius<Character>(obj.Position, radius, otherObj => otherObj.FactionGroup == FactionGroup.Horde, obj.Phase);
            }

            return Character.EmptyArray;
        }

        /// <summary>
        /// Gets all alliance players in the given radius.
        /// </summary>
        public static ICollection<Character> GetNearbyAllianceCharacters<O>(this O obj, float radius) where O : WorldObject
        {
            if (obj.Map != null)
            {
                return obj.Map.GetObjectsInRadius<Character>(obj.Position, radius, otherObj => otherObj.FactionGroup == FactionGroup.Alliance, obj.Phase);
            }

            return Character.EmptyArray;
        }

        /// <summary>
        /// Gets all units that are at least neutral with it who can see this object
        /// </summary>
        public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj) where O : WorldObject
        {
            return obj.GetNearbyAtLeastNeutralUnits(WorldObject.BroadcastRange);
        }

        /// <summary>
        /// Gets all units that are at least neutral with it who can see this object
        /// </summary>
        public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj, bool includeSelf) where O : WorldObject
        {
            return obj.GetNearbyAtLeastNeutralUnits(WorldObject.BroadcastRange, includeSelf);
        }

        /// <summary>
        /// Gets all units that are at least neutral with it who can see this object
        /// </summary>
        public static ICollection<Unit> GetNearbyAtLeastNeutralUnits<O>(this O obj, float radius, bool includeSelf = true) where O : WorldObject
        {
            if (obj.Map != null && obj.AreaCharCount > 0)
            {
                Func<Unit, bool> visCheck =
                    otherObj => otherObj.CanSee(obj) && (obj != otherObj || includeSelf) && obj.IsAtLeastNeutralWith(otherObj);

                return obj.Map.GetObjectsInRadius(obj.Position, radius, visCheck, obj.Phase);
            }

            return new Unit[0];
        }

        public static GameObject GetNearbyGO<O>(this O wObj, GOEntryId id) where O : WorldObject
        {
            return wObj.GetNearbyGO(id, WorldObject.BroadcastRange);
        }

        public static GameObject GetNearbyGO<O>(this O wObj, GOEntryId id, float radius) where O : WorldObject
        {
            GameObject go = null;
            wObj.IterateEnvironment(radius, obj =>
            {
                if (wObj.CanSee(obj) && obj is GameObject && ((GameObject)obj).Entry.GOId == id)
                {
                    go = (GameObject)obj;
                    return false;
                }
                return true;
            });
            return go;
        }

        public static NPC GetNearbyNPC<O>(this O wObj, NPCId id) where O : WorldObject
        {
            return wObj.GetNearbyNPC(id, WorldObject.BroadcastRange);
        }

        public static NPC GetNearbyNPC<O>(this O wObj, NPCId id, float radius) where O : WorldObject
        {
            NPC npc = null;
            wObj.IterateEnvironment(radius, obj =>
            {
                if (wObj.CanSee(obj) && obj is NPC && ((NPC)obj).Entry.NPCId == id)
                {
                    npc = (NPC)obj;
                    return false;
                }
                return true;
            });
            return npc;
        }

        /// <summary>
        /// Gets a random nearby Character in WorldObject.BroadcastRange who is alive and visible.
        /// </summary>
        public static Character GetNearbyRandomHostileCharacter<O>(this O wObj) where O : WorldObject
        {
            return wObj.GetNearbyRandomHostileCharacter(WorldObject.BroadcastRange);
        }

        /// <summary>
        /// Gets a random nearby Character in WorldObject.BroadcastRange who is alive and visible.
        /// </summary>
        public static Character GetNearbyRandomHostileCharacter<O>(this O wObj, float radius) where O : WorldObject
        {
            if (wObj.AreaCharCount == 0)
            {
                return null;
            }

            if (radius > WorldObject.BroadcastRange)
            {
                // TODO: This won't quite work
                LogManager.GetCurrentClassLogger().Warn("Called GetNearbyRandomHostileCharacter with radius = {0} > BroadcastRange = {1}", radius, WorldObject.BroadcastRange);
                return null;
            }

            Character chr = null;
            var r = Utility.Random(0, wObj.AreaCharCount);
            var i = 0;
            var radiusSq = radius * radius;
            wObj.IterateEnvironment(WorldObject.BroadcastRange, obj =>
            {
                if (obj is Character)
                {
                    if (!wObj.CanSee(obj) ||
                        !((Character)obj).IsAlive ||
                        !wObj.IsHostileWith(obj) ||
                        !wObj.IsInRadiusSq(obj, radiusSq))
                    {
                        // does not count
                        r--;
                        return true;
                    }
                    chr = (Character)obj;
                    ++i;
                }
                return i != r;
            });
            return chr;
        }

        /// <summary>
        /// Returns the Unit that is closest within the given Radius around this Object
        /// TODO: Should add visibility test?
        /// </summary>
        public static Unit GetNearestUnit(this IWorldLocation wObj, float radius)
        {
            Unit unit = null;
            var sqDist = float.MaxValue;
            wObj.IterateEnvironment<Unit>(radius, obj =>
            {
                var curSqDist = obj.GetDistanceSq(wObj);
                if (curSqDist < sqDist)
                {
                    sqDist = curSqDist;
                    unit = obj;
                }
                return true;
            }
            );
            return unit;
        }

        /// <summary>
        /// Returns the Unit that is closest within the given Radius around this Object and passes the filter
        /// </summary>
        public static Unit GetNearestUnit(this IWorldLocation wObj, Func<Unit, bool> filter)
        {
            return wObj.GetNearestUnit(WorldObject.BroadcastRange, filter);
        }

        /// <summary>
        /// Returns the Unit that is closest within the given Radius around this Object and passes the filter
        /// </summary>
        public static Unit GetNearestUnit(this IWorldLocation wObj, float radius, Func<Unit, bool> filter)
        {
            Unit target = null;
            var sqDist = float.MaxValue;
            wObj.IterateEnvironment<Unit>(radius, unit =>
            {
                if (filter(unit))
                {
                    var curSqDist = unit.GetDistanceSq(wObj);
                    if (curSqDist < sqDist)
                    {
                        sqDist = curSqDist;
                        target = unit;
                    }
                }
                return true;
            }
            );
            return target;
        }

        public static Unit GetRandomUnit<O>(this O wObj, float radius, bool checkVisible = true) where O : WorldObject
        {
            return (Unit)wObj.GetObjectsInRadius(radius, ObjectTypes.Unit, checkVisible, 0).GetRandom();
        }

        public static Unit GetRandomVisibleUnit(this WorldObject wObj, float radius, Func<Unit, bool> filter)
        {
            return (Unit)wObj.GetVisibleObjectsInRadius(radius, obj => obj is Unit && filter((Unit)obj), 0).GetRandom();
        }

        public static Unit GetNearbyRandomAlliedUnit<O>(this O wObj) where O : WorldObject
        {
            return wObj.GetNearbyRandomAlliedUnit(WorldObject.BroadcastRange);
        }

        public static Unit GetNearbyRandomAlliedUnit<O>(this O wObj, float radius) where O : WorldObject
        {
            return wObj.GetRandomVisibleUnit(radius, unit => unit.IsAlliedWith(wObj));
        }

        public static Unit GetNearbyRandomHostileUnit<O>(this O wObj, float radius) where O : WorldObject
        {
            return wObj.GetRandomVisibleUnit(radius, wObj.MayAttack);
        }

        #endregion Nearby objects/clients
    }
}
