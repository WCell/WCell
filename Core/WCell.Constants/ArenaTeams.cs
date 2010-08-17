
namespace WCell.Constants.ArenaTeams
{
    public enum ArenaTeamTypes : uint
    {
        ARENA_TEAM_2v2 = 2,
        ARENA_TEAM_3v3 = 3,
        ARENA_TEAM_5v5 = 5
    };

    public enum ArenaTeamSlot : uint
    {
        TWO_VS_TWO = 0,
        THREE_VS_THREE = 1,
        FIVE_VS_FIVE = 2,
        END = 3
    };

    public enum ArenaTeamStatsTypes : byte
    {
        STAT_TYPE_RATING = 0,
        STAT_TYPE_GAMES_WEEK = 1,
        STAT_TYPE_WINS_WEEK = 2,
        STAT_TYPE_GAMES_SEASON = 3,
        STAT_TYPE_WINS_SEASON = 4,
        STAT_TYPE_RANK = 5
    };
    /// <summary>
    /// Common arena team events
    /// </summary>
    public enum ArenaTeamEvents : byte
    {
        JOINED_SS = 3,            
        LEAVED_SS = 4,            
        REMOVE_SSS = 5,            
        LEADER_IS_SS = 6,           
        LEADER_CHANGED_SSS = 7,            
        DISBANDED_S = 8            
    };

    // PLAYER_FIELD_ARENA_TEAM_INFO_1_1 offsets
    public enum ArenaTeamInfoType : int
    {
        ARENA_TEAM_ID = 0,
        ARENA_TEAM_TYPE = 1,                        // new in 3.2 - team type?
        ARENA_TEAM_MEMBER = 2,                        // 0 - captain, 1 - member
        ARENA_TEAM_GAMES_WEEK = 3,
        ARENA_TEAM_GAMES_SEASON = 4,
        ARENA_TEAM_WINS_SEASON = 5,
        ARENA_TEAM_PERSONAL_RATING = 6,
        ARENA_TEAM_END = 7
    };
}
