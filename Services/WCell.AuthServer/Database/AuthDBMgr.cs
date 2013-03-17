using System;
using WCell.AuthServer.Database.Entities;
using WCell.Core.Initialization;
using WCell.AuthServer.Accounts;
using WCell.Database;
using WCell.Util.Logging;

namespace WCell.AuthServer.Database
{
	public static class AuthDBMgr
	{
		public static string DefaultCharset = "UTF8";
		public static DatabaseProvider DatabaseProvider;

		private static Logger s_log = LogManager.GetCurrentClassLogger();

		public static void OnDBError(Exception e)
		{
			s_log.Error("Database Error " + e.Message + " " + e.Source + " " + e.StackTrace ,e);
			//DatabaseProvider.OnDBError(e, "This will erase all Accounts!");
		}

		[Initialization(InitializationPass.Third, "Initialize database")]
		public static bool Initialize()
		{
			//DatabaseUtil.DBErrorHook = exception => AccountMgr.Instance.Count < 100; //TODO: This is a horrible way to find if we're on production, we need something better than this

			//DatabaseUtil.DBType = AuthServerConfiguration.DBType;
			DatabaseProvider = new DatabaseProvider(AuthServerConfiguration.DBServer,AuthServerConfiguration.DBUsername,AuthServerConfiguration.DBPassword,AuthServerConfiguration.DBDatabase);
			//DatabaseUtil.DefaultCharset = DefaultCharset; TODO: Find if this is required anymore

			/*var asm = typeof(AuthDBMgr).Assembly;

			try
			{
				if (!DatabaseUtil.InitAR(asm))
				{
					return false;
				}
			}
			catch (Exception e)
			{
				OnDBError(e);

				// repeat init
				DatabaseUtil.InitAR(asm);
			}*/


			// create tables if not already existing
			/*var count = 0;
			try
			{
				count = Account.GetCount();
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
			
			return true;
		}
	}
}