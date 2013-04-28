using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Database.Entities
{
	public class GuildEventLogEntry
	{
		//private static readonly Order CreatedOrder = new Order("Created", false);

		public static IEnumerable<GuildEventLogEntry> LoadAll(int guildId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<GuildEventLogEntry>(x => x.GuildId == guildId).OrderBy(entry => entry.TimeStamp); //TODO: Check this is going to do things as we want
		}

		long Guid
		{
			get;
			set;
		}

		public int GuildId { get; set; }

		public GuildEventLogEntryType Type
		{
			get;
			set;
		}

		public int Character1LowId
		{
			get;
			set;
		}

		public int Character2LowId
		{
			get;
			set;
		}

		public int NewRankId
		{
			get;
			set;
		}

		public DateTime TimeStamp
		{
			get;
			set;
		}

		public GuildEventLogEntry(Database.Entities.Guild guild, GuildEventLogEntryType type,
			int character1LowId, int character2LowId, int newRankId, DateTime timeStamp)
		{
			GuildId = guild.Id;
			Type = type;
			Character1LowId = character1LowId;
			Character2LowId = character2LowId;
			NewRankId = newRankId;
			TimeStamp = timeStamp;
		}

		public static GuildEventLogEntry[] FindAll(Guild guild)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<GuildEventLogEntry>(x => x.GuildId == guild.Id).ToArray();
		}
	}
}