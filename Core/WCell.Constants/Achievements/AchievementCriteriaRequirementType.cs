namespace WCell.Constants.Achievements
{
    public enum AchievementCriteriaRequirementType
    {
        None = 0,               // 0              0
        Creature = 1,           // creature_id    0
        PlayerClassRace = 2,    // class_id       race_id
        PlayerLessHealth = 3,   // health_percent 0
        PlayerDead = 4,         // own_team       0             not corpse (not released body), own_team==false if enemy team expected
        Aura1 = 5,              // spell_id       effect_idx    For the player
        Area = 6,               // area id        0
        Aura2 = 7,              // spell_id       effect_idx    For the target 
        Value = 8,              // minvalue                     value provided with achievement update must be not less that limit
        Level = 9,              // minlevel                     minlevel of target
        Gender = 10,            // gender                       0=male; 1=female
        Disabled = 11,          //                              used to prevent achievement creteria complete if not all requirement implemented and listed in table
        MapDifficulty = 12,     // difficulty                   normal/heroic difficulty for current event map
        MapPlayerCount = 13,    // count                        "with less than %u people in the zone"
        Team = 14,              // team                         HORDE(67), ALLIANCE(469)
        Drunk = 15,             // drunken_state  0             (enum DrunkenState) of player
        Holiday = 16,           // holiday_id     0             event in holiday time
        BgLossTeamScore = 17,   // min_score      max_score     player's team win bg and opposition team have team score in range
        InstanceScript = 18,    // 0              0             maker instance script call for check current criteria requirements fit
        EquippedItemLevel = 19, // item_level     item_quality  fir equipped item in slot `misc1` to item level and quality
        NthBirthday = 20,       // N                            login on day of N-th Birthday
        KnownTitle = 21,        // titleId
        End = 22,
    };
}
