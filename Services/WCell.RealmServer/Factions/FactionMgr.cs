/*************************************************************************
 *
 *   file		: FactionHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-11 00:18:43 -0800 (Mon, 11 Feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 125 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Factions
{
	public static class FactionMgr
	{
		const uint MaxTemplateId = 4000;

		private static bool initialized;

		#region Init
		[Initialization(InitializationPass.Second, "Initialize Factions")]
		public static void Initialize()
		{
			if (!initialized)
			{
				initialized = true;
				InitFactionDBC();
				InitFactionTemplateDBC();
			}
		}

		public static readonly List<Faction> AlliancePlayerFactions = new List<Faction>(5);
		public static readonly List<Faction> HordePlayerFactions = new List<Faction>(5);

		public static readonly Faction[] ByRace = new Faction[(uint)Utility.GetMaxEnum<RaceId>() + 1];

		[NotVariable]
		public static Faction[] ById = new Faction[(uint)Utility.GetMaxEnum<FactionId>() + 1];

		[NotVariable]
		public static Faction[] ByReputationIndex = new Faction[(uint)Utility.GetMaxEnum<FactionReputationIndex>() + 1];

		[NotVariable]
		public static Faction[] ByTemplateId = new Faction[MaxTemplateId];

		public static readonly Dictionary<FactionId, FactionEntry> FactionEntries =
			new Dictionary<FactionId, FactionEntry>();

		public static readonly Dictionary<uint, FactionTemplateEntry> FactionTplEntries =
			new Dictionary<uint, FactionTemplateEntry>();

		private static void InitFactionDBC()
		{
			var dbcRdr =
				new MappedDBCReader<FactionEntry, FactionConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_FACTIONS));

			foreach (var entry in dbcRdr.Entries.Values)
			{
				FactionEntries[entry.Id] = entry;
			}
		}

		private static void InitFactionTemplateDBC()
		{
			var dbcRdr =
				new MappedDBCReader<FactionTemplateEntry, FactionTemplateConverter>(
					RealmServerConfiguration.GetDBCFile(WCellDef.DBC_FACTION_TEMPLATES));

			foreach (var templ in dbcRdr.Entries.Values)
			{
				FactionTplEntries[templ.Id] = templ;

				if (templ.FactionId == 0)
				{
					// some templates do not have an actual faction
					continue;
				}

				var entry = FactionEntries[templ.FactionId];
				var faction = new Faction(entry, templ);
				ArrayUtil.Set(ref ByTemplateId, templ.Id, faction);

				// there are several templates for one faction
				if (Get(templ.FactionId) != null)
				{
					continue;
				}

				ArrayUtil.Set(ref ById, (uint)entry.Id, faction);

				if (entry.FactionIndex > 0)
				{
					ArrayUtil.Set(ref ByReputationIndex, (uint)entry.FactionIndex, faction);
				}
			}

			// add Factions for Races and set their faction-group
			Faction fac;
			ByRace[(uint)RaceId.Human] = fac = ById[(uint)FactionId.PLAYERHuman];
			fac.SetAlliancePlayer();

			ByRace[(uint)RaceId.Dwarf] = fac = ById[(uint)FactionId.PLAYERDwarf];
			fac.SetAlliancePlayer();

			ByRace[(uint)RaceId.NightElf] = fac = ById[(uint)FactionId.PLAYERNightElf];
			fac.SetAlliancePlayer();

			ByRace[(uint)RaceId.Gnome] = fac = ById[(uint)FactionId.PLAYERGnome];
			fac.SetAlliancePlayer();

			ByRace[(uint)RaceId.Draenei] = fac = ById[(uint)FactionId.PLAYERDraenei];
			fac.SetAlliancePlayer();


			ByRace[(uint)RaceId.Orc] = fac = ById[(uint)FactionId.PLAYEROrc];
			fac.SetHordePlayer();

			ByRace[(uint)RaceId.Undead] = fac = ById[(uint)FactionId.PLAYERUndead];
			fac.SetHordePlayer();

			ByRace[(uint)RaceId.Tauren] = fac = ById[(uint)FactionId.PLAYERTauren];
			fac.SetHordePlayer();

			ByRace[(uint)RaceId.Troll] = fac = ById[(uint)FactionId.PLAYERTroll];
			fac.SetHordePlayer();

			ByRace[(uint)RaceId.BloodElf] = fac = ById[(uint)FactionId.PLAYERBloodElf];
			fac.SetHordePlayer();

			foreach (var faction in ById)
			{
				if (faction != null)
				{
					faction.Init();
					if (faction.Entry.ParentId != FactionId.None)
					{
						var parent = Get(faction.Entry.ParentId);
						// some factions are pointing to invalid other factions (so we have to use TryGetValue)
						if (parent != null)
						{
							parent.Children.Add(faction);
						}
					}
				}
			}
		}
		#endregion

		#region IDs and entries

		public static Faction Get(FactionReputationIndex repuataionIndex)
		{
			if ((uint)repuataionIndex >= ByReputationIndex.Length ||

				repuataionIndex < 0)
			{
				return null;
			}
			return ByReputationIndex[(uint)repuataionIndex];
		}

		public static Faction Get(FactionId id)
		{
			if ((uint)id >= ById.Length)
			{
				return null;
			}
			return ById[(uint)id];
		}

		public static Faction Get(FactionTemplateId id)
		{
			if ((uint)id >= ByTemplateId.Length)
			{
				return null;
			}
			return ByTemplateId[(uint)id];
		}

		public static Faction Get(RaceId race)
		{
			if ((uint)race >= ByRace.Length)
			{
				return null;
			}
			return ByRace[(uint)race];
		}

		public static FactionId GetId(FactionReputationIndex reputationIndex)
		{
			if (reputationIndex != FactionReputationIndex.End && (uint)reputationIndex < ByReputationIndex.Length)
			{
				var faction = ByReputationIndex[(uint)reputationIndex];
				if (faction != null)
				{
					return faction.Id;
				}
			}
			return 0;
		}

		public static FactionReputationIndex GetFactionIndex(FactionId id)
		{
			if ((uint)id < ByReputationIndex.Length)
			{
				var faction = ById[(uint)id];
				if (faction != null)
				{
					return faction.ReputationIndex;
				}
			}
			return 0;
		}
		#endregion

		/// <summary>
		/// Returns the FactionGroup of the given race.
		/// Throws KeyNotFoundException if race is not a valid player-race.
		/// </summary>
		/// <param name="race">the race</param>
		/// <returns>The FactionGroup of the race.</returns>
		public static FactionGroup GetFactionGroup(RaceId race)
		{
			return ByRace[(uint)race].Group;
		}
	}
}