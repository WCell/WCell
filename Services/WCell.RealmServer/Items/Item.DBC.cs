using WCell.Constants.Items;
using WCell.Constants.Skills;
using WCell.Core.ClientDB;
using WCell.RealmServer.Items.Enchanting;
using WCell.Util;

namespace WCell.RealmServer.Items
{
	public class ItemRandomPropertiesConverter : AdvancedClientDBRecordConverter<ItemRandomPropertyEntry>
	{
		public override ItemRandomPropertyEntry ConvertTo(byte[] rawData, ref int id)
		{
			var entry = new ItemRandomPropertyEntry();

			int currentIndex = 0;
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

	public class ItemRandomSuffixConverter : AdvancedClientDBRecordConverter<ItemRandomSuffixEntry>
	{
		public override ItemRandomSuffixEntry ConvertTo(byte[] rawData, ref int id)
		{
			var suffix = new ItemRandomSuffixEntry();
			var maxCount = 5u;

			int currentIndex = 0;
			suffix.Id = id = GetInt32(rawData, currentIndex++);//0

			currentIndex++; //string nameSuffix

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
						suffix.Values[i] = GetInt32(rawData, (int)(currentIndex + maxCount));
					}
				}
				currentIndex++;
			}

			ArrayUtil.Trunc(ref suffix.Enchants);
			ArrayUtil.TruncVals(ref suffix.Values);

			return suffix;
		}
	}

	public class ItemRandPropPointConverter : AdvancedClientDBRecordConverter<ItemLevelInfo>
	{
		public override ItemLevelInfo ConvertTo(byte[] rawData, ref int id)
		{
			var info = new ItemLevelInfo();

			int currentIndex = 0;
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

	public class ItemEnchantmentConverter : AdvancedClientDBRecordConverter<ItemEnchantmentEntry>
	{
		public override ItemEnchantmentEntry ConvertTo(byte[] rawData, ref int id)
		{
			var enchant = new ItemEnchantmentEntry();

            var currentIndex = 0;
            var effectsCount = 3;
            enchant.Id = (uint)(id = GetInt32(rawData, currentIndex++));
            enchant.Charges = GetUInt32(rawData, currentIndex++);
            enchant.Effects = new ItemEnchantmentEffect[effectsCount];

            for (var i = 0; i < effectsCount; i++)
			{
				var type = (ItemEnchantmentType)GetUInt32(rawData, 2 + i);
				if (type != ItemEnchantmentType.None)
				{
					var effect = new ItemEnchantmentEffect();
					enchant.Effects[i] = effect;
					effect.Type = type;
					effect.MinAmount = GetInt32(rawData, 5 + i);
					effect.MaxAmount = GetInt32(rawData, 8 + i);
					effect.Misc = GetUInt32(rawData, 11 + i);
				}
			}
			ArrayUtil.Prune(ref enchant.Effects);
            currentIndex = (4 * effectsCount) + 1; //we just read 4 fields of arrays, so increment our current position to reflect that

            enchant.Description = GetString(rawData, currentIndex++);

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

	public class ItemEnchantmentConditionConverter : AdvancedClientDBRecordConverter<ItemEnchantmentCondition>
	{
		public override ItemEnchantmentCondition ConvertTo(byte[] rawData, ref int id)
		{
			var condition = new ItemEnchantmentCondition();

			int currentIndex = 0;
			condition.Id = (uint)(id = GetInt32(rawData, currentIndex++));
			return condition;
		}
	}

    public class ScalingStatDistributionConverter : AdvancedClientDBRecordConverter<ScalingStatDistributionEntry>
    {
        public override ScalingStatDistributionEntry ConvertTo(byte[] rawData, ref int id)
        {
            var ssd = new ScalingStatDistributionEntry();

            int currentIndex = 0;
            ssd.Id = (uint)(id = GetInt32(rawData, currentIndex++));
            for(var i = 0; i < 10; i++)
            {
                ssd.StatMod[i] = GetInt32(rawData, currentIndex++);
            }
            for(var i = 0; i < 10; i++)
            {
                ssd.Modifier[i] = GetUInt32(rawData, currentIndex++);
            }
            ssd.MaxLevel = GetUInt32(rawData, currentIndex++);
            return ssd;
        }
    }

    public class ScalingStatValuesConverter : AdvancedClientDBRecordConverter<ScalingStatValues>
    {
        public override ScalingStatValues ConvertTo(byte[] rawData, ref int id)
        {
            var ssv = new ScalingStatValues();

            int currentIndex = 0;
            ssv.Id = (uint)(id = GetInt32(rawData, currentIndex++));
            ssv.Level = GetUInt32(rawData, currentIndex++);
            
            var ssdMultiplier = 0;
            for(; ssdMultiplier < 4; ssdMultiplier++)
            {
                ssv.SsdMultiplier[ssdMultiplier] = GetUInt32(rawData, currentIndex++);
            }
            
            var armorMod = 0;
            for(; armorMod < 5; armorMod++)
            {
                ssv.ArmorMod[armorMod] = GetUInt32(rawData, currentIndex++);
            }
            
            for(var i = 0; i < 6; i++)
            {
                ssv.DpsMod[i] = GetUInt32(rawData, currentIndex++);
            }
            
            ssv.SpellBonus = GetUInt32(rawData, currentIndex++);
            
            for(; ssdMultiplier < 6; ssdMultiplier++)
                ssv.SsdMultiplier[ssdMultiplier] = GetUInt32(rawData, currentIndex++);
            
            for(; armorMod < 8; armorMod++)
                ssv.ArmorMod[armorMod] = GetUInt32(rawData, currentIndex++);

            return ssv;
        }
    }
}