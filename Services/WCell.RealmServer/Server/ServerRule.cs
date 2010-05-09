//using WCell.RealmServer.Entities;

//namespace WCell.RealmServer.Server
//{
//    /// <summary>
//    /// Defines how and a rule was broken and how to handle it
//    /// </summary>
//    public class ServerRule
//    {
//        /// <summary>
//        /// The type of this rule
//        /// </summary>
//        public readonly ServerRuleType RuleType;

//        /// <summary>
//        /// Is called when this Rule gets broken
//        /// </summary>
//        public event RuleBreakHandler RuleBroken;


//        public ServerRule(ServerRuleType type)
//        {
//            RuleType = type;
//        }

//        public bool Trigger(Character ruleBreaker)
//        {
//            var handle = RuleBroken;
//            return handle != null && handle(ruleBreaker);
//        }
//    }
//}
