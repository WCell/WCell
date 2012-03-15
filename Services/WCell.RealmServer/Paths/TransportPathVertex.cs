using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.Core.Terrain.Paths
{
    public struct TransportPathVertex
    {
        public MapId MapId;

        public Vector3 Position;

        public bool Teleport;

        public uint Time;
    }
}