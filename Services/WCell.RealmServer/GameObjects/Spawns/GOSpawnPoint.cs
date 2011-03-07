using System;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects.Spawns
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns GOs.
	/// </summary>
	public class GOSpawnPoint : SpawnPoint<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>, IWorldLocation
	{
	}
}