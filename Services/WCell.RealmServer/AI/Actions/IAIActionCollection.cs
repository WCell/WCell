using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
	public delegate AIAction AIActionCreator(Unit owner);

	public delegate AITargetedAction AITargetedActionCreator(Unit owner);

	/// <summary>
	/// Returns a priority to indicate whether an Action is suitable or not:
	/// higher values = better suitability
	/// values smaller 0 = unsuitable
	/// </summary>
	/// <returns></returns>
	public delegate int AIActionEvaluator(Unit owner);

	/// <summary>
	/// An interface to provide Actions using IAIActionCollectionFactory
	/// to provide Actions
	/// </summary>
	public interface IAIActionCreatorCollection : IAIActionCollection
	{
		/// <summary>
		/// Adds a factory
		/// </summary>
		/// <param name="creator">a functor accepting the owner of action and returning an AIAction</param>
		void AddCreator(BrainState state, AIActionCreator creator);

		/// <summary>
		/// Adds a factory with evalution function
		/// </summary>
		/// <param name="creator">a functor accepting the owner of action and returning an AIAction</param>
		/// <param name="evalutor">a functor accepting a brain and returning the suitability of its owner for the action produced by factory</param>
		void AddCreator(BrainState state, AIActionCreator creator, AIActionEvaluator evalutor);
	}

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