using System;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Spells;
using WCell.Util.Variables;

namespace WCell.RealmServer.Battlegrounds
{
	public abstract class Battleground : InstancedMap, IBattlegroundRange
	{
		/// <summary>
		/// Whether to add to a Team even if it already has more players
		/// </summary>
		[Variable("BGAddPlayersToBiggerTeam")]
		public static bool AddPlayersToBiggerTeamDefault = true;

		/// <summary>
		/// Whether to buff <see cref="Character"/>s of smaller Teams if they have less players
		/// </summary>
		[Variable("BGBuffSmallerTeam")]
		public static bool BuffSmallerBGTeam = true;

		/// <summary>
		/// Default delay until shutdown when player-count drops 
		/// below minimum in seconds
		/// </summary>
		[Variable("BGDefaultShutdownDelay")]
		public static int DefaultShutdownDelayMillis = 200000;

		/// <summary>
		/// Start the BG once this many % of players joined
		/// </summary>
		[Variable("BGStartPlayerPct")]
		public static uint StartPlayerPct = 80;

		[Variable("BGUpdateQueueSeconds")]
		public static int UpdateQueueMillis = 10000;

		protected readonly BattlegroundTeam[] _teams;

		/// <summary>
		/// All currently pending requests of Characters who want to join this particular Battleground.
		/// </summary>
		protected InstanceBattlegroundQueue _instanceQueue;
		protected GlobalBattlegroundQueue _parentQueue;
		private Spell _preparationSpell;
		protected TimerEntry _queueTimer;
		protected TimerEntry _shutdownTimer;
		protected DateTime _startTime;
		protected BattlegroundStatus _status;
		protected BattlegroundTeam _winner;
		protected BattlegroundTemplate _template;
		protected int _minLevel, _maxLevel;

		public Battleground(int minLevel, int maxlevel, BattlegroundTemplate template)
		{
			_minLevel = minLevel;
			_maxLevel = maxlevel;
			_template = template;
		}

		public Battleground()
		{
			if (HasQueue)
			{
				_queueTimer = new TimerEntry(dt => ProcessPendingPlayers());
				RegisterUpdatable(_queueTimer);
			}

			_status = BattlegroundStatus.None;
			AddPlayersToBiggerTeam = AddPlayersToBiggerTeamDefault;

			_teams = new BattlegroundTeam[2];

			_shutdownTimer = new TimerEntry(dt => Delete());
			RegisterUpdatable(_shutdownTimer);
		}

		/// <summary>
		/// The current <see cref="BattlegroundStatus"/>
		/// </summary>
		public BattlegroundStatus Status
		{
			get { return _status; }
			protected set { _status = value; }
		}

		/// <summary>
		/// The <see cref="BattlegroundQueue"/> that manages requests for this particular Instance
		/// </summary>
		public InstanceBattlegroundQueue InstanceQueue
		{
			get { return _instanceQueue; }
		}

		/// <summary>
		/// The <see cref="BattlegroundQueue"/> that manages general requests (not for a particular Instance)
		/// </summary>
		public GlobalBattlegroundQueue ParentQueue
		{
			get { return _parentQueue; }
			protected internal set
			{
				_parentQueue = value;
				_minLevel = _parentQueue.MinLevel;
				_maxLevel = _parentQueue.MaxLevel;
				_template = _parentQueue.Template;
			}
		}

		/// <summary>
		/// The team that won (or null if still in progress)
		/// </summary>
		public BattlegroundTeam Winner
		{
			get { return _winner; }
			protected set
			{
				_winner = value;
				value.ForeachCharacter( chr => chr.Achievements.CheckPossibleAchievementUpdates(Constants.Achievements.AchievementCriteriaType.CompleteBattleground, (uint)MapId ,1));
			}
		}

		public virtual SpellId PreparationSpellId
		{
			get { return SpellId.Preparation; }
		}

		/// <summary>
		/// Get/sets whether this Battleground will shutdown soon
		/// </summary>
		public bool IsShuttingDown
		{
			get { return _shutdownTimer.IsRunning; }
			set
			{
				if (value)
				{
					RemainingShutdownDelay = DefaultShutdownDelayMillis;
				}
				else
				{
					RemainingShutdownDelay = -1;
				}
			}
		}

		/// <summary>
		/// The time when this BG started. default(DateTime) before it actually started.
		/// </summary>
		public DateTime StartTime
		{
			get { return _startTime; }
		}

		/// <summary>
		/// Returns the time since the Battleground started in millis or 0 while still preparing.
		/// </summary>
		public int RuntimeMillis
		{
			get
			{
				if (_status == BattlegroundStatus.Active ||
				   _status == BattlegroundStatus.Finished)
				{
					return (int)(DateTime.Now - StartTime).TotalMilliseconds;
				}
				return 0;
			}
		}

