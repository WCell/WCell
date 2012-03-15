namespace WCell.RealmServer.AI.Actions
{
    /// <summary>
    /// All possible types of AI actions
    /// AIActionType is stored by AIActionCollection rather than by AIAction itself
    /// </summary>
    //public enum AIActionType
    //{
    //    None,

    //    MoveToTarget,
    //    MoveToPoint,
    //    Roam,
    //    Retreat,
    //    Follow,

    //    OutOfCombatBuff,
    //    OutOfCombatDecoration,
    //    OutOfCombatWander,

    //    InCombatBuff,
    //    InCombatDebuff,
    //    InCombatDecoration,

    //    InCombatRunAway,

    //    InCombatRangedWhiteDamage,
    //    InCombatMeleeWhiteDamage,
    //    InCombatRangedSpellDamage,
    //    InCombatMeleeSpellDamage,

    //    InCombatAOE,

    //    HealSelf,
    //    HealAlly
    //}

    /// <summary>
    /// All possible outcomes of AI Action.
    /// Some actions will never succeed (eg following a moving Target),
    /// some cannot even be executed (eg follow Master without a Master)
    /// </summary>
    public enum AIActionResult
    {
        /// <summary>
        /// The action has been successfully executed
        /// </summary>
        Success,

        /// <summary>
        /// The action is in execution, needs an update
        /// </summary>
        Executing,

        /// <summary>
        /// The action cannot be executed.
        /// Indicates to the Brain that it should resume DefaultAction
        /// </summary>
        Failure
    }
}