/*************************************************************************
 *
 *   file		: AccountInfo.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-28 09:34:05 +0100 (lø, 28 mar 2009) $
 
 *   revision		: $Rev: 826 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Runtime.Serialization;
using WCell.Constants;

namespace WCell.Intercommunication.DataTypes
{
	[DataContract]
	public class FullAccountInfo : IAccount
	{
		/// <summary>
		/// ID of this account
		/// </summary>
		[DataMember]
		public long AccountId { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public bool IsActive
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? StatusUntil
		{
			get;
			set;
		}

		/// <summary>
		/// E-mail address of this account
		/// </summary>
		[DataMember]
		public string EmailAddress { get; set; }

		/// <summary>
		/// Highest supported version
		/// </summary>
		[DataMember]
		public ClientId ClientId { get; set; }

		/// <summary>
		/// The name of the Account's RoleGroup
		/// </summary>
		[DataMember]
		public string RoleGroupName { get; set; }

		[DataMember]
		public byte[] LastIP { get; set; }

		[DataMember]
		public DateTime? LastLogin { get; set; }

		[DataMember]
		public int HighestCharLevel
		{
			get;
			set;
		}

		[DataMember]
		public ClientLocale Locale
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Holds information about an account
	/// </summary>
	[DataContract]
	public class AccountInfo : IAccountInfo
	{
		/// <summary>
		/// ID of this account
		/// </summary>
		[DataMember]
		public long AccountId
		{
			get;
			set;
		}

		/// <summary>
		/// E-mail address of this account
		/// </summary>
		[DataMember]
		public string EmailAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Highest supported version
		/// </summary>
		[DataMember]
		public ClientId ClientId
		{
			get;
			set;
		}

		/// <summary>
		/// The name of the Account's RoleGroup
		/// </summary>
		[DataMember]
		public string RoleGroupName
		{
			get;
			set;
		}

		[DataMember]
		public byte[] LastIP
		{
			get;
			set;
		}

		[DataMember]
		public DateTime? LastLogin
		{
			get;
			set;
		}

		[DataMember]
		public int HighestCharLevel
		{
			get;
			set;
		}

		[DataMember]
		public ClientLocale Locale
		{
			get;
			set;
		}
	}

	public interface IAccountInfo
	{
		/// <summary>
		/// ID of this account
		/// </summary>
		long AccountId
		{
			get;
		}

		/// <summary>
		/// E-mail address of this account
		/// </summary>
		string EmailAddress
		{
			get;
		}

		/// <summary>
		/// Supported WoW version
		/// </summary>
		ClientId ClientId
		{
			get;
		}

		/// <summary>
		/// The name of the Account's RoleGroup
		/// </summary>
		string RoleGroupName
		{
			get;
		}

		byte[] LastIP
		{
			get;
		}

		DateTime? LastLogin
		{
			get;
		}

		int HighestCharLevel
		{
			get;
		}

		ClientLocale Locale
		{
			get;
		}
	}

	public interface IAccount : IAccountInfo
	{
		string Name
		{
			get;
		}

		bool IsActive
		{
			get;
		}

		DateTime? StatusUntil
		{
			get;
		}
	}
}