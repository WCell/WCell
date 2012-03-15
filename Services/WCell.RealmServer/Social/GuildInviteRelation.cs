using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Interaction
{
    /// <summary>
    /// Represents a guild invite relationship between two <see cref="Character"/> entities.
    /// </summary>
    public sealed class GuildInviteRelation : BaseRelation
    {
        public GuildInviteRelation(uint charId, uint relatedCharId)
            : base(charId, relatedCharId)
        {
        }

        public override bool RequiresOnlineNotification
        {
            get { return false; }
        }

        public override CharacterRelationType Type
        {
            get { return CharacterRelationType.GuildInvite; }
        }
    }
}