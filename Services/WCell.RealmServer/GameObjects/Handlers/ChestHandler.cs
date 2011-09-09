using WCell.Constants.GameObjects;
using WCell.Constants.Looting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Looting;

namespace WCell.RealmServer.GameObjects.Handlers
{
	/// <summary>
	/// GO Type 3
	/// </summary>
	public class ChestHandler : GameObjectHandler
	{
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
				LootMgr.CreateAndSendObjectLoot(m_go, user, LootEntryType.GameObject, user.Map.IsHeroic);
			}
			return true;
		}
	}
}