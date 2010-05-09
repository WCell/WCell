using NLog;
using WCell.Constants.GameObjects;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;
using WCell.Constants.Looting;

namespace WCell.RealmServer.GameObjects.Handlers
{
	/// <summary>
	/// GO Type 3
	/// </summary>
	public class ChestHandler : GameObjectHandler
	{
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			if (m_go.Entry.IsConsumable)
			{
				m_go.State = GameObjectState.Disabled;
			}

			if (m_go.Loot != null)
			{
				LootHandler.SendLootResponse(user, m_go.Loot);
			}
			else
			{
				LootMgr.CreateAndSendObjectLoot(m_go, user, LootEntryType.GameObject, user.Region.IsHeroic);
			}
			return true;
		}
	}
}
