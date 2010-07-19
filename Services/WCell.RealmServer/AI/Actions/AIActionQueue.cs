//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.AI.Actions;

//namespace WCell.RealmServer.AI.Actions
//{
//    /// <summary>
//    /// Unused.
//    /// Queue of AI Actions to be executed
//    /// </summary>
//    public class AIActionQueue : AIAction
//    {
//        private Queue<AIAction> m_queue;
//        private AIAction m_currentAction;

//        public bool IsExecuting
//        {
//            get
//            {
//                return m_currentAction != null;
//            }
//        }

//        public int PendingCount
//        {
//            get { return m_queue.Count; }
//        }

//        //public AIAction Current
//        //{
//        //    get { return m_currentAction; }
//        //    set
//        //    {
//        //        m_currentAction = value;
//        //        Update();
//        //    }
//        //}

//        public AIActionQueue()
//        {
//            m_queue = new Queue<AIAction>();
//        }

//        public void Enqueue(AIAction item)
//        {
//            // Maybe use something like AIPrioritizedAction?
//            throw new NotImplementedException();
//            //if (item == null)
//            //    return;

//            //if (item.IsPrimary && m_queue.Count > 0)
//            //{
//            //    var firstAction = m_queue.Peek();

//            //    if (m_currentAction != null && !m_currentAction.IsPrimary)
//            //    {
//            //        Stop();

//            //        m_currentAction = item;
//            //    }
//            //    else if (firstAction != null && !firstAction.IsPrimary)
//            //    {
//            //        Stop();

//            //        m_currentAction = item;
//            //    }
//            //}
//            //else
//            //    m_queue.Enqueue(item);
//        }

//        public override AIActionResult Start()
//        {
//            return Update();
//        }

//        public override AIActionResult Update()
//        {
//            AIActionResult result;

//            if (m_currentAction == null)
//            {
//                if (m_queue.Count == 0)
//                    return AIActionResult.Success;

//                m_currentAction = m_queue.Dequeue();

//                result = m_currentAction.Start();
//            }
//            else
//            {
//                result = m_currentAction.Update();
//            }

//            switch (result)
//            {
//                case AIActionResult.Success:
//                case AIActionResult.Failure:
//                    m_currentAction = null;
//                    Update();
//                    break;

//                case AIActionResult.Executing:
//                    break;
//            }
//            return result;
//        }

//        public override void Stop()
//        {
//            m_queue.Clear();

//            if (m_currentAction != null)
//            {
//                var action = m_currentAction;
//                m_currentAction = null;
//                action.Stop();
//            }
//        }
//    }
//}