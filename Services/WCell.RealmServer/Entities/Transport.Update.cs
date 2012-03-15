using WCell.Constants.Updates;
using WCell.Core.Network;

namespace WCell.RealmServer.Entities
{
    public partial class Transport
    {
        public override UpdateFlags UpdateFlags
        {
            get
            {
                return UpdateFlags.Flag_0x10 | UpdateFlags.Transport | UpdateFlags.StationaryObject | UpdateFlags.HasRotation;
            }
        }

        protected override void WriteMovementUpdate(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            // UpdateFlag.HasPosition
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Position.Z);
            writer.WriteFloat(Orientation);
        }

        protected override void WriteUpdateFlag_0x10(PrimitiveWriter writer, UpdateFieldFlags relation)
        {
            writer.Write(150754760); // ?
        }
    }
}