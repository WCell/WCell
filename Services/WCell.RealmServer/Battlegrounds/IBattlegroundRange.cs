using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Battlegrounds
{
	public interface IBattlegroundRange
	{
		int MinLevel { get; }

		int MaxLevel { get; }

		BattlegroundTemplate Template { get; }
	}
}
