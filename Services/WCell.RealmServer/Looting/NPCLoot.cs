using WCell.Constants.Looting;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Looting
{
    public class NPCLoot : Loot
    {
        public NPCLoot()
        {
        }

        public NPCLoot(ILootable looted, uint money, LootItem[] items)
            : base(looted, money, items)
        {
        }

        public NPCLoot(ILootable looted, uint money, ItemStackTemplate[] items)
            : base(looted, money, LootItem.Create(items))
        {
        }

        public override LootResponseType ResponseType
        {
            get { return LootResponseType.Default; }
        }
    }
}

//public static void GetInitialLooters(Character initialLooter, ILootable lootable, ICollection<LooterEntry> looters)
//{
//    var groupMember = initialLooter.GroupMember;
//    if (groupMember != null)
//    {
//        var group = groupMember.Group;
//        var method = group.LootMethod;
//        var usesRoundRobin = method != LootMethod.FreeForAll &&
//            (LootMgr.RoundRobinDefault || method == LootMethod.RoundRobin);

//        if (usesRoundRobin)
//        {
//            var member = group.GetNextRoundRobinMember();
//            if (member != null)
//            {
//                looters.Add(member.Character.LooterEntry);
//            }
//        }
//        else
//        {
//            GetNearbyLooters(lootable, initialLooter.Group, initialLooter, looters);
//        }

//        var decision = method == LootMethod.MasterLoot ? LootDecision.Master : LootDecision.Rolling;

//        // TODO: masterlooter
//        foreach (var item in Items)
//        {
//            // TODO: Check whether Item is valid for char (check quest requirements etc)
//            if ((item.Template.Flags & ItemFlags.MultiLoot) != 0)
//            {
//                if (nearbyLooters == null)
//                {
//                    // if we didn't fetch all nearby looters yet, we have to do so now
//                    nearbyLooters = new List<LooterEntry>();
//                    GetNearbyLooters(initialLooter, nearbyLooters);
//                }

//                item.AddMultiLooters(nearbyLooters);
//            }
//            else if (item.Template.Quality >= Threshold)
//            {
//                if (nearbyLooters == null)
//                {
//                    // if we didn't fetch all nearby looters yet, we have to do so now
//                    nearbyLooters = new List<LooterEntry>();
//                    GetNearbyLooters(initialLooter, nearbyLooters);
//                }

//                item.Decision = decision;
//                if (decision == LootDecision.Rolling)
//                {
//                    item.RollProgress = new LootRollProgress(this, item, nearbyLooters);
//                    LootHandler.SendStartRoll(this, item, nearbyLooters);
//                }
//            }
//        }
//    }
//    else
//    {
//        Method = LootMethod.FreeForAll;
//        Looters.Add(initialLooter.LooterEntry);
//    }
//}