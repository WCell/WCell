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

        #region Events

        public event BaseHandler BaseCapture;

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

        #endregion

        protected ArathiBase(ArathiBasin instance, GOEntry flagstand)
        {
            Instance = instance;
            FlagStand = flagstand;
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
        /// Begins the capturing process.
        /// </summary>
        /// <param name="chr"></param>
        public void BeginCapture(Character chr)
        {
            Capturer = chr;
            Challenged = true;

            var evt = BaseCapture;
            if (evt != null)
            {
                evt(chr);
            }
        }

        /// <summary>
        /// Call to interrupt the capturing process
        /// </summary>
        public void InterruptCapture(Character chr)
        {
            Capturer = null;
            Challenged = false;

            var evt = CaptureInterrupted;
            if (evt != null)
            {
                evt(chr);
            }
        }

        /// <summary>
        /// Captures a base. (Changes side, casts spell, etc)
        /// </summary>
        public void Capture()
        {
            var evt = BaseCaptured;
            if (evt != null)
            {
                evt(Capturer);
            }
        }

        ///// <summary>
        ///// Call periodically to give score to the current owner.
        ///// </summary>
        //public void UpdateBase()
        //{
        //    if (BaseOwner == BattlegroundSide.End)
        //    {
        //        return;
        //    }

        //    if (BaseOwner == BattlegroundSide.Alliance)
        //    {
        //        Instance.AllianceScore++;
        //        return;
        //    }
        //    Instance.HordeScore++;
        //}

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
