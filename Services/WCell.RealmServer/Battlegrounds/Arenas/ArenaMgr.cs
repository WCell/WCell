using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Battlegrounds.Arenas
{
	public sealed class ArenaMgr : Manager<ArenaMgr>
	{
		private static uint arenateamCharter2v2Cost = 800000;
		private static uint arenateamCharter3v3Cost = 1200000;
		private static uint arenateamCharter5v5Cost = 2000000;

		public static uint ArenaTeamCharter2v2Cost
		{
			get { return arenateamCharter2v2Cost; }
			set
			{
				arenateamCharter2v2Cost = value;
				PetitionerEntry.ArenaPetition2v2Entry.Cost = value;
			}
		}

		public static uint ArenaTeamCharter3v3Cost
		{
			get { return arenateamCharter3v3Cost; }
			set
			{
				arenateamCharter3v3Cost = value;
				PetitionerEntry.ArenaPetition3v3Entry.Cost = value;
			}
		}

		public static uint ArenaTeamCharter5v5Cost
		{
			get { return arenateamCharter5v5Cost; }
			set
			{
				arenateamCharter5v5Cost = value;
				PetitionerEntry.ArenaPetition5v5Entry.Cost = value;
			}
		}

		private static int requiredCharter2v2Signature = 2;
		private static int requiredCharter3v3Signature = 3;
		private static int requiredCharter5v5Signature = 5;

		public static int RequiredCharter2v2Signature
		{
			get { return requiredCharter2v2Signature; }
			set
			{
				requiredCharter2v2Signature = value;
				PetitionerEntry.ArenaPetition2v2Entry.RequiredSignatures = value;
			}
		}

		public static int RequiredCharter3v3Signature
		{
			get { return requiredCharter3v3Signature; }
			set
			{
				requiredCharter3v3Signature = value;
				PetitionerEntry.ArenaPetition3v3Entry.RequiredSignatures = value;
			}
		}

		public static int RequiredCharter5v5Signature
		{
			get { return requiredCharter5v5Signature; }
			set
			{
				requiredCharter5v5Signature = value;
				PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures = value;
			}
		}

		protected override bool InternalStart()
		{
			return true;
		}

		protected override bool InternalStop()
		{
			return true;
		}
	}
}