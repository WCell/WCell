using System;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class MirrorImageHandler
	{
		[ClientPacketHandler(RealmServerOpCode.CMSG_GET_MIRRORIMAGE_DATA)]
		public static void HandleGetMirrorImageData(IRealmClient client, RealmPacketIn packet)
		{
			var guid = packet.ReadEntityId();
			var image = client.ActiveCharacter.Map.GetObject(guid) as NPC;
			SendMirrorImageData(client, image);
		}
		public static void SendMirrorImageData(IRealmClient client, NPC mirrorimage)
		{
			var owner = mirrorimage.PlayerOwner;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MIRRORIMAGE_DATA, 68))
			{
				packet.Write(mirrorimage.EntityId);
				packet.Write(owner.DisplayId);
				if (owner != null) //player
				{
					packet.Write((byte)owner.Race);
					packet.Write((byte)owner.Gender);
					packet.Write((byte)owner.Class);
					packet.Write(owner.Skin);
					packet.Write(owner.Facial);
					packet.Write(owner.HairStyle);
					packet.Write(owner.HairColor);
					packet.Write(owner.FacialHair);
					packet.Write(owner.GuildId);

					foreach(VisibleEquipmentSlot slot in Enum.GetValues(typeof(VisibleEquipmentSlot)))
					{
						var item = owner.Inventory.Equipment[(EquipmentSlot)slot];
						if (slot == VisibleEquipmentSlot.Head && ((owner.PlayerFlags & PlayerFlags.HideHelm) != 0))
						{
							packet.Write(0);
						}
						else if (slot == VisibleEquipmentSlot.Back && ((owner.PlayerFlags & PlayerFlags.HideCloak) != 0))
						{
							packet.Write(0);
						}
						else if (item != null)
						{
							packet.Write(item.Template.DisplayId);
						}
						else
							packet.Write(0);
					}
					
				}
				else //creature
				{
					for (int i = 0; i < 14; i++)
					{
						packet.Write(0);
					}

				}
				client.Send(packet);
			}
		}

	}
}
