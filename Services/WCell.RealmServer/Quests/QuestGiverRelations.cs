using WCell.Constants.Quests;
using WCell.RealmServer.Content;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
	#region GOs

	[DataHolder]
	public class GOQuestGiverRelation : IDataHolder
	{
		public QuestGiverRelationType RelationType;
		public uint QuestGiverId;
		public uint QuestId;

		public void FinalizeDataHolder()
		{
			var template = QuestMgr.GetTemplate(QuestId);

			if (template == null)
			{
				ContentMgr.OnInvalidDBData(GetType().Name + " (QuestGiverId: {0}) referred to invalid QuestId: " +
					QuestId, QuestGiverId);
			}
			else
			{
				var entry = GOMgr.GetEntry(QuestGiverId);
				if (entry == null)
				{
					ContentMgr.OnInvalidDBData(GetType().Name + " (QuestId: {0}) referred to invalid QuestGiverId: " +
						QuestGiverId, QuestId);
				}
				else
				{
					var qgEntry = entry.QuestHolderInfo;
					if (qgEntry == null)
					{
						entry.QuestHolderInfo = qgEntry = new QuestHolderInfo();
					}

					switch (RelationType)
					{
						case QuestGiverRelationType.Starter:
							{
								qgEntry.QuestStarts.Add(template);
								template.Starters.Add(entry);
								break;
							}
						case QuestGiverRelationType.Finisher:
							{
								qgEntry.QuestEnds.Add(template);
								template.Finishers.Add(entry);
								break;
							}
						default:
							ContentMgr.OnInvalidDBData(GetType().Name + " (Quest: {0}, QuestGiver: {1}) had invalid QuestGiverRelationType: " +
														 RelationType, QuestId, QuestGiverId);
							break;
					}
				}
			}
		}
	}
	#endregion
	
	#region NPCs
	[DataHolder]
	public class NPCQuestGiverRelation : IDataHolder
	{
		public QuestGiverRelationType RelationType;
		public uint QuestId;
		public uint QuestGiverId;

		public void FinalizeDataHolder()
		{
			var template = QuestMgr.GetTemplate(QuestId);

			if (template == null)
			{
				ContentMgr.OnInvalidDBData(GetType().Name + " (QuestGiverId: {0}) refers to invalid QuesIdt: " +
					QuestId, QuestGiverId);
			}
			else
			{
				var entry = NPCMgr.GetEntry(QuestGiverId);
				if (entry == null)
				{
					ContentMgr.OnInvalidDBData(GetType().Name + " (QuestId: {0}) refers to invalid QuestGiverId: " +
						QuestGiverId, QuestId);
				}
				else
				{
					var qgEntry = entry.QuestHolderInfo;
				    var newQg = qgEntry == null;
                    if (newQg)
					{
						entry.QuestHolderInfo = qgEntry = new QuestHolderInfo();
					}

					switch (RelationType)
					{
						case QuestGiverRelationType.Starter:
							{
								qgEntry.QuestStarts.Add(template);
                                QuestMgr._questStarterCount++;
                                if (newQg)
                                {
                                    template.Starters.Add(entry);
                                }
							    break;
							}
						case QuestGiverRelationType.Finisher:
							{
								qgEntry.QuestEnds.Add(template);
                                template.Finishers.Add(entry);
                                if (newQg)
                                {
                                    QuestMgr._questFinisherCount++;
                                }
							    break;
							}
						default:
							ContentMgr.OnInvalidDBData(GetType().Name + " (Quest: {0}, QuestGiver: {1}) had invalid QuestGiverRelationType: " +
														 RelationType, QuestId, QuestGiverId);
							break;
					}
				}
			}
		}
	}
	#endregion
}