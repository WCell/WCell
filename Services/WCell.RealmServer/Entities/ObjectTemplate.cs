using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Factions;
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

		/// <summary>
		/// The QuestHolderEntry of this template, if this is a QuestGiver
		/// </summary>
		[NotPersistent]
		public QuestHolderInfo QuestHolderInfo
		{
			get;
			set;
		}

		public abstract IWorldLocation[] GetInWorldTemplates();

		#endregion
	}
}
