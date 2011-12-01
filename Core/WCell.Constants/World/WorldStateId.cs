namespace WCell.Constants.World
{
    public enum WorldStateId
    {
        WSGAllianceScore = 1581,
        WSGHordeScore = 1582,
        WSGAlliancePickupState = 1545,
        WSGHordePickupState = 1546,
        WSGUnknown = 1547,
        WSGMaxScore = 1601,

        WSGHordeFlagState = 2338,
        WSGAllianceFlagState = 2339,
        // 1581 alliance flag captures
        // 8 1582 horde flag captures
        // 9 1545 unk, set to 1 on alliance flag pickup...
        // 10 1546 unk, set to 1 on horde flag pickup, after drop it's -1
        // 11 1547 unk
        // 13 2338 horde (0 - hide, 1 - flag ok, 2 - flag picked up (flashing), 3 - flag picked up (not flashing)
        // 14 2339 alliance (0 - hide, 1 - flag ok, 2 - flag picked up (flashing), 3 - flag picked up (not flashing)

        COTMedvihsShield = 2540,
        COTTimeRiftsOpened = 2784,

        LightHopeChapelBattleDKTimeRemaining = 3604,

        TheramoreMarksmanRemaining = 3082,
        /// <summary>
        /// Violet Hold
        /// </summary>
        PortalsOpened = 3810,
        PrisonSealIntegrity = 3815,

        UndercityBattleTimeToStart = 3877,

        WGHours = 3975,
        WG10Minutes = 3976,
        WGMinutes = 3782,
        WG10Seconds = 3784,
        WGSeconds = 3785,

        ABOccupiedBasesHorde = 1778,
        ABOccupiedBasesAlliance = 1779,
        ABResourcesAlliance = 1776,
        ABResourcesHorde = 1777,
        ABMaxResources = 1780,

        ABShowStableIcon = 1842,                    // Neutral
        ABShowStableIconAlliance = 1767,            // Alliance controlled
        ABShowStableIconHorde = 1768,               // Horde controlled
        ABShowStableIconAllianceContested = 1769,   // Alliance contested
        ABShowStableIconHordeContested = 1770,      // Horde contested
        
        ABShowGoldMineIcon = 1843,                  // Neutral
        ABShowGoldMineIconAlliance = 1787,          // Alliance controlled
        ABShowGoldMineIconHorde = 1788,             // Horde controlled
        ABShowGoldMineIconAllianceContested = 1789, // Alliance contested
        ABShowGoldMineIconHordeContested = 1790,    // Horde contested

        ABShowLumberMillIcon = 1844,                // Neutral
        ABShowLumberMillIconAlliance = 1792,        // Alliance controlled
        ABShowLumberMillIconHorde = 1793,           // Horde controlled
        ABShowLumberMillIconAllianceContested = 1794, // Alliance contested
        ABShowLumberMillIconHordeContested = 1795,  // Horde contested

        ABShowFarmIcon = 1845,                      // Neutral
        ABShowFarmIconAlliance = 1772,              // Alliance controlled
        ABShowFarmIconHorde = 1773,                 // Horde controlled
        ABShowFarmIconAllianceContested = 1774,     // Alliance contested
        ABShowFarmIconHordeContested = 1775,        // Horde contested

        ABShowBlacksmithIcon = 1846,                // Neutral
        ABShowBlacksmithIconAlliance = 1782,        // Alliance controlled
        ABShowBlacksmithIconHorde = 1783,           // Horde controlled
        ABShowBlacksmithIconAllianceContested = 1784, // Alliance contested
        ABShowBlacksmithIconHordeContested = 1785,  // Horde contested

        ABNearVictoryWarning = 1955,

        AlgalonTimeToSignal = 4131,
        End
    }
}