using System;
using NLog;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.AI.Actions.States;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Brains
{
	/// <summary>
	/// The default class for monsters' AI
	/// </summary>
	public class BaseBrain : IBrain
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static BrainState DefaultBrainState = BrainState.Roam;

		/// <summary>
		/// Current state
		/// </summary>
		protected BrainState m_state;

		/// <summary>
		/// Default state
		/// </summary>
		protected BrainState m_defaultState;

		protected Unit m_owner;

		/// <summary>
		/// Actions to be executed in the idle state
		/// </summary>
		protected Vector3 m_SourcePoint;

		protected IAIActionCollection m_actions;
		protected IAIAction m_currentAction;

		protected bool m_IsAggressive;

		protected bool m_IsRunning;

		#region Constructors

		public BaseBrain(Unit owner)
			: this(owner, new DefaultAIActionCollection(), DefaultBrainState)
		{
		}

		public BaseBrain(Unit owner, BrainState defaultState)
			: this(owner, new DefaultAIActionCollection(), defaultState)
		{
		}

		public BaseBrain(Unit owner, IAIActionCollection actions)
			: this(owner, actions, BrainState.Idle)
		{
		}

		public BaseBrain(Unit owner, IAIActionCollection actions, BrainState defaultState)
		{
			m_owner = owner;

			m_defaultState = defaultState;
			m_state = m_defaultState;

			m_actions = actions;

			m_IsAggressive = true;
		}

		#endregion

		#region Properties
		public Unit Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Owner as NPC.
		/// Returns null if Owner is not an NPC
		/// </summary>
		public NPC NPC
		{
			get { return m_owner as NPC; }
		}

		public IAIAction CurrentAction
		{
			get { return m_currentAction; }
			set
			{
				if (m_currentAction != null)
				{
					//m_currentAction.Stop();
				}
				m_currentAction = value;
				if (value != null)
				{
					if (m_currentAction.IsGroupAction && (!(m_owner is NPC) || ((NPC)m_owner).Group == null))
					{
						log.Error("{0} tried to execute {1} but is not in Group.", m_owner, m_currentAction);
						m_currentAction = null;
						return;
					}
					m_currentAction.Start();
				}
			}
		}

		public BrainState State
		{
			get { return m_state; }
			set
			{
				if (m_state == value && m_currentAction != null)
				{
					return;
				}

				if (!m_owner.IsInWorld)
				{
					m_state = value;
					return;
				}

				var action = m_actions[value];
				if (action == null)
				{
					if (m_state != m_defaultState)
					{
						m_state = m_defaultState;
						State = m_defaultState;
					}
				}
				else
				{
#if DEBUG
					//m_owner.Say(m_state + " => " + value);
#endif
					m_state = value;

					if (m_currentAction != null)
					{
						m_currentAction.Stop();
					}
					CurrentAction = action;
				}
			}
		}

		/// <summary>
		/// The State to fall back to when nothing else is up.
		/// </summary>
		public BrainState DefaultState
		{
			get { return m_defaultState; }
			set
			{
				var shouldEnter = m_defaultState == m_state;
				m_defaultState = value;
				if (shouldEnter)
				{
					State = value;
				}
			}
		}

		public UpdatePriority UpdatePriority
		{
			get { return m_IsRunning && m_currentAction != null ? m_currentAction.Priority : UpdatePriority.Background; }
		}

		public bool IsRunning
		{
			get { return m_IsRunning; }
			set
			{
				if (m_IsRunning == value)
					return;

				if (value)
					Start();
				else
					Stop();
			}
		}

		/// <summary>
		/// Collection of all actions that this brain can execute
		/// </summary>
		public IAIActionCollection Actions
		{
			get { return m_actions; }
		}

		/// <summary>
		/// The point of attraction where we took off when we started with the
		/// last action
		/// </summary>
		public Vector3 SourcePoint
		{
			get { return m_SourcePoint; }
			set { m_SourcePoint = value; }
		}

		public bool IsAggressive
		{
			get { return m_IsAggressive; }
			set { m_IsAggressive = value; }
		}

		/// <summary>
		/// Returns the default AIAction for the Combat BrainState (to be executed when using State = BrainState.Combat).
		/// Returns null if that Action is not an AICombatAction - In that case use Actions[BrainState.Combat] instead.
		/// </summary>
		public AICombatAction DefaultCombatAction
		{
			get { return m_actions[BrainState.Combat] as AICombatAction; }
		}
		#endregion

		/// <summary>
		/// Updates the AIAction by calling Perform. Called every tick by the Region
		/// </summary>
		/// <param name="dt">not used</param>
		public virtual void Update(int dt)
		{
			if (!m_IsRunning)
				return;

			//if (m_owner.IsAlive && m_owner.Health < 1)
			//{
			//    log.Error(m_owner + " has IsAlive property set to true, but has " + m_owner.Health + " health!");
			//}
			Perform();
		}

		public void EnterDefaultState()
		{
			State = m_defaultState;
		}

		#region Handlers
		#endregion

		protected virtual void Start()
		{
			m_IsRunning = true;
		}

		protected virtual void Stop()
		{
			m_IsRunning = false;

			StopCurrentAction();
		}

		public void StopCurrentAction()
		{
			if (m_currentAction != null)
			{
				m_currentAction.Stop();
			}
			m_currentAction = null;
		}

		/// <summary>
		/// Performs a full Brain cycle
		/// </summary>
		public void Perform()
		{
			// update current Action if any
			if (m_currentAction == null)
			{
				m_currentAction = m_actions[m_state];
				if (m_currentAction == null)
				{
					// no Action found for current state
					State = m_defaultState;
					return;
				}
				m_currentAction.Start();
			}
			else
			{
				m_currentAction.Update();
			}
		}

		#region Events Handlers
		public virtual void OnEnterCombat()
		{
		}

		public virtual void OnLeaveCombat()
		{
		}

		public virtual void OnHeal(Unit healer, Unit healed, int amtHealed)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		public virtual void OnDamageReceived(IDamageAction action)
		{
		}

		public virtual void OnDamageDealt(IDamageAction action)
		{
		}

		public virtual void OnDebuff(Unit caster, SpellCast cast, Aura debuff)
		{
		}

		public virtual void OnKilled(Unit killerUnit, Unit victimUnit)
		{
		}

		public virtual void OnDeath()
		{
			State = BrainState.Dead;
		}

		/// <summary>
		/// Called when entering the World and when resurrected
		/// </summary>
		public virtual void OnActivate()
		{
			if (!m_actions.IsInitialized)
			{
				// execute only on world enter (not on resurrect)
				m_actions.Init(m_owner);
			}
			m_SourcePoint = m_owner.Position;
			CurrentAction = m_actions[m_state];
		}

		public virtual void OnCombatTargetOutOfRange()
		{
		}

		#endregion

		#region Combat
		public virtual bool CheckCombat()
		{
			if (!(m_owner is NPC))
			{
				return false;
			}

			if (!m_owner.CanDoHarm)
			{
				return false;
			}

			if (!m_owner.IsAreaActive && !m_owner.Region.ScanInactiveAreas)
			{
				// don't scan inactive Nodes
				return false;
			}

			var owner = (NPC)m_owner;

			// attack highest threat unit or look for possible new targets
			if ((owner.ThreatCollection.CurrentAggressor != null && owner.CanReachForCombat(owner.ThreatCollection.CurrentAggressor)) ||
				 (m_IsAggressive && ScanAndAttack()))
			{
				owner.Brain.State = BrainState.Combat;
				return true;
			}
			return false;
		}

		public void OnGroupChange(AIGroup newGroup)
		{
			if (newGroup != null)
			{
				// now in new/different group
				DefaultState = newGroup.DefaultState;
				EnterDefaultState();
			}
			else
			{
				// left Group
				DefaultState = DefaultBrainState;
				if (m_currentAction.IsGroupAction)
				{
					m_currentAction.Stop();
					EnterDefaultState();
				}
			}
		}

		/// <summary>
		/// Returns whether it found enemies and started attacking or false if none found.
		/// </summary>
		/// <returns></returns>
		public virtual bool ScanAndAttack()
		{
			if (!(m_owner is NPC))
			{
				return false;
			}
			var owner = (NPC)m_owner;

			// look around for possible enemies to attack (inverted predicate)
			return !owner.IterateEnvironment(Unit.AggroMaxRangeDefault, obj =>
			{
				if (!(obj is Unit))
				{
					return true;
				}

				var unit = (Unit)obj;

                // Do not attack gamemasters unless provoked.
                if(unit is Character)
                {
                    var chr = (Character)unit;
                    if(chr.GodMode)
                    {
                        return true;
                    }
                }
				// targets must be hostile, visible, alive, in range etc
				if (unit.CanGenerateThreat &&
				    m_owner.IsHostileWith(unit) &&
				    m_owner.CanSee(unit) &&
					unit.IsInRadiusSq(owner, owner.GetAggroRangeSq(unit)) &&

					// add this constraint, so NPCs don't randomly attack weak neutrals
					(!(unit is NPC) || ((NPC)unit).ThreatCollection.CurrentAggressor != null || unit.IsHostileWith(owner)))
				{
					owner.ThreatCollection.AddNewIfNotExisted(unit);
					if (owner.CanReachForCombat(unit))
					{
						return false;
					}
				}
				return true;
			});
		}
		#endregion

		public void Dispose()
		{
			m_owner = null;
		}
	}
}