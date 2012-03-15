using System;

namespace WCell.RealmServer.Spells
{
    public partial class SpellChannel
    {
        /// <summary>
        /// Is called on every SpellChannel tick.
        /// </summary>
        public static event Action<SpellChannel> Ticked;
    }
}