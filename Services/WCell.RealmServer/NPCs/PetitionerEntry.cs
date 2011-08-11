using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Items;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.NPCs
{
	public struct PetitionerEntry
	{
		public static PetitionerEntry GuildPetitionEntry = new PetitionerEntry
		{
			Index = 1,
			ItemId = ItemId.GuildCharter,
			DisplayId = 16161,
			Cost = GuildMgr.GuildCharterCost,
			RequiredSignatures = GuildMgr.RequiredCharterSignature
		};

		public static PetitionerEntry ArenaPetition2v2Entry = new PetitionerEntry
		{
			Index = 1,
			ItemId = ItemId.ArenaTeamCharter2v2,
			DisplayId = 16161,
			Cost = ArenaMgr.ArenaTeamCharter2v2Cost,
			RequiredSignatures = ArenaMgr.RequiredCharter2v2Signature
		};

		public static PetitionerEntry ArenaPetition3v3Entry = new PetitionerEntry
		{
			Index = 2,
			ItemId = ItemId.ArenaTeamCharter3v3,
			DisplayId = 16161,
			Cost = ArenaMgr.ArenaTeamCharter3v3Cost,
			RequiredSignatures = ArenaMgr.RequiredCharter3v3Signature
		};

		public static PetitionerEntry ArenaPetition5v5Entry = new PetitionerEntry
		{
			Index = 3,
			ItemId = ItemId.ArenaTeamCharter5v5,
			DisplayId = 16161,
			Cost = ArenaMgr.ArenaTeamCharter5v5Cost,
			RequiredSignatures = ArenaMgr.RequiredCharter5v5Signature
		};

		public uint Index;
		public ItemId ItemId;
		public uint DisplayId;
		public uint Cost;
		public int RequiredSignatures;
	}
}