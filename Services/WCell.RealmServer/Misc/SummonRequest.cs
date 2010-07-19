using System;
using WCell.RealmServer.Global;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Misc
{
	public class SummonRequest
	{
		/// <summary>
		/// Timeout in seconds
		/// </summary>
		public static int DefaultTimeout = 120;

		public WorldObject Portal;
		public DateTime ExpiryTime;
		public Region TargetRegion;
		public Vector3 TargetPos;
		public Zone TargetZone;
	}
}