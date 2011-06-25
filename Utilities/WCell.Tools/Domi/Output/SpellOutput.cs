/*************************************************************************
 *
 *   file		: SpellOutput.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 13:29:18 +0100 (to, 28 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1230 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.Core.Initialization;
using WCell.RealmServer;
using WCell.RealmServer.Addons;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;
using System.Reflection;
using WCell.RealmServer.Skills;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Util.Toolshed;
using WCell.RealmServer.Content;
using WCell.RealmServer.Database;
using WCell.Constants.Items;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Talents;
using WCell.Util;

namespace WCell.Tools.Domi.Output
{
	[Tool]
	public class SpellOutput
	{
	    public static readonly string DumpFile = ToolConfig.OutputDir + "SpellsAndEffects.txt";

		public static SpellEffectType[] AllEffectTypes = (SpellEffectType[]) Enum.GetValues(typeof (SpellEffectType));

		static SpellOutput()
		{
			Array.Sort(AllEffectTypes, (effect1, effect2) => { return effect1.ToString().CompareTo(effect2.ToString()); });
		}

		public static void Init()
		{
			RealmDBMgr.Initialize();
		    var mgr = RealmServer.RealmServer.InitMgr;
            RealmAddonMgr.Initialize(mgr);
            mgr.PerformInitialization();
			
			ContentMgr.Initialize();

			SpellHandler.LoadSpells();
			FactionMgr.Initialize();
			SpellHandler.Initialize2();
		}

		public static void Foreach(Action<Spell> action)
		{
			foreach (var spell in SpellHandler.ById)
			{
				if (spell != null)
				{
					action(spell);
				}
			}
		}

		public static Dictionary<SpellEffectType, List<Spell>> GetSpellsByType()
		{
			var SpellsByType = new Dictionary<SpellEffectType, List<Spell>>(200);

			foreach (var effect in AllEffectTypes)
			{
				SpellsByType.Add(effect, new List<Spell>(2000));
			}

			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null)
				{
					continue;
				}

				foreach (var effect in spell.Effects)
				{
					if (effect.EffectType != SpellEffectType.None)
					{
						SpellsByType[effect.EffectType].Add(spell);
					}
				}
			}
			return SpellsByType;
		}

		public static void WriteSpellsByEffect()
		{
			var spells = GetSpellsByType();

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "SpellsByEffect.txt", false))
			{
				foreach (var pair in spells)
				{
					if (pair.Key != SpellEffectType.None)
					{
						writer.WriteLine("Effect: " + pair.Key);
						foreach (Spell spell in pair.Value)
						{
							writer.WriteLine("\tSpell: " + spell);
						}
						writer.WriteLine();
						writer.WriteLine();
					}
				}
			}
		}

		[Tool("WriteSpellsAndEffects")]
        public static void WriteSpellsAndEffects()
		{
			Init();

			using (var writer = new StreamWriter(DumpFile, false))
			{
				foreach (Spell spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					spell.Dump(writer, "\t");
					writer.WriteLine();
					writer.WriteLine("##################################################################");
					writer.WriteLine();
				}
			}
		}

		public static void WritePassiveEffects()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "PassiveEffects.txt", false))
			{
				HashSet<SpellEffect> passiveEffects = new HashSet<SpellEffect>();
				HashSet<SpellEffect> activePassiveEffects = new HashSet<SpellEffect>();

				foreach (Spell spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.IsPassive)
					{
						foreach (SpellEffect effect in spell.Effects)
						{
							passiveEffects.Add(effect);
						}
					}
				}
				foreach (Spell spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (!spell.IsPassive)
					{
						foreach (SpellEffect effect in spell.Effects)
						{
							if (passiveEffects.Contains(effect))
							{
								activePassiveEffects.Add(effect);
							}
						}
					}
				}


				writer.WriteLine("Passive Effects:");
				foreach (SpellEffect effect in passiveEffects)
				{
					//effect.DumpInfo(writer, "\t");
					writer.WriteLine("\t" + effect.EffectType);
				}


				writer.WriteLine();
				writer.WriteLine("Passive and Active Effects:");

				foreach (SpellEffect effect in activePassiveEffects)
				{
					//effect.DumpInfo(writer, "\t");
					writer.WriteLine("\t" + effect.EffectType);
				}
			}
		}


		public static void WriteChanneledSpells()
		{
			List<Spell> spells = new List<Spell>(1000);
			foreach (Spell spell in SpellHandler.ById)
			{
				if (spell == null)
					continue;

				if (spell.IsChanneled)
				{
					spells.Add(spell);
				}
			}

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "ChanneledSpells.txt", false))
			{
				var caster = new ObjectReference();
				foreach (var spell in spells)
				{
					writer.WriteLine("Spell: " + spell);
					bool hasAmpl = false;
					bool hasCustomScript = false;
					foreach (SpellEffect effect in spell.Effects)
					{
						effect.DumpInfo(writer, "\t");
						hasAmpl = hasAmpl || effect.Amplitude > 0;
						hasCustomScript = hasCustomScript || effect.IsScripted;
					}
					if (spell.GetDuration(caster, null) < 1)
					{
						Console.WriteLine(spell);
					}
					writer.WriteLine();
					writer.WriteLine();
				}
			}
		}

		private struct TargetPair
		{
			public ImplicitSpellTargetType TargetA;
			public ImplicitSpellTargetType TargetB;

			public TargetPair(ImplicitSpellTargetType targetA, ImplicitSpellTargetType targetB)
			{
				TargetA = targetA;
				TargetB = targetB;
			}

			public override bool Equals(object obj)
			{
				if (obj is TargetPair)
				{
					TargetPair b = (TargetPair) obj;
					return b.TargetA == TargetA && b.TargetB == TargetB;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return TargetA.GetHashCode()*TargetB.GetHashCode();
			}
		}

		public static void WriteDoubleTargetSpells()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "DoubleTargetSpells.txt", false))
			{
				HashSet<TargetPair> uniqueTargetTypes = new HashSet<TargetPair>();
				foreach (Spell spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					bool contin = false;
					foreach (SpellEffect effect in spell.Effects)
					{
						if (effect.ImplicitTargetA != ImplicitSpellTargetType.None && effect.ImplicitTargetB != ImplicitSpellTargetType.None)
						{
							contin = true;

							uniqueTargetTypes.Add(new TargetPair(effect.ImplicitTargetA, effect.ImplicitTargetB));
						}
					}
					if (!contin)
					{
						continue;
					}

					DumpSpell(writer, spell);
				}

				writer.WriteLine();
				writer.WriteLine();
				writer.WriteLine();

				var uniqueTTs = uniqueTargetTypes.ToArray();

				Array.Sort(uniqueTTs, (pairA, pairB) => { return pairA.TargetA.ToString().CompareTo(pairB.TargetA.ToString()); });
				foreach (var tt in uniqueTTs)
				{
					writer.WriteLine("{0} + {1}", tt.TargetA, tt.TargetB);
				}
			}
		}

		public static void WriteRideSpells()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "RideSpells.txt", false))
			{
				var spells = new List<Spell>();
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.HasEffectWith((effect) => { return effect.AuraType == AuraType.Mounted; }))
					{
						spells.Add(spell);
						DumpSpell(writer, spell);
					}
				}

				writer.WriteLine();
				writer.WriteLine();

				foreach (Spell spell in spells)
				{
					writer.WriteLine(spell);
				}
			}
		}


		public static void WriteFinishingMoves()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "FinishingMoves.txt", false))
			{
				var spells = new List<Spell>();
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					//if ((spell.Attributes & SpellAttributes.Flag0x10) != 0 &&
					//    (spell.Attributes & SpellAttributes.Flag0x10000) != 0 &&
					//    (spell.Attributes & SpellAttributes.Flag0x40000) != 0) {
					//    DumpSpell(writer, spell);
					//}
					if (spell.Description.IndexOf("Finishing move") > -1)
					{
						DumpSpell(writer, spell);
						spells.Add(spell);
					}
				}

				writer.WriteLine();
				writer.WriteLine();

				foreach (Spell spell in spells)
				{
					writer.WriteLine(spell);
				}
			}
		}


		public static void WriteDynamicObjects()
		{
			SpellHandler.LoadSpells(true);

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "DynamicObjects.txt", false))
			{
				var spells = new Dictionary<string, Spell>();
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.DOEffect != null && !spells.ContainsKey(spell.Name))
					{
						writer.WriteLine("{0} (Id: {1})", spell.Name, spell.Id);
						spells[spell.Name] = spell;
					}
				}

				writer.WriteLine();
				writer.WriteLine();

				foreach (Spell spell in spells.Values)
				{
					DumpSpell(writer, spell);
				}
			}
		}


		public static void WriteModRatingSpells()
		{
			WriteMaskedModSpells(AuraType.ModRating);
		}


		public static void WriteAddModifierPercentSpells()
		{
			WriteMaskedModSpells(AuraType.AddModifierPercent);
		}


		public static void WriteAddModifierFlatSpells()
		{
			WriteMaskedModSpells(AuraType.AddModifierFlat);
		}

		private static void WriteMaskedModSpells(AuraType type)
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "" + type + ".txt", false))
			{
				var spells = new Dictionary<string, Spell>();
				SpellEffect statEffect = null;
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.HasEffectWith((effect) =>{
					                        	if (effect.AuraType == type)
					                        	{
					                        		statEffect = effect;
					                        		return true;
					                        	}
					                        	return false;
					                        }))
					{
						spells[spell.Name] = spell;
					}
				}

				foreach (Spell spell in spells.Values)
				{
					spell.HasEffectWith((effect) =>{
					                    	if (effect.AuraType == type)
					                    	{
					                    		statEffect = effect;
					                    		return true;
					                    	}
					                    	return false;
					                    });
					writer.WriteLine("{0} (Id: {1}), Type: {2}{3}", spell.Name, spell.Id, (SpellModifierType) statEffect.MiscValue,
					                 statEffect.ItemId != 0 ? ", ItemType: " + statEffect.ItemId.ToString("X8") : "");
				}


				writer.WriteLine();
				writer.WriteLine();

				foreach (Spell spell in spells.Values)
				{
					DumpSpell(writer, spell);
				}
			}
		}


		public static void WriteInvisSpells()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "InvisSpells.txt", false))
			{
				var invisSpells = new List<Spell>(100);
				var detectSpells = new List<Spell>(100);
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.HasEffectWith((effect) => { return effect.AuraType == AuraType.ModInvisibility; }))
					{
						invisSpells.Add(spell);
					}
					else if (spell.HasEffectWith((effect) => { return effect.AuraType == AuraType.ModInvisibilityDetection; }))
					{
						detectSpells.Add(spell);
					}
				}

				writer.WriteLine("Invis spells:");
				foreach (Spell spell in invisSpells)
				{
					string type = null;
					spell.HasEffectWith((effect) =>{
					                    	if (effect.AuraType == AuraType.ModInvisibility)
					                    	{
					                    		if (type != null)
					                    		{
					                    			if (type.Contains(effect.MiscValue.ToString()))
					                    			{
					                    				type += ", " + effect.MiscValue;
					                    			}
					                    		}
					                    		else
					                    		{
					                    			type = effect.MiscValue.ToString();
					                    		}
					                    		return true;
					                    	}
					                    	return false;
					                    });
					writer.WriteLine("\t{0} (Id: {1}) - Type: {2}", spell.Name, spell.Id, type);
				}

				writer.WriteLine();
				writer.WriteLine();

				writer.WriteLine("Detect spells:");
				foreach (Spell spell in detectSpells)
				{
					string type = null;
					spell.HasEffectWith((effect) =>{
					                    	if (effect.AuraType == AuraType.ModInvisibilityDetection)
					                    	{
					                    		if (type != null)
					                    		{
					                    			if (type.Contains(effect.MiscValue.ToString()))
					                    			{
					                    				type += ", " + effect.MiscValue;
					                    			}
					                    		}
					                    		else
					                    		{
					                    			type = effect.MiscValue.ToString();
					                    		}
					                    		return true;
					                    	}
					                    	return false;
					                    });
					writer.WriteLine("\t{0} (Id: {1}) - Type: {2}", spell.Name, spell.Id, type);
				}

				writer.WriteLine();

				foreach (Spell spell in invisSpells)
				{
					DumpSpell(writer, spell);
				}
				foreach (Spell spell in detectSpells)
				{
					DumpSpell(writer, spell);
				}
			}
		}


		public static void WritePeriodicAreaAuras()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "PeriodicAreaAuras.txt", false))
			{
				var spells = new Dictionary<string, Spell>();
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.HasEffectWith(effect => effect.IsAreaAuraEffect && effect.Amplitude > 0) &&
					    !spells.ContainsKey(spell.Name))
					{
						writer.WriteLine("{0} (Id: {1})", spell.Name, spell.Id);
						spells[spell.Name] = spell;
					}
				}

				writer.WriteLine();
				writer.WriteLine();

				foreach (Spell spell in spells.Values)
				{
					DumpSpell(writer, spell);
				}
			}
		}


		public static void WriteSpellFocusSpells()
		{
			using (StreamWriter writer = new StreamWriter(ToolConfig.OutputDir + "SpellFocus.txt", false))
			{
				Dictionary<SpellFocus, Dictionary<string, Spell>> spells =
					new Dictionary<SpellFocus, Dictionary<string, Spell>>(5000);
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (spell.RequiredSpellFocus != SpellFocus.None)
					{
						Dictionary<string, Spell> focusSpells;
						if (!spells.TryGetValue(spell.RequiredSpellFocus, out focusSpells))
						{
							focusSpells = new Dictionary<string, Spell>();
							spells.Add(spell.RequiredSpellFocus, focusSpells);
						}
						focusSpells[spell.Name] = spell;
					}
				}

				foreach (var dict in spells)
				{
					writer.WriteLine(dict.Key);
					foreach (var spell in dict.Value.Values)
					{
						writer.WriteLine("\t{0} (Id: {1})", spell.Name, spell.Id);
					}
					writer.WriteLine();
					writer.WriteLine("##############################");
					writer.WriteLine();
				}
			}
		}

		public static void WriteSkillSpells()
		{
			WriteSpells("SkillSpells", (spell, list) =>{
			                           	return spell.HasEffectWith((effect) => effect.EffectType == SpellEffectType.Skill ||
			                           	                                       effect.EffectType == SpellEffectType.SkillStep);
			                           },
			            (writer, spell) =>{
			            	var effect = spell.GetEffect(SpellEffectType.Skill);
			            	if (effect == null)
			            	{
			            		effect = spell.GetEffect(SpellEffectType.SkillStep);
			            	}

			            	writer.WriteLine("\tSkill: " + (SkillId) effect.MiscValue);
			            });
		}

		/// <summary>
		/// WriteIndent all spells that match the given filter
		/// </summary>
		/// <param name="filter"></param>
		public static void WriteSpells(string name, Func<Spell, Dictionary<string, Spell>, bool> filter,
		                               Action<TextWriter, Spell> extraOuput)
		{
			using (var writer = new StreamWriter(ToolConfig.OutputDir + name + ".txt", false))
			{
				var spells = new Dictionary<string, Spell>(5000);
				foreach (var spell in SpellHandler.ById)
				{
					if (spell == null)
						continue;

					if (filter(spell, spells))
					{
						spells[spell.Name] = spell;
					}
				}


				writer.WriteLine("Found {0} \"{1}\" - spells:", spells.Count, name);
				writer.WriteLine();


				foreach (var spell in spells.Values)
				{
					writer.WriteLine("{0} (Id: {1})", spell.Name, spell.Id);

					if (extraOuput != null)
					{
						extraOuput(writer, spell);
					}
				}

				writer.WriteLine();
				writer.WriteLine("##########################################");
				writer.WriteLine();

				foreach (var spell in spells.Values)
				{
					DumpSpell(writer, spell);
				}
			}
		}

		public static void WriteAllFields()
		{
			if (SpellHandler.ById.Length == 0)
			{
				SpellHandler.LoadSpells();
			}

			var spellType = typeof (Spell);
			var fields = spellType.GetFields(BindingFlags.Public | BindingFlags.Instance);

			var testSpell = SpellHandler.ById[100];

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "SpellFields.txt", false))
			{
				var types = new HashSet<Object>();
				types.Add(testSpell);
				WriteFields(writer, testSpell, fields, "", types);
			}
		}

		public static void WriteFields(TextWriter writer, Object obj, FieldInfo[] fields, string prefix,
		                               HashSet<Object> lookedUp)
		{
			foreach (var field in fields)
			{
				var fieldType = field.FieldType;
				var name = field.Name;

				var nameAndPrefix = prefix + name;
				var fieldValue = field.GetValue(obj);
				if (fieldType.IsArray)
				{
					if (fieldValue != null)
					{
						var arrType = fieldValue.GetType();
						var arr = fieldValue as Array;
						for (int i = 0; i < arr.Length; i++)
						{
							var val = arr.GetValue(i);
							var arrPrefix = nameAndPrefix + "_" + i;
							WriteField(writer, arrPrefix, val.GetType(), val, lookedUp);
						}
					}
					else
					{
						writer.WriteLine(nameAndPrefix + "[]");
					}
				}
				else
				{
					WriteField(writer, nameAndPrefix, fieldType, fieldValue, lookedUp);
				}
			}
		}

		public static void WriteField(TextWriter writer, string nameAndPrefix, Type fieldType, object obj,
		                              HashSet<Object> lookedUp)
		{
			var str = nameAndPrefix;
			if (obj != null)
			{
				if (lookedUp.Contains(obj))
				{
					return;
				}
				//lookedUp.Add(obj);

				var fieldFields = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);

				if (!(obj is string) && !(obj is Spell) && fieldFields.Length > 0 && !fieldType.IsEnum)
				{
					WriteFields(writer, obj, fieldFields, nameAndPrefix + "_", lookedUp);
				}
				else
				{
					writer.WriteLine("{0} ({1})", str, fieldType.Name);
				}
			}
			else
			{
				writer.WriteLine(str);
			}
		}

		#region Missing ItemIds

		public static void WriteMissingItemIds()
		{
			Tools.StartRealm();

			var items = new Dictionary<uint, Spell>(500);
			foreach (var spell in SpellHandler.ById)
			{
				if (spell != null)
				{
					spell.ForeachEffect(effect =>{
					                    	if (effect.EffectType == SpellEffectType.CreateItem && effect.ItemId != 0)
					                    	{
					                    		if (ItemMgr.GetTemplate(effect.ItemId) == null)
					                    		{
					                    			items[effect.ItemId] = spell;
					                    		}
					                    	}
					                    });
				}
			}

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "MissingSpellItems.txt", false))
			{
				foreach (var pair in items)
				{
					writer.WriteLine("{0} ({1})", pair.Key, pair.Value);
				}
			}
		}

		#endregion

		#region Variable Durations

		public static void WriteVariableDurationSpells()
		{
			Tools.StartRealm();

			using (var writer = new StreamWriter(ToolConfig.OutputDir + "VariableDurationSpells.txt", false))
			{
				foreach (var spell in SpellHandler.ById)
				{
					if (spell != null && spell.Durations.Min > 0 && spell.Durations.Min != spell.Durations.Max)
					{
						writer.WriteLine("{0}", spell);
					}
				}
			}
		}

		#endregion

		#region Spell Families

		private static bool IsAbility(Spell spell)
		{
			return spell.SpellClassSet != SpellClassSet.Generic && spell.Ability != null &&
			       (spell.SpellClassMask.Contains(val => val != 0) ||
			        !spell.HasEffectWith(effect =>
			                             effect.AuraType == AuraType.AddModifierFlat ||
			                             effect.AuraType == AuraType.AddModifierPercent ||
			                             effect.AuraType == AuraType.OverrideClassScripts ||
			                             effect.AuraType == AuraType.ProcTriggerSpell ||
			                             effect.AuraType == AuraType.ProcTriggerDamage));
		}

		private static bool IsEnhancer(Spell spell)
		{
			return spell.SpellClassSet != SpellClassSet.Generic && spell.Ability != null &&
			       !spell.SpellClassMask.Contains(val => val != 0);
		}

		public static void WriteSpellFamilies()
		{
			Init();

			var name = "SpellFamily";

			// first find all actual abilities
			var spells = new Dictionary<SpellClassSet, Dictionary<Spell, List<Spell>>>(100);
			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null)
					continue;

				if (IsAbility(spell))
				{
					Dictionary<Spell, List<Spell>> map;
					if (!spells.TryGetValue(spell.SpellClassSet, out map))
					{
						spells[spell.SpellClassSet] = map = new Dictionary<Spell, List<Spell>>();
					}

					if (!map.Keys.Contains(sp => sp.Name == spell.Name))
					{
						map[spell] = new List<Spell>();
					}
				}
			}

			// find all modifiers of all abilities
			foreach (var spell in SpellHandler.ById)
			{
				if (spell == null)
					continue;

				if (spell.SpellClassSet != SpellClassSet.Generic && spell.Ability != null &&
				    IsEnhancer(spell) && !IsAbility(spell))
				{
					Dictionary<Spell, List<Spell>> map;
					var fam = spell.SpellClassSet;
					if (fam == SpellClassSet.HunterPets)
					{
						fam = SpellClassSet.Hunter;
					}
					if (!spells.TryGetValue(fam, out map))
					{
						continue;
					}

					foreach (var effect in spell.Effects)
					{
						if (effect == null) continue;

						foreach (var pair in map)
						{
							if (pair.Key.MatchesMask(effect.AffectMask) &&
							    !pair.Value.Contains(sp => sp.Name == spell.Name))
							{
								pair.Value.Add(spell);
							}
						}
					}
				}
			}

			foreach (var map in spells.Values)
			{
				var fam = map.First().Key.SpellClassSet;
				using (var writer = new StreamWriter(ToolConfig.OutputDir + name + fam + ".txt", false))
				{
					writer.WriteLine("{0} ({1}):", fam, map.Count);
					writer.WriteLine("#####################################################################");

					foreach (var pair in map)
					{
						var spell = pair.Key;
						var enhancers = pair.Value;
						writer.WriteLine(spell.Name + " (" + (spell.RankDesc != "" ? spell.RankDesc + ", " : "") +
						                 "Id: " + spell.Id + ", " + spell.SpellId + ")");
						if (enhancers.Count > 0)
						{
							writer.WriteLine("\t" + enhancers.ToString(", "));
						}
					}

					writer.WriteLine("");
					writer.WriteLine("###################################### Dumps ##########################################");

					foreach (var pair in map)
					{
						var spell = pair.Key;
						var enhancers = pair.Value;
						spell.Dump(writer, "\t");
						writer.WriteLine("{0} Enhancer(s).", enhancers.Count);
						writer.WriteLine();
						if (enhancers.Count > 0)
						{
							writer.WriteLine();
							foreach (var enhancer in enhancers)
							{
								enhancer.Dump(writer, "\t");
								writer.WriteLine();
							}
						}
						writer.WriteLine();
						writer.WriteLine("##################################################################");
						writer.WriteLine();
					}
				}
			}
		}

		#endregion

		public static void DumpSpell(TextWriter writer, Spell spell)
		{
			spell.Dump(writer, "\t");
			writer.WriteLine();
			writer.WriteLine("#################################################################");
			writer.WriteLine();
		}
	}
}