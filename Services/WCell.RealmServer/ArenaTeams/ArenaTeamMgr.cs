using System;
using System.Collections.Generic;
using WCell.Constants.ArenaTeams;
using WCell.Core;
using WCell.Core.Database;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.NPCs;
using WCell.Util.Collections;

namespace WCell.RealmServer.ArenaTeams
{
	public static class ArenaTeamMgr
	{
        #region Charters
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
        #endregion 

        /// <summary>
        /// Maps char-id to the corresponding ArenaTeamMember object so it can be looked up when char reconnects
        /// </summary>
        public static readonly IDictionary<uint, ArenaTeamMember> OfflineChars = new SynchronizedDictionary<uint, ArenaTeamMember>();
        public static readonly IDictionary<uint, ArenaTeam> ArenaTeamsById = new SynchronizedDictionary<uint, ArenaTeam>();
        public static readonly IDictionary<string, ArenaTeam> ArenaTeamsByName = new SynchronizedDictionary<string, ArenaTeam>(StringComparer.InvariantCultureIgnoreCase);

        #region Init
        //[Initialization(InitializationPass.Fifth, "Initialize Arena Teams")]
        public static void Initialize()
        {
            LoadFromDB();
        }
        public static void LoadFromDB()
        {
            ArenaTeam[] teams;
#if DEBUG
            try
            {
#endif
                teams = WCellRecord<ArenaTeam>.FindAll();
#if DEBUG
            }
            catch (Exception e)
            {
                RealmDBMgr.OnDBError(e);
                teams = WCellRecord<ArenaTeam>.FindAll();
            }
#endif
            if (teams != null)
            {
                foreach (var team in teams)
                {
                    team.InitAfterLoad();
                }
            }
        }
        #endregion

        /// <summary>
        /// New or loaded Arena Team
        /// </summary>
        /// <param name="guild"></param>
        public static void RegisterArenaTeam(ArenaTeam team)
        {
            ArenaTeamsById.Add(team.Id, team);
            ArenaTeamsByName.Add(team.Name, team);

            foreach (var atm in team.Members.Values)
            {
                if (atm.Character == null && !OfflineChars.ContainsKey(atm.Id))
                    OfflineChars.Add(atm.Id, atm);
            }
        }

        public static void UnregisterArenaTeam(ArenaTeam team)
        {
            ArenaTeamsById.Remove(team.Id);
            ArenaTeamsByName.Remove(team.Name);
        }

        public static void RegisterArenaTeamMember(ArenaTeamMember atm)
        {
            if (atm.Character == null)
                OfflineChars.Add(atm.Id, atm);
        }

        public static void UnregisterArenaTeamMember(ArenaTeamMember atm)
        {
            if (OfflineChars.ContainsKey(atm.Id))
                OfflineChars.Remove(atm.Id);
        }

        public static ArenaTeam GetArenaTeam(uint teamId)
        {
            ArenaTeam team;
            ArenaTeamsById.TryGetValue(teamId, out team);
            return team;
        }

        public static ArenaTeam GetArenaTeam(string name)
        {
            ArenaTeam team;
            ArenaTeamsByName.TryGetValue(name, out team);
            return team;
        }

        public static ArenaTeamSlot GetSlotByType(uint type)
        {
            switch(type)
            {
                case (uint)ArenaTeamTypes.ARENA_TEAM_2v2:
                    return ArenaTeamSlot.TWO_VS_TWO;
                case (uint)ArenaTeamTypes.ARENA_TEAM_3v3:
                    return ArenaTeamSlot.THREE_VS_THREE;
                case (uint)ArenaTeamTypes.ARENA_TEAM_5v5:
                    return ArenaTeamSlot.FIVE_VS_FIVE;
                default:
                    throw new Exception("Invalid Type of arena team !");
            }
        }

        #region Checks
        public static bool CanUseName(string name)
        {
            if (IsValidArenaTeamName(name))
            {
                return GetArenaTeam(name) == null;
            }
            return false;
        }

        public static bool DoesArenaTeamExist(string name)
        {
            return GetArenaTeam(name) != null;
        }

        public static bool IsValidArenaTeamName(string name)
        {
            name = name.Trim();
            if (name.Length < 3 && name.Length > MaxArenaTeamNameLength)
            {
                return false;
            }

            return true;
        }
        #endregion

        public static int MaxArenaTeamNameLength = 24;
	}
}