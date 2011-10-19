using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NHibernate.Criterion;
using NLog;
using WCell.AuthServer.Database;
using WCell.Constants;
using WCell.Core.Cryptography;
using WCell.Core.Initialization;
using WCell.Core.Timers;
using WCell.Util;
using WCell.Util.NLog;
using resources = WCell.AuthServer.Res.WCell_AuthServer;

namespace WCell.AuthServer.Accounts
{
	// TODO: Add contracts for all props

	/// <summary>
	/// Use this class to retrieve Accounts.
	/// Caching can be specified in the Config. 
	/// If activated, Accounts will be cached and retrieved instantly
	/// instead of querying them from the server.
	/// Whenever accessing any of the 2 Account collections,
	/// make sure to also synchronize against the <c>Lock</c>.
	/// </summary>
	public class AccountMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public static AccountMgr Instance = new AccountMgr();

		public static int MinAccountNameLen = 3;

		public static int MaxAccountNameLen = 20;

		/// <summary>
		/// Is called everytime, Accounts are (re-)fetched from DB (if caching is used)
		/// </summary>
		public event Action AccountsResync;

		public static readonly Account[] EmptyAccounts = new Account[0];

		ReaderWriterLockWrapper m_lock;
		readonly Dictionary<long, Account> m_cachedAccsById;
		readonly Dictionary<string, Account> m_cachedAccsByName;
		bool m_IsCached;
		private DateTime m_lastResyncTime;

		/// <summary>
		/// Interval in milliseconds between reloading the account cache from the database
		/// if caching is enabled. Default is 180000ms == 3 minutes.
		/// </summary>
		public static int AccountReloadIntervalMs
		{
			set
			{
				_accountReloadIntervalMs = value;

				if (Instance._accountsReloadTimer == null)
					return;

				Instance._accountsReloadTimer.IntervalMillis = value;
				Instance._accountsReloadTimer.Start();
			}
			
			get { return _accountReloadIntervalMs; }
		}

		private static int _accountReloadIntervalMs = 180000;

		private TimerEntry _accountsReloadTimer;

		protected AccountMgr()
		{
			m_cachedAccsById = new Dictionary<long, Account>();
			m_cachedAccsByName = new Dictionary<string, Account>(StringComparer.InvariantCultureIgnoreCase);
		}

		#region Properties
		/// <summary>
		/// All cached Accounts by Id or null if not cached.
		/// </summary>
		public Dictionary<long, Account> AccountsById
		{
			get
			{
				return m_cachedAccsById;
			}
		}

		/// <summary>
		/// All cached Accounts by Name or null if not cached.
		/// </summary>
		public Dictionary<string, Account> AccountsByName
		{
			get
			{
				return m_cachedAccsByName;
			}
		}

		/// <summary>
		/// The count of all Accounts
		/// </summary>
		public int Count
		{
			get
			{
				return m_IsCached ? m_cachedAccsById.Count : Account.GetCount();
			}
		}

		/// <summary>
		/// The lock of the Account Manager.
		/// Make sure to use it when accessing, reading or writing any of the two cached collections.
		/// </summary>
		//public ReaderWriterLockWrapper Lock
		//{
		//    get
		//    {
		//        return m_lock;
		//    }
		//}

		/// <summary>
		/// Whether all Accounts are cached.
		/// Setting this value will correspondingly
		/// activate or deactivate caching.
		/// </summary>
		public bool IsCached
		{
			get
			{
				return m_IsCached;
			}
			set
			{
				if (m_IsCached != value)
				{
					if (value)
					{
						Cache();
					}
					else
					{
						Purge();
					}
					m_IsCached = value;
				}
			}
		}
		#endregion

		public void ForeachAccount(Action<Account> action)
		{
			using (m_lock.EnterReadLock())
			{
				foreach (var acc in AccountsById.Values)
				{
					action(acc);
				}
			}
		}

		public IEnumerable<Account> GetAccounts(Predicate<Account> predicate)
		{
			using (m_lock.EnterReadLock())
			{
				foreach (var acc in AccountsById.Values)
				{
					if (predicate(acc))
					{
						yield return acc;
					}
				}
			}
		}

