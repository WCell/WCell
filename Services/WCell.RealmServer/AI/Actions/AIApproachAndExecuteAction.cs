namespace WCell.RealmServer.AI.Actions
{
    /// <summary>
    /// A complex action consisting of approaching to the target of AITargetedAction and then executing
    /// the AITargetedAction
    /// </summary>
    //public class AIApproachAndExecuteAction : AICompositeAction
    //{
    //    protected AITargetedAction m_action;

    //    public AIApproachAndExecuteAction(Unit owner, AITargetedAction action) : base(owner)
    //    {
    //        m_action = action;
    //    }

    //    public override AIActionResult Start()
    //    {
    //        var target = m_action.Target;

    //        if (!IsCloseEnough(target, m_owner))
    //        {
    //            AIAction approachAction = new AITargetMoveAction(m_owner)
    //            {
    //                Range = new SimpleRange(m_action.Range.MinRange, m_action.Range.MaxRange)
    //            };

    //            m_actions = new[] { approachAction, m_action };
    //        }
    //        else
    //        {
    //            m_actions = new[] { m_action };
    //        }

    //        return base.Start();
    //    }

    //    private bool IsCloseEnough(Unit target, Unit owner)
    //    {
    //        if (target != null && target != owner)
    //        {
    //            var distanceSq = owner.GetDistanceToUnitSq(target);

    //            if (distanceSq < (m_action.Range.MinRange * m_action.Range.MinRange) || (distanceSq > m_action.Range.MaxRange * m_action.Range.MaxRange))
    //                return true;
    //        }

    //        return false;
    //    }
    //}
}