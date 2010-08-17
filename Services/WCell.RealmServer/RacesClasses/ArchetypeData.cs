using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
	#region Spells
	[DataHolder]
	public class PlayerSpellEntry : IDataHolder
	{
		public RaceId Race;
		public ClassId Class;

		public SpellId SpellId;

		public override string ToString()
		{
			return SpellId.ToString();
		}

		public void FinalizeDataHolder()
		{
			var spell = SpellHandler.Get(SpellId);
			if (spell == null)
			{
				ContentHandler.OnInvalidDBData(GetType().Name + " for \"{0} {1}\" refers to invalid Spell: {2}.", Race, Class, this);
			}
			else
			{
				var archetypes = ArchetypeMgr.GetArchetypes(Race, Class);
				if (archetypes == null)
				{
					ContentHandler.OnInvalidDBData(GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.", this, Race, Class);
				}
				else
				{
					foreach (var archetype in archetypes)
					{
						archetype.Spells.Add(spell);
					}
				}
			}
		}
	}
	#endregion

	#region Skills
	///// <summary>
	///// 
	///// </summary>
	//[DataHolder]
	//public class PlayerSkillEntry : IDataHolder
	//{
	//    public RaceId Race;
	//    public ClassId Class;

	//    public SkillId SkillId;
	//    public uint Value;
	//    public uint MaxValue;

	//    [NotPersistent]
	//    public SkillLine Skill;

	//    public void FinalizeAfterLoad()
	//    {
	//        Skill = SkillHandler.Get(SkillId);
	//        if (Skill == null)
	//        {
	//            ContentHandler.OnInvalidDBData("SkillLine for " + GetType().Name + " \"{0}\" does not exist.", SkillId);
	//        }
	//        else
	//        {
	//            var archetypes = RaceClassMgr.GetArchetypes(Race, Class);
	//            if (archetypes == null)
	//            {
	//                ContentHandler.OnInvalidDBData(GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.", this, this, this);
	//            }
	//            else
	//            {
	//                foreach (var archetype in archetypes)
	//                {
	//                    archetype.Skills.Add(this);
	//                }
	//            }
	//        }
	//    }

	//    public override string ToString()
	//    {
	//        return string.Format("{0} ({1} / {2})", Skill, Value, MaxValue);
	//    }
	//}
	#endregion

	#region Actionbar
	/// <summary>
	/// 
	/// </summary>
	[DataHolder]
	public class PlayerActionButtonEntry : IDataHolder
	{
		public RaceId Race;
		public ClassId Class;

		public uint Index;
		public ushort Action;
		public byte Type;
		public byte Info;

		public void FinalizeDataHolder()
		{
			var archetypes = ArchetypeMgr.GetArchetypes(Race, Class);
			if (archetypes == null)
			{
				ContentHandler.OnInvalidDBData(GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.", this, Race, Class);
			}
			else
			{
				foreach (var archetype in archetypes)
				{
					ActionButton.Set(archetype.ActionButtons, Index, Action, Type, Info);
				}
			}
		}

		public override string ToString()
		{
			return string.Format("Action {0} (Index: {1})", Action, Index);
		}
	}
	#endregion

    #region LevelStatInfo
    [DataHolder]
    public class LevelStatInfo : IDataHolder
    {
        public RaceId Race;
        public ClassId Class;
        public int Level;

		[NotPersistent]
    	public int[] Stats = new int[(int)StatType.End];

    	public int Strength
		{
			get { return Stats[(int)StatType.Strength]; }
			set { Stats[(int) StatType.Strength] = value; }
    	}

    	public int Agility
    	{
			get { return Stats[(int)StatType.Agility]; }
			set { Stats[(int)StatType.Agility] = value; }
    	}

		public int Stamina
		{
			get { return Stats[(int)StatType.Stamina]; }
			set { Stats[(int)StatType.Stamina] = value; }
		}

		public int Intellect
		{
			get { return Stats[(int)StatType.Intellect]; }
			set { Stats[(int)StatType.Intellect] = value; }
		}

		public int Spirit
		{
			get { return Stats[(int)StatType.Spirit]; }
			set { Stats[(int)StatType.Spirit] = value; }
		}

        public void FinalizeDataHolder() 
        {
            var level = Level > 0 ? Level : 1;
			if (level > RealmServerConfiguration.MaxCharacterLevel)
			{
				return;
			}

        	var archetype = ArchetypeMgr.GetArchetype(Race, Class);
			if (Level == 1)
			{
				archetype.FirstLevelStats = this;
			}
            archetype.LevelStats[level - 1] = this;
        }
    }
	#endregion

	#region Items
	//[DataHolder]
	//public class PlayerItemEntry : IDataHolder
	//{
	//    public RaceId Race;
	//    public ClassId Class;

	//    public ItemId ItemId;
	//    public uint Amount;
	//    public InventorySlot Slot;

	//    public override string ToString()
	//    {
	//        return ItemId.ToString();
	//    }

	//    public void FinalizeAfterLoad()
	//    {
	//        var template = ItemMgr.GetTemplate(ItemId);
	//        if (template == null)
	//        {
	//            ContentHandler.OnInvalidDBData(GetType().Name + " for \"{0} {1}\" refers to invalid Item: {2}.", Race, Class, this);
	//        }
	//        else
	//        {
	//            var info = new SlotItemInfo {
	//                Amount = Amount,
	//                Slot = Slot,
	//                Template = template
	//            };

	//            var archetypes = ArchetypeMgr.GetArchetypes(Race, Class);
	//            if (archetypes == null)
	//            {
	//                ContentHandler.OnInvalidDBData(GetType().Name + " \"{0}\" refers to invalid Archetype: {1} {2}.", this, Race, Class);
	//            }
	//            else
	//            {
	//                foreach (var archetype in archetypes)
	//                {
	//                    if (Slot == InventorySlot.Bag1)
	//                    {
	//                        ToString();
	//                    }

	//                    var slotMatch = archetype.Items.Find(item => item.Slot == Slot);
	//                    if (slotMatch.Slot == Slot && slotMatch.Template != null)
	//                    {
	//                        ContentHandler.OnInvalidDBData("Archetype {0} has two or more inintial Items in Slot #{1} ({2}): {3} and {4}",
	//                        archetype, (int)Slot, Slot, template, slotMatch.Template);
	//                    }
	//                    else
	//                    {
	//                        archetype.MaleItems.Add(info);
	//                    }
	//                }
	//            }
	//        }
	//    }
	//}
	#endregion
}