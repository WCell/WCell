using System;
using WCell.Database;
using WCell.Core.Initialization;
using WCell.Util.Logging;
using WCell.Util.Variables;

namespace WCell.RealmServer.Database
{
	/// <summary>
	/// TODO: Add method and command to re-create entire DB and resync server correspondingly
	/// </summary>
	[GlobalMgr]
	public static class RealmWorldDBMgr
	{
		public static string DefaultCharset = "UTF8";

		private static Logger log = LogManager.GetCurrentClassLogger();
		[NotVariable]
		public static bool Initialized;
        public static DatabaseProvider DatabaseProvider;

		public static void OnDBError(Exception e)
		{
            log.Error("Database Error " + e.Message + " " + e.Source + " " + e.StackTrace, e);
			//DatabaseUtil.OnDBError(e, "This will erase all Characters!");
		}

		[Initialization(InitializationPass.First, "Initialize database")]
		public static bool Initialize()
		{
		    if (!Initialized)
		    {
		        Initialized = true;
		        DatabaseProvider = new DatabaseProvider(RealmServerConfiguration.DBWorldConnectionString);
		    }
		    //DatabaseUtil.DBErrorHook = exception => CharacterRecord.GetCount() < 100;

				//DatabaseUtil.DBType = RealmServerConfiguration.DBType;
				//DatabaseUtil.ConnectionString = RealmServerConfiguration.DBConnectionString;
				//DatabaseUtil.DefaultCharset = DefaultCharset;

                /*
				var asm = typeof(RealmDBMgr).Assembly;

				try
				{
					if (!DatabaseUtil.InitAR(asm))
					{
						return false;
					}
				}
				catch (Exception e)
				{
					// repeat init
					OnDBError(e);
					try
					{
						if (!DatabaseUtil.InitAR(asm))
						{
							return false;
						}
					}
					catch (Exception e2)
					{
						LogUtil.ErrorException(e2, true, "Failed to initialize the Database.");
					}
				}
			}

			// Create tables if not already existing:
			// NHibernate wraps up all added Persistors once the first connection to the DB is established
			// which again happens during the first query to the DB - The line to check for any existing Characters.
			
			// After the first query, you cannot register any further types.
			var count = 0;
			try
			{
				count = CharacterRecord.GetCount();
			}
			catch
			{
			}

			if (count == 0)
			{
				// in case that the CharacterRecord table does not exist -> Recreate schema
				DatabaseUtil.CreateSchema();
			}*/

			//NHIdGenerator.InitializeCreators(OnDBError);

            RealmServer.InitMgr.SignalGlobalMgrReady(typeof(RealmWorldDBMgr));
			return true;
		}
	}
}