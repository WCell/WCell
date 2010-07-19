using System;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Help.Tickets;
using WCell.Util.Commands;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Addition to every Character that is a StaffMember to append staff-only related information without wasting extra memory
	/// on non-staff members.
	/// TODO: Get rid off this
	/// </summary>
	public class ExtraInfo : IDisposable
	{
		private Character m_owner;
		internal GOSelection m_goSelection;
		internal BaseCommand<RealmServerCmdArgs> m_selectedCommand;
		internal Ticket m_handlingTicket;

		public ExtraInfo(Character chr)
		{
			m_owner = chr;
		}

		#region GO-Selection
		/// <summary>
		/// The currently selected GO of this Character. Set to null to deselect.
		/// </summary>
		public GameObject SelectedGO
		{
			get
			{
				if (m_goSelection != null)
				{
					return m_goSelection.GO;
				}
				return null;
			}
			set
			{
				GOSelectMgr.Instance[m_owner] = value;
			}
		}
		#endregion

		#region Command Selection
		public BaseCommand<RealmServerCmdArgs> SelectedCommand
		{
			get { return m_selectedCommand; }
		}
		#endregion

		#region Ticket Handling
		/// <summary>
		/// The ticket that this Character is currently working on (or null)
		/// </summary>
		public Ticket HandlingTicket
		{
			get { return m_handlingTicket; }
			set { m_handlingTicket = value; }
		}

		public Ticket EnsureSelectedHandlingTicket()
		{
			if (m_handlingTicket == null)
			{
				m_handlingTicket = TicketMgr.Instance.GetNextUnhandledTicket(m_owner);
			}
			return m_handlingTicket;
		}
		#endregion

		public void Dispose()
		{
			GOSelectMgr.Instance.Deselect(this);
			m_owner = null;
		}
	}
}