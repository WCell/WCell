using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.WMO.Components
{
    public enum FogFlags
    {
        F_IEBLEND = 0x1
    }

    // SMOFog
    public class Fog
    {
        public FogFlags Flags;
        public Vector3 Position;
        public float Start;
        public float End;
        public FogInfo FogInfo_FOG;
        public FogInfo FogInfo_UWFOG;
    }

    // SMOFog::Fog
    public struct FogInfo
    {
        public float End;
        public float StartScalar;
        public Color4 Color;
    }
}
