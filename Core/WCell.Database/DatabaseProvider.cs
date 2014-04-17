using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;

namespace WCell.Database
{
	public class DatabaseProvider
	{
		public ISessionFactory SessionFactory;
		public ISession Session;
		public ITransaction Transaction;
		public Assembly SourceAssembly;
		public Configuration NHibernateConfiguration;
		public FluentConfiguration FluentNHibernateConfiguration;
        public delegate void DeleteEntityEventHandler(object sender, EventArgs e);
        public event DeleteEntityEventHandler OnDeleteEntity;
		public Dialect CurrentDialect
		{
			get { return Dialect.GetDialect(NHibernateConfiguration.Properties); }
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="server">The database server to which we are connecting</param>
		/// <param name="username">The username for the server connection</param>
		/// <param name="password">The password for the server connection</param>
		/// <param name="database">The database on the server which we will be dealing with</param>
		/// <param name="sourceAssembly">The assembly from which to load the tables, if not set defaults to calling assembly</param>
		public DatabaseProvider(string server, string username, string password, string database,
		                        Assembly sourceAssembly = null)
		{
			SourceAssembly = sourceAssembly ?? Assembly.GetCallingAssembly();
			SessionFactory = CreateSessionFactory(server, username, password, database);
			Session = SessionFactory.OpenSession();
			Transaction = Session.BeginTransaction();
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString">The connection string for the database server</param>
        /// <param name="sourceAssembly">The assembly from which to load the tables, if not set defaults to calling assembly</param>
        public DatabaseProvider(string connectionString,
                                Assembly sourceAssembly = null)
        {
            SourceAssembly = sourceAssembly ?? Assembly.GetCallingAssembly();
            SessionFactory = CreateSessionFactory(connectionString);
            Session = SessionFactory.OpenSession();
            Transaction = Session.BeginTransaction();
        }

		public class StoreConfiguration : DefaultAutomappingConfiguration
		{
			public override bool ShouldMap(Type type)
			{
				return type.GetCustomAttributes(typeof (ShouldAutoMap), false).Any(); //TODO: Find out if this will act as required
				//GetCustomAttribute(typeof (ShouldAutoMap)) != null; //TODO: Check if this is safe if the attrib is not found
			}
		}

		public ISessionFactory CreateSessionFactory(string server, string username, string password, string database)
		{
			var fluconf = Fluently.Configure();
			//TODO: Support other database connection types (cleanly)
			var dbconf = MySQLConfiguration.Standard.ConnectionString(builder =>
				{
					builder.Server(server);
					builder.Username(username);
					builder.Password(password);
					builder.Database(database);
				});
			fluconf.Database(dbconf);
		    return CreateSessionFactoryWithFluentConfiguration(fluconf);
		}

        public ISessionFactory CreateSessionFactory(string connectionString)
        {
            var fluconf = Fluently.Configure();
            //TODO: Support other database connection types (cleanly)
            var dbconf = MySQLConfiguration.Standard.ConnectionString(builder => builder.Is(connectionString));
            fluconf.Database(dbconf);
            return CreateSessionFactoryWithFluentConfiguration(fluconf);
        }

	    private ISessionFactory CreateSessionFactoryWithFluentConfiguration(FluentConfiguration fluentConfiguration)
	    {
	        var storeConfiguration = new StoreConfiguration(); //TODO: Hmm using Assembly as a property is kinda gash..
            fluentConfiguration.Mappings(m =>
	            {
	                m.FluentMappings.AddFromAssembly(SourceAssembly);
	                m.AutoMappings.Add(AutoMap.Assembly(SourceAssembly, storeConfiguration));
	            });
            FluentNHibernateConfiguration = fluentConfiguration.ExposeConfiguration(configuration =>
	            {
	                NHibernateConfiguration = configuration;
	                new SchemaUpdate(configuration).Execute(false, true);
	            });
            return fluentConfiguration.BuildSessionFactory();
	    }

	    public void CommitTransaction()
		{
			Transaction.Commit();
			Transaction = Session.BeginTransaction();
		}

		public T First<T>(Expression<Func<T, bool>> expression)
		{
			return FindFirst<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression)));
		}

