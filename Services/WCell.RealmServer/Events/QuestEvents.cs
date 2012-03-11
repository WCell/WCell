using System;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Quests
{
    public partial class QuestTemplate
    {
        public event Action<Quest> QuestStarted;
        public event Action<Quest> QuestFinished;
        public event QuestCancelHandler QuestCancelled;
        public event QuestNPCHandler NPCInteracted;
        public event QuestGOHandler GOInteraction;

        public delegate void QuestNPCHandler(Quest quest, NPC npc);
        public delegate void QuestGOHandler(Quest quest, GameObject go);
        public delegate void QuestCancelHandler(Quest quest, bool failed);

        /// <summary>
        /// Single handler to verify whether a Quest has been completed
        /// </summary>
        [NotPersistent]
        public Func<Quest, bool> CompleteHandler;
    }
}