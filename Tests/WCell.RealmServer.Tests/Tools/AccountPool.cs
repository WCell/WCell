using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Privileges;
using WCell.Intercommunication.DataTypes;
using WCell.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Util;

namespace WCell.RealmServer.Tests.Misc
{
	public class AccountPool
	{
		/// <summary>
		/// Role is used when no role is specified.
		/// </summary>
		public static RoleGroup GlobalDefaultRole = PrivilegeMgr.Instance.HighestRole;

		/// <summary>
		/// Default instance.
		/// </summary>
		public static readonly AccountPool Instance = new AccountPool();

		TestAccount[] m_accounts;

		public AccountPool()
			: this(GlobalDefaultRole)
		{
		}

		public AccountPool(RoleGroup defaultRole)
		{
			m_accounts = new TestAccount[100];
			DefaultRole = defaultRole;
		}

		public RoleGroup DefaultRole
		{
			get;
			set;
		}

		public TestAccount First
		{
			get
			{
				return this[0];
			}
		}

		/// <summary>
		/// Returns the <c>index</c>'th Account.
		/// Creates a new one, if it doesn't exist yet.
		/// </summary>
		public TestAccount this[uint index]
		{
			get
			{
				if (m_accounts[index] == null)
				{
					var acc = CreateAccount();

					ArrayUtil.Set(ref m_accounts, index, acc);
				}
				return m_accounts[index];
			}
		}

		/// <summary>
		/// Creates a new Account and adds it to the pool.
		/// </summary>
		/// <returns></returns>
		public TestAccount CreateAccount()
		{
			Assert.IsNotNull(DefaultRole);

			var index = m_accounts.GetFreeIndex();			
			var acc = new TestAccount(DefaultRole, index);
			
			ArrayUtil.Set(ref m_accounts, index, acc);

			return acc;
		}

		public void Delete(uint id)
		{
			m_accounts[id] = null;
		}
	}
}
