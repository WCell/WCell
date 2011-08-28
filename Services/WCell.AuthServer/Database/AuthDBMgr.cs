using System;
using NLog;
using WCell.AuthServer.Accounts;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;

namespace WCell.AuthServer.Database
{
	public static class AuthDBMgr
	{
		public static string DefaultCharset = "UTF8";

		private static Logger s_log = LogManager.GetCurrentClassLogger();

		public static void OnDBError(Exception e)
		{
			DatabaseUtil.OnDBError(e, "This will erase all Accounts!");
		}

		[Initialization(InitializationPass.Third, "Initialize database")]
		public static bool Initialize()
		{
			DatabaseUtil.DBErrorHook = exception => AccountMgr.Instance.Count < 100;

			DatabaseUtil.DBType = AuthServerConfiguration.DBType;
			DatabaseUtil.ConnectionString = AuthServerConfiguration.DBConnectionString;
			DatabaseUtil.DefaultCharset = DefaultCharset;

			var asm = typeof(AuthDBMgr).Assembly;

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
			}


			// create tables if not already existing
			var count = 0;
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
			}

			NHIdGenerator.InitializeCreators(OnDBError);

			return true;
		}
	}
}