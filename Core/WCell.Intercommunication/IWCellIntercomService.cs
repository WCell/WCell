/*************************************************************************
 *
 *   file		: ClientServiceContract.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-02 18:37:54 +0800 (Wed, 02 Sep 2009) $
 
 *   revision		: $Rev: 1070 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.CodeDom.Compiler;
using System.ServiceModel;
using System;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Intercommunication.DataTypes;

namespace WCell.Intercommunication
{
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	[ServiceContract(ConfigurationName = "IAuthenticationService")]
	public interface IWCellIntercomService
	{
		#region Accounts & Authentication
		/// <summary>
		/// Handles authentication information requests
		/// </summary>
		/// <param name="accountName">the account name to return information on</param>
		/// <returns>the AuthenticationInfo for an account</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/DequeueAuthenticationInfo",
			ReplyAction = "http://www.wcell.org/IServerIPC/DequeueAuthenticationInfoResponse")]
		AuthenticationInfo GetAuthenticationInfo(string accountName);

		/// <summary>
		/// Handles account information requests
		/// </summary>
		/// <param name="accountName">the account name to return information on</param>
		/// <returns>the AccountInfo for an account</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/RequestAccountInfo",
			ReplyAction = "http://www.wcell.org/IServerIPC/RequestAccountInfoResponse")]
		AccountInfo RequestAccountInfo(string accountName, byte[] requestAddr);

		/// <summary>
		/// Retrieves all information of the corresponding Account
		/// </summary>
		/// <param name="accountName">the account name to return information on</param>
		/// <returns>the AccountInfo for an account</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/RequestFullAccountInfo",
			ReplyAction = "http://www.wcell.org/IServerIPC/RequestFullAccountInfoResponse")]
		FullAccountInfo RequestFullAccountInfo(string accountName);
		#endregion

		/// <summary>
		/// Registers a realm server with the authentication server
		/// </summary>
		/// <param name="realmName">the name of the server</param>
		/// <param name="serverType">the type of the server</param>
		/// <param name="flags">the up/down status of the serer (green/red)</param>
		/// <param name="serverCategory">the timezone the server is in</param>
		/// <param name="serverStatus">the status of the server (locked or not)</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/RegisterRealmServer",
			ReplyAction = "http://www.wcell.org/IServerIPC/RegisterRealmServerResponse")]
		void RegisterRealmServer(string realmName, string addr, int port, int chrCount, int capacity, RealmServerType serverType,
		                         RealmFlags flags, RealmCategory serverCategory, RealmStatus serverStatus, ClientVersion version);

		/// <summary>
		/// Updates a realm server's entry in the realm list
		/// </summary>
		/// <param name="serverName">the name of the server</param>
		/// <param name="serverType">the type of the server</param>
		/// <param name="flags">the up/down status of the serer (green/red)</param>
		/// <param name="serverCategory">the timezone the server is in</param>
		/// <param name="serverStatus">the status of the server (locked or not)</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/UpdateRealmServer",
			ReplyAction = "http://www.wcell.org/IServerIPC/UpdateRealmServerResponse")]
		bool UpdateRealmServer(string serverName, int chrCount, int capacity, RealmServerType serverType,
		                       RealmFlags flags, RealmCategory serverCategory, RealmStatus serverStatus);

		[OperationContract(Action = "http://www.wcell.org/IServerIPC/UnregisterRealmServer",
			ReplyAction = "http://www.wcell.org/IServerIPC/UnregisterRealmServerResponse")]
		void UnregisterRealmServer();

		#region Logon
		/// <summary>
		/// Sets multiple accounts as logged in
		/// </summary>
		/// <param name="accNames">the account names to login</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAllActiveAccounts",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAllActiveAccountsResponse")]
		void SetAllActiveAccounts(string[] accNames);

		/// <summary>
		/// Removes multiple accounts from the 'logged in' list
		/// </summary>
		/// <param name="accNames">the account names to log out</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetMultipleAccountsLoggedOut",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetMassAccountLoggedOutResponse")]
		void SetMultipleAccountsLoggedOut(string[] accNames);

		/// <summary>
		/// Sets an account as logged in
		/// </summary>
		/// <param name="accName">the account to log in</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountLoggedIn",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountLoggedInResponse")]
		void SetAccountLoggedIn(string accName);

		/// <summary>
		/// Removes an account from the 'logged in' list
		/// </summary>
		/// <param name="accName">the account to log out</param>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountLoggedOut",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountLoggedOutResponse")]
		void SetAccountLoggedOut(string accName);
		#endregion

		#region Privileges
		/// <summary>
		/// Retrieves the RoleGroupInfos
		/// </summary>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/RequestRoleGroups",
			ReplyAction = "http://www.wcell.org/IServerIPC/RequestRoleGroupsResponse")]
		RoleGroupInfo[] RetrieveRoleGroups();

		/// <summary>
		/// Retrieves the RoleGroupInfo
		/// </summary>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/RequestRoleGroup",
			ReplyAction = "http://www.wcell.org/IServerIPC/RequestRoleGroupResponse")]
		RoleGroupInfo RetrieveRoleGroup(string name);

		/// <summary>
		/// Sets the Account's Role
		/// </summary>
		/// <returns>Whether it succeeded.</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountRole",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountRoleResponse")]
		bool SetAccountRole(long accountId, string role);
		#endregion

		/// <summary>
		/// Sets the Account's Role
		/// </summary>
		/// <returns>Whether it succeeded.</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountEmail",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountEmailResponse")]
		bool SetAccountEmail(long id, string email);

		/// <summary>
		/// Activates or deactivates the Account
		/// </summary>
		/// <returns>Whether it succeeded.</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountActive",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountActiveResponse")]
		bool SetAccountActive(long accountId, bool active, DateTime? statusUntil);

		/// <summary>
		/// Sets the Account password
		/// </summary>
		/// <returns>Whether it succeeded.</returns>
		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetAccountPass",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetAccountPassResponse")]
		bool SetAccountPass(long id, string oldPassStr, byte[] pass);

		[OperationContract(Action = "http://www.wcell.org/IServerIPC/SetHighestLevel",
			ReplyAction = "http://www.wcell.org/IServerIPC/SetHighestLevelResponse")]
		void SetHighestLevel(long id, int level);

		[OperationContract(Action = "http://www.wcell.org/IServerIPC/ExecuteCommand",
			ReplyAction = "http://www.wcell.org/IServerIPC/ExecuteCommandResponse")]
		BufferedCommandResponse ExecuteCommand(string cmd);
	}
}