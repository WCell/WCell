using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.DBC;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.RealmServer.Items;
using NLog;

namespace WCell.RealmServer.RacesClasses
{
	#region ChrRaces.dbc
	public class DBCRaceConverter : AdvancedDBCRecordConverter<BaseRace>
	{
		public override BaseRace ConvertTo(byte[] rawData, ref int id)
		{
			var race = new BaseRace
			{
				Id = ((RaceId)(id = (int)GetUInt32(rawData, 0))),
				FactionTemplateId = (FactionTemplateId)GetUInt32(rawData, 8),
				MaleDisplayId = GetUInt32(rawData, 4),
				FemaleDisplayId = GetUInt32(rawData, 5),
				Scale = GetFloat(rawData, 7),
				Name = GetString(rawData, 14),
				ClientId = (ClientId)GetUInt32(rawData, 68)
			};
			return race;
		}
	}
	#endregion

	#region ChrStartOutfit.dbc
	public class DBCStartOutfitConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var raceClassGender = GetUInt32(rawData, 1);
			var race = (RaceId)(raceClassGender & 0xFF);
		    var clss = (ClassId) ((raceClassGender & 0xFF00) >> 8);
		    var gender = (GenderType) ((raceClassGender & 0xFF0000) >> 16);

			var archetype = ArchetypeMgr.GetArchetype(race, clss);
			if (archetype == null)
			{
				return;
			}

			var items = archetype.GetInitialItems(gender);

			for (var i = 2; i <= 25; i++)
			{
				var itemId = GetInt32(rawData, i);
				if (itemId > 0)
				{
					var templ = ItemMgr.GetTemplate((ItemId)itemId);
					if (templ == null)
					{
						LogManager.GetCurrentClassLogger().Warn("Missing initial Item in DB: " + itemId + " (" + (uint)itemId + ")");
					}
					else
					{
						if (templ.IsStackable)
						{
							var index = items.FindIndex(stack => stack.Template.Id == itemId);
							if (index > -1)
							{
								items[index] = new ItemStack 
								{
									Template = templ,
									Amount = items[index].Amount + 1
								};
							}
							else
							{
								items.Add(new ItemStack
								{
									Template = templ,
									Amount = templ.IsAmmo ? templ.MaxAmount : 1
								});
							}
						}
						else
						{
							items.Add(new ItemStack
							{
								Template = templ,
								Amount = 1
							});
						}
					}
				}
			}
		}
	}
	#endregion
}