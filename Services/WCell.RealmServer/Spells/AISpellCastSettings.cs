using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells
{
	public enum AISpellCastTarget
	{
		Default,

		// hostile
		NearestHostilePlayer,
		RandomHostilePlayer,
		SecondHighestThreatTarget,

		// allied
		RandomAlliedUnit
	}
	
	/// <summary>
	/// Determines how AI should cast this spell
	/// </summary>
	public class AISpellCastSettings
	{
		/// <summary>
		/// Amount of time to idle after casting the spell
		/// </summary>
		public int IdleTimeAfterCastMillis = 1000;
		public AISpellCastTarget Target;
		public int CooldownMin, CooldownMax;
	}
}
