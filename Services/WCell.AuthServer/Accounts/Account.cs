/*************************************************************************
 *
 *   file		: Account.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-11 01:51:02 +0800 (Sun, 11 May 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 333 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Net;
using Castle.ActiveRecord;
using NLog;
using WCell.AuthServer.Commands;
using WCell.AuthServer.Network;
using WCell.Constants;
using WCell.Core.Database;
using WCell.Intercommunication.DataTypes;
using WCell.RealmServer.Database;
using WCell.Util;

namespace WCell.AuthServer.Accounts
{
	/// <summary>
	/// Class for performing account-related tasks.
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class Account : WCellRecord<Account>, IAccount
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(Account), "AccountId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		private bool m_IsActive;

		/// <summary>
		/// Queries the DB for the count of all existing Accounts.
		/// </summary>
		/// <returns></returns>
		internal static int GetCount()
		{
			return Count();
		}

		/// <summary>
		/// Event is raised when the given Account logs in successfully with the given client.
		/// </summary>
		public static event Action<Account, IAuthClient> LoggedIn;

		public Account()
		{
		}

		public Account(string username, byte[] hash, string email)
		{
			Name = username;
			Password = hash;
			EmailAddress = email;
		    LastIP = IPAddress.Any.GetAddressBytes();
			AccountId = NextId();
			State = RecordState.New;
		}

		internal void OnLogin(IAuthClient client)
		{
			var addr = client.ClientAddress;
			if (addr == null)
			{
				// client disconnected
				return;
            }

            LastIP = addr.GetAddressBytes();
            LastLogin = DateTime.Now;
			Locale = client.Info.Locale;
			ClientVersion = client.Info.Version.ToString();
			UpdateAndFlush();

			AuthCommandHandler.AutoExecute(this);

			var evt = LoggedIn;
			if (evt != null)
			{
				evt(this, client);
			}

			s_log.Info("Account \"{0}\" logged in from {1}.", Name, client.ClientAddress);
		}

		public void Update(IAccount newInfo)
		{
			// TODO: Status changed - kick Account if banned?

			IsActive = newInfo.IsActive;
			StatusUntil = newInfo.StatusUntil;
			ClientId = newInfo.ClientId;
			EmailAddress = newInfo.EmailAddress;
			RoleGroupName = newInfo.RoleGroupName;
		}

		#region Props
		[PrimaryKey(PrimaryKeyType.Assigned)]
		public long AccountId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public DateTime Created
		{
			get;
			set;
		}

		[Property(Length = 16, NotNull = true, Unique = true)]
		public string Name
		{
			get;
			set;
		}

		[Property(ColumnType = "BinaryBlob", Length = 20, NotNull = true)]
		public byte[] Password
		{
			get;
			set;
		}

		[Property]
		public string EmailAddress
		{
			get;
			set;
		}

		[Property]
		public ClientId ClientId
		{
			get;
			set;
		}

		[Property]
		public string ClientVersion
		{
			get;
			set;
		}

		[Property(Length = 16, NotNull = true)]
		public string RoleGroupName
		{
			get;
			set;
		}

		public RoleGroupInfo Role
		{
			get { return Privileges.PrivilegeMgr.Instance.GetRoleGroup(RoleGroupName); }
		}

		/// <summary>
		/// Whether the Account may currently be used 
		/// (inactive Accounts are banned).
		/// </summary>
		[Property(NotNull = true)]
		public bool IsActive
		{
			get { return m_IsActive; }
			set
			{
				m_IsActive = value;
				StatusUntil = null;
			}
		}

		/// <summary>
		/// If set: Once this time is reached,
		/// the Active status of this account will be toggled
		/// (from inactive to active or vice versa)
		/// </summary>
		[Property]
		public DateTime? StatusUntil
		{
			get;
			set;
		}

		/// <summary>
		/// The time of when this Account last changed from outside. Used for Synchronization.
		/// </summary>
		/// <remarks>Only Accounts that changed, will be fetched from DB during resync when caching is enabled.</remarks>
		[Property]
		public DateTime? LastChanged
		{
			get;
			set;
		}

		[Property]
		public DateTime? LastLogin
		{
			get;
			set;
		}

		[Property]
		public byte[] LastIP
		{
			get;
			set;
		}

		[Property]
		public int HighestCharLevel
		{
			get;
			set;
		}

		[Property]
		public ClientLocale Locale
		{
			get;
			set;
		}

		#endregion

		public string LastIPStr
		{
			get
			{
				return new IPAddress(LastIP).ToString();
			}
		}

		public bool CheckActive()
		{
			if (StatusUntil != null && StatusUntil > DateTime.Now)
			{
				m_IsActive = !m_IsActive;
				StatusUntil = null;
				Save();
			}
			return m_IsActive;
		}

		public override void Delete()
		{
			AccountMgr.Instance.Remove(this);
			base.Delete();
		}

		public override void DeleteAndFlush()
		{
			AccountMgr.Instance.Remove(this);
			base.DeleteAndFlush();
		}

		public string Details
		{
			get
			{
				return string.Format("Account: {0} ({1}) is {7} " +
									 "({9}Role: {2}, Age: {3}, Last IP: {4}, Last Login: {5}, Version: {6}, Locale: {8})",
									 Name, AccountId, RoleGroupName,
									 (DateTime.Now - Created).Format(),
									 LastIPStr,
									 LastLogin != null ? LastLogin.ToString() : "<Never>",
									 ClientId,
									 AuthenticationServer.Instance.IsAccountLoggedIn(Name) ? "Online" : "Offline",
									 Locale,
									 (IsActive ? "" : "INACTIVE") + (StatusUntil != null ? " (Until: " + StatusUntil : ""));
			}
		}


		public override string ToString()
		{
			return Name + " (Id: " + AccountId + ")";
		}
	}
}