using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
	/// <summary>
	/// NPC leaves combat and goes home
	/// </summary>
	public class AIEvadeAction : AIAction, IAIStateAction
	{
		public AIEvadeAction(Unit owner)
			: base(owner)
		{
		}

		public override void Start()
		{
			m_owner.IsEvading = true;
			m_owner.Target = null;

			// get moving
			if (m_owner.Movement.MoveTo(m_owner.Brain.SourcePoint, true))
			{
				// already at home
				m_owner.OnEvaded();
			}
		}

		public override void Update()
		{
			if (m_owner.Movement.Update())
			{
				m_owner.OnEvaded();
			}
		}

		public override void Stop()
		{
			m_owner.IsEvading = false;
			m_owner.Movement.Stop();
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Active; }
		}
	}
}