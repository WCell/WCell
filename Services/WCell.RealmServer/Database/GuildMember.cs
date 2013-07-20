using System;
using System.Collections.Generic;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Database.Entities
{
	public partial class GuildMember
	{
		public long CharacterLowId
		{
			get;
			private set;
		}

		private string _name;

		private int _lastLevel;

		private DateTime _lastLogin;

		private int _lastZoneId;

		private int _class;

		private int _rankId;

		private int m_GuildId;

		public uint GuildId
		{
			get { return (uint)m_GuildId; }
			set { m_GuildId = (int)value; }
		}

		private string _publicNote;

		private string _officerNote;

		private int _remainingMoneyAllowance;

		private DateTime _moneyAllowanceResetTime;

		/// <summary>
		/// Loads all members of the given guild from the DB
		/// </summary>
		public static IEnumerable<GuildMember> FindAll(uint guildId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<GuildMember>(x => x.m_GuildId == (int)guildId);
		}

		public GuildMember(CharacterRecord chr, Database.Entities.Guild guild, GuildRank rank)
			: this()
		{
			var zoneId = (int)chr.Zone;

			Guild = guild;

			CharacterLowId = (int)chr.EntityLowId;
			_rankId = rank.RankIndex;
			_name = chr.Name;
			_lastLevel = chr.Level;
			_lastLogin = DateTime.Now;
			_lastZoneId = zoneId;
			_class = (int)chr.Class;
			_publicNote = string.Empty;
			_officerNote = string.Empty;
		}
	}
}