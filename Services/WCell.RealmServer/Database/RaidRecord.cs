using System;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Database.Entities
{
    public class RaidRecord : IMapId
	{
		private int _characterLow;

		private int _instanceId;

		private int m_MapId;

        public long RecordId
        {
            get;
            set;
        }

        public uint CharacterLow
        {
			get
			{
				return (uint)_characterLow;
			}
			set
			{
				_characterLow = (int)value;
			}
		}

		public uint InstanceId
		{
			get
			{
				return (uint)_instanceId;
			}
			set
			{
				_instanceId = (int)value;
			}
		}

		public MapId MapId
    	{
    		get { return (MapId) m_MapId; }
    		set { m_MapId = (int) value; }
    	}

		public DateTime Until
		{
			get;
			set;
		}
    }
}