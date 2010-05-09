using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Items;
using WCell.Core;
using WCell.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Items;

namespace WCell.RealmServer.Tests.Tools
{
	public class ItemPool
	{
		ItemTemplate m_defaultItemTemplate;
		ItemTemplate[] m_defaultItemTemplates = new ItemTemplate[(int)EquipmentSlot.End];

		public ItemTemplate DefaultItemTemplate
		{
			get
			{
				if (m_defaultItemTemplate == null)
				{
					Setup.EnsureItemsLoaded();
					foreach (var template in ItemMgr.Templates)
					{
						if (template != null)
						{
							m_defaultItemTemplate = template;
							break;
						}
					}
				}
				return m_defaultItemTemplate;
			}
		}

		/// <summary>
		/// Returns a default item that can be equipped to the given slot
		/// </summary>
		public ItemTemplate GetDefault(EquipmentSlot slot)
		{
			var template = m_defaultItemTemplates[(int)slot];
			if (template == null)
			{
				Setup.EnsureItemsLoaded();
				foreach (var temp in ItemMgr.Templates)
				{
					if (temp != null && temp.EquipmentSlots.Contains(slot))
					{
						m_defaultItemTemplates[(int)slot] = template = temp;
					}
				}
			}

			Assert.IsNotNull(template);
			return template;
		}
	}
}