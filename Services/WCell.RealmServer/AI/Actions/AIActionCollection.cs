using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public class AIActionCollection : IAIActionCollection
	{
		public readonly AIAction[] Actions = new AIAction[(int)BrainState.End];
		protected Unit m_owner;

		public Unit Owner
		{
			get { return m_owner; }
		}

		public bool IsInitialized
		{
			get { return m_owner != null; }
		}

		public AIAction this[BrainState state]
		{
			get { return Actions[(int)state]; }
			set
			{
				var oldAction = Actions[(int)state];
				if (oldAction == value) return;

				Actions[(int)state] = value;

				var brain = m_owner.Brain;
				if (brain != null && brain.State == state && brain.CurrentAction == oldAction)
				{
					// we can quite safely assume that the Brain is using this collection
					// owner already selected the action -> override it
					brain.CurrentAction = value;
				}
			}
		}

		public virtual void Init(Unit owner)
		{
			m_owner = owner;
		}
	}
}