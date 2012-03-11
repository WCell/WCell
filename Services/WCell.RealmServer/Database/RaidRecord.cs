using System;
using Castle.ActiveRecord;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Database
{
    [ActiveRecord(Access = PropertyAccess.Property)]
    public class RaidRecord : ActiveRecordBase<RaidRecord>, IMapId
    {
        [Field("CharLowId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
        private int _characterLow;

        [Field("InstanceId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
        private int _instanceId;

        [Field("MapId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
        private int m_MapId;

        [PrimaryKey(PrimaryKeyType.Native, "InstanceRelationId")]
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
            get { return (MapId)m_MapId; }
            set { m_MapId = (int)value; }
        }

        [Property(NotNull = true)]
        public DateTime Until
        {
            get;
            set;
        }
    }
}