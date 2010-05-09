using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public class AITameFocusAction : AIFollowTargetAction
	{
		public AITameFocusAction(NPC owner)
			: base(owner)
		{
		}

		public override void Start()
		{
			var tamer = ((NPC)m_owner).CurrentTamer;
			if (tamer == null)
			{
				Stop();
				m_owner.Brain.EnterDefaultState();
			}
			else
			{
				Target = tamer;
				base.Start();
			}
		}

		public override void Update()
		{
			var tamer = ((NPC)m_owner).CurrentTamer;
			if (tamer == null)
			{
				Stop();
				m_owner.Brain.EnterDefaultState();
			}
			else
			{
				base.Update();
			}
		}

		public override void Stop()
		{
			((NPC)m_owner).CurrentTamer = null;
			base.Stop();
		}
	}
}
