using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public delegate AIAction AIActionCreator(Unit owner);

	public delegate AITargetedAction AITargetedActionCreator(Unit owner);

	/// <summary>
	/// An interface for AI Actions collections (i.e. a set of AI Action factories)
	/// </summary>
	public interface IAIActionCollection
	{
		Unit Owner { get; }

		bool IsInitialized
		{
			get;
		}

		/// <summary>
		/// Gets action of specific type. If there is more than one suitable action, the best fitting one is chosen
		/// (using evalutor)
		/// </summary>
		/// <returns>AI Action</returns>
		AIAction this[BrainState state]
		{
			get;
			set;
		}

		void Init(Unit owner);
	}
}