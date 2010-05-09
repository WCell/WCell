using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.AI.Brains;

namespace WCell.RealmServer.AI.Actions.Movement
{
	public class AIMoveThenExecAction : AIAction
	{
		public AIMoveThenExecAction(Unit owner, UnitActionCallback actionCallback)
			: base(owner)
		{
			ActionCallback = actionCallback;
		}

		/// <summary>
		/// The Action to execute, once arrived
		/// </summary>
		public UnitActionCallback ActionCallback
		{
			get;
			set;
		}

		#region Overrides of AIAction

		public override void Start()
		{
		}

		public override void Update()
		{
			if (m_owner.Movement.Update())
			{
				// Arrived, now execute:
				if (ActionCallback != null)
				{
					ActionCallback(m_owner);
				}
			}
		}

		public override void Stop()
		{
			m_owner.Movement.Stop();
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}

		#endregion
	}
}