		public List<T> GetWhere<T>(Expression<Func<T, bool>> expression)
		{
			return new List<T>(FindAll<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression))));
		}

		/// <summary>
		/// Retrieves the entity with the given id
		/// </summary>
		/// <param name="id"></param>
		/// <returns>the entity or null if it doesn't exist</returns>
		public T Get<T>(object id)
		{
			return Session.Get<T>(id);
		}

		/// <summary>
		/// Saves or updates the given entity
		/// </summary>
		/// <param name="entity"></param>
		public void SaveOrUpdate<T>(T entity)
		{
			Session.SaveOrUpdate(entity);
			CommitTransaction();
		}

		/// <summary>
		/// Saves the given entity
		/// </summary>
		/// <param name="entity"></param>
		public void Save<T>(T entity)
		{
			Session.Save(entity);
			CommitTransaction();
		}

		public void Update<T>(T entity)
		{
			Session.Update(entity);
			CommitTransaction();
		}

		public IEnumerable<T> FindAll<T>() where T : class
		{
			return Session.QueryOver<T>().List();
		}

        public IEnumerable<T> FindAll<T>(Expression<Func<T, bool>> expression)
        {
            return FindAll<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression)));
        }

		/// <summary>
		/// Returns each entity that matches the given criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public IEnumerable<T> FindAll<T>(DetachedCriteria criteria)
		{
			var results = criteria.GetExecutableCriteria(Session).List<T>();
			CommitTransaction();
			return results;
		}

		/// <summary>
		/// Returns each entity that maches the given criteria, and orders the results
		/// according to the given Orders
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="orders"></param>
		/// <returns></returns>
		public IEnumerable<T> FindAll<T>(DetachedCriteria criteria, params Order[] orders)
		{
			if (orders != null)
			{
				foreach (var order in orders)
				{
					criteria.AddOrder(order);
				}
			}

			return FindAll<T>(criteria);
		}

		/// <summary>
		/// Returns each entity that matches the given criteria, with support for paging,
		/// and orders the results according to the given Orders
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="firstResult"></param>
		/// <param name="numberOfResults"></param>
		/// <param name="orders"></param>
		/// <returns></returns>
		public IEnumerable<T> FindAll<T>(DetachedCriteria criteria, int firstResult, int numberOfResults,
		                                 params Order[] orders)
		{
			criteria.SetFirstResult(firstResult).SetMaxResults(numberOfResults);
			return FindAll<T>(criteria, orders);
		}

		/// <summary>
		/// Returns the one entity that matches the given criteria. Throws an exception if
		/// more than one entity matches the criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public T FindOne<T>(DetachedCriteria criteria)
		{
			return criteria.GetExecutableCriteria(Session).UniqueResult<T>();
		}

        public T FindOne<T>(Expression<Func<T, bool>> expression)
        {
            return FindOne<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression)));
        }

		public T FindOne<T>(SimpleExpression expression)
		{
			return FindOne<T>(DetachedCriteria.For<T>().Add(expression));
		}

		/// <summary>
		/// Returns the first entity to match the given criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public T FindFirst<T>(DetachedCriteria criteria)
		{
			var results = criteria.SetFirstResult(0).SetMaxResults(1)
			                      .GetExecutableCriteria(Session).List<T>();

			return results.Count > 0 ? results[0] : default(T);
		}

        public T FindFirst<T>(Expression<Func<T, bool>> expression)
        {
            return FindFirst<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression)));
        }

		/// <summary>
		/// Returns the total number of entities that match the given criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public long Count(DetachedCriteria criteria)
		{
			return Convert.ToInt64(criteria.GetExecutableCriteria(Session)
			                               .SetProjection(Projections.RowCountInt64()));
		}

		/// <summary>
		/// Returns the total number of entities that exist
		/// </summary>
		public int Count<T>() where T : class
		{
			return Session.QueryOver<T>()
			              .Select(Projections.Count(Projections.Id()))
			              .FutureValue<int>().Value;
		}

        /// <summary>
        /// Returns the total number of entities that match the given criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public long Count<T>(DetachedCriteria criteria) where T : class
        {
            var results = criteria.GetExecutableCriteria(Session).List<T>();

			return results.Count;
        }

        /// <summary>
        /// Returns the total number of entities that match the given criteria
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public long Count<T>(Expression<Func<T, bool>> expression) where T : class
        {
            return Count<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression)));
        }

        /// <summary>
        /// Returns the total number of entities that match the given criteria
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public long Count<T>(SimpleExpression expression) where T : class
        {
            return Session.QueryOver<T>().Where(expression)
                          .Select(Projections.Count(Projections.Id()))
                          .FutureValue<int>().Value;
        }

		/// <summary>
		/// Returns true if at least one entity exists that matches the given criteria
		/// </summary>
		/// <param name="criteria"></param>
		/// <returns></returns>
		public bool Exists(DetachedCriteria criteria)
		{
			return Count(criteria) > 0;
		}

		public bool Exists<T>(Expression<Func<T, bool>> expression)
		{
			return Count(DetachedCriteria.For<T>().Add(Restrictions.Where(expression))) > 0;
		}

		/// <summary>
		/// Deletes the given entity
		/// </summary>
		/// <param name="entity"></param>
		public bool Delete<T>(T entity)
		{
			try
			{
				Session.Delete(entity);
				CommitTransaction();
			}
			catch (Exception e) //TODO: Catch exception and log
			{
				return false;
			}

            OnDeleteEntity(this, null);
            return true;
		}

		/// <summary>
		/// Deletes every entity that matches the given criteria
		/// </summary>
		/// <param name="criteria"></param>
		public void Delete<T>(DetachedCriteria criteria)
		{
			// Note: If you use HQL then it won't honor the cascade operation either..
			// This could be much improved most likely, as currently 2 trips are required however for now it works.
			foreach (T entity in FindAll<T>(criteria))
			{
				Delete(entity);
			}
			CommitTransaction();
		}

        /// <summary>
        /// Deletes every entity that matches the given criteria
        /// </summary>
        /// <param name="expression"></param>
        public void Delete<T>(Expression<Func<T, bool>> expression)
        {
            // Note: If you use HQL then it won't honor the cascade operation either..
            // This could be much improved most likely, as currently 2 trips are required however for now it works.
            foreach (T entity in FindAll<T>(DetachedCriteria.For<T>().Add(Restrictions.Where(expression))))
            {
                Delete(entity);
            }
            CommitTransaction();
        }

		/*public void OnDBError(Exception exception, string thisWillEraseAllAccounts)
		{
		???
		}*/

		public void CreateSchema()
		{
			FluentNHibernateConfiguration.ExposeConfiguration(config => new SchemaExport(NHibernateConfiguration).Execute(false, true, false));
		}
	}
}