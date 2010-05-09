using System;
using System.Collections.Generic;
using System.Linq;
using Cell.Core.Collections;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.Util.Threading;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Guilds
{
	public class GuildEventLog
	{
		protected const int MAX_ENTRIES_COUNT = 100;

		protected readonly Guild m_guild;
		protected readonly LockfreeQueue<GuildEventLogEntry> m_entries;

		public LockfreeQueue<GuildEventLogEntry> Entries
		{
			get { return m_entries; }
		}

		internal GuildEventLog(Guild guild, bool isNew)
			: this(guild)
		{
			if (!isNew)
			{
				var entries = GuildEventLogEntry.FindAllByProperty("m_GuildId", (int)guild.Id);
				foreach (var entry in entries)
				{
					m_entries.Enqueue(entry);
				}
			}
		}

		internal GuildEventLog(Guild guild)
		{
			m_guild = guild;
			m_entries = new LockfreeQueue<GuildEventLogEntry>();
		}

		public void AddEvent(GuildEventLogEntryType type, uint character1LowId, uint character2LowId,
		                     int newRankId)
		{
			var evt = new GuildEventLogEntry(m_guild, type,
			                                 (int)character1LowId,
			                                 (int)character2LowId,
			                                 newRankId, DateTime.Now);
			m_entries.Enqueue(evt);

			evt.CreateLater();

			if (m_entries.Count > MAX_ENTRIES_COUNT)
			{
				var entry = m_entries.Dequeue();
				RealmServer.Instance.AddMessage(new Message(entry.Delete));
			}
		}

		public void AddInviteEvent(uint inviterLowId, uint inviteeLowId)
		{
			AddEvent(GuildEventLogEntryType.INVITE_PLAYER, inviterLowId, inviteeLowId, 0);
		}

		public void AddRemoveEvent(uint removerLowId, uint removedLowId)
		{
			AddEvent(GuildEventLogEntryType.UNINVITE_PLAYER, removerLowId, removedLowId, 0);
		}

		public void AddJoinEvent(uint playerLowId)
		{
			AddEvent(GuildEventLogEntryType.JOIN_GUILD, playerLowId, 0, 0);
		}

		public void AddPromoteEvent(uint promoterLowId, uint targetLowId, int newRankId)
		{
			AddEvent(GuildEventLogEntryType.PROMOTE_PLAYER, promoterLowId, targetLowId, newRankId);
		}

		public void AddDemoteEvent(uint demoterLowId, uint targetLowId, int newRankId)
		{
			AddEvent(GuildEventLogEntryType.DEMOTE_PLAYER, demoterLowId, targetLowId, newRankId);
		}

		public void AddLeaveEvent(uint playerLowId)
		{
			AddEvent(GuildEventLogEntryType.LEAVE_GUILD, playerLowId, 0, 0);
		}
	}
}