using System;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions
{
    /// <summary>
    /// Abstract atomary action of AI
    /// </summary>
    public abstract class AIAction : IAIAction
    {
        protected Unit m_owner;

        protected AIAction(Unit owner)
        {
            m_owner = owner;
        }

        protected AIAction()
        {
        }

        /// <summary>
        /// Owner of the action
        /// </summary>
        public Unit Owner
        {
            get { return m_owner; }
        }

        public bool UsesSpells
        {
            get { return m_owner.HasSpells; }
        }

        public bool HasSpellReady
        {
            get { return ((NPC)m_owner).NPCSpells.ReadyCount > 0; }
        }

        public virtual bool IsGroupAction
        {
            get { return false; }
        }

        /// <summary>
        /// What can break the action.
        /// </summary>
        public virtual ProcTriggerFlags InterruptFlags
        {
            get { return ProcTriggerFlags.None; }
        }

        /// <summary>
        /// Start a new Action
        /// </summary>
        /// <returns></returns>
        public abstract void Start();

        /// <summary>
        /// Update
        /// </summary>
        /// <returns></returns>
        public abstract void Update();

        /// <summary>
        /// Stop (usually called before switching to another Action)
        /// </summary>
        /// <returns></returns>
        public abstract void Stop();

        public abstract UpdatePriority Priority
        {
            get;
        }

        public virtual void Dispose()
        {
        }
    }

    public interface IAIAction : IDisposable
    {
        Unit Owner
        {
            get;
        }

        UpdatePriority Priority
        {
            get;
        }

        bool IsGroupAction { get; }

        ProcTriggerFlags InterruptFlags
        {
            get;
        }

        /// <summary>
        /// Start executing current action
        /// </summary>
        /// <returns></returns>
        void Start();

        /// <summary>
        /// Updates current action
        /// </summary>
        /// <returns></returns>
        void Update();

        /// <summary>
        /// Stops this Action
        /// </summary>
        /// <returns></returns>
        void Stop();
    }
}