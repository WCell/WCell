using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.AI.Actions
{
	/// <summary>
	/// Abstract class representing an AI Action that has a target
	/// (casting spell for example)
	/// </summary>
	public abstract class AITargetedAction : AIAction
	{
		protected AITargetedAction(Unit owner)
			: base(owner)
		{
		}
	
		protected SimpleRange m_range;

		/// <summary>
		/// Range in which the action can be executed
		/// </summary>
		public SimpleRange Range
		{
			get { return m_range; }
			set { m_range = value; }
		}
	}
}