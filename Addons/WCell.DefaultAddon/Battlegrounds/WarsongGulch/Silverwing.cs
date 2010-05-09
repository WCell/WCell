using System;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{

    /// <summary>
    /// Type representing the Silverwing side of WSG.
    /// Create this in a battleground's corresponding CreateTeam override. (alliance)
    /// Add callbacks to the timers' Action field. (to Spawn and to Return)
    /// </summary>
    public class Silverwing : WSGFaction, IDisposable
    {
		public Silverwing(WarsongGulch instance) :
			base(instance,
				SpellId.SilverwingFlag,
				SpellId.AllianceFlagDrop,
				SpellId.RecentlyDroppedFlag,
				WarsongGulch.AllianceFlagDebuffSpellId,
				WarsongGulch.SilverwingFlagStandId,
                WarsongGulch.SilverwingFlagId)
		{
		}

		public override BattlegroundSide Side
		{
			get { return BattlegroundSide.Alliance; }
		}

    	public override string Name
    	{
			get { return "Silverwing"; }
    	}

    	public override WorldStateId ScoreStateId
    	{
    		get { return WorldStateId.WSGAllianceScore; }
    	}
    }
}