using WCell.Constants.Quests;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public abstract class QuestGiverRelation : IDataHolder
    {
        public QuestGiverRelationType RelationType;
        public uint QuestGiverId;
        public uint QuestId;

        public abstract ObjectTemplate ObjectTemplate
        {
            get;
        }

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
                var entry = ObjectTemplate;
                if (entry == null)
                {
                    ContentMgr.OnInvalidDBData(GetType().Name + " (QuestId: {0}) referred to invalid QuestGiverId: " +
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
                                template.Starters.Add(entry);
                                if (newQg)
                                {
                                    QuestMgr._questStarterCount++;
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

    [DataHolder]
    public class GOQuestGiverRelation : QuestGiverRelation
    {
        public override ObjectTemplate ObjectTemplate
        {
            get { return GOMgr.GetEntry(QuestGiverId); }
        }
    }

    [DataHolder]
    public class NPCQuestGiverRelation : QuestGiverRelation
    {
        public override ObjectTemplate ObjectTemplate
        {
            get { return NPCMgr.GetEntry(QuestGiverId); }
        }
    }
}