		/// <summary>
		/// Preparation time of this BG
		/// </summary>
		public virtual int PreparationTimeMillis
		{
			get { return 2 * 60 * 1000; }
		}

		/// <summary>
		/// Time until shutdown.
		/// Non-positive value cancels shutdown.
		/// </summary>
		public int RemainingShutdownDelay
		{
			get { return _shutdownTimer.RemainingInitialDelayMillis; }
			set
			{
				EnsureContext();

				if ((value > 0) != _shutdownTimer.IsRunning)
				{
					if (value <= 0)
					{
						_shutdownTimer.Stop();
					}
					else
					{
						StartShutdown(value);
					}
					// TODO: Inform players
				}
			}
		}

		/// <summary>
		/// Whether or not this battleground is currently active. (in progress)
		/// </summary>
		public new bool IsActive
		{
			get { return _status == BattlegroundStatus.Active; }
		}

		/// <summary>
		/// Whether to allow people to join this Battleground
		/// </summary>
		public virtual bool IsOpen
		{
			get { return _status != BattlegroundStatus.Finished; }
		}

		/// <summary>
		/// Whether enqueued Characters are actively being processed
		/// </summary>
		public virtual bool IsAddingPlayers
		{
			get { return IsActive; }
		}

        /// <summary>
        /// Whether to start the mode "Call To Arms" and change timers
        /// </summary>
        public virtual bool IsHolidayBG
        {
            get { return WorldEventMgr.IsHolidayActive(BattlegroundMgr.GetHolidayIdByBGId(Template.Id)); }
        }

		/// <summary>
		/// Whether to start the Shutdown timer when <see cref="Map.PlayerCount"/> drops below the minimum
		/// </summary>
		public bool CanShutdown { get; set; }

		/// <summary>
		/// Whether to add Players from the Queue even if the Team
		/// already has more Players
		/// </summary>
		public bool AddPlayersToBiggerTeam { get; set; }

		#region IBattlegroundRange Members

		public override int MinLevel
		{
			get { return _minLevel; }
		}

		public override int MaxLevel
		{
			get { return _maxLevel; }
		}

		/// <summary>
		/// 
		/// </summary>
		public BattlegroundTemplate Template
		{
			get { return _template; }
		}

		#endregion

		/// <summary>
		/// Starts the shutdown timer with the given delay
		/// </summary>
		protected virtual void StartShutdown(int millis)
		{
			_shutdownTimer.Start(millis);

			foreach (var chr in m_characters)
			{
				chr.Auras.Remove(_preparationSpell);
			}
		}

		public override bool CanEnter(Character chr)
		{
			return IsOpen && base.CanEnter(chr);
		}

		public virtual void StartPreparation()
		{
			ExecuteInContext(() =>
			{
				_status = BattlegroundStatus.Preparing;

				if (_preparationSpell != null)
				{
					foreach (Character chr in m_characters)
					{
						chr.SpellCast.TriggerSelf(_preparationSpell);
					}
				}

				CallDelayed(PreparationTimeMillis / 2, OnPrepareHalftime);

				OnPrepare();
			});
		}

		public virtual void StartFight()
		{
			ExecuteInContext(() =>
			{
				if (!IsDisposed && _status != BattlegroundStatus.Active)
				{
					_startTime = DateTime.Now;
					_status = BattlegroundStatus.Active;
					foreach (var chr in m_characters)
					{
						chr.Auras.Remove(_preparationSpell);
					}
					OnStart();
				}
			});
		}

		/// <summary>
		/// Enter the final state
		/// </summary>
		public virtual void FinishFight()
		{
			_status = BattlegroundStatus.Finished;
			RewardPlayers();
			SendPvpData();
			ExecuteInContext(() => FinalizeBattleground(false));
		}

		/// <summary>
		/// Toggle <see cref="IsShuttingDown"/> or <see cref="CanShutdown"/> if required
		/// </summary>
		protected void CheckShutdown()
		{
			if (PlayerCount < MaxPlayerCount && CanShutdown)
			{
				if (!IsShuttingDown)
				{
					IsShuttingDown = true;
				}
			}
			else
			{
				if (IsShuttingDown)
				{
					IsShuttingDown = false;
				}
			}
		}

		/// <summary>
		/// Returns the <see cref="BattlegroundTeam"/> of the given side
		/// </summary>
		/// <param name="side"></param>
		/// <returns></returns>
		public BattlegroundTeam GetTeam(BattlegroundSide side)
		{
			return _teams[(int)side];
		}

		public BattlegroundTeam AllianceTeam
		{
			get { return GetTeam(BattlegroundSide.Alliance); }
		}

		public BattlegroundTeam HordeTeam
		{
			get { return GetTeam(BattlegroundSide.Horde); }
		}

		#region Join

