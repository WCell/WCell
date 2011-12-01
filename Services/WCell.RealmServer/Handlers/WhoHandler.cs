using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class WhoHandler
	{
		/// <summary>
		/// Handles an incoming who list request
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_WHO)]
		public static void WhoListRequest(IRealmClient client, RealmPacketIn packet)
		{
			var search = new WhoSearch {
				MaxResultCount = WhoList.MaxResultCount,
				Faction = client.ActiveCharacter.Faction.Group,
				MinLevel = (byte)packet.ReadUInt32(),
				MaxLevel = (byte)packet.ReadUInt32(),
				Name = packet.ReadCString(),
				GuildName = packet.ReadCString(),
				RaceMask = (RaceMask2)packet.ReadUInt32(),
				ClassMask = (ClassMask2)packet.ReadUInt32()
			};

			uint zoneCount = packet.ReadUInt32();
			if (zoneCount > 0 && zoneCount <= 10)
			{
				for (int i = 0; i < zoneCount; i++)
					search.Zones.Add((ZoneId)packet.ReadUInt32());
			}

			uint nameCount = packet.ReadUInt32();
			if (nameCount > 0 && nameCount <= 10)
			{
				for (int i = 0; i < nameCount; i++)
					search.Names.Add(packet.ReadCString().ToLower());
			}

			//Performs the search and retrieves matching characters
			var characters = search.RetrieveMatchedCharacters();

			//Send the character list to the client
			SendWhoList(client, characters);
		}

		/// <summary>
		/// Sends to the specified client the Who List based on the given characters
		/// </summary>
		/// <param name="client">The client to send the list</param>
		/// <param name="characters">The list of characters that matched the Who List search</param>
		public static void SendWhoList(IPacketReceiver client, ICollection<Character> characters)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_WHO))
			{
				packet.Write(characters.Count);
				packet.Write(characters.Count);

				foreach (Character character in characters)
				{
					packet.WriteCString(character.Name);
                    packet.WriteCString(character.Guild != null ? character.Guild.Name : string.Empty);
					packet.Write(character.Level);
					packet.WriteUInt((byte)character.Class);
					packet.WriteUInt((byte)character.Race);
					packet.WriteByte(0); //New in 2.4.x
					packet.Write(character.Zone != null ? (uint)character.Zone.Id : 0);
				}
				client.Send(packet);
			}
		}
	}
}