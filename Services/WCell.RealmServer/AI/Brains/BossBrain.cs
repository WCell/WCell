using System.Collections.Generic;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.AI.Brains
{
	public class BossPhase : IAIEventsHandler
	{
		public BossPhase(IBrain owner)
		{
			m_owner = owner;
		}

		protected IBrain m_owner;

		public IBrain Owner
		{
			get { return m_owner; }
			set { m_owner = value; }
		}

		#region Events handlers

		public virtual void OnEnterCombat()
		{
		}

		public virtual void OnDamageTaken(Misc.IDamageAction action)
		{
		}

		public virtual void OnDebuff(Unit attackingUnit, SpellCast cast, Aura debuff)
		{

		}

		public virtual void OnDamageDealt(Misc.IDamageAction action)
		{
		}

		public virtual void OnLeaveCombat()
		{
		}

		public virtual void OnKilled(Unit killerUnit, Unit victimUnit)
		{
		}

		public virtual void OnDeath()
		{
		}

		public virtual void OnSpawn()
		{
		}

		public virtual void OnCombatTargetOutOfRange()
		{
		}

		public virtual void OnHeal(Unit healer, Unit healed, int amtHealed)
		{
		}

		#endregion
	}

	public class BossBrain : MobBrain
	{
		public BossBrain(NPC owner)
			: base(owner)
		{
		}

		protected LinkedListNode<BossPhase> CurrentPhase;
		protected LinkedList<BossPhase> Phases;

		#region Events Handlers

		public override void OnEnterCombat()
		{
			CurrentPhase.Value.OnEnterCombat();
			base.OnEnterCombat();
		}

		public override void  OnDamageReceived(Misc.IDamageAction action)
		{
			CurrentPhase.Value.OnDamageTaken(action);
			base.OnDamageReceived(action);
		}

		public override void  OnDamageDealt(Misc.IDamageAction action)
		{
			CurrentPhase.Value.OnDamageDealt(action);
			base.OnDamageDealt(action);
		}

		public override void OnLeaveCombat()
		{
			CurrentPhase.Value.OnLeaveCombat();
			base.OnLeaveCombat();
		}

		public override void OnKilled(Unit killerUnit, Unit victimUnit)
		{
			CurrentPhase.Value.OnKilled(killerUnit, victimUnit);
			base.OnKilled(killerUnit, victimUnit);
		}

		public override void OnDeath()
		{
			CurrentPhase.Value.OnDeath();
			base.OnDeath();
		}

		public override void OnActivate()
		{
			CurrentPhase.Value.OnSpawn();
			base.OnActivate();
		}

		public override void OnCombatTargetOutOfRange()
		{
			CurrentPhase.Value.OnCombatTargetOutOfRange();
			base.OnCombatTargetOutOfRange();
		}

		public override void OnHeal(Unit healer, Unit healed, int amtHealed)
		{
			CurrentPhase.Value.OnHeal(healer, healed, amtHealed);
			base.OnHeal(healer, healed, amtHealed);
		}

		#endregion
	}
}