		/// <summary>
		/// Adds the given Character (or optionally his/her Group) to the Queue of this Battleground if possible.
		/// Make sure that HasQueue is true before calling this method.
		/// </summary>
		/// <param name="chr"></param>
		/// <param name="asGroup"></param>
		public void TryJoin(Character chr, bool asGroup, BattlegroundSide side)
		{
			ExecuteInContext(() => DoTryJoin(chr, asGroup, side));
		}

		protected void DoTryJoin(Character chr, bool asGroup, BattlegroundSide side)
		{
			EnsureContext();

			// if the character isn't in game, they can't join.
			if (!chr.IsInWorld)
				return;

			// can't join if the battleground isn't open
			if (!IsOpen)
				return;

			var team = GetTeam(side);
			var teamQueue = _instanceQueue.GetTeamQueue(side);

			var chrs = teamQueue.GetCharacterSet(chr, asGroup);
			if (chrs != null)
			{
				team.Enqueue(chrs);
			}
		}

		#endregion

		#region Queue Processing

		/// <summary>
		/// Add as many players possible to both Teams
		/// </summary>
		public void ProcessPendingPlayers()
		{
			ProcessPendingPlayers(_teams[(int)BattlegroundSide.Alliance]);
			ProcessPendingPlayers(_teams[(int)BattlegroundSide.Horde]);
		}

		/// <summary>
		/// Removes up to the max possible amount of players from the Queues
		/// and adds them to the Battleground.
		/// </summary>
		public int ProcessPendingPlayers(BattlegroundTeam team)
		{
			if (!IsAddingPlayers)
				return 0;

			int amount = team.OpenPlayerSlotCount;

			if (amount > 0)
			{
				return ProcessPendingPlayers(team.Side, amount);
			}

			return 0;
		}

		/// <summary>
		/// Removes up to the given amount of players from the Queues
		/// and adds them to the Battleground.
		/// </summary>
		/// <param name="amount"></param>
		/// <remarks>Map-Context required. Cannot be used once the Battleground is over.</remarks>
		/// <returns>The amount of remaining players</returns>
		public int ProcessPendingPlayers(BattlegroundSide side, int amount)
		{
			EnsureContext();

			if (_instanceQueue == null)
				return 0;

			// dequeue from the instance queue first
			amount -= _instanceQueue.GetTeamQueue(side).DequeueCharacters(amount, this);

			if (amount > 0)
			{
				// we have slots left, try to pull from the global queue
				amount -= _parentQueue.GetTeamQueue(side).DequeueCharacters(amount, this);
			}

			// return the number of player slots that didn't get filled
			return amount;
		}

		#endregion

		#region Finish up

		/// <summary>
		/// Starts to clean things up once the BG is over.
		/// Might be called right before the BG is disposed.
		/// </summary>
		protected void FinalizeBattleground(bool disposing)
		{
			EnsureContext();
			if (_shutdownTimer != null)
			{
				_shutdownTimer = null;

				OnFinish(disposing);
				if (_instanceQueue != null)
				{
					_instanceQueue.Dispose();
					_instanceQueue = null;
				}

				if (_parentQueue != null)
				{
					_parentQueue.OnRemove(this);
				}
			}
		}

		public override void DeleteNow()
		{
			BattlegroundMgr.Instances.RemoveInstance(Template.Id, InstanceId);
			FinalizeBattleground(true); // make sure that things are cleaned up

			base.DeleteNow();
		}

		public void SendPvpData()
		{
			foreach (var chr in m_characters)
			{
				BattlegroundHandler.SendPvpData(chr, chr.Battlegrounds.Team.Side, this);
			}
		}

		protected override void Dispose()
		{
			base.Dispose();

			foreach (var team in _teams)
			{
				team.Dispose();
			}

			_parentQueue = null;
		}

		#endregion

		#region Events

		protected virtual void OnPrepareHalftime()
		{
			CallDelayed(PreparationTimeMillis / 2, StartFight);
		}

		protected virtual void OnPrepare()
		{
		}

		protected virtual void OnStart()
		{
            MiscHandler.SendPlaySoundToMap(this, (uint)BattlegroundSounds.BgStart);
		}

		protected virtual void OnFinish(bool disposing)
		{
            MiscHandler.SendPlaySoundToMap(this, Winner.Side == BattlegroundSide.Horde ? (uint)BattlegroundSounds.HordeWins 
                                                                                           : (uint)BattlegroundSounds.AllianceWins);
        }

        public virtual void OnPlayerClickedOnflag(GameObject go, Character chr)
        {
        }

		#endregion

		#region Overrides
		public virtual bool HasQueue { get { return true; } }

