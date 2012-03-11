﻿using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;

namespace WCell.RealmServer.GameObjects.Spawns
{
    public class GOSpawnPoolTemplate : SpawnPoolTemplate<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
    {
        public GOSpawnPoolTemplate()
            : this(0, 0)
        {
        }

        /// <summary>
        /// Constructor for custom pools
        /// </summary>
        public GOSpawnPoolTemplate(int maxSpawnAmount)
            : this(0, maxSpawnAmount)
        {
        }

        /// <summary>
        /// Constructor for custom pools
        /// </summary>
        public GOSpawnPoolTemplate(GOSpawnEntry entry, int maxSpawnAmount = 0) :
            this(maxSpawnAmount)
        {
            AddEntry(entry);
        }

        internal GOSpawnPoolTemplate(uint id, int maxSpawnAmount)
            : base(id, maxSpawnAmount)
        {
        }

        internal GOSpawnPoolTemplate(SpawnPoolTemplateEntry entry)
            : base(entry)
        {
        }

        public override List<GOSpawnPoolTemplate> PoolTemplatesOnSameMap
        {
            get { return GOMgr.GetOrCreateSpawnPoolTemplatesByMap(MapId); }
        }
    }
}
