using WCell.Constants.Updates;
using WCell.RealmServer.Items;
using WCell.RealmServer.UpdateFields;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// An equippable container item, such as a bag etc
	/// </summary>
	public partial class Container : Item, IContainer
	{
		public new static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Container);

		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		public override UpdateFieldHandler.DynamicUpdateFieldHandler[] DynamicUpdateFieldHandlers
		{
			get { return UpdateFieldHandler.DynamicContainerFieldHandlers; }
		}

		ContainerInventory m_inventory;

		protected internal Container()
		{
		}


		protected override void OnInit()
		{
			Type |= ObjectTypes.Container;
			ContainerSlots = m_template.ContainerSlots;
			m_inventory = new ContainerInventory(this, ContainerFields.SLOT_1, m_template.ContainerSlots);
		}

		protected override void OnLoad()
		{
			Type |= ObjectTypes.Container;
			SetInt32(ContainerFields.NUM_SLOTS, m_record.ContSlots);
			m_inventory = new ContainerInventory(this, ContainerFields.SLOT_1, m_record.ContSlots);
		}

		public override ObjectTypeId ObjectTypeId
		{
			get { return ObjectTypeId.Container; }
		}

		/// <summary>
		/// NUM_SLOTS
		/// </summary>
		public int ContainerSlots
		{
			get { return GetInt32(ContainerFields.NUM_SLOTS); }
			set
			{
				SetInt32(ContainerFields.NUM_SLOTS, value);
				m_record.ContSlots = value;
				// TODO: Trunc inventory
			}
		}

		public BaseInventory BaseInventory
		{
			get { return m_inventory; }
		}

		public override ObjectTypeCustom CustomType
		{
			get { return ObjectTypeCustom.Object | ObjectTypeCustom.Item | ObjectTypeCustom.Container; }
		}

		//protected internal override void WriteObjectCreationUpdate(Character chr)
		//{
		//    base.WriteObjectCreationUpdate(chr);
		//    foreach (var item in m_inventory.Items)
		//    {
		//        if (item != null)
		//        {
		//            item.WriteObjectCreationUpdate(chr);
		//        }
		//    }
		//}

		protected internal override void DoDestroy()
		{
			foreach (var item in m_inventory)
			{
				item.Destroy();
			}
			base.DoDestroy();
		}
	}
}
