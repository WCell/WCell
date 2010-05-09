/*************************************************************************
 *
 *   file		: ServiceAdapter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-29 16:55:24 +0800 (Sun, 29 Jun 2008) $
 *   last author	: $LastChangedBy: nosferatus99 $
 *   revision		: $Rev: 538 $
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
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Channels;
using NLog;
using WCell.AuthServer;
using WCell.AuthServer.Localization;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Core.Cryptography;
using WCell.Intercommunication;
using WCell.Intercommunication.DataTypes;
using NHibernate.Criterion;
using WCell.AuthServer.Privileges;
using WCell.AuthServer.Accounts;
using WCell.Core.Database;
using Cell.Core;
using WCell.Constants.Realm;
using WCell.Util;
using WCell.AuthServer.Firewall;
using WCell.AuthServer.Commands;

namespace WCell.AuthServer.IPC
{
    /// <summary>
    /// Defines the service that runs on the authentication server that
    /// the realm servers connect to in order to request account information
    /// and register their presence.
    /// Most methods require to be executed from a remote IPC channel and are often
    /// only valid from a registered Realm.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
	public class IPCServiceAdapter : IWCellIntercomService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static AuthenticationServer s_authServer;

        /// <summary>
        /// The delay after which to retry creating the IPC ServiceAdapter if it closed down.
        /// </summary>
        public static TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

		public static string FormatEP(RemoteEndpointMessageProperty ep)
		{
			return ep.Address + ":" + ep.Port;
		}

        /// <summary>
        /// Default constructor
        /// </summary>
        public IPCServiceAdapter()
        {
            s_authServer = AuthenticationServer.Instance;
			var channel = OperationContext.Current.Channel;

            channel.Faulted += (sender, args) =>
            {
            	var realm = GetRealmByChannel((IContextChannel) sender);
            	String msg;
				if (realm != null)
				{
					msg = realm.ToString();
					realm.SetOffline(AuthServerConfiguration.RemoveOfflineRealms);
				}
				else
				{
					msg = "<Unknown>";
				}
				log.Warn(Resources.RealmDisconnected, msg);
            };
        }

    	private RealmEntry GetRealmByChannel(IContextChannel chan)
    	{
    		foreach (var realm in AuthenticationServer.Realms.Values)
    		{
    			if (realm.Channel == chan)
    			{
					return realm;
    			}
    		}
			return null;
    	}

    	/// <summary>
        /// Returns the Id of the RealmServer that called the current method.
        /// Can only be used from remote IPC Channels.
        /// </summary>
        /// <returns></returns>
        public string GetCurrentId()
        {
        	var context = OperationContext.Current;
			if (context == null)
			{
				return "";
			}
            var channel = context.Channel;
            if (channel == null)
            {
            	return "";
			}
			return channel.InputSession.Id;
        }

        /// <summary>
        /// Returns the RealmEntry that belongs to the Channel
        /// that is performing the current communication.
        /// Can only be used from remote IPC Channels.
        /// </summary>
        public RealmEntry GetCurrentRealm()
        {
            return AuthenticationServer.GetRealmById(GetCurrentId());
        }

        public RemoteEndpointMessageProperty GetCurrentEndPoint()
        {
            return (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
        }

        /// <summary>
        /// Handles authentication information requests
        /// </summary>
        /// <param name="accountName">the account name to return information on</param>
        /// <returns>the AuthenticationInfo for an account</returns>
        public AuthenticationInfo GetAuthenticationInfo(string accountName)
        {
        	var authInfo = s_authServer.GetAuthenticationInfo(accountName);

            if (authInfo == null)
            {
                s_authServer.Error(null, Resources.CannotRetrieveAuthenticationInfo, accountName);
            }

            return authInfo;
        }

        #region Accounts
        /// <summary>
        /// Handles account information requests
        /// </summary>
        /// <param name="accountName">the account name to return information on</param>
        /// <returns>the AccountInfo for an account</returns>
        public AccountInfo RequestAccountInfo(string accountName, byte[] requestAddr)
        {
            var acc = AccountMgr.GetAccount(accountName);

            if (acc == null)
            //|| (requestAddr != 0 && acc.LastIP != requestAddr))
            {
                log.Warn(string.Format(Resources.AttemptedRequestForUnknownAccount,
                    accountName,
                    requestAddr,
                    acc != null ? acc.LastIPStr : "()"));
                return null;
            }

            var info = new AccountInfo
            {
                AccountId = acc.AccountId,
                EmailAddress = acc.EmailAddress,
                ClientId = acc.ClientId,
                RoleGroupName = acc.RoleGroupName,
                LastIP = acc.LastIP,
                LastLogin = acc.LastLogin,
                Locale = acc.Locale
            };
            return info;
        }

        public FullAccountInfo RequestFullAccountInfo(string accountName)
        {
            var acc = AccountMgr.GetAccount(accountName);
            if (acc == null)
            {
                log.Error(string.Format(Resources.AttemptedRequestForUnknownAccount, accountName));
                return null;
            }

            return GetFullAccountInfo(acc);
        }

        public FullAccountInfo GetFullAccountInfo(Account acc)
        {
            var info = new FullAccountInfo
            {
                Name = acc.Name,
                IsActive = acc.IsActive,
                StatusUntil = acc.StatusUntil,
                AccountId = acc.AccountId,
                EmailAddress = acc.EmailAddress,
                ClientId = acc.ClientId,
                RoleGroupName = acc.RoleGroupName,
                LastIP = acc.LastIP,
                LastLogin = acc.LastLogin,
                Locale = acc.Locale
            };
            return info;
        }

        /// <summary>
        /// Sets all active accounts of the communicating RealmServer
        /// </summary>
        /// <param name="accNames">the account names to login</param>
        public void SetAllActiveAccounts(string[] accNames)
        {
            SetAllActiveAccounts(GetCurrentId(), accNames);
        }

        /// <summary>
        /// Sets all active accounts of one realmserver
        /// </summary>
        /// <param name="accNames">the account names to login</param>
        public void SetAllActiveAccounts(string realmId, string[] accNames)
        {
            s_authServer.ClearAccounts(realmId);
            var realm = AuthenticationServer.GetRealmById(realmId);
            if (realm != null)
            {
                for (int i = 0; i < accNames.Length; i++)
                {
                    s_authServer.SetAccountLoggedIn(realm, accNames[i]);
                }
            }
        }

        /// <summary>
        /// Removes multiple accounts from the 'logged in' list
        /// </summary>
        /// <param name="accNames">the account names to log out</param>
        public void SetMultipleAccountsLoggedOut(string[] accNames)
        {
            var realm = GetCurrentRealm();
            if (realm != null)
            {
                SetMultipleAccountsLoggedOut(realm, accNames);
            }
        }

        /// <summary>
        /// Removes multiple accounts from the 'logged in' list
        /// </summary>
        /// <param name="accNames">the account names to log out</param>
        public void SetMultipleAccountsLoggedOut(RealmEntry realm, string[] accNames)
        {
            for (var i = 0; i < accNames.Length; i++)
            {
                s_authServer.SetAccountLoggedOut(accNames[i]);
            }
        }

        /// <summary>
        /// Sets an account as logged in
        /// </summary>
        /// <param name="accName">the account to log in</param>
        public void SetAccountLoggedIn(string accName)
        {
            var realm = GetCurrentRealm();
            if (realm != null)
            {
                SetAccountLoggedIn(realm, accName);
            }
        }

        /// <summary>
        /// Sets an account as logged in
        /// </summary>
        /// <param name="accName">the account to log in</param>
        public void SetAccountLoggedIn(RealmEntry realm, string accName)
        {
            s_authServer.SetAccountLoggedIn(realm, accName);
        }

        /// <summary>
        /// Removes an account from the 'logged in' list
        /// </summary>
        /// <param name="accName">the account to log out</param>
        public void SetAccountLoggedOut(string accName)
        {
            SetAccountLoggedOut(GetCurrentId(), accName);
        }

        /// <summary>
        /// Removes an account from the 'logged in' list
        /// </summary>
        /// <param name="accName">the account to log out</param>
        public void SetAccountLoggedOut(string id, string accName)
        {
            s_authServer.SetAccountLoggedOut(accName);
        }
        #endregion

        #region RealmServer
        /// <summary>
        /// Registers a realm server with the authentication server
        /// </summary>
        /// <param name="serverName">the name of the server</param>
        /// <param name="serverType">the type of the server</param>
        /// <param name="flags">the up/down status of the serer (green/red)</param>
        /// <param name="serverCategory">the timezone the server is in</param>
        /// <param name="serverStatus">the status of the server (locked or not)</param>
        public void RegisterRealmServer(string serverName, string addr, int port, int chrCount, int capacity, 
			RealmServerType serverType, RealmFlags flags, RealmCategory serverCategory, 
			RealmStatus serverStatus, ClientVersion clientVersion)
        {
        	var context = OperationContext.Current;
			if (context == null)
			{
				return;
			}

        	var channel = context.Channel;
			if (channel == null)
			{
				return;
			}

            var id = GetCurrentId();
            var realm = AuthenticationServer.GetRealmById(id);
            var ep = GetCurrentEndPoint();

            // find out whether this server is just re-registering (came back online)
            var isNew = realm == null;

            if (isNew)
            {
                realm = AuthenticationServer.GetRealmByName(serverName);
                if (isNew = (realm == null))
				{
					if (!AuthServerConfiguration.RealmIPs.Contains(ep.Address))
					{
						// Ignore unknown realms
						log.Warn("Unallowed Realm (\"{0}\") tried to register from: {1}", serverName, ep.Address);
						var chan = OperationContext.Current.Channel;
						if (chan != null)
						{
							try
							{
								chan.Close();
							}
							catch (Exception) {}
						}
						return;
					}
                    realm = new RealmEntry();
                }
                realm.ChannelId = id;
            }

            if (string.IsNullOrEmpty(addr))
            {
                // no host given
                addr = ep.Address;
            }

            realm.Name = serverName;
            realm.Address = addr;
            realm.Port = port;
            realm.Flags = flags;
            realm.Status = serverStatus;
            realm.ServerType = serverType;
            realm.Category = serverCategory;
            realm.CharCount = chrCount;
            realm.CharCapacity = capacity;
        	realm.ClientVersion = clientVersion;

        	realm.Channel = channel;
            realm.ChannelAddress = ep.Address;
            realm.ChannelPort = ep.Port;


            if (isNew)
            {
                // register after setting all infos
                lock (AuthenticationServer.Realms)
                {
                    realm.ChannelId = id;
                    AuthenticationServer.Realms.Add(id, realm);
                }
            }
			log.Info(Resources.RealmRegistered, realm); //realm.ChannelAddress);
        }

        /// <summary>
        /// Updates a realm server's entry in the realm list
        /// </summary>
        /// <param name="serverName">the name of the server</param>
        /// <param name="serverType">the type of the server</param>
        /// <param name="flags">the up/down status of the serer (green/red)</param>
        /// <param name="serverCategory">the timezone the server is in</param>
        /// <param name="serverStatus">the status of the server (locked or not)</param>
        public bool UpdateRealmServer(string serverName, int chrCount, int capacity, RealmServerType serverType,
                                      RealmFlags flags, RealmCategory serverCategory, RealmStatus serverStatus)
        {
            var realm = GetCurrentRealm();

			if (realm == null)
			{
				return false;
			}

            realm.Name = serverName;
            realm.Flags = flags;
            realm.Status = serverStatus;
            realm.ServerType = serverType;
            realm.Category = serverCategory;
            realm.CharCount = chrCount;
            realm.CharCapacity = capacity;
            realm.LastUpdate = DateTime.Now;

            //log.Debug(Resources.RealmUpdated, realm.Name);
        	return true;
        }

        /// <summary>
        /// RealmServer went offline.
        /// </summary>
        public void UnregisterRealmServer()
        {
            var realm = GetCurrentRealm();
			if (realm != null)
			{
				realm.SetOffline(true);
				log.Info(Resources.RealmUnregistered, realm);
			}
        }
        #endregion

        #region Privileges


        public RoleGroupInfo[] RetrieveRoleGroups()
        {
            return PrivilegeMgr.Instance.RoleGroups.Values.ToArray();
        }

        public RoleGroupInfo RetrieveRoleGroup(string name)
        {
            return PrivilegeMgr.Instance.GetRoleGroup(name);
        }

        public bool SetAccountRole(long accountId, string role)
        {
            var acc = AccountMgr.GetAccount(accountId);
            if (acc != null)
            {
                acc.RoleGroupName = role;

				AsyncQuery acctSaveQuery = QueryFactory.CreateNonResultQuery(acc.SaveAndFlush);
                ThreadMgr.QueueJob(acctSaveQuery);

                return true;
            }

            return false;
        }

        public bool SetAccountEmail(long accountId, string email)
        {
            var acc = AccountMgr.GetAccount(accountId);
            if (acc != null)
            {
                acc.EmailAddress = email;

                AsyncQuery acctSaveQuery = QueryFactory.CreateNonResultQuery(acc.SaveAndFlush);
                ThreadMgr.QueueJob(acctSaveQuery);

                return true;
            }

            return false;
        }

        public bool SetAccountActive(long accountId, bool active, DateTime? statusUntil)
        {
            var acc = AccountMgr.GetAccount(accountId);
            if (acc != null)
            {
                if (acc.IsActive != active || acc.StatusUntil != statusUntil)
                {
                    acc.IsActive = active;
                    acc.StatusUntil = statusUntil;

                    var acctSaveQuery = QueryFactory.CreateNonResultQuery(acc.SaveAndFlush);
                    return ThreadMgr.QueueJob(acctSaveQuery);
                }
            }

            return false;
        }

        public bool SetAccountPass(long id, string oldPassStr, byte[] pass)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc != null)
            {
            	if (oldPassStr != null)
            	{
            		var oldPass = SecureRemotePassword.GenerateCredentialsHash(acc.Name, oldPassStr);
					if (!oldPass.SequenceEqual(acc.Password))
					{
						return false;
					}
            	}
            	acc.Password = pass;
            	acc.SaveAndFlush();
            	return true;
            }

        	return false;
        }

        public void SetHighestLevel(long id, int level)
        {
            var acc = AccountMgr.GetAccount(id);
            if (acc != null)
            {
                acc.HighestCharLevel = level;

                var acctSaveQuery = QueryFactory.CreateNonResultQuery(acc.SaveAndFlush);
                ThreadMgr.QueueJob(acctSaveQuery);
            }
        }

    	public BufferedCommandResponse ExecuteCommand(string cmd)
    	{
    		return AuthCommandHandler.ExecuteBufferedCommand(cmd);
    	}

    	#endregion
    }
}