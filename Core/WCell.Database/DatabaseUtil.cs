namespace WCell.Database
{
	/*/// <summary>
	/// Temporary class - Will need cleanup.
	/// </summary>
	public static class DatabaseUtil
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The TextReader from which to read Input
		/// </summary>
		public static SingleStringMover Input = new ConsoleStringMover();

		public static readonly Dictionary<Assembly, Type[]> Types = new Dictionary<Assembly, Type[]>();

		private static TextReader s_configReader;
		private static XmlConfigurationSource s_config;
		private static Dialect s_dialect;

		/// <summary>
		/// Is called when the DB creates an error and asks the User whether or not
		/// to auto-recreate the DB. Will not query the user and directly throw the Exception
		/// if false is returned (in order to avoid DB-deletion of production systems).
		/// </summary>
		public static Predicate<Exception> DBErrorHook;

		public static Dialect Dialect
		{
			get { return s_dialect; }
		}

		/// <summary>
		/// Whether it is currently waiting for user-input.
		/// </summary>
		public static bool IsWaiting
		{
			get;
			set;
		}

		public static HibernateCfg Config
		{
			get
			{
                if (Holder != null)
				{

                    return Holder.GetConfiguration(typeof(ActiveRecordBase));
				}
				return null;
			}
		}

		public static ISessionFactoryImplementor SessionFactory
		{
			get
			{
                if (Holder != null)
				{

                    return (ISessionFactoryImplementor)Holder.GetSessionFactory(typeof(ActiveRecordBase));
				}
				return null;
			}
		}

        private static ISessionFactoryHolder Holder
        {
            get
            {
                return ActiveRecordMediator.GetSessionFactoryHolder();
            }
        }

	    public static Settings Settings
		{
			get
			{
				return SessionFactory.Settings;
			}
		}

		public static bool IsConnected
		{
			get
			{
				var sess = Session;
				return sess != null && sess.IsConnected;
			}
		}

		public static ISessionImplementor Session
		{
			get
			{
                if (Holder != null)
				{

                    return (ISessionImplementor)Holder.CreateSession(typeof(ActiveRecordBase));
				}
				return null;
			}
		}

		public static string DBType
		{
			get;
			set;
		}

		public static string ConnectionString
		{
			get;
			set;
		}

		public static string DefaultCharset
		{
			get
			{
			    return "UTF8";// Settings.DefaultCharset;
			}
			set
			{
				//Settings.DefaultCharset = value;
			}
		}

		/// <summary>
		/// Console should not be read from anymore at this point.
		/// </summary>
		public static void ReleaseConsole()
		{
			Input = new SingleStringMover();
		}

		public static void OnDBError(Exception e, string warning)
		{
			try
			{
				// probably a production system - Don't drop.
				if (DBErrorHook != null && !DBErrorHook(e))
				{
					throw e;
				}
			}
			catch (Exception ex)
			{
				log.ErrorException("", ex);
			}

			var errMsg = "Database Error occured";
			LogUtil.ErrorException(e, false, errMsg);
			log.Warn("");
			foreach (var msg in e.GetAllMessages())
			{
				log.Warn(msg);
			}
			log.Warn("");
			log.Warn("Database could not be initialized!");
			log.Warn("Re-create Database schema? (y/n)");
			log.Warn("WARNING: " + warning);

			IsWaiting = true;
			bool doDrop;
			try
			{
				doDrop = StringStream.GetBool(Input.Read());
			}
			catch
			{
				// no Console available (running Tests etc)
				doDrop = true;
			}
			IsWaiting = false;

			if (doDrop)
			{
				log.Warn("Dropping database schema...");
				DropSchema();
				log.Warn("Done.");

				log.Warn("Re-creating database schema...");
				try
				{
					CreateSchema();
				}
				catch (Exception ex)
				{
					// damn it! No console output...
					throw new InvalidOperationException("", ex);
				}
				log.Warn("Done.");
			}
			else
			{
				throw new InvalidOperationException("", e);
			}
		}

		/// <summary>
		/// Called to initialize setup NHibernate and ActiveRecord
		/// </summary>
		/// <param name="asm"></param>
		/// <param name="dbType"></param>
		/// <param name="connStr"></param>
		/// <returns>Whether its a fatal error</returns>
		public static bool InitAR(Assembly asm)
		{
			if (s_configReader == null)
			{
				s_configReader = DatabaseConfiguration.GetARConfiguration(DBType, ConnectionString);

				if (s_configReader == null)
				{
					throw new Exception("Invalid Database Type: " + DBType);
				}
			}

			s_config = new XmlConfigurationSource(s_configReader);
			
			NHibernate.Cfg.Environment.UseReflectionOptimizer = true;

			ActiveRecordStarter.Initialize(asm, s_config);
			if (!IsConnected)
			{
				throw new Exception(string.Format("Failed to connect to Database."));
			}

			s_dialect = Dialect.GetDialect(Config.Properties) ?? Dialect.GetDialect();

			return true;
		}

		/// <summary>
		/// (Drops and re-)creates the Schema of all tables that this has originally initialized with.
		/// </summary>
		public static void CreateSchema()
		{
			ActiveRecordStarter.CreateSchema();
		}

		/// <summary>
		/// Drops the Schema of all tables that this has originally initialized with
		/// </summary>
		public static void DropSchema()
		{
			ActiveRecordStarter.DropSchema();
		}

		#region Quotes
		public static string ToSqlValueString(string str)
		{
			return "'" + str + "'";
		}
		#endregion
	}

	public class ConsoleStringMover : SingleStringMover
	{
		public override string Read()
		{
			return Console.In.ReadLine();
		}

		public override void Write(string s)
		{
			throw new Exception("Cannot write to ConsoleStringMover");
		}
	}

	public class SingleStringMover
	{
		SynchronizedQueue<string> q = new SynchronizedQueue<string>();

		public virtual string Read()
		{
			while (q.Count == 0)
			{
				lock (q)
				{
					Monitor.Wait(q);
				}
			}
			return q.Dequeue();
		}

		public virtual void Write(string s)
		{
			q.Enqueue(s);
			lock (q)
			{
				Monitor.Pulse(q);
			}
		}
	}*/
}