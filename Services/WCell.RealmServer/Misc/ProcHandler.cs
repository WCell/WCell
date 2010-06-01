using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.Constants;

namespace WCell.RealmServer.Misc
{
	public delegate bool ProcValidator(Unit triggerer, IUnitAction action);
	public delegate bool ProcCallback(Unit triggerer, IUnitAction action);

	public interface IProcHandler : IDisposable
	{
		ProcTriggerFlags ProcTriggerFlags
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
		public static ProcValidator DodgeBlockOrParryValidator = (Unit target, IUnitAction action) =>
		{
			var aaction = action as AttackAction;
			if (aaction == null)
			{
				return false;
			}
			return aaction.VictimState == VictimState.Dodge ||
				aaction.VictimState == VictimState.Parry ||
				aaction.Blocked > 0;
		};

		public readonly Unit Owner;
		public readonly ProcHandlerTemplate Template;
		private int m_stackCount;

		public ProcHandler(Unit owner, ProcHandlerTemplate template)
		{
			Owner = owner;
			Template = template;
			m_stackCount = template.StackCount;
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
			return Template.IsAttackerTriggerer == active && Template.Validator(triggerer, action);
		}

		public void TriggerProc(Unit triggerer, IUnitAction action)
		{
			var proced = Template.ProcAction(triggerer, action);

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

		public ProcHandlerTemplate(ProcValidator validator, ProcCallback procAction)
			: this(ProcTriggerFlags.All, validator, procAction)
		{
		}

		public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcValidator validator, ProcCallback procAction)
			: this(triggerFlags, validator, procAction, 100)
		{
		}

		public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcValidator validator, ProcCallback procAction, uint procChance)
			: this(triggerFlags, validator, procAction, procChance, 0)
		{
		}

		public ProcHandlerTemplate(ProcTriggerFlags triggerFlags, ProcValidator validator, ProcCallback procAction, uint procChance, int stackCount)
		{
			ProcTriggerFlags = triggerFlags;
			Validator = validator;
			ProcChance = procChance;
			ProcAction = procAction;
			m_stackCount = stackCount;
		}

		public bool IsAttackerTriggerer { get; internal set; }

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
	public class TriggerSpellProcHandler : ProcHandlerTemplate
	{
		public Spell Spell { get; set; }

		public TriggerSpellProcHandler(ProcValidator validator, Spell spell)
		{
			Validator = validator;
			ProcAction = ProcSpell;
			Spell = spell;
		}

		public TriggerSpellProcHandler(ProcTriggerFlags triggerFlags, ProcValidator validator, Spell spell) :
			this(validator, spell)
		{
			ProcTriggerFlags = triggerFlags;
		}

		public TriggerSpellProcHandler(ProcTriggerFlags triggerFlags, ProcValidator validator, Spell spell, uint procChance)
			: this(triggerFlags, validator, spell)
		{
			ProcChance = procChance;
		}

		public TriggerSpellProcHandler(ProcTriggerFlags triggerFlags, ProcValidator validator, Spell spell, uint procChance, int stackCount)
			: this(triggerFlags, validator, spell, procChance)
		{
			StackCount = stackCount;
		}

		public bool ProcSpell(Unit triggerer, IUnitAction action)
		{
			WorldObject caster;
			if (IsAttackerTriggerer)
			{
				caster = action.Attacker;
			}
			else
			{
				caster = action.Victim;
			}

			if (caster != null)
			{
				caster.SpellCast.Trigger(Spell);
			}
			return false;
		}
	}

}
