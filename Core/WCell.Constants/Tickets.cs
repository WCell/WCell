using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Tickets
{
	public enum TicketType : uint
	{
		Stuck = 1,
		Harassment = 2,
		Guild = 3,
		Item = 4,
		Environmental = 5,
		NonQuestNPC = 6,
		QuestNpc = 7,
		Technical = 8,
		AccountBilling = 9,
		Character = 10,
	};

	public enum TicketInfoResponse : uint
	{
		Fail = 1,
		Saved = 2,
		Pending = 6,
		Deleted = 9,
		NoTicket = 10
	}
}