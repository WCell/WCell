using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Tickets
{
	public enum TicketType : uint
	{
		ReportProblem = 1,
		TalkToGm = 17
	}

	public enum TicketReportLagType : uint
	{
		Loot = 0,
		AuctionHouse = 1,
		Mail = 2,
		Chat = 3,
		Movement = 4,
		Spell = 5
	}

	public enum TicketInfoResponse : uint
	{
		Fail = 1,
		Saved = 2,
		Pending = 6,
		Deleted = 9,
		NoTicket = 10
	}
}