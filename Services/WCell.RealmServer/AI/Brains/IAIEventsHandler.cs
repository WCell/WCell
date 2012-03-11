using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI.Brains
{
    public interface IAIEventsHandler
    {
        void OnEnterCombat();

        void OnDamageTaken(Misc.IDamageAction action);

        void OnDebuff(Unit attackingUnit, SpellCast cast, Aura debuff);

        void OnDamageDealt(Misc.IDamageAction action);

        void OnLeaveCombat();

        void OnKilled(Unit killerUnit, Unit victimUnit);

        void OnDeath();

        void OnSpawn();

        void OnCombatTargetOutOfRange();

        void OnHeal(Unit healer, Unit healed, int amtHealed);
    }
}