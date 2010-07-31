using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.Constants;
using System.Net;
using WCell.Core.Initialization;
using WCell.Util.Threading;
using System.Threading;
using WCell.AuthServer.Database;

namespace WCell.AuthServer.Firewall
{
	/// <summary>
	/// TODO: Use some kind of string-tree to improve lookup
	/// </summary>
	public static class BanMgr
	{
		//BinarySearchTree<long, IPRange> m_bannedIps;
		internal static List<BanEntry> m_bans;

		public static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		public static readonly int[] LocalHostBytes = new[] { 127, 0, 0, 1, 0, 0 };

		[Initialization(InitializationPass.Fourth, "Caching Ban list...")]
		public static void InitBanMgr()
		{
			try
			{
				m_bans = BanEntry.FindAll().ToList();
			}
			catch (Exception e)
			{
				AuthDBUtil.OnDBError(e);
				m_bans = BanEntry.FindAll().ToList();
			}
		}

		public static List<BanEntry> AllBans
		{
			get
			{
				Lock.EnterReadLock();
				try
				{
					var list = new List<BanEntry>(m_bans.Count);
					foreach (var ban in m_bans)
					{
						if (ban.CheckValid())
						{
							list.Add(ban);
						}
					}
					return list;
				}
				finally
				{
					Lock.ExitReadLock();
				}
			}
		}

		public static bool IsBanned(long ip)
		{
			return IsBanned(new IPAddress(ip));
		}

		public static bool IsBanned(IPAddress ip)
		{
			Lock.EnterReadLock();
			try
			{
				var parts = ip.GetAddressBytes();
				for (var i = 0; i < m_bans.Count; i++)
				{
					var ban = m_bans[i];
					if (ban.Matches(parts))
					{
						return true;
					}
				}
			}
			finally
			{
				Lock.ExitReadLock();
			}
			return false;
		}

		/// <summary>
		/// Returns the first Ban that matches the given IP or null if none matches
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static BanEntry GetBan(IPAddress ip)
		{
			var parts = ip.GetAddressBytes();

			Lock.EnterReadLock();
			try
			{
				for (var i = 0; i < m_bans.Count; i++)
				{
					var ban = m_bans[i];
					if (ban.Matches(parts))
					{
						return ban;
					}
				}
			}
			finally
			{
				Lock.ExitReadLock();
			}
			return null;
		}

		public static List<BanEntry> GetBanList(string mask)
		{
			Lock.EnterReadLock();
			try
			{
				var entries = new List<BanEntry>();
				var parts = GetBytes(mask);
				for (var i = 0; i < m_bans.Count; i++)
				{
					var ban = m_bans[i];
					if (ban.Matches(parts))
					{
						entries.Add(ban);
					}
				}
				return entries;
			}
			finally
			{
				Lock.ExitReadLock();
			}
		}

		public static BanEntry AddBan(DateTime? until, string mask, string reason)
		{
			Lock.EnterWriteLock();
			try
			{
				var ban = new BanEntry(
					DateTime.Now,
					until,
					mask,
					reason
				);

				ban.Save();
				m_bans.Add(ban);

				return ban;
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		public static BanEntry AddBan(TimeSpan? lastsFor, string mask, string reason)
		{
			Lock.EnterWriteLock();
			try
			{
				var ban = new BanEntry (
					DateTime.Now,
					lastsFor != null ? DateTime.Now + lastsFor : null,
					mask,
					reason
				);

				ban.Save();
				m_bans.Add(ban);

				return ban;
			}
			finally
			{
				Lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Returnes whether the given bytes either match the Localhost address
		/// or only consist of wildcards or zeros.
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static bool IsInvalid(int[] bytes)
		{
			if (Match(LocalHostBytes, bytes))
			{
				return true;
			}

			var found = -2;
			foreach (var bte in bytes)
			{
				if (bte < -1)
				{
					return true;
				}

				if ((bte != -1 && bte != 0) || (found != -2 && bte != found))
				{
					return false;
				}
				found = bte;
			}
			return true;
		}

		public static bool Match(int[] bytes, int[] matchBytes)
		{
			for (var i = 0; i < matchBytes.Length; i++)
			{
				var bte = bytes[i];
				if (!Matches(bte, matchBytes[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static bool Matches(int bte, int matchByte)
		{
			return matchByte == -1 || bte == matchByte;
		}

		public static int[] GetBytes(string mask)
		{
			var maskParts = new int[6];
			var parts = mask.Trim().Split('.');
			int i;
			for (i = 0; i < parts.Length; i++)
			{
				var part = parts[i];
				int b;
				if (int.TryParse(part, out b))
				{
					maskParts[i] = b;
				}
				else
				{
					maskParts[i] = -1;
				}
			}
			for (; i < maskParts.Length; i++)
			{
				maskParts[i] = -1;
			}
			return maskParts;
		}
	}
}