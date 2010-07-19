using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Quests;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Global;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AreaTriggers
{
	/// <summary>
	/// All information associated with AreaTriggers
	/// </summary>
	[DataHolder]
	public class ATTemplate : IDataHolder
	{
		public uint Id;

		public string Name;

		public MapId TargetMap;

		public Vector3 TargetPos;

		public AreaTriggerType Type;

		public float TargetOrientation;

		public uint TargetScreen;

		/// <summary>
		/// Item, required for triggering
		/// </summary>
		public ItemId RequiredItemId;

		/// <summary>
		/// Item, required for triggering
		/// </summary>
		public ItemId RequiredItem2Id;

		/// <summary>
		/// Required heoric key
		/// </summary>
		public ItemId RequiredHeroicKeyId;

		/// <summary>
		/// Required heoric key
		/// </summary>
		public ItemId RequiredHeroicKey2Id;

		/// <summary>
		/// Required quest to be finished
		/// </summary>
		public uint RequiredQuestId;

		/// <summary>
		/// Quest to be triggered
		/// </summary>
		public uint TriggerQuestId;

		/// <summary>
		/// Unused
		/// </summary>
		public string RequiredFailedText;

		public uint RequiredLevel;

		[NotPersistent]
		public AreaTriggerHandler Handler;

		#region Loading

		public uint GetId()
		{
			return Id;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			var trigger = AreaTriggerMgr.AreaTriggers.Get(Id);
			if (trigger == null)
			{
				ContentHandler.OnInvalidDBData("AreaTriggerEntry {0} (#{1}, Type: {2}) had invalid AreaTrigger-id.", Name, Id, Type);
				return;
			}
			else
			{
				trigger.Template = this;
			}

			if (TargetPos.IsSet)
			{
				var region = World.GetRegionInfo(TargetMap);
				if (region != null)
				{
					Type = AreaTriggerType.Teleport;
					ArrayUtil.AddOnlyOne(ref region.EntrancePositions, TargetPos);
				}
			}

			Handler = AreaTriggerMgr.GetHandler(Type);
		}
		#endregion

		public override string ToString()
		{
			return Name + string.Format(" (in {0}, Lvl {1})", TargetMap, RequiredLevel);
		}

		public void Write(IndentTextWriter writer)
		{
			writer.WriteLine("Type: " + Type);
			writer.WriteLineNotDefault(RequiredItemId, "RequiredItemId: " + RequiredItemId);
			writer.WriteLineNotDefault(RequiredItem2Id, "RequiredItem2Id: " + RequiredItem2Id);
			writer.WriteLineNotDefault(RequiredHeroicKeyId, "RequiredHeroicKeyId: " + RequiredHeroicKeyId);
			writer.WriteLineNotDefault(RequiredHeroicKey2Id, "RequiredHeroicKey2Id: " + RequiredHeroicKey2Id);
			writer.WriteLineNotDefault(RequiredQuestId, "RequiredQuestId: " + RequiredQuestId);
			writer.WriteLineNotDefault(RequiredLevel, "RequiredLevel: " + RequiredLevel);
			writer.WriteLineNotDefault(RequiredHeroicKeyId, "RequiredHeroicKeyId: " + RequiredHeroicKeyId);
			writer.WriteLineNotDefault(RequiredHeroicKeyId, "RequiredHeroicKeyId: " + RequiredHeroicKeyId);
		}
	}
}