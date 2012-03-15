using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.Handlers
{
    /// <summary>
    /// If you want to attach a custom GossipMenu to a GO, simply use GameObject.GossipMenu and/or GOEntry.GossipMenu
    /// </summary>
    public class GossipGOHandler : GameObjectHandler
    {
        public override bool Use(Character user)
        {
            return true;
        }
    }
}