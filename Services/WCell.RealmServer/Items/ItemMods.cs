using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Modifiers
{
    internal static class ItemMods
    {
        public delegate void ItemModHandler(Character owner, int value);

        public static readonly ItemModHandler[] AddHandlers = new ItemModHandler[(int)ItemModType.End];
        public static readonly ItemModHandler[] RemoveHandlers = new ItemModHandler[(int)ItemModType.End];

        static ItemMods()
        {
            AddHandlers[(int)ItemModType.Unused] = AddUnused;
            AddHandlers[(int)ItemModType.Power] = AddPower;
            AddHandlers[(int)ItemModType.Health] = AddHealth;
            AddHandlers[(int)ItemModType.Agility] = AddAgility;
            AddHandlers[(int)ItemModType.Strength] = AddStrength;
            AddHandlers[(int)ItemModType.Intellect] = AddIntellect;
            AddHandlers[(int)ItemModType.Spirit] = AddSpirit;
            AddHandlers[(int)ItemModType.Stamina] = AddStamina;
            AddHandlers[(int)ItemModType.WeaponSkillRating] = AddWeaponSkillRating;
            AddHandlers[(int)ItemModType.DefenseRating] = AddDefenseRating;
            AddHandlers[(int)ItemModType.DodgeRating] = AddDodgeRating;
            AddHandlers[(int)ItemModType.ParryRating] = AddParryRating;
            AddHandlers[(int)ItemModType.BlockRating] = AddBlockRating;
            AddHandlers[(int)ItemModType.MeleeHitRating] = AddMeleeHitRating;
            AddHandlers[(int)ItemModType.RangedHitRating] = AddRangedHitRating;
            AddHandlers[(int)ItemModType.SpellHitRating] = AddSpellHitRating;
            AddHandlers[(int)ItemModType.MeleeCriticalStrikeRating] = AddMeleeCriticalStrikeRating;
            AddHandlers[(int)ItemModType.RangedCriticalStrikeRating] = AddRangedCriticalStrikeRating;
            AddHandlers[(int)ItemModType.SpellCriticalStrikeRating] = AddSpellCriticalStrikeRating;
            AddHandlers[(int)ItemModType.MeleeHitAvoidanceRating] = AddMeleeHitAvoidanceRating;
            AddHandlers[(int)ItemModType.RangedHitAvoidanceRating] = AddRangedHitAvoidanceRating;
            AddHandlers[(int)ItemModType.SpellHitAvoidanceRating] = AddSpellHitAvoidanceRating;
            AddHandlers[(int)ItemModType.MeleeCriticalAvoidanceRating] = AddMeleeCriticalAvoidanceRating;
            AddHandlers[(int)ItemModType.RangedCriticalAvoidanceRating] = AddRangedCriticalAvoidanceRating;
            AddHandlers[(int)ItemModType.SpellCriticalAvoidanceRating] = AddSpellCriticalAvoidanceRating;
            AddHandlers[(int)ItemModType.MeleeHasteRating] = AddMeleeHasteRating;
            AddHandlers[(int)ItemModType.RangedHasteRating] = AddRangedHasteRating;
            AddHandlers[(int)ItemModType.SpellHasteRating] = AddSpellHasteRating;
            AddHandlers[(int)ItemModType.HitRating] = AddHitRating;
            AddHandlers[(int)ItemModType.CriticalStrikeRating] = AddCriticalStrikeRating;
            AddHandlers[(int)ItemModType.HitAvoidanceRating] = AddHitAvoidanceRating;
            AddHandlers[(int)ItemModType.CriticalAvoidanceRating] = AddCriticalAvoidanceRating;
            AddHandlers[(int)ItemModType.ResilienceRating] = AddResilienceRating;
            AddHandlers[(int)ItemModType.HasteRating] = AddHasteRating;
            AddHandlers[(int)ItemModType.ExpertiseRating] = AddExpertiseRating;

            RemoveHandlers[(int)ItemModType.Unused] = RemoveUnused;
            RemoveHandlers[(int)ItemModType.Power] = RemovePower;
            RemoveHandlers[(int)ItemModType.Health] = RemoveHealth;
            RemoveHandlers[(int)ItemModType.Agility] = RemoveAgility;
            RemoveHandlers[(int)ItemModType.Strength] = RemoveStrength;
            RemoveHandlers[(int)ItemModType.Intellect] = RemoveIntellect;
            RemoveHandlers[(int)ItemModType.Spirit] = RemoveSpirit;
            RemoveHandlers[(int)ItemModType.Stamina] = RemoveStamina;
            RemoveHandlers[(int)ItemModType.WeaponSkillRating] = RemoveWeaponSkillRating;
            RemoveHandlers[(int)ItemModType.DefenseRating] = RemoveDefenseRating;
            RemoveHandlers[(int)ItemModType.DodgeRating] = RemoveDodgeRating;
            RemoveHandlers[(int)ItemModType.ParryRating] = RemoveParryRating;
            RemoveHandlers[(int)ItemModType.BlockRating] = RemoveBlockRating;
            RemoveHandlers[(int)ItemModType.MeleeHitRating] = RemoveMeleeHitRating;
            RemoveHandlers[(int)ItemModType.RangedHitRating] = RemoveRangedHitRating;
            RemoveHandlers[(int)ItemModType.SpellHitRating] = RemoveSpellHitRating;
            RemoveHandlers[(int)ItemModType.MeleeCriticalStrikeRating] = RemoveMeleeCriticalStrikeRating;
            RemoveHandlers[(int)ItemModType.RangedCriticalStrikeRating] = RemoveRangedCriticalStrikeRating;
            RemoveHandlers[(int)ItemModType.SpellCriticalStrikeRating] = RemoveSpellCriticalStrikeRating;
            RemoveHandlers[(int)ItemModType.MeleeHitAvoidanceRating] = RemoveMeleeHitAvoidanceRating;
            RemoveHandlers[(int)ItemModType.RangedHitAvoidanceRating] = RemoveRangedHitAvoidanceRating;
            RemoveHandlers[(int)ItemModType.SpellHitAvoidanceRating] = RemoveSpellHitAvoidanceRating;
            RemoveHandlers[(int)ItemModType.MeleeCriticalAvoidanceRating] = RemoveMeleeCriticalAvoidanceRating;
            RemoveHandlers[(int)ItemModType.RangedCriticalAvoidanceRating] = RemoveRangedCriticalAvoidanceRating;
            RemoveHandlers[(int)ItemModType.SpellCriticalAvoidanceRating] = RemoveSpellCriticalAvoidanceRating;
            RemoveHandlers[(int)ItemModType.MeleeHasteRating] = RemoveMeleeHasteRating;
            RemoveHandlers[(int)ItemModType.RangedHasteRating] = RemoveRangedHasteRating;
            RemoveHandlers[(int)ItemModType.SpellHasteRating] = RemoveSpellHasteRating;
            RemoveHandlers[(int)ItemModType.HitRating] = RemoveHitRating;
            RemoveHandlers[(int)ItemModType.CriticalStrikeRating] = RemoveCriticalStrikeRating;
            RemoveHandlers[(int)ItemModType.HitAvoidanceRating] = RemoveHitAvoidanceRating;
            RemoveHandlers[(int)ItemModType.CriticalAvoidanceRating] = RemoveCriticalAvoidanceRating;
            RemoveHandlers[(int)ItemModType.ResilienceRating] = RemoveResilienceRating;
            RemoveHandlers[(int)ItemModType.HasteRating] = RemoveHasteRating;
            RemoveHandlers[(int)ItemModType.ExpertiseRating] = RemoveExpertiseRating;

            // new modifiers
            AddHandlers[(int)ItemModType.SpellDamageDone] = AddSpellDamageDone;
            RemoveHandlers[(int)ItemModType.SpellDamageDone] = RemoveSpellDamageDone;
            AddHandlers[(int)ItemModType.SpellHealingDone] = AddSpellHealingDone;
            RemoveHandlers[(int)ItemModType.SpellHealingDone] = RemoveSpellHealingDone;
            AddHandlers[(int)ItemModType.SpellPower] = AddSpellPower;
            RemoveHandlers[(int)ItemModType.SpellPower] = RemoveSpellPower;

            AddHandlers[(int)ItemModType.BlockValue] = AddBlockValue;
            RemoveHandlers[(int)ItemModType.BlockValue] = RemoveBlockValue;

            AddHandlers[(int)ItemModType.ManaRegeneration] = AddManaRegen;	// TODO: Depends on PowerType
            RemoveHandlers[(int)ItemModType.ManaRegeneration] = RemoveManaRegen;
            AddHandlers[(int)ItemModType.HealthRegenration] = AddHealthRegen;
            RemoveHandlers[(int)ItemModType.HealthRegenration] = RemoveHealthRegen;
        }

        public static void ApplyStatMods(this ItemTemplate template, Character owner)
        {
            for (var i = 0; i < template.Mods.Length; i++)
            {
                var mod = template.Mods[i];
                if (mod.Value != 0)
                {
                    ApplyStatMod(owner, mod.Type, mod.Value);
                }
            }
        }

        public static void ApplyStatMod(this Character owner, ItemModType modType, int value)
        {
            var handler = AddHandlers[(int)modType];
            if (handler != null)
            {
                handler(owner, value);
            }
        }

        public static void RemoveStatMods(this ItemTemplate template, Character owner)
        {
            foreach (var mod in template.Mods)
            {
                if (mod.Value != 0)
                {
                    RemoveStatMod(owner, mod.Type, mod.Value);
                }
            }
        }

        public static void RemoveStatMod(this Character owner, ItemModType modType, int value)
        {
            var handler = RemoveHandlers[(int)modType];
            if (handler != null)
            {
                handler(owner, value);
            }
        }

        #region Add

        private static void AddPower(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Power, value);
        }

        private static void AddHealth(Character owner, int value)
        {
            owner.MaxHealthModFlat += value;
        }

        private static void AddUnused(Character owner, int value)
        {
        }

        private static void AddAgility(Character owner, int value)
        {
            owner.AddStatMod(StatType.Agility, value);
        }

        private static void AddStrength(Character owner, int value)
        {
            owner.AddStatMod(StatType.Strength, value);
        }

        private static void AddIntellect(Character owner, int value)
        {
            owner.AddStatMod(StatType.Intellect, value);
        }

        private static void AddSpirit(Character owner, int value)
        {
            owner.AddStatMod(StatType.Spirit, value);
        }

        private static void AddStamina(Character owner, int value)
        {
            owner.AddStatMod(StatType.Stamina, value);
        }

        private static void AddWeaponSkillRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.WeaponSkill, value);
        }

        private static void AddDefenseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.DefenseSkill, value);
        }

        private static void AddDodgeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Dodge, value);
        }

        private static void AddParryRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Parry, value);
        }

        private static void AddBlockRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Block, value);
        }

        private static void AddMeleeHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, value);
        }

        private static void AddRangedHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHitChance, value);
        }

        private static void AddSpellHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHitChance, value);
        }

        private static void AddMeleeCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, value);
        }

        private static void AddRangedCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedCritChance, value);
        }

        private static void AddSpellCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellCritChance, value);
        }

        private static void AddMeleeHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, value);
        }

        private static void AddRangedHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedAttackerHit, value);
        }

        private static void AddSpellHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellAttackerHit, value);
        }

        private static void AddMeleeCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, value);
        }

        private static void AddRangedCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedResilience, value);
        }

        private static void AddSpellCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellResilience, value);
        }

        private static void AddMeleeHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, value);
        }

        private static void AddRangedHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHaste, value);
        }

        private static void AddSpellHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHaste, value);
        }

        private static void AddHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, value);
            owner.ModCombatRating(CombatRating.RangedHitChance, value);
            owner.ModCombatRating(CombatRating.SpellHitChance, value);
        }

        private static void AddCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, value);
            owner.ModCombatRating(CombatRating.RangedCritChance, value);
        }

        private static void AddHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void AddCriticalAvoidanceRating(Character owner, int value)
        {
        }

        private static void AddResilienceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, value);
            owner.ModCombatRating(CombatRating.RangedResilience, value);
            owner.ModCombatRating(CombatRating.SpellResilience, value);
        }

        private static void AddHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, value);
            owner.ModCombatRating(CombatRating.RangedHaste, value);
        }

        private static void AddExpertiseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Expertise, value);
        }

        #endregion Add

        #region Remove

        private static void RemovePower(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierInt.Power, -value);
        }

        private static void RemoveHealth(Character owner, int value)
        {
            owner.MaxHealthModFlat -= value;
        }

        private static void RemoveUnused(Character owner, int value)
        {
        }

        private static void RemoveAgility(Character owner, int value)
        {
            owner.RemoveStatMod(StatType.Agility, value);
        }

        private static void RemoveStrength(Character owner, int value)
        {
            owner.RemoveStatMod(StatType.Strength, value);
        }

        private static void RemoveIntellect(Character owner, int value)
        {
            owner.RemoveStatMod(StatType.Intellect, value);
        }

        private static void RemoveSpirit(Character owner, int value)
        {
            owner.RemoveStatMod(StatType.Spirit, value);
        }

        private static void RemoveStamina(Character owner, int value)
        {
            owner.RemoveStatMod(StatType.Stamina, value);
        }

        private static void RemoveWeaponSkillRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.WeaponSkill, -value);
        }

        private static void RemoveDefenseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.DefenseSkill, -value);
        }

        private static void RemoveDodgeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Dodge, -value);
        }

        private static void RemoveParryRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Parry, -value);
        }

        private static void RemoveBlockRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Block, -value);
        }

        private static void RemoveMeleeHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
        }

        private static void RemoveRangedHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHitChance, -value);
        }

        private static void RemoveSpellHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHitChance, -value);
        }

        private static void RemoveMeleeCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
        }

        private static void RemoveRangedCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedCritChance, -value);
        }

        private static void RemoveSpellCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellCritChance, -value);
        }

        private static void RemoveMeleeHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void RemoveRangedHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedAttackerHit, -value);
        }

        private static void RemoveSpellHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellAttackerHit, -value);
        }

        private static void RemoveMeleeCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, -value);
        }

        private static void RemoveRangedCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedResilience, -value);
        }

        private static void RemoveSpellCriticalAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellResilience, -value);
        }

        private static void RemoveMeleeHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, -value);
        }

        private static void RemoveRangedHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.RangedHaste, -value);
        }

        private static void RemoveSpellHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.SpellHaste, -value);
        }

        private static void RemoveHitRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHitChance, -value);
            owner.ModCombatRating(CombatRating.RangedHitChance, -value);
        }

        private static void RemoveCriticalStrikeRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeCritChance, -value);
            owner.ModCombatRating(CombatRating.RangedCritChance, -value);
        }

        private static void RemoveHitAvoidanceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeAttackerHit, -value);
        }

        private static void RemoveCriticalAvoidanceRating(Character owner, int value)
        {
        }

        private static void RemoveResilienceRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeResilience, -value);
            owner.ModCombatRating(CombatRating.RangedResilience, -value);
            owner.ModCombatRating(CombatRating.SpellResilience, -value);
        }

        private static void RemoveHasteRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.MeleeHaste, -value);
            owner.ModCombatRating(CombatRating.RangedHaste, -value);
        }

        private static void RemoveExpertiseRating(Character owner, int value)
        {
            owner.ModCombatRating(CombatRating.Expertise, -value);
        }

        #endregion Remove

        private static void AddSpellPower(Character owner, int value)
        {
            AddSpellDamageDone(owner, value);
            AddSpellHealingDone(owner, value);
        }

        private static void RemoveSpellPower(Character owner, int value)
        {
            RemoveSpellDamageDone(owner, value);
            RemoveSpellHealingDone(owner, value);
        }

        private static void AddSpellDamageDone(Character owner, int value)
        {
            owner.AddDamageDoneMod(SpellConstants.AllDamageSchoolSet, value);
        }

        private static void RemoveSpellDamageDone(Character owner, int value)
        {
            owner.RemoveDamageDoneMod(SpellConstants.AllDamageSchoolSet, value);
        }

        private static void AddSpellHealingDone(Character owner, int value)
        {
            owner.HealingDoneMod += value;
        }

        private static void RemoveSpellHealingDone(Character owner, int value)
        {
            owner.HealingDoneMod -= value;
        }

        private static void AddBlockValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.BlockValue, value);
        }

        private static void RemoveBlockValue(Character owner, int value)
        {
            owner.ChangeModifier(StatModifierFloat.BlockValue, -value);
        }

        private static void AddManaRegen(Character owner, int value)
        {
            if (owner.PowerType == PowerType.Mana)
            {
                owner.ChangeModifier(StatModifierInt.PowerRegen, value);
            }
        }

        private static void RemoveManaRegen(Character owner, int value)
        {
            if (owner.PowerType == PowerType.Mana)
            {
                owner.ChangeModifier(StatModifierInt.PowerRegen, -value);
            }
        }

        private static void AddHealthRegen(Character owner, int value)
        {
            if (owner.PowerType == PowerType.Mana)
            {
                owner.ChangeModifier(StatModifierInt.HealthRegen, value);
            }
        }

        private static void RemoveHealthRegen(Character owner, int value)
        {
            if (owner.PowerType == PowerType.Mana)
            {
                owner.ChangeModifier(StatModifierInt.HealthRegen, -value);
            }
        }
    }
}