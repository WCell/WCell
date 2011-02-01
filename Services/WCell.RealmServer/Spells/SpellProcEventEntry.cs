using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Util.Data;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Util;
using WCell.RealmServer.Misc;

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
		public uint[] SpellFamilyMaskA;

		[Persistent(3)]
		public uint[] SpellFamilyMaskB;

		[Persistent(3)]
		public uint[] SpellFamilyMaskC;

		public ProcTriggerFlags ProcTriggerFlags;

		public uint ProcEx; //proc Extend info
		public float PpmRate;
		public float CustomChance;
		public uint Cooldown;
		#endregion

		public uint[] GetSpellFamilyMask(int index)
		{
			switch (index)
			{
				case 0: return SpellFamilyMaskA;
				case 1: return SpellFamilyMaskB;
				case 3: return SpellFamilyMaskC;
			}
			return null;
		}

		public DataHolderState DataHolderState
		{
			get;
			set;
		}
		public void FinalizeDataHolder()
		{
			ProcEventHelper.Entries.Add(SpellId, this);

			// sieve out "null masks"
			if (SpellFamilyMaskA.Sum() == 0)
			{
				SpellFamilyMaskA = null;
			}
			if (SpellFamilyMaskB.Sum() == 0)
			{
				SpellFamilyMaskB = null;
			}
			if (SpellFamilyMaskC.Sum() == 0)
			{
				SpellFamilyMaskC = null;
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
		/// Applies the correct AffectMasks from spell_proc_event to a given spell.
		/// If the spell is part of a SpellLine, it applies the changes to all spells in that line too
		/// </summary>
		public static bool PatchAffectMasks(Spell spell)
		{
			SpellProcEventEntry spellprocentry;
			if (!Entries.TryGetValue(spell.SpellId, out spellprocentry))
			{
				return false;
			}

			if (spellprocentry.SchoolMask != 0)
			{
				spell.SchoolMask = spellprocentry.SchoolMask;
			}

			if (spellprocentry.SpellClassSet != 0)
			{
				spell.SpellClassSet = spellprocentry.SpellClassSet;
			}

			if (spellprocentry.ProcTriggerFlags != 0)
			{
				spell.ProcTriggerFlags = spellprocentry.ProcTriggerFlags;
			}

			if (spellprocentry.CustomChance != 0)
			{
				// like the DBC, CustomChance only contains natural percentages
				spell.ProcChance = (uint)spellprocentry.CustomChance;
			}

			var line = spell.Line;

			if (line != null)
			{
				line.LineId.Apply(spellh => PatchSpell(spellh, spellprocentry));
			}
			else
			{
				SpellHandler.Apply(spellh => PatchSpell(spellh, spellprocentry), spell.SpellId);
			}
			return true;
		}

		static void PatchSpell(Spell spell, SpellProcEventEntry procEntry)
		{
			foreach (var effect in spell.Effects)
			{
				if (effect.AuraType == AuraType.ProcTriggerSpell)
				{
					if (effect.EffectIndex == -1) continue;	// custom effect

					var mask = procEntry.GetSpellFamilyMask(effect.EffectIndex);
					if (mask == null) continue;
					
					// copy AffectMask
					effect.AffectMask = mask.ToArray();
				}
			}
		}
	}
}