		#region Caching/Purging
		private void Cache()
		{
			log.Info(resources.CachingAccounts);
			m_lock = new ReaderWriterLockWrapper();
			m_lastResyncTime = default(DateTime);

			Resync();
		}

		private void Purge()
		{
			using (m_lock.EnterWriteLock())
			{
				m_cachedAccsById.Clear();
				m_cachedAccsByName.Clear();
			}
		}

		/// <summary>
		/// Purge and re-cache everything again
		/// </summary>
		public void ResetCache()
		{
			Purge();
			Cache();
		}

		internal void Remove(Account acc)
		{
			using (m_lock.EnterWriteLock())
			{
				RemoveUnlocked(acc);
			}
		}

		private void RemoveUnlocked(Account acc)
		{
			m_cachedAccsById.Remove(acc.AccountId);
			m_cachedAccsByName.Remove(acc.Name);
		}

		/// <summary>
		/// Reloads all account-data
		/// </summary>
		public void Resync()
		{
			var lastTime = m_lastResyncTime;
			m_lastResyncTime = DateTime.Now;

			Account[] accounts = null;
			try
			{
				using (m_lock.EnterWriteLock())
				{
					//if (lastTime == default(DateTime))
					//{
					//    m_cachedAccsById.Clear();
					//    m_cachedAccsByName.Clear();
					//    accounts = Account.FindAll();
					//}
					//else
					//{
					//    accounts = Account.FindAll(Expression.Ge("LastChanged", lastTime));
					//}
					m_cachedAccsById.Clear();
					m_cachedAccsByName.Clear();
					accounts = Account.FindAll();
				}
			}
			catch (Exception e)
			{
#if DEBUG
				AuthDBMgr.OnDBError(e);
				accounts = Account.FindAll();
#else
				throw e;
#endif
			}
			finally
			{
				if (accounts != null)
				{
					// remove accounts
					var toRemove = new List<Account>(5);
					foreach (var acc in m_cachedAccsById.Values)
					{
						if (!accounts.Contains(acc))
						{
							toRemove.Add(acc);
						}
					}
					foreach (var acc in toRemove)
					{
						RemoveUnlocked(acc);
					}

					// update existing accounts
					foreach (var acc in accounts)
					{
						Update(acc);
					}
				}
			}

			log.Info(resources.AccountsCached, accounts != null ? accounts.Count() : 0);

			var evt = AccountsResync;
			if (evt != null)
			{
				evt();
			}
		}

		private void Update(Account acc)
		{
			Account oldAcc;
			if (!m_cachedAccsById.TryGetValue(acc.AccountId, out oldAcc))
			{
				m_cachedAccsById[acc.AccountId] = acc;
				m_cachedAccsByName[acc.Name] = acc;
			}
			else
			{
				oldAcc.Update(acc);
			}
		}
		#endregion

