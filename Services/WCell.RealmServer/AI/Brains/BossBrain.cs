//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.RealmServer.Entities;
//using WCell.RealmServer.Spells;
//using WCell.RealmServer.Spells.Auras;

//namespace WCell.RealmServer.AI
//{
//    public class BossPhase : IAIEventsHandler
//    {
//        public BossPhase(IBrain owner)
//        {
//            m_owner = owner;
//        }

//        protected IBrain m_owner;

//        public IBrain Owner
//        {
//            get { return m_owner; }
//            set { m_owner = value; }
//        }

//        #region Events handlers

//        public virtual void OnEnterCombat(Unit attackingUnit)
//        {
//        }

//        public virtual void OnDamageTaken(Unit attackingUnit, SpellCast cast, int damageAmt)
//        {
//        }

//        public virtual void OnDebuff(Unit attackingUnit, SpellCast cast, Aura debuff)
//        {
			
//        }

//        public virtual void OnDamageDealt(Unit victimUnit, SpellCast cast, int damageAmt)
//        {
//        }

//        public virtual void OnLeaveCombat()
//        {
//        }

//        public virtual void OnKilled(Unit killerUnit, Unit victimUnit)
//        {
//        }

//        public virtual void OnDeath(Unit killerUnit)
//        {
//        }

//        public virtual void OnSpawn()
//        {
//        }

//        public virtual void OnCombatTargetOutOfRange()
//        {
//        }

//        #endregion
//    }
	
//    public class BossBrain : MobBrain
//    {
//        public BossBrain(NPC owner) : base(owner)
//        {
//        }

//        protected LinkedListNode<BossPhase> m_currentPhase;
//        protected LinkedList<BossPhase> m_phases;

//        #region Events Handlers

//        public override void OnEnterCombat(Unit attackingUnit)
//        {
//            m_currentPhase.Value.OnEnterCombat(attackingUnit);
//        }

//        public override void OnDamageTaken(Unit attackingUnit, SpellCast spell, int damageAmt)
//        {
//            m_currentPhase.Value.OnDamageTaken(attackingUnit, spell, damageAmt);
//        }

//        public override void OnDamageDealt(Unit victimUnit, SpellCast spell, int damageAmt)
//        {
//            m_currentPhase.Value.OnDamageDealt(victimUnit, spell, damageAmt);
//        }

//        public override void OnLeaveCombat()
//        {
//            m_currentPhase.Value.OnLeaveCombat();
//        }

//        public override void OnKilled(Unit killerUnit, Unit victimUnit)
//        {
//            m_currentPhase.Value.OnKilled(killerUnit, victimUnit);
//        }

//        public override void OnDeath(Unit killerUnit)
//        {
//            m_currentPhase.Value.OnDeath(killerUnit);
//        }

//        public override void OnSpawn()
//        {
//            m_currentPhase.Value.OnSpawn();
//        }

//        public override void OnCombatTargetOutOfRange()
//        {
//            m_currentPhase.Value.OnCombatTargetOutOfRange();
//        }

//        #endregion
//    }
//}