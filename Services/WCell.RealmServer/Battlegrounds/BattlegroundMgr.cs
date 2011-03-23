using System;
using System.Collections.Generic;
using WCell.Util.Collections;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.DynamicAccess;
using WCell.Util.Variables;

namespace WCell.RealmServer.Battlegrounds
{
	public delegate Battleground BattlegroundCreator();

	/* TODO
	 * Remove Arenas from current battlegrounds and implement a seperate queue for them
	 * 
	 */

	public static class BattlegroundMgr
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region Fields
		/// <summary>
		/// All Battleground instances
		/// </summary>
		public static readonly WorldInstanceCollection<BattlegroundId, Battleground> Instances =
			 new WorldInstanceCollection<BattlegroundId,Battleground>(BattlegroundId.End);

		public static MappedDBCReader<BattlemasterList, BattlemasterConverter> BattlemasterListReader;

        public static MappedDBCReader<PvPDifficultyEntry, PvPDifficultyConverter> PVPDifficultyReader;

		/// <summary>
		/// Indexed by BattlegroundId
		/// </summary>
		public static readonly BattlegroundTemplate[] Templates =
			new BattlegroundTemplate[(uint)BattlegroundId.End];

		private static Spell deserterSpell;

		private static SpellId deserterSpellId = SpellId.Deserter;

		/// <summary>
		/// Whether to flag <see cref="Character"/>s with the <see cref="DeserterSpell"/>
		/// </summary>
		[Variable("BGFlagDeserters")]
		public static bool FlagDeserters = true;

		/// <summary>
		/// Time until an invitation to a Battleground will be cancelled.
		/// Default: 2 minutes
		/// </summary>
		[Variable("BGInvitationTimeoutMillis")]
		public static int InvitationTimeoutMillis = 60 * 2 * 1000;

		[Variable("BGMaxAwardedHonor")]
		public static int MaxHonor = 10;

		[Variable("BGMaxHonorLevelDiff")]
		public static int MaxLvlDiff = 5;

		/// <summary>
		/// Amount of deaths that yield honor to the killing opponent
		/// </summary>
		[Variable("BGMaxHonorableDeaths")]
		public static int MaxHonorableDeaths = 50;

		/// <summary>
		/// Max amount of Battlegrounds one Character may queue up for at a time
		/// </summary>
		[Variable("BGMaxQueuesPerChar")]
		public static int MaxQueuesPerChar = 5;

		[NotVariable]
		public static Dictionary<int, WorldSafeLocation> WorldSafeLocs;

		/// <summary>
		/// The spell casted on players who leave a battleground before completion.
		/// </summary>
		public static SpellId DeserterSpellId
		{
			get { return deserterSpellId; }
			set
			{
				Spell spell = SpellHandler.Get(value);
				if (spell != null)
				{
					deserterSpellId = value;
					DeserterSpell = spell;
				}
			}
		}

		[NotVariable]
		public static Spell DeserterSpell
		{
			get { return deserterSpell; }
			set
			{
				deserterSpell = value;
				if (deserterSpell == null)
				{
					log.Error("Invalid DeserterSpellId: " + DeserterSpellId);
				}
				else
				{
					deserterSpellId = deserterSpell.SpellId;
				}
			}
		}
		#endregion

		#region Initialize
		public static bool Loaded { get; private set; }

