/*************************************************************************
 *
 *   file		: Ban.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 12:30:51 +0800 (Mon, 16 Feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Net;
using NHibernate.Criterion;
using WCell.RealmServer.Database;
using PropertyAccess = Castle.ActiveRecord.PropertyAccess;
using Castle.ActiveRecord;
using System.Net.Sockets;
using WCell.Util.Threading;
using WCell.Core.Database;

namespace WCell.AuthServer.Firewall
{
	/// <summary>
	/// Represents a Ban entry
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class BanEntry : WCellRecord<BanEntry>
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(BanEntry), "BanId");

		/// <summary>
		/// Returns the next unique Id for a new SpellRecord
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}
		private string m_mask;
		private int[] m_MaskBytes;

		public BanEntry(DateTime created, DateTime? expires, string banmask, string reason)
		{
			Created = created;
			Expires = expires;
			BanMask = banmask;
			Reason = reason;
			New = true;
		}

		public BanEntry()
		{
		}

		[PrimaryKey(PrimaryKeyType.Assigned)]
		public long BanId
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

		[Property]
		public DateTime? Expires
		{
			get;
			set;
		}

		[Property]
		public string Reason
		{
			get;
			set;
		}

		/// <summary>
		/// A mask matching IP-Addresses in the format:
		/// 123.45.*.1 (also matches 123.45.*.1.*.*)
		/// 41.3.*.23.*.243
		/// </summary>
		[Property(NotNull = true)]
		public string BanMask
		{
			get
			{
				return m_mask;
			}
			set
			{
				m_mask = value.Trim();
				m_MaskBytes = BanMgr.GetBytes(m_mask);
			}
		}

		public bool Matches(IPAddress addr)
		{
			return Matches(addr.GetAddressBytes());
		}

		public bool Matches(byte[] bytes)
		{
			if (!CheckValid())
			{
				return false;
			}
			for (var i = 0; i < m_MaskBytes.Length; i++)
			{
				var bte = bytes[i];
				if (!BanMgr.Matches(m_MaskBytes[i], bte))
				{
					return false;
				}
			}
			return false;
		}

		public bool Matches(int[] bytes)
		{
			if (!CheckValid())
			{
				return false;
			}
			return BanMgr.Match(m_MaskBytes, bytes);
		}

		public override string ToString()
		{
			return string.Format("{0} (Created: {1}, Banned {2}{3})",
				BanMask,
				Created,
				Expires != null ? "until: " + Expires : "indefinitely",
				string.IsNullOrEmpty(Reason) ? "" : ", Reason: " + Reason);
		}

		public bool CheckValid()
		{
			if (Expires != null && Expires.Value <= DateTime.Now)
			{
				// remove the expired bans
				AuthenticationServer.IOQueue.AddMessage(new Message(DeleteAndFlush));
				return false;
			}
			else
			{
				return true;
			}
		}

		public override void Delete()
		{
			BanMgr.Lock.EnterWriteLock();
			try
			{
				BanMgr.m_bans.Remove(this);
				base.Delete();
			}
			finally
			{
				BanMgr.Lock.ExitWriteLock();
			}
		}

		public override void DeleteAndFlush()
		{
			BanMgr.Lock.EnterWriteLock();
			try
			{
				BanMgr.m_bans.Remove(this);
				base.DeleteAndFlush();
			}
			finally
			{
				BanMgr.Lock.ExitWriteLock();
			}
		}
	}
}