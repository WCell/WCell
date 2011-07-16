/*************************************************************************
 *
 *   file		: ClientAdapter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-17 08:08:03 +0800 (Sun, 17 Aug 2008) $
 
 *   revision		: $Rev: 598 $
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
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using System.CodeDom.Compiler;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Intercommunication.DataTypes;
using System.ServiceModel.Channels;
using WCell.Constants.Realm;
using System.Net;

namespace WCell.Intercommunication.Client
{
	[DebuggerStepThrough()]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	public class AuthenticationClientAdapter : ClientBase<IWCellIntercomService>, IWCellIntercomService
	{
		public event Action<Exception> Error;

		public AuthenticationClientAdapter()
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName)
			:
				base(endpointConfigurationName)
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName, string remoteAddress)
			:
				base(endpointConfigurationName, remoteAddress)
		{
		}

		public AuthenticationClientAdapter(string endpointConfigurationName, EndpointAddress remoteAddress)
			:
				base(endpointConfigurationName, remoteAddress)
		{
		}

		public AuthenticationClientAdapter(Binding binding, EndpointAddress remoteAddress)
			:
				base(binding, remoteAddress)
		{
		}

		private void OnError(Exception e)
		{
			var evt = Error;
			if (evt != null)
			{
				evt(e);
			}
		}

		#region Accounts & Authentication
		public AuthenticationInfo GetAuthenticationInfo(string accountName)
		{
			try
			{
				return Channel.GetAuthenticationInfo(accountName);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}

		public AccountInfo RequestAccountInfo(string accountName, byte[] requestAddr)
		{
			try
			{
				return Channel.RequestAccountInfo(accountName, requestAddr);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}

		public FullAccountInfo RequestFullAccountInfo(string accountName)
		{
			try
			{
				return Channel.RequestFullAccountInfo(accountName);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}
		#endregion

		public void RegisterRealmServer(string realmName, string addr, int port, int chrCount, int capacity, RealmServerType serverType,
									  RealmFlags flags, RealmCategory serverCategory, RealmStatus serverStatus, ClientVersion version)
		{
			try
			{
				Channel.RegisterRealmServer(realmName, addr, port, chrCount, capacity, serverType, flags, serverCategory,
											serverStatus, version);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		/// <summary>
		/// Updates this Realm's status at the AuthServer.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="serverType"></param>
		/// <param name="flags"></param>
		/// <param name="serverCategory"></param>
		/// <param name="serverStatus"></param>
		/// <returns></returns>
		public bool UpdateRealmServer(string serverName, int chrCount, int capacity, RealmServerType serverType,
									  RealmFlags flags, RealmCategory serverCategory, RealmStatus serverStatus)
		{
			try 
			{
				return Channel.UpdateRealmServer(serverName, chrCount, capacity, serverType, flags, serverCategory,
											serverStatus);
			}
			catch (Exception e) {
				OnError(e);
				return false;
			}
		}

		public void UnregisterRealmServer()
		{
			try
			{
				Channel.UnregisterRealmServer();
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public void SetAllActiveAccounts(string[] accNames)
		{
			try
			{
				Channel.SetAllActiveAccounts(accNames);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public void SetMultipleAccountsLoggedOut(string[] accNames)
		{
			try
			{
				Channel.SetMultipleAccountsLoggedOut(accNames);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public void SetAccountLoggedIn(string accName)
		{
			try
			{
				Channel.SetAccountLoggedIn(accName);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public void SetAccountLoggedOut(string accName)
		{
			try
			{
				Channel.SetAccountLoggedOut(accName);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public RoleGroupInfo[] RetrieveRoleGroups()
		{
			try
			{
				return Channel.RetrieveRoleGroups();
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}

		public RoleGroupInfo RetrieveRoleGroup(string name)
		{
			try
			{
				return Channel.RetrieveRoleGroup(name);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}

		/// <summary>
		/// Set the Account information remotely.
		/// </summary>
		public bool SetAccountRole(long id, string role)
		{
			try
			{
				return Channel.SetAccountRole(id, role);
			}
			catch (Exception e)
			{
				OnError(e);
				return false;
			}
		}

		/// <summary>
		/// Set the Account information remotely.
		/// </summary>
		public bool SetAccountEmail(long id, string email)
		{
			try
			{
				return Channel.SetAccountEmail(id, email);
			}
			catch (Exception e)
			{
				OnError(e);
				return false;
			}
		}

		/// <summary>
		/// Set the Account information remotely.
		/// </summary>
		public bool SetAccountActive(long accountId, bool active, DateTime? statusUntil)
		{
			try
			{
				return Channel.SetAccountActive(accountId, active, statusUntil);
			}
			catch (Exception e)
			{
				OnError(e);
				return false;
			}
		}

		/// <summary>
		/// Set the Account information remotely.
		/// </summary>
		public bool SetAccountPass(long id, string oldPassStr, byte[] pass)
		{
			try
			{
				return Channel.SetAccountPass(id, oldPassStr, pass);
			}
			catch (Exception e)
			{
				OnError(e);
				return false;
			}
		}

		/// <summary>
		/// Set the Account information remotely.
		/// </summary>
		public void SetHighestLevel(long id, int level)
		{
			try
			{
				Channel.SetHighestLevel(id, level);
			}
			catch (Exception e)
			{
				OnError(e);
			}
		}

		public BufferedCommandResponse ExecuteCommand(string cmd)
		{
			try
			{
				return Channel.ExecuteCommand(cmd);
			}
			catch (Exception e)
			{
				OnError(e);
				return null;
			}
		}
	}
}