		[Initialization(InitializationPass.Eighth, "Initialize Battlegrounds")]
		public static void InitializeBGs()
		{
            BattlemasterListReader = new MappedDBCReader<BattlemasterList, BattlemasterConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_BATTLEMASTERLIST));

            PVPDifficultyReader = new MappedDBCReader<PvPDifficultyEntry, PvPDifficultyConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_PVPDIFFICULTY));

			ContentMgr.Load<BattlegroundTemplate>();

			DeserterSpell = SpellHandler.Get(DeserterSpellId);

			Loaded = true;

			BattlegroundConfig.LoadSettings();
			//LoadWorldSafeLocs();

			EnsureBattlemasterRelations();
		}

		internal static void EnsureBattlemasterRelations()
		{
			if (NPCMgr.Loaded && Loaded)
			{
				ContentMgr.Load<BattlemasterRelation>();
			}
		}

		[Initialization(InitializationPass.Fourth, "Initialize WorldSafeLocs")]
		public static void LoadWorldSafeLocs()
		{
			var reader =
				new MappedDBCReader<WorldSafeLocation, DBCWorldSafeLocationConverter>(
                    RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_WORLDSAFELOCATION));
			WorldSafeLocs = reader.Entries;
		}

		#endregion

		#region Getters/Setters
		public static void SetCreator(BattlegroundId id, string typeName)
		{
			var type = RealmServer.GetType(typeName);
			var template = GetTemplate(id);
			if (type == null || template == null)
			{
				//throw new ArgumentException("Invalid Creator for type \"" + id + "\": " + typeName);
				object cause, causer;
				if (type == null)
				{
					cause = "type";
					causer = string.Format("({0}) - Please correct it in the Battleground-config file: {1}",
						typeName,
						BattlegroundConfig.Filename);
				}
				else
				{
					cause = "Template";
					causer = "<not in DB>";
				}
				log.Warn("Battleground {0} has invalid {1} {2}", id, cause, causer);
			}
			else
			{
				var producer = AccessorMgr.GetOrCreateDefaultProducer(type);
				template.Creator = () => (Battleground)producer.Produce();
			}
		}

		public static void SetCreator(BattlegroundId id, BattlegroundCreator creator)
		{
			GetTemplate(id).Creator = creator;
		}

		public static BattlegroundTemplate GetTemplate(BattlegroundId bgid)
		{
			return Templates.Get((uint)bgid);
		}

		/// <summary>
		/// Gets the global <see cref="BattlegroundQueue"/> for the given Battleground for
		/// the given Character.
		/// </summary>
		/// <param name="bgid"></param>
		/// <returns></returns>
		public static GlobalBattlegroundQueue GetGlobalQueue(BattlegroundId bgid, Unit unit)
		{
			return GetGlobalQueue(bgid, unit.Level);
		}

		/// <summary>
		/// Gets the global <see cref="BattlegroundQueue"/> for the given Battleground for
		/// the given Character.
		/// </summary>
		/// <param name="bgid"></param>
		/// <returns></returns>
		public static GlobalBattlegroundQueue GetGlobalQueue(BattlegroundId bgid, int level)
		{
			return Templates.Get((uint)bgid).GetQueue(level); //.GetTeamQueue(chr);
		}

		/// <summary>
		/// Gets the <see cref="BattlegroundQueue"/> for a specific instance of the given Battleground for
		/// the given Character.
		/// </summary>
		/// <param name="bgid"></param>
		/// <returns></returns>
		public static BattlegroundQueue GetInstanceQueue(BattlegroundId bgid, uint instanceId, Unit unit)
		{
			return GetInstanceQueue(bgid, instanceId, unit.Level);
		}

		/// <summary>
		/// Gets the <see cref="BattlegroundQueue"/> for a specific instance of the given Battleground for
		/// the given Character.
		/// </summary>
		/// <param name="bgid"></param>
		/// <param name="level">The level determines the bracket id of the queue.</param>
		/// <returns></returns>
		public static BattlegroundQueue GetInstanceQueue(BattlegroundId bgid, uint instanceId, int level)
		{
			Battleground bg = Templates.Get((uint)bgid).GetQueue(level).GetBattleground(instanceId);
			if (bg != null)
			{
				return bg.InstanceQueue; //.GetTeamQueue(chr);
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="bm"></param>
		/// <param name="chr"></param>
		public static void TalkToBattlemaster(this NPC bm, Character chr)
		{
			chr.OnInteract(bm);

			BattlegroundTemplate templ = bm.Entry.BattlegroundTemplate;
			if (templ != null)
			{
				GlobalBattlegroundQueue queue = templ.GetQueue(chr.Level);
				if (queue != null)
				{
					BattlegroundHandler.SendBattlefieldList(chr, queue);
				}
			}
		}

        public static uint GetHolidayIdByBGId(BattlegroundId bgId)
        {
            switch (bgId)
            {
                case BattlegroundId.AlteracValley:
                    return (uint)BattlegroundHolidays.CallToArmsAV;
                case BattlegroundId.WarsongGulch:
                    return (uint)BattlegroundHolidays.CallToArmsWS;
                case BattlegroundId.ArathiBasin:
                    return (uint)BattlegroundHolidays.CallToArmsAB;
                case BattlegroundId.EyeOfTheStorm:
                    return (uint)BattlegroundHolidays.CallToArmsEY;
                case BattlegroundId.StrandOfTheAncients:
                    return (uint)BattlegroundHolidays.CallToArmsSA;
                case BattlegroundId.IsleOfConquest:
                    return (uint)BattlegroundHolidays.CallToArmsIsleOfConquest;
                default:
                    return (uint)BattlegroundHolidays.None;
            }
        }
		#endregion

		#region Helper Methods

		public static BattlegroundSide GetBattlegroundSide(this FactionGroup faction)
		{
			if (faction == FactionGroup.Horde)
			{
				return BattlegroundSide.Horde;
			}
			return BattlegroundSide.Alliance;
		}

		public static BattlegroundSide GetOppositeSide(this BattlegroundSide side)
		{
			return side == BattlegroundSide.Alliance ? BattlegroundSide.Horde : BattlegroundSide.Alliance;
		}

		public static FactionGroup GetFactionGroup(this BattlegroundSide side)
		{
			if (side == BattlegroundSide.Horde)
			{
				return FactionGroup.Horde;
			}
			return FactionGroup.Alliance;
		}

		#endregion

		/// <summary>
		/// Enqueues players in a battleground queue.
		/// </summary>
		/// <param name="chr">the character who enqueued</param>
		/// <param name="bgId">the type of battleground</param>
		/// <param name="instanceId">the instance id of the battleground</param>
		/// <param name="asGroup">whether or not to enqueue the character or his/her group</param>
		internal static void EnqueuePlayers(Character chr, BattlegroundId bgId, uint instanceId, bool asGroup)
		{
			if (!chr.Battlegrounds.HasAvailableQueueSlots)
			{
				BattlegroundHandler.SendBattlegroundError(chr, BattlegroundJoinError.Max3Battles);
				return;
			}
			// cannot enqueue twice for the same bg
			if (chr.Battlegrounds.IsEnqueuedFor(bgId))
				return;

			var template = GetTemplate(bgId);

			// TODO: Is Character just updating an existing queue slot?
			if (template != null)
			{
				template.TryEnqueue(chr, asGroup, instanceId);
			}
		}
	}
}