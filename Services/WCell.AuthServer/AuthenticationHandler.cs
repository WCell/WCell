/*************************************************************************
 *
 *   file		: Authentication.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 12:30:51 +0800 (Mon, 16 Feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NLog;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Database;
using WCell.AuthServer.Network;
using resources = WCell.AuthServer.Res.WCell_AuthServer;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Database;
using WCell.Core.Network;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Threading;
using WCell.AuthServer.Firewall;

namespace WCell.AuthServer
{
	/// <summary>
	/// Class that handles all authentication of the client.
	/// </summary>
	public static class AuthenticationHandler
	{
		public delegate void LoginFailedHandler(IAuthClient client, AccountStatus error);

		/// <summary>
		/// Is called whenever a client failed to authenticate itself
		/// </summary>
		public static event LoginFailedHandler LoginFailed;

		private static Logger s_log = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// The time in milliseconds to let a client wait when failing to login
		/// </summary>
		public static int FailedLoginDelay = 200;

		private static readonly Dictionary<IPAddress, LoginFailInfo> failedLogins = new Dictionary<IPAddress, LoginFailInfo>();

		#region Send methods

		/// <summary>
		/// Sends an authentication challenge error to the client.
		/// </summary>
		/// <param name="client">the client</param>
		/// <param name="error">the authentication challenge error to send</param>
		public static void SendAuthChallengeErrorReply(IAuthClient client, AccountStatus error)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_LOGON_CHALLENGE))
			{
				packet.Write((byte)0x00);
				packet.Write((byte)error);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends an authentication challenge success reply to the client.
		/// </summary>
		/// <param name="client">the client</param>
		public static void SendAuthChallengeSuccessReply(IAuthClient client)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_LOGON_CHALLENGE))
			{
				packet.Write((byte)AccountStatus.Success);
				// Grunt command
				packet.Write((byte)0x00);

				client.Authenticator.WriteServerChallenge(packet);

				//var rand = new BigInteger(new Random(Environment.TickCount), 128);
				//packet.WriteBigInt(rand, 16);
				Random rand = new Random(Environment.TickCount);
				byte[] randbytes = new byte[16];
				rand.NextBytes(randbytes);
				packet.Write(randbytes);

				const byte securityFlag = 0x0;
				packet.Write(securityFlag);

				// Require PIN input
				//if ((securityFlag & 0x1) == 0x1)
				//{
				//    packet.WriteInt(0);
				//    packet.Write(new byte[16]);
				//}

				// Matrix input
				//if ((securityFlag & 0x2) == 0x2)
				//{
				//    packet.Write((byte)0);
				//    packet.Write((byte)0);
				//    packet.Write((byte)0);
				//    packet.Write((byte)0);
				//    packet.Write(0UL);
				//}
				// Require Security Token input
				//if ((securityFlag & 0x4) == 0x4)
				//{
				//    packet.Write((byte)1);
				//}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends an authentication proof error to the client.
		/// </summary>
		/// <param name="client">the client</param>
		/// <param name="error">the authentication proof to send</param>
		public static void SendAuthProofErrorReply(IAuthClient client, AccountStatus error)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_LOGON_PROOF))
			{
				packet.Write((byte)error);
				packet.Write((byte)0x03);
				packet.Write((byte)0x00);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends an authentication proof success reply to the client.
		/// </summary>
		/// <param name="client">the client</param>
		public static void SendAuthProofSuccessReply(IAuthClient client)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_LOGON_PROOF))
			{
				packet.Write((byte)AccountStatus.Success);
				client.Authenticator.WriteServerProof(packet);

				packet.WriteInt(0);
				packet.WriteInt(0);
				packet.WriteShort(0);

				client.Send(packet);
			}
		}

		#endregion

		#region Packet Handlers

		/// <summary>
		/// Handles an incoming authentication challenge.
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(AuthServerOpCode.AUTH_LOGON_CHALLENGE)]
		public static void AuthChallengeRequest(IAuthClient client, AuthPacketIn packet)
		{
			packet.SkipBytes(6); // Skip game name and packet size

			client.Info = ClientInformation.ReadFromPacket(packet);

			// Account-names are always sent upper-case by the client (make sure, the tradition is kept alive)
			var accName = packet.ReadPascalString().ToUpper();

			s_log.Debug(resources.AccountChallenge, accName);

			client.AccountName = accName;
			AuthenticationServer.Instance.AddMessage(new Message1<IAuthClient>(client, AuthChallengeRequestCallback));
		}

		/// <summary>
		/// Check for bans and already logged in Accounts, else continue the journey
		/// </summary>
		/// <param name="client"></param>
		private static void AuthChallengeRequestCallback(IAuthClient client)
		{
			if (!client.IsConnected)
			{
				// Client disconnected in the meantime
				return;
			}

			if (BanMgr.IsBanned(client.ClientAddress))
			{
				OnLoginError(client, AccountStatus.AccountBanned);
			}
			//else if (client.Server.IsAccountLoggedIn(client.AccountName))
			//{
			//    OnLoginError(client, AccountStatus.AccountInUse);
			//}
			else
			{
				var acctQuery = new Action(() =>
				{
					var acc = AccountMgr.GetAccount(client.AccountName);
					QueryAccountCallback(client, acc);
				});

				AuthenticationServer.Instance.AddMessage(acctQuery);
			}
		}

		private static void QueryAccountCallback(IAuthClient client, Account acct)
		{
			if (!client.IsConnected)
			{
				return;
			}

			var accName = client.AccountName;

			if (acct == null)
			{
				// Account doesn't exist yet -> Check for auto creation
				if (AuthServerConfiguration.AutocreateAccounts)
				{
					if (!AccountMgr.NameValidator(ref accName))
					{
						OnLoginError(client, AccountStatus.InvalidInformation);
						return;
					}

					// Fill in this client's info with the username they gave.
					// We have to go through the authentication phase, and if 
					// the password ends up matching the username, we know it's
					// an autocreation attempt.

					var passHash = SecureRemotePassword.GenerateCredentialsHash(accName, accName);

					client.Authenticator = new Authenticator(new SecureRemotePassword(accName, passHash, true));
					SendAuthChallengeSuccessReply(client);
				}
				else
				{
					OnLoginError(client, AccountStatus.InvalidInformation);
				}
			}
			else
			{
				// check if Account may be used
				if (acct.CheckActive())
				{
					client.Account = acct;
					client.Authenticator = new Authenticator(new SecureRemotePassword(accName, acct.Password, true));
					SendAuthChallengeSuccessReply(client);
				}
				else
				{
					// Account has been deactivated
					if (client.Account.StatusUntil == null)
					{
						// temporarily suspended
						OnLoginError(client, AccountStatus.AccountBanned);
					}
					else
					{
						// deactivated
						OnLoginError(client, AccountStatus.AccountFrozen);
					}
				}
			}
		}

		/// <summary>
		/// Handles an incoming authentication proof.
		/// </summary>
		/// <param name="client">the client the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(AuthServerOpCode.AUTH_LOGON_PROOF)]
		public static void AuthProofRequest(IAuthClient client, AuthPacketIn packet)
		{
			if (client.Authenticator == null)
			{
				client.Server.DisconnectClient(client);
			}
			else if (client.IsConnected)
			{
				if (client.Authenticator.IsClientProofValid(packet))
				{
					AuthenticationServer.Instance.AddMessage(() =>
					{
						LoginClient(client);
					});
				}
				else
				{
					s_log.Debug(resources.InvalidClientProof, client.AccountName);

					OnLoginError(client, AccountStatus.InvalidInformation);
				}
			}
		}

		/// <summary>
		/// Client passed login challenge and can be logged in
		/// </summary>
		/// <param name="client"></param>
		private static void LoginClient(IAuthClient client)
		{
			var acc = client.Account;

			if (acc == null)
			{
				// Pass and username are identical so an Account can be auto-created
				// the corresponding check happened before
				s_log.Debug(resources.AutocreatingAccount, client.AccountName);

				if (AccountMgr.DoesAccountExist(client.AccountName))
				{
					// account was already created								
					SendAuthProofErrorReply(client, AccountStatus.Failure);
					return;
				}
				client.Account = acc = AutoCreateAccount(client);
			}

			var authInfo = new AuthenticationInfo
			{
				SessionKey = client.Authenticator.SRP.SessionKey.GetBytes(40),
				Salt = client.Authenticator.SRP.Salt.GetBytes(32),
				Verifier = client.Authenticator.SRP.Verifier.GetBytes(),
				SystemInformation = ClientInformation.Serialize(client.Info)
			};
			client.Server.StoreAuthenticationInfo(client.AccountName, authInfo);

			acc.OnLogin(client);
			SendAuthProofSuccessReply(client);
		}

		/// <summary>
		/// Used to create accounts when using Auto-account creation
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		private static Account AutoCreateAccount(IAuthClient client)
		{
			string role;
			if (IPAddress.IsLoopback(client.ClientAddress))
			{
				// local users get the highest role
				role = RoleGroupInfo.HighestRole.Name;
			}
			else
			{
				// remote users get default role
				role = AuthServerConfiguration.DefaultRole;
			}

			return AccountMgr.Instance.CreateAccount(
				client.AccountName,
				client.Authenticator.SRP.Credentials.GetBytes(20),
				null,
				role,
				ClientId.Wotlk
				);
		}


		[ClientPacketHandler(AuthServerOpCode.AUTH_RECONNECT_CHALLENGE)]
		public static void AuthReconnectChallenge(IAuthClient client, AuthPacketIn packet)
		{
			SendAuthReconnectChallenge(client);
		}

		[ClientPacketHandler(AuthServerOpCode.AUTH_RECONNECT_PROOF)]
		public static void AuthReconnectProof(IAuthClient client, AuthPacketIn packet)
		{
			var authInfo = AuthenticationServer.Instance.GetAuthenticationInfo(client.AccountName);
			if (authInfo != null)
			{
				if (client.Authenticator.IsReconnectProofValid(packet, authInfo))
				{
					SendAuthReconnectProof(client);
				}
			}

		}

		#endregion

		public static void SendAuthReconnectChallenge(IAuthClient client)
		{
			//using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_RECONNECT_CHALLENGE))
			//{
			//    //client.Authenticator.WriteReconnectChallenge(packet);
			//    client.Send(packet);
			//}

			// drop silently for now
			OnLoginError(client, AccountStatus.Failure, true);
		}

		public static void SendAuthReconnectProof(IAuthClient client)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_RECONNECT_PROOF))
			{
				packet.Write((byte)0);// error
				packet.Write((short)0);
				client.Send(packet);
			}
		}

		/// <summary>
		/// Called when the given client failed to login due to the given reason.
		/// Delays, fires the LoginFailed event and sends the reply.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="error"></param>
		public static void OnLoginError(IAuthClient client, AccountStatus error)
		{
			OnLoginError(client, error, false);
		}

		/// <summary>
		/// Called when the given client failed to login due to the given reason.
		/// Delays, fires the LoginFailed event and sends the reply.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="error"></param>
		public static void OnLoginError(IAuthClient client, AccountStatus error, bool silent)
		{
			if (!silent)
			{
				s_log.Debug("Client {0} failed to login: {1}", client, error);
			}

			LoginFailInfo failInfo;
			if (!failedLogins.TryGetValue(client.ClientAddress, out failInfo))
			{
				failedLogins.Add(client.ClientAddress, failInfo = new LoginFailInfo(DateTime.Now));
			}
			else
			{
				failInfo.LastAttempt = DateTime.Now;
				failInfo.Count++;
				// TODO: Ban, if trying too often?
			}

			// delay the reply
			ThreadPool.RegisterWaitForSingleObject(failInfo.Handle, (state, timedOut) =>
			{
				if (client.IsConnected)
				{
					var evt = LoginFailed;
					if (evt != null)
					{
						evt(client, error);
					}

					SendAuthProofErrorReply(client, error);
				}
			}, null, FailedLoginDelay, true);
		}
	}
}