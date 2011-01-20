using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.Util;
using WCell.Util.Variables;
using WCell.RealmServer.Quests;
using WCell.Constants.AreaTriggers;

namespace WCell.RealmServer.AreaTriggers
{
	/// <summary>
	/// Returns whether the given Character has triggered the given trigger or false if not allowed.
	/// </summary>
	/// <param name="chr"></param>
	/// <param name="trigger"></param>
	/// <returns></returns>
	public delegate bool AreaTriggerHandler(Character chr, AreaTrigger trigger);

	[GlobalMgr]
	public static class AreaTriggerMgr
	{
		static Logger log = LogManager.GetCurrentClassLogger();
	    private static bool _loaded;

		[NotVariable]
		public static AreaTrigger[] AreaTriggers = new AreaTrigger[3000];

		public static readonly AreaTriggerHandler[] Handlers = new AreaTriggerHandler[(int)AreaTriggerType.End];

		static AreaTriggerMgr()
		{
			Handlers[(int)AreaTriggerType.None] = NoAction;
			Handlers[(int)AreaTriggerType.QuestTrigger] = HandleQuestTrigger;
			Handlers[(int)AreaTriggerType.Rest] = HandleRest;
			Handlers[(int)AreaTriggerType.Teleport] = HandleTeleport;
			Handlers[(int)AreaTriggerType.Spell] = HandleSpell;
		}

		/// <summary>
		/// Do nothing
		/// </summary>
		/// <param name="arg1"></param>
		/// <param name="arg2"></param>
		/// <returns></returns>
		public static bool NoAction(Character arg1, AreaTrigger arg2) { return false; }

		public static void SetHandler(AreaTriggerType type, AreaTriggerHandler handler)
		{
			Handlers[(int)type] = handler;
		}

		/// <summary>
		/// Teleports into an instance
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public static bool HandleTeleport(Character chr, AreaTrigger trigger)
		{
			var mapInfo = World.GetMapTemplate(trigger.Template.TargetMap);
#if DEBUG
			chr.SendSystemMessage("Target location: {0}", trigger.Template.TargetMap);
#endif

			if (mapInfo.IsInstance)
			{
				if (mapInfo.Type == MapType.Normal)
				{
					InstanceMgr.LeaveInstance(chr, mapInfo, trigger.Template.TargetPos);
					return true;
				}
				else
				{
					return InstanceMgr.EnterInstance(chr, mapInfo, trigger.Template.TargetPos);
				}
			}
			else if (mapInfo.BGTemplate == null)
			{
				var rgn = World.GetMap(mapInfo.Id);
				if (rgn != null)
				{
					chr.TeleportTo(rgn, trigger.Template.TargetPos, trigger.Template.TargetOrientation);
					return true;
				}
				else
				{
					ContentMgr.OnInvalidDBData("Invalid Map: " + rgn);
				}
			}
			return true;
		}

		/// <summary>
		/// Triggers Quest
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public static bool HandleQuestTrigger(Character chr, AreaTrigger trigger)
		{
			var quest = chr.QuestLog.GetActiveQuest(trigger.Template.TriggerQuestId);
			if (quest != null)
			{
				// progress Quest
				quest.SignalATVisited(trigger.Id);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Start resting
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public static bool HandleRest(Character chr, AreaTrigger trigger)
		{
			chr.RestTrigger = trigger;
			return true;
		}

		/// <summary>
		/// Cast a spell on the Character.
		/// [NYI]
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="trigger"></param>
		/// <returns></returns>
		public static bool HandleSpell(Character chr, AreaTrigger trigger)
		{
			return false;
		}

		public static AreaTriggerHandler GetHandler(AreaTriggerType type)
		{
			var handler = Handlers[(int)type];
			if (handler == null)
			{
				handler = NoAction;
			}
			return handler;
		}

		/// <summary>
		/// Depends on Table-Creation (Third)
		/// </summary>
		[Initialization(InitializationPass.Fourth, "Initialize AreaTriggers")]
		public static void Initialize()
		{
            var reader = new MappedDBCReader<AreaTrigger, ATConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_AREATRIGGER));

			foreach (var at in reader.Entries)
			{
				ArrayUtil.Set(ref AreaTriggers, (uint)at.Key, at.Value);
			}

			ContentMgr.Load<ATTemplate>();

			if (RealmServer.InitMgr != null)
			{
				RealmServer.InitMgr.SignalGlobalMgrReady(typeof (AreaTriggerMgr));
			}
		}

        /// <summary>
        /// Loaded flag
        /// </summary>
        public static bool Loaded
        {
            get { return _loaded; }
            private set
            {
                if (_loaded = value)
                {
                    if (RealmServer.InitMgr != null)
                    {
                        RealmServer.InitMgr.SignalGlobalMgrReady(typeof(AreaTriggerMgr));
                    }
                }
            }
        }

		[Initialization]
		[DependentInitialization(typeof(AreaTriggerMgr))]
		[DependentInitialization(typeof(QuestMgr))]
		public static void InitializeQuestTriggers()
		{
			foreach (var at in AreaTriggers)
			{
				if (at == null)
				{
					continue;
				}
				var templ = at.Template;
				if (templ != null && templ.TriggerQuestId != 0)
				{
					templ.Type = AreaTriggerType.QuestTrigger;
					var quest = QuestMgr.GetTemplate(templ.TriggerQuestId);
					if (quest != null)
					{
						templ.Type = AreaTriggerType.QuestTrigger;
						quest.AddAreaTriggerObjective(at.Id);
					}
				}
			}
            if(AreaTriggers.Length > 0)
            {
		        Loaded = true;                
            }
		}

		public static AreaTrigger GetTrigger(uint id)
		{
			return AreaTriggers.Get(id);
		}

		public static AreaTrigger GetTrigger(AreaTriggerId id)
		{
			return AreaTriggers.Get((uint)id);
		}

	}
}