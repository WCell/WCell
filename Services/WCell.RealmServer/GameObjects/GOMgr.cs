using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.GameObjects
{
	/// <summary>
	/// General GameObject -utility and -container class
	/// </summary>
	[GlobalMgr]
	public static partial class GOMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Usually a Unit must be at least this close (in yards) to a GO in order to be allowed to interact with it
		/// </summary>
		public static uint DefaultInteractDistanceSq = 100;

		[NotVariable]
		/// <summary>
		/// All existing GOEntries by Entry-Id
		/// </summary>
		public static readonly Dictionary<uint, GOEntry> Entries =
		new Dictionary<uint, GOEntry>(10000);

		[NotVariable]
		/// <summary>
		/// All existing GOTemplates by MapId
		/// </summary>
		public static List<GOSpawn>[] TemplatesByMap = new List<GOSpawn>[(int)MapId.End];

		public static GOEntry GetEntry(uint id)
		{
			GOEntry entry;
			Entries.TryGetValue(id, out entry);
			return entry;
		}

		public static GOEntry GetEntry(GOEntryId id)
		{
			if (!loaded)
			{
				log.Warn("Tried to get GOEntry but GOs are not loaded: {0}", id);
				return null;
			}
			GOEntry entry;
			if (!Entries.TryGetValue((uint)id, out entry))
			{
				if (ContentHandler.ForceDataPresence)
				{
					throw new ContentException("Tried to get GOEntry but GOs are not loaded: {0}", id);
				}
			}
			return entry;
		}

		public static List<GOSpawn> GetTemplates(MapId map)
		{
			return TemplatesByMap.Get((uint)map);
		}

		#region Init / Load
		[Initialization(InitializationPass.Fifth, "Initialize GameObjects")]
		public static void Initialize()
		{
#if !DEV
				LoadAll();
#endif
		}

		private static bool loaded;

		/// <summary>
		/// Loaded flag
		/// </summary>
		public static bool Loaded
		{
			get { return loaded; }
			private set
			{
				if (loaded = value)
				{
					if (RealmServer.InitMgr != null)
					{
						RealmServer.InitMgr.SignalGlobalMgrReady(typeof(GOMgr));
					}
				}
			}
		}

		public static void AddTemplate(GOSpawn spawn)
		{
			var list = TemplatesByMap[(int)spawn.MapId];
			if (list == null)
			{
				TemplatesByMap[(int)spawn.MapId] = list = new List<GOSpawn>(100);
			}
			list.Add(spawn);
		}

		public static void LoadAll()
		{
			if (!Loaded)
			{
				ContentHandler.Load<GOEntry>();
				ContentHandler.Load<GOSpawn>();

				new GOPortalEntry().FinalizeDataHolder();

				foreach (var entry in Entries.Values)
				{
					if (entry.LinkedTrapId > 0)
					{
						entry.LinkedTrap = GetEntry(entry.LinkedTrapId) as GOTrapEntry;
					}
					if (entry.Templates.Count == 0)
					{
						// make sure, every Entry has at least one template
						entry.Templates.Add(new GOSpawn
						{
							Entry = entry,
							Rotations = new float[0],
							State = GameObjectState.Enabled
						});
					}
				}
				SetSummonSlots();

				Loaded = true;
			}
		}

		private static void SetSummonSlots()
		{
			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null) continue;

				foreach (var effect in spell.Effects)
				{
					if (effect.EffectType >= SpellEffectType.SummonObjectSlot1 &&
						effect.EffectType <= SpellEffectType.SummonObjectSlot4)
					{
						var entry = GetEntry((uint)effect.MiscValue);
						if (entry != null)
						{
							entry.SummonSlotId = (uint)(effect.EffectType - SpellEffectType.SummonObjectSlot1);
						}
					}
				}
			}
		}

		#endregion

		#region Handlers
		/// <summary>
		/// Contains a set of <see cref="GameObjectHandler"/>, indexed by <see cref="GameObjectType"/>. 
		/// </summary>
		public static readonly Func<GameObjectHandler>[] Handlers =
			new Func<GameObjectHandler>[(int)Utility.GetMaxEnum<GameObjectType>() + 1];

		static GOMgr()
		{
			Handlers[(int)GameObjectType.Door] = () => new DoorHandler();
			Handlers[(int)GameObjectType.Button] = () => new ButtonHandler();
			Handlers[(int)GameObjectType.QuestGiver] = () => new QuestGiverHandler();
			Handlers[(int)GameObjectType.Chest] = () => new ChestHandler();
			Handlers[(int)GameObjectType.Binder] = () => new BinderHandler();
			Handlers[(int)GameObjectType.Generic] = () => new GenericHandler();
			Handlers[(int)GameObjectType.Trap] = () => new TrapHandler();
			Handlers[(int)GameObjectType.Chair] = () => new ChairHandler();
			Handlers[(int)GameObjectType.SpellFocus] = () => new SpellFocusHandler();
			Handlers[(int)GameObjectType.Text] = () => new TextHandler();
			Handlers[(int)GameObjectType.Goober] = () => new GooberHandler();
			Handlers[(int)GameObjectType.Transport] = () => new TransportHandler();
			Handlers[(int)GameObjectType.AreaDamage] = () => new AreaDamageHandler();
			Handlers[(int)GameObjectType.Camera] = () => new CameraHandler();
			Handlers[(int)GameObjectType.MapObject] = () => new MapObjectHandler();
			Handlers[(int)GameObjectType.MOTransport] = () => new MOTransportHandler();
			Handlers[(int)GameObjectType.DuelFlag] = () => new DuelFlagHandler();
			Handlers[(int)GameObjectType.FishingNode] = () => new FishingNodeHandler();
			Handlers[(int)GameObjectType.SummoningRitual] = () => new SummoningRitualHandler();
			Handlers[(int)GameObjectType.Mailbox] = () => new MailboxHandler();
			//Handlers[(int)GameObjectType.DONOTUSE] = () => new AuctionHouseHandler();
			Handlers[(int)GameObjectType.GuardPost] = () => new GuardPostHandler();
			Handlers[(int)GameObjectType.SpellCaster] = () => new SpellCasterHandler();
			Handlers[(int)GameObjectType.MeetingStone] = () => new MeetingStoneHandler();
			Handlers[(int)GameObjectType.FlagStand] = () => new FlagStandHandler();
			Handlers[(int)GameObjectType.FishingHole] = () => new FishingHoleHandler();
			Handlers[(int)GameObjectType.FlagDrop] = () => new FlagDropHandler();
			Handlers[(int)GameObjectType.MiniGame] = () => new MiniGameHandler();
			Handlers[(int)GameObjectType.LotteryKiosk] = () => new LotteryKioskHandler();
			Handlers[(int)GameObjectType.CapturePoint] = () => new CapturePointHandler();
			Handlers[(int)GameObjectType.AuraGenerator] = () => new AuraGeneratorHandler();
			Handlers[(int)GameObjectType.DungeonDifficulty] = () => new DungeonDifficultyHandler();
			Handlers[(int)GameObjectType.BarberChair] = () => new BarberChairHandler();
			Handlers[(int)GameObjectType.DestructibleBuilding] = () => new DestructibleBuildingHandler();
			Handlers[(int)GameObjectType.GuildBank] = () => new GuildBankHandler();
			Handlers[(int)GameObjectType.TrapDoor] = () => new TrapDoorHandler();

			Handlers[(int)GameObjectType.Custom] = () => new CustomGOHandler();
		}
		#endregion

		#region GOEntries
		/// <summary>
		/// Contains a set of GOEntry-creators, indexed by <see cref="GameObjectType"/>. 
		/// Override these to create custom GOs.
		/// </summary>
		public static readonly Func<GOEntry>[] GOEntryCreators = new Func<Func<GOEntry>[]>(() =>
		{
			var goEntryCreators = new Func<GOEntry>[(int)Utility.GetMaxEnum<GameObjectType>() + 1];

			goEntryCreators[(int)GameObjectType.Door] = () => new GODoorEntry();
			goEntryCreators[(int)GameObjectType.Button] = () => new GOButtonEntry();
			goEntryCreators[(int)GameObjectType.QuestGiver] = () => new GOQuestGiverEntry();
			goEntryCreators[(int)GameObjectType.Chest] = () => new GOChestEntry();
			goEntryCreators[(int)GameObjectType.Binder] = () => new GOBinderEntry();
			goEntryCreators[(int)GameObjectType.Generic] = () => new GOGenericEntry();
			goEntryCreators[(int)GameObjectType.Trap] = () => new GOTrapEntry();
			goEntryCreators[(int)GameObjectType.Chair] = () => new GOChairEntry();
			goEntryCreators[(int)GameObjectType.SpellFocus] = () => new GOSpellFocusEntry();
			goEntryCreators[(int)GameObjectType.Text] = () => new GOTextEntry();
			goEntryCreators[(int)GameObjectType.Goober] = () => new GOGooberEntry();
			goEntryCreators[(int)GameObjectType.Transport] = () => new GOTransportEntry();
			goEntryCreators[(int)GameObjectType.AreaDamage] = () => new GOAreaDamageEntry();
			goEntryCreators[(int)GameObjectType.Camera] = () => new GOCameraEntry();
			goEntryCreators[(int)GameObjectType.MapObject] = () => new GOMapObjectEntry();
			goEntryCreators[(int)GameObjectType.MOTransport] = () => new GOMOTransportEntry();
			goEntryCreators[(int)GameObjectType.DuelFlag] = () => new GODuelFlagEntry();
			goEntryCreators[(int)GameObjectType.FishingNode] = () => new GOFishingNodeEntry();
			goEntryCreators[(int)GameObjectType.SummoningRitual] = () => new GOSummoningRitualEntry();
			goEntryCreators[(int)GameObjectType.Mailbox] = () => new GOMailboxEntry();
			goEntryCreators[(int)GameObjectType.DONOTUSE] = () => new GOAuctionHouseEntry();
			goEntryCreators[(int)GameObjectType.GuardPost] = () => new GOGuardPostEntry();
			goEntryCreators[(int)GameObjectType.SpellCaster] = () => new GOSpellCasterEntry();
			goEntryCreators[(int)GameObjectType.MeetingStone] = () => new GOMeetingStoneEntry();
			goEntryCreators[(int)GameObjectType.FlagStand] = () => new GOFlagStandEntry();
			goEntryCreators[(int)GameObjectType.FishingHole] = () => new GOFishingHoleEntry();
			goEntryCreators[(int)GameObjectType.FlagDrop] = () => new GOFlagDropEntry();
			goEntryCreators[(int)GameObjectType.MiniGame] = () => new GOMiniGameEntry();
			goEntryCreators[(int)GameObjectType.LotteryKiosk] = () => new GOLotteryKioskEntry();
			goEntryCreators[(int)GameObjectType.CapturePoint] = () => new GOCapturePointEntry();
			goEntryCreators[(int)GameObjectType.AuraGenerator] = () => new GOAuraGeneratorEntry();
			goEntryCreators[(int)GameObjectType.DungeonDifficulty] = () => new GODungeonDifficultyEntry();
			goEntryCreators[(int)GameObjectType.BarberChair] = () => new GOBarberChairEntry();
			goEntryCreators[(int)GameObjectType.DestructibleBuilding] = () => new GODestructibleBuildingEntry();
			goEntryCreators[(int)GameObjectType.GuildBank] = () => new GOGuildBankEntry();
			goEntryCreators[(int)GameObjectType.TrapDoor] = () => new GOTrapDoorEntry();

			return goEntryCreators;
		})();
		#endregion

		public static GOSpawn GetClosestTemplate(this ICollection<GOSpawn> templates, IWorldLocation pos)
		{
			var closestDistSq = float.MaxValue;
			GOSpawn closest = null;
			if (pos == null)
			{
				return templates.First();
			}

			foreach (var template in templates)
			{
				if (template == null || template.RegionId != pos.RegionId)
				{
					continue;
				}

				var distSq = pos.Position.DistanceSquared(template.Pos);
				if (distSq < closestDistSq)
				{
					closestDistSq = distSq;
					closest = template;
				}
			}
			return closest;
		}
	}
}