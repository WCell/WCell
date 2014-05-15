using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Guilds
{
	public class GuildMember : INamed
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
		public static IEnumerable<GuildMember> FindAll(int guildId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<GuildMember>(x => x.m_GuildId == guildId);
		}

		public GuildMember(CharacterRecord chr, Guild guild, GuildRank rank)
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
		private Character m_chr;
		private Guild m_Guild;

		#region Properties
		/// <summary>
		/// The low part of the Character's EntityId. Use EntityId.GetPlayerId(Id) to get a full EntityId
		/// </summary>
		public uint Id
		{
			get { return (uint)CharacterLowId; }
		}

		/// <summary>
		/// The name of this GuildMember's character
		/// </summary>
		public string Name
		{
			get { return _name; }
		}

		public Guild Guild
		{
			get { return m_Guild; }
			private set
			{
				m_Guild = value;
				GuildId = (uint)value.Id; //TODO: uint type correction again..
			}
		}

		public string PublicNote
		{
			get { return _publicNote; }
			set
			{
				_publicNote = value;

				SaveLater();
			}
		}

		public string OfficerNote
		{
			get { return _officerNote; }
			set
			{
				_officerNote = value;

				SaveLater();
			}
		}

		/// <summary>
		/// Current level of this GuildMember or his saved level if member already logged out
		/// </summary>
		public int Level
		{
			get
			{
				if (m_chr != null)
				{
					return m_chr.Level;
				}
				return _lastLevel;
			}
			internal set { _lastLevel = value; }
		}

		/// <summary>
		/// Time of last login of member
		/// </summary>
		public DateTime LastLogin
		{
			get
			{
				if (m_chr != null)
				{
					return DateTime.Now;
				}
				return _lastLogin;
			}
			internal set { _lastLogin = value; }
		}

		/// <summary>
		/// The current or last Id of zone in which this GuildMember was
		/// </summary>
		public int ZoneId
		{
			get
			{
				if (m_chr != null)
				{
					return m_chr.Zone != null ? (int)m_chr.Zone.Id : 0;
				}
				return _lastZoneId;
			}
			internal set { _lastZoneId = value; }
		}

		/// <summary>
		/// Class of GuildMember
		/// </summary>
		public ClassId Class
		{
			get
			{
				if (m_chr != null)
				{
					return m_chr.Class;
				}
				return (ClassId)_class;
			}
			internal set { _class = (int)value; }
		}

		public GuildRank Rank
		{
			get { return Guild.Ranks[RankId]; }
		}

		public int RankId
		{
			get { return _rankId; }
			set
			{
				_rankId = value;

				if (m_chr != null)
				{
					m_chr.GuildRank = (uint)value;
					foreach (var right in m_chr.GuildMember.Rank.BankTabRights.Where(right => right != null))
					{
						right.GuildRankId = (uint)value;
					}
				}

				SaveLater();
			}
		}

		/// <summary>
		/// Returns the Character or null, if this member is offline
		/// </summary>
		public Character Character
		{
			get { return m_chr; }
			internal set
			{
				m_chr = value;

				if (m_chr != null)
				{
					_name = m_chr.Name;
					m_chr.GuildMember = this;
				}
			}
		}

		public uint BankMoneyWithdrawlAllowance
		{
			get
			{
				if (IsLeader)
				{
					return GuildMgr.UNLIMITED_BANK_MONEY_WITHDRAWL;
				}

				if (DateTime.Now >= BankMoneyAllowanceResetTime)
				{
					BankMoneyAllowanceResetTime = DateTime.Now.AddHours(GuildMgr.BankMoneyAllowanceResetDelay);
					_remainingMoneyAllowance = (int)Rank.DailyBankMoneyAllowance;
				}
				return (uint)_remainingMoneyAllowance;
			}
			set
			{
				_remainingMoneyAllowance = (int)value;
			}
		}

		public DateTime BankMoneyAllowanceResetTime
		{
			get { return _moneyAllowanceResetTime; }
			set { _moneyAllowanceResetTime = value; }
		}

		public bool IsLeader
		{
			get
			{
				return Guild.Leader == this;
			}
		}
		#endregion

		#region Constructors
		internal GuildMember()
		{
		}
		#endregion

		#region Methods
		internal void Init(Guild guild, Character chr)
		{
			Guild = guild;
			Character = chr;
		}

		/// <summary>
		/// Removes this member from its Guild
		/// </summary>
		public void LeaveGuild()
		{
			Guild.RemoveMember(this, true);
		}

		public bool HasBankTabRight(int tabId, GuildBankTabPrivileges privilege)
		{
			if (Rank == null || Rank.BankTabRights[tabId] == null)
				return false;

			return Rank.BankTabRights[tabId].Privileges.HasAnyFlag(privilege);
		}


		public override string ToString()
		{
			return "GuildMember: " + Name;
		}

		public bool HasRight(GuildPrivileges privilege)
		{
			return IsLeader || Rank.Privileges.HasAnyFlag(privilege);
		}

		public void SaveLater()
		{
			RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(this);
		}
		#endregion

	}
}