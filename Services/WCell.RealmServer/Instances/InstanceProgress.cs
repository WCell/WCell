using System;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
	/// <summary>
	/// Defines the progress inside an instance that can also be saved to DB
	/// </summary>
	public class InstanceProgress : IMapId //: WCellRecord<InstanceProgress>
	{
		//[Field("InstanceId", NotNull = true, Access = PropertyAccess.Field)]
		private int m_InstanceId;

		private InstanceProgress()
		{
			
		}

		public InstanceProgress(MapId mapId, uint instanceId)
		{
			MapId = mapId;
			InstanceId = instanceId;
		}

		public int Id { get; set; }

		//[Property(NotNull = true)]
		public MapId MapId
		{
			get;
			set;
		}

		public uint InstanceId
		{
			get { return (uint) m_InstanceId; }
			set { m_InstanceId = (int) value; }
		}

		//[Property(NotNull = true)]
		public uint DifficultyIndex
		{
			get;
			set;
		}

		//[Property]
		public DateTime ResetTime
		{
			get;
			set;
		}

		//[Property]
		/// <summary>
		/// The version number, so we know how CustomData looks like, even if we changed the layout
		/// </summary>
		public int CustomDataVersion
		{
			get;
			set;
		}

		//[Property]
		/// <summary>
		/// Instance-dependent data: Many instances need custom data saved to DB, they can use this field for that.
		/// TODO: Use SerializerCollection to make this safe to use
		/// </summary>
		public byte[] CustomData
		{
			get;
			set;
		}
	}
}
