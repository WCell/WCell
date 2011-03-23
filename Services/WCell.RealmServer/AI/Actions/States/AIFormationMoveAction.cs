using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;
using WCell.Util.Logging;

namespace WCell.RealmServer.AI.Actions.States
{
	/// <summary>
	/// Lets the owner move in Formation
	/// </summary>
	public class AIFormationMoveAction : AIAction
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public AIFormationMoveAction(NPC owner)
			: base(owner)
		{
		}

		public override bool IsGroupAction
		{
			get { return true; }
		}

		public override void Start()
		{
		}

		public override void Update()
		{
			
		}

		public override void Stop()
		{
			
		}

		public override UpdatePriority Priority
		{
			get { return ((NPC)m_owner).Group.UpdatePriority; }
		}
	}
}