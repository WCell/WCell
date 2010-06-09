using System;

namespace WCell.Tools.Maps.Parsing.ADT.Components
{
    /// <summary>
    /// Class for the MCNK chunk (vertex information for the ADT)
    /// </summary>
    public class MCNK
    {
        public int Flags;
        public int IndexX;
        public int IndexY;
        public uint nLayers;
        public uint nDoodadRefs;
        /// <summary>
        /// Offset to the MCVT chunk
        /// </summary>
        public uint ofsHeight;
        /// <summary>
        /// Offset to the MCNR chunk
        /// </summary>
        public uint ofsNormal;
        /// <summary>
        /// Offset to the MCLY chunk
        /// </summary>
        public uint ofsLayer;
        /// <summary>
        /// Offset to the MCRF chunk
        /// </summary>
        public uint ofsRefs;
        /// <summary>
        /// Offset to the MCAL chunk
        /// </summary>
        public uint ofsAlpha;
        public uint sizeAlpha;
        /// <summary>
        /// Offset to the MCSH chunk
        /// </summary>
        public uint ofsShadow;
        public uint sizeShadow;
        public int AreaId;
        public uint nMapObjRefs;

        /// <summary>
        /// Holes of the MCNK
        /// </summary>
        public ushort Holes;

        public ushort[] predTex;
        public byte[] nEffectDoodad;
        
        /// <summary>
        /// Offset to the MCSE chunk
        /// </summary>
        public uint ofsSndEmitters;
        public uint nSndEmitters;
        /// <summary>
        /// Offset to the MCLQ chunk (deprecated)
        /// </summary>
        public uint ofsLiquid;
        public uint sizeLiquid;
        /// <summary>
        /// X position of the MCNK
        /// </summary>
        public float X;
        /// <summary>
        /// Y position of the MCNK
        /// </summary>
        public float Y;
        /// <summary>
        /// Z position of the MCNK
        /// </summary>
        public float Z;
        /// <summary>
        /// Offset to the MCCV chunk, if present
        /// </summary>
        public int offsColorValues; // formerly textureId
        public int props;
        public int effectId;

        public bool[,] GetHolesMap()
        {
            var lret = new bool[4,4];
            for (var i = 0; i < 16; i++)
            {
                lret[i%4, i/4] = (((Holes >> (i)) & 1) == 1);
            }
            return lret;
        }

        public MCNK()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">X position of the MCNK</param>
        /// <param name="y">Y position of the MCNK</param>
        /// <param name="z">Z position of the MCNK</param>
        /// <param name="holes">Holes in this MCNK</param>
        public MCNK(float x, float y, float z,UInt16 holes)
        {
            X = x;
            Y = y;
            Z = z;
            Holes = holes;
        }

            
    }
}