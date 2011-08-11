/*************************************************************************
 *
 *   file		: HelpMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 19:35:36 +0800 (Thu, 31 Jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace WCell.RealmServer.Help.Tickets
{
	/// <summary>
	/// Manages support, help-requests, GM Tickets, surveys etc.
	///
	/// TODO: Commands - Summon, Goto, Next, Previous
	/// TODO: Staff chat-channeling for enforced help
	/// TODO: Individual settings for staff/ticket issuing? Use ticket types?
	/// TODO: Backup command to store certain tickets?
	/// </summary>
	public class TicketMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public static readonly TicketMgr Instance = new TicketMgr();

		internal readonly Dictionary<uint, Ticket> ticketsById = new Dictionary<uint, Ticket>();
		internal readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();
		internal Ticket first;
		internal Ticket last;

		public Ticket First
		{
			get { return first; }
		}

		public Ticket Last
		{
			get { return last; }
		}

		public int TicketCount
		{
			get { return ticketsById.Count; }
		}

		public void AddTicket(Ticket ticket)
		{
			lck.EnterWriteLock();
			try
			{
				if (first == null)
				{
					first = last = ticket;
				}
				else
				{
					last.m_next = ticket;
					ticket.m_previous = last;
					last = ticket;
				}
				ticketsById.Add(ticket.CharId, ticket);
			}
			finally
			{
				lck.ExitWriteLock();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="charId"></param>
		/// <returns>The Ticket of the Character with the given charId or null if the Character did not issue any.</returns>
		public Ticket GetTicket(uint charId)
		{
			lck.EnterReadLock();
			try
			{
				Ticket ticket;
				ticketsById.TryGetValue(charId, out ticket);
				return ticket;
			}
			finally
			{
				lck.ExitReadLock();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="handler">May be null</param>
		/// <returns>Returns the next Ticket that is not already handled and may be handled by the given handler.</returns>
		public Ticket GetNextUnhandledTicket(ITicketHandler handler)
		{
			lck.EnterReadLock();
			try
			{
				var current = first;
				while (current != null)
				{
					if (handler == null || handler.MayHandle(current))
					{
						return current;
					}
					current = current.Next;
				}
			}
			finally
			{
				lck.ExitReadLock();
			}
			return null;
		}

		public Ticket HandleNextUnhandledTicket(ITicketHandler handler)
		{
			Ticket ticket;
			do
			{
				ticket = GetNextUnhandledTicket(handler);
				if (ticket != null)
				{
					lck.EnterWriteLock();
					try
					{
						if (ticket.Handler == null)
						{
							ticket.SetHandlerUnlocked(handler);
							break;
						}
						// Ticket got handled in the very short time in which the lock was released -> continue
					}
					finally
					{
						lck.ExitWriteLock();
					}
				}
			} while (ticket != null);
			return ticket;
		}

		public bool HandleTicket(ITicketHandler handler, Ticket ticket)
		{
			if (handler.MayHandle(ticket))
			{
				ticket.Handler = handler;
				return true;
			}
			return false;
		}

		public Ticket[] GetAllTickets()
		{
			lck.EnterReadLock();
			var tickets = new Ticket[ticketsById.Count];
			try
			{
				var current = first;
				int i = 0;
				while (current != null)
				{
					tickets[i++] = current;
					current = current.Next;
				}
			}
			finally
			{
				lck.ExitReadLock();
			}
			return tickets;
		}

		public IEnumerator<Ticket> GetEnumerator()
		{
			return new TicketEnumerator(this);
		}

		class TicketEnumerator : IEnumerator<Ticket>
		{
			private TicketMgr mgr;
			private Ticket current;

			public TicketEnumerator(TicketMgr mgr)
			{
				this.mgr = mgr;
			}

			public void Dispose()
			{
				mgr = null;
				current = null;
			}

			public bool MoveNext()
			{
				if (current == null)
				{
					current = mgr.first;
					return current != null;
				}

				if (current.Next != null)
				{
					current = current.Next;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				current = null;
			}

			public Ticket Current
			{
				get { return current; }
			}

			object IEnumerator.Current
			{
				get { return current; }
			}
		}
	}
}