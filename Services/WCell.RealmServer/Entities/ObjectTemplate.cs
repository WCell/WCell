using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Quests;
using WCell.Util.Data;

namespace WCell.RealmServer.Entities
{
	public abstract class ObjectTemplate : IQuestHolderEntry
	{
		/// <summary>
		/// Entry Id
		/// </summary>
		public uint Id
		{
			get;
			set;
		}

		public float Scale
		{
			get;
			set;
		}

		public abstract ResolvedLootItemList GetLootEntries();

		#region Implementation of IQuestHolderEntry

		private QuestHolderInfo m_QuestHolderInfo;

		/// <summary>
		/// The QuestHolderEntry of this template, if this is a QuestGiver
		/// </summary>
		[NotPersistent]
		public QuestHolderInfo QuestHolderInfo
		{
			get { return m_QuestHolderInfo; }
			set
			{
				m_QuestHolderInfo = value;
			}
		}

		[NotPersistent]
		public GossipMenu DefaultGossip { get; set; }

		public abstract IWorldLocation[] GetInWorldTemplates();

		#endregion
	}
}
