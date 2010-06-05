using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Criterion;
using NLog;
using WCell.AuthServer.Database;
using WCell.AuthServer.Res;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Initialization;
using WCell.Intercommunication.DataTypes;
using WCell.Util.NLog;

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
	public class AccountMgr : Manager<AccountMgr>
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public static int MinAccountNameLen = 3;

		public static int MaxAccountNameLen = 20;

		/// <summary>
		/// Is called everytime, Accounts are (re-)fetched from DB (if caching is used)
		/// </summary>
		public event Action AccountsResync;

		public static readonly Account[] EmptyAccounts = new Account[0];

		new ReaderWriterLockSlim m_lock;
		readonly Dictionary<long, Account> m_cachedAccsById;
		readonly Dictionary<string, Account> m_cachedAccsByName;
		bool m_IsCached;
		private DateTime m_lastResyncTime;

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
		public ReaderWriterLockSlim Lock
		{
			get
			{
				return m_lock;
			}
		}

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

		#region Caching/Purging
		private void Cache()
		{
			log.Info(resources.CachingAccounts);
			m_lock = new ReaderWriterLockSlim();
			m_lastResyncTime = default(DateTime);

			Resync();
		}

		private void Purge()
		{
			m_lock.EnterWriteLock();
			try
			{
				m_cachedAccsById.Clear();
				m_cachedAccsByName.Clear();
			}
			finally
			{
				m_lock.ExitWriteLock();
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
			m_lock.EnterWriteLock();
			try
			{
				RemoveUnlocked(acc);
			}
			finally
			{
				m_lock.ExitWriteLock();
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
			m_lock.EnterWriteLock();

			var lastTime = m_lastResyncTime;
			m_lastResyncTime = DateTime.Now;

			Account[] accounts = null;
			try
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
			catch (Exception e)
			{
#if DEBUG
				AuthDBUtil.OnDBError(e);
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
				m_lock.ExitWriteLock();
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
					AuthDBUtil.OnDBError(e);
					usr.CreateAndFlush();
#else
					throw e;
#endif
				}

				if (IsCached)
				{
					m_lock.EnterWriteLock();
					try
					{
						Update(usr);
					}
					finally
					{
						m_lock.ExitWriteLock();
					}
				}

				s_log.Info(resources.AccountCreated, username, usr.RoleGroupName);
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
				serv.m_lock.EnterReadLock();
				try
				{
					return serv.m_cachedAccsByName.ContainsKey(accName);
				}
				finally
				{
					serv.m_lock.ExitReadLock();
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
					m_lock.EnterReadLock();
					try
					{
						Account acc;
						m_cachedAccsByName.TryGetValue(accountName, out acc);
						return acc;
					}
					finally
					{
						m_lock.ExitReadLock();
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
					m_lock.EnterReadLock();
					try
					{
						Account acc;
						m_cachedAccsById.TryGetValue(id, out acc);
						return acc;
					}
					finally
					{
						m_lock.ExitReadLock();
					}

				}
				try
				{
					return Account.FindOne(Restrictions.Eq("AccountId", id));
				}
				catch (Exception e)
				{
					AuthDBUtil.OnDBError(e);
					return Account.FindOne(Restrictions.Eq("AccountId", id));
				}
			}
		}
		#endregion

		#region Initialize/Start/Stop
		[Initialization(InitializationPass.Fifth, "Initialize Accounts")]
		public static void Initialize()
		{
			Instance.InternalStart();
		}

		protected override bool InternalStart()
		{
			try
			{
				IsCached = AuthServerConfiguration.CacheAccounts;

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
				AuthDBUtil.OnDBError(e);
			}
			return true;
		}

		protected override bool InternalStop()
		{
			return true;
		}
		#endregion

		/// <summary>
		/// TODO: Improve name-verification
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool ValidateNameDefault(ref string name)
		{
			// Account-names are always upper case
			name = name.Trim().ToUpper();

			if (name.Length >= MinAccountNameLen && name.Length <= MaxAccountNameLen)
			{
				foreach (var c in name)
				{
					if (c < '0' || c > 'z')
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public delegate bool NameValidationHandler(ref string name);

		public static NameValidationHandler NameValidator = ValidateNameDefault;
	}
}
