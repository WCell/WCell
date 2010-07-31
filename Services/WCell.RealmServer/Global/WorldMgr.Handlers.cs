/*************************************************************************
 *
 *   file		: WorldMgr.Handlers.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-02 11:23:10 +0800 (Wed, 02 Apr 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 214 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using WCell.Core;
using WCell.Cryptography;
using WCell.Intercommunication.Client;
using WCell.Core.Network;
using WCell.RealmServer;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Localization;
using WCell.RealmServer.Races;
using Version = System.Version;
using WCell.Core.DBC;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.World
{
	public partial class WorldMgr
	{
		/// <summary>
		/// Handles an incoming ping request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_PING, IsGamePacket = false)]
		public static void PingRequest(RealmClient client, RealmPacketIn packet)
		{
			SendPingReply(client, packet.ReadUInt32());

			client.Latency = packet.ReadUInt32();
		}

		/// <summary>
		/// Handles an incoming auth session request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[PacketHandler(RealmServerOpCode.CMSG_AUTH_SESSION, IsGamePacket = false)]
		public static void AuthSessionRequest(RealmClient client, RealmPacketIn packet)
		{
			packet.SkipBytes(8);
			string accName = packet.ReadString();
			uint clientSeed = packet.ReadUInt32();
			BigInteger clientDigest = packet.ReadBigInteger(20);
			AuthenticationInfo authInfo;
			SecureRemotePassword srp;
			AuthenticationErrorCodes errorCode = AuthenticationErrorCodes.AuthFailed;

			client.Account = new Account(client, accName);

			if (!client.Account.Initialize())
			{
				errorCode = AuthenticationErrorCodes.UnknownAccount;

				goto sendErrorReply;
			}

			if (client.Server.RequestAuthenticationInfo(accName, out authInfo))
			{
				srp = new SecureRemotePassword(accName, authInfo.Verifier, new BigInteger(authInfo.Salt, 32));

				client.Account.SessionKey = authInfo.SessionKey;
				client.SystemInfo = SystemInformation.Deserialize(authInfo.SystemInformation);
			}
			else
			{
				goto sendErrorReply;
			}

			BigInteger clientVerifier = srp.Hash(srp.Username, new byte[4], clientSeed, client.Server.AuthSeed,
													client.Account.SessionKey);

			client.IsEncrypted = true; // all packets from here on are encrypted, including the AuthSessionReplys

			if (clientVerifier == clientDigest)
			{
                AddonHandler.ReadAddOns(client, packet);

				client.Server.LoginAccount(client.Account.Username);

				if (AuthQueue.QueuedClients > 0 ||
					client.Server.NumberOfClients > client.Server.Config.ServerCapacity)
				{
					AuthQueue.EnqueueClient(client);

					return;
				}

				SendAuthSessionSuccess(client);

				return;
			}
			else
			{
				goto sendErrorReply;
			}

		sendErrorReply:
			SendAuthSessionErrorReply(client, errorCode);

			client.Disconnect();
		}

		/// <summary>
		/// Handles an incoming character enum request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>        
		[PacketHandler(RealmServerOpCode.CMSG_CHAR_ENUM, IsGamePacket = false)]
		public static void CharEnumRequest(RealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			SendCharEnum(client);
		}

		/// <summary>
		/// Handles an incoming character creation request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>        
		[PacketHandler(RealmServerOpCode.CMSG_CHAR_CREATE, IsGamePacket = false)]
		public static void CharCreateRequest(RealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			try
			{
				string characterName = packet.ReadString();

				if (Character.Exists(characterName))
				{
					SendCharCreateReply(client, CharacterErrorCodes.CreateNameInUse);
					return;
				}

				if (characterName.Length < 3)
				{
					SendCharCreateReply(client, CharacterErrorCodes.NameTooShort);
					return;
				}

				if (characterName.Length > 12)
				{
					SendCharCreateReply(client, CharacterErrorCodes.NameTooLong);
					return;
				}

				if (Character.DoesNameViolate(characterName))
				{
					SendCharCreateReply(client, CharacterErrorCodes.NameProfanity);
					return;
				}

				RaceType chrRace = (RaceType)packet.ReadByte();
				ClassType chrClass = (ClassType)packet.ReadByte();

				CharacterErrorCodes createError;
				if (!RealmServer.Instance.ServerRules.CanCreateCharacter(client, chrRace, chrClass, out createError))
				{
					SendCharCreateReply(client, createError);
					return;
				}

				CharacterRecord ch = CharacterRecord.CreateNewCharacterRecord(client.Account, characterName);

				if (ch == null)
				{
					s_log.Error("Unable to create character record!");
					SendCharCreateReply(client, CharacterErrorCodes.CreateError);

					return;
				}

				ch.Race = chrRace;
				ch.Class = chrClass;
				ch.Gender = (GenderType)packet.ReadByte();
				ch.Skin = packet.ReadByte();
				ch.Face = packet.ReadByte();
				ch.HairStyle = packet.ReadByte();
				ch.HairColor = packet.ReadByte();
				ch.FacialHair = packet.ReadByte();
				ch.Outfit = packet.ReadByte();

#warning // TODO - Ogre: This should be handled elsewhere, when we have world events and stuff
				BaseRace race = GetRace(ch.Race);

				ch.Level = 1;
				ch.PositionX = race.StartPosition.X;
				ch.PositionY = race.StartPosition.Y;
				ch.PositionZ = race.StartPosition.Z;
				ch.Orientation = race.StartPosition.W;
				ch.CurrentMap = race.StartMap;
				ch.CurrentZone = race.StartZone;
				ch.TotalPlayTime = 0;
				ch.LevelPlayTime = 0;
				ch.PrivilegeLevel = "Guest";
				ch.TutorialFlags = new byte[32];
				ch.SerializedFields = new byte[0];

				if (race.Type == RaceType.BloodElf)
				{
					ch.DisplayId = race.ModelOffset - (uint)ch.Gender;
				}
				else
				{
					ch.DisplayId = race.ModelOffset + (uint)ch.Gender;
				}

				if (DBSetup.IsActive)
				{
					ch.CreateAndFlush();

					// Is it necessary to reload all?
					client.Account.ReloadCharacters();
				}
				else
					client.Account.Characters.Add(EntityId.GetPlayerId(ch.EntityId), ch);
				
				SendCharCreateReply(client, CharacterErrorCodes.CreateSucceeded);
			}
			catch (Exception e)
			{
				s_log.Error(e);
				SendCharCreateReply(client, CharacterErrorCodes.CreateError);
			}
		}

		/// <summary>
		/// Handles an incmming character deletion request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>        
		[PacketHandler(RealmServerOpCode.CMSG_CHAR_DELETE, IsGamePacket = false)]
		public static void CharDeleteRequest(RealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			CharacterErrorCodes returnCode = CharacterErrorCodes.DeleteSucceeded;
			EntityId eid = packet.ReadEntityId();
			CharacterRecord record = null;

			try
			{
				if (client.Account.Characters.ContainsKey(eid) && client.Account.Characters[eid] != null)
				{
					record = client.Account.Characters[eid];
				}
				else
				{
					// log error
					returnCode = CharacterErrorCodes.DeleteFailed;
					return;
				}

				EntityIdStorage.RecycleLowerEntityId(eid.Low, EntityIdType.Player);

				client.Account.Characters.Remove(eid);

#warning TODO (tobz): check/update/remove guild if player is last player in guild/guild leader
#warning TODO (tobz): return any unclaimed COD mails to their sender as server identity

				RelationMgr.Instance.RemoveForPlayer(eid.Low);

				record.DeleteAndFlush();
				record = null;
			}
			catch (Exception ex)
			{
				if (!client.Account.Characters.ContainsKey(eid) && record != null)
				{
					client.Account.Characters.Add(eid, record);
				}

				s_log.Error("Failed to delete character!", ex);

				returnCode = CharacterErrorCodes.DeleteFailed;
			}
			finally
			{
				SendCharDeleteReply(client, returnCode);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_CHAR_RENAME, IsGamePacket = false)]
		public static void CharacterRenameRequest(RealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			CharacterRecord cr = null;

			EntityId guid = packet.ReadEntityId();
			string newName = packet.ReadString();

			if (!client.Account.Characters.ContainsKey(guid))
			{
				s_log.Error(Resources.IllegalRenameAttempt, guid.ToString(), RealmClient.GetInfo(client));
				return;
			}
			else
			{
				cr = client.Account.Characters[guid];

				if (((CharEnumFlags)cr.CharacterFlags & CharEnumFlags.NeedsRename) != CharEnumFlags.NeedsRename)
				{
					// their character isn't flagged to be renamed, what do they think they're doing? ;)
					client.Disconnect();
					return;
				}
			}

			if (newName.Length == 0)
			{
				SendCharacterRenameError(client, AccountCharacterErrorCodes.ACCNT_MANIP_CHAR_NAME_INVALID);
				return;
			}

			if (Character.Exists(newName))
			{
				SendCharacterRenameError(client, AccountCharacterErrorCodes.ACCNT_MANIP_CHAR_NAME_USED);
				return;
			}

			if (newName.Length < 3)
			{
				SendCharacterRenameError(client, AccountCharacterErrorCodes.ACCNT_MANIP_CHAR_NAME_MIN_3);
				return;
			}

			if (newName.Length > 12)
			{
				SendCharacterRenameError(client, AccountCharacterErrorCodes.ACCNT_MANIP_CHAR_NAME_MAX_12);
				return;
			}

			if (Character.DoesNameViolate(newName))
			{
				SendCharacterRenameError(client, AccountCharacterErrorCodes.ACCNT_MANIP_CHAR_NAME_PROFANITY);
				return;
			}

			s_log.Debug(Resources.RenamingCharacter, cr.Name, newName);

			cr.Name = newName.ToCapitalizedString();
			cr.UpdateAndFlush();

			client.Account.ReloadCharacters();

			SendCharacterRenameSuccess(client, guid, newName);
		}

		public static void SendCharacterRenameError(RealmClient client, AccountCharacterErrorCodes error)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_RENAME, 1))
			{
				packet.WriteByte((byte)error);
				client.Send(packet);
			}
		}

		public static void SendCharacterRenameSuccess(RealmClient client, EntityId guid, string newName)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_RENAME, 10 + newName.Length))
			{
				packet.WriteByte((byte)AccountCharacterErrorCodes.ACCNT_MANIP_OK);
				packet.Write(guid.Full);
				packet.WriteCString(newName);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Handles an incoming player login request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>        
		[PacketHandler(RealmServerOpCode.CMSG_PLAYER_LOGIN, IsGamePacket = false)]
		public static void PlayerLoginRequest(RealmClient client, RealmPacketIn packet)
		{
			if (client.Account == null)
			{
				return;
			}

			try
			{
				EntityId charGUID = packet.ReadEntityId();

				if (charGUID != EntityId.Zero)
				{
					if (s_characters.ContainsKey(charGUID) && !s_characters[charGUID].IsLoggingOut)
					{
						s_log.Error(string.Format(Resources.CharacterAlreadyConnected, charGUID, client.Account.Username));

						Character.SendCharacterLoginFail(client, LoginErrorCodes.CHAR_LOGIN_DUPLICATE_CHARACTER);
					}
					else
					{
						if (!client.Account.Characters.ContainsKey(charGUID))
						{
							s_log.Error(string.Format(Resources.CharacterNotFound, charGUID, client.Account.Username));

							Character.SendCharacterLoginFail(client, LoginErrorCodes.CHAR_LOGIN_NO_CHARACTER);

							client.Server.DisconnectClient(client);
						}
						else
						{
							Character existingChar = null;

							// this handles character reconnection cases, which relinks a character
							// in the process of being logged out.
							if (s_characters.TryGetValue(charGUID, out existingChar))
							{
								existingChar.ReconnectCharacter(client);
							}
							else
							{
								// Set and register the current character.
								client.ActiveCharacter = new Character(client.Account, client.Account.Characters[charGUID], client);
								RegisterCharacter(client.ActiveCharacter);

								// Start character login process
								client.ActiveCharacter.FirstLogin();
							}

							// Give them our version. (as if they care :p)
							Version ver = Assembly.GetCallingAssembly().GetName().Version;
							string message = string.Format("Welcome to WCell {0}.{1}.{2}.{3}",
								ver.Major, ver.Minor, ver.Revision, ver.Build);

							ChatMgr.SendSystemMessage(client, message);
						}
					}
				}
			}
			catch (Exception e)
			{
				s_log.Error(e);
				Character.SendCharacterLoginFail(client, LoginErrorCodes.CHAR_LOGIN_FAILED);
			}
		}

		/// <summary>
		/// Handles an incoming zone update notification.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_ZONEUPDATE)]
		public static void HandleZoneUpdate(RealmClient client, RealmPacketIn packet)
		{
			ZoneId newZoneId = (ZoneId)packet.ReadUInt32();
			ZoneId oldZoneId = client.ActiveCharacter.CurrentZone.ID;

			if (oldZoneId != newZoneId)
			{
				Zone newZone = WorldMgr.GetZone(newZoneId);

				if (newZone != null)
				{
					client.ActiveCharacter.CurrentZone = newZone;

					Channel.UpdatePlayerChannels(client.ActiveCharacter, oldZoneId);
				}
			}
		}

		/// <summary>
		/// Handles an incoming time query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_QUERY_TIME)]
		public static void QueryTimeRequest(RealmClient client, RealmPacketIn packet)
		{
			SendQueryTimeReply(client);
		}

		/// <summary>
		/// Handles an incoming name query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_NAME_QUERY)]
		public static void NameQueryRequest(RealmClient client, RealmPacketIn packet)
		{
			CharacterRecord characterRecord = null;

			EntityId id = packet.ReadEntityId();
			Character chr = GetCharacter(id);

			if (chr != null)
			{
				characterRecord = chr.DBObject;
			}
			else
			{
				characterRecord = CharacterRecord.GetRecordByGUID(id);
			}

			if (characterRecord != null)
			{
				SendNameQueryReply(client, characterRecord);
			}
		}

		/// <summary>
		/// Handles an incoming creature name query.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_CREATURE_QUERY)]
		public static void CreatureNameQueryRequest(RealmClient client, RealmPacketIn packet)
		{
			uint creatureEntry = packet.ReadUInt32();
			EntityId creatureEntityId = packet.ReadEntityId();

			var npc = WorldMgr.GetObject(creatureEntityId) as NPC;
			if (npc != null)
			{
				var entry = npc.Entry;
				var name = entry.Name;
				var title = entry.Title;
				var info = entry.Info;
				using (RealmPacketOut pkt = new RealmPacketOut(RealmServerOpCode.SMSG_CREATURE_QUERY_RESPONSE, 
					5 + 4 + 1 + 36 + 2 + name.Length + title.Length))
				{
					pkt.WriteUInt(creatureEntry);
					pkt.Write(name);
					pkt.WriteByte(0);							// Name2
					pkt.WriteByte(0);							// Name3
					pkt.WriteByte(0);							// Name4
					pkt.WriteCString(title);
					//pkt.WriteCString(info);
					pkt.WriteUInt(entry.Flags);
					pkt.Write((uint)entry.Type);
					pkt.WriteUInt(entry.Family);
					pkt.Write((uint)entry.Rank);
					pkt.WriteUInt(entry.UInt1);
					pkt.WriteUInt(entry.SpellDataId);
					pkt.WriteUInt(npc.DisplayId);
					pkt.Write(entry.Float1);
					pkt.Write(entry.Float2);
					pkt.WriteByte(0);
					pkt.WriteByte(entry.IsLeader);


					client.Send(pkt);
				}
			}
		}

		/// <summary>
		/// Handles an incmming immediate logout request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_PLAYER_LOGOUT)]
		public static void PlayerImediateLogoutRequest(RealmClient client, RealmPacketIn packet)
		{
			SendPlayerImmediateLogoutReply(client);

			Character chr = client.ActiveCharacter;

			chr.FlagForLogout(false);

			bool noDelay = chr.IsGameMaster || chr.CurrentZone.IsCity;
			chr.PrepareToLogout(noDelay);
		}

		/// <summary>
		/// Handles an incoming logout request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_LOGOUT_REQUEST)]
		public static void PlayerLogoutRequest(RealmClient client, RealmPacketIn packet)
		{
			Character chr = client.ActiveCharacter;

			chr.FlagForLogout(false);

			bool noDelay = chr.IsGameMaster || chr.CurrentZone.IsCity;

			if (noDelay)
			{
				SendPlayerImmediateLogoutReply(client);
				chr.PrepareToLogout(noDelay);
			}
			else
			{
				SendLogoutReply(client, LogoutResponseCodes.LOGOUT_RESPONSE_ACCEPTED);
				chr.PrepareToLogout(noDelay);
			}
		}

		/// <summary>
		/// Handles an incoming logout cancel request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_LOGOUT_CANCEL)]
		public static void PlayerLogoutCancel(RealmClient client, RealmPacketIn packet)
		{
			SendLogoutCancelReply(client);
		}

		/// <summary>
		/// Handles an incoming stand state change request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param> 
		[PacketHandler(RealmServerOpCode.CMSG_STANDSTATECHANGE)]
		public static void PlayerChangeStandState(RealmClient client, RealmPacketIn packet)
		{
			byte standState = packet.ReadByte();

			client.ActiveCharacter.StandState = (StandState)standState;
		}
	}
}