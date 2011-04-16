/*************************************************************************
 *
 *   file		: Authentication.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
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
using WCell.AuthServer.Localization;
using WCell.AuthServer.Network;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Database;
using WCell.Core.Network;
using WCell.Intercommunication.DataTypes;

namespace WCell.AuthServer
{
	/// <summary>
	/// Class that handles all authentication of the client.
	/// </summary>
	public static class Authentication
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
		public static int LoginFailedDelay = 2000;

		private static readonly Dictionary<IPAddress, TimeoutWaitHandle> failedLogins = new Dictionary<IPAddress, TimeoutWaitHandle>();

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
                packet.Write((byte) AccountStatus.Success);
                // Grunt command
                packet.Write((byte) 0x00);

                client.Authenticator.WriteServerChallenge(packet);

                var rand = new BigInteger(new Random(Environment.TickCount), 128);

                packet.WriteBigInt(rand, 16);

                byte securityFlag = 0x0;

                packet.Write(securityFlag);

                // Require PIN input
                if ((securityFlag & 0x1) == 0x1)
                {
                    packet.WriteInt(0);
                    packet.Write(new byte[16]);
                }

                if ((securityFlag & 0x2) == 0x2)
                {
                    packet.Write((byte) 0);
                    packet.Write((byte) 0);
                    packet.Write((byte) 0);
                    packet.Write((byte) 0);
                    packet.WriteULong(0);
                }
                // Require Security Token input
                if ((securityFlag & 0x4) == 0x4)
                {
                    packet.Write((byte) 1);
                }

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
			    packet.Write((byte) error);
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
			    packet.Write((byte) AccountStatus.Success);
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

			client.ClientInfo = ClientInformation.ReadFromPacket(packet);

			if (!WCellInfo.RequiredVersion.IsSupported(client.ClientInfo.Version))
			{
				OnLoginError(client, AccountStatus.WrongBuild);
			}
			else
			{
				string accName = packet.ReadPascalString();

				s_log.Debug(Resources.AccountChallenge, accName);

				client.CurrentUser = accName;

				var banQuery = QueryFactory.CreateResultQuery
					(
				    () => Ban.GetBan(client.ClientAddress),
					AuthChallengeRequestCallback,
					client
					);

				client.Server.EnqueueTask(banQuery);
			}
		}

		private static void AuthChallengeRequestCallback(IAuthClient client, Ban clientBan)
		{
			if (clientBan != null)
			{
				if (clientBan.IPMinimum != 0 && clientBan.IPMaximum != 0)
				{
					if (clientBan.Expires <= DateTime.Now)
					{
						// remove the ban since it's expired
					    client.Server.EnqueueTask(QueryFactory.CreateNonResultQuery(clientBan.DeleteAndFlush));
					}
					else
					{
						OnLoginError(client, AccountStatus.Failure);
						return;
					}
				}
			}

			if (client.Server.IsAccountLoggedIn(client.CurrentUser))
			{
				OnLoginError(client, AccountStatus.AccountInUse);
			}
			else
			{
				var acctQuery = QueryFactory.CreateResultQuery(
				    () => AccountMgr.GetAccount(client.CurrentUser),
												QueryAccountCallback,
												client
											);
				client.Server.EnqueueTask(acctQuery);
			}
		}

		private static void QueryAccountCallback(IAuthClient client, Account acct)
		{
			string accName = client.CurrentUser;

			if (acct == null)
			{
				if (AuthServerConfiguration.AutocreateAccounts)
				{
					// Fill in this client's info with the username they gave.
					// We have to go through the authentication phase, and if 
					// the password ends up matching the username, we know it's
					// an autocreation attempt.

					client.IsAutocreated = true;

					var passHash = new BigInteger(SecureRemotePassword.GenerateCredentialsHash(accName, accName));

					client.Authenticator = new Authenticator(new SecureRemotePassword(accName, passHash, true));
					SendAuthChallengeSuccessReply(client);
				}
				else
				{
					s_log.Debug(Resources.AccountNotFound, accName);

					OnLoginError(client, AccountStatus.InvalidInformation);
				}
			}
			else
			{
				// check if Account may be used
				if (acct.Status != AccountStatus.Success)
				{
					SendAuthChallengeErrorReply(client, acct.Status);
				}
				else
				{
					client.Authenticator = new Authenticator(new SecureRemotePassword(accName, acct.Password, true));
					acct.OnLogin(client);
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
			else
			{
				if (client.Authenticator.IsClientProofValid(packet))
				{
					if (client.IsAutocreated)
					{
						// Their stuff matched, which means they gave us the same password
						// as their username, which is what must occur to autocreate. Create
						// the account for them before proceeding.

						s_log.Debug(Resources.AutocreatingAccount, client.CurrentUser);

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

						var acctCreateQuery = QueryFactory.CreateResultQuery(
							() => AccountMgr.Instance.CreateAccount(
							          client.CurrentUser,
							          client.Authenticator.SRP.Credentials.GetBytes(20),
							          null,
							          role,
							          ClientId.Wotlk
							          ),
							AutocreateAccountCallback,
							client
							);

						client.Server.EnqueueTask(acctCreateQuery);
					}
					else
					{
						// The following was sent twice
						var authInfo = new AuthenticationInfo {
							SessionKey = client.Authenticator.SRP.SessionKey.GetBytes(40),
							Salt = client.Authenticator.SRP.Salt.GetBytes(32),
							Verifier = client.Authenticator.SRP.Verifier.GetBytes(),
							SystemInformation = ClientInformation.Serialize(client.ClientInfo)
						};

						client.Server.StoreAuthenticationInfo(client.CurrentUser, authInfo);

						SendAuthProofSuccessReply(client);
					}
				}
				else
				{
					s_log.Debug(Resources.InvalidClientProof, client.CurrentUser);

					OnLoginError(client, AccountStatus.InvalidInformation);
				}
			}
		}

		private static void AutocreateAccountCallback(IAuthClient client, Account acct)
		{
			if (acct == null)
			{
				OnLoginError(client, AccountStatus.InvalidInformation);

				return;
			}

			var authInfo = new AuthenticationInfo {
				SessionKey = client.Authenticator.SRP.SessionKey.GetBytes(40),
				Salt = client.Authenticator.SRP.Salt.GetBytes(32),
				Verifier = client.Authenticator.SRP.Verifier.GetBytes(),
				SystemInformation = ClientInformation.Serialize(client.ClientInfo)
			};

			client.Server.StoreAuthenticationInfo(client.CurrentUser, authInfo);

			SendAuthProofSuccessReply(client);
		}


		[ClientPacketHandler(AuthServerOpCode.AUTH_RECONNECT_CHALLENGE)]
		public static void AuthReconnectChallenge(IAuthClient client, AuthPacketIn packet)
		{
			SendAuthReconnectChallenge(client);
		}

		[ClientPacketHandler(AuthServerOpCode.AUTH_RECONNECT_PROOF)]
		public static void AuthReconnectProof(IAuthClient client, AuthPacketIn packet)
		{
			AuthenticationInfo authInfo;
			if (AuthenticationServer.Instance.GetAuthenticationInfo(client.CurrentUser, out authInfo))
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
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_RECONNECT_CHALLENGE))
			{
				client.Authenticator.WriteReconnectChallenge(packet);
			}
		}

		public static void SendAuthReconnectProof(IAuthClient client)
		{
			using (var packet = new AuthPacketOut(AuthServerOpCode.AUTH_RECONNECT_PROOF))
			{
				packet.Write((byte)0);// error
				packet.Write((short)0);
			}
		}

		/// <summary>
		/// Called when the given client failed to login due to the given reason.
		/// Delays, fires the LoginFailed event and sends the reply.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="error"></param>
		private static void OnLoginError(IAuthClient client, AccountStatus error)
		{
			TimeoutWaitHandle delayHandle;
			if (!failedLogins.TryGetValue(client.ClientAddress, out delayHandle))
			{
				failedLogins.Add(client.ClientAddress, delayHandle = new TimeoutWaitHandle(DateTime.Now));
			}
			else
			{
				delayHandle.LastAttempt = DateTime.Now;
			}

			ThreadPool.RegisterWaitForSingleObject(delayHandle.Handle, (state, timedOut) => {
				if (client.IsConnected)
				{
					var evt = LoginFailed;
					if (evt != null)
					{
						evt(client, error);
					}
					SendAuthProofErrorReply(client, error);
				}
			}, null, LoginFailedDelay, true);
		}
	}
}