using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// An interface for a repeating timer with ticks (that maybe controls other timers)
    /// </summary>
    public interface ITickTimer
    {
        /// <summary>
        /// The total amount of remaining milliseconds
        /// </summary>
        int TimeLeft
        {
            get;
            //set;
        }

        /// <summary>
        /// The Environment.TickCount of the next Tick
        /// </summary>
        //int NextTick
        //{
        //    get;
        //}

        /// <summary>
        /// The Environment.TickCount of the last Tick (Timer will close afterwards)
        /// </summary>
        int Until
        {
            get;
            //set;
        }

        /// <summary>
        /// The average amplitude between ticks (in ms)
        /// </summary>
        int AuraPeriod
        {
            get;
        }

        /// <summary>
        /// The total duration of this Timer (in ms)
        /// </summary>
        int Duration
        {
            get;
        }

        /// <summary>
        /// The amount of ticks that already passed
        /// </summary>
        int Ticks
        {
            get;
            //set;
        }

        /// <summary>
        /// The maximum amount of ticks
        /// </summary>
        int MaxTicks
        {
            get;
            //set;
        }

		/// <summary>
		/// Called when the given Aura is removed from the given owner
		/// </summary>
        void OnRemove(Unit owner, Aura aura);

        void Cancel();
    }
}