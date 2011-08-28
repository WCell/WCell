using System;

namespace WCell.Constants
{

	public enum MailResult : int
	{
		MailSent = 0,
		MoneyTaken = 1,
		ItemTaken = 2,
		ReturnedToSender = 3,
		Deleted = 4,
		MadePermanent = 5
	}

	public enum MailType : int
	{
		Normal = 0,
		CashOnDelivery = 1,
		Auction = 2,
		Creature = 3,
		GameObject = 4,
		Item = 5
	}

	public enum MailStationary
	{
		Unknown = 0x01,
		Normal = 0x29,
		GM = 0x3D,
		Auction = 0x3E,
		Val = 0x40,
		Chr = 0x41
	}

	public enum MailError
	{
		OK = 0,
		BAG_FULL = 1,
		CANNOT_SEND_TO_SELF = 2,
		NOT_ENOUGH_MONEY = 3,
		RECIPIENT_NOT_FOUND = 4,
		NOT_YOUR_ALLIANCE = 5,
		INTERNAL_ERROR = 6,
		DISABLED_FOR_TRIAL_ACCOUNT = 14,
		RECIPIENT_CAP_REACHED = 15,
		CANT_SEND_WRAPPED_COD = 16,
		MAIL_AND_CHAT_SUSPENDED = 17
	}

	[Flags]
	public enum MailListFlags : uint
	{
		NotRead = 0,
		Read = 0x01,
		Delete = 0x02,
		Auction = 0x04,
		//CODPaymentChecked = 8,
		Return = 0x10
	}

	public enum MailAuctionAnswers : byte
	{
		Outbid = 0,
		Won = 1,
		Successful = 2,
		Expired = 3,
		CancelledToBidder = 4,
		Cancelled = 5
	}
}