		#region Creation
		/// <summary>
		/// Creates a game account.
		/// Make sure that the Account-name does not exist before calling this method.
		/// </summary>
		/// <param name="username">the username of the account</param>
		/// <param name="passHash">the hashed password of the account</param>
		public Account CreateAccount(string username,
			byte[] passHash, string email, string privLevel, ClientId clientId)
		{
			try
			{
				var usr = new Account(
					username,
					passHash,
					email) {
					ClientId = clientId,
					RoleGroupName = privLevel,
					Created = DateTime.Now,
					IsActive = true
				};

				try
				{
					usr.CreateAndFlush();
				}
				catch (Exception e)
				{
#if DEBUG
					AuthDBMgr.OnDBError(e);
					usr.CreateAndFlush();
#else
					throw e;
#endif
				}

				if (IsCached)
				{
					using (m_lock.EnterWriteLock())
					{
						Update(usr);
					}
				}

				log.Info(resources.AccountCreated, username, usr.RoleGroupName);
				return usr;
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, resources.AccountCreationFailed, username);
			}
			return null;
		}

		/// <summary>
		/// Creates a game account. 
		/// Make sure that the Account-name does not exist before calling this method.
		/// </summary>
		/// <param name="username">the username of the account</param>
		/// <param name="password">the plaintextpassword of the account</param>
		public Account CreateAccount(string username, string password,
			string email, string privLevel, ClientId clientId)
		{
			// Account-names are always upper case
			username = username.ToUpper();

			var passHash = SecureRemotePassword.GenerateCredentialsHash(username, password.ToUpper());

			return CreateAccount(username, passHash, email, privLevel, clientId);
		}
		#endregion

		#region Retrieving
		/// <summary>
		/// Checks to see if an account already exists.
		/// </summary>
		/// <returns>true if the account exists; false otherwise</returns>
		public static bool DoesAccountExist(string accName)
		{
			var serv = Instance;
			if (serv.IsCached)
			{
				using (serv.m_lock.EnterReadLock())
				{
					return serv.m_cachedAccsByName.ContainsKey(accName);
				}
			}

			return Account.Exists((ICriterion)Restrictions.InsensitiveLike("Name", accName, MatchMode.Exact));
		}

		public static Account GetAccount(string accountName)
		{
			return Instance[accountName];
		}

		public static Account GetAccount(long uid)
		{
			return Instance[uid];
		}

		public Account this[string accountName]
		{
			get
			{
				if (IsCached)
				{
					using (m_lock.EnterReadLock())
					{
						Account acc;
						m_cachedAccsByName.TryGetValue(accountName, out acc);
						return acc;
					}
				}
				return Account.FindOne(Restrictions.Eq("Name", accountName));
			}
		}

		public Account this[long id]
		{
			get
			{
				if (IsCached)
				{
					using (m_lock.EnterReadLock())
					{
						Account acc;
						m_cachedAccsById.TryGetValue(id, out acc);
						return acc;
					}

				}
				try
				{
					return Account.FindOne(Restrictions.Eq("AccountId", id));
				}
				catch (Exception e)
				{
					AuthDBMgr.OnDBError(e);
					return Account.FindOne(Restrictions.Eq("AccountId", id));
				}
			}
		}
		#endregion

		#region Initialize/Start/Stop
		[Initialization(InitializationPass.Fifth, "Initialize Accounts")]
		public static bool Initialize()
		{
			return Instance.Start();
		}

		protected bool Start()
		{
			try
			{
				IsCached = AuthServerConfiguration.CacheAccounts;

				//I would have liked this to be a readonly field but it must be
				//initialised here otherwise in the ctor AccountReloadIntervalMs
				//wont have been init'd which would mean we cant customise the timer easily
				_accountsReloadTimer = new TimerEntry(0, AccountReloadIntervalMs, delay =>
				{
					if (Instance.IsCached)
						Instance.Resync();
				});
				_accountsReloadTimer.Start();
				AuthenticationServer.IOQueue.RegisterUpdatable(_accountsReloadTimer);

				if (Count == 0)
				{
					log.Info("Detected empty Account-database.");
					//if (!DoesAccountExist("Administrator"))
					//{
					//    CreateAccount("Administrator", DefaultAdminPW, null, RoleGroupInfo.HighestRole.Name, ClientId.Wotlk);
					//    log.Warn("Created new Account \"Administrator\" with same password.");
					//}
				}
			}
			catch (Exception e)
			{
				AuthDBMgr.OnDBError(e);
			}
			return true;
		}

		protected bool Stop()
		{
			_accountsReloadTimer.Stop();
			AuthenticationServer.IOQueue.UnregisterUpdatable(_accountsReloadTimer);
			return true;
		}
		#endregion

		/// <summary>
		/// Validates the name against the stored Regex <see cref="DefaultNameValidationRegex"/>
		/// </summary>
		/// <param name="name">The name to be validated</param>
		/// <returns>A Boolean value true is the name is valid; otherwise false</returns>
		public static bool ValidateNameDefault(ref string name)
		{
			// Account-names are always upper case
			name = name.Trim().ToUpper();

			if (name.Length >= MinAccountNameLen && name.Length <= MaxAccountNameLen)
			{
				return DefaultNameValidationRegex.IsMatch(name);
			}
			return false;
		}

		public delegate bool NameValidationHandler(ref string name);
		public static readonly Regex DefaultNameValidationRegex = new Regex("^[A-Za-z0-9]+$");
		public static NameValidationHandler NameValidator = ValidateNameDefault;
	}
}