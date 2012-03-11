using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI
{
    public interface IAICombatEventHandler
    {
        /// <summary>
        /// Called on entering combat
        /// </summary>
        void OnEnterCombat();

        /// <summary>
        /// Called on Brain's owner taking damage
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="cast">instance of SpellCast (null for physical damage)</param>
        void OnDebuff(Unit caster, SpellCast cast, Aura debuff);

        /// <summary>
        /// Called on leaving combat
        /// </summary>
        void OnLeaveCombat();

        void OnDamageReceived(IDamageAction action);

        void OnDamageDealt(IDamageAction action);

        /// <summary>
        /// Called when the given healed Unit was healed by the given healer
        /// </summary>
        /// <param name="healer"></param>
        /// <param name="healed"></param>
        /// <param name="amtHealed"></param>
        void OnHeal(Unit healer, Unit healed, int amtHealed);

        /// <summary>
        /// Called when any Unit in visible range has been killed
        /// </summary>
        /// <param name="killerUnit">killer unit</param>
        /// <param name="victimUnit">killed unit</param>
        void OnKilled(Unit killerUnit, Unit victimUnit);

        /// <summary>
        /// Called when the Owner is killed
        /// </summary>
        void OnDeath();

        /// <summary>
        /// Called when receiver is out of range for a strike in combat
        /// </summary>
        void OnCombatTargetOutOfRange();

        /// <summary>
        /// Called when the receiver's enters the world or gets resurrected
        /// </summary>
        void OnActivate();
    }
}