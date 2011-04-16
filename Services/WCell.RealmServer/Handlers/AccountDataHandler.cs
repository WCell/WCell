using System;
using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
	public static class AccountDataHandler
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
		
		public enum CacheMask : uint
		{
			GlobalCache = 0x15,
			PerCharacterCache = 0xEA
		}


		#region IN
		[PacketHandler(RealmServerOpCode.CMSG_REQUEST_ACCOUNT_DATA, RequiresLogin = false, IsGamePacket = false)]
		public static void HandleRequestAccountData(IRealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			var type = packet.ReadUInt32();

			if (type > 8)
			{
				s_log.Error("{0} sent data type > 8", client);
				client.Disconnect();
				return;
			}

			SendAccountData(client, type);
		}

		[PacketHandler(RealmServerOpCode.CMSG_UPDATE_ACCOUNT_DATA, RequiresLogin = false, IsGamePacket = false)]
		public static void HandleUpdateAccountData(IRealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			var type = packet.ReadUInt32();
			var time = packet.ReadUInt32();
			var compressedSize = packet.ReadUInt32();

			if (compressedSize > 65535)
			{
				s_log.Warn("{0} sent a too large data update: " + compressedSize, client.Account);
				client.Disconnect();
				return;
			}

			client.Account.AccountData.SetAccountData(type, time, packet.ReadBytes(packet.RemainingLength), compressedSize);
		}

		[PacketHandler(RealmServerOpCode.CMSG_READY_FOR_ACCOUNT_DATA_TIMES, RequiresLogin = false, IsGamePacket = false)]
		public static void HandleReadyForAccountDataTimes(IRealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			SendAccountDataTimes(client, CacheMask.GlobalCache);
		}
		#endregion

		#region OUT

		/// <summary>
		/// Send the unix timestamps of each AccountDataType to the client.
		/// </summary>
		/// <param name="client"></param>
		public static void SendAccountDataTimes(IRealmClient client)
		{
			SendAccountDataTimes(client, CacheMask.PerCharacterCache);
		}

		/// <summary>
		/// Send the unix timestamps of each AccountDataType to the client.
		/// </summary>
		/// <param name="client"></param>
		public static void SendAccountDataTimes(IRealmClient client, CacheMask mask)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACCOUNT_DATA_TIMES))
			{
				var now = Utility.GetEpochTimeFromDT(DateTime.Now);
				packet.Write(now);		// current server time
				packet.Write((byte)1);	// unknown
				packet.Write((uint)mask);

				if (client.Account != null &&
					client.Account.AccountData != null &&
					client.Account.AccountData.TimeStamps != null)
				{
					for (var i = 0; i < 8; i++)
					{
						if ((((uint)mask) & (1 << i)) != 0)
						{
							packet.Write(client.Account.AccountData.TimeStamps[i]);
						}
					}
				}
				else
				{
					LogManager.GetCurrentClassLogger().Debug("Client was not properly logged in when sending ACCOUNT_DATA_TIMES: " + client);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends account data of a specific type to the specified client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="type"></param>
		public static void SendAccountData(IRealmClient client, uint type)
		{
			int deCompressedSize = client.Account.AccountData.SizeHolder[type];

			using (var dataPacket = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_ACCOUNT_DATA, deCompressedSize + 20))
			{
			    var guid = client.ActiveCharacter != null ? client.ActiveCharacter.EntityId.Full : EntityId.Zero;
				dataPacket.Write(guid);
				dataPacket.Write(type);
				dataPacket.Write(client.Account.AccountData.TimeStamps[type]);

				dataPacket.Write(deCompressedSize);

				if (client.Account.AccountData.DataHolder[type] != null)
				{
					dataPacket.Write(client.Account.AccountData.DataHolder[type]);
				}

				client.Send(dataPacket);
			}
			return;
		}
		#endregion
	}
}