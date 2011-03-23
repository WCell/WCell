/*************************************************************************
 *
 *   file		: RoleGroup.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1227 $
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
using WCell.Util.Logging;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Commands;
using WCell.Util.Commands;

namespace WCell.RealmServer.Privileges
{
	/// <summary>
	/// Defines a group with specific traits and permissions.
	/// </summary>
	[Serializable]
	public class RoleGroup : IComparable<RoleGroup>, IComparable<int>, IRoleGroup
	{
		readonly HashSet<Command<RealmServerCmdArgs>> m_commands = new HashSet<Command<RealmServerCmdArgs>>();

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RoleGroup()
		{
			Name = "";
			Rank = 0;
			InheritanceList = new string[0];
			CommandNames = new string[0];
		}

		public RoleGroup(RoleGroupInfo info)
		{
			Name = info.Name;
			Rank = info.Rank;
			Status = info.Status;
			AppearAsGM = info.AppearAsGM;
			AppearAsQA = info.AppearAsQA;
			CanUseCommandsOnOthers = info.CanUseCommandsOnOthers;
			CanHandleTickets = info.CanHandleTickets;
			MaySkipAuthQueue = info.MaySkipAuthQueue;
			ScrambleChat = info.ScrambleChat;

			if (info.InheritanceList != null)
			{
				InheritanceList = info.InheritanceList;
			}
			else
			{
				InheritanceList = new string[0];
			}

			CommandNames = info.CommandNames;
			foreach (var cmdName in info.CommandNames)
			{
				if (cmdName == RoleGroupInfo.AllCommands)
				{
					// all commands
					foreach (var command in RealmCommandHandler.Instance.Commands)
					{
						if (!m_commands.Contains(command))
						{
							m_commands.Add(command);
						}
					}
					break;
				}
				else if (cmdName == RoleGroupInfo.StatusCommands)
				{
					// all default Commands allowed for the given Role
					foreach (var command in RealmCommandHandler.Instance.Commands)
					{
						if (command is RealmServerCommand && info.Status >= ((RealmServerCommand)command).RequiredStatusDefault
							//&& !m_commands.Contains(command)
							)
						{
							m_commands.Add(command);
						}
					}
				}
				else
				{
					var cmd = RealmCommandHandler.Instance[cmdName];
					if (cmd != null)
					{
						m_commands.Add(cmd);
					}
					else
					{
						LogManager.GetCurrentClassLogger().Warn("Invalid Command \"{0}\" specified for Role \"{1}\"", cmdName, info);
					}
				}
			}
		}

		#region Properties

		/// <summary>
		/// The name of the role.
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// RoleStatus indicates the relevance of a role
		/// </summary>
		public RoleStatus Status
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the player is a staff-member
		/// </summary>
		public bool IsStaff
		{
			get { return Status >= RoleStatus.Staff; }
		}

		/// <summary>
		/// Whether the player can always login, even if the Realm is full
		/// </summary>
		public bool MaySkipAuthQueue
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the player's chat will be scrambled
		/// </summary>
		public bool ScrambleChat
		{
			get;
			set;
		}

		/// <summary>
		/// Whether or not the role makes the player a GM.
		/// </summary>
		public bool AppearAsGM
		{
			get;
			set;
		}

		/// <summary>
		/// Whether or not the role makes the player a QA.
		/// </summary>
		public bool AppearAsQA
		{
			get;
			set;
		}

		/// <summary>
		/// The actual Rank of this Role
		/// </summary>
		public int Rank
		{
			get;
			set;
		}

		public bool CanUseCommandsOnOthers
		{
			get;
			set;
		}

		public bool CanHandleTickets
		{
			get;
			set;
		}

		/// <summary>
		/// A list of the other roles the role inherits from, permissions-wise.
		/// </summary>
		public string[] InheritanceList
		{
			get;
			set;
		}

		/// <summary>
		/// A list of the names of all allowed Commands.
		/// </summary>
		public string[] CommandNames
		{
			get;
			set;
		}

		/// <summary>
		/// A list of all allowed Commands.
		/// </summary>
		public HashSet<Command<RealmServerCmdArgs>> Commands
		{
			get
			{
				return m_commands;
			}
		}

		public override bool Equals(object obj)
		{
			return (obj is RoleGroup && ((RoleGroup)obj).Rank == Rank);
		}

		public override int GetHashCode()
		{
			return Rank;
		}
		#endregion

		#region IComparable<int> Members

		public int CompareTo(int other)
		{
			return Rank - other;
		}

		#endregion

		#region IComparable<RoleGroup> Members
		public int CompareTo(RoleGroup other)
		{
			return Rank - other.Rank;
		}

		#endregion

		#region Comparison Operators
		public static bool operator >(RoleGroup left, RoleGroup right)
		{
			return null == right || left.Rank > right.Rank;
		}

		public static bool operator >=(RoleGroup left, RoleGroup right)
		{
			return null == right || left.Rank >= right.Rank;
		}

		public static bool operator <(RoleGroup left, RoleGroup right)
		{
			return null != right && left.Rank < right.Rank;
		}

		public static bool operator <=(RoleGroup left, RoleGroup right)
		{
			return null != right && left.Rank <= right.Rank;
		}


		public static bool operator >(RoleGroup left, int right)
		{
			return left.Rank > right;
		}

		public static bool operator <(RoleGroup left, int right)
		{
			return left.Rank < right;
		}

		public static bool operator >=(RoleGroup left, int right)
		{
			return left.Rank >= right;
		}

		public static bool operator <=(RoleGroup left, int right)
		{
			return left.Rank <= right;
		}

		public static bool operator ==(RoleGroup left, int right)
		{
			return left.Rank == right;
		}

		public static bool operator !=(RoleGroup left, int right)
		{
			return left.Rank != right;
		}

		public static bool operator >(RoleGroup left, RoleStatus right)
		{
			return left.Status > right;
		}

		public static bool operator <(RoleGroup left, RoleStatus right)
		{
			return left.Status < right;
		}

		public static bool operator >=(RoleGroup left, RoleStatus right)
		{
			return left.Status >= right;
		}

		public static bool operator <=(RoleGroup left, RoleStatus right)
		{
			return left.Status <= right;
		}

		public static bool operator ==(RoleGroup left, RoleStatus right)
		{
			return left.Status == right;
		}

		public static bool operator !=(RoleGroup left, RoleStatus right)
		{
			return left.Status != right;
		}
		#endregion

		public override string ToString()
		{
			return Name + " (Rank: " + Rank + ")";
		}

		public bool MayUse(Command<RealmServerCmdArgs> cmd)
		{
			return Commands.Contains(cmd);
		}
	}
}