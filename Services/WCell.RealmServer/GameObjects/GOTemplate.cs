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
	/// Represents a template to create a GameObject.
	/// </summary>
	[DataHolder]
	public class GOTemplate : IDataHolder, IWorldLocation
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public uint Id;
		public GOEntryId EntryId;

		[NotPersistent]
		public uint EntryIdRaw;
		public GameObjectState State;

        /// <summary>
        /// Whether this template will be added to its Region automatically (see <see cref="Global.Region.SpawnRegion"/>)
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

		[NotPersistent]
		public GOEntry Entry;

		[NotPersistent]
		public LootItemEntry LootEntry;

		public GOTemplate()
		{
		}

		public GOTemplate(GOEntry entry, GameObjectState state, 
			MapId mapId, ref Vector3 pos, float orientation, float scale, float[] rotations)
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
		}

		public uint LootMoney
		{
			get { return 0; }
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given region
		/// </summary>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(Region region)
		{
			if (Entry == null)
			{
				return null;
			}
			var go = GameObject.Create(Entry, this);
			go.Position = Pos;
			region.AddObject(go);
			return go;
		}

		/// <summary>
		/// Spawns and returns a new GameObject from this template into the given region
		/// </summary>
		/// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
		public GameObject Spawn(IWorldLocation pos)
		{
			var go = GameObject.Create(Entry, this);
			go.Position = pos.Position;
			pos.Region.AddObject(go);
			return go;
		}

		public override string ToString()
		{
			return (Entry != null ? Entry.DefaultName : "") + " (EntryId: " + EntryId + " (" + (int)EntryId + ")" //+ ", Id: " + Id
				+ ")";
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
					ContentHandler.OnInvalidDBData("GOTemplate ({0}) had an invalid EntryId.", this);
					return;
				}
			}
			if (Rotations == null)
			{
				Rotations = new float[GOConstants.MaxRotations];
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
        /// Set this to fix incorrect map ids.
        /// </summary>
		public MapId RegionId
		{
			get { return MapId; }
		}

		/// <summary>
		/// Is null if Template is not spawned in a continent
		/// </summary>
		public Region Region
		{
			get { return World.GetRegion(MapId); }
		}
		#endregion

		public static IEnumerable<GOTemplate> GetAllDataHolders()
		{
			var list = new List<GOTemplate>(10000);
			foreach (var entry in GOMgr.Entries.Values)
			{
				if (entry != null)
				{
					list.AddRange(entry.Templates);
				}
			}
			return list;
		}
	}
}