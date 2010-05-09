using System;

namespace WCell.Collision
{
    internal class TileReference
    {
        private DateTime m_expireTime = DateTime.Now;

        public WorldMapTile Tile
        {
            get;
            set;
        }

        public object LoadLock
        {
            get;
            private set;
        }

        public bool Broken
        {
            get;
            set;
        }

        public bool Expired
        {
            get { return m_expireTime < DateTime.Now; }
        }

        public TileReference(WorldMapTile tile)
        {
            Tile = tile;
            LoadLock = new object();
            KeepAlive();
        }

        public TileReference()
            : this(null)
        {
        }

        public void KeepAlive()
        {
            m_expireTime = DateTime.Now.AddMinutes(4);
        }
    }
}