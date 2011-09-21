using System;
using Cell.Core;
using WCell.Constants.World;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Global
{
	public class WorldStateCollection
	{
		public readonly IWorldSpace Area;
		public readonly WorldState[] States;
		public readonly byte[] CompiledState;

		public WorldStateCollection(IWorldSpace area, WorldState[] states)
		{
			Area = area;
			States = states;
			CompiledState = new byte[8 * States.Length];

			// init default states
			for (var i = 0; i < States.Length; i++)
			{
				var state = States[i];
				Array.Copy(BitConverter.GetBytes((uint)state.Key), 0, CompiledState, i * 8, 4);
				Array.Copy(BitConverter.GetBytes(state.DefaultValue), 0, CompiledState, 4 + i * 8, 4);
			}
		}

		public int FieldCount
		{
			get { return States.Length; }
		}

		public void SetInt32(WorldStateId id, int value)
		{
			SetInt32(WorldStates.GetState(id), value);
		}

		public void SetInt32(WorldState state, int value)
		{
			Array.Copy(BitConverter.GetBytes(value), 0, CompiledState, 4 + state.Index * 8, 4);
			OnStateChanged(state, value);
		}

		public void SetUInt32(WorldStateId id, uint value)
		{
			SetUInt32(WorldStates.GetState(id), value);
		}

		public void SetUInt32(WorldState state, uint value)
		{
			Array.Copy(BitConverter.GetBytes(value), 0, CompiledState, 4 + state.Index * 8, 4);
			OnStateChanged(state, (int)value);
		}

		public uint GetUInt32(WorldStateId id)
		{
			return GetUInt32(WorldStates.GetState(id).Index);
		}

		public uint GetUInt32(uint index)
		{
			return CompiledState.GetUInt32(1 + index * 2);
		}

		public int GetInt32(WorldStateId id)
		{
			return GetInt32(WorldStates.GetState(id).Index);
		}

		public int GetInt32(uint index)
		{
			return CompiledState.GetInt32(1 + index * 2);
		}

		internal void UpdateWorldState(uint index, int value)
		{
			Array.Copy(BitConverter.GetBytes(value), 0, CompiledState, 4 + index * 8, 4);
		}

		private void OnStateChanged(WorldState state, int value)
		{
			Area.CallOnAllCharacters(chr => WorldStateHandler.SendUpdateWorldState(chr, state.Key, value));
		}

	}
}