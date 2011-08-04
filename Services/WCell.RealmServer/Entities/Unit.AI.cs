using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Spawns;
using WCell.Util;
using WCell.RealmServer.AI.Actions;
using WCell.Constants.Spells;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	public partial class Unit
	{

		#region AI
		/// <summary>
		/// Whether this Unit can aggro NPCs.
		/// </summary>
		public bool CanGenerateThreat
		{
			get { return IsInWorld && IsAlive && !IsEvading; }
		}

		public abstract LinkedList<WaypointEntry> Waypoints { get; }

		public abstract NPCSpawnPoint SpawnPoint { get; }

		public bool CanBeAggroedBy(Unit target)
		{
			return target.CanGenerateThreat &&
				   IsHostileWith(target) &&
				   CanSee(target);
		}

		/// <summary>
		/// Is called when a Unit successfully evaded (and arrived at its original location)
		/// </summary>
		internal void OnEvaded()
		{
			IsEvading = false;
			if (m_brain != null)
			{
				m_brain.EnterDefaultState();
			}
		}
		#endregion

		#region Actions
		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <remarks>Requires Brain</remarks>
		public void Follow(Unit target)
		{
			if (CheckBrain())
			{
				Target = target;
				m_brain.CurrentAction = new AIFollowTargetAction(this);
			}
		}

		/// <summary>
		/// Moves towards the given target and then executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Vector3 pos, UnitActionCallback actionCallback)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				m_Movement.MoveTo(pos);
				m_brain.CurrentAction = new AIMoveThenExecAction(this, actionCallback);
			}
		}

		/// <summary>
		/// Moves towards the given target and then executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToPointsThenExecute(List<Vector3> points, UnitActionCallback actionCallback)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				m_Movement.MoveToPoints(points);
				m_brain.CurrentAction = new AIMoveThenExecAction(this, actionCallback);
			}
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback)
		{
			MoveToThenExecute(unit, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				Target = unit;
				m_brain.CurrentAction = new AIMoveToThenExecAction(this, actionCallback);
			}
		}

		/// <summary>
		/// Moves in front of the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback)
		{
			MoveInFrontThenExecute(unit, actionCallback, 0);
		}

		/// <summary>
		/// Moves in front of the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback)
		{
			MoveInFrontThenExecute(go, actionCallback, 0);
		}

		/// <summary>
		/// Moves in front of the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveInFrontThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
		{
			MoveToThenExecute(unit, 0f, actionCallback);
		}

		/// <summary>
		/// Moves in front of the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveInFrontThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
		{
			MoveToThenExecute(go, 0f, actionCallback);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback)
		{
			MoveBehindThenExecute(unit, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback)
		{
			MoveBehindThenExecute(go, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveBehindThenExecute(Unit unit, UnitActionCallback actionCallback, int millisTimeout)
		{
			MoveToThenExecute(unit, MathUtil.PI, actionCallback);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveBehindThenExecute(GameObject go, UnitActionCallback actionCallback, int millisTimeout)
		{
			MoveToThenExecute(go, MathUtil.PI, actionCallback);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback actionCallback)
		{
			MoveToThenExecute(unit, angle, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback actionCallback)
		{
			MoveToThenExecute(go, angle, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, float angle, UnitActionCallback callback, int millisTimeout)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();

				Target = unit;
				var action = new AIMoveIntoAngleThenExecAction(this, angle, callback)
				{
					TimeoutMillis = millisTimeout
				};

				m_brain.CurrentAction = action;
			}
		}

		/// <summary>
		/// Moves to the given gameobject and once within default range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(GameObject go, float angle, UnitActionCallback callback, int millisTimeout)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();

				var action = new AIMoveToGameObjectIntoAngleThenExecAction(this, go, angle, callback)
				{
					TimeoutMillis = millisTimeout
				};

				m_brain.CurrentAction = action;
			}
		}

		/// <summary>
		/// Moves to the given target and once within the given range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback)
		{
			MoveToThenExecute(unit, range, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within the given range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback)
		{
			MoveToThenExecute(go, range, actionCallback, 0);
		}

		/// <summary>
		/// Moves to the given target and once within the given range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(Unit unit, SimpleRange range, UnitActionCallback actionCallback, int millisTimeout)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				Target = unit;
				m_brain.CurrentAction = new AIMoveIntoRangeThenExecAction(this, range, actionCallback)
				{
					TimeoutMillis = millisTimeout
				};
			}
		}
		
		/// <summary>
		/// Moves to the given target and once within the given range, executes the given action
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenExecute(GameObject go, SimpleRange range, UnitActionCallback actionCallback, int millisTimeout)
		{
			if (CheckBrain())
			{
				m_brain.CurrentAction = new AIMoveIntoRangeOfGOThenExecAction(this, go, range, actionCallback)
				{
					TimeoutMillis = millisTimeout
				};
			}
		}

		/// <summary>
		/// Moves this Unit to the given position and then goes Idle
		/// </summary>
		/// <param name="pos"></param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenIdle(ref Vector3 pos)
		{
			MoveToThenEnter(ref pos, BrainState.Idle);
		}

		/// <summary>
		/// Moves this Unit to the given position and then goes Idle
		/// </summary>
		/// <param name="pos"></param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenIdle(IHasPosition pos)
		{
			MoveToThenEnter(pos, BrainState.Idle);
		}

		/// <summary>
		/// Moves this Unit to the given position and then assumes arrivedState
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="arrivedState">The BrainState to enter once arrived</param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenEnter(ref Vector3 pos, BrainState arrivedState)
		{
			MoveToThenEnter(ref pos, true, arrivedState);
		}

		/// <summary>
		/// Moves this Unit to the given position and then assumes arrivedState
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="arrivedState">The BrainState to enter once arrived</param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenEnter(ref Vector3 pos, bool findPath, BrainState arrivedState)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				m_Movement.MoveTo(pos, findPath);
				m_brain.CurrentAction = new AIMoveThenEnterAction(this, arrivedState);
			}
		}

		/// <summary>
		/// Moves this Unit to the given position and then assumes arrivedState
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="arrivedState">The BrainState to enter once arrived</param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenEnter(IHasPosition pos, BrainState arrivedState)
		{
			MoveToThenEnter(pos, true, arrivedState);
		}

		/// <summary>
		/// Moves this Unit to the given position and then assumes arrivedState
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="arrivedState">The BrainState to enter once arrived</param>
		/// <remarks>Requires Brain</remarks>
		public void MoveToThenEnter(IHasPosition pos, bool findPath, BrainState arrivedState)
		{
			if (CheckBrain())
			{
				//m_brain.StopCurrentAction();
				m_Movement.MoveTo(pos.Position, findPath);
				m_brain.CurrentAction = new AIMoveThenEnterAction(this, arrivedState);
			}
		}

		/// <summary>
		/// Idles for the given time and then executes the given action.
		/// Also will unset the current Target and stop fighting.
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void IdleThenExecute(int millis, Action action)
		{
			IdleThenExecute(millis, action, ProcTriggerFlags.None);
		}

		/// <summary>
		/// Idles for the given time and then executes the given action.
		/// Also will unset the current Target and stop fighting.
		/// </summary>
		/// <param name="interruptFlags">What can interrupt the action.</param>
		/// <remarks>Requires Brain</remarks>
		public void IdleThenExecute(int millis, Action action, ProcTriggerFlags interruptFlags)
		{
			if (CheckBrain())
			{
				Target = null;
				m_brain.CurrentAction = new AITemporaryIdleAction(millis, interruptFlags, () =>
				{
					m_brain.StopCurrentAction();
					action();
				});
			}
		}

		/// <summary>
		/// Idles for the given time before resuming its normal activities
		/// Also will unset the current Target and stop fighting.
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void Idle(int millis)
		{
			Idle(millis, ProcTriggerFlags.None);
		}

		/// <summary>
		/// Idles until the given flags have been triggered.
		/// Also will unset the current Target and stop fighting.
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void Idle(ProcTriggerFlags interruptFlags)
		{
			Idle(int.MaxValue, interruptFlags);
		}

		/// <summary>
		/// Idles for the given time before resuming its normal activities
		/// Also will unset the current Target and stop fighting.
		/// </summary>
		/// <remarks>Requires Brain</remarks>
		public void Idle(int millis, ProcTriggerFlags interruptFlags)
		{
			if (CheckBrain())
			{
				Target = null;
				m_brain.CurrentAction = new AITemporaryIdleAction(millis, interruptFlags, () =>
				{
					m_brain.StopCurrentAction();
				});
			}
		}

		protected bool CheckBrain()
		{
			if (m_brain == null)
			{
				// TODO: Just add a brain and enforce movement anyway?
				//m_brain = new BaseBrain(this);
				//UnitFlags |= UnitFlags.Influenced;
				Say("I do not have a Brain.");
				return false;
			}
			return true;
		}
		#endregion
	}
}