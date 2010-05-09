//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using WCell.Constants;
//using WCell.Core.Initialization;
//using WCell.Util;
//using RulesetPair = System.Collections.Generic.KeyValuePair<WCell.RealmServer.Server.ServerTypeAttribute, WCell.RealmServer.Server.IServerRules>;

//namespace WCell.RealmServer.Server
//{
//    public static class ServerRulesMgr
//    {
//        private static Dictionary<RealmServerType, IServerRules> m_rulesets;

//        static ServerRulesMgr()
//        {
//            m_rulesets = new Dictionary<RealmServerType, IServerRules>();
//        }

//        private static void LoadRulesets()
//        {
//            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
//            {
//                ServerTypeAttribute[] attribs = t.GetCustomAttributes<ServerTypeAttribute>();

//                if (attribs.Length == 0)
//                {
//                    continue;
//                }

//                if (!t.ImplementsType(typeof(IServerRules)))
//                {
//                    continue;
//                }

//                m_rulesets.Add(attribs[0].ServerServerType, Activator.CreateInstance(t) as IServerRules);
//            }
//        }

//        public static bool GetRuleset(RealmServerType serverType, out IServerRules ruleset)
//        {
//            return m_rulesets.TryGetValue(serverType, out ruleset);
//        }

//        [Initialization(InitializationPass.Fifth)]
//        public static void Initialize()
//        {
//            LoadRulesets();
//        }
//    }
//}
