using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class TutorialHandler
	{
		[ClientPacketHandler(RealmServerOpCode.CMSG_TUTORIAL_FLAG)]
		public static void HandleSetTutorialFlag(IRealmClient client, RealmPacketIn packet)
		{
			uint flagIndex = packet.ReadUInt32();

			if (flagIndex >= 256)
			{
				// seems to be 10242 sometimes

				// client.Disconnect();
				// return;
			}
			else
			{
				client.ActiveCharacter.TutorialFlags.SetFlag(flagIndex);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_TUTORIAL_CLEAR)]
		public static void HandleClearTutorialFlags(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.TutorialFlags.ClearFlags();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_TUTORIAL_RESET)]
		public static void HandleResetTutorialFlags(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.TutorialFlags.ResetFlags();
		}

		public static void SendTutorialFlags(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TUTORIAL_FLAGS, 32))
			{
				packet.Write(chr.TutorialFlags.FlagData);

				chr.Client.Send(packet);
			}
		}
	}
}