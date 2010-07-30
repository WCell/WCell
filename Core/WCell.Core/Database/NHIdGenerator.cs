using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.ActiveRecord.Queries;
using WCell.Core.Initialization;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// Gives out next Primary Key for a table with assigned Primary Keys
	/// </summary>
	public class NHIdGenerator
	{
		private static readonly List<NHIdGenerator> _creators = new List<NHIdGenerator>();
		private static bool _DBInitialized;
		private static Action<Exception> OnError;

		public static void InitializeCreators(Action<Exception> onError)
		{
			OnError = onError;
			foreach (var creator in _creators)
			{
				creator.Init();
			}

			_DBInitialized = true;
		}

		private string m_table, m_idMember;
		private Type m_type;
		private long m_highestId, m_minId;

		public NHIdGenerator(Type type, string idMember, long minId = 1)
			: this(type, idMember, type.Name, minId)
		{
		}

		public NHIdGenerator(Type type, string idMember, string tableName, long minId = 1)
		{
			m_type = type;
			m_table = tableName;
			m_idMember = idMember;
			m_minId = minId;
			if (_DBInitialized)
			{
				Init();
			}
			else
			{
				_creators.Add(this);
			}
		}

		private void Init()
		{
			var str = string.Format("SELECT max(r.{0}) FROM {1} r", m_idMember, m_table);
			var query = new ScalarQuery<object>(m_type, str);
			object highestId;
			try
			{
				highestId = query.Execute();
			}
			catch (Exception e)
			{
				OnError(e);
				highestId = query.Execute();
			}

			if (highestId == null)
			{
				m_highestId = 0;
			}
			else
			{
				m_highestId = (long)Convert.ChangeType(highestId, typeof(long));
			}

			if (m_highestId < m_minId)
			{
				m_highestId = m_minId;
			}
		}

		public long LastId
		{
			get { return Interlocked.Read(ref m_highestId); }
		}

		public long Next()
		{
			return Interlocked.Increment(ref m_highestId);
		}
	}
}