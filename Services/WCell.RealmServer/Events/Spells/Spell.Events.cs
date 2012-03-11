using System;
using WCell.Constants.Spells;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells
{
    public partial class Spell
    {
        /// <summary>
        /// Is called after all preparations have been made and the Spell is about to start casting.
        /// Return anything but <c>SpellFailedReason.None</c> to cancel casting.
        /// </summary>
        public event Func<SpellCast, SpellFailedReason> Casting;

        /// <summary>
        /// Is called before SpellCast is cancelled for the given reason.
        /// </summary>
        public event Action<SpellCast, SpellFailedReason> Cancelling;

        /// <summary>
        /// Is called after a SpellCast has been casted.
        /// </summary>
        public event Action<SpellCast> Casted;

        /// <summary>
        /// Is called before SpellCast is cancelled for the given reason.
        /// </summary>
        public event Action<Aura> AuraRemoved;

        /// <summary>
        /// Triggers the Casting event
        /// </summary>
        internal SpellFailedReason NotifyCasting(SpellCast cast)
        {
            var evt = Casting;
            if (evt != null)
            {
                var err = evt(cast);
                if (err != SpellFailedReason.Ok)
                {
                    cast.Cancel(err);
                    return err;
                }
            }
            return SpellFailedReason.Ok;
        }

        internal void NotifyCancelled(SpellCast cast, SpellFailedReason reason)
        {
            var evt = Cancelling;
            if (evt != null)
            {
                evt(cast, reason);
            }
        }

        internal void NotifyCasted(SpellCast cast)
        {
            var evt = Casted;
            if (evt != null)
            {
                evt(cast);
            }
        }

        internal void NotifyAuraRemoved(Aura aura)
        {
            var evt = AuraRemoved;
            if (evt != null)
            {
                evt(aura);
            }
        }

        /// <summary>
        /// Will let the Caster play the given text and sound after casting
        /// </summary>
        public void AddTextAndSoundEvent(NPCAiText text)
        {
            if (text != null)
            {
                Casted += cast => cast.CasterObject.PlayTextAndSound(text);
            }
        }
    }
}
