/*************************************************************************
 *
 *   file		: AuthServer.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 17:02:58 +0800 (Tue, 08 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
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
using System.ServiceModel;
using Cell.Core;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.IPC;
using WCell.AuthServer.Lang;
using WCell.AuthServer.Network;
using WCell.Constants.Login;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.Collections;
using WCell.Util.Variables;
using resources = WCell.AuthServer.Res.WCell_AuthServer;

namespace WCell.AuthServer
{
	/// <summary>
	/// Server class for the authentication server. Handles all initial 
	/// incoming connections and does authentication of users.
	/// </summary>
	[VariableClass(true)]
	public sealed class AuthenticationServer : ServerApp<AuthenticationServer>
	{
		#region Events
		/// <summary>
		/// Is called everytime a new Realm connects
		/// </summary>
		public static event Action<RealmEntry> RealmConnected;

		/// <summary>
		/// Is called everytime a connected Realm disconnects
		/// </summary>
		public static event Action<RealmEntry> RealmDisconnected;

		/// <summary>
		/// Is called everytime an Account logs into a Realm.
		/// Parameters are: Account name and the Realm that it connected to.
		/// </summary>
		public static event Action<string, RealmEntry> AccountLoggedIn;

		/// <summary>
		/// Is called everytime an Account logs out.
		/// Parameters are: Account name and the Realm that it was connected to.
		/// </summary>
		public static event Action<string, RealmEntry> AccountLoggedOut;
		#endregion

		internal static readonly ImmutableDictionary<string, RealmEntry> m_realmsById = new ImmutableDictionary<string, RealmEntry>();
		private readonly SynchronizedDictionary<string, AuthenticationRecord> m_AuthRecords =
			new SynchronizedDictionary<string, AuthenticationRecord>(StringComparer.InvariantCultureIgnoreCase);
		private readonly SynchronizedDictionary<string, RealmEntry> m_loggedInAccounts =
			new SynchronizedDictionary<string, RealmEntry>(StringComparer.InvariantCultureIgnoreCase);
		private readonly AuthServerConfiguration m_configuration;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public AuthenticationServer()
		{
			m_configuration = new AuthServerConfiguration(EntryLocation);
		}

		#region Properties
		/// <summary>
		/// The configuration for the authentication server.
		/// </summary>
		public AuthServerConfiguration Configuration
		{
			get { return m_configuration; }
		}

		/// <summary>
		/// Collection of the realms, indexed by their unique ID.
		/// </summary>
		public static IEnumerable<RealmEntry> Realms
		{
			get { return m_realmsById.Values; }
		}

		/// <summary>
		/// Number of available realms.
		/// </summary>
		public static int RealmCount
		{
			get { return m_realmsById.Values.Count(realm => realm.IsOnline); }
		}

		/// <summary>
		/// Collection of logged in accounts, indexed by name.
		/// </summary>
		public SynchronizedDictionary<string, RealmEntry> LoggedInAccounts
		{
			get { return m_loggedInAccounts; }
		}

		public override string Host
		{
			get { return AuthServerConfiguration.Host; }
		}

		public override int Port
		{
			get { return AuthServerConfiguration.Port; }
		}
		#endregion

		#region AuthenticationInfo
		/// <summary>
		/// Stores the SRP information for an account;
		/// </summary>
		/// <param name="accName">the account name</param>
		/// <param name="authInfo">the SRP object</param>
		public void StoreAuthenticationInfo(string accName, AuthenticationInfo authInfo)
		{
			var record = new AuthenticationRecord(accName, authInfo);
			AuthenticationRecord oldRecord;
			if (m_AuthRecords.TryGetValue(accName, out oldRecord))
			{
				oldRecord.StopTimer();
			}
			m_AuthRecords[accName] = record;
		}

		/// <summary>
		/// Tries to get the authentication information for the given account.
		/// </summary>
		/// <param name="accName">the account name to get the auth info object for</param>
		/// <returns>true if the auth info object was found/returned, false otherwise</returns>
		public AuthenticationInfo GetAuthenticationInfo(string accName)
		{
			AuthenticationRecord record;
			m_AuthRecords.TryGetValue(accName, out record);
			if (record != null)
			{
				return record.AuthInfo;
			}
			return null;
		}

		public AuthenticationRecord GetAuthenticationRecord(string accName)
		{
			AuthenticationRecord record;
			m_AuthRecords.TryGetValue(accName, out record);
			return record;
		}

		internal bool RemoveAuthenticationInfo(string accName)
		{
			return m_AuthRecords.Remove(accName);
		}
		#endregion

		/// <summary>
		/// Returns whether the account with the given name is logged in.
		/// </summary>
		/// <param name="accName">the name of the account</param>
		/// <returns>true if the account is logged in; false otherwise</returns>
		public bool IsAccountLoggedIn(string accName)
		{
			return m_loggedInAccounts.ContainsKey(accName);
		}

		#region GetRealm*
		/// <summary>
		/// Returns the Realm with the given name or null
		/// </summary>
		/// <param name="name"></param>
		public static RealmEntry GetRealmByName(string name)
		{
			//lock (m_realmsById.SyncLock)
			{
				return m_realmsById.Values.Where(realm => realm.Name.Equals(name)).FirstOrDefault();
			}
		}

		/// <summary>
		/// Returns the Realm with the given id or null
		/// </summary>
		public static RealmEntry GetRealmById(string id)
		{
			RealmEntry entry;
			m_realmsById.TryGetValue(id, out entry);
			return entry;
		}

		/// <summary>
		/// Returns the no'th Realm
		/// </summary>
		public static RealmEntry GetRealmByNumber(int no)
		{
			int i = 1;
			foreach (var realm in Realms)
			{
				if (i++ == no)
				{
					return realm;
				}
			}
			return null;
		}
		#endregion

		/// <summary>
		/// Disconnects and removes the realm with the given name
		/// </summary>
		public static bool RemoveRealmByName(string realmName)
		{
			var realm = GetRealmByName(realmName);
			if (realm != null)
			{
				realm.Disconnect(true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Disconnects and removes the realm with the given id
		/// </summary>
		public static bool RemoveRealmById(string id)
		{
			var realm = GetRealmById(id);
			if (realm != null)
			{
				realm.Disconnect(true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Disconnects and removes all currently connected realms
		/// </summary>
		public void RemoveAllRealms()
		{
			foreach (var realm in m_realmsById.Values.ToArray())
			{
				realm.Disconnect(true);
			}
		}

		#region Private/Internal Management
		internal static void RemoveRealm(RealmEntry realm, bool remove)
		{
			Instance.ClearAccounts(realm.ChannelId);

			realm.Flags = RealmFlags.Offline;
			realm.Status = RealmStatus.Locked;

			//m_maintenanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
			//m_maintenanceTimer.Dispose();

			if (remove && m_realmsById.Remove(realm.ChannelId))
			{
				var evt = RealmDisconnected;
				if (evt != null)
				{
					evt(realm);
				}
			}
			Instance.UpdateTitle();
		}

		protected override void OnClientDisconnected(IClient client, bool forced)
		{
			var acc = ((AuthClient)client).Account;
			if (acc != null)
			{
				// let the auth info expire
				var record = GetAuthenticationRecord(acc.Name);
				if (record != null)
				{
					record.StartTimer();
				}
			}
			base.OnClientDisconnected(client, forced);
		}

		internal static void AddRealm(RealmEntry realm)
		{
			m_realmsById.Add(realm.ChannelId, realm);

			var evt = RealmConnected;
			if (evt != null)
			{
				evt(realm);
			}

			Instance.UpdateTitle();
		}

		/// <summary>
		/// Clears all logged in accounts of the given Server.
		/// </summary>
		/// <param name="serverId"></param>
		internal void ClearAccounts(string serverId)
		{
			lock (m_loggedInAccounts.SyncLock)
			{
				// create a copy so that deleting won't cause issues
				var acctsToRemove = (from account in m_loggedInAccounts
									 where account.Value.ChannelId == serverId
									 select account.Key).ToArray();

				foreach (var account in acctsToRemove)
				{
					SetAccountLoggedOut(account, false);
				}
			}
		}

		/// <summary>
		/// Marks an account as logged in or out.
		/// </summary>
		/// <param name="accName">the name of the account</param>
		internal void SetAccountLoggedIn(RealmEntry realm, string accName)
		{
			if (!m_loggedInAccounts.ContainsKey(accName))
			{
				m_loggedInAccounts.Add(accName, realm);
				log.Info("Account \"{0}\" logged into: {1}", accName, realm.Name);

				var evt = AccountLoggedIn;
				if (evt != null)
				{
					evt(accName, realm);
				}
			}
		}

		internal void SetAccountLoggedOut(string accName, bool showLog)
		{
			RealmEntry realm;
			if (m_loggedInAccounts.TryGetValue(accName, out realm))
			{
				m_loggedInAccounts.Remove(accName);
				if (showLog)
				{
					log.Info("Account \"{0}\" logged off from: {1}", accName, realm.Name);
				}
				var evt = AccountLoggedOut;
				if (evt != null)
				{
					evt(accName, realm);
				}
			}
		}
		#endregion

		#region ServerBase

		/// <summary>
		/// Starts the authentication server.
		/// </summary>
		public override void Start()
		{
			base.Start();
			if (_running)
			{
				try
				{
					IPCServiceHost.StartService();
				}
				catch (AddressAlreadyInUseException)
				{
					Log.Fatal(resources.AuthServiceAlreadyListening);
					Stop();
				}
			}
		}

		public override void Stop()
		{
			// TODO tobz : add the ability to register cleanup stuff along with initialiation stuff

			IPCServiceHost.StopService();

			base.Stop();
		}


		/// <summary>
		/// Called when a UDP packet is received.
		/// </summary>
		/// <param name="num_bytes">the number of bytes received</param>
		/// <param name="buf">byte[] of the datagram</param>
		/// <param name="ip">the source IP of the datagram</param>
		protected override void OnReceiveUDP(int num_bytes, byte[] buf, IPEndPoint ip)
		{
			throw new Exception("UDP messages are not part of the protocol.");
		}

		/// <summary>
		/// Called when a UDP packet is sent.
		/// </summary>
		/// <param name="clientIP">the destination IP of the datagram</param>
		/// <param name="num_bytes">the number of bytes sent</param>
		protected override void OnSendTo(IPEndPoint clientIP, int num_bytes)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Creates a client object for a newly connected client.
		/// </summary>
		/// <returns>a new AuthClient object</returns>
		protected override IClient CreateClient()
		{
			return new AuthClient(this);
		}

		/// <summary>
		/// Called when a client connects.
		/// </summary>
		/// <param name="client">the client object</param>
		/// <returns>true if everything is good; false otherwise</returns>
		protected override bool OnClientConnected(IClient client)
		{
			base.OnClientConnected(client);

			//if (BanMgr.IsBanned(client.ClientAddress))
			//{
			//    return false;
			//}

			return true;
		}
		#endregion

		/// <summary>
		/// Do necessary cleanup
		/// </summary>
		protected override void OnShutdown()
		{
			if (AuthServerConfiguration.Instance.AutoSave)
			{
				// save config
				AuthServerConfiguration.Instance.Save(true, true);
			}

			Log.Info("Initiating Shutdown...");
			IPCServiceHost.StopService();
			Log.Info("Shutting down...");
		}

		public override string ToString()
		{
			return base.ToString() + " [" + AuthLocalizer.Instance.Translate(AuthLangKey.XRealmsConnected, RealmCount) + "]";
		}
	}
}