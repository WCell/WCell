using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// GO Type 6
	/// </summary>
	public class TrapHandler : GameObjectHandler
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		protected internal override void Initialize(GameObject go)
		{
		    //var hidden = (go.Entry as GOTrapEntry).Hidden;
			//if (hidden)
			{
				// hide go
			}
		}

		public override bool Use(Character user)
		{
			return true;
		}
	}
}
