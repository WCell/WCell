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
    public class WarsongClan : WSGFaction, IDisposable
    {
    	/// <summary>
    	/// Creates a new team containing everything related to the Horde (warsong) side.
    	/// </summary>
    	public WarsongClan(WarsongGulch instance) :
    		base(instance,
				SpellId.WarsongFlag,
				SpellId.HordeFlagDrop,
				SpellId.RecentlyDroppedFlag_2,
				WarsongGulch.HordeFlagDebuffSpellId,
				WarsongGulch.WarsongClanFlagStandId,
                WarsongGulch.WarsongFlagId)
        {
        }

    	public override BattlegroundSide Side
    	{
			get { return BattlegroundSide.Horde; }
    	}

    	public override string Name
    	{
    		get { return "Warsong"; }
    	}

    	public override WorldStateId ScoreStateId
    	{
			get { return WorldStateId.WSGHordeScore; }
    	}
    }
}