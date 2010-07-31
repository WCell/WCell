using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;

namespace WCell.Constants.World
{
	public static class WorldStates
	{
		public static readonly WorldState[] EmptyStates = new WorldState[0];

		public static readonly WorldState[] AllStates = new WorldState[6000];

		public static WorldState[] GlobalStates = new WorldState[1];

		public static readonly WorldState[][] RegionStates = new WorldState[(int)MapId.End][];

		public static readonly WorldState[][] ZoneStates = new WorldState[(int)ZoneId.End][];

		static WorldStates()
		{
			CreateStates();

			foreach (var state in AllStates)
			{
				if (state != null)
				{
					WorldState[] arr;
					if (state.ZoneId != ZoneId.None)
					{
						arr = GetStates(state.ZoneId);
						if (arr == null)
						{
							arr = new WorldState[1];
						}
						ArrayUtil.AddOnlyOne(ref arr, state);
						ZoneStates[(int)state.MapId] = arr;
					}
					else if (state.MapId != MapId.End)
					{
						arr = GetStates(state.MapId);
						if (arr == null)
						{
							arr = new WorldState[1];
						}
						ArrayUtil.AddOnlyOne(ref arr, state);
						RegionStates[(int)state.MapId] = arr;
					}
					else
					{
						ArrayUtil.AddOnlyOne(ref GlobalStates, state);
						arr = GlobalStates;
					}
					state.Index = (uint)arr.Length - 1;
				}
			}
		}

		private static void AddState(WorldState state)
		{
			AllStates[(int)state.Key] = state;
		}

		public static WorldState[] GetStates(MapId region)
		{
			return RegionStates.Get((uint)region) ?? new WorldState[0];
		}

		public static WorldState[] GetStates(ZoneId zone)
		{
			return ZoneStates.Get((uint)zone) ?? new WorldState[0];
		}

		public static WorldState GetState(WorldStateId id)
		{
			return AllStates[(int)id];
		}
		
		#region State Definitions
		static void CreateStates()
		{
			// global states
			AddState(new WorldState(2264, 0));
			AddState(new WorldState(2263, 0));
			AddState(new WorldState(2262, 0));
			AddState(new WorldState(2261, 0));
			AddState(new WorldState(2260, 0));
			AddState(new WorldState(2259, 0));
			AddState(new WorldState(3191, 0));
			AddState(new WorldState(3901, 1));

			// WSG
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceScore, 0));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeScore, 0));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAlliancePickupState, 0));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordePickupState, 0));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGUnknown, 2));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGMaxScore, 3));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGHordeFlagState, 1));
			AddState(new WorldState(MapId.WarsongGulch, WorldStateId.WSGAllianceFlagState, 1));

            // Arathi Basin
            AddState(new WorldState(MapId.ArathiBasin, WorldStateId.ABMaxResources, 1600));
		}
		#endregion
	}
}