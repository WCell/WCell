/*************************************************************************
 *
 *   file		: MonsterWaypoint.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-06 22:29:21 +0800 (Wed, 06 Feb 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 111 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants.Misc;
using WCell.Core.Paths;
using WCell.RealmServer.Content;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util.Data;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
	[DataHolder]
	public class WaypointEntry : IDataHolder, IPathVertex
	{
		public static readonly LinkedList<WaypointEntry> EmptyList = new LinkedList<WaypointEntry>();

		public uint SpawnId { get; set; }

		/// <summary>
		/// Id of this waypoint in the chain
		/// </summary>
		public uint Id { get; set; }

		public Vector3 Position { get; set; }

		public float Orientation { get; set; }

		public uint WaitTime { get; set; }

		public float GetDistanceToNext()
		{
			throw new NotImplementedException("Not implemented yet.");
		}

		public uint Flags;

		public EmoteType Emote;

		public SpellId SpellId;

		public uint ArriveDisplayId, LeaveDisplayId;

		[NotPersistent]
		public NPCSpawnEntry SpawnEntry;

		[NotPersistent]
		public LinkedListNode<WaypointEntry> Node;

		public void FinalizeDataHolder()
		{
			SpawnEntry = NPCMgr.GetSpawnEntry(SpawnId);
			if (SpawnEntry == null)
			{
				ContentMgr.OnInvalidDBData("{0} had an invalid SpawnId.", this);
			}
			else
			{
				var added = false;
				var cur = SpawnEntry.Waypoints.First;
				while (cur != null)
				{
					if (cur.Value.Id > Id)
					{
						Node = cur.List.AddBefore(cur, this);
						added = true;
						break;
					}
					
					if (cur.Value.Id == Id)
					{
						ContentMgr.OnInvalidDBData("Found multiple Waypoints with the same Id {0} for SpawnEntry {1}", Id, SpawnEntry);
						return;
					}
					cur = cur.Next;
				}

				if (!added)
				{
					SpawnEntry.HasDefaultWaypoints = false;
					Node = SpawnEntry.Waypoints.AddLast(this);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("NPCWaypoint {0} {1}", SpawnId, Id);
		}

		public static IEnumerable<WaypointEntry> GetAllDataHolders()
		{
			var list = new List<WaypointEntry>(NPCMgr.SpawnEntries.Length * 10);
			foreach (var npc in NPCMgr.SpawnEntries)
			{
				if (npc != null && npc.Waypoints != null)
				{
					list.AddRange(npc.Waypoints);
				}
			}
			return list;
		}
	}
}