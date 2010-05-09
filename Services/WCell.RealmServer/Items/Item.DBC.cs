using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Core.DBC;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;

namespace WCell.RealmServer.Items
{
	public class ItemRandomPropertiesConverter : AdvancedDBCRecordConverter<ItemRandomPropertyEntry>
	{
		public override ItemRandomPropertyEntry ConvertTo(byte[] rawData, ref int id)
		{
			var entry = new ItemRandomPropertyEntry();

			uint currentIndex = 0;
			entry.Id = (uint)(id = GetInt32(rawData, currentIndex++));
			currentIndex++; // skip the name

			for (int i = 0; i < entry.Enchants.Length; i++)
			{
				var enchantId = GetUInt32(rawData, currentIndex++);
				entry.Enchants[i] = EnchantMgr.GetEnchantmentEntry(enchantId);
			}

			return entry;
		}
	}

	public class ItemRandomSuffixConverter : AdvancedDBCRecordConverter<ItemRandomSuffixEntry>
	{
		public override ItemRandomSuffixEntry ConvertTo(byte[] rawData, ref int id)
		{
			var suffix = new ItemRandomSuffixEntry();
			var maxCount = 5u;

			uint currentIndex = 0;
			suffix.Id = id = GetInt32(rawData, currentIndex++);//0

			currentIndex += 18;

			suffix.Enchants = new ItemEnchantmentEntry[maxCount];
			suffix.Values = new int[maxCount];
			for (var i = 0; i < maxCount; i++)
			{
				var enchantId = GetUInt32(rawData, currentIndex);
				if (enchantId != 0)
				{
					var enchant = EnchantMgr.GetEnchantmentEntry(enchantId);
					if (enchant != null)
					{
						suffix.Enchants[i] = enchant;
						suffix.Values[i] = GetInt32(rawData, currentIndex + maxCount);
					}
				}
				currentIndex++;
			}

			ArrayUtil.Trunc(ref suffix.Enchants);
			ArrayUtil.TruncVals(ref suffix.Values);

			return suffix;
		}
	}

	public class ItemRandPropPointConverter : AdvancedDBCRecordConverter<ItemLevelInfo>
	{
		public override ItemLevelInfo ConvertTo(byte[] rawData, ref int id)
		{
			var info = new ItemLevelInfo();

			uint currentIndex = 0;
			info.Level = (uint)(id = GetInt32(rawData, currentIndex++));

			for (var i = 0; i < ItemConstants.MaxRandPropPoints; i++)
			{
				info.EpicPoints[i] = GetUInt32(rawData, currentIndex++);
			}

			for (var i = 0; i < ItemConstants.MaxRandPropPoints; i++)
			{
				info.RarePoints[i] = GetUInt32(rawData, currentIndex++);
			}

			for (var i = 0; i < ItemConstants.MaxRandPropPoints; i++)
			{
				info.UncommonPoints[i] = GetUInt32(rawData, currentIndex++);
			}

			return info;
		}
	}

	public class ItemEnchantmentConverter : AdvancedDBCRecordConverter<ItemEnchantmentEntry>
	{
		public override ItemEnchantmentEntry ConvertTo(byte[] rawData, ref int id)
		{
			var enchant = new ItemEnchantmentEntry();

			enchant.Id = (uint)(id = GetInt32(rawData, 0));
		    enchant.Charges = GetUInt32(rawData, 1);			
            enchant.Description = GetString(rawData, 14);
			enchant.Effects = new ItemEnchantmentEffect[3];

			for (var i = 0; i < 3; i++)
			{
				var type = (ItemEnchantmentType)GetUInt32(rawData, (uint)(2 + i));
				if (type != ItemEnchantmentType.None)
				{
					var effect = new ItemEnchantmentEffect();
					enchant.Effects[i] = effect;
					effect.Type = type;
					effect.MinAmount = GetInt32(rawData, (uint)(5 + i));
					effect.MaxAmount = GetInt32(rawData, (uint)(8 + i));
					effect.Misc = GetUInt32(rawData, (uint)(11 + i));
				}
			}
			ArrayUtil.Prune(ref enchant.Effects);

			var currentIndex = 31u;

			enchant.Visual = GetUInt32(rawData, currentIndex++);
			enchant.Flags = GetUInt32(rawData, currentIndex++);
		    enchant.SourceItemId = GetUInt32(rawData, currentIndex++);

			var conditionId = GetUInt32(rawData, currentIndex++);
			if (conditionId > 0)
			{
				enchant.Condition = EnchantMgr.GetEnchantmentCondition(conditionId);
			}

			enchant.RequiredSkillId = (SkillId)GetUInt32(rawData, currentIndex++);			
            enchant.RequiredSkillAmount = GetInt32(rawData, currentIndex);			
            
            return enchant;
		}
	}

	public class ItemEnchantmentConditionConverter : AdvancedDBCRecordConverter<ItemEnchantmentCondition>
	{
		public override ItemEnchantmentCondition ConvertTo(byte[] rawData, ref int id)
		{
			var condition = new ItemEnchantmentCondition();

			uint currentIndex = 0;
			condition.Id = (uint)(id = GetInt32(rawData, currentIndex++));
			return condition;
		}
	}
}