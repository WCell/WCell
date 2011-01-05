using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Addons.Default.Lang;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Global;
using WCell.Constants;
using WCell.RealmServer.Spells;
using WCell.Core;
using WCell.Constants.World;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{
	/// <summary>
	/// Represents progress of one Faction within WarsongGulch
	/// </summary>
	public abstract class WSGFaction : BattlegroundFaction
	{
		#region Events

		public event FlagActionHandler FlagDropped;

		public event FlagActionHandler FlagPickedUp;

		/// <summary>
		/// Character can be null.
		/// </summary>
		public event FlagActionHandler FlagReturned;

		public event FlagActionHandler FlagCaptured;

		#endregion

		#region Fields
		public readonly WarsongGulch Instance;
		public readonly List<FlagCapture> FlagCaptures = new List<FlagCapture>();
		public Character FlagCarrier;

		private DateTime _flagPickUpTime;
		private GameObject _flag;
		private bool _isFlagHome;
		private int _flagRespawnTime;
		public GOEntry FlagStandEntry;
		public GOEntry DroppedFlagEntry;

		/// <summary>
		/// Whether the flag is currently being captured
		/// </summary>
		public bool IsFlagCap;

		private Spell _flagSpell;
		private Spell _flagDropSpell;
		private Spell _flagDropDebuff;
		private Spell _flagCarrierDebuffSpell;

		// TODO: check this. it's always null, so...?
#pragma warning disable 0649
		private OneShotUpdateObjectAction _debuffUpdate;
#pragma warning restore 0649

		#endregion

		protected WSGFaction(WarsongGulch instance,
			SpellId flagSpell,
			SpellId flagDropSpell,
			SpellId flagDropDebuff,
			SpellId flagCarrierDebuffSpellId,
			GOEntryId flagStand,
			GOEntryId flagDropId)
		{
			Instance = instance;
			_flagSpell = SpellHandler.Get(flagSpell);
			_flagDropSpell = SpellHandler.Get(flagDropSpell);
			_flagDropDebuff = SpellHandler.Get(flagDropDebuff);
			_flagCarrierDebuffSpell = SpellHandler.Get(flagCarrierDebuffSpellId);
			FlagStandEntry = GOMgr.GetEntry(flagStand);
			DroppedFlagEntry = GOMgr.GetEntry(flagDropId);

			_flagRespawnTime = WarsongGulch.FlagRespawnTimeMillis;
			Score = 0;
		}

		#region Properties
		public abstract BattlegroundSide Side
		{
			get;
		}

		public abstract string Name
		{
			get;
		}

		public BattlegroundTeam Team
		{
			get { return Instance.GetTeam(Side); }
		}

		public WSGFaction Opponent
		{
			get { return Instance.GetFaction(Side.GetOppositeSide()); }
		}

		public bool IsFlagHome
		{
			get { return _isFlagHome; }
			set { _isFlagHome = value; }
		}

		/// <summary>
		/// The current  flag. Null if flag is being carried.
		/// Set whenever a new flag is spawned or dropped or the flag is picked up.
		/// </summary>
		public GameObject Flag
		{
			get { return _flag; }
			set { _flag = value; }
		}

		public abstract WorldStateId ScoreStateId
		{
			get;
		}

		public int Score
		{
			get { return Instance.WorldStates.GetInt32(ScoreStateId); }
			set
			{
				Instance.WorldStates.SetInt32(ScoreStateId, value);
				if (value >= Instance.MaxScore)
				{
					Instance.FinishFight();
				}
			}

		}

		/// <summary>
		/// The time the flag was picked up.
		/// </summary>
		public DateTime FlagPickUpTime
		{
			get { return _flagPickUpTime; }
		}

		#endregion

		/// <summary>
		/// Spawns the  flag at the base flagstand (if not already existing)
		/// </summary>
		public GameObject RespawnFlag()
		{
			if (_flag == null)
			{
				_flag = FlagStandEntry.FirstSpawn.Spawn(Instance);

				_isFlagHome = true;

				return _flag;
			}
			return _flag;
		}

		/// <summary>
		/// Checks whether a character is currently allowed to pick up a flag
		/// </summary>
		/// <param name="chr">The character using the flag.</param>
		/// <returns></returns>
		public bool CanPickupFlag(Character chr)
		{
			return chr.Role.IsStaff || (IsFlagHome && !chr.Auras.Contains(_flagDropDebuff.SpellId));
		}

		/// <summary>
		/// Picks up this side's flag.
		/// </summary>
		/// <param name="chr">The character picking up the flag.</param>
		public void PickupFlag(Character chr)
		{
			FlagCarrier = chr;

			// Shows the flag on the character. Does all kinds of stuff in the handler.
			chr.Auras.CreateSelf(_flagSpell, true);
			_debuffUpdate = chr.CallDelayed(_flagRespawnTime, obj => ApplyFlagCarrierDebuff());

			if (_flag != null)
			{
				_flag.SendDespawn(); // Dispose of the GO.
				_flag.Delete();
				_flag = null;
			}

			_flagPickUpTime = DateTime.Now;
			_isFlagHome = false;

			Instance.Characters.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.WSPickupFlag, Name, chr.Name));
			var evt = FlagPickedUp;
			if (evt != null)
			{
				evt(chr);
			}
		}

		/// <summary>
		/// Caps the flag. Notifies the participants.
		/// </summary>
		public bool CaptureFlag(Character capturer)
		{
			IsFlagCap = true;

			var msg = DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.WSCaptureFlag, capturer.Name, Name);
			ChatMgr.SendSystemMessage(Instance.Characters, msg);

			Opponent.Score++;

			OnFlagAuraRemoved(capturer);

			var evt = FlagCaptured;
			if (evt != null)
			{
				FlagCaptured(FlagCarrier);
			}

			if (FlagCarrier.Auras.Cancel(_flagSpell.SpellId))
			{
				Instance.CallDelayed(_flagRespawnTime, () => RespawnFlag()); //Respawn the flag in X seconds

				// We no longer have a carrier.
				FlagCarrier = null;
				IsFlagCap = false;
				FlagCaptures.Add(new FlagCapture(capturer, DateTime.Now));
				var stats = (WSGStats)capturer.Battlegrounds.Stats;
				stats.FlagCaptures++;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Drops the  flag to the world. Notifies the participants.
		/// Casts the debuffs.
		/// </summary>
		public void DropFlag()
		{
			if (FlagCarrier != null && FlagCarrier.IsInWorld)
			{
				FlagCarrier.SpellCast.Trigger(_flagDropSpell);
				FlagCarrier.Auras.CreateSelf(_flagDropDebuff, false); // confirmed from logs

                var msg = DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.WSDropFlag, Name, FlagCarrier.Name);
				ChatMgr.SendSystemMessage(Instance.Characters, msg);

				OnFlagAuraRemoved(FlagCarrier);

				var evt = FlagDropped;
				if (evt != null)
				{
					evt(FlagCarrier);
				}

				FlagCarrier = null; // We no longer have a carrier
			}

			_flag.CallDelayed(5 * 1000, obj => ReturnFlag(null)); //5 s return timer, cancelled when picked up/returned
		}

		/// <summary>
		/// Summons the dropped flag.
		/// </summary>
		/// <param name="location">The target location (can [and should] be a unit)</param>
		public void SummonDroppedFlag(IWorldLocation location)
		{
			_flag = DroppedFlagEntry.Spawn(location, null); // Flags never have an owner.
		}

		public bool CanReturnFlag(Character chr)
		{
			// The side matches and the flag is *not* the flag stand. Should be rather safe.
			if (chr.Battlegrounds.Team.Side == Side && Flag.Entry.GOId != FlagStandEntry.GOId)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Returns the flag to it's base. Character can be null.
		/// </summary>
		/// <param name="returner">Returner</param>
		public void ReturnFlag(Character returner)
		{
			if (_flag != null)
			{
				_flag.Delete();
				_flag = null;
			}

			RespawnFlag();

			if (returner != null)
			{
				var msg = DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.WSReturnFlagByPlayer, Name, returner.Name);
				ChatMgr.SendSystemMessage(Instance.Characters, msg);
				var stats = (WSGStats)returner.Battlegrounds.Stats;
				stats.FlagReturns++;
			}
			else
			{
				var msg = DefaultAddonLocalizer.Instance.Translate(AddonMsgKey.WSReturnFlag, Name);
				ChatMgr.SendSystemMessage(Instance.Characters, msg);
			}

			var evt = FlagReturned;
			if (evt != null)
			{
				evt(returner);
			}
		}

		/// <summary>
		/// Called when flag is captured, or dropped
		/// </summary>
		void OnFlagAuraRemoved(Character chr)
		{
			chr.RemoveUpdateAction(_debuffUpdate);
		}

		private void ApplyFlagCarrierDebuff()
		{
			if (FlagCarrier != null && FlagCarrier.IsInWorld)
			{
				FlagCarrier.Auras.CreateSelf(_flagCarrierDebuffSpell, false);
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Flag = null;
			FlagCarrier = null;
		}

		#endregion
	}

	public class FlagCapture
	{
		private uint m_capturerLowId;
		private DateTime m_captureTime;


		public FlagCapture(Character capturer, DateTime capture)
		{
			if (capturer != null)
			{
				m_capturerLowId = capturer.EntityId.Low;
			}
			m_captureTime = capture;
		}

		/// <summary>
		/// The low EntityId of the Capturing character.
		/// </summary>
		public uint CapturerLowId
		{
			get { return m_capturerLowId; }
		}

		/// <summary>
		/// The capturing Character. 
		/// Might be null if Character already logged out.
		/// </summary>
		public Character Capturer
		{
			get { return World.GetCharacter(m_capturerLowId); }
		}

		public DateTime CaptureTime
		{
			get { return m_captureTime; }
		}
	}
}