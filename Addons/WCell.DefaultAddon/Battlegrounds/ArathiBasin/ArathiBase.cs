using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Addons.Default.Battlegrounds.AlteracValley;
using WCell.RealmServer.Entities;
using WCell.Constants;
using WCell.RealmServer.Battlegrounds;
using WCell.Core.Timers;
using WCell.RealmServer.GameObjects;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    public abstract class ArathiBase
    {
        // TODO: Spawn a different flag (GO) whenever a state changes 
        // TODO: (horde capped, challenged, alliance capped, neutral)
        // TODO: Figure out score appending
        // TODO: (Tick length and score/tick changes according to the number of bases capped)

        public static uint ABConversionTime = 20;

        #region Events

        public event BaseHandler BaseChallenged;

        public event BaseHandler CaptureInterrupted;

        public event BaseHandler BaseCaptured;

        #endregion

        #region Fields
        private BattlegroundSide _side = BattlegroundSide.End;

        public GOEntry FlagStand;

        /// <summary>
        /// The character currently capturing the flag.
        /// </summary>
        public Character Capturer;
        public uint Score;
        public ArathiBasin Instance;

        //Whether or not the flag is being captured.
        public bool Challenged;
        public bool GivesScore;

        public TimerEntry StartScoreTimer = new TimerEntry();
        public TimerEntry CaptureTimer = new TimerEntry();
        #endregion

        protected ArathiBase(ArathiBasin instance, GOEntry flagstand)
        {
            Instance = instance;
            FlagStand = flagstand;

            Instance.RegisterUpdatableLater(StartScoreTimer);
            Instance.RegisterUpdatableLater(CaptureTimer);
        }

        public abstract string BaseName
        {
            get;
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
        /// TODO: Spawn the neutral flag
        /// Begins the capturing process. A base will turn if not taken back
        /// </summary>
        /// <param name="chr"></param>
        public void BeginCapture(Character chr)
        {
            Capturer = chr;
            Challenged = true;

            var evt = BaseChallenged;
            if (evt != null)
            {
                evt(chr);
            }

            CaptureTimer.Start(0, ABConversionTime * 1000, (i) =>
                                                 {
                                                     Capture(); 
                                                     CaptureTimer.Stop();
                                                 });
        }

        /// <summary>
        /// Call to interrupt the capturing process
        /// </summary>
        /// <param name="chr">The interrupting character</param>
        public void InterruptCapture(Character chr)
        {
            Capturer = null;
            Challenged = false;

            var stats = (ArathiStats)chr.Battlegrounds.Stats;
            stats.BasesDefended++;

            var evt = CaptureInterrupted;
            if (evt != null)
            {
                evt(chr);
            }

            CaptureTimer.Stop();
            StartScoreTimer.Stop();
        }

        /// <summary>
        /// TODO: Spawn the side's flag
        /// Finalizes a capture (Flag changes colour (de/respawns, casts spells, etc)
        /// </summary>
        public void Capture()
        {
            var stats = (ArathiStats)Capturer.Battlegrounds.Stats;
            stats.BasesAssaulted++;

            if (Capturer.Battlegrounds.Team.Side == BattlegroundSide.Horde)
            {
                Instance.HordeBaseCount++;
            }
            else
            {
                Instance.AllianceBaseCount++;
            }

            var evt = BaseCaptured;
            if (evt != null)
            {
                evt(Capturer);
            }

            // It takes a few minutes before a captured flag begins to give score.
            StartScoreTimer.Start(0, 2 * 1000 * 60, (i) =>
                                                        {
                                                            GivesScore = true;
                                                            StartScoreTimer.Stop();
                                                        });
        }


        public void RegisterFlagstand()
        {
            FlagStand.Used += (go, chr) =>
            {
                if(Challenged)
                {
                    InterruptCapture(chr);
                    return true;
                }

                BeginCapture(chr);
                return true;
            };
        }

        
        public void Destroy()
        {
            Capturer = null;
            Instance = null;
            FlagStand = null;
        }
    }
}
