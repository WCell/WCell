using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

namespace WCell.Tools.Ralek
{
	public enum SpellClassSet
	{
	}

	public static class SpellStudies
	{
		static SpellStudies()
		{
			SpellHandler.LoadSpells();
		}

		public static void FindAllWithAttribute(SpellAttributes attr)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.Has(attr)
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, spell.Attributes);
				Console.WriteLine();
			}

			Console.WriteLine("{0} spells have Attribute {1}", query.Count(), attr);
		}

		public static void FindAllWithAttributeEx(SpellAttributesEx attr)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.Has(attr)
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, spell.AttributesEx);
				Console.WriteLine();
			}

			Console.WriteLine("{0} spells have Attribute {1}", query.Count(), attr);
			Console.WriteLine();
		}

		public static void FindAllWithAttributeExB(SpellAttributesExB attr)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.Has(attr)
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, spell.AttributesExB);
				Console.WriteLine();
			}

			Console.WriteLine("{0} spells have Attribute {1}", query.Count(), attr);
			Console.WriteLine();
		}

		public static void FindAllWithFamilyName(Constants.Spells.SpellClassSet set)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.SpellClassSet == set
			            select spell;

			foreach (var spell in query)
			{
				//Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, spell.FamilyFlags);
				//Console.WriteLine();
			}

			Console.WriteLine("{0} spells have Name {1}", query.Count(), set);
			Console.WriteLine();
		}

		public static void FindSpellsWithFacingFlag(SpellFacingFlags flag)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.HasFacingFlag(flag)
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, spell.FacingFlags);
				Console.WriteLine();
			}

			Console.WriteLine("{0} spells have FacingFlag {1}", query.Count(), flag);
			Console.WriteLine();
		}

		public static void FindSpellsWhere(Predicate<Spell> spellPred, Func<Spell, object> specialdisplay)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spellPred(spell)
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} - {2}", spell.Id, spell.Name, specialdisplay(spell));
				Console.WriteLine();
			}

			Console.WriteLine("{0} spells matched condition", query.Count());
			Console.WriteLine();
		}

		private static bool SpellHasMask(Spell spell)
		{
			foreach (var effect in spell.Effects)
			{
				if (effect.AffectMask[0] > 0 || effect.AffectMask[1] > 0 || effect.AffectMask[2] > 0)
					return true;
			}
			return false;
		}

		public static void FindSpellsWithEffectMasks()
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where SpellHasMask(spell)
			            select spell;
			int i = 0;
			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} \t {2}{3}{4}", spell.Id, spell.Name, spell.SpellClassMask[0].ToString("X8"),
				                  spell.SpellClassMask[1].ToString("X8"), spell.SpellClassMask[2].ToString("X8"));
				foreach (var effect in spell.Effects)
				{
					Console.WriteLine("{0} - {1}{2}{3}", effect.EffectType, effect.AffectMask[0].ToString("X8"),
					                  effect.AffectMask[1].ToString("X8"), effect.AffectMask[2].ToString("X8"));
				}
				i++;
			}

			Console.WriteLine("{0} have effects", i);
		}

		public static void FindSpellsWithFamilyMask(uint u1, uint u2, uint u3)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where spell.SpellClassMask[0] == u1 && spell.SpellClassMask[1] == u2 && spell.SpellClassMask[2] == u3
			            select spell;

			foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} \t {2}{3}{4}", spell.Id, spell.Name, spell.SpellClassMask[0].ToString("X8"),
				                  spell.SpellClassMask[1].ToString("X8"), spell.SpellClassMask[2].ToString("X8"));
				foreach (var effect in spell.Effects)
				{
					Console.WriteLine("{0} - {1}{2}{3}", effect.EffectType, effect.AffectMask[0].ToString("X8"),
					                  effect.AffectMask[1].ToString("X8"), effect.AffectMask[2].ToString("X8"));
				}
			}
		}

        public static void FindFubecasFrickinSpells()
        {
            var spell = SpellHandler.ById[53270];
            var spellMask0 = spell.Effects[0].AffectMask;
            var spellMask1 = spell.Effects[1].AffectMask;
            var spellSet = spell.SpellClassSet;

            FindSpellWithSpellClassMaskAndSpellClassSet(spellMask0[0], spellMask0[1], spellMask0[2], spellSet);
            FindSpellWithSpellClassMaskAndSpellClassSet(spellMask1[0], spellMask1[1], spellMask1[2], spellSet);
        }

        public static void FindSpellWithSpellClassMaskAndSpellClassSet( uint u1, uint u2, uint u3, Constants.Spells.SpellClassSet set)
        {
            var query = from spell in SpellHandler.ById
			            where spell != null
			            where ((spell.SpellClassMask[0] & u1) != 0) || ((spell.SpellClassMask[1] & u2) != 0) || ((spell.SpellClassMask[2] & u3) != 0)
                        where spell.SpellClassSet == set
			            select spell;
            foreach (var spell in query)
			{
				Console.WriteLine("{0}: {1} \t {2}{3}{4}", spell.Id, spell.Name, spell.SpellClassMask[0].ToString("X8"),
				                  spell.SpellClassMask[1].ToString("X8"), spell.SpellClassMask[2].ToString("X8"));
				foreach (var effect in spell.Effects)
				{
					Console.WriteLine("\t{0} - {1}{2}{3}", effect.EffectType, effect.AffectMask[0].ToString("X8"),
					                  effect.AffectMask[1].ToString("X8"), effect.AffectMask[2].ToString("X8"));
				}
                Console.WriteLine("");
			}
        }

		#region Effect Methods

		public static SpellEffectType FindHighestEffectType()
		{
			var spells = GetSpells();

			SpellEffectType highest = SpellEffectType.None;

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.EffectType > highest)
						highest = effect.EffectType;
				}
			}

			Console.WriteLine("Highest EffectType: {0}", highest);

			return highest;
		}

		public static void FindSpellsWithEffect(SpellEffectType effectType)
		{
			var spells = GetSpells();

			foreach (var spell in spells)
			{
				if (spell.HasEffectWith(effect => effect.EffectType == effectType))
				{
					Console.WriteLine("{0}: {1}", spell.Id, spell.Name);
					foreach (var eff in spell.Effects)
					{
						eff.DumpInfo(Console.Out, "\t");
					}
				}
			}
		}

		public static void FindUnusedEffectTypes()
		{
			var highest = FindHighestEffectType();
			var spells = GetSpells();

			SpellEffectType[] effs = (SpellEffectType[]) Enum.GetValues(typeof (SpellEffectType));
			HashSet<SpellEffectType> allEffects = new HashSet<SpellEffectType>();


			foreach (var spell in spells)
			{
				foreach (var eff in spell.Effects)
				{
					allEffects.Add(eff.EffectType);
				}
			}

			Console.WriteLine("Unused Effects");
			for (SpellEffectType effect = SpellEffectType.None; effect < highest; effect++)
			{
				if (!allEffects.Contains(effect))
				{
					Console.WriteLine(effect);
				}
			}
		}

		public static void FindUsedEffectTypes()
		{
			var highest = FindHighestEffectType();
			var spells = GetSpells();

			SpellEffectType[] effs = (SpellEffectType[]) Enum.GetValues(typeof (SpellEffectType));
			HashSet<SpellEffectType> allEffects = new HashSet<SpellEffectType>();


			foreach (var spell in spells)
			{
				foreach (var eff in spell.Effects)
				{
					allEffects.Add(eff.EffectType);
				}
			}

			Console.WriteLine("Used Effects");
			foreach (var effect in allEffects)
			{
				Console.WriteLine(effect);
			}
		}

		#endregion

		#region Aura Methods

		public static AuraType FindHighestAuraType()
		{
			var spells = GetSpells();

			AuraType highest = AuraType.None;

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.AuraType > highest)
						highest = effect.AuraType;
				}
			}

			Console.WriteLine("Highest AuraType: {0}", highest);

			return highest;
		}

		public static void FindUnusedAuraTypes()
		{
			var highest = FindHighestAuraType();
			var spells = GetSpells();

			HashSet<AuraType> allAuras = new HashSet<AuraType>();

			foreach (var spell in spells)
			{
				foreach (var eff in spell.Effects)
				{
					allAuras.Add(eff.AuraType);
				}
			}

			Console.WriteLine("Unused Effects");
			for (var effect = AuraType.None; effect < highest; effect++)
			{
				if (!allAuras.Contains(effect))
				{
					Console.WriteLine(effect);
				}
			}
		}

		public static void FindUsedAuraTypes()
		{
			var highest = FindHighestAuraType();
			var spells = GetSpells();

			HashSet<AuraType> allAuras = new HashSet<AuraType>();

			foreach (var spell in spells)
			{
				foreach (var eff in spell.Effects)
				{
					allAuras.Add(eff.AuraType);
				}
			}

			Console.WriteLine("Used Auras");
			foreach (var auraType in allAuras)
			{
				Console.WriteLine(auraType);
			}
		}

		public static void FindSpellsWithAura(AuraType aura)
		{
			var spells = GetSpells();
			foreach (var spell in spells)
			{
				if (spell.HasEffectWith(effect => effect.AuraType == aura))
				{
					Console.WriteLine("{0}: {1}", spell.Id, spell.Name);
					foreach (var eff in spell.Effects)
					{
						eff.DumpInfo(Console.Out, "\t");
					}
				}
			}
		}

		#endregion

		#region ImplicitTargetType Methods

		public static ImplicitTargetType FindHighestImplicitTargetType()
		{
			var spells = GetSpells();

			var highest = ImplicitTargetType.None;

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.ImplicitTargetA > highest)
						highest = effect.ImplicitTargetA;
					if (effect.ImplicitTargetB > highest)
						highest = effect.ImplicitTargetB;
				}
			}

			Console.WriteLine("Highest ImplicitTargetType: {0}", highest);

			return highest;
		}

		public static void FindUnusedImplicitTargetTypes()
		{
			var highest = FindHighestImplicitTargetType();
			var spells = GetSpells();

			var allAuras = new HashSet<ImplicitTargetType>();

			foreach (var spell in spells)
			{
				foreach (var eff in spell.Effects)
				{
					allAuras.Add(eff.ImplicitTargetA);
					allAuras.Add(eff.ImplicitTargetB);
				}
			}

			Console.WriteLine("Unused Effects");
			for (var effect = ImplicitTargetType.None; effect < highest; effect++)
			{
				if (!allAuras.Contains(effect))
				{
					Console.WriteLine(effect);
				}
			}
		}

		#endregion

		#region Effect MiscValueB

		public static IEnumerable<Spell> FindSpellsWithMiscValueB()
		{
			var spells = GetSpells();

			var query = from spell in spells
			            where spell.HasEffectWith((e) => e.MiscValueB != 0)
			            select spell;

			foreach (var spell in query)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.MiscValueB != 0)
					{
						Console.WriteLine("{0}: {1} - {2} - {3}", spell.Id, spell.Name, effect.EffectType, effect.MiscValueB);
					}
				}
			}
			Console.WriteLine("{0} spells matched condition", query.Count());

			return query;
		}

		public static IEnumerable<Spell> FindSpellsWithMiscValueBWithEffect(SpellEffectType effectType)
		{
			var spells = GetSpells();

			var query = from spell in spells
			            where spell.HasEffectWith((e) => e.MiscValueB != 0)
			            select spell;

			foreach (var spell in query)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.EffectType == effectType)
					{
						if (effect.MiscValueB != 0)
						{
							Console.WriteLine("{0}: {1} - {2} - {3}", spell.Id, spell.Name, effect.EffectType, effect.MiscValueB);
						}
					}
				}
			}
			Console.WriteLine("{0} spells matched condition", query.Count());

			return query;
		}

		public static IEnumerable<Spell> FindSpellsWithMiscValueB(int val)
		{
			var spells = GetSpells();

			HashSet<Spell> effects = new HashSet<Spell>();

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.MiscValueB == val)
					{
						effects.Add(spell);
					}
				}
			}

			foreach (var effect in effects)
			{
				Console.WriteLine(effect);
			}

			return spells;
		}

		/// <summary>
		/// Summon and ApplyAura
		/// </summary>
		public static void FindEffectsThatUseMiscValueB()
		{
			var spells = GetSpells();

			HashSet<SpellEffectType> effects = new HashSet<SpellEffectType>();

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					if (effect.MiscValueB != 0)
					{
						effects.Add(effect.EffectType);
					}
				}
			}

			foreach (var effect in effects)
			{
				Console.WriteLine(effect);
			}
		}

		public static void MiscValueBTest()
		{
			var spells = FindSpellsWithMiscValueB();

			HashSet<int> vals = new HashSet<int>();

			foreach (var spell in spells)
			{
				foreach (var effect in spell.Effects)
				{
					vals.Add(effect.MiscValueB);
				}
			}
			Console.WriteLine();
			Console.WriteLine();

			foreach (var val in vals)
			{
				Console.WriteLine(val);
			}
		}

		#endregion

		private static IEnumerable<Spell> GetSpells()
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            select spell;

			return query;
		}

		private static IEnumerable<Spell> GetSpells(Predicate<Spell> pred)
		{
			var query = from spell in SpellHandler.ById
			            where spell != null
			            where pred(spell)
			            select spell;

			return query;
		}


		public static void DisplayInfo(int spellId)
		{
			var spell = SpellHandler.ById[spellId];

			if (spell != null)
			{
				Console.WriteLine("{0}: {1}", spell.Id, spell.Name);

				Console.WriteLine("A: {0}", spell.Attributes);
				Console.WriteLine("AEx: {0}", spell.AttributesEx);
				Console.WriteLine("AExB: {0}", spell.AttributesExB);
				Console.WriteLine("AExC: {0}", spell.AttributesExC);
				Console.WriteLine("AExD: {0}", spell.AttributesExD);
				Console.WriteLine("AExE: {0}", spell.AttributesExE);
				Console.WriteLine("AExF: {0}", spell.AttributesExF);
				Console.WriteLine("FacingFlags: {0}", spell.FacingFlags);

				Console.WriteLine("TargetType: {0}", spell.TargetFlags);
				Console.WriteLine("SpellClassMask: {0:X8}{1:X8}{2:X8}", spell.SpellClassMask[0], spell.SpellClassMask[1],
				                  spell.SpellClassMask[2]);
				Console.WriteLine("Effects");
				foreach (var effect in spell.Effects)
				{
					effect.DumpInfo(Console.Out, "");
				}
				Console.WriteLine("Effects End");


				Console.WriteLine("PreventionType: {0}", spell.PreventionType);

				Console.WriteLine("DefenseType: {0}", spell.DefenseType);


				Console.WriteLine();
			}
		}
	}


	public static class SpellExtensions
	{
		public static bool Has(this Spell spell, SpellAttributes toCheck)
		{
			if (spell == null)
				return false;

			return (spell.Attributes & toCheck) == toCheck;
		}

		public static bool Has(this Spell spell, SpellAttributesEx toCheck)
		{
			if (spell == null)
				return false;

			return (spell.AttributesEx & toCheck) == toCheck;
		}

		public static bool Has(this Spell spell, SpellAttributesExB toCheck)
		{
			if (spell == null)
				return false;

			return (spell.AttributesExB & toCheck) == toCheck;
		}

		public static bool HasFacingFlag(this Spell spell, SpellFacingFlags toCheck)
		{
			if (spell == null)
				return false;

			return (spell.FacingFlags & toCheck) == toCheck;
		}
	}
}