using System;
using NLog;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Looting;
using WCell.Util;
using WCell.Util.Data;
using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// Represents a spawn entry for a GameObject
	/// </summary>
	[DataHolder]
	public class GOSpawnEntry : IDataHolder, IWorldLocation
	{
		public uint Id;
		public GOEntryId EntryId;

		[NotPersistent]
		public uint EntryIdRaw;
		public GameObjectState State;

        /// <summary>
        /// Whether this template will be added to its Map automatically (see <see cref="Map.SpawnMap"/>)
        /// </summary>
	    public bool AutoSpawn = true;

		private MapId m_MapId = MapId.End;
		public MapId MapId
		{
			get { return m_MapId; }
			set
			{
				if (m_MapId != MapId.End)
				{
					GOMgr.GetTemplates(m_MapId).Remove(this);
				}

				m_MapId = value;

				if (value != MapId.End)
				{
					GOMgr.AddTemplate(this);
				}
			}
		}

		public Vector3 Pos;
		public float Orientation;
		public float Scale = 1;

		[Persistent(GOConstants.MaxRotations)]
		public float[] Rotations;

		public byte AnimProgress;

		public uint PhaseMask = 1;

		public uint EventId;

		/// <summary>
		/// Time in seconds until this GO will respawn after it was destroyed.
		/// Value less or equal 0 means, it will not automatically respawn.
		/// </summary>
		public int RespawnTime;

		/// <summary>
		/// Time in seconds until this GO will despawn.
		/// Value less or equal 0 means, it will not automatically despawn.
		/// </summary>
		public int DespawnTime;

		public bool WillRespawn
		{
			get { return RespawnTime > 0; }
		}

		[NotPersistent]
		public GOEntry Entry;

		[NotPersistent]
		public LootItemEntry LootEntry;

		public GOSpawnEntry()
		{
		}

		public GOSpawnEntry(GOEntry entry, GameObjectState state,
			MapId mapId, ref Vector3 pos, float orientation, float scale, float[] rotations, int respawnTimeSecs = 600)
		{
			//Id = id;
			Entry = entry;
			EntryId = entry.GOId;
			State = state;
			MapId = mapId;
			Pos = pos;
			Orientation = orientation;
			Scale = scale;
			Rotations = rotations;
			RespawnTime = respawnTimeSecs;
		}

		public uint LootMoney
		{
			get { return 0; }
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given map
		/// </summary>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(Map map)
		{
			if (Entry == null)
			{
				return null;
			}
			var go = GameObject.Create(Entry, new WorldLocationStruct(map, Pos), this);
			return go;
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template at the given location
		/// </summary>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(IWorldLocation pos)
		{
			return GameObject.Create(Entry, pos, this);
		}

		public uint GetId()
		{
			return Id;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			if (Scale == 0)
			{
				Scale = 1;
			}

			if (EntryId == 0)
			{
				EntryId = (GOEntryId)EntryIdRaw;
			}
			else
			{
				EntryIdRaw = (uint)EntryId;
			}

			if (Entry == null)
			{
				GOMgr.Entries.TryGetValue((uint)EntryId, out Entry);
				if (Entry == null)
				{
					ContentMgr.OnInvalidDBData("GOTemplate ({0}) had an invalid EntryId.", this);
					return;
				}
			}

			if (Rotations == null)
			{
				Rotations = new float[GOConstants.MaxRotations];
			}

			if (RespawnTime < 0)
			{
				// in UDB, a negative DespawnTime implies that the GO will not auto-spawn and despawn after the negative amount of seconds
				AutoSpawn = false;
				DespawnTime = -RespawnTime;
			}

			Entry.Templates.Add(this);
			//GOMgr.Templates.Add(Id, this);
		}

		#region IWorldLocation
		public Vector3 Position
		{
			get { return Pos; }
		}

		/// <summary>
		/// Is null if Template is not spawned in a continent
		/// </summary>
		public Map Map
		{
			get { return World.GetMap(MapId); }
		}
		#endregion

		public static IEnumerable<GOSpawnEntry> GetAllDataHolders()
		{
			var list = new List<GOSpawnEntry>(10000);
			foreach (var entry in GOMgr.Entries.Values)
			{
				if (entry != null)
				{
					list.AddRange(entry.Templates);
				}
			}
			return list;
		}

		public override string ToString()
		{
			return (Entry != null ? Entry.DefaultName : "") + " (EntryId: " + EntryId + " (" + (int)EntryId + ")" //+ ", Id: " + Id
				+ ")";
		}
	}
}