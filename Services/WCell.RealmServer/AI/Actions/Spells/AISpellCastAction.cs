using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.AI.Actions.Spells
{
	/// <summary>
	/// AI Action for casting a spell
	/// </summary>
	public class AISpellCastAction : AITargetedAction
	{
		protected Spell m_spell;

		public AISpellCastAction(Unit owner, Spell spell)
			: base(owner)
		{
			m_spell = spell;

			//if (m_target != null)
			//{
			//    m_range = new SimpleRange(m_spell.Range.Min, m_spell.Range.Max);
			//}
		}
  
		public override void Start()
		{
			var spellCast = m_owner.SpellCast;
			spellCast.Start(m_spell, false);
		}

		public override void Update()
		{
			var spellCast = m_owner.SpellCast;

			if (!spellCast.IsCasting)
			{
				m_owner.Brain.StopCurrentAction();
			}
		}

		public override void Stop()
		{
			if (m_owner.IsUsingSpell && m_owner.SpellCast.Spell == m_spell)
			{
				m_owner.CancelSpellCast();
			}
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.Active; }
		}
	}
}