using WCell.Constants;
using WCell.Constants.Battlegrounds;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.Addons.Default.Lang;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Lang;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.Util.Variables;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
	public abstract class ArathiBase
	{
		/// <summary>
		/// Time to convert a cap point in millis
		/// </summary>
		[Variable("ABCapturePointConversionDelayMillis")]
		public static int CapturePointConversionDelayMillis = 60000;

		/// <summary>
		/// Time until score starts rolling in, after capturing
		/// </summary>
		[Variable("ABScoreDelayMillis")]
		public static int ScoreDelayMillis = 2*1000*60;

		#region Events

		public event BaseHandler BaseChallenged;

		public event BaseHandler CaptureInterrupted;

		public event BaseHandler BaseCaptured;

		#endregion

		#region Fields
		private BattlegroundSide _side = BattlegroundSide.End;
        private BaseState _state = BaseState.Neutral;

		public GameObject FlagStand;
        public GameObject ActualAura;

        protected GOSpawnEntry neutralBannerSpawn;
        protected GOSpawnEntry neutralAuraSpawn;

        protected GOSpawnEntry allianceBannerSpawn;
        protected GOSpawnEntry allianceAuraSpawn;

        protected GOSpawnEntry hordeBannerSpawn;
        protected GOSpawnEntry hordeAuraSpawn;

        protected GOSpawnEntry allianceAttackBannerSpawn;
        protected GOSpawnEntry hordeAttackBannerSpawn;

        protected WorldStateId showIconNeutral;
        protected WorldStateId showIconAllianceControlled;
        protected WorldStateId showIconAllianceContested;
        protected WorldStateId showIconHordeControlled;
        protected WorldStateId showIconHordeContested;

		/// <summary>
		/// The character currently capturing the flag.
		/// </summary>
		public Character Capturer;
		public ArathiBasin Instance;

		public bool GivesScore;

		public TimerEntry StartScoreTimer;
		public TimerEntry CaptureTimer;
		#endregion

		protected ArathiBase(ArathiBasin instance)
		{
			Instance = instance;

			// init timers
			CaptureTimer = new TimerEntry(dt =>
			{
				Capture();
			});

			StartScoreTimer = new TimerEntry(dt =>
			{
				GivesScore = true;
			});

			Instance.RegisterUpdatableLater(StartScoreTimer);
			Instance.RegisterUpdatableLater(CaptureTimer);

            Names = new string[(int)ClientLocale.End];
            AddSpawns();
			SpawnNeutral();
		}

		public abstract string BaseName
		{
			get;
		}

        public virtual string[] Names
        {
            get;
            protected set;
        }

		/// <summary>
		/// The side currently in control of this base.
		/// If End, base is neutral.
		/// </summary>
		public BattlegroundSide BaseOwner
		{
			get { return _side; }
			set { _side = value; }
		}

        /// <summary>
        /// The state currently of this base
        /// </summary>
        public BaseState State
        {
            get { return _state; }
            set { _state = value; }
        }

		/// <summary>
		/// Begins the capturing process. A base will turn if not taken back
		/// </summary>
		/// <param name="chr"></param>
		public void BeginCapture(Character chr)
        {
            Capturer = chr;

			CaptureTimer.Start(CapturePointConversionDelayMillis, 0);
            SpawnContested();
            
            ABSounds sound;
            if (_side == BattlegroundSide.End)
            {
                sound = ABSounds.NodeContested;
                Instance.WorldStates.SetInt32(showIconNeutral, 0);
            }
            else if (_side == BattlegroundSide.Alliance)
            {
                sound = ABSounds.NodeAssaultedHorde;
            }
            else
            {
                sound = ABSounds.NodeAssaultedAlliance;
            }

            if (chr.Battlegrounds.Team.Side == BattlegroundSide.Alliance)
            {
                State = BaseState.ContestedAlliance;
                Instance.WorldStates.SetInt32(showIconAllianceContested, 1);
                Instance.WorldStates.SetInt32(showIconHordeControlled, 0);
            }
            else
            {
                State = BaseState.ContestedHorde;
                Instance.WorldStates.SetInt32(showIconHordeContested, 1);
                Instance.WorldStates.SetInt32(showIconAllianceControlled, 0);
            }

            var time = RealmLocalizer.FormatTimeSecondsMinutes(CapturePointConversionDelayMillis / 1000);
            foreach (Character character in Instance.Characters)
            {
                character.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(character.Locale, AddonMsgKey.ABBaseClaimed, chr.Name, Names[(int)character.Locale], chr.Battlegrounds.Team.Side, time));
            }

            MiscHandler.SendPlaySoundToMap(Instance, (uint)sound);

            var evt = BaseChallenged;
            if (evt != null)
            {
                evt(chr);
            }
        }

		/// <summary>
		/// Call to interrupt the capturing process
		/// </summary>
		/// <param name="chr">The interrupting character</param>
		public void InterruptCapture(Character chr)
		{
			Capturer = null;

			var stats = (ArathiStats)chr.Battlegrounds.Stats;
			stats.BasesDefended++;

			CaptureTimer.Stop();

            if (BaseOwner == BattlegroundSide.Horde)
            {
                SpawnHorde();
                State = BaseState.CapturedHorde;
                Instance.WorldStates.SetInt32(showIconHordeControlled, 1);
                Instance.WorldStates.SetInt32(showIconAllianceContested, 0);
            }
            else
            {
                SpawnAlliance();
                State = BaseState.CapturedAlliance;
                Instance.WorldStates.SetInt32(showIconAllianceControlled, 1);
                Instance.WorldStates.SetInt32(showIconHordeContested, 0);
            }

            foreach (Character character in Instance.Characters)
            {
                character.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(character.Locale, AddonMsgKey.ABBaseDefended, chr.Name, Names[(int)character.Locale]));
            }

			var evt = CaptureInterrupted;
			if (evt != null)
			{
				evt(chr);
			}
		}

        /// <summary>
        /// Call to interrupt the capturing process and begin a new capturing process
        /// </summary>
        public void ChangeCapture(Character chr)
        {
            Capturer = chr;
            CaptureTimer.Stop();
            StartScoreTimer.Stop();

            CaptureTimer.Start(CapturePointConversionDelayMillis, 0);
            if (chr.Battlegrounds.Team.Side == BattlegroundSide.Alliance)
            {
                State = BaseState.ContestedAlliance;
                Instance.WorldStates.SetInt32(showIconAllianceContested, 1);
                Instance.WorldStates.SetInt32(showIconHordeContested, 0);
            }
            else
            {
                State = BaseState.ContestedHorde;
                Instance.WorldStates.SetInt32(showIconHordeContested, 1);
                Instance.WorldStates.SetInt32(showIconAllianceContested, 0);
            }

            foreach (Character character in Instance.Characters)
            {
                character.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(character.Locale, AddonMsgKey.ABBaseAssaulted, chr.Name, Names[(int)character.Locale]));
            }

            SpawnContested();
        }
		/// <summary>
		/// Finalizes a capture (Flag changes colour (de/respawns, casts spells, etc)
		/// </summary>
		public void Capture()
		{
			var stats = (ArathiStats)Capturer.Battlegrounds.Stats;
			stats.BasesAssaulted++;

            FlagStand.Delete();
            ActualAura.Delete();

            ABSounds sound;
			if (Capturer.Battlegrounds.Team.Side == BattlegroundSide.Horde)
			{
				BaseOwner = BattlegroundSide.Horde;
                State = BaseState.CapturedHorde;
                SpawnHorde();
                sound = ABSounds.NodeCapturedHorde;
                Instance.WorldStates.SetInt32(showIconHordeControlled, 1);
                Instance.WorldStates.SetInt32(showIconHordeContested, 0);
			}
			else
			{
				BaseOwner = BattlegroundSide.Alliance;
                State = BaseState.CapturedAlliance;
                SpawnAlliance();
                sound = ABSounds.NodeCapturedAlliance;
                Instance.WorldStates.SetInt32(showIconAllianceControlled, 1);
                Instance.WorldStates.SetInt32(showIconAllianceContested, 0);
			}

			// It takes a few minutes before a captured flag begins to give score.
			StartScoreTimer.Start(ScoreDelayMillis, 0);

            foreach (Character character in Instance.Characters)
            {
                character.SendSystemMessage(DefaultAddonLocalizer.Instance.Translate(character.Locale, AddonMsgKey.ABBaseTaken, BaseOwner, Names[(int)character.Locale]));
            }

            MiscHandler.SendPlaySoundToMap(Instance, (uint)sound);
			var evt = BaseCaptured;
			if (evt != null)
			{
				evt(Capturer);
			}
		}

		public void PlayerClickedOnFlagstand(GameObject go, Character chr)
        {
                FlagStand.Delete();
                ActualAura.Delete();

                if (_state == BaseState.Neutral || _state == BaseState.CapturedAlliance || _state == BaseState.CapturedHorde)
                {
                    BeginCapture(chr);
                }
                else
                {
                    if (_side == BattlegroundSide.End)
                        ChangeCapture(chr);
                    else
                        InterruptCapture(chr);
                }
		}

        protected virtual void AddSpawns()
        {
        }

        /// <summary>
        /// Spawn neutral flag (use only at the beginning)
        /// </summary>
		protected void SpawnNeutral()
        {
            FlagStand = neutralBannerSpawn.Spawn(Instance);
            ActualAura = neutralAuraSpawn.Spawn(Instance);
        }

        /// <summary>
        /// Spawn Horde flag (use only when CaptureTimer = 0)
        /// </summary>
		protected void SpawnHorde()
		{
            FlagStand = hordeBannerSpawn.Spawn(Instance);
            ActualAura = hordeAuraSpawn.Spawn(Instance);
		}

        /// <summary>
        /// Spawn Alliance flag (use only when CaptureTimer = 0)
        /// </summary>
		protected void SpawnAlliance()
		{
            FlagStand = allianceBannerSpawn.Spawn(Instance);
            ActualAura = allianceAuraSpawn.Spawn(Instance);
		}

        /// <summary>
        /// Spawn contested flag according to the team which attacks the base
        /// </summary>
        protected void SpawnContested()
        {
            if (Capturer.Battlegrounds.Team.Side == BattlegroundSide.Alliance)
                FlagStand = allianceAttackBannerSpawn.Spawn(Instance);
            else
                FlagStand = hordeAttackBannerSpawn.Spawn(Instance);

            ActualAura = neutralAuraSpawn.Spawn(Instance);
        }

		public void Destroy()
		{
			Capturer = null;
			Instance = null;
            FlagStand.Delete();
            FlagStand.Dispose();
            ActualAura.Delete();
            ActualAura.Dispose();
		}
	}
}