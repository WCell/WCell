namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class MirrorImageHandler : AuraEffectHandler
    {
        //Applying this most likely causes the client to send CMSG_GET_MIRRORIMAGE_DATA
        protected override void Apply()
        {
            var chr = Owner.PlayerOwner;
            var image = Owner;
            if (chr != null)
            {
                image.DisplayId = chr.DisplayId;
                image.UnitFlags2 |= Constants.UnitFlags2.MirrorImage;
            }
        }

        protected override void Remove(bool cancelled)
        {
            var chr = Owner.PlayerOwner;
            var image = Owner;
            if (chr != null)
            {
                image.DisplayId = image.NativeDisplayId;
                image.UnitFlags2 ^= Constants.UnitFlags2.MirrorImage;
            }
        }
    }
}
