/*************************************************************************
 *
 *   file		: PrivilegeMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-14 13:26:25 +0800 (Wed, 14 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 351 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using WCell.Util.Logging;
using resources = WCell.AuthServer.Res.WCell_AuthServer;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;

namespace WCell.AuthServer.Privileges
{
	/// <summary>
	/// Handles the management of role groups, and their permissions.
	/// </summary>
	public class PrivilegeMgr : Manager<PrivilegeMgr>
	{
		/// <summary>
		/// The location of the configuration file within the 
		/// </summary>
		public static string RoleGroupFile = "RoleGroups.xml";

		#region Fields
		private FileSystemWatcher m_configWatcher;
		private Dictionary<string, RoleGroupInfo> m_roleGroups;
		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		private PrivilegeMgr()
		{
			var dir = AuthServerConfiguration.ConfigDir;
			if (!File.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			m_configWatcher = new FileSystemWatcher(dir);

			m_configWatcher.Changed += ConfigChanged;
		}

		public IDictionary<string, RoleGroupInfo> RoleGroups
		{
			get { return m_roleGroups; }
		}

		protected void LoadConfig()
		{
			LoadConfiguration();

			var role = AuthServerConfiguration.DefaultRole;
			if (!Instance.Exists(role))
			{
				throw new Exception("Default Role (Config: DefaultRole) does not exist: " + role);
			}
		}

		public void LoadConfiguration()
		{
			var pcfg = RoleGroupConfig.LoadConfigOrDefault(RoleGroupFile);
			m_roleGroups = new Dictionary<string, RoleGroupInfo>(StringComparer.InvariantCultureIgnoreCase);

			foreach (var group in pcfg.RoleGroups)
			{
				m_roleGroups.Add(group.Name, group);
			}
		}

		private void ConfigChanged(object sender, FileSystemEventArgs e)
		{
			if ((e.ChangeType == WatcherChangeTypes.Changed) &&
				e.Name == RoleGroupFile)
			{
				LogManager.GetCurrentClassLogger().Info(resources.PrivilegeConfigChanged);

				try
				{
					LoadConfig();
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to reload Configuration.", ex);
				}
			}
		}

		/// <summary>
		/// Clears all currently-loaded commands and roles.
		/// </summary>
		public void ClearConfiguration()
		{
			m_roleGroups.Clear();
		}

		public bool Exists(string privLevelName)
		{
			return m_roleGroups.ContainsKey(privLevelName);
		}

		/// <summary>
		/// Gets a role group by name.
		/// </summary>
		/// <returns>the RoleGroup if it exists; null otherwise</returns>
		public RoleGroupInfo GetRoleGroup(string roleGroupName)
		{
			RoleGroupInfo retGrp;

			if (m_roleGroups.TryGetValue(roleGroupName, out retGrp))
			{
				return retGrp;
			}

			return null;
		}

		#region Initialization/teardown

		[Initialization(InitializationPass.Fourth, "Privilege manager")]
		public static void Initialize()
		{
			Instance.LoadConfig();
		}

		#endregion

		/// <summary>
		/// Returns the given PrivLevel or the default one, if role is invalid.
		/// </summary>
		public string GetRoleOrDefault(string role)
		{
			RoleGroupInfo retGrp;

			if (m_roleGroups.TryGetValue(role, out retGrp))
			{
				return retGrp.Name;
			}

			return AuthServerConfiguration.DefaultRole;
		}
	}
}