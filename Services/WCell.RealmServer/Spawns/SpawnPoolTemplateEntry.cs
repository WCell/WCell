using WCell.Util.Data;

namespace WCell.RealmServer.Spawns
{
	[DataHolder]
	public class SpawnPoolTemplateEntry : IDataHolder
	{
		public uint PoolId;

		public int MaxSpawnAmount;

		public void FinalizeDataHolder()
		{
			SpawnMgr.SpawnPoolTemplateEntries.Add(PoolId, this);
		}
	}
}
