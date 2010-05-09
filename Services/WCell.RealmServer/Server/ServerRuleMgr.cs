//using System;
//using WCell.Core.Initialization;
//using WCell.RealmServer.Entities;
//using WCell.Util;

//namespace WCell.RealmServer.Server
//{
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <returns>whether to ignore rulebreak.</returns>
//    public delegate bool RuleBreakHandler(Character abuser);
//    public delegate bool RuleBreakGlobalHandler(Character abuser, ServerRule rule);
	
//    /// <summary>
//    /// Global container and utility class to manager and handler cheaters, abusers and other kind of rule-breakers.
//    /// </summary>
//    public static class ServerRuleMgr
//    {
//        /// <summary>
//        /// Event is fired whenever anyone breaks a rule
//        /// </summary>
//        public static event RuleBreakGlobalHandler RuleBroken;

//        static ServerRule[] m_rules;

//        /// <summary>
//        /// All default rules.
//        /// </summary>
//        public static ServerRule[] Rules
//        {
//            get
//            {
//                return m_rules;
//            }
//        }


//        [Initialization(InitializationPass.Fourth)]
//        public static void Initialize()
//        {
//            m_rules = new ServerRule[(int)Utility.GetMaxEnum<ServerRuleType>() + 1];
			
//            var rules = (ServerRuleType[])Enum.GetValues(typeof(ServerRuleType));

//            for (int i = 0; i < rules.Length; i++)
//            {
//                m_rules[(int)rules[i]] = new ServerRule(rules[i]);
//            }
//        }

//        /// <summary>
//        /// Gets the Rule definition of this RuleType.
//        /// </summary>
//        public static ServerRule GetRule(ServerRuleType rule)
//        {
//            return m_rules.Get((uint)rule);
//        }

//        /// <summary>
//        /// Trigger the rulebreak.
//        /// </summary>
//        /// <returns>whether to to ignore this rulebreak or avoid it.</returns>
//        public static bool Trigger(Character ruleBreaker, ServerRuleType ruleType)
//        {
//            var rule = GetRule(ruleType);
//            if (rule != null)
//            {
//                rule.Trigger(ruleBreaker);
//            }
//            return true;
//        }
//    }
//}
