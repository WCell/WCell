using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AreaTriggers
{
    public partial class AreaTrigger
    {
        public delegate void ATUseHandler(AreaTrigger at, Character triggerer);

        public event ATUseHandler Triggered;
    }
}