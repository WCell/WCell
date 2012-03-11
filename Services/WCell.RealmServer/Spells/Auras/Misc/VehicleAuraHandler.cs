using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs.Vehicles;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class VehicleAuraHandler : AuraEffectHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        private Unit Caster;
        private Vehicle Vehicle;
        private VehicleSeat Seat;

        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
            Caster = casterReference.Object as Unit;
            if (Caster == null || Caster is Vehicle)
            {
                log.Warn("Invalid SpellCaster \"{0}\" for Spell: {1}", Caster, SpellEffect.Spell);
                failReason = SpellFailedReason.Error;
                return;
            }

            Vehicle = target as Vehicle;
            if (Vehicle == null)
            {
                failReason = SpellFailedReason.BadTargets;
            }
            else
            {
                Seat = Vehicle.GetSeatFor(Caster);
                if (Seat == null)
                {
                    // must never happen since Vehicle is unclickable when full
                    failReason = SpellFailedReason.BadTargets;
                }
            }
        }

        protected override void Apply()
        {
            Seat.Enter(Caster);
        }

        protected override void Remove(bool cancelled)
        {
            if (Caster.IsInWorld && Seat.Passenger == Caster)
            {
                Seat.ClearSeat();
            }
        }
    }
}