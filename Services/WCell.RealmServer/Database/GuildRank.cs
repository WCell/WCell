using System;
using System.Linq;
using System.Threading;
using WCell.Constants.Guilds;
using WCell.RealmServer.Database;
using WCell.Util.Threading;

namespace WCell.RealmServer.Guilds
{
	//[ActiveRecord("GuildRank", Access = PropertyAccess.Property)]
	public partial class GuildRank //: WCellRecord<GuildRank>
	{
		//private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(GuildRank), "_id");

		#region Id Generator
		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void Init()
		{
			//long highestId;
			try
			{
				_highestId = RealmWorldDBMgr.DatabaseProvider.Query<GuildRank>().Max(guildRank => guildRank._id);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				_highestId = RealmWorldDBMgr.DatabaseProvider.Query<GuildRank>().Max(guildRank => guildRank._id);
			}

			//_highestId = (long)Convert.ChangeType(highestId, typeof(long));

			_idGeneratorInitialised = true;
		}

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			if (!_idGeneratorInitialised)
				Init();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					Init();
				return Interlocked.Read(ref _highestId);
			}
		}
		#endregion

		//[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		private int _id
		{
			get;
			set;
		}

		//[Field("GuildId", NotNull = true)]
		private int m_GuildId;

		public uint GuildId
		{
			get { return (uint)m_GuildId; }
			set { m_GuildId = (int)value; }
		}

		//[Property(NotNull = true)]
		public string Name
		{
			get;
			set;
		}

		//[Property(NotNull = true)]
		public int RankIndex
		{
			get;
			set;
		}

		//[Field("Privileges", NotNull = true)]
		private int _privileges;

		//[Field("BankMoneyPerDay")]
		private int _moneyPerDay;

		private GuildBankTabRights[] m_BankTabRights;

		public GuildBankTabRights[] BankTabRights //TODO: When mapping, make this a reference
		{
			get { return m_BankTabRights; }
			set { m_BankTabRights = value; }
		}

		public GuildRank(Guild guild, string name, GuildPrivileges privileges, int id)
		{
			_id = (int)NextId();
			GuildId = (uint)guild.Id;	 //TODO: Conversion from int to uint, find a way to solve this
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
			return RealmWorldDBMgr.DatabaseProvider.Query<GuildRank>().Where(guildRank => guildRank.GuildId == guild.Id).ToArray(); // FindAllByProperty("m_GuildId", (int)guild.Id);
		}

		public void SaveLater()
		{
			RealmServer.IOQueue.AddMessage(() => { RealmWorldDBMgr.DatabaseProvider.Save(this); });
		}

		//TODO: Implement events

		//public void Create()
		//{
		//	RealmWorldDBMgr.DatabaseProvider.Save(this);
		//	OnCreated();
		//}

		//public void CreateAndFlush()
		//{
		//	RealmWorldDBMgr.DatabaseProvider.Save(this);
		//	OnCreated();
		//}

		//private void OnCreated()
		//{
		//	foreach (var right in m_BankTabRights)
		//	{
		//		RealmWorldDBMgr.DatabaseProvider.Save(right);
		//	}
		//}

		//protected void OnDelete()
		//{
		//	base.OnDelete();
		//	foreach (var right in m_BankTabRights)
		//	{
		//		RealmWorldDBMgr.DatabaseProvider.Delete(right);
		//	}
		//}

		/// <summary>
		/// Init a loaded Rank
		/// </summary>
		internal void InitRank()
		{
			m_BankTabRights = RealmWorldDBMgr.DatabaseProvider.Query<GuildBankTabRights>().Where(guildBankTabRights => guildBankTabRights.GuildRankId == _id).ToArray(); //GuildBankTabRights.FindAllByProperty("m_GuildRankId", _id);
			var count = m_BankTabRights.Length;
			Array.Resize(ref m_BankTabRights, GuildMgr.MAX_BANK_TABS);
			for (var i = count; i < m_BankTabRights.Length; i++)
			{
				m_BankTabRights[i] = new GuildBankTabRights(i, (uint)_id);
			}
		}
	}
}