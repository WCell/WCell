using System;
using System.Collections.Generic;
using WCell.Util.Data;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Util;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells
{
	/// <summary>
	/// Represents a row from spell_proc_event database table
	/// </summary>
	[DataHolder]
	public class SpellProcEventEntry : IDataHolder
	{
		#region DBfields
		public SpellId entry;
		public DamageSchoolMask SchoolMask;
		public SpellClassSet SpellFamilyName;
		public uint SpellFamilyMaskA0;
		public uint SpellFamilyMaskA1;
		public uint SpellFamilyMaskA2;
		public uint SpellFamilyMaskB0;
		public uint SpellFamilyMaskB1;
		public uint SpellFamilyMaskB2;
		public uint SpellFamilyMaskC0;
		public uint SpellFamilyMaskC1;
		public uint SpellFamilyMaskC2;
		public ProcTriggerFlags procFlags;
		public uint procEx; //proc Extend info
		public float ppmRate;
		public float customChance;
		public uint Cooldown;
		#endregion

		public DataHolderState DataHolderState
		{
			get;
			set;
		}
		public void FinalizeDataHolder()
		{
			ProcEventHelper.entries.Add(entry, this);
		}


	}
	/// <summary>
	/// Contains a list of all SpellProcEventEntries and some helper functions
	/// </summary>
	public static class ProcEventHelper
	{
		public static Dictionary<SpellId, SpellProcEventEntry> entries = new Dictionary<SpellId, SpellProcEventEntry>();

		/// <summary>
		/// returns a ProcLoadIndex value which tells us which spelleffectindexes we need to modify
		/// </summary>
		public static ProcLoadIndex GetProcEffectIndex(Spell spell)
		{
			ProcLoadIndex index = 0;

			if (spell.ProcHandlers != null) //this spell has custom handlers so we don't care
				return ProcLoadIndex.None;


			if (spell.Effects[0].AuraType == AuraType.ProcTriggerSpell)
				index |= ProcLoadIndex.Index0;

			if (spell.EffectHandlerCount > 1)
			{
				if (spell.Effects[1].AuraType == AuraType.ProcTriggerSpell)
					index |= ProcLoadIndex.Index1;
			}
			if (spell.EffectHandlerCount > 2)
			{
				if (spell.Effects[2].AuraType == AuraType.ProcTriggerSpell)
					index |= ProcLoadIndex.Index2;
			}

			return (ProcLoadIndex)index;
		}
		/// <summary>
		/// Applies the correct AffectMasks from spell_proc_event to a given spell.
		/// If the spell is part of a SpellLine, it applies the changes to all spells in that line too
		/// </summary>
		public static void PatchAffectMasks(Spell spell, ProcLoadIndex index)
		{
			SpellProcEventEntry spellprocentry;
			entries.TryGetValue(spell.SpellId, out spellprocentry);
			var IsSpellLine = spell.Line != null ? true : false;

			switch (index)
			{
				case ProcLoadIndex.None: //why are there spells in the db that don't have Proctriggerspellaura?
					break;
				case ProcLoadIndex.Index0:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index1:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index0and1:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index2:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index0and2:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index1and2:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						}, spell.SpellId);
					}
					break;
				case ProcLoadIndex.Index0and1and2:
					if (IsSpellLine)
					{
						spell.Line.LineId.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						});
					}
					else
					{
						SpellHandler.Apply(spellh =>
						{
							spellh.Effects[0].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA0, spellprocentry.SpellFamilyMaskB0, spellprocentry.SpellFamilyMaskC0 };
							spellh.Effects[1].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA1, spellprocentry.SpellFamilyMaskB1, spellprocentry.SpellFamilyMaskC1 };
							spellh.Effects[2].AffectMask = new uint[] { spellprocentry.SpellFamilyMaskA2, spellprocentry.SpellFamilyMaskB2, spellprocentry.SpellFamilyMaskC2 };
						}, spell.SpellId);
					}
					break;
			}


		}


	}

	[Flags]
	public enum ProcLoadIndex : uint
	{
		None = 0,
		Index0 = 1,
		Index1 = 2,
		Index0and1 = 3,
		Index2 = 4,
		Index0and2 = 5,
		Index1and2 = 6,
		Index0and1and2 = 7
	}
}
