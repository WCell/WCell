using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.RacesClasses;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.Network;
using WCell.Util.Threading;
using System.Threading;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Tests.Misc
{
	/// <summary>
	/// A pool to easily create Character objects. 
	/// Every Character has its own Account and Client instance.
	/// </summary>
	public class CharacterPool
	{
		public static Archetype AllianceArchetype
		{
			get { return ArchetypeMgr.GetArchetype(RaceId.Human, ClassId.Warrior); }
		}

		public static Archetype HordeArchetype
		{
			get { return ArchetypeMgr.GetArchetype(RaceId.Orc, ClassId.Warrior); }
		}

		private static List<CharacterPool> allPools = new List<CharacterPool>();

		public static IEnumerable<CharacterPool> AllPools
		{
			get { return allPools; }
		}

		public static void RemoveAllChars()
		{
			foreach (var region in World.Regions)
			{
				if (region != null)
				{
					region.AddMessageAndWait(() =>
					{
						foreach (var obj in region.CopyObjects())
						{
							if (obj is TestCharacter)
							{
								((TestCharacter)obj).Remove();
							}
						}
					});
				}
			}
		}

		TestCharacter[] m_chars;
		int m_count;

		bool m_living, m_inWorld, m_sameRegion;

		public CharacterPool(FactionGroup faction)
		{
			Setup.EnsureBasicSetup();

			if (faction == FactionGroup.Alliance)
			{
				DefaultArchetype = AllianceArchetype;
			}
			else
			{
				DefaultArchetype = HordeArchetype;
			}

			m_chars = new TestCharacter[100];
			m_count = 0;

			DefaultIsNew = true;
			allPools.Add(this);
		}

		public Archetype DefaultArchetype
		{
			get;
			set;
		}

		/// <summary>
		/// Whether CharacterRecords should be tagged as New
		/// </summary>
		public bool DefaultIsNew
		{
			get;
			set;
		}

		public TestCharacter First
		{
			get { return this[0]; }
		}

		/// <summary>
		/// Ensures that all Characters in this Pool are alive after creation and after this is set to true
		/// </summary>
		public bool EnsureLiving
		{
			get { return m_living; }
			set
			{
				if (m_living != value)
				{
					m_living = value;
					foreach (var chr in this)
					{
						chr.EnsureLiving();
					}
				}
			}
		}

		/// <summary>
		/// Ensures that all Characters in this Pool are in the world after creation and after this is set to true
		/// </summary>
		public bool InWorld
		{
			get { return m_inWorld; }
			set
			{
				if (m_inWorld != value)
				{
					m_inWorld = value;
					foreach (var chr in this)
					{
						chr.EnsureInWorld();
					}
				}
			}
		}

		/// <summary>
		/// Ensures that all Characters are spawned in Kalimdor when created and when this is set to true
		/// </summary>
		public bool EnsureSameRegion
		{
			get { return m_sameRegion; }
			set
			{
				if (m_sameRegion != value)
				{
					m_sameRegion = value;
					if (value)
					{
						if (m_count > 0)
						{
							Region region = Setup.Kalimdor;

							var lockObj = new object();
							var count = 0;
							bool allDone = false;
							Action<Region, Character> onTeleported = (rgn, chr) =>
							{
								lock (lockObj)
								{
									count--;
									if (count == 0 && allDone)
									{
										Monitor.PulseAll(lockObj);
									}
								}
							};
							region.RegionInfo.PlayerEntered += onTeleported;
							foreach (var chr in this)
							{
								if (chr.Region != region)
								{
									chr.TeleportTo(region, false);
									count++;
								}
							}
							allDone = true;

							lock (lockObj)
							{
								if (count > 0)
								{
									Monitor.Wait(lockObj);
								}
								region.RegionInfo.PlayerEntered -= onTeleported;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns the <c>index</c>'th Character.
		/// Creates a new one, if it doesn't exist yet.
		/// Keep in mind that the Account's Client's ActiveCharacter is always set
		/// to the last created Character of the Account's CharacterPool.
		/// </summary>
		public TestCharacter this[uint index]
		{
			get
			{
				Ensure(index);
				return m_chars[index];
			}
			set
			{
				ArrayUtil.Set(ref m_chars, index, value);
			}
		}

		void Ensure(uint index)
		{
			if (m_chars[index] == null)
			{
				var chr = CreateChar(DefaultIsNew, DefaultArchetype);
				OnCreate(chr);
				m_chars[index] = chr;
			}
		}

		/// <summary>
		/// Ensure that the First character exists
		/// </summary>
		public void EnsureFirst()
		{
			Ensure(0);
		}

		/// <summary>
		/// Adds a new TestCharacter and adds it to this Pool.
		/// </summary>
		/// <returns></returns>
		public TestCharacter Create()
		{
			var chr = CreateChar(DefaultIsNew, DefaultArchetype);
			OnCreate(chr);
			ArrayUtil.Add(ref m_chars, chr);
			return chr;
		}

		void OnCreate(TestCharacter chr)
		{
			m_count++;

			if (EnsureLiving)
			{
				chr.EnsureLiving();
			}

			if (EnsureSameRegion)
			{
				chr.TeleportTo(Setup.Kalimdor, true);
			}
			else// if (!InWorld)
			{
				InWorld = true;
			}
		}

		/// <summary>
		/// Removes all Characters from this Pool.
		/// Clearing while adding is not thread-safe.
		/// Deletion is not instant. 
		/// Enqueue to the I/O-Queue to make sure that all Characters have been disposed completely.
		/// </summary>
		public void Clear()
		{
			if (m_count > 0)
			{
				var regions = new HashSet<Region>();
				foreach (var chr in m_chars)
				{
					if (chr != null)
					{
						Action<TestCharacter> action = (character) =>
						{
							character.Logout(true);
						};

						if (chr.Region != null && chr.Region.IsRunning)
						{
							regions.Add(chr.Region);
							chr.Region.AddMessage(new Message1<TestCharacter>(chr, action));
						}
						else
						{
							action(chr);
						}

					}
				}

				// make sure all logout messages have been processed before exiting this method
				foreach (var region in regions)
				{
					region.WaitOneTick();
				}

				m_count = 0;
				m_chars = new TestCharacter[100];
			}
		}

		public static TestCharacter CreateChar(bool isNew, Archetype archetype)
		{
			if (isNew)
			{
				Setup.EnsureBasicSetup();
			}
			else
			{
				// load items from DB -> Items need to be initialized
				Setup.EnsureItemsLoaded();
			}
			var chr = new TestCharacter(archetype, isNew);
			//chr.Load(); TODO: Fix this!
			return chr;
		}

		public IEnumerator<TestCharacter> GetEnumerator()
		{
			foreach (var chr in m_chars)
			{
				if (chr != null)
					yield return chr;
			}
		}

		private static int charNum = 1;
		/// <summary>
		/// Adds the given amount of Characters next to each other in Kalimdor
		/// </summary>
		public TestCharacter[] AddCharacters(int num)
		{
			var region = Setup.Kalimdor;
			var added = 0;

			if (DefaultIsNew)
			{
				Setup.EnsureBasicSetup();
			}
			else
			{
				Setup.EnsureItemsLoaded();
			}
			GC.Collect();

			var chars = new TestCharacter[num];
			for (uint index = 0; index < num; index++)
			{
				ThreadPool.QueueUserWorkItem((iObj) =>
				{
					var i = (uint)iObj;

					var chr = Create();
					chr.SetName("Char" + charNum++);

					// abuse entryid as identifier
					chr.SetUInt32(ObjectFields.ENTRY, i + 1);

					chars[i] = chr;

					region.AddMessage(new Message(() =>
					{
						var pos = Setup.DefaultPosition;
						region.AddObjectNow(chr, ref pos);

						added++;

						if (added == num)
						{
							lock (chars)
							{
								Monitor.Pulse(chars);
							}
						}
					}));
				}, index);
			}

			lock (chars)
			{
				// wait until all chars have been added
				Monitor.Wait(chars);
			}

			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);

			// wait three more ticks on the region to let all pending updates process
			chars.Last().Region.WaitTicks(3);

			Setup.WriteRamUsage("Added {0} Characters", num);

			return chars;
		}
	}
}