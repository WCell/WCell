using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.AI;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Waypoints;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;
using NLog;

namespace WCell.RealmServer.NPCs
{
	/// <summary>
	/// Spawn-information for NPCs
	/// </summary>
	[DataHolder]
	public partial class SpawnEntry : IDataHolder, IWorldLocation
	{
		private static uint highestSpawnId;
		public static uint GenerateSpawnId()
		{
			return ++highestSpawnId;
		}

		public SpawnEntry()
		{

		}

		public uint SpawnId;
		public NPCId EntryId;

		private MapId m_RegionId = MapId.End;

		public MapId RegionId
		{
			get { return m_RegionId; }
			set { m_RegionId = value; }
		}

		public Region Region
		{
			get { return World.GetRegion(RegionId); }
		}

		/// <summary>
		/// The position of this SpawnEntry
		/// </summary>
		public Vector3 Position
		{
			get;
			set;
		}

		public uint PhaseMask = 1;

		public float Orientation;
		public AIMovementType MoveType;

		public uint Bytes;
		public uint Bytes2;
		public EmoteType EmoteState;
		public uint MountId;

		/// <summary>
		/// The amount of units to be spawned max
		/// </summary>
		public int MaxAmount;

		public int RespawnSeconds;

		/// <summary>
		/// Min Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMin;

		/// <summary>
		/// Max Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMax;

		public bool IsDead;

		public uint DisplayIdOverride;

		public string AuraIdStr;

		public uint EquipmentId;

		public uint EventId;

		public bool AutoSpawn = true;

		[NotPersistent]
		public NPCEquipmentEntry Equipment;

		[NotPersistent]
		public SpellId[] AuraIds;

		/// <summary>
		/// If the number of spawned creatures drops below this limit, the critical respawn-delay is used
		/// </summary>
		//public uint CriticalAmount;

		/// <summary>
		/// Min Delay when amount of spawned NPCs drops below CriticalAmount
		/// </summary>
		//public uint CriticalDelayMin;

		/// <summary>
		/// Max Delay when amount of spawned NPCs drops below CriticalAmount
		/// </summary>
		//public uint CriticalDelayMax;

		[NotPersistent]
		public NPCEntry Entry;

		[NotPersistent]
		public readonly LinkedList<WaypointEntry> Waypoints = new LinkedList<WaypointEntry>();

		private bool m_HasDefaultWaypoints;

		/// <summary>
		/// Whether this SpawnEntry has fixed Waypoints from DB
		/// </summary>
		[NotPersistent]
		public bool HasDefaultWaypoints
		{
			get { return m_HasDefaultWaypoints; }
			set
			{
				m_HasDefaultWaypoints = value;
				Entry.MovesRandomly = false;
			}
		}

		[NotPersistent]
		public List<Spell> Auras;

		public void AddAura(SpellId spellId)
		{
			var spell = SpellHandler.Get(spellId);
			if (spell == null)
			{
				throw new ArgumentException("Trying to add invalid Spell: " + spellId);
			}
			Auras.Add(spell);
		}

		public void RemoveAura(SpellId spellId)
		{
			Auras.RemoveFirst(spell => spell.SpellId == spellId);
		}

		public int GetRandomRespawnMillis()
		{
			return 1000 * Utility.Random(RespawnSecondsMin, RespawnSecondsMax);
		}

		#region WPs
		public int WPCount
		{
			get { return Waypoints.Count; }
		}

		public WaypointEntry CreateWP(Vector3 pos, float orientation)
		{
			var last = Waypoints.Last;
			WaypointEntry entry;
			if (last != null)
			{
				entry = new WaypointEntry
				{
					Id = last.Value.Id + 1,
					SpawnEntry = this,
				};
			}
			else
			{
				entry = new WaypointEntry
				{
					Id = 1,
					SpawnEntry = this,
				};
			}

			entry.Position = pos;
			entry.Orientation = orientation;
			return entry;
		}

		/// <summary>
		/// Adds the given positions as WPs
		/// </summary>
		/// <param name="wps"></param>
		public void AddWPs(Vector3[] wps)
		{
			if (wps.Length < 1)
			{
				throw new ArgumentException("wps are empty.");
			}
			var len = wps.Length;
			Vector3 first;
			if (Waypoints.Count > 0)
			{
				// adding to already existing WPs: Make sure that the orientation is correct
				first = Waypoints.First.Value.Position;
				var last = Waypoints.Last.Value;
				last.Orientation = last.Position.GetAngleTowards(wps[0]);
			}
			else
			{
				first = wps[0];
			}

			for (var i = 0; i < len; i++)
			{
				var pos = wps[i];
				WaypointEntry wp;
				if (i < len - 1)
				{
					wp = CreateWP(pos, pos.GetAngleTowards(wps[i+1]));
				}
				else if (i > 0)
				{
					wp = CreateWP(pos, pos.GetAngleTowards(first));
				}
				else
				{
					wp = CreateWP(pos, Utility.Random(0f, 2 * MathUtil.PI));
				}
				Waypoints.AddLast(wp);
			}
		}

		public LinkedListNode<WaypointEntry> AddWP(Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddLast(newWp);
		}

