using WCell.Constants.Quests;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Quests
{
	public class QuestGossipInfo
	{
		public uint QuestId;
		public QuestStatus Status;
		public string Title;

		public QuestGossipInfo(QuestTemplate qt, IRealmClient client)
		{
			QuestId = qt.Id;
			//Status = QuestMgr.GetQuestStatus(qt,) // kua jak do tohohle?
			Title = qt.Title;
		}
	}
}