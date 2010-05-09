using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Actions.Combat;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI
{
	public class DefaultAIActionCollection : AIActionCollection
	{

		/// <summary>
		/// Method is called before the Unit is actually in the World
		/// to initialize the set of Actions.
		/// </summary>
		/// <param name="owner"></param>
		public override void Init(Unit owner)
		{
			base.Init(owner);

			this[BrainState.Idle] = new AIIdleAction(owner);
			this[BrainState.Dead] = this[BrainState.Idle];

			this[BrainState.Evade] = new AIEvadeAction(owner);
			this[BrainState.Roam] = new AIRoamAction(owner);

			this[BrainState.Follow] = new AIFollowMasterAction(owner);
			this[BrainState.Guard] = new AIGuardMasterAction(owner);

			if (owner is NPC)
			{
				this[BrainState.Combat] = new AICombatAction((NPC)owner) {
					Strategy = new AIAttackAction((NPC)owner)
				};

				this[BrainState.FormationMove] = new AIFormationMoveAction((NPC)owner);
			}
		}
	}
}
