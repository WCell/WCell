//using System;
//using System.Collections.Generic;
//using System.Text;
//using WCell.RealmServer.AI.Brains;
//using WCell.RealmServer.Entities;

//namespace WCell.RealmServer.AI.Actions
//{
//    /// <summary>
//    /// Class for wrapping multiple actions together
//	  /// TODO: Needs an overhaul. Implement Actions that have a property indicating their state
//    /// </summary>
//    public class AICompositeAction : AIAction
//    {
//        protected AIAction[] m_actions;
//        protected int m_idx;
//        protected bool m_curActionStarted;

//        internal AICompositeAction(Unit owner, AIAction[] actions)
//            : this(owner)
//        {
//            m_actions = actions;

//            foreach (AIAction action in m_actions)
//            {
//                if (action.Owner != owner)
//                    throw new ArgumentException("AICompositeAction constructor called with action argument belonging to other Brain (" +
//                                                action.Owner + " instead of " + owner);
//            }
//        }

//        protected AICompositeAction(Unit owner)
//            : base(owner)
//        {
//        }

//        /// <summary>
//        /// Start executing current action
//        /// </summary>
//        /// <returns></returns>
//        public override void Start()
//        {
//            InternalUpdate();
//        }

//        public override void Update()
//        {
//            InternalUpdate();
//        }

//        private void InternalUpdate()
//        {
//            if (m_actions == null || m_actions.Length == 0)
//                return;

//            UpdateSubaction();
//        }

//        private AIActionResult UpdateSubaction()
//        {
//            AIActionResult result;
//            if (!m_curActionStarted)
//            {
//                result = m_actions[m_idx].Start();
//                m_curActionStarted = true;
//            }
//            else
//            {
//                result = m_actions[m_idx].Update();
//            }
//            return result;
//        }

//        public override void Stop()
//        {
//            m_actions[m_idx].Stop();
//        }
//    }
//}