using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Entities
{
	public partial class GameObject
	{
		public void SendCustomAnim(uint anim)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_CUSTOM_ANIM, 12))
			{
				packet.Write(EntityId);
				packet.Write(anim);
				SendPacketToArea(packet);
			}
		}

		public void SendDespawn()
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_DESPAWN_ANIM, 8))
			{
				packet.Write(EntityId);

				SendPacketToArea(packet);
			}
		}
	}
}

namespace WCell.RealmServer.GameObjects
{
    public static partial class GOMgr
    {
        [ClientPacketHandler(RealmServerOpCode.CMSG_GAMEOBJECT_QUERY)]
		public static void HandleGOQuery(IRealmClient client, RealmPacketIn packet)
		{
			var entryId = packet.ReadUInt32();
			//EntityId goId = packet.ReadEntityId();

			if (Loaded)
			{
				var entry = GetEntry(entryId);
				if (entry != null)
				{
					SendGameObjectInfo(client, entry);
				}
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_GAMEOBJ_REPORT_USE)]
		public static void HandleGOReportUse(IRealmClient client, RealmPacketIn packet)
		{
			var goId = packet.ReadEntityId();

			var go = client.ActiveCharacter.Region.GetGO(goId.Low);
			var chr = client.ActiveCharacter;
			if (go != null && go.CanUseInstantly(chr) && (chr.LooterEntry.Loot == null || !object.ReferenceEquals(chr.LooterEntry.Loot.Lootable, go) ))
			{
				go.Use(client.ActiveCharacter);
			}
		}

        public static void SendGameObjectInfo(IRealmClient client, GOEntry entry)
        {
            var name = entry.Names.Localize(client);
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE,
                                                   13 + 6 + name.Length + (24 * 4)))
            {
                packet.Write(entry.Id);
                packet.Write((uint)entry.Type);
                packet.Write(entry.DisplayId);
                packet.Write(name);
                packet.Write((byte)0); // Name2
                packet.Write((byte)0); // Name3
                packet.Write((byte)0); // Name4
                packet.Write((byte)0); // string IconName
                packet.Write((byte)0); // string. Casting bar text
                packet.Write((byte)0); // string

                int i = 0;
                for (; i < entry.Fields.Length; i++)
                {
                    packet.Write(entry.Fields[i]);
                }

                // must be 24 fields
                while (i < GOConstants.EntryFieldCount)
                {
                    packet.Write(0);
                    i++;
                }

                packet.Write(entry.DefaultScale); // size

                for (i = 0; i < 4; i++)
                {
                    packet.Write(0); // new 3.1
                }

                client.Send(packet);
            }
        }
    }
}