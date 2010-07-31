using WCell.RealmServer.Chat;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.Help.Tickets
{
	/// <summary>
	/// Represents anyone who can (but not necessarily is allowed to) handle Tickets
	/// </summary>
	public interface ITicketHandler : IGenericChatTarget, INamed, IHasRole
	{
		/// <summary>
		/// The ticket that is currently being handled by this TicketHandler
		/// </summary>
		Ticket HandlingTicket { get; set; }

		/// <summary>
		/// Indicates whether this Handler may handle the given Ticket
		/// </summary>
		/// <param name="ticket"></param>
		/// <returns></returns>
		bool MayHandle(Ticket ticket);
	}
}