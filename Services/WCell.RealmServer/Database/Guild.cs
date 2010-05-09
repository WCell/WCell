using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.NLog;

namespace WCell.RealmServer.Guilds
{
	[ActiveRecord("Guild", Access = PropertyAccess.Property)]
	public partial class Guild : ActiveRecordBase<Guild>
	{
		private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(Guild), "_id");

		[PrimaryKey(PrimaryKeyType.Assigned, "Id")]
		private long _id
		{
			get;
			set;
		}

		[Field("Name", NotNull = true, Unique = true)]
		private string _name;

		[Field("MOTD", NotNull = true)]
		private string _MOTD;

		[Field("Info", NotNull = true)]
		private string _info;

		[Field("Created", NotNull = true)]
		private DateTime _created;

		[Field("LeaderLowId", NotNull = true)]
		private int _leaderLowId;

		public uint LeaderLowId
		{
			get { return (uint)_leaderLowId; }
		}

		[Nested("Tabard")]
		private GuildTabard _tabard
		{
			get;
			set;
		}

		[Property]
		public int PurchasedBankTabCount
		{
			get;
			internal set;
		}

		[Property]
		public long Money
		{
			get;
			set;
		}

		public override void Save()
		{
			base.Update();
		}

		public override void SaveAndFlush()
		{
			UpdateAndFlush();
		}

		public void UpdateLater()
		{
			RealmServer.Instance.AddMessage(Update);
		}

		public void UpdateLater(Action triggerAction)
		{
			triggerAction();
			RealmServer.Instance.AddMessage(Update);
		}

		public override void Create()
		{
			try
			{
				base.Create();
				OnCreate();
			}
			catch (Exception e)
			{
				RealmDBUtil.OnDBError(e);
			}
		}

		public override void CreateAndFlush()
		{
			try
			{
				base.CreateAndFlush();
				OnCreate();
			}
			catch (Exception e)
			{
				RealmDBUtil.OnDBError(e);
			}
		}

		private void OnCreate()
		{
			m_syncRoot.Enter();
			try
			{
				foreach (var rank in m_ranks)
				{
					rank.Create();
				}
				foreach (var member in Members.Values)
				{
					member.Create();
				}
				foreach (var tab in Bank.BankTabs)
				{
					tab.Create();
				}
			}
			finally
			{
				m_syncRoot.Exit();
			}
		}

		protected override void OnDelete()
		{
			base.OnDelete();

			m_syncRoot.Enter();
			try
			{
				GuildEventLogEntry.DeleteAll("GuildId = " + Id);
				foreach (var rank in m_ranks)
				{
					rank.Delete();
				}
				foreach (var tab in Bank.BankTabs)
				{
					tab.Delete();
				}
			}
			finally
			{
				m_syncRoot.Exit();
			}
		}
	}
}