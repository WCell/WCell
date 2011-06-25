/*************************************************************************
 *
 *   file		: RealmServer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-12 23:56:01 +0800 (Tue, 12 Aug 2008) $

 *   revision		: $Rev: 590 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Cell.Core;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Core;
using WCell.RealmServer.Content;
using WCell.RealmServer.Res;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using WCell.RealmServer.Privileges;
using WCell.Util;
using WCell.Core.Initialization;
using WCell.Util.Variables;
using WCell.RealmServer.Handlers;
using AuthenticationClient = WCell.RealmServer.Events.AuthenticationClient;

namespace WCell.RealmServer
{
	/// <summary>
	/// Server class for the realm server. Handles all initial
	/// connections and verifies authentication with the 
	/// authentication server 
	/// </summary>
	[VariableClassAttribute(true)]
	public sealed partial class RealmServer : ServerApp<Events.RealmServer>
	{
		static DateTime timeStart;
		private static long timeStartTicks;

		[Variable(IsReadOnly = true)]
		public static DateTime IngameTime
		{
			get
			{
				return timeStart.AddMinutes(((DateTime.Now.Ticks - timeStartTicks) * RealmServerConfiguration.IngameMinutesPerSecond)
					/ TimeSpan.TicksPerSecond);
			}
			set
			{
				timeStart = value;
				timeStartTicks = DateTime.Now.Ticks;

				if (Instance.IsRunning)
				{
					foreach (var chr in World.GetAllCharacters())
					{
						var client = chr.Client;
						if (client == null)
						{
							return;
						}
						CharacterHandler.SendTimeSpeed(client);
					}
				}
			}
		}

		private readonly RealmServerConfiguration m_configuration;

		private readonly AuthenticationClient m_authServiceClient = new AuthenticationClient();
		public readonly Dictionary<string, RealmAccount> LoggedInAccounts = new Dictionary<string, RealmAccount>(StringComparer.InvariantCultureIgnoreCase);

		private volatile int m_acceptedClients;
		private readonly byte[] m_authSeed = BitConverter.GetBytes(new Random().Next());

		/// <summary>
		/// Default constructor
		/// </summary>
		public RealmServer()
		{
			// this needs to be the entry assembly (the console executable) so that we
			// load our console app configs.
			m_configuration = new RealmServerConfiguration(EntryLocation);

			m_acceptedClients = 0;
		}

		#region Properties
		/// <summary>
		/// The configuration for the realm server.
		/// </summary>
		public RealmServerConfiguration Configuration
		{
			get { return m_configuration; }
		}

		/// <summary>
		/// The authentication service client instance.
		/// </summary>
		public AuthenticationClient AuthClient
		{
			get { return m_authServiceClient; }
		}

		/// <summary>
		/// Number of clients fully accepted and authenticated.
		/// </summary>
		public int AcceptedClients
		{
			get { return m_acceptedClients; }
		}

		/// <summary>
		/// The randomly-generated seed used for pre-login authentication.
		/// </summary>
		public byte[] AuthSeed
		{
			get { return m_authSeed; }
		}

		public override string Host
		{
			get { return RealmServerConfiguration.Host; }
		}

		public override int Port
		{
			get { return RealmServerConfiguration.Port; }
		}

		/// <summary>
		/// Can only be used if RealmServerConfiguration.RegisterExternalAddress is true
		/// or if already connected, else will throw Exception.
		/// </summary>
		public string ExternalAddress
		{
			get
			{
				string addr;
				if (RealmServerConfiguration.RegisterExternalAddress)
				{
					addr = RealmServerConfiguration.ExternalAddress;
				}
				else
				{
					addr = null;
				}
				return addr;
			}
		}
		#endregion

		#region Start
		/// <summary>
		/// Starts the server and begins accepting connections.
		/// Requires IO-Context.
		/// Also see <c>StartLater</c>
		/// </summary>
		public override void Start()
		{
			base.Start();

			if (_running)
			{
				ConnectToAuthService();
			}
		}

		[Initialization(InitializationPass.Last)]
		public static void FinishSetup()
		{
			timeStart = DateTime.Now;
			timeStartTicks = timeStart.Ticks;
		}

		internal static void ResetTimeStart()
		{
			timeStart = IngameTime;
			timeStartTicks = DateTime.Now.Ticks;
		}
		#endregion

		/// <summary>
		/// Establishes the initial connection with the authentication service.
		/// </summary>
		private void ConnectToAuthService()
		{
			m_authServiceClient.StartConnect(RealmServerConfiguration.AuthenticationServerAddress);
		}

		internal void OnStatusChange(RealmStatus oldStatus)
		{
			Instance.UpdateRealm();
			var evt = StatusChanged;
			if (evt != null)
			{
				evt(oldStatus);
			}
		}

		#region IPC
		/// <summary>
		/// Registers this Realm with the Authentication-Server
		/// </summary>
		public void RegisterRealm()
		{
			if (!m_authServiceClient.IsConnected || !IsRunning)
			{
				//s_log.Error(Resources.RegisterNotRunning);
			}
			else
			{
				m_authServiceClient.Channel.RegisterRealmServer(
					RealmServerConfiguration.RealmName,
					ExternalAddress,
					RealmServerConfiguration.Port,
					World.CharacterCount,
					RealmServerConfiguration.MaxClientCount,
					RealmServerConfiguration.ServerType,
					RealmServerConfiguration.Flags,
					RealmServerConfiguration.Category,
					RealmServerConfiguration.Status,
					WCellInfo.RequiredVersion);

				if (m_authServiceClient.IsConnected)
				{
					// set all active accounts (or just clear accounts if re-starting)
					m_authServiceClient.Channel.SetAllActiveAccounts(LoggedInAccounts.Keys.ToArray());
				}

				//m_authServiceClient.IsRegisteredAtAuthServer = true;
				PrivilegeMgr.Instance.Setup();
			}
		}

		/// <summary>
		/// Updates this Realm at the Authentication-Server.
		/// Is called automatically on a regular basis.
		/// </summary>
		public bool UpdateRealm()
		{
			if (!m_authServiceClient.IsConnected || !IsRunning)
			{
				return false;
			}

			if (!m_authServiceClient.Channel.UpdateRealmServer(RealmServerConfiguration.RealmName,
														  World.CharacterCount,
														  RealmServerConfiguration.MaxClientCount,
														  RealmServerConfiguration.ServerType,
														  RealmServerConfiguration.Flags,
														  RealmServerConfiguration.Category,
														  RealmServerConfiguration.Status))
			{
				RegisterRealm();
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		internal void UnregisterRealm()
		{
			log.Info(Resources.IPCProxyDisconnected);
		}
		#endregion

		#region ServerBase Implementation
		/// <summary>
		/// Called when a UDP packet is received
		/// </summary>
		/// <param name="num_bytes">the number of bytes received</param>
		/// <param name="buf">byte[] of the datagram</param>
		/// <param name="ip">the source IP of the datagram</param>
		protected override void OnReceiveUDP(int num_bytes, byte[] buf, IPEndPoint ip)
		{
		}

		/// <summary>
		/// Called when a UDP packet is sent
		/// </summary>
		/// <param name="clientIP">the destination IP of the datagram</param>
		/// <param name="num_bytes">the number of bytes sent</param>
		protected override void OnSendTo(IPEndPoint clientIP, int num_bytes)
		{
		}

		/// <summary>
		/// Creates a client object for a newly connected client
		/// </summary>
		/// <returns>a new IRealmClient object</returns>
		protected override IClient CreateClient()
		{
			return new RealmClient(this);
		}

		/// <summary>
		/// Called when a client connects.
		/// A client cannot connect while the Realm is not connected
		/// to the AuthServer.
		/// </summary>
		/// <param name="client">the client object</param>
		/// <returns>false to shutdown the server</returns>
		protected override bool OnClientConnected(IClient client)
		{
			if (AuthClient.IsConnected)
			{
				base.OnClientConnected(client);
				LoginHandler.SendAuthChallenge((IRealmClient)client);
				return true;
			}
			return false;
		}

		// TODO: (Domi) Override DisconnectClient(IClientBase client, bool forced) to not disconnect instantly but add logout delay
		// Will be called if client disconnects without saying goodbye.

		/// <summary>
		/// Called when a client disconnects
		/// </summary>
		/// <param name="client">the client object</param>
		/// <param name="forced">indicates if the client disconnection was forced</param>
		protected override void OnClientDisconnected(IClient client, bool forced)
		{
			var realmClient = client as IRealmClient;

			if (realmClient != null)
			{
				if (!realmClient.IsOffline)
				{
					realmClient.IsOffline = true;			// so we don't try and send anymore packets

					LoginHandler.NotifyLogout(realmClient);
					var acc = realmClient.Account;
					if (acc != null)
					{
						SetAccountLoggedIn(acc, false);
						acc.Client = null;

						var chr = realmClient.ActiveCharacter;
						if (chr != null)
						{
							if (!chr.IsPinnedDown)
							{
								chr.Logout(false);
							}
						}
						else
						{
							realmClient.Account = null;
						}
					}
				}
			}

			m_acceptedClients--;

			base.OnClientDisconnected(client, forced);
		}

		/// <summary>
		/// Called when a login is accepted.
		/// </summary>
		/// <param name="sender">the caller of the event</param>
		/// <param name="args">the arguments of the event</param>
		internal void OnClientAccepted(object sender, EventArgs args)
		{
			m_acceptedClients++;
		}
		#endregion

		#region Accounts
		/// <summary>
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsAccountLoggedIn(string name)
		{
			return LoggedInAccounts.ContainsKey(name);
		}

		/// <summary>
		/// Returns the logged in account with the given name.
		/// Requires IO-Context.
		/// </summary>
		public RealmAccount GetLoggedInAccount(string name)
		{
			RealmAccount acc;
			LoggedInAccounts.TryGetValue(name, out acc);
			return acc;
		}

		/// <summary>
		/// Registers the given Account as currently connected.
		/// Requires IO-Context.
		/// </summary>
		internal void RegisterAccount(RealmAccount acc)
		{
			LoggedInAccounts.Add(acc.Name, acc);
			SetAccountLoggedIn(acc, true);
		}

		/// <summary>
		/// Removes the given Account from the list from currently connected Accounts.
		/// Requires IO-Context.
		/// </summary>
		/// <returns>Whether the Account was even flagged as logged in.</returns>
		internal void UnregisterAccount(RealmAccount acc)
		{
			acc.ActiveCharacter = null;
			if (LoggedInAccounts.ContainsKey(acc.Name))
			{
				LoggedInAccounts.Remove(acc.Name);
			}
			else
			{
				s_log.Warn("Tried to unregister non-registered account: " + acc);
			}
		}

		/// <summary>
		/// Updates the AuthServer about the login-status of the account with the given name.
		/// Accounts that are flagged as logged-in cannot connect again until its unset again.
		/// Called whenever client connects/disconnects.
		/// </summary>
		/// <param name="acc"></param>
		/// <param name="loggedIn"></param>
		internal void SetAccountLoggedIn(RealmAccount acc, bool loggedIn)
		{
			if (m_authServiceClient.IsConnected)
			{
				if (loggedIn)
				{
					acc.OnLogin();
					m_authServiceClient.Channel.SetAccountLoggedIn(acc.Name);
				}
				else
				{
					acc.OnLogout();
					IOQueue.AddMessage(new Message1<RealmAccount>(acc, UnregisterAccount));
					m_authServiceClient.Channel.SetAccountLoggedOut(acc.Name);
				}
			}
		}

		/// <summary>
		/// Gets the AccountInfo for the given name from the corresponding logged in account or requests it from the AuthServer
        /// 
        /// TODO: search by byte[] value
		/// </summary>
		/// <param name="requestAddr">If not null, will only return the Account if not banned.</param>
		/// <remarks>Requires IO-Context.</remarks>
		/// <returns>The information of the requested account or null if the Account did not exist or the AuthServer could not be reached</returns>
		public IAccountInfo RequestAccountInfo(string accountName, byte[] requestAddr)
		{
			RealmAccount acc;
			if (!LoggedInAccounts.TryGetValue(accountName, out acc))
			{
				if (m_authServiceClient.IsConnected)
				{
					return m_authServiceClient.Channel.RequestAccountInfo(accountName, requestAddr);
				}
			}
			else
			{
				if (!acc.IsActive)
				{
					return null;
				}
			}
			return acc;
		}

		/// <summary>
		/// Gets all information of the account with the given Name.
		/// </summary>
		/// <remarks>Requires IO-Context.</remarks>
		/// <returns>The information of the requested account or null if the Account 
		/// did not exist or the AuthServer could not be reached</returns>
		public RealmAccount GetOrRequestAccount(string accountName)
		{
			RealmAccount acc;
			if (!LoggedInAccounts.TryGetValue(accountName, out acc))
			{
				if (m_authServiceClient.IsConnected)
				{
					var info = m_authServiceClient.Channel.RequestAccountInfo(accountName, null);
					if (info != null)
					{
						acc = new RealmAccount(accountName, info);
					}
				}
			}
			return acc;
		}
		#endregion

		/// <summary>
		/// Requests the temporary authentication information of an account.
		/// </summary>
		/// <param name="accountName">the account name</param>
		/// <returns>The request information or null if it did not succeed</returns>
		public AuthenticationInfo GetAuthenticationInfo(string accountName)
		{
			if (m_authServiceClient.IsConnected)
			{
				return m_authServiceClient.Channel.GetAuthenticationInfo(accountName);
			}
			return null;
		}

		#region Send methods



		#endregion

		#region Shutdown
		public override void ShutdownIn(uint delayMillis)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SERVER_MESSAGE))
			{
				packet.Write((uint)ServerMessagesType.ServerShutdownStart);
				packet.Write(TimeSpan.FromMilliseconds(delayMillis).Format());
				World.Broadcast(packet);
			}
			base.ShutdownIn(delayMillis);
		}

		public override void CancelShutdown()
		{
			if (IsPreparingShutdown)
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SERVER_MESSAGE))
				{
					packet.Write((uint)ServerMessagesType.ServerShutdownCancelled);
					packet.Write(string.Empty);
					World.Broadcast(packet);
				}
			}
			base.CancelShutdown();
		}

		public override void Stop()
		{
			if (m_authServiceClient != null)
			{
				m_authServiceClient.IsRunning = false;
			}

			base.Stop();
		}

		/// <summary>
		/// 
		/// </summary>
		protected override void OnShutdown()
		{
			World.Broadcast("Initiating Shutdown...");
			World.Broadcast("Halting server and saving World...");

			World.Save(true);

			if (RealmServerConfiguration.Instance.AutoSave)
			{
				RealmServerConfiguration.Instance.Save(true, true);
			}

			World.Broadcast("World saved.");

			if (m_authServiceClient != null && m_authServiceClient.IsConnected)
			{
				// unset all accounts
				IOQueue.AddMessageAndWait(true, () => m_authServiceClient.Channel.SetAllActiveAccounts(EmptyStringArr));
				Thread.Sleep(100);		// sleep for a short while to let the client send the msg to the AuthServer
				m_authServiceClient.IsRunning = false;
			}
			World.Broadcast("Shutting down...");
		}
		#endregion


		public static string Title
		{
			get
			{
				return string.Format("{0} - WCell {1}", RealmServerConfiguration.RealmName, WCellInfo.Codename);
			}
		}


		public static string FormattedTitle
		{
			get { return ChatUtility.Colorize(Title, Color.Purple); }
		}

		public override string ToString()
		{
			return string.Format("{2} - WCell {0} (v{1})", GetType().Name,
				AssemblyVersion, RealmServerConfiguration.RealmName);
		}
	}
}