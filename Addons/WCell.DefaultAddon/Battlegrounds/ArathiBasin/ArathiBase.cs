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
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Battlegrounds.ArathiBasin
{
    public abstract class ArathiBase
    {
        // TODO: Spawn a flag from one template from each GOEntry
        // TODO: Check GO Use registering

        public static uint ABConversionTime = 20;

        #region Events

        public event BaseHandler BaseChallenged;

        public event BaseHandler CaptureInterrupted;

        public event BaseHandler BaseCaptured;

        #endregion

        #region Fields
        private BattlegroundSide _side = BattlegroundSide.End;

        public GameObject FlagStand;
        public GOEntry FlagStandNeutral;
        public GOEntry FlagStandHorde;
        public GOEntry FlagStandAlliance;
        
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
            FlagStandNeutral = flagstand;

            Instance.RegisterUpdatableLater(StartScoreTimer);
            Instance.RegisterUpdatableLater(CaptureTimer);

            SpawnNeutral();
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

            CaptureTimer.Start(0, ABConversionTime * 1000, (i) =>
                                                 {
                                                     Capture(); 
                                                     CaptureTimer.Stop();
                                                 });
            var evt = BaseChallenged;
            if (evt != null)
            {
                evt(chr);
            }
        }

        /// <summary>
        /// Call to interrupt the capturing process
        /// TODO: Change the flag back to the owner's
        /// </summary>
        /// <param name="chr">The interrupting character</param>
        public void InterruptCapture(Character chr)
        {
            Capturer = null;
            Challenged = false;

            var stats = (ArathiStats)chr.Battlegrounds.Stats;
            stats.BasesDefended++;

            CaptureTimer.Stop();
            StartScoreTimer.Stop();

            var evt = CaptureInterrupted;
            if (evt != null)
            {
                evt(chr);
            }
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
                BaseOwner = BattlegroundSide.Horde;
            }
            else
            {
                Instance.AllianceBaseCount++;
                BaseOwner = BattlegroundSide.Alliance;
            }

            // It takes a few minutes before a captured flag begins to give score.
            StartScoreTimer.Start(0, 2 * 1000 * 60, (i) =>
                                                        {
                                                            GivesScore = true;
                                                            StartScoreTimer.Stop();
                                                        });
            var evt = BaseCaptured;
            if (evt != null)
            {
                evt(Capturer);
            }
        }


        public void RegisterFlagstand(GOTemplate template)
        {
            template.Entry.Used += (go, chr) =>
            {
                if (go == FlagStand)
                {
                    if (Challenged)
                    {
                        InterruptCapture(chr);
                        return true;
                    }

                    BeginCapture(chr);
                    return true;
                }
                return false;
            };
        }

        private void SpawnNeutral()
        {
            
        }

        private void SpawnHorde()
        {
            
        }

        private void SpawnAlliance()
        {

        }
        
        public void Destroy()
        {
            Capturer = null;
            Instance = null;
        }
    }
}
