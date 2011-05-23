using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Spells.Mage
{
	public static class MageArcaneFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixMe()
		{
			// conjure water and food don't have any per level bonus
			SpellLineId.MageConjureRefreshment.Apply(spell => spell.ForeachEffect(effect => effect.RealPointsPerLevel = 0));

            SpellLineId.MageArcaneArcanePotency.Apply(spell =>
            {
                spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new AddProcHandler(new ArcanePotencyProcHandler(
                    ProcTriggerFlags.HealOther | ProcTriggerFlags.SpellCast,
                    spell.GetEffect(AuraType.Dummy).CalcEffectValue()));

            });

            SpellHandler.Apply(spell =>
            {
                spell.AddEffect((cast, effect) => new ClearCastingAndPresenceOfMindHandler(cast, effect), ImplicitSpellTargetType.Self);
            }, SpellId.EffectClearcasting, SpellId.MageArcanePresenceOfMind);

            
		}
	}

    public class ClearCastingAndPresenceOfMindHandler : SpellEffectHandler
    {
        public ClearCastingAndPresenceOfMindHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect) { }

        protected override void Apply(WorldObject target)
        {
            var caster = m_cast.CasterChar;
            if (caster == null) return;
            var aura = caster.Auras[SpellLineId.MageArcaneArcanePotency];
            if (aura != null)
            {
                var handler = caster.GetProcHandler<ArcanePotencyProcHandler>();
                handler.trigger = true;       
            }
        }
    }

    #region Arcane Potency
    public class ArcanePotencyProcHandler : IProcHandler
    {
        public ArcanePotencyProcHandler(ProcTriggerFlags flags, int valPercentage)
        {
            modPercentage = valPercentage;
            trigger = false;
        }
        public bool trigger;
        private int modPercentage;
        public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
        {
            var dAction = action as DamageAction;
            if (dAction != null)
                return true;
            return false;
        }
        public void TriggerProc(Unit triggerer, IUnitAction action)
        {
            if (trigger)
            {
                var dAction = action as DamageAction;
                if (dAction.CanCrit)
                {
                    dAction.AddBonusCritChance(modPercentage);
                    trigger = false;
                }
            }
        }
        public Unit Owner
        {
            get;
            private set;
        }
        public Spell ProcSpell
        {
            get { return null; }
        }
        public uint ProcChance
        {
            get { return 100; }
        }
        public DateTime NextProcTime
        {
            get;
            set;
        }

        public int MinProcDelay
        {
            get { return 0; }
        }
        public int StackCount
        {
            get { return 0; }
        }
        public ProcTriggerFlags ProcTriggerFlags
        {
            get { return ProcTriggerFlags.HealOther | ProcTriggerFlags.SpellCast; }
        }

        public void Dispose()
        {
            Owner.RemoveProcHandler(this);
        }
    }
    #endregion  
}
