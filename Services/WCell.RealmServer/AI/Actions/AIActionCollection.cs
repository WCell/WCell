using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public class AIActionCollection : IAIActionCollection
	{
		public readonly AIAction[] Actions = new AIAction[(int)BrainState.End];
		protected Unit m_owner;

		public bool IsInitialized
		{
			get { return m_owner != null; }
		}

		public virtual AIAction this[BrainState state]
		{
			get { return Actions[(int)state]; }
			set { Actions[(int)state] = value; }
		}

		public void SetAction(BrainState state, AIAction action)
		{
			Actions[(int)state] = action;
		}

		public bool Contains(BrainState state)
		{
			return Actions[(int)state] != null;
		}

		public virtual void Init(Unit owner)
		{
			m_owner = owner;
		}
	}
}
