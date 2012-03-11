using System.Collections.Generic;
using WCell.Constants.Quests;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Lang;
using WCell.Util;

namespace WCell.RealmServer.Quests
{
    public interface IQuestHolderInfo
    {
        QuestStatus GetHighestQuestGiverStatus(Character chr);

        List<QuestTemplate> GetAvailableQuests(Character chr);

        List<QuestMenuItem> GetQuestMenuItems(Character chr);
    }

    /// <summary>
    /// TODO: Add methods related to query QuestGiver-specific information etc
    ///
    /// Represents all information that a QuestGiver has
    /// </summary>
    public class QuestHolderInfo : IQuestHolderInfo
    {
        /// <summary>
        /// Set of Quests that may start at this QuestHolder
        /// </summary>
        public List<QuestTemplate> QuestStarts;

        /// <summary>
        /// Set of Quests that may be turned in at this QuestHolder
        /// </summary>
        public List<QuestTemplate> QuestEnds;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestHolderInfo"/> class.
        /// </summary>
        public QuestHolderInfo()
        {
            QuestStarts = new List<QuestTemplate>(1);
            QuestEnds = new List<QuestTemplate>(1);
        }

        /// <summary>
        /// Gets the Highest Status of any quest newly available or continuable for the given Character
        /// </summary>
        /// <param name="chr">The character which is status calculated with.</param>
        /// <returns></returns>
        public QuestStatus GetHighestQuestGiverStatus(Character chr)
        {
            var highestStatus = QuestStatus.NotAvailable;
            if (QuestStarts != null)
            {
                foreach (var qt in QuestStarts)
                {
                    var qs = qt.GetStartStatus(this, chr);
                    if (qs > highestStatus)
                    {
                        highestStatus = qs;
                    }
                }
            }

            if (QuestEnds != null)
            {
                foreach (var qt in QuestEnds)
                {
                    var qs = qt.GetEndStatus(chr);
                    if (qs > highestStatus)
                    {
                        highestStatus = qs;
                    }
                }
            }
            return highestStatus;
        }

        /// <summary>
        /// Gets list of quests, which are activatable by this Character (not low leveled nor unavailable).
        /// </summary>
        /// <param name="chr">The client.</param>
        /// <returns>List of the active quests.</returns>
        public List<QuestTemplate> GetAvailableQuests(Character chr)
        {
            var activeQuestList = new List<QuestTemplate>();
            foreach (var qt in QuestEnds)
            {
                var quest = chr.QuestLog.GetQuestById(qt.Id);
                if (quest != null)
                {
                    activeQuestList.Add(qt);
                }
            }

            foreach (var qt in QuestStarts)
            {
                var qs = qt.GetStartStatus(this, chr);
                if (qs.IsAvailable())
                {
                    activeQuestList.Add(qt);
                }
            }

            return activeQuestList;
        }

        /// <summary>
        /// Gets the QuestMenuItems for a <see href="GossiGossipMenu">GossipMenu</see>
        /// </summary>
        /// <param name="chr">The client.</param>
        /// <returns></returns>
        public List<QuestMenuItem> GetQuestMenuItems(Character chr)
        {
            var items = new List<QuestMenuItem>();

            foreach (var qt in QuestEnds)
            {
                var quest = chr.QuestLog.GetQuestById(qt.Id);
                if (quest != null)
                {
                    items.Add(new QuestMenuItem(qt.Id, 4, qt.Level, qt.Titles.Localize(chr.Locale)));
                }
            }

            foreach (var qt in QuestStarts)
            {
                var qs = qt.GetStartStatus(this, chr);
                if (qs.IsAvailable())
                {
                    items.Add(new QuestMenuItem(qt.Id, qs == QuestStatus.Available ? 2u : 4u, qt.Level, qt.Titles.Localize(chr.Locale)));
                }
            }
            return items;
        }

        public override string ToString()
        {
            return string.Format("QuestHolder - Starts: {0}; Ends: {1}",
                Utility.GetStringRepresentation(QuestStarts),
                Utility.GetStringRepresentation(QuestEnds));
        }
    }
}