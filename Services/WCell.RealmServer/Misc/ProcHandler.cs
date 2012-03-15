using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WeakRef = WCell.Util.WeakReference<WCell.RealmServer.Entities.Unit>;

namespace WCell.RealmServer.Misc
{
    public delegate bool ProcValidator(Unit triggerer, IUnitAction action);
    public delegate bool ProcCallback(Unit owner, Unit triggerer, IUnitAction action);

    /// <summary>
    /// Customizable ProcHandler
    /// </summary>
    public interface IProcHandler : IDisposable
    {
        /// <summary>
        /// The one who the proc handler is applied to
        /// </summary>
        Unit Owner
        {
            get;
        }

        ProcTriggerFlags ProcTriggerFlags
        {
            get;
        }

        ProcHitFlags ProcHitFlags
        {
            get;
        }

        /// <summary>
        /// Probability to proc in percent (0-100)
        /// </summary>
        uint ProcChance { get; }

        /// <summary>
        /// The Spell to be triggered (if any)
        /// </summary>
        Spell ProcSpell { get; }

        int StackCount { get; }

        int MinProcDelay
        {
            get;
        }

        /// <summary>
        /// Time when this proc may be triggered again (or small value, if always)
        /// </summary>
        DateTime NextProcTime
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this handler can trigger the given Proc
        /// </summary>
        /// <param name="target"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active);

        void TriggerProc(Unit triggerer, IUnitAction action);
    }

    /// <summary>
    /// Default implementation for IProcHandler
    /// </summary>
    public class ProcHandler : IProcHandler
    {
        public static ProcValidator DodgeBlockOrParryValidator = (target, action) =>
        {
            var aaction = action as DamageAction;
            if (aaction == null)
            {
                return false;
            }

            return aaction.VictimState == VictimState.Dodge ||
                aaction.VictimState == VictimState.Parry ||
                aaction.Blocked > 0;
        };

        public static ProcValidator DodgeValidator = (target, action) =>
        {
            var aaction = action as DamageAction;
            if (aaction == null)
            {
                return false;
            }

            return aaction.VictimState == VictimState.Dodge;
        };

        public static ProcValidator StunValidator = (target, action) =>
        {
            var aaction = action as DamageAction;
            if (aaction == null || aaction.Spell == null ||
                !aaction.Spell.IsAura || !action.Attacker.MayAttack(action.Victim))
            {
                return false;
            }

            //var stunEffect = aaction.Spell.GetEffectsWhere(effect => effect.AuraType == AuraType.ModStun);
            return aaction.Spell.Attributes.HasAnyFlag(SpellAttributes.MovementImpairing);
        };

        public readonly WeakRef CreatorRef;
        public readonly ProcHandlerTemplate Template;
        private int m_stackCount;

        public ProcHandler(Unit creator, Unit owner, ProcHandlerTemplate template)
        {
            CreatorRef = new WeakRef(creator);
            Owner = owner;
            Template = template;
            m_stackCount = template.StackCount;
        }

        public Unit Owner
        {
            get;
            private set;
        }

        /// <summary>
        /// The amount of times that this Aura has been applied
        /// </summary>
        public int StackCount
        {
            get { return m_stackCount; }
            set { m_stackCount = value; }
        }

        public ProcTriggerFlags ProcTriggerFlags
        {
            get { return Template.ProcTriggerFlags; }
        }

        public ProcHitFlags ProcHitFlags
        {
            get { return Template.ProcHitFlags; }
        }

        public Spell ProcSpell
        {
            get { return null; }
        }

        /// <summary>
        /// Chance to proc in %
        /// </summary>
        public uint ProcChance
        {
            get { return Template.ProcChance; }
        }

        public int MinProcDelay
        {
            get { return Template.MinProcDelay; }
        }

        public DateTime NextProcTime
        {
            get;
            set;
        }

        /// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
        public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
        {
            return Template.Validator == null || Template.Validator(triggerer, action);
        }

        public void TriggerProc(Unit triggerer, IUnitAction action)
        {
            if (!CreatorRef.IsAlive)
            {
                Dispose();
                return;
            }

            var proced = Template.ProcAction(CreatorRef, triggerer, action);

            // consume a charge
            if (proced && m_stackCount > 0)
            {
                m_stackCount--;
            }
        }

        public void Dispose()
        {
            Owner.RemoveProcHandler(this);
        }
    }

    /// <summary>
    /// Default implementation for IProcHandler
    /// </summary>
    public class ProcHandlerTemplate
    {
        protected int m_stackCount;

        protected ProcHandlerTemplate()
        {
        }

        public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction, uint procChance = 100u, int stackCount = 0)
        {
            ProcTriggerFlags = triggerFlags;
            ProcHitFlags = hitFlags;
            ProcChance = procChance;
            Validator = null;
            ProcAction = procAction;
            m_stackCount = stackCount;
        }

        public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags, ProcCallback procAction, ProcValidator validator = null, uint procChance = 100u, int stackCount = 0)
        {
            ProcTriggerFlags = triggerFlags;
            ProcHitFlags = hitFlags;
            ProcChance = procChance;
            Validator = validator;
            ProcAction = procAction;
            m_stackCount = stackCount;
        }

        public ProcValidator Validator { get; set; }

        public ProcCallback ProcAction
        {
            get;
            set;
        }

        /// <summary>
        /// The amount of times that this Aura has been applied
        /// </summary>
        public int StackCount
        {
            get { return m_stackCount; }
            set { m_stackCount = value; }
        }

        public ProcTriggerFlags ProcTriggerFlags
        {
            get;
            set;
        }

        public ProcHitFlags ProcHitFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Chance to proc in %
        /// </summary>
        public uint ProcChance
        {
            get;
            set;
        }

        /// <summary>
        /// In Milliseconds
        /// </summary>
        public int MinProcDelay
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Triggers a spell on proc
    /// </summary>
    public class TriggerSpellProcHandlerTemplate : ProcHandlerTemplate
    {
        public Spell Spell { get; set; }

        public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags, ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100u, int stackCount = 0)
            : this(spell, triggerFlags, null, hitFlags, procChance, stackCount)
        {
        }

        public TriggerSpellProcHandlerTemplate(Spell spell, ProcTriggerFlags triggerFlags, ProcValidator validator = null, ProcHitFlags hitFlags = ProcHitFlags.None, uint procChance = 100u, int stackCount = 0)
            : base(triggerFlags, hitFlags, null, validator, procChance, stackCount)
        {
            Spell = spell;
            ProcAction = ProcSpell;
        }

        public bool ProcSpell(Unit creator, Unit triggerer, IUnitAction action)
        {
            //if (triggerer != null)
            SpellCast.ValidateAndTriggerNew(Spell, creator, triggerer, null, null, action);
            return false;
        }
    }
}