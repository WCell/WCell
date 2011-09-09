using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class WorldStateHandler
	{
		public static void SendInitWorldStates(IPacketReceiver rcv, WorldStateCollection states, Zone newZone)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INIT_WORLD_STATES, 300))
			{
				packet.Write((uint)newZone.Map.Id);
				packet.Write((uint)newZone.ParentZoneId);
				packet.Write((uint)newZone.Id);

				var countPos = packet.Position;
				packet.Position += 2;

				var count = AppendWorldStates(packet, newZone);

				packet.Position = countPos;
				packet.Write((ushort)count);

				rcv.Send(packet);
			}
		}

		static int AppendWorldStates(RealmPacketOut packet, IWorldSpace space)
		{
			var count = 0;
			if (space.ParentSpace != null)
			{
				count += AppendWorldStates(packet, space.ParentSpace);
			}
			if (space.WorldStates != null)
			{
				count += space.WorldStates.FieldCount;
				packet.Write(space.WorldStates.CompiledState);
			}
			return count;
		}

		public static void SendInitWorldStates(IPacketReceiver rcv, MapId map, ZoneId zone, uint areaId, params WorldState[] states)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INIT_WORLD_STATES, 300))
			{
				packet.Write((uint)map);
				packet.Write((uint)zone);
				packet.Write(areaId);
				packet.Write((ushort)states.Length);
				foreach (var state in states)
				{
					packet.Write((uint)state.Key);
					packet.Write(state.DefaultValue);
				}
				rcv.Send(packet);
			}
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldState state)
		{
			SendUpdateWorldState(rcv, state.Key, state.DefaultValue);
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldStateId key, int value)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_WORLD_STATE, 300))
			{
				packet.Write((uint)key);
				packet.Write(value);
				rcv.Send(packet);
			}
		}
	}
}
