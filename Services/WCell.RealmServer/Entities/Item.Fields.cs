using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Entities
{
	public partial class Item
	{
		public Character OwningCharacter
		{
			get { return m_owningCharacter; }
			internal set
			{
				m_owningCharacter = value;
				if (m_owningCharacter != null)
				{
					m_isInWorld = m_unknown = true;
					SetEntityId(ItemFields.OWNER, value.EntityId);
					m_record.OwnerId = (int)value.EntityId.Low;
				}
				else
				{
					SetEntityId(ItemFields.OWNER, EntityId.Zero);
					m_record.OwnerId = 0;
				}
			}
		}

		/// <summary>
		/// The Inventory of the Container that contains this Item
		/// </summary>
		public BaseInventory Container
		{
			get { return m_container; }
			internal set
			{
				if (m_container != value)
				{
					if (value != null)
					{
						var cont = value.Container;
						SetEntityId(ItemFields.CONTAINED, cont.EntityId);
						m_record.ContainerSlot = cont.BaseInventory.Slot;
					}
					else
					{
						SetEntityId(ItemFields.CONTAINED, EntityId.Zero);
						m_record.ContainerSlot = 0;
					}
					m_container = value;
				}
			}
		}

		/// <summary>
		/// The life-time of this Item in seconds
		/// </summary>
		//public uint ExistingDuration
		//{
		//    get
		//    {
		//        return m_record.ExistingDuration;
		//    }
		//    set
		//    {
		//        m_record.ExistingDuration = value;
		//    }
		//}

		public EntityId Creator
		{
			get { return new EntityId((ulong)m_record.CreatorEntityId); }
			set
			{
				SetEntityId(ItemFields.CREATOR, value);
				m_record.CreatorEntityId = (long)value.Full;
			}
		}

		public EntityId GiftCreator
		{
			get { return new EntityId((ulong)m_record.GiftCreatorEntityId); }
			set
			{
				SetEntityId(ItemFields.GIFTCREATOR, value);
				m_record.GiftCreatorEntityId = (long)value.Full;
			}
		}

		/// <summary>
		/// The Slot of this Item within its <see cref="Container">Container</see>.
		/// </summary>
		public int Slot
		{
			get
			{
				return m_record.Slot;
			}
			internal set
			{
				m_record.Slot = value;

			}
		}

		/// <summary>
		/// Ensures, new value won't exceed UniqueCount.
		/// Returns how many items actually got added. 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int ModAmount(int value)
		{
			if (value != 0)
			{
				if (m_owningCharacter != null)
				{
					int uniqueCount;
					if (value > 0 && m_template.UniqueCount > 0)
					{
						uniqueCount = m_owningCharacter.Inventory.GetUniqueCount(m_template.ItemId);
					}
					else
					{
						uniqueCount = int.MaxValue;
					}

					if (value > uniqueCount)
					{
						value = uniqueCount;
					}

					m_owningCharacter.Inventory.OnAmountChanged(this, value);
				}

				m_record.Amount += value;
				return value;
			}
			return 0;
		}

		/// <summary>
		/// Current amount of items in this stack.
		/// Setting the Amount to 0 will destroy the Item.
		/// Keep in mind that this is uint and thus can never become smaller than 0!
		/// </summary>
		public int Amount
		{
			get { return m_record.Amount; }
			set
			{
				if (value <= 0)
				{
					Destroy();
				}
				else
				{
					var diff = value - m_record.Amount;
					if (diff != 0)
					{
						if (m_owningCharacter != null)
						{
							m_owningCharacter.Inventory.OnAmountChanged(this, diff);
						}

						SetInt32(ItemFields.STACK_COUNT, value);
						m_record.Amount = value;
					}
				}
			}
		}

		public uint Duration
		{
			get { return (uint)m_record.Duration; }
			set
			{
				SetUInt32(ItemFields.DURATION, value);
				m_record.Duration = (int)value;
			}
		}

		/// <summary>
		/// Charges of the <c>UseSpell</c> of this Item.
		/// </summary>
		public uint SpellCharges
		{
			get
			{
				return (uint)m_record.Charges;
			}
			set
			{
				if (value <= 0)
				{
					Amount--;
					return;
				}
				m_record.Charges = (short)value;
				if (m_template.UseSpell != null)
				{
					SetSpellCharges(m_template.UseSpell.Index, value);
				}
			}
		}

		public uint GetSpellCharges(uint index)
		{
			return GetUInt32(ItemFields.SPELL_CHARGES + (int)index);
		}

		public void ModSpellCharges(uint index, int delta)
		{
			SetUInt32((int)ItemFields.SPELL_CHARGES + (int)index, (uint)(GetSpellCharges(index) + delta));
		}

		public void SetSpellCharges(uint index, uint value)
		{
			SetUInt32((int)ItemFields.SPELL_CHARGES + (int)index, value);
		}

		public ItemFlags Flags
		{
			get { return m_record.Flags; }
			set
			{
				SetUInt32(ItemFields.FLAGS, (uint)value);
				m_record.Flags = value;
			}
		}

		public bool IsAuctioned
		{
			get { return m_record.IsAuctioned; }
			set { m_record.IsAuctioned = true; }
		}

		#region ItemFlag Helpers

		public bool IsSoulbound
		{
			get { return Flags.HasFlag(ItemFlags.Soulbound); }
		}

		public bool IsGiftWrapped
		{
			get { return Flags.HasFlag(ItemFlags.GiftWrapped); }
		}

		public bool IsConjured
		{
			get { return Flags.HasFlag(ItemFlags.Conjured); }
		}

		#endregion

		public uint PropertySeed
		{
			get { return GetUInt32(ItemFields.PROPERTY_SEED); }
			set
			{
				SetUInt32(ItemFields.PROPERTY_SEED, value);
				m_record.RandomSuffix = (int)value;
			}
		}

		public uint RandomPropertiesId
		{
			get { return (uint)m_record.RandomProperty; }
			set
			{
				SetUInt32(ItemFields.RANDOM_PROPERTIES_ID, value);
				m_record.RandomProperty = (int)value;
			}
		}

		public int Durability
		{
			get { return m_record.Durability; }
			set
			{
				SetInt32(ItemFields.DURABILITY, value);
				m_record.Durability = value;
			}
		}

		public int MaxDurability
		{
			get
			{
				return GetInt32(ItemFields.MAXDURABILITY);
				//return m_Template.MaxDurability;
			}
			protected set
			{
				SetInt32(ItemFields.MAXDURABILITY, value);
			}
		}

		public void RepairDurability()
		{
			Durability = MaxDurability;
		}

		public uint TextId
		{
			get { return m_record.ItemTextId; }
			internal set
			{
			//TODO: Items don't have the Text ID field anymore
				//SetUInt32(ItemFields.ITEM_TEXT_ID, value);
				m_record.ItemTextId = value;
			}
		}

		public string ItemText
		{
			get { return m_record.ItemText; }
			internal set { m_record.ItemText = value; }
		}

		#region IWeapon
		public DamageInfo[] Damages
		{
			get
			{
				return m_template.Damages;
			}
		}

		public SkillId Skill
		{
			get
			{
				return m_template.ItemProfession;
			}
		}

		public bool IsRanged
		{
			get
			{
				return (m_template.RangeModifier > 0.0f);
			}
		}

		public bool IsMelee
		{
			get
			{
				return (m_template.RangeModifier == 0.0f);
			}
		}

		/// <summary>
		/// The minimum Range of this weapon
		/// TODO: temporary values
		/// </summary>
		public float MinRange
		{
			get
			{
				if (IsMelee)
				{
					return 0.0f;
				}
				return Unit.DefaultMeleeDistance;
			}
		}

		/// <summary>
		/// The maximum Range of this Weapon
		/// TODO: temporary values
		/// </summary>
		public float MaxRange
		{
			get
			{
				if (IsMelee)
				{
					return Unit.DefaultMeleeDistance;
				}
				return Unit.DefaultRangedDistance;
			}
		}

		/// <summary>
		/// The time in milliseconds between 2 attacks
		/// </summary>
		public int AttackTime
		{
			get
			{
				return m_template.AttackTime;
			}
		}
		#endregion

		public ItemRecord Record
		{
			get
			{
				return m_record;
			}
		}

		public override ObjectTypeCustom CustomType
		{
			get
			{
				return ObjectTypeCustom.Object | ObjectTypeCustom.Item;
			}
		}
	}
}