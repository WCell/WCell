using System;

namespace MPQNav.MPQ.WMO.Components
{
    /// <summary>
    /// Triangle Material Information
    /// </summary>
    public struct MOPY
    {
        /// <summary>
        /// MaterialFlags that affect the triangles in this group.
        /// </summary>
        public MaterialFlags Flags;
        /// <summary>
        /// an index into the material table in the root WMO file's MOMT chunk
        /// If -1, this tri is used to make a collision mesh (guess)
        /// </summary>
        public byte MaterialIndex;

        /// <summary>
        /// Affect how a WMOGroup Triangle is handled
        /// </summary>
        [Flags]
        public enum MaterialFlags : byte
        {
            /// <summary>
            /// No flags set
            /// </summary>
            None = 0,
            /// <summary>
            /// The camera will not collide with this triangle
            /// </summary>
            NoCamCollide = 1,
            /// <summary>
            /// ???
            /// </summary>
            Detail = 2,
            /// <summary>
            /// This triangle is not used in collision detection
            /// </summary>
            NoCollision = 4,
            /// <summary>
            /// ???
            /// </summary>
            Hint = 8,
            /// <summary>
            /// Render this triangle
            /// </summary>
            Render = 0x10,
            /// <summary>
            /// Collision
            /// </summary>
            CollideHit = 0x20,
            /// <summary>
            /// This triangle is part of a wall
            /// </summary>
            WallSurface = 0x40,
            /// <summary>
            /// ???
            /// </summary>
            Flag_0x80 = 0x80,


            CollisionMask = Hint | CollideHit,
        }
    }
}