		protected internal override void InitMap()
		{
			base.InitMap();
			BattlegroundMgr.Instances.AddInstance(Template.Id, this);

			_preparationSpell = SpellHandler.Get(PreparationSpellId);

			if (HasQueue)
			{
				_instanceQueue = new InstanceBattlegroundQueue(this);
				_queueTimer.Start(0, UpdateQueueMillis);
			}

			_teams[(int)BattlegroundSide.Alliance] = CreateAllianceTeam();
			_teams[(int)BattlegroundSide.Horde] = CreateHordeTeam();

			ProcessPendingPlayers();
		}

		/// <summary>
		/// Creates a new BattlegroundTeam during initialization of the BG
		/// </summary>
		protected virtual BattlegroundTeam CreateHordeTeam()
		{
			return new BattlegroundTeam(
				_instanceQueue != null ? _instanceQueue.GetTeamQueue(BattlegroundSide.Horde) : null, BattlegroundSide.Horde, this);
		}

		/// <summary>
		/// Creates a new BattlegroundTeam during initialization of the BG
		/// </summary>
		protected virtual BattlegroundTeam CreateAllianceTeam()
		{
			return new BattlegroundTeam(
				_instanceQueue != null ? _instanceQueue.GetTeamQueue(BattlegroundSide.Alliance) : null, BattlegroundSide.Alliance, this);
		}

		protected virtual BattlegroundStats CreateStats()
		{
			return new BattlegroundStats();
		}

		/// <summary>
		/// Override this to give the players mark of honors and whatnot.
		/// Note: Usually should trigger  a spell on the characters
		/// e.g. SpellId.CreateWarsongMarkOfHonorWInner
		/// </summary>
		protected virtual void RewardPlayers()
		{
			return;
		}

		public override void TeleportInside(Character chr)
		{
			var invite = chr.Battlegrounds.Invitation;
			BattlegroundTeam team;

			if (invite != null)
			{
				team = invite.Team;
			}
			else
			{
				// joins without invitiation
				team = GetTeam(chr.FactionGroup.GetBattlegroundSide());
			}

			chr.TeleportTo(this, team.StartPosition, team.StartOrientation);
		}

		public override void TeleportOutside(Character chr)
		{
			chr.Battlegrounds.TeleportBack();
		}

		protected override void OnEnter(Character chr)
		{
			base.OnEnter(chr);

			var invitation = chr.Battlegrounds.Invitation;

			if (invitation == null)
			{
				// don't join a team if not invited
				return;
			}

			var team = invitation.Team;
			if (team.Battleground != this)
			{
				// decided for another BG in the meantime
				team.Battleground.TeleportInside(chr);
				return;
			}

			// stop cancel timer
			chr.RemoveUpdateAction(invitation.CancelTimer);

			// join team
			JoinTeam(chr, team);

			BattlegroundHandler.SendStatusActive(chr, this, invitation.QueueIndex);
		}

		protected void JoinTeam(Character chr, BattlegroundTeam team)
		{
			if (chr.Battlegrounds.Team != null)
			{
				chr.Battlegrounds.Team.RemoveMember(chr);
			}
			chr.Battlegrounds.Stats = CreateStats();
			team.AddMember(chr);

			if (_status == BattlegroundStatus.None &&
			   PlayerCount >= (Template.MapTemplate.MaxPlayerCount * StartPlayerPct) / 100)
			{
				StartPreparation();
			}
		}

		/// <summary>
		/// Is called when a Character logs back in
		/// </summary>
		internal protected virtual bool LogBackIn(Character chr)
		{
			var side = chr.Record.BattlegroundTeam;
			var isStaff = chr.Role.IsStaff;
			if (side == BattlegroundSide.End ||
				(!isStaff && GetTeam(side).IsFull))
			{
				if (!isStaff)
				{
					// may not stay
					TeleportOutside(chr);
					return false;
				}
			}
			else
			{
				// make sure, Character will be added back to the Team
				var queue = InstanceQueue.GetTeamQueue(side);
				var index = chr.Battlegrounds.AddRelation(new BattlegroundRelation(queue, chr, false));
				chr.Battlegrounds.Invitation = new BattlegroundInvitation(GetTeam(side), index);
			}
			return true;
		}

		protected override void OnLeave(Character chr)
		{
			base.OnLeave(chr);

			var team = chr.Battlegrounds.Team;
			if (team != null)
			{
				if (chr.Battlegrounds.Invitation != null)
				{
					// remove BG status
					chr.Battlegrounds.RemoveRelation(Template.Id);
				}

				// remove from Team
				team.RemoveMember(chr);

				// add players who are waiting in Queue
				ProcessPendingPlayers(team);

				if (IsActive && !chr.Role.IsStaff)
				{
					// flag as deserter
					chr.Auras.CreateSelf(BattlegroundMgr.DeserterSpell, false);
				}

				// check if the BG is too empty to continue
				CheckShutdown();
			}
		}

		#endregion
	}
}