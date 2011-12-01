using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.PacketAnalysis.Logs;
using WCell.PacketAnalysis.Updates;
using WCell.Util;

namespace WCell.PacketAnalysis.Samples
{
	public class TrainerInfoExtractor
	{
		private static IndentTextWriter writer;

		private static Dictionary<FactionReputationIndex, ReputationInfo> InfosById = 
			new Dictionary<FactionReputationIndex, ReputationInfo>(100);

		private static Dictionary<EntityId, ReputationInfo> InfosByEntity = 
			new Dictionary<EntityId, ReputationInfo>();


		public static void Extract(string dir, string outFile)
		{
			using (writer = new IndentTextWriter(outFile))
			{
				GenericLogParser.ParseDir(SniffitztLogConverter.Extract, dir,
										  new LogHandler(opcode => opcode == RealmServerOpCode.SMSG_INITIALIZE_FACTIONS, HandleInitFactions),
										  new LogHandler(opcode => opcode == RealmServerOpCode.SMSG_SET_FACTION_STANDING, UpdateReputations),
										  new LogHandler(opcode => opcode == RealmServerOpCode.SMSG_TRAINER_LIST, HandleTrainerList),
										  new LogHandler(HandleUpdates)
										  );
			}
		}

		static void HandleTrainerList(PacketParser parser)
		{
			var spells = parser.ParsedPacket["Spells"].List;
			foreach (var spellSegment in spells)
			{
				var id = (SpellId)spellSegment["Spell"].UIntValue;
				var moneyCost = spellSegment["MoneyCost"].IntValue;
				var talentCost = spellSegment["TalentCost"].IntValue;
				var profCost = spellSegment["ProfessionPointCost"].IntValue;
				int reqLevel = spellSegment["RequiredLevel"].ByteValue;
				var reqSkill = (SkillId)spellSegment["RequiredSkill"].UIntValue;
				var reqSkillValue = spellSegment["RequiredSkillLevel"].IntValue;
				var reqSpells = new SpellId[3];

				reqSpells[0] = (SpellId)spellSegment["RequiredSpellId1"].UIntValue;
				reqSpells[1] = (SpellId)spellSegment["RequiredSpellId2"].UIntValue;
				reqSpells[2] = (SpellId)spellSegment["RequiredSpellId3"].UIntValue;

				// TODO: Calc exact money cost, depending on the faction
			}
		}

		static void HandleInitFactions(PacketParser parser)
		{
			var reps = parser.ParsedPacket.List;

			for (var i = 0; i < reps.Count; i++)
			{
				var rep = reps[i];
				var index = (FactionReputationIndex)i;
				var value = rep["Value"].IntValue;

				InfosById.Add(index, new ReputationInfo(index) { Value = value });
			}
		}

		static void UpdateReputations(PacketParser parser)
		{
			var reps = parser.ParsedPacket["Factions"].List;

			foreach (var rep in reps)
			{
				var id = (FactionReputationIndex)rep["Faction"].UIntValue;
				var value = rep["Value"].IntValue;
				var info = GetInfo(id);

				info.Value += value;
			}
		}

		static void HandleUpdates(ParsedUpdatePacket packet)
		{
			foreach (var block in packet.Blocks)
			{
				if (block.EntityId.High == HighId.Unit && 
					block.Values != null && 
					block.IsSet(UnitFields.FACTIONTEMPLATE))
				{
					var factionTemplate = block.GetUInt(UnitFields.FACTIONTEMPLATE);
					//var factionIndex = 
					//if (!InfosByEntity.Contains(factionIndex))
					//{
					//    InfosByEntity.Add(GetInfo(factionIndex));
					//}
					
				}
			}
		}

		static ReputationInfo GetInfo(FactionReputationIndex index)
		{
			ReputationInfo rep;
			if (!InfosById.TryGetValue(index, out rep))
			{
				InfosById.Add(index, rep = new ReputationInfo(index));
			}
			return rep;
		}
	}

	class ReputationInfo
	{
		public FactionReputationIndex Faction;
		public int Value;

		public ReputationInfo(FactionReputationIndex faction)
		{
			Faction = faction;
		}
	}
}