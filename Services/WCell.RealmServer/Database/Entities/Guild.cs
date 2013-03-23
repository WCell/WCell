using System;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database.Entities
{
	public class Guild
	{
		//TODO: Decide which fields should exist at all, or made private
		public long Id;
		public string Name;
		public string MOTD;
		public string Info;
		public DateTime Created;
		
		private uint _leaderLowId;
		public uint LeaderLowId {get { return _leaderLowId; }};

		public GuildTabard Tabard;
		public int PurchasedBankTabCount;
		public long Money;
	}
}