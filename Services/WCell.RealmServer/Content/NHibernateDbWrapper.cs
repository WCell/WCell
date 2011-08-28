using System;
using System.Data;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using WCell.Core.Database;
using WCell.Util.DB;

namespace WCell.RealmServer.Content
{
	/// <summary>
	/// <see cref="AbstractEntityPersister"/>
	/// <see cref="SingleTableEntityPersister"/>
	/// </summary>
	public class NHibernateDbWrapper : IDbWrapper
	{
		public static readonly SqlType[] EmptySqlTypeArr = new SqlType[0];

		private ISessionFactoryImplementor m_factory;
		private ISessionImplementor m_session;
		private IDbCommand[] m_selectCommands;

		/// <summary>
		/// Make sure to initialize ActiveRecord before calling this ctor
		/// </summary>
		public NHibernateDbWrapper()
		{
			//m_factory = (ISessionFactoryImplementor)ActiveRecordBase.Holder.GetSessionFactory(typeof(ActiveRecordBase));
			//m_session = (ISessionImplementor)ActiveRecordBase.Holder.CreateSession(typeof(ActiveRecordBase));
			m_factory = DatabaseUtil.SessionFactory;
			m_session = DatabaseUtil.Session;
			if (m_factory == null || m_session == null)
			{
				throw new InvalidOperationException("ActiveRecord was not initialized.");
			}
		}

		public void Prepare(LightDBMapper mapper)
		{
			var tables = mapper.Mapping.TableDefinitions;
			m_selectCommands = new IDbCommand[tables.Length];
			for (var i = 0; i < tables.Length; i++)
			{
				var table = tables[i];
				var cmd = CreateCommand(SqlUtil.BuildSelect(table.AllColumns, table.Name)
					//+ " ORDER BY " + table.PrimaryColumns[0].Name
						);
				//tables[i].QueryString, emptySqlTypeArr);
				m_selectCommands[i] = cmd;
			}
		}

		public IDataReader CreateReader(TableDefinition def, int tableIndex)
		{
			return m_session.Batcher.ExecuteReader(m_selectCommands[tableIndex]);
		}

		public IDataReader Query(string query)
		{
			return Query(new SqlString(query));
		}

		public void Insert(KeyValueListBase list)
		{
			ExecuteComand(SqlUtil.BuildInsert(list));
		}

		public void Update(UpdateKeyValueList list)
		{
			ExecuteComand(SqlUtil.BuildUpdate(list));
		}

		public void Delete(KeyValueListBase list)
		{
			ExecuteComand(SqlUtil.BuildDelete(list.TableName, SqlUtil.BuildWhere(list.Pairs)));
		}

		public IDataReader Query(SqlString query)
		{
			var cmd = m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text, query, EmptySqlTypeArr);
			return m_session.Batcher.ExecuteReader(cmd);
		}

		public IDbCommand CreateCommand(string sql)
		{
			return m_factory.ConnectionProvider.Driver.GenerateCommand(CommandType.Text,
			                                                    new SqlString(sql), EmptySqlTypeArr);
		}

		public void ExecuteComand(string sql)
		{
			var cmd = CreateCommand(sql);
			m_session.Batcher.ExecuteNonQuery(cmd);
		}

        /// <summary>
        /// Should return a version string in the format of a float.
        /// </summary>
		public string GetDatabaseVersion(string tableName, string columnName)
		{
			var reader = Query(SqlUtil.BuildSelect(new[] { columnName }, tableName));
			reader.Read();
			return reader.GetValue(0).ToString();
		}
	}
}