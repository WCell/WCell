//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.AI.Brains;
//using WCell.Util;
//using WCell.RealmServer.AI.Actions.Movement;
//using WCell.RealmServer.Entities;

//namespace WCell.RealmServer.AI.Actions
//{
//    /// <summary>
//    /// Provide Actions to a Brain using AIActionCreators
//    /// to provide Actions
//    /// </summary>
//    public class AIActionCreatorCollection : IAIActionCreatorCollection
//    {
//        private Unit m_owner;

//        /// <summary>
//        /// The tuple containing a Factory and its Evalutor
//        /// </summary>
//        public class CreatorEvaluatorTuple
//        {
//            public AIActionCreator Creator;
//            public AIActionEvaluator Evalutor; 
//        }

//        protected Dictionary<BrainState, List<CreatorEvaluatorTuple>> m_dictionary;

//        public AIActionCreatorCollection()
//        {
//            m_dictionary = new Dictionary<BrainState, List<CreatorEvaluatorTuple>>();
//        }

//        public void AddCreator(BrainState state, AIActionCreator creator)
//        {
//            AddCreator(state, creator, (owner) => 0);
//        }

//        public void AddCreator(BrainState state, AIActionCreator factory, AIActionEvaluator evalutor)
//        {
//            var tuple = new CreatorEvaluatorTuple() {
//                Evalutor = evalutor,
//                Creator = factory
//            };

//            m_dictionary.GetOrCreate(state).Add(tuple);
//        }

//        public void AddCreator(BrainState state, CreatorEvaluatorTuple tuple)
//        {
//            m_dictionary.GetOrCreate(state).Add(tuple);
//        }

//        public void AddApproachAndExecuteFactory(BrainState state,
//                                                 AITargetedActionCreator targetedActionFactory)
//        {
//            AddCreator(state, (owner) => new AIApproachAndExecuteAction(owner, targetedActionFactory(owner)));
//        }

//        public void AddApproachAndExecuteFactory(BrainState state,
//            AITargetedActionCreator targetedActionCreator,
//            AIActionEvaluator evalutor)
//        {
//            AddCreator(state, (owner) => new AIApproachAndExecuteAction(owner, targetedActionCreator(owner)),
//                       evalutor);
//        }

//        public AIAction this[BrainState state]
//        {
//            get
//            {
//                List<CreatorEvaluatorTuple> tupleList;
//                m_dictionary.TryGetValue(state, out tupleList);
//                return GetAction(m_owner, tupleList);
//            }
//        }

//        public virtual void Init(Unit unit)
//        {
//            m_owner = unit;
//        }

//        public bool Contains(BrainState state)
//        {
//            return m_dictionary.ContainsKey(state);
//        }

//        protected AIAction GetAction(Unit owner, List<CreatorEvaluatorTuple> tupleList)
//        {
//            if (tupleList == null)
//                return null;

//            int count = tupleList.Count;

//            if (count == 0)
//                return null;

//            if (count == 1)
//            {
//                var tuple = tupleList[0];

//                if (tuple.Evalutor(owner) >= 0)
//                    return tuple.Creator(owner);
//                return null;
//            }

//            var currentBestEvalution = -1;
//            var factories = new List<AIActionCreator>();

//            foreach (var tuple in tupleList)
//            {
//                int evalution = tuple.Evalutor(owner);

//                if (evalution >= 0)
//                {
//                    if (currentBestEvalution == evalution)
//                        factories.Add(tuple.Creator);
//                    else if (currentBestEvalution < evalution)
//                    {
//                        currentBestEvalution = evalution;
//                        factories.Clear();
//                        factories.Add(tuple.Creator);
//                    }
//                }
//            }

//            if (factories.Count == 0)
//                return null;

//            if (factories.Count == 1)
//                return factories[0](owner);

//            return factories[Utility.Random(0, factories.Count - 1)](owner);
//        }
//    }

//    public class DefaultAIActionCreatorCollection : AIActionCreatorCollection
//    {
//        //public static readonly DefaultAIActionCreatorCollection Instance = new DefaultAIActionCreatorCollection();

//        //public DefaultAIActionCreatorCollection()
//        //{
//        //    // Moving
//        //    //AddCreator(AIActionType.MoveToPoint, (owner) => new AIPointMovementAction(owner));
//        //    //AddCreator(AIActionType.MoveToTarget, (owner) => new AIMoveToTargetAction(owner));
//        //    AddCreator(BrainState.Retreat, (owner) => new AIRetreatAction(owner));
//        //    AddCreator(BrainState.Roam, (owner) => new AIRoamAction(owner));
//        //    AddCreator(BrainState.Follow, (owner) => new AIFollowAction(owner));

//        //    AddApproachAndExecuteFactory(BrainState.Combat,
//        //        (brain) => new AIAttackAction(brain),
//        //        (brain) => {
//        //            var owner = brain.Owner;

//        //            if (owner.Target == null)
//        //                return -1;

//        //            float distanceSq = owner.GetDistanceSq(owner.Target);

//        //            float minRange = owner.MainWeapon.MinRange,
//        //                  maxRange = owner.MainWeapon.MaxRange;

//        //            minRange *= minRange;
//        //            maxRange *= maxRange;

//        //            if (distanceSq >= minRange && distanceSq <= maxRange)
//        //                return 100;
//        //            else if (distanceSq < minRange)
//        //                return 50;
//        //            else
//        //                return 0;
//        //        });
//        //}
//    }
//}
