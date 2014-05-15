using System;

namespace WCell.RealmServer.Guilds
{
	public class GuildBankTabItem
	{
		public long Guid
		{
			get;
			set;
		}

		//[BelongsTo("Items")] TODO: Work out what to do about this
		public GuildBankTab BankTab
		{
			get;
			set;
		}

		public byte TabSlot
		{
			get;
			set;
		}

		public DateTime? LastModifiedOn
		{
			get;
			set;
		}
	}
}