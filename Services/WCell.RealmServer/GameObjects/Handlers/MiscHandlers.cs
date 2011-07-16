using System.Collections.Generic;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.GameObjects.Handlers
{
	/// <summary>
	/// GO Type 0: A door.
	/// </summary>
	public class DoorHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GODoorEntry;

            m_go.AnimationProgress = m_go.AnimationProgress == 100 ? (byte)0 : (byte)100;
			return true;
		}
	}

	/// <summary>
	/// GO Type 1: Use a Button to trigger something
	/// </summary>
	public class ButtonHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOButtonEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 2: An object that can start or end a quest.
	/// </summary>
	public class QuestGiverHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			var entry = (GOQuestGiverEntry)m_go.Entry;
			// usually there is nothing to do, since quests are resolved through the GO's GossipMenu
			return true;
		}
	}

	/// <summary>
	/// GO Type 4
	/// </summary>
	public class BinderHandler : GameObjectHandler
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			// This GOType is apprently unused.
			return true;
		}
	}

	/// <summary>
	/// GO Type 5
	/// </summary>
	public class GenericHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOGenericEntry;
			return true;
		}
	}

	/// <summary>
	/// GOType 7
	/// </summary>
	public class ChairHandler : GameObjectHandler
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The amount of people currently using this GO
		/// </summary>
		public int UserAmount;

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOChairEntry;
			bool onlyCreatorUse = entry.PrivateChair;
			if (onlyCreatorUse && m_go.CreatedBy != EntityId.Zero && user.EntityId != m_go.CreatedBy)
			{
				return false;
			}

			if (UserAmount < entry.MaxCount)
			{
				UserAmount++;
				user.Map.MoveObject(user, m_go.Position);
				user.Orientation = m_go.Orientation;
				user.StandState = entry.SitState;
				MovementHandler.SendHeartbeat(user, m_go.Position, m_go.Orientation);
			}
			return true;
		}
	}

	/// <summary>
	/// Type 8
	/// </summary>
	public class SpellFocusHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOSpellFocusEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 9
	/// </summary>
	public class TextHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOTextEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 10
	/// </summary>
	public class GooberHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			var entry = (GOGooberEntry)m_go.Entry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 11
	/// </summary>
	public class TransportHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOTransportEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 12
	/// </summary>
	public class AreaDamageHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOAreaDamageEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 13
	/// </summary>
	public class CameraHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOCameraEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 14
	/// </summary>
	public class MapObjectHandler : GameObjectHandler
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			// This object has no associated Fields.
			return true;
		}
	}

	/// <summary>
	/// GO Type 15
	/// </summary>
	public class MOTransportHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOMOTransportEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 16
	/// </summary>
	public class DuelFlagHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The Duel that is currently being fought
		/// </summary>
		public Duel Duel;

		public override bool Use(Character user)
		{
			// This object has no associated Fields.
			return true;
		}

		protected internal override void OnRemove()
		{
			if (Duel != null)
			{
				Duel.Cleanup();
				Duel = null;
			}
		}
	}

	/// <summary>
	/// GO Type 17
	/// </summary>
	public class FishingNodeHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			// This object has no associated Fields.
			return true;
		}
	}

	/// <summary>
	/// GO Type 18
	/// </summary>
	public class SummoningRitualHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public readonly List<Character> Users = new List<Character>(5);
		internal Unit Target;

		public override bool Use(Character user)
		{
			var entry = (GOSummoningRitualEntry)m_go.Entry;
			if (!(user is Character))
			{
				return false;
			}

			var chr = (Character)user;
			var caster = m_go.Owner;

			if ((entry.CastersGrouped && !caster.IsAlliedWith(chr)) || Users.Contains(chr))
			{
				return false;
			}

			Users.Add(chr);

			if (Users.Count >= entry.CasterCount - 1)
			{
				TriggerSpell(caster, Target);

				for (var i = 0; i < Users.Count; i++)
				{
					var curUser = Users[i];
					curUser.m_currentRitual = null;
				}
				Users.Clear();
			}
			return true;
		}

		public void TriggerSpell(Unit caster, Unit target)
		{
			caster.SpellCast.TriggerSingle(SpellHandler.Get(((GOSummoningRitualEntry)m_go.Entry).SpellId), target);
		}

		public void Remove(Character chr)
		{
			Users.Remove(chr);
			chr.m_currentRitual = null;
		}
	}

	/// <summary>
	/// GO Type 20
	/// </summary>
	public class AuctionHouseHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOAuctionHouseEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 21
	/// </summary>
	public class GuardPostHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOGuardPostEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 22
	/// </summary>
	public class SpellCasterHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private int chargesLeft;

		protected internal override void Initialize(GameObject go)
		{
			base.Initialize(go);

			var entry = (GOSpellCasterEntry)m_go.Entry;
			chargesLeft = entry.Charges;
		}

		public override bool Use(Character user)
		{
		    var entry = (GOSpellCasterEntry) m_go.Entry;
			if (entry.Spell == null)
			{
				return false;
			}

			m_go.SpellCast.Trigger(entry.Spell, user);
			if (chargesLeft == 1)
			{
				// party's over
				m_go.Delete();
			}
			else if (chargesLeft > 0)
			{
				chargesLeft--;
			}
			return true;
		}
	}

	/// <summary>
	/// GO Type 23
	/// </summary>
	public class MeetingStoneHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = (GOMeetingStoneEntry)m_go.Entry;
			if (!(user is Character))
			{
				return false;
			}

			var chr = (Character)user;
			var target = chr.Target;
			if (target == null || target == chr || !chr.IsAlliedWith(target))
			{
				return false;
			}

			var level = chr.Level;
			if (level < entry.MinLevel || level > entry.MaxLevel)
			{
				// try this:
				//SpellHandler.SendCastFailed(chr, 0, SpellId.MeetingStoneSummon, SpellFailedReason.LevelRequirement);
				return false;
			}

			chr.SpellCast.Start(SpellHandler.Get(SpellId.MeetingStoneSummon), false, new[] { target });

			return true;
		}
	}

	/// <summary>
	/// GO Type 24: Used for CTF
	/// </summary>
	public class FlagStandHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOFlagStandEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 25
	/// </summary>
	public class FishingHoleHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOFishingHoleEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 26
	/// </summary>
	public class FlagDropHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOFlagDropEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 27
	/// </summary>
	public class MiniGameHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOMiniGameEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 28
	/// </summary>
	public class LotteryKioskHandler : GameObjectHandler
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			// There are no Fields associated with this object.
			return true;
		}
	}

	/// <summary>
	/// GO Type 29
	/// </summary>
	public class CapturePointHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = (GOCapturePointEntry)m_go.Entry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 30
	/// </summary>
	public class AuraGeneratorHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GOAuraGeneratorEntry;
			return true;
		}
	}

	/// <summary>
	/// GO Type 31: Instance Portals (apparently)
	/// </summary>
	public class DungeonDifficultyHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GODungeonDifficultyEntry;
			return true;
		}
	}

	public class BarberChairHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			if (m_go.Entry is GOBarberChairEntry)
			{
				var entry = m_go.Entry as GOBarberChairEntry;

				// NPCs obviously can't do this.
				if (!(user is Character))
				{
					return false;
				}

				var character = user as Character;

				// Teleport to the GO.
				character.Orientation = m_go.Orientation;
				character.TeleportTo(m_go);

				// Notify client.
				CharacterHandler.SendEnableBarberShop(character);

				character.StandState = entry.SitState;
			}
			else
			{
				log.Error("BarberChairHandler: Incorrect underlying Entry type: ({0}) for this handler.", m_go.Entry);
			}
			return true;
		}
	}

	public class DestructibleBuildingHandler : GameObjectHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public override bool Use(Character user)
		{
			var entry = m_go.Entry as GODestructibleBuildingEntry;
			return true;
		}
	}

	public class GuildBankHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			// Does this object has fields ?
			return true;
		}
	}

	public class TrapDoorHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			// TODO: Trap doors
			return true;
		}
	}

	public class CustomGOHandler : GameObjectHandler
	{
		public override bool Use(Character user)
		{
			return true;
		}
	}
}