		public LinkedListNode<WaypointEntry> InsertWPAfter(WaypointEntry entry, Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddAfter(entry.Node, newWp);
		}

		public LinkedListNode<WaypointEntry> InsertWPBefore(WaypointEntry entry, Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddBefore(entry.Node, newWp);
		}
		#endregion

		/// <summary>
		/// Increases the Id of all Waypoints, starting from the given node
		/// </summary>
		/// <param name="node">May be null</param>
		public void IncreaseWPIds(LinkedListNode<WaypointEntry> node)
		{
			while (node != null)
			{
				node.Value.Id++;
				node = node.Next;
			}
		}

		//public int GetRandomCriticalDelay()
		//{
		//    return (int)Utility.Random(CriticalDelayMin, CriticalDelayMax);
		//}

		public void FinalizeDataHolder()
		{
			if (Entry == null)
			{
				Entry = NPCMgr.GetEntry(EntryId);
				if (Entry == null)
				{
					ContentHandler.OnInvalidDBData("{0} had an invalid EntryId.", this);
					return;
				}
			}

			var entry = Entry;

			foreach (var handler in entry.SpawnTypeHandlers)
			{
				if (handler != null)
				{
					handler(this);
				}
			}

			if (MaxAmount == 0)
			{
				MaxAmount = 1;
			}

			var def = RespawnSeconds != 0 ? RespawnSeconds : NPCMgr.DefaultMinRespawnDelay;

			if (RespawnSecondsMin == 0)
			{
				RespawnSecondsMin = def;
			}

			if (RespawnSecondsMax == 0)
			{
				RespawnSecondsMax = Math.Max(def, RespawnSecondsMin);
			}

			if (PhaseMask == 0)
			{
				PhaseMask = 1;
			}

			if (EquipmentId != 0)
			{
				Equipment = NPCMgr.GetEquipment(EquipmentId);
			}

			// seems to be a DB issue where the id was inserted as a float
			if (DisplayIdOverride == 16777215)
			{
				DisplayIdOverride = 0;
			}

			SetAuras();

			if (SpawnId > highestSpawnId)
			{
				highestSpawnId = SpawnId;
			}
			else if (SpawnId == 0)
			{
				SpawnId = GenerateSpawnId();
			}

			if (RegionId != MapId.End)
			{
				Entry.SpawnEntries.Add(this);
				ArrayUtil.Set(ref NPCMgr.SpawnEntries, SpawnId, this);

				if ((uint)RegionId >= NPCMgr.SpawnEntriesByMap.Length)
				{
					ArrayUtil.EnsureSize(ref NPCMgr.SpawnEntriesByMap, (int)RegionId + 100);
				}

				var list = NPCMgr.SpawnEntriesByMap[(uint)RegionId];
				if (list == null)
				{
					list = NPCMgr.SpawnEntriesByMap[(uint)RegionId] = new List<SpawnEntry>(5000);
				}

				list.Add(this);

				if (RealmServer.Instance.IsRunning && AutoSpawn)
				{
					var rgn = World.GetRegion(RegionId);
					if (rgn != null && rgn.NPCsSpawned)
					{
						rgn.ExecuteInContext(() => rgn.AddSpawn(this));
					}
				}
			}
		}

		private void SetAuras()
		{
			if (!string.IsNullOrEmpty(AuraIdStr))
			{
				AuraIds = AuraIdStr.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).TransformArray(idStr =>
				{
					uint id;
					if (!uint.TryParse(idStr.Trim(), out id))
					{
						LogManager.GetCurrentClassLogger().Warn("Invalidly formatted Aura ({0}) in AuraString for SpawnEntry: {1}", idStr, this);
					}
					return (SpellId)id;
				});
			}

			if (AuraIds != null)
			{
				Auras = new List<Spell>(AuraIds.Length);
				foreach (var auraId in AuraIds)
				{
					var spell = SpellHandler.Get(auraId);
					if (spell != null)
					{
						if (spell.Durations.Min > 0 && spell.Durations.Min < int.MaxValue)
						{
							// not permanent -> Spell
							if (Entry.Spells == null || !Entry.Spells.ContainsKey(spell.Id))
							{
								Entry.AddSpell(spell);
							}
						}
						else
						{
							Auras.Add(spell);
						}
					}
				}
			}
			else
			{
				Auras = new List<Spell>(1);
			}
		}

		#region Events
		internal void NotifySpawned(NPC npc)
		{
			var evt = Spawned;
			if (evt != null)
			{
				evt(npc);
			}
		}
		#endregion

		public void GenerateRandomWPs()
		{
			Waypoints.Clear();
			AddRandomWPs();
		}

		public void AddRandomWPs()
		{
			var terrain = TerrainMgr.GetTerrain(RegionId);
			if (terrain != null)
			{
				var gen = new RandomWaypointGenerator();
				var wps = gen.GenerateWaypoints(terrain, Position);
				AddWPs(wps);
			}
		}

		public override string ToString()
		{
			return string.Format(GetType().Name + " #{0} ({1} #{2})", SpawnId, EntryId, (uint)EntryId);
		}

		public static IEnumerable<SpawnEntry> GetAllDataHolders()
		{
			return NPCMgr.SpawnEntries;
		}
	}
}
