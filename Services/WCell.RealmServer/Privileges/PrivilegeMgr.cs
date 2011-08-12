/*************************************************************************
 *
 *   file		: PrivilegeMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 00:08:42 +0100 (sø, 24 jan 2010) $

 *   revision		: $Rev: 1212 $
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
using System.Reflection;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Privileges
{
	/// <summary>
	/// Handles the management of role groups, and their permissions.
	/// </summary>
	public class PrivilegeMgr : Manager<PrivilegeMgr>
	{
		#region Fields
		private Dictionary<string, RoleGroup> m_roleGroups;
		#endregion

		/// <summary>
		/// Default constructor.
		/// </summary>
		private PrivilegeMgr()
		{
		}

		public void SetGroupInfo(RoleGroupInfo[] infos)
		{
			var groups = new Dictionary<string, RoleGroup>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var info in infos)
			{
				groups.Add(info.Name, new RoleGroup(info));
			}
			m_roleGroups = groups;
		}

		/// <summary>
		/// Returns the RoleGroup with the highest Rank.
		/// </summary>
		public RoleGroup HighestRole
		{
			get
			{
				if (m_roleGroups == null)
				{
					SetGroupInfo(RoleGroupInfo.CreateDefaultGroups().ToArray());
				}
				return m_roleGroups[RoleGroupInfo.HighestRole.Name];
			}
		}

		/// <summary>
		/// Returns the RoleGroup with the highest Rank.
		/// </summary>
		public RoleGroup LowestRole
		{
			get
			{
				return m_roleGroups[RoleGroupInfo.LowestRole.Name];
			}
		}

		/// <summary>
		/// All existing RoleGroups
		/// </summary>
		public Dictionary<string, RoleGroup> RoleGroups
		{
			get
			{
				return m_roleGroups;
			}
		}

		public bool IsInitialized
		{
			get;
			internal set;
		}

		#region Methods

		/// <summary>
		/// Gets a role group by name.
		/// </summary>
		/// <returns>the RoleGroup if it exists; null otherwise</returns>
		public RoleGroup GetRoleOrDefault(string roleGroupName)
		{
			RoleGroup retGrp;

			if (m_roleGroups.TryGetValue(roleGroupName, out retGrp))
			{
				return retGrp;
			}

			return m_roleGroups.Values.First();
		}

		public RoleGroup GetRole(string roleGroupName)
		{
			RoleGroup retGrp;

			if (m_roleGroups.TryGetValue(roleGroupName, out retGrp))
			{
				return retGrp;
			}

			return null;
		}

		#endregion

		#region Initialization/teardown

		public void Setup()
		{
			RealmServer.Instance.AuthenticationService.Call(channel =>
			                                                	{
			                                                		var groups = channel.RetrieveRoleGroups();
			                                                		if (groups != null)
			                                                		{
			                                                			SetGroupInfo(groups);
			                                                		}
			                                                	});
		}

		#endregion
	}
}