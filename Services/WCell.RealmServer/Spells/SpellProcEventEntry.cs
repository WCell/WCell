using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents a row from UDB's spell_proc_event database table.
	/// Defines some corrections for spells
	/// </summary>
	[DataHolder]
	public class SpellProcEventEntry : IDataHolder
	{
		#region DBfields
		public SpellId SpellId;
		public DamageSchoolMask SchoolMask;
		public SpellClassSet SpellClassSet;

		[Persistent(3)]
		public uint[] SpellFamilyMask0;

		[Persistent(3)]
		public uint[] SpellFamilyMask1;

		[Persistent(3)]
		public uint[] SpellFamilyMask2;

		public ProcTriggerFlags ProcFlags;
		public ProcFlagsExLegacy ProcFlagsEx; //proc Extend info
		public float PpmRate;
		public float CustomChance;
		public uint Cooldown;
		#endregion

		public uint[] GetSpellFamilyMask(EffectIndex index)
		{
			switch (index)
			{
				case EffectIndex.Zero: return SpellFamilyMask0;
				case EffectIndex.One: return SpellFamilyMask1;
				case EffectIndex.Two: return SpellFamilyMask2;
			}
			return null;
		}

		[NotPersistent]
		public DataHolderState DataHolderState
		{
			get;
			set;
		}

		public void FinalizeDataHolder()
		{
			ProcEventHelper.Entries.Add(SpellId, this);

			// sieve out "null masks"
			if (SpellFamilyMask0.Sum() == 0)
			{
				SpellFamilyMask0 = null;
			}
			if (SpellFamilyMask1.Sum() == 0)
			{
				SpellFamilyMask1 = null;
			}
			if (SpellFamilyMask2.Sum() == 0)
			{
				SpellFamilyMask2 = null;
			}
		}


	}
	/// <summary>
	/// Contains a list of all SpellProcEventEntries and some helper functions
	/// </summary>
	public static class ProcEventHelper
	{
		public static readonly Dictionary<SpellId, SpellProcEventEntry> Entries = new Dictionary<SpellId, SpellProcEventEntry>();

        /// <summary>
        /// Apply custom proc settings from SpellProcEventEntry to all spells
        /// </summary>
        public static void PatchSpells(Spell[] spells)
        {
            foreach (var spellId in Entries.Keys)
            {
                var spell = spells[(int)spellId];

                if (spell == null) continue;

                var entry = Entries[spell.SpellId];

                if (spell.Line == null)
                {
                    PatchSpell(spell, entry);
                }
                else
                {
                    // Part of the spell line -> apply to all ranks
                    spell.Line.LineId.Apply(spellToPatch => PatchSpell(spellToPatch, entry));
                }
            }
        }

        /// <summary>
        /// Apply custom proc settings from SpellProcEventEntry to a given spell
        /// </summary>
        private static void PatchSpell(Spell spell, SpellProcEventEntry procEntry)
        {
            if (procEntry.SchoolMask != 0)
            {
                spell.SchoolMask = procEntry.SchoolMask;
            }

            if (procEntry.SpellClassSet != 0)
            {
                spell.SpellClassSet = procEntry.SpellClassSet;
            }

            if (procEntry.ProcFlags != 0)
            {
                spell.ProcTriggerFlags = procEntry.ProcFlags;
            }

            if (procEntry.ProcFlagsEx != 0)
            {
                // Take just hit flags we don't need others
                spell.ProcHitFlags = (ProcHitFlags)procEntry.ProcFlagsEx & ProcHitFlags.All;
            }

            if (procEntry.CustomChance != 0)
            {
                // like the DBC, CustomChance only contains natural percentages
                spell.ProcChance = (uint)procEntry.CustomChance;
            }

            var procEffects = from effect in spell.Effects
                              where effect.AuraType == AuraType.ProcTriggerSpell
                              && effect.EffectIndex != EffectIndex.Custom
                              select effect;

            foreach (var procEffect in procEffects)
            {
                var mask = procEntry.GetSpellFamilyMask(procEffect.EffectIndex);
                if (mask == null) continue;

                // copy AffectMask
                procEffect.AffectMask = mask.ToArray();
            }
        }
	}
}
