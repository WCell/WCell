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
using System.Linq;
using System.Net;
using System.ServiceModel;
using Cell.Core;
using Cell.Core.Collections;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Firewall;
using WCell.AuthServer.IPC;
using WCell.AuthServer.Localization;
using WCell.AuthServer.Network;
using WCell.Core;
using WCell.Intercommunication.DataTypes;
using WCell.Util.NLog;
using WCell.Util.Variables;

namespace WCell.AuthServer
{
	/// <summary>
	/// Server class for the authentication server. Handles all initial 
	/// incoming connections and does authentication of users.
	/// </summary>
	[VariableClassAttribute(true)]
	public sealed class AuthenticationServer : ServerApp<AuthenticationServer>
	{
		private static readonly ImmutableDictionary<string, RealmEntry> m_realmsById = new ImmutableDictionary<string, RealmEntry>();
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
		public static ImmutableDictionary<string, RealmEntry> Realms
		{
			get { return m_realmsById; }
		}

		/// <summary>
		/// Number of available realms.
		/// </summary>
		public int RealmCount
		{
			get { return m_realmsById.Count; }
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

		/// <summary>
		/// Checks if an account is logged in.
		/// </summary>
		/// <param name="accName">the name of the account</param>
		/// <returns>true if the account is logged in; false otherwise</returns>
		public bool IsAccountLoggedIn(string accName)
		{
			return m_loggedInAccounts.ContainsKey(accName);
		}

		protected override void OnClientDisconnected(IClient client, bool forced)
		{
			var acc = ((AuthClient) client).Account;
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

		#region Private/Internal Management
		internal RealmEntry GetOrCreateRealm(string id)
		{
			RealmEntry realm;
			if (!m_realmsById.TryGetValue(id, out realm))
			{
				m_realmsById.Add(id, realm = new RealmEntry());
			}
			return realm;
		}

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
			foreach (var realm in Realms.Values)
			{
				if (i++ == no)
				{
					return realm;
				}
			}
			return null;
		}

		/// <summary>
		/// Clears all logged in accounts of the given Server.
		/// </summary>
		/// <param name="serverId"></param>
		internal void ClearAccounts(string serverId)
		{
			lock (m_loggedInAccounts.SyncLock)
			{
				var acctsToRemove = (from account in m_loggedInAccounts
									 where account.Value.ChannelId == serverId
									 select account.Key).ToList();

				foreach (var account in acctsToRemove)
				{
					m_loggedInAccounts.Remove(account);
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
			}
		}

		internal void SetAccountLoggedOut(string accName)
		{
			log.Info("Account \"{0}\" logged off.", accName);
			m_loggedInAccounts.Remove(accName);
		}

		internal void ClearRealms()
		{
			m_loggedInAccounts.Clear();
			m_realmsById.Clear();
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
					s_log.Fatal(Resources.AuthServiceAlreadyListening);
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
		/// Do some necessary cleanup
		/// </summary>
		protected override void OnShutdown()
		{
			if (AuthServerConfiguration.Instance.AutoSave)
			{
				AuthServerConfiguration.Instance.Save(true, true);
			}

			s_log.Info("Initiating Shutdown...");
			IPCServiceHost.StopService();
			s_log.Info("Shutting down...");
		}
	}
}