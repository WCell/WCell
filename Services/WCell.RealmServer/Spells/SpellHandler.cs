/*************************************************************************
 *
 *   file		: SpellHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-11 11:41:10 +0100 (to, 11 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1254 $
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
using WCell.RealmServer.Lang;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.Util.Collections;
using NLog;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells.Effects;
using WCell.Util;
using WCell.Util.Variables;
using WCell.RealmServer.Spells.Auras;
using WCell.Constants;
using WCell.RealmServer.Spells.Effects.Auras;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Spells
{
	public delegate SpellEffectHandler SpellEffectHandlerCreator(SpellCast cast, SpellEffect effect);

	/// <summary>
	/// Static helper class for packet sending/receiving and container of all spells
	/// </summary>
	public static partial class SpellHandler
	{
		/// <summary>
		/// Whether to cast the learn spell when adding spells
		/// </summary>
		[NotVariable]
		public static bool AnimateSpellAdd = true;

		/// <summary>
		/// Minimum length of cooldowns that are to be saved to DB in milliseconds
		/// </summary>
		public static int MinCooldownSaveTimeMillis = 30;

		public static float SpellCritBaseFactor = 1.5f;

		private static bool loaded;

		[NotVariable]
		/// <summary>
		/// All spells by id.
		/// </summary>
		public static Spell[] ById = new Spell[70000];

		public static uint HighestId
		{
			get;
			internal set;
		}

		/// <summary>
		/// All spells that require tools
		/// </summary>
		internal static readonly List<Spell> SpellsRequiringTools = new List<Spell>(2000);

		/// <summary>
		/// All spells that represent DynamicObjects.
		/// </summary>
		public static readonly Dictionary<SpellId, Spell> DOSpells =
			new Dictionary<SpellId, Spell>(500);

		/// <summary>
		/// All staticly spawned DynamicObjects
		/// </summary>
		public static readonly SynchronizedDictionary<EntityId, DynamicObject> StaticDOs =
			new SynchronizedDictionary<EntityId, DynamicObject>();

		public static readonly List<Spell> QuestCompletors = new List<Spell>(100);

		public static readonly Dictionary<uint, Dictionary<uint, Spell>> NPCSpells =
			new Dictionary<uint, Dictionary<uint, Spell>>(1000);

		public static readonly ShapeshiftEntry[] ShapeshiftEntries = new ShapeshiftEntry[(int)(ShapeshiftForm.End + 10)];

		/// <summary>
		/// Returns the spell with the given spellId or null if it doesn't exist
		/// </summary>
		public static Spell Get(uint spellId)
		{
			if (spellId >= ById.Length)
				return null;
			return ById[spellId];
		}

		/// <summary>
		/// Returns the spell with the given spellId or null if it doesn't exist
		/// </summary>
		public static Spell Get(SpellId spellId)
		{
			if ((uint)spellId >= ById.Length)
				return null;
			return ById[(uint)spellId];
		}

		#region Add / Remove

		internal static void AddSpell(Spell spell)
		{
			ArrayUtil.Set(ref ById, spell.Id, spell);
			HighestId = Math.Max(spell.Id, HighestId);
		}

		/// <summary>
		/// Can be used to add a Spell that does not exist.
		/// </summary>
		public static Spell AddCustomSpell(string name)
		{
			return AddCustomSpell(HighestId + 1, name);
		}

		/// <summary>
		/// Can be used to add a Spell that does not exist.
		/// Usually used for spells that are unknown to the client to signal a certain state.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static Spell AddCustomSpell(uint id, string name)
		{
			if (Get(id) != null)
			{
				throw new ArgumentException("Invalid custom spell id is already in use: " + id + " - " + name);
			}
			var spell = new Spell
			{
				Id = id,
				SpellId = (SpellId)id,
				Name = "[" + RealmLocalizer.Instance.Translate(RealmLangKey.Custom).ToUpper() + "] " + name,
				Effects = new SpellEffect[0],
				RequiredToolIds = new uint[0]
			};
			AddSpell(spell);
			return spell;
		}

		public static void RemoveSpell(uint id)
		{
			ById[id] = null;
		}

		public static void RemoveSpell(SpellId id)
		{
			ById[(int)id] = null;
		}
		#endregion

		#region Applying changes to Spells
		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(this SpellLineId id, Action<Spell> action)
		{
			var line = SpellLines.GetLine(id);
			if (line == null)
			{
				throw new Exception("Invalid SpellLineId: " + id);
			}
			Apply(action, line);
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(Action<Spell> action, SpellLineId id, params SpellId[] ids)
		{
			var line = SpellLines.GetLine(id);
			if (line == null)
			{
				throw new Exception("Invalid SpellLineId: " + id);
			}
			Apply(action, line);
			Apply(action, (IEnumerable<SpellId>)ids);
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(Action<Spell> action, SpellLineId id, SpellLineId id2, params SpellId[] ids)
		{
			var line = SpellLines.GetLine(id);
			if (line == null)
			{
				throw new Exception("Invalid SpellLineId: " + id);
			}
			var line2 = SpellLines.GetLine(id2);
			if (line2 == null)
			{
				throw new Exception("Invalid SpellLineId: " + id2);
			}
			Apply(action, line);
			Apply(action, line2);
			Apply(action, (IEnumerable<SpellId>)ids);
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(this Action<Spell> action, params SpellId[] ids)
		{
			Apply(action, (IEnumerable<SpellId>)ids);
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(this Action<Spell> action, params SpellLineId[] ids)
		{
			foreach (var lineId in ids)
			{
				var line = SpellLines.GetLine(lineId);
				Apply(action, line);
			}
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(this Action<Spell> action, IEnumerable<SpellId> ids)
		{
			foreach (var id in ids)
			{
				var spell = Get(id);
				if (spell == null)
				{
					throw new Exception("Invalid SpellId: " + id);
				}
				action(spell);
			}
		}

		/// <summary>
		/// Apply the given action on all Spells with the given ids
		/// </summary>
		/// <param name="action"></param>
		public static void Apply(this Action<Spell> action, IEnumerable<Spell> spells)
		{
			foreach (var spell in spells)
			{
				action(spell);
			}
		}
		#endregion

		/// <summary>
		/// Returns a list of all SpellLines that are affected by the given spell family set (very long bit field)
		/// </summary>
		public static IEnumerable<SpellLine> GetAffectedSpellLines(ClassId clss, uint[] mask)
		{
			var lines = SpellLines.GetLines(clss);
			var affected = new HashSet<SpellLine>();
			if (lines != null)
			{
				foreach (var line in lines)
				{
					foreach (var spell in line)
					{
						if (spell.MatchesMask(mask))
						{
							affected.Add(line);
							break;
						}
					}
				}
				//foreach (var spell in ById)
				//{
				//    if (spell != null && spell.ClassId == clss && spell.MatchesMask(mask))
				//    {
				//        if (spell.Line != null)
				//        {
				//            affected.Add(line);
				//        }
				//        break;
				//    }
				//}
			}
			return affected;
		}

		#region Init
		[Initialization(InitializationPass.First, "Initialize Spells")]
		public static void LoadSpells()
		{
			LoadSpells(false);
		}

		public static void LoadSpells(bool init)
		{
			if (!loaded)
			{
				InitEffectHandlers();
				LoadOtherDBCs();

				SpellEffect.InitMiscValueTypes();
				loaded = true;
				Spell.InitDbcs();
				new DBCReader<Spell.SpellDBCConverter>(RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELL));

				ContentMgr.Load<SpellLearnRelation>();
				InitSummonHandlers();
				SkillHandler.Initialize();
				TalentMgr.Initialize();

				SpellLines.InitSpellLines();

				ContentMgr.Load<SpellProcEventEntry>();
				foreach (var spell in ById)
				{
					if (spell != null)
					{
						// set custom proc settings
						ProcEventHelper.PatchAffectMasks(spell);
					}
				}
			}

			if (init)
			{
				Initialize2();
			}
		}

		/// <summary>
		/// Second initialization pass
		/// </summary>
		[Initialization(InitializationPass.Third, "Initialize Spells (2)")]
		public static void Initialize2()
		{
			LoadOverrides();
			var learnSpells = new List<Spell>(5900);

			// set TriggerSpells and find TriggerSpell effects
			foreach (var spell in ById)
			{
				if (spell == null)
				{
					continue;
				}
				spell.Initialize();

				if (spell.IsTeachSpell)
				{
					learnSpells.Add(spell);
				}
				if (spell.DOEffect != null)
				{
					DOSpells[spell.SpellId] = spell;
				}
			}

			AuraHandler.RegisterAuraUIDEvaluators();

			// 2nd init
			foreach (var spell in ById)
			{
				if (spell != null)
				{
					spell.Init2();
				}
			}
			SkillHandler.Initialize2();
		}

		/// <summary>
		/// Load given Spell-data from DB
		/// </summary>
		private static void LoadOverrides()
		{
			var mapper = ContentMgr.GetMapper<Spell>();
			mapper.AddObjectsUInt(ById);
			ContentMgr.Load(mapper);
		}

		internal static void InitTools()
		{
			foreach (var spell in SpellsRequiringTools)
			{
				foreach (var id in spell.RequiredToolIds)
				{
					if (id > 0)
					{
						var templ = ItemMgr.Templates.Get(id);
						if (templ != null)
						{
							if (spell.RequiredTools == null)
							{
								spell.RequiredTools = new ItemTemplate[spell.RequiredToolIds.Length];
							}
							ArrayUtil.Add(ref spell.RequiredTools, templ);
						}
					}
				}

				if (spell.RequiredTools != null)
				{
					ArrayUtil.Prune(ref spell.RequiredTools);
				}
			}
		}

		private static void LoadOtherDBCs()
		{
			new DBCReader<ShapeshiftEntryConverter>(RealmServerConfiguration.GetDBCFile("SpellShapeshiftForm"));
			new DBCReader<SummonPropertiesConverter>(RealmServerConfiguration.GetDBCFile("SummonProperties"));
		}
		#endregion

		#region SpellEffectHandlers

		/// <summary>
		/// All effect handler-creation delegates, indexed by their type
		/// </summary>
		public static readonly SpellEffectHandlerCreator[] SpellEffectCreators =
			new SpellEffectHandlerCreator[(int)Convert.ChangeType(Utility.GetMaxEnum<SpellEffectType>(), typeof(int)) + 1];

		static void InitEffectHandlers()
		{
			SpellEffectCreators[(int)SpellEffectType.InstantKill] = (cast, effect) => new InstantKillEffectHandler(cast, effect);								// 1
			SpellEffectCreators[(int)SpellEffectType.SchoolDamage] = (cast, effect) => new SchoolDamageEffectHandler(cast, effect);								// 2
			SpellEffectCreators[(int)SpellEffectType.TeleportUnits] = (cast, effect) => new TeleportUnitsEffectHandler(cast, effect);							// 5
			SpellEffectCreators[(int)SpellEffectType.ApplyAura] = (cast, effect) => new ApplyAuraEffectHandler(cast, effect);									// 6
			SpellEffectCreators[(int)SpellEffectType.EnvironmentalDamage] = (cast, effect) => new EnvironmentalDamageEffectHandler(cast, effect);				// 7
			SpellEffectCreators[(int)SpellEffectType.PowerDrain] = (cast, effect) => new PowerDrainEffectHandler(cast, effect);									// 8
			SpellEffectCreators[(int)SpellEffectType.HealthLeech] = (cast, effect) => new HealthLeechEffectHandler(cast, effect);								// 9
			SpellEffectCreators[(int)SpellEffectType.Heal] = (cast, effect) => new HealEffectHandler(cast, effect);												// 10
			SpellEffectCreators[(int)SpellEffectType.Portal] = (cast, effect) => new PortalHandler(cast, effect);												// 12
			SpellEffectCreators[(int)SpellEffectType.QuestComplete] = (cast, effect) => new QuestCompleteEffectHandler(cast, effect);							// 16
			SpellEffectCreators[(int)SpellEffectType.WeaponDamageNoSchool] = (cast, effect) => new WeaponDamageNoSchoolEffectHandler(cast, effect);				// 17
			SpellEffectCreators[(int)SpellEffectType.Resurrect] = (cast, effect) => new ResurrectEffectHandler(cast, effect);									// 18
			SpellEffectCreators[(int)SpellEffectType.AddExtraAttacks] = (cast, effect) => new AddExtraAttacksEffectHandler(cast, effect);						// 19
			SpellEffectCreators[(int)SpellEffectType.CreateItem] = (cast, effect) => new CreateItemEffectHandler(cast, effect);									// 24
			SpellEffectCreators[(int)SpellEffectType.Weapon] = (cast, effect) => new WeaponEffectHandler(cast, effect);											// 25
			SpellEffectCreators[(int)SpellEffectType.PersistantAreaAura] = (cast, effect) => new PersistantAreaAuraEffectHandler(cast, effect);					// 27
			SpellEffectCreators[(int)SpellEffectType.Summon] = (cast, effect) => new SummonEffectHandler(cast, effect);											// 28
			SpellEffectCreators[(int)SpellEffectType.Energize] = (cast, effect) => new EnergizeEffectHandler(cast, effect);										// 30
			SpellEffectCreators[(int)SpellEffectType.WeaponPercentDamage] = (cast, effect) => new WeaponDamageEffectHandler(cast, effect);						// 31
			SpellEffectCreators[(int)SpellEffectType.OpenLock] = (cast, effect) => new OpenLockEffectHandler(cast, effect);										// 33
			SpellEffectCreators[(int)SpellEffectType.ApplyAreaAura] = (cast, effect) => new ApplyAreaAuraEffectHandler(cast, effect);							// 35
			SpellEffectCreators[(int)SpellEffectType.ApplyRaidAura] = (cast, effect) => new ApplyAreaAura2EffectHandler(cast, effect);							// 65
			SpellEffectCreators[(int)SpellEffectType.LearnSpell] = (cast, effect) => new LearnSpellEffectHandler(cast, effect);									// 36
			SpellEffectCreators[(int)SpellEffectType.Dispel] = (cast, effect) => new DispelEffectHandler(cast, effect);											// 38
			SpellEffectCreators[(int)SpellEffectType.Language] = (cast, effect) => new LanguageEffectHandler(cast, effect);										// 39
			SpellEffectCreators[(int)SpellEffectType.DualWeild] = (cast, effect) => new DualWeildEffectHandler(cast, effect);									// 40
			SpellEffectCreators[(int)SpellEffectType.SkillStep] = (cast, effect) => new SkillStepEffectHandler(cast, effect);									// 44
			SpellEffectCreators[(int)SpellEffectType.Stealth] = (cast, effect) => new StealthEffectHandler(cast, effect);										// 48
			SpellEffectCreators[(int)SpellEffectType.SummonObject] = (cast, effect) => new SummonObjectEffectHandler(cast, effect);								// 50
			SpellEffectCreators[(int)SpellEffectType.SummonObjectWild] = (cast, effect) => new SummonObjectWildEffectHandler(cast, effect);						// 76
			SpellEffectCreators[(int)SpellEffectType.EnchantItem] = (cast, effect) => new EnchantItemEffectHandler(cast, effect);								// 53
			SpellEffectCreators[(int)SpellEffectType.EnchantItemTemporary] = (cast, effect) => new EnchantItemTemporaryEffectHandler(cast, effect);				// 54
			SpellEffectCreators[(int)SpellEffectType.TameCreature] = (cast, effect) => new TameCreatureEffectHandler(cast, effect);								// 55
			SpellEffectCreators[(int)SpellEffectType.SummonPet] = (cast, effect) => new SummonPetEffectHandler(cast, effect);									// 56
			SpellEffectCreators[(int)SpellEffectType.WeaponDamage] = (cast, effect) => new WeaponDamageEffectHandler(cast, effect);								// 58
			SpellEffectCreators[(int)SpellEffectType.Proficiency] = (cast, effect) => new AddProficiencyHandler(cast, effect);									// 60
			SpellEffectCreators[(int)SpellEffectType.SendEvent] = (cast, effect) => new SendEventEffectHandler(cast, effect);									// 61
			SpellEffectCreators[(int)SpellEffectType.Threat] = (cast, effect) => new ThreatHandler(cast, effect);												// 63
			SpellEffectCreators[(int)SpellEffectType.TriggerSpell] = (cast, effect) => new TriggerSpellEffectHandler(cast, effect);								// 64
			SpellEffectCreators[(int)SpellEffectType.CreateManaGem] = (cast, effect) => new CreateManaGemEffectHandler(cast, effect);							// 66
			SpellEffectCreators[(int)SpellEffectType.HealMaxHealth] = (cast, effect) => new HealMaxHealthEffectHandler(cast, effect);							// 67
			SpellEffectCreators[(int)SpellEffectType.InterruptCast] = (cast, effect) => new InterruptCastEffectHandler(cast, effect);							// 68
			SpellEffectCreators[(int)SpellEffectType.Distract] = (cast, effect) => new DistractEffectHandler(cast, effect);										// 69
			SpellEffectCreators[(int)SpellEffectType.ApplyGlyph] = (cast, effect) => new ApplyGlyphEffectHandler(cast, effect);									// 74
			SpellEffectCreators[(int)SpellEffectType.Sanctuary] = (cast, effect) => new RemoveImpairingEffectsHandler(cast, effect);							// 79
			SpellEffectCreators[(int)SpellEffectType.AddComboPoints] = (cast, effect) => new AddComboPointsEffectHandler(cast, effect);							// 80
			SpellEffectCreators[(int)SpellEffectType.Duel] = (cast, effect) => new DuelEffectHandler(cast, effect);												// 83
			SpellEffectCreators[(int)SpellEffectType.SummonPlayer] = (cast, effect) => new SummonPlayerEffectHandler(cast, effect);								// 85
			SpellEffectCreators[(int)SpellEffectType.SelfResurrect] = (cast, effect) => new SelfResurrectEffectHandler(cast, effect);							// 94
			SpellEffectCreators[(int)SpellEffectType.Skinning] = (cast, effect) => new SkinningEffectHandler(cast, effect);										// 95
			SpellEffectCreators[(int)SpellEffectType.Charge] = (cast, effect) => new ChargeEffectHandler(cast, effect);											// 96
			SpellEffectCreators[(int)SpellEffectType.KnockBack] = (cast, effect) => new KnockBackEffectHandler(cast, effect);									// 98
			SpellEffectCreators[(int)SpellEffectType.Disenchant] = (cast, effect) => new DisenchantEffectHandler(cast, effect);									// 99
			SpellEffectCreators[(int)SpellEffectType.Inebriate] = (cast, effect) => new Inebriate(cast, effect);												// 100
			SpellEffectCreators[(int)SpellEffectType.DismissPet] = (cast, effect) => new DismissPetEffectHandler(cast, effect);									// 102
			SpellEffectCreators[(int)SpellEffectType.DispelMechanic] = (cast, effect) => new DispelMechanicEffectHandler(cast, effect);							// 108
			SpellEffectCreators[(int)SpellEffectType.SummonDeadPet] = (cast, effect) => new SummonDeadPetEffectHandler(cast, effect);							// 109
			SpellEffectCreators[(int)SpellEffectType.ResurrectFlat] = (cast, effect) => new ResurrectFlatEffectHandler(cast, effect);							// 113
			SpellEffectCreators[(int)SpellEffectType.Skill] = (cast, effect) => new SkillEffectHandler(cast, effect);											// 118
			SpellEffectCreators[(int)SpellEffectType.ApplyPetAura] = (cast, effect) => new ApplyPetAuraEffectHandler(cast, effect);								// 119
			SpellEffectCreators[(int)SpellEffectType.NormalizedWeaponDamagePlus] = (cast, effect) => new NormalizedWeaponDamagePlusEffectHandler(cast, effect); // 121
			SpellEffectCreators[(int)SpellEffectType.Video] = (cast, effect) => new VideoEffectHandler(cast, effect);											// 123
			SpellEffectCreators[(int)SpellEffectType.StealBeneficialBuff] = (cast, effect) => new StealBeneficialBuffEffectHandler(cast, effect);				// 126
			SpellEffectCreators[(int)SpellEffectType.Prospecting] = (cast, effect) => new ProspectingEffectHandler(cast, effect);								// 127
			SpellEffectCreators[(int)SpellEffectType.ApplyStatAura] = (cast, effect) => new ApplyStatAuraEffectHandler(cast, effect);							// 128
			SpellEffectCreators[(int)SpellEffectType.ApplyStatAuraPercent] = (cast, effect) => new ApplyStatAuraPercentEffectHandler(cast, effect);				// 129
			SpellEffectCreators[(int)SpellEffectType.ForgetSpecialization] = (cast, effect) => new ForgetSpecializationEffectHandler(cast, effect);				// 133
			SpellEffectCreators[(int)SpellEffectType.RestoreHealthPercent] = (cast, effect) => new RestoreHealthPercentEffectHandler(cast, effect);				// 136
			SpellEffectCreators[(int)SpellEffectType.RestoreManaPercent] = (cast, effect) => new RestoreManaPercentEffectHandler(cast, effect);					// 137
			SpellEffectCreators[(int)SpellEffectType.ApplyAuraToMaster] = (cast, effect) => new ApplyAuraToMasterEffectHandler(cast, effect);					// 143
			SpellEffectCreators[(int)SpellEffectType.TriggerRitualOfSummoning] = (cast, effect) => new TriggerRitualOfSummoningEffectHandler(cast, effect);		// 151
			SpellEffectCreators[(int)SpellEffectType.FeedPet] = (cast, effect) => new FeedPetEffectHandler(cast, effect);										// 101
			SpellEffectCreators[(int)SpellEffectType.SummonObjectSlot1] = (cast, effect) => new SummonObjectSlot1Handler(cast, effect);							// 104
			SpellEffectCreators[(int)SpellEffectType.SummonObjectSlot2] = (cast, effect) => new SummonObjectSlot2Handler(cast, effect);							// 105
			SpellEffectCreators[(int)SpellEffectType.SummonObjectSlot3] = (cast, effect) => new SummonObjectSlot1Handler(cast, effect);							// 106
			SpellEffectCreators[(int)SpellEffectType.SummonObjectSlot4] = (cast, effect) => new SummonObjectSlot2Handler(cast, effect);							// 107
			SpellEffectCreators[(int)SpellEffectType.DestroyAllTotems] = (cast, effect) => new DestroyAllTotemsHandler(cast, effect);							// 110
			SpellEffectCreators[(int)SpellEffectType.SetNumberOfTalentGroups] = (cast, effect) => new SetNumberOfTalentGroupsHandler(cast, effect);				// 161
			SpellEffectCreators[(int)SpellEffectType.ActivateTalentGroup] = (cast, effect) => new ActivateTalentGroupHandler(cast, effect);						// 162

			for (var i = 0; i < SpellEffectCreators.Length; i++)
			{
				if (SpellEffectCreators[i] == null)
				{
					SpellEffectCreators[i] = (cast, effect) => new NotImplementedEffectHandler(cast, effect);
				}
			}

			// useless effects
			UnsetHandler(SpellEffectType.None);
			UnsetHandler(SpellEffectType.Dodge);
			UnsetHandler(SpellEffectType.Defense);
			UnsetHandler(SpellEffectType.SpellDefense);
			UnsetHandler(SpellEffectType.Block);
			UnsetHandler(SpellEffectType.ScriptEffect);
			UnsetHandler(SpellEffectType.Detect);
			UnsetHandler(SpellEffectType.Dummy);
			UnsetHandler(SpellEffectType.Parry);
		}

		public static void UnsetHandler(SpellEffectType type)
		{
			//if (SpellEffectCreators[(int)type] != null && SpellEffectCreators[(int)type].GetType() == typeof(NotImplementedEffectHandler))
			{
				SpellEffectCreators[(int)type] = null;
			}
		}

		#endregion

		#region Summons
		public static readonly Dictionary<SummonType, SpellSummonEntry> SummonEntries =
			new Dictionary<SummonType, SpellSummonEntry>();



		public static readonly SpellSummonHandler
			DefaultSummonHandler = new SpellSummonHandler(),
			PetSummonHandler = new SpellSummonPetHandler();

		static void InitSummonHandlers()
		{
			foreach (var entry in SummonEntries.Values)
			{
				if (entry.Id == SummonType.Totem)
				{
					// "Totem" entries do not have Type set to Totem!
					entry.Type = SummonPropertyType.Totem;
				}
				if (entry.Type == SummonPropertyType.Totem)
				{
					// totem
					entry.Handler = new SpellSummonTotemHandler(MathUtil.ClampMinMax(entry.Slot - 1, 0, PetMgr.MaxTotemSlots - 1));
					entry.DetermineAmountBySpellEffect = false;	// totem effect values are always health
				}
				else
				{
					switch (entry.Group)
					{
						case SummonGroup.Controllable:
							entry.Handler = DefaultSummonHandler;
							break;
						case SummonGroup.Friendly:
							entry.Handler = DefaultSummonHandler;
							break;
						case SummonGroup.Pets:
							entry.Handler = PetSummonHandler;
							break;
						case SummonGroup.Wild:
							entry.Handler = DefaultSummonHandler;
							break;
						default:
							entry.Handler = DefaultSummonHandler;
							break;
					}
				}
			}

			// non combat pets
			SummonEntries[SummonType.Critter].Handler = DefaultSummonHandler;
			SummonEntries[SummonType.Critter2].Handler = DefaultSummonHandler;
			SummonEntries[SummonType.Critter3].Handler = DefaultSummonHandler;

			// default
			SummonEntries[SummonType.Demon].Handler = DefaultSummonHandler;


			// 
			SummonEntries[SummonType.DoomGuard].Handler = new SpellSummonDoomguardHandler();
		}

		public static SpellSummonEntry GetSummonEntry(SummonType type)
		{
			SpellSummonEntry entry;
			if (!SummonEntries.TryGetValue(type, out entry))
			{
				log.Warn("Missing SpellSummonEntry for type: " + type);
				return SummonEntries[SummonType.SummonPet];
			}
			return entry;
		}
		#endregion

		#region Shapeshifting
		public static ShapeshiftEntry GetShapeshiftEntry(ShapeshiftForm form)
		{
			return ShapeshiftEntries[(int)form];
		}
		#endregion

		public static ClassId ToClassId(this SpellClassSet classSet)
		{
			switch (classSet)
			{
				case SpellClassSet.Mage:
					return ClassId.Mage;
				case SpellClassSet.Warrior:
					return ClassId.Warrior;
				case SpellClassSet.Warlock:
					return ClassId.Warlock;
				case SpellClassSet.Priest:
					return ClassId.Priest;
				case SpellClassSet.Druid:
					return ClassId.Druid;
				case SpellClassSet.Rogue:
					return ClassId.Rogue;
				case SpellClassSet.Hunter:
					return ClassId.Hunter;
				case SpellClassSet.Paladin:
					return ClassId.Paladin;
				case SpellClassSet.Shaman:
					return ClassId.Shaman;
				case SpellClassSet.DeathKnight:
					return ClassId.DeathKnight;
			}
			return 0;
		}
	}
}