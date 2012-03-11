using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Core.Network;
using WCell.PacketAnalysis.Xml;
using WCell.Util;

namespace WCell.PacketAnalysis
{
    public class PacketDefinition
    {
        public static readonly PacketDefinition Default = new PacketDefinition(new[] { PacketId.Unknown }, PacketSender.Any, (PacketSegmentStructure)null);
        public static readonly PacketDefinition[] Empty = new PacketDefinition[0];

        public readonly PacketId[] PacketIds;
        public readonly PacketSegmentStructure Structure;
        public readonly PacketSender Sender;

        public PacketDefinition(PacketId[] packetIds, PacketSender sender, params PacketSegmentStructure[] structure)
            : this(packetIds, sender, new ComplexPacketSegmentStructure(structure))
        {
        }

        public PacketDefinition(PacketId[] packetIds, PacketSender sender, PacketSegmentStructure structure)
        {
            PacketIds = packetIds;
            Sender = sender;
            Structure = structure ?? new ComplexPacketSegmentStructure();
        }

        public PacketDefinition(PacketId[] packetIds, PacketSender sender, List<PacketSegmentStructure> structure)
            : this(packetIds, sender, new ComplexPacketSegmentStructure(null, structure))
        {
        }

        public PacketDefinition(PacketId packetId, PacketSender sender, params PacketSegmentStructure[] structure)
            : this(new[] { packetId }, sender, new ComplexPacketSegmentStructure(structure))
        {
        }

        public PacketDefinition(PacketId packetId, PacketSender sender, PacketSegmentStructure structure)
        {
            PacketIds = new[] { packetId };
            Sender = sender;
            Structure = structure ?? new ComplexPacketSegmentStructure();
        }

        public PacketDefinition(PacketId packetId, PacketSender sender, List<PacketSegmentStructure> structure)
            : this(new[] { packetId }, sender, new ComplexPacketSegmentStructure(null, structure))
        {
        }

        public PacketSegmentStructure GetSegment(string name)
        {
            return GetSegment(Structure, name);
        }

        public PacketSegmentStructure GetSegment(PacketSegmentStructure segment, string name)
        {
            foreach (var subSegment in segment)
            {
                if (subSegment == null)
                {
                    // from here on segments arent initialized yet -> stop search
                    return null;
                }
                if (subSegment.Name == name)
                {
                    return subSegment;
                }
                var found = GetSegment(subSegment, name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        public XmlPacketDefinition Serialize()
        {
            var first = PacketIds.First();
            if (first.Service == ServiceType.Realm)
            {
                return new RealmPacketDefinition((RealmServerOpCode)first.RawId, Structure);
            }
            return new AuthPacketDefinition((AuthServerOpCode)first.RawId, Structure);
        }

        public override string ToString()
        {
            return PacketIds.ToString(", ") + (Sender != PacketSender.Any ? " (from " + Sender + ")" : "");
        }

        public void Init()
        {
            Structure.Init(this);
            Structure.Index = 0;
        }
    }
}