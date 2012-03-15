namespace WCell.RealmServer.Groups
{
    public partial class Group
    {
        public delegate void GroupMemberHandler(GroupMember member);
        public delegate void GroupLeaderChangedHandler(GroupMember oldLeader, GroupMember newLeader);

        public static event GroupMemberHandler MemberAdded;

        public static event GroupMemberHandler MemberRemoved;

        public static event GroupLeaderChangedHandler LeaderChanged;
    }
}