using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Items;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.RacesClasses
{
	public static class ArchetypeMgr
	{
		#region Fields
		static readonly Func<BaseClass>[] ClassCreators = new Func<BaseClass>[WCellDef.ClassTypeLength];

		internal static BaseClass CreateClass(ClassId id)
		{
			return ClassCreators[(int)id]();
		}

		/// <summary>
		/// Use Archetypes for any customizations.
		/// </summary>
		internal static readonly BaseClass[] BaseClasses = new BaseClass[WCellDef.ClassTypeLength + 20];

		/// <summary>
		/// Use Archetypes for any customizations.
		/// </summary>
		internal static readonly BaseRace[] BaseRaces = new BaseRace[WCellDef.RaceTypeLength + 20];

		/// <summary>
		/// Use Archetype objects to customize basic settings.
		/// Index: [class][race]
		/// </summary>
		public static readonly Archetype[][] Archetypes = new Archetype[WCellDef.ClassTypeLength][];
		#endregion

		#region Getters
		/// <summary>
		/// Returns the Class with the given type
		/// </summary>
		public static BaseClass GetClass(ClassId id)
		{
			return (uint)id >= BaseClasses.Length ? null : BaseClasses[(uint)id];
		}

		/// <summary>
		/// Returns the Race with the given type
		/// </summary>
		public static BaseRace GetRace(RaceId id)
		{
			return (uint)id >= BaseRaces.Length ? null : BaseRaces[(uint)id];
		}

		/// <summary>
		/// Returns the corresponding <see cref="Archetype"/>.
		/// </summary>
		/// <exception cref="NullReferenceException">If Archetype does not exist</exception>
		public static Archetype GetArchetypeNotNull(RaceId race, ClassId clss)
		{
			Archetype type;
			if ((uint)clss >= WCellDef.ClassTypeLength || (uint)race >= WCellDef.RaceTypeLength ||
				((type = Archetypes[(uint)clss][(uint)race]) == null))
			{
				throw new ArgumentException(string.Format("Archetype \"{0} {1}\" does not exist.", race, clss));
			}
			return type;
		}

		public static Archetype GetArchetype(RaceId race, ClassId clssId)
		{
			if ((uint)clssId >= WCellDef.ClassTypeLength || (uint)race >= WCellDef.RaceTypeLength)
			{
				return null;
			}
			var clss = Archetypes[(uint)clssId];
			return clss != null ? clss[(uint)race] : null;
		}

		/// <summary>
		/// Returns all archetypes with the given race/class combination.
		/// 0 for race or class means all.
		/// </summary>
		/// <returns></returns>
		public static List<Archetype> GetArchetypes(RaceId race, ClassId clss)
		{
			if ((uint)clss >= WCellDef.ClassTypeLength || (uint)race >= WCellDef.RaceTypeLength)
			{
				return null;
			}

			var list = new List<Archetype>();
			if (clss == 0)
			{
				// applies to all classes
				foreach (var raceArchetypes in Archetypes)
				{
					if (raceArchetypes != null)
					{
						if (race == 0)
						{
							// applies to all classes and races
							foreach (var archetype in raceArchetypes)
							{
								if (archetype != null)
								{
									list.Add(archetype);
								}
							}
						}
						else
						{
							if (raceArchetypes[(uint)race] != null)
							{
								list.Add(raceArchetypes[(uint)race]);
							}
						}
					}
				}
			}
			else
			{
				if (race == 0)
				{
					// applies to all races
					foreach (var archetype in Archetypes[(uint)clss])
					{
						if (archetype != null)
						{
							list.Add(archetype);
						}
					}
				}
				else
				{
					// just one
					if (Archetypes[(uint)clss][(uint)race] != null)
					{
						list.Add(Archetypes[(uint)clss][(uint)race]);
					}
				}
			}

			if (list.Count == 0)
			{
				return null;
			}
			return list;
		}
		#endregion

		#region Init
		static ArchetypeMgr()
		{
			for (var c = 0; c < Archetypes.Length; c++)
			{
				Archetypes[c] = new Archetype[WCellDef.RaceTypeLength];
			}
		}

		public static void EnsureInitialize()
		{
			if (ClassCreators[(int)ClassId.Warrior] == null)
			{
				Initialize();
			}
		}

		/// <summary>
		/// Note: This step is depending on Skills, Spells and WorldMgr
		/// </summary>
		[Initialization(InitializationPass.Seventh, "Initializing Races and Classes")]
		public static void Initialize()
		{
			if (!loaded)
			{
				InitClasses();
				InitRaces();
				ContentHandler.Load<ClassLevelSetting>();
				ContentHandler.Load<Archetype>();

				ContentHandler.Load<PlayerSpellEntry>();
				//ContentHandler.Load<PlayerSkillEntry>();
				ContentHandler.Load<PlayerActionButtonEntry>();
				ContentHandler.Load<LevelStatInfo>();

				if (ItemMgr.Loaded)
				{
					LoadItems();
				}

				for (var i = 0; i < SpellLines.SpellLinesByClass.Length; i++)
				{
					var lines = SpellLines.SpellLinesByClass[i];
					if (lines == null) continue;

					var clss = GetClass((ClassId)i);
					if (clss != null)
					{
						clss.SpellLines = lines;
					}
				}

				loaded = true;
			}
		}

		private static void InitClasses()
		{
			AddClass(new WarriorClass());
			AddClass(new PaladinClass());
			AddClass(new HunterClass());
			AddClass(new RogueClass());
			AddClass(new PriestClass());
			AddClass(new DeathKnightClass());
			AddClass(new ShamanClass());
			AddClass(new MageClass());
			AddClass(new WarlockClass());
			AddClass(new DruidClass());
		}

		private static void AddClass(BaseClass clss)
		{
			BaseClasses[(int)clss.Id] = clss;
		}

		private static void InitRaces()
		{
			//ContentHandler.Load<BaseRace>();
			var reader = new ListDBCReader<BaseRace, DBCRaceConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_CHRRACES));
			foreach (var race in reader.EntryList)
			{
				race.FinalizeAfterLoad();
			}
		}

		private static bool loaded;

		public static bool Loaded
		{
			get { return loaded; }
		}

		public static void LoadItems()
		{
			// ContentHandler.Load<PlayerItemEntry>();
			//var reader = 
			new DBCReader<DBCStartOutfitConverter>(
                RealmServerConfiguration.GetDBCFile(WCellDef.DBC_CHARSTARTOUTFIT));
		}
		#endregion
	}
}