using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Global
{
    /// <summary>
    ///
    /// </summary>
    public static class WorldLocationMgr
    {
        /// <summary>
        /// All world locations definition
        /// </summary>
        public static readonly Dictionary<string, INamedWorldZoneLocation> WorldLocations =
            new Dictionary<string, INamedWorldZoneLocation>(StringComparer.InvariantCultureIgnoreCase);

        public static List<INamedWorldZoneLocation> WorldLocationList
        {
            get { return WorldLocations.Values.ToList(); }
        }

        /// <summary>
        /// For faster iteration (Do we even need the dictionary?)
        /// </summary>
        private static INamedWorldZoneLocation[] LocationCache;

        public static INamedWorldZoneLocation Stormwind;

        public static INamedWorldZoneLocation Orgrimmar;

        /// <summary>
        /// Depends on Table-Creation (Third)
        /// </summary>
        [Initialization(InitializationPass.Third, "Initialize WorldLocations")]
        public static void Initialize()
        {
            ContentMgr.Load<WorldZoneLocation>();
            LocationCache = WorldLocations.Values.ToArray();

            Stormwind = GetFirstMatch("Stormwind");
            Orgrimmar = GetFirstMatch("Orgrimmar");
        }

        /// <summary>
        /// Searches in loaded world locations for a specific world location name
        /// </summary>
        /// <param name="name">Name of the location to search</param>
        /// <returns>WorldLocation for the selected location. Returns null if not found</returns>
        public static INamedWorldZoneLocation Get(string name)
        {
            INamedWorldZoneLocation worldloc;
            WorldLocations.TryGetValue(name, out worldloc);
            return worldloc;
        }

        /// <summary>
        /// Gets the first <see cref="WorldZoneLocation"/> matching the given name parts
        /// </summary>
        /// <param name="partialName"></param>
        /// <returns></returns>
        public static INamedWorldZoneLocation GetFirstMatch(string partialName)
        {
            var parts = partialName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < LocationCache.Length; i++)
            {
                var loc = LocationCache[i];
                var found = true;
                for (var j = 0; j < parts.Length; j++)
                {
                    var part = parts[j];
                    if (loc.DefaultName.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) == -1)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return loc;
                }
            }
            return null;
        }

        public static List<INamedWorldZoneLocation> GetMatches(string partialName)
        {
            var list = new List<INamedWorldZoneLocation>(3);
            var parts = partialName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < LocationCache.Length; i++)
            {
                var loc = LocationCache[i];
                var found = true;
                for (var j = 0; j < parts.Length; j++)
                {
                    var part = parts[j];
                    if (loc.DefaultName.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) == -1)
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    list.Add(loc);
                }
            }
            return list;
        }

        /// <summary>
        /// Creates a GossipMenu of all locations
        /// </summary>
        /// <returns></returns>
        public static GossipMenu CreateTeleMenu()
        {
            return CreateTeleMenu(WorldLocationList);
        }

        public static GossipMenu CreateTeleMenu(List<INamedWorldZoneLocation> locations)
        {
            return CreateTeleMenu(locations, (convo, loc) => convo.Character.TeleportTo(loc));
        }

        public static GossipMenu CreateTeleMenu(List<INamedWorldZoneLocation> locations, Action<GossipConversation, INamedWorldZoneLocation> callback)
        {
            // create gossip of all options
            var menu = new GossipMenu();
            foreach (var location in locations)
            {
                var loc = location;		// create local reference

                // TODO: Localize names
                menu.AddItem(new GossipMenuItem(loc.Names.LocalizeWithDefaultLocale(), convo =>
                {
                    callback(convo, loc);
                }));
            }
            return menu;
        }
    }
}