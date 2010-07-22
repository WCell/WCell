using NLog;
using WCell.RealmServer.Content;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Transports;
using WCell.Util;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOMOTransportEntry : GOEntry
	{
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The TaxiPathId from TaxiPaths.dbc
		/// </summary>
		public int TaxiPathId
		{
			get { return Fields[0]; }
		}

		/// <summary>
		/// The speed this object moves at.
		/// </summary>
		public int MoveSpeed
		{
			get { return Fields[1]; }
		}

		/// <summary>
		/// The rate this object accelerates at.
		/// </summary>
		public int AccelRate
		{
			get { return Fields[2]; }
		}

		/// <summary>
		/// The Id of an Event to call when this object is activated (?)
		/// </summary>
		public int StartEventId
		{
			get { return Fields[3]; }
		}

		/// <summary>
		/// The Id of an Event to call when this object is deactivated (?)
		/// </summary>
		public int StopEventId
		{
			get { return Fields[4]; }
		}

		/// <summary>
		/// Ref to TransportPhysics.dbc
		/// </summary>
		public int TransportPhysics
		{
			get { return Fields[5]; }
		}

		/// <summary>
		/// The Id of a Map this object is associated with (?)
		/// </summary>
		public int MapId
		{
			get { return Fields[6]; }
		}

		public int WorldState1
		{
			get { return Fields[7]; }
		}

		protected TaxiPath m_path;

		public TaxiPath Path
		{
			get { return m_path; }
			internal set { m_path = value; }
		}

		public override void FinalizeDataHolder()
		{
			m_path = TaxiMgr.PathsById.Get((uint)TaxiPathId);

			TransportEntry transportEntry;

			TransportMgr.TransportEntries.TryGetValue(GOId, out transportEntry);

			if (m_path == null)
			{
				ContentHandler.OnInvalidDBData("GOEntry for MOTransport \"{0}\" has invalid Path-id (Field 0): " + TaxiPathId, this);
			}
			else
			{
				var movement = new TransportMovement(this, transportEntry == null ? 0 : transportEntry.Period);
				base.FinalizeDataHolder();
			}
		}

		public override bool IsTransport
		{
			get
			{
				return true;
			}
		}

		protected internal override void InitGO(GameObject go)
		{
			base.InitGO(go);

			//((Transport)go).GenerateWaypoints(Path);
		}
	}
}