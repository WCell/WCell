using System;
using WCell.Constants.Tickets;
using WCell.Constants.World;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Help.Tickets
{
	/// <summary>
	/// Represents a ticket issued by a player.
	/// TODO: Save to DB
	/// </summary>
	public partial class Ticket : IWorldLocation
	{
		private uint m_charId;
		private Character m_owner;
		private string m_ownerName;
		private TicketType m_Type;
		private Map m_Map;
		private string m_Message;
		private DateTime m_Timestamp;

		internal Ticket m_previous, m_next;
		internal ITicketHandler m_handler;


		public Ticket(Character chr, string message, TicketType type)
		{
			m_owner = chr;
			m_ownerName = chr.Name;
			m_charId = chr.EntityId.Low;
			m_Message = message;

			m_Map = chr.Map;
			Position = chr.Position;
			Phase = chr.Phase;

			m_Timestamp = DateTime.Now;
			m_Type = type;
		}

		//public Ticket(Character chr, TicketType type, Map map, float positionX, float positionY, float positionZ, string message)
		//{
		//    m_owner = chr;
		//    m_ownerName = chr.Name;
		//    m_charId = chr.EntityId.Low;
		//    m_Message = message;
		//    m_map = map;
		//    m_position.X = positionX;
		//    m_position.Y = positionY;
		//    m_position.Z = positionZ;
		//    m_Type = type;
		//    m_Timestamp = DateTime.Now;
		//}

		//public Ticket(uint lowCharId, string ownerName, TicketType type, Map map, float positionX, float positionY, float positionZ, string message, DateTime timestamp)
		//{
		//    m_charId = lowCharId;
		//    m_ownerName = ownerName;
		//    m_Message = message;
		//    m_map = map;
		//    m_position.X = positionX;
		//    m_position.Y = positionY;
		//    m_position.Z = positionZ;
		//    m_Timestamp = timestamp;
		//    m_Type = type;
		//}

		public Ticket Previous
		{
			get { return m_previous; }
		}

		public Ticket Next
		{
			get { return m_next; }
		}

		/// <summary>
		/// The low part of the EntityId of the Character who issued this Ticket
		/// </summary>
		public uint CharId
		{
			get { return m_charId; }
			set { m_charId = value; }
		}

		/// <summary>
		/// The owner of this Ticket or null if not online
		/// </summary>
		public Character Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// The owner of this Ticket or null if not online
		/// </summary>
		public string OwnerName
		{
			get { return m_ownerName; }
		}

		public TicketType Type
		{
			get { return m_Type; }
			set { m_Type = value; }
		}

		public MapId MapId
		{
			get { return m_Map.Id; }
		}

		/// <summary>
		/// The Map where this Ticket was submitted or the Owner last logged out.
		/// </summary>
		public Map Map
		{
			get { return m_Map; }
			set { m_Map = value; }
		}

		/// <summary>
		/// The Position where this Ticket was submitted or the Owner last logged out.
		/// </summary>
		public Vector3 Position
		{
			get;
			set;
		}

		public uint Phase
		{
			get;
			set;
		}

		public string Message
		{
			get { return m_Message; }
			set { m_Message = value; }
		}

		/// <summary>
		/// The time when the ticket was submitted
		/// </summary>
		public DateTime Timestamp
		{
			get { return m_Timestamp; }
			set { m_Timestamp = value; }
		}

		public string TimestampStr
		{
			get
			{
				return m_Timestamp.ToString();
			}
		}

		/// <summary>
		/// The time between now and when the ticket was submitted
		/// </summary>
		public TimeSpan Age
		{
			get
			{
				return DateTime.Now - m_Timestamp;
			}
		}

		/// <summary>
		/// The current Handler of this Ticket.
		/// Setting the handler is synchronized and also automatically sets the Handler's current HandlingTicket to this.
		/// </summary>
		public ITicketHandler Handler
		{
			get { return m_handler; }
			set
			{
				if (m_handler != value)
				{
					TicketMgr.Instance.lck.EnterWriteLock();
					try
					{
						SetHandlerUnlocked(value);
					}
					finally
					{
						TicketMgr.Instance.lck.ExitWriteLock();
					}
				}
			}
		}

		internal void SetHandlerUnlocked(ITicketHandler handler)
		{
			if (m_handler != handler)
			{
				var oldHandler = m_handler;
				m_handler = handler;
				if (handler != null)
				{
					handler.HandlingTicket = this;
				}
				var evt = TicketHandlerChanged;
				if (evt != null)
				{
					evt(this, oldHandler);
				}
			}
		}

		public void Delete()
		{
			TicketMgr.Instance.lck.EnterWriteLock();
			try
			{
				if (m_previous != null)
				{
					m_previous.m_next = m_next;
				}

				if (m_next != null)
				{
					m_next.m_previous = m_previous;
				}

				var mgr = TicketMgr.Instance;
				if (this == mgr.first)
				{
					mgr.first = null;
				}

				if (this == mgr.last)
				{
					mgr.last = null;
				}

				if (m_handler != null)
				{
					m_handler.HandlingTicket = null;
					m_handler = null;
				}

				if (m_owner != null)
				{
					m_owner.Ticket = null;
				}

				TicketMgr.Instance.ticketsById.Remove(m_charId);
			}
			finally
			{
				TicketMgr.Instance.lck.ExitWriteLock();
			}
		}

		public void Display(ITriggerer triggerer, string info)
		{
			triggerer.Reply("-------------");
			triggerer.Reply("| " + info + Type + " in " + m_Map.Name + " |");
			triggerer.Reply("-------------------------------------------------------------------");
			triggerer.Reply("| by " + m_ownerName + (m_owner == null ? " (Offline)" : "") + ", " + Age + " ago.");
			triggerer.Reply("-------------------------------------------------------------------");
			triggerer.Reply(Message);
			triggerer.Reply("-------------------------------------------------------------------");
		}

		public void DisplayFormat(ITriggerer triggerer, string info)
		{
			triggerer.ReplyFormat("-------------");
			triggerer.ReplyFormat("| " + info + Type + " in " + m_Map.Name + " |");
			triggerer.ReplyFormat("-------------------------------------------------------------------");
			triggerer.ReplyFormat("| by " + m_ownerName + (m_owner == null ? ChatUtility.Colorize(" (Offline)", Color.Red, true) : "") + ", " + Age + " ago.");
			triggerer.ReplyFormat("-------------------------------------------------------------------");
			triggerer.ReplyFormat(Message);
			triggerer.ReplyFormat("-------------------------------------------------------------------");
		}

		internal void OnOwnerLogin(Character chr)
		{
			TicketMgr.Instance.lck.EnterWriteLock();
			try
			{
				m_owner = chr;
				if (m_handler != null)
				{
					m_handler.SendMessage("Owner of the Ticket you are handling came back -{0}-.", ChatUtility.Colorize("online", Color.Green));
				}
			}
			finally
			{
				TicketMgr.Instance.lck.ExitWriteLock();
			}
		}

		internal void OnOwnerLogout()
		{
			TicketMgr.Instance.lck.EnterWriteLock();
			try
			{
				Position = m_owner.Position;
				m_Map = m_owner.Map;
				Phase = m_owner.Phase;

				m_owner = null;
				var handler = m_handler;
				if (handler != null)
				{
					handler.SendMessage("Owner of the Ticket you are handling went -{0}-.", ChatUtility.Colorize("offline", Color.Red));
				}
			}
			finally
			{
				TicketMgr.Instance.lck.ExitWriteLock();
			}
		}
	}
}