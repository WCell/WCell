using System;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Guilds
{
	[ActiveRecord("GuildEventLogEntries", Access = PropertyAccess.Property)]
	public class GuildEventLogEntry : ActiveRecordBase<GuildEventLogEntry>
	{
		[PrimaryKey(PrimaryKeyType.Increment)]
		long Guid
		{
			get;
			set;
		}

		[Field("GuildId", NotNull = true)]
		private int m_GuildId;

		public uint GuildId
		{
			get { return (uint)m_GuildId; }
			set { m_GuildId = (int)value; }
		}

		[Property(NotNull = true)]
		public GuildEventLogEntryType Type
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Character1LowId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Character2LowId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int NewRankId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public DateTime TimeStamp
		{
			get;
			set;
		}

		public GuildEventLogEntry(Guild guild, GuildEventLogEntryType type,
			int character1LowId, int character2LowId, int newRankId, DateTime timeStamp)
		{
			GuildId = guild.Id;
			Type = type;
			Character1LowId = character1LowId;
			Character2LowId = character2LowId;
			NewRankId = newRankId;
			TimeStamp = timeStamp;
		}

		public GuildEventLogEntry()
		{

		}

		public static GuildEventLogEntry[] FindAll(Guild guild)
		{
			return FindAllByProperty("m_GuildId", (int)guild.Id);
		}
	}
}
