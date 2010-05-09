using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.Util.Threading;

namespace WCell.RealmServer.Guilds
{
	[ActiveRecord("GuildRank", Access = PropertyAccess.Property)]
	public partial class GuildRank : ActiveRecordBase<GuildRank>
	{
		private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildRank), "_id");

		[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		private int _id
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
		public string Name
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int RankIndex
		{
			get;
			set;
		}

		[Field("Privileges", NotNull = true)]
		private int _privileges;

		[Field("BankMoneyPerDay")]
		private int _moneyPerDay;

		private GuildBankTabRights[] m_BankTabRights;

		public GuildBankTabRights[] BankTabRights
		{
			get { return m_BankTabRights; }
			set { m_BankTabRights = value; }
		}

		public GuildRank(Guild guild, string name, GuildPrivileges privileges, int id)
		{
			_id = (int)_idGenerator.Next();
			GuildId = guild.Id;
			Name = name;
			Privileges = privileges;
			RankIndex = id;
			BankTabRights = new GuildBankTabRights[GuildMgr.MAX_BANK_TABS];
			for (var i = 0; i < GuildMgr.MAX_BANK_TABS; ++i)
			{
				BankTabRights[i] = new GuildBankTabRights(i, (uint)_id);
			}
		}

		public static GuildRank[] FindAll(Guild guild)
		{
			return FindAllByProperty("m_GuildId", (int)guild.Id);
		}

		public void SaveLater()
		{
			RealmServer.Instance.AddMessage(new Message(Update));
		}

		public override void Create()
		{
			base.Create();
			OnCreated();
		}

		public override void CreateAndFlush()
		{
			base.CreateAndFlush();
			OnCreated();
		}

		private void OnCreated()
		{
			foreach (var right in m_BankTabRights)
			{
				right.Create();
			}
		}

		protected override void OnDelete()
		{
			base.OnDelete();
			foreach (var right in m_BankTabRights)
			{
				right.Delete();
			}
		}

		/// <summary>
		/// Init a loaded Rank
		/// </summary>
		internal void InitRank()
		{
			m_BankTabRights = GuildBankTabRights.FindAllByProperty("m_GuildRankId", _id);
			var count = m_BankTabRights.Length;
			Array.Resize(ref m_BankTabRights, GuildMgr.MAX_BANK_TABS);
			for (var i = count; i < m_BankTabRights.Length; i++)
			{
				m_BankTabRights[i] = new GuildBankTabRights(i, (uint)_id);
			}
		}
	}
}
