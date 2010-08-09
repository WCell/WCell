using System;
using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.AI.Actions.States
{
	/// <summary>
	/// An action of NPCs that selects targets, according to Threat and other factors and then
	/// updates the given <see cref="Strategy"/> to kill the current target.
	/// </summary>
	public class AICombatAction : AIAction, IAIStateAction
	{
		/// <summary>
		/// Only check for a Threat update every 20 ticks
		/// </summary>
		public static int ReevaluateThreatTicks = 20;

		private static Logger log = LogManager.GetCurrentClassLogger();

		protected AIAction m_Strategy;
		private bool m_init;

		public AICombatAction(NPC owner)
			: base(owner)
		{
		}

		public AICombatAction(NPC owner, AIAction combatAction)
			: base(owner)
		{
			m_Strategy = combatAction;
		}

		/// <summary>
		/// Action to be executed after the highest aggro target has been selected.
		/// Start/stop is called everytime the Target changes.
		/// </summary>
		public AIAction Strategy
		{
			get { return m_Strategy; }
			set { m_Strategy = value; }
		}

		/// <summary>
		/// If true, the owner wants to retreat from combat and go back to its AttractionPoint
		/// due to too big distance and not being hit or hitting itself
		/// </summary>
		public bool WantsToRetreat
		{
			get
			{
				return
					m_owner is NPC &&
					((NPC)m_owner).CanEvade &&
					!m_owner.IsInRadiusSq(m_owner.Brain.SourcePoint, NPCMgr.DefaultMaxHomeDistanceInCombatSq) &&
					   ((((NPC)m_owner).Entry.Rank >= CreatureRank.RareElite) ||
						Environment.TickCount - m_owner.LastCombatTime > NPCMgr.GiveUpCombatDelay);
			}
		}

		public override void Start()
		{
			m_init = false;
			m_owner.Movement.MoveType = AIMoveType.Run;
		}

		public override void Update()
		{
			if (m_owner.IsUsingSpell || !m_owner.CanDoHarm)
			{
				// busy
				return;
			}

			var owner = (NPC)m_owner;

			if (WantsToRetreat)
			{
				m_owner.Brain.State = BrainState.Evade;
			}
			else
			{
				if (owner.Target == null ||
					!m_owner.CanBeAggroedBy(owner.Target) ||
					owner.CheckTicks(ReevaluateThreatTicks))
				{
					Unit target;
					while ((target = owner.ThreatCollection.CurrentAggressor) != null)
					{
						// if target is dead or gone, check for other targets or retreat
						if (!m_owner.CanBeAggroedBy(target))
						{
							// remove dead and invalid targets from aggro list
							owner.ThreatCollection.Remove(target);
						}
						else
						{
							if (m_Strategy == null)
							{
								// no action set - must not happen
								log.Error("Executing " + GetType().Name + " without having a Strategy set.");
							}
							else
							{
								if (owner.Target != target || !m_init)
								{
									m_init = true;

									// change target and start Action again
									owner.Target = target;
									owner.IsFighting = true;
									m_Strategy.Start();
								}
								else
								{
									m_Strategy.Update();
								}
								return;
							}
						}
					}
				}
				else
				{
					if (!m_init)
					{
						m_Strategy.Start();
					}
					else
					{
						m_Strategy.Update();
					}
					return;
				}

				// no one left to attack
				if (owner.CanEvade)
				{
					// evade
					if (Math.Abs(Environment.TickCount - owner.LastCombatTime) > NPCMgr.CombatEvadeDelay)
					{
						// check if something came up again
						if (!m_owner.Brain.CheckCombat())
						{
							// run back
							owner.Brain.State = BrainState.Evade;
						}
					}
				}
				else
				{
					// cannot evade -> Just go back to default if there are no more targets
					if (!owner.Brain.CheckCombat())
					{
						// go back to what we did before
						owner.Brain.EnterDefaultState();
					}
				}
			}
		}

		public override void Stop()
		{
			((NPC)m_owner).ThreatCollection.Clear();
			if (m_Strategy != null)
			{
				m_Strategy.Stop();
			}

			m_owner.IsInCombat = false;
			m_owner.Target = null;

			m_owner.MarkUpdate(UnitFields.DYNAMIC_FLAGS);
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.HighPriority; }
		}
	}
}