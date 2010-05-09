using System;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Handlers
{
	public class InstanceHandler
	{
		#region Handling
		[ClientPacketHandler(RealmServerOpCode.CMSG_RESET_INSTANCES)]
		public static void HandleInstanceReset(IRealmClient client, RealmPacketIn packet)
		{
			if (client.ActiveCharacter != null && client.ActiveCharacter.HasInstanceCollection)
			{
				client.ActiveCharacter.Instances.TryResetInstances();
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_REQUEST_RAID_INFO)]
		public static void RequestRaidInfo(IRealmClient client, RealmPacketIn packet)
		{
			SendRaidInfo(client.ActiveCharacter);
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_SET_RAID_DIFFICULTY)]
		public static void HandleRaidDifficulty(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var difficultyIndex = packet.ReadUInt32();
			// TODO: set difficulty & reset
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_SET_DUNGEON_DIFFICULTY)]
		public static void HandleDungeonDifficulty(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			if (chr.Region.IsInstance)
			{
				// Cannot change difficulty while in instance
				return;
			}

			var difficultyIndex = packet.ReadUInt32();

			var group = chr.Group;
			if (group != null)
			{
				if (group.Leader.Character != chr)
				{
					// Only leader can change difficulty
					return;
				}
			}

			var instances = chr.Instances;
			if (instances.TryResetInstances())
			{
				if (group != null)
				{
					group.DungeonDifficulty = difficultyIndex;
				}
				else
				{
					chr.DungeonDifficulty = (DungeonDifficulty)difficultyIndex;
				}
			}
		}

		#endregion


		#region Send
		/// <summary>
		/// An instance has been reset
		/// </summary>
		public static void SendInstanceReset(IPacketReceiver client, MapId mapId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_LAST_INSTANCE, 4))
			{
				packet.Write((int)mapId);
				client.Send(packet);
			}
		}

		/// <summary>
		/// An instance has been saved
		/// </summary>
		public static void SendInstanceSave(IPacketReceiver client, MapId mapId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_LAST_INSTANCE,4))
			{
				packet.Write((int)mapId);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Starts the kick timer
		/// </summary>
		public static void SendRequiresRaid(IPacketReceiver client, int time)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RAID_GROUP_ONLY, 8))
			{
				packet.Write(time);
				packet.Write(1);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Stops the kick timer
		/// </summary>
		public static void SendRaidTimerReset(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RAID_GROUP_ONLY, 8))
			{
				packet.Write(0);
				packet.Write(0);
				client.Send(packet);
			}
		}

		public static void SendRaidInfo(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RAID_INSTANCE_INFO))
			{
				if (chr.HasInstanceCollection)
				{
					packet.Position += 4;
					uint count = 0;
					chr.Instances.ForeachBinding(BindingType.Hard, binding =>
					{
						var timeLeft = binding.NextResetTime - DateTime.Now;
						if (timeLeft.Ticks > 0)
						{
							count++;
							packet.Write((uint)binding.RegionId);
                            packet.Write(binding.DifficultyIndex);
							packet.Write(binding.InstanceId);
                            packet.WriteByte(0x1); // expired = 0
                            packet.WriteByte(0x0); // extended = 1
                            packet.Write((uint)timeLeft.TotalSeconds);
                            //packet.Write(0); // unk, extended?
						}
					});
					packet.Position = packet.HeaderSize;
					packet.Write(count);
				}
				else
				{
					packet.Write(0);
				}

				chr.Client.Send(packet);
			}
		}
		/// <summary>
		/// Sends the result of an instance reset attempt
		/// </summary>
		/// <param name="client"></param>
		/// <param name="reason"></param>
		public static void SendResetFailure(IPacketReceiver client, MapId map, InstanceResetFailed reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INSTANCE_RESET_FAILED, 8))
			{
				packet.Write((uint)reason);
				packet.Write((uint)map);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Warns a player within the instance that the leader is attempting to reset the instance
		/// </summary>
		/// <param name="client"></param>
		public static void SendResetWarning(IPacketReceiver client, MapId map)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESET_FAILED_NOTIFY, 4))
			{
				packet.Write((uint)map);
				client.Send(packet);
			}
		}

		public static void SendRaidDifficulty(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_SET_RAID_DIFFICULTY, 12))
			{
				var group = chr.Group;
				if (group is RaidGroup)
				{
					packet.Write(group.DungeonDifficulty);
					packet.Write(1); // val
					packet.Write(1); // isingroup
				}
				else // this is wrong? one packet def should be enough
				{
					packet.Write((int)chr.Record.DungeonDifficulty);
					packet.Write(1);
					packet.Write(0);
				}
			}
		}

		public static void SendDungeonDifficulty(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_SET_DUNGEON_DIFFICULTY, 12))
			{
				var group = chr.Group;
				if (group != null && !group.Flags.Has(GroupFlags.Raid))
				{
					packet.Write(group.DungeonDifficulty);
					packet.Write(1); // val
					packet.Write(1); // isingroup
				}
				else // this is wrong? one packet def should be enough
				{
					packet.Write((int) chr.Record.DungeonDifficulty);
					packet.Write(1);
					packet.Write(0);
				}

				chr.Send(packet);
			}
		}
		#endregion
	}
}
