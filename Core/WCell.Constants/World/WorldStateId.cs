using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        AlgalonTimeToSignal = 4131,
        End
	}
}