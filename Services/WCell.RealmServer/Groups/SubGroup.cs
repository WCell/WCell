/*************************************************************************
 *
 *   file		: SubGroup.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-27 14:51:08 +0800 (Thu, 27 Mar 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 203 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Groups
{
	public class SubGroup : IEnumerable<GroupMember>, ICharacterSet
	{
		public const int MaxMemberCount = 5;

		protected internal IList<GroupMember> m_members;
		private readonly Group m_group;
		private readonly byte m_Id;

		#region Constructors

		public SubGroup(Group group, byte groupUnitId)
		{
			m_group = group;
			m_Id = groupUnitId;
			m_members = new List<GroupMember>(MaxMemberCount);
		}

		#endregion

		#region Properties

		public Group Group
		{
			get { return m_group; }
		}

		/// <summary>
		/// Whether this SubGroup has already the max amount of members
		/// </summary>
		public bool IsFull
		{
			get { return (m_members.Count == MaxMemberCount); }
		}

		public byte Id
		{
			get { return m_Id; }
		}

		public int Count
		{
			get { return m_members.Count; }
		}

		public FactionGroup FactionGroup
		{
			get { return m_group.FactionGroup; }
		}

		public void ForeachCharacter(Action<Character> callback)
		{

		}

		public Character[] GetCharacters()
		{
			var chrs = new Character[MaxMemberCount];
			var c = 0;

			m_group.SyncRoot.EnterReadLock();
			try
			{
				foreach (var member in m_members)
				{
					if (member.Character != null)
					{
						chrs[c++] = member.Character;
					}
				}
			}
			finally
			{
				m_group.SyncRoot.ExitReadLock();
			}

			if (chrs.Length > c)
			{
				Array.Resize(ref chrs, c);
			}
			return chrs;
		}

		public GroupMember[] Members
		{
			get { return m_members.ToArray(); }
		}

		#endregion

		#region Methods

		public bool AddMember(GroupMember member)
		{
			if (!IsFull)
			{
				m_members.Add(member);
				member.SubGroup = this;
				return true;
			}
			return false;
		}

		internal bool RemoveMember(GroupMember member)
		{
			if (m_members.Remove(member))
			{
				member.SubGroup = null;
				return true;
			}
			return false;
		}

		public GroupMember this[uint lowMemberId]
		{
			get
			{
				foreach (var member in m_members)
				{
					if (member.Id == lowMemberId)
					{
						return member;
					}
				}
				return null;
			}
		}

		public GroupMember this[string name]
		{
			get
			{
				var capName = name.ToLower();

				foreach (var member in m_members)
				{
					if (member.Name.ToLower() == capName)
						return member;
				}
				return null;
			}
		}

		#endregion

		#region IEnumerable

		public IEnumerator<GroupMember> GetEnumerator()
		{
			return m_members.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_members.GetEnumerator();
		}

		#endregion

		/// <summary>
		/// Send a packet to every group member except for the one specified.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		/// <param name="ignored">the member that won't receive the packet</param>
		public void Send(RealmPacketOut packet, GroupMember ignored)
		{
			Character charMember;
			m_group.SyncRoot.EnterReadLock();
			try
			{
				foreach (var member in m_members)
				{
					if (member != ignored)
					{
						charMember = member.Character;

						if (charMember != null)
						{
							charMember.Client.Send(packet);
						}
					}
				}
			}
			finally
			{
				m_group.SyncRoot.ExitReadLock();
			}
		}

		public void Send(RealmPacketOut packet)
		{
			Send(packet, null);
		}
	}
}