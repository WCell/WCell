using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Transports
{
	public class TransportMgr
	{
		public static bool Loaded
		{
			get;
			private set;
		}

		public static Dictionary<GOEntryId, Transport> Transports = new Dictionary<GOEntryId, Transport>();
		public static Dictionary<GOEntryId, TransportEntry> TransportEntries = new Dictionary<GOEntryId, TransportEntry>();

		[Initialization(InitializationPass.Sixth, "Initialize Transports")]
		public static void Initialize()
		{
			//#if !DEV
			LoadTransportEntries();
			//#endif
		}

		public static void LoadTransportEntries()
		{
			if (Loaded) return;

			ContentMgr.Load<TransportEntry>();

			Loaded = true;
		}

		public static bool SpawnTransport(GOEntryId entryId)
		{
			TransportEntry entry;

			if (!TransportEntries.TryGetValue(entryId, out entry))
				return false;



			return true;
		}
	}
}