using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.World
{
	public class WorldState
	{
		public static readonly WorldState[] EmptyArray = new WorldState[0];

		public readonly MapId MapId = MapId.End;
		public readonly ZoneId ZoneId;
		public uint Index
		{
			get;
			internal set;
		}

		public readonly WorldStateId Key;
		public int DefaultValue;

		public WorldState(WorldStateId key, int value) : this(MapId.End, key, value)
		{
		}

		public WorldState(MapId mapId, WorldStateId key, int value) : this(mapId, ZoneId.None, key, value)
		{
		}

		public WorldState(MapId mapId, ZoneId zoneId, WorldStateId key, int value)
		{
			MapId = mapId;
			ZoneId = zoneId;
			Key = key;
			DefaultValue = value;
		}

		public WorldState(uint key, int value) : this(MapId.End, key, value)
		{
		}

		public WorldState(MapId mapId, uint key, int value) : this(mapId, ZoneId.None, key, value)
		{
		}

		public WorldState(MapId mapId, ZoneId zoneId, uint key, int value)
		{
			MapId = mapId;
			ZoneId = zoneId;
			Key = (WorldStateId)key;
			DefaultValue = value;
		}

		public bool IsGlobal
		{
			get { return MapId == MapId.End; }
		}

		public bool IsRegional
		{
			get { return ZoneId == ZoneId.None; }
		}
	}
}