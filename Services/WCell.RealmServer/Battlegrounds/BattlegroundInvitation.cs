using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Battlegrounds
{
	public class BattlegroundInvitation
	{
		public readonly BattlegroundTeam Team;

		internal OneShotUpdateObjectAction CancelTimer;

		public BattlegroundInvitation(BattlegroundTeam team, int queueIndex)
		{
			Team = team;
			QueueIndex = queueIndex;
		}

		public int QueueIndex
		{
			get;
			internal set;
		}
	}
}