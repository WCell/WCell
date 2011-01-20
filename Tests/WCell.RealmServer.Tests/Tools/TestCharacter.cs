using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Talents;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.RealmServer.Database;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Tests.Network;

namespace WCell.RealmServer.Tests.Misc
{
	public class TestCharacter : Character
	{
		public static CharacterRecord PrepareRecord(TestAccount acc, CharacterRecord record, Archetype archetype, bool isNew)
		{
			var race = archetype.Race;

			record.Name = "TestChar" + record.EntityLowId;
			record.AccountId = acc.AccountId;
			record.Created = DateTime.Now;
			record.New = isNew;
			record.TotalPlayTime = 0;
			record.LevelPlayTime = 0;
			record.TutorialFlags = new byte[32];
			record.SetupNewRecord(archetype);

			record.CreateAndFlush();

			return record;
		}

		static TestFakeClient CreateClient(TestAccount acc)
		{
			return new TestFakeClient(acc);
		}

		public TestCharacter(Archetype archetype, bool isNew)
			: this(Setup.AccountPool.CreateAccount(), Setup.CreateCharRecord(), archetype, isNew)
		{
		}

		public TestCharacter(CharacterRecord record, Archetype archetype, bool isNew)
			: this(Setup.AccountPool.CreateAccount(), record, archetype, isNew)
		{
		}

		public TestCharacter(TestAccount acc, CharacterRecord record, Archetype archetype, bool isNew)
		{
			Create(acc, PrepareRecord(acc, record, archetype, isNew), CreateClient(acc));
			record.ExploredZones = new byte[UpdateFieldMgr.ExplorationZoneFieldSize * 4];
			acc.Characters.Add(record);
			acc.Character = this;

			BaseHealth = 1000;
			Health = 1000;
			Assert.AreEqual(1000, BaseHealth);
			Asser.GreaterOrEqual(MaxHealth, BaseHealth);
			Assert.AreEqual(1000, Health);
			Assert.IsFalse(string.IsNullOrEmpty(Name));

			SpecProfiles = new[] { WCell.RealmServer.Talents.SpecProfile.NewSpecProfile(this, 0) };
		}

		public TestAccount TestAccount
		{
			get
			{
				return Account as TestAccount;
			}
			set
			{
				Account = value;
			}
		}

		public TestFakeClient FakeClient
		{
			get
			{
				return Client as TestFakeClient;
			}
			set
			{
				Client = value;
			}
		}

		/// <summary>
		/// Gets the default CharacterPool of the opposing Faction
		/// </summary>
		public CharacterPool Enemies
		{
			get
			{
				if (Faction.Group == FactionGroup.Alliance)
				{
					return Setup.HordeCharacterPool;
				}
				return Setup.AllianceCharacterPool;
			}
		}

		void DeleteAccount()
		{
			Setup.AccountPool.Delete((uint)Account.AccountId);
			TestAccount.Character = null;
			TestAccount = null;
		}

		public void SetName(string name)
		{
			m_name = name;
		}

		public void EnsureNoGroup()
		{
			if (m_groupMember != null)
			{
				m_groupMember.LeaveGroup();
				Assert.IsNull(m_groupMember);
			}
		}

		public void EnsureNoGuild()
		{
			if (m_guildMember != null)
			{
				m_guildMember.LeaveGuild();
				Assert.IsNull(m_guildMember);
			}
		}

		public void EnsureFacing(WorldObject obj)
		{
			Face(obj);
			//Assert.IsTrue(obj.IsInFrontOf(this), "Either Facing or IsInFront-calculation is broken.");
		}

		public void EnsureFacing(Vector3 pos)
		{
			Face(pos);
			//Assert.IsTrue(IsInFrontOfThis(pos), "Either Facing or IsInFront-calculation is broken.");
		}

		public void EnsureFacing(float orientation)
		{
			Face(orientation);
			//Assert.AreEqual(orientation, Orientation);
		}

		/// <summary>
		/// Moves this Character in distance to the given object, by moving this Character 
		/// on the YZ-Plane of the given object and adjust the X-distance.
		/// X-Axis: East -> West.
		/// Y-Axis: Down -> Up.
		/// Z-Axis: South -> North.
		/// </summary>
		public void EnsureXDistance(WorldObject obj, float distance, bool wait)
		{
			var pos = new Vector3(obj.Position.X + distance, obj.Position.Y, obj.Position.Z);
			TeleportTo(obj.Map, ref pos, null, wait);
			if (wait)
			{
				Assert.AreEqual(obj.GetDistance(this), distance, "Teleporting did not work correctly.");
			}
		}

		/// <summary>
		/// Moves this Character in distance to the given object, by moving this Character 
		/// on the XZ-Plane of the given object and adjust the Y-distance.
		/// </summary>
		public void EnsureYDistance(WorldObject obj, float distance, bool wait)
		{
			var pos = new Vector3(obj.Position.X, obj.Position.Y + distance, obj.Position.Z);
			TeleportTo(obj.Map, ref pos, null, wait);
			if (wait)
			{
				Assert.AreEqual(obj.GetDistance(this), distance, "Teleporting did not work correctly.");
			}
		}

		/// <summary>
		/// Moves this Character in distance to the given object, by moving this Character 
		/// on the XY-Plane of the given object and adjust the Z-distance.
		/// </summary>
		public void EnsureZDistance(WorldObject obj, float distance, bool wait)
		{
			var pos = new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z + distance);
			TeleportTo(obj.Map, ref pos, null, wait);
			if (wait)
			{
				Assert.AreEqual(obj.GetDistance(this), distance, "Teleporting did not work correctly.");
			}
		}

		public void TeleportTo(Map map, bool wait)
		{
			TeleportTo(map, ref m_position, 3f, wait);
		}

		public void TeleportTo(Map map, ref Vector3 pos, float? orientation, bool wait)
		{
			if (!HasNode)
			{
				m_Map = null;
			}

			Assert.IsNotNull(map);
			if (Map == map)
			{
				Assert.IsTrue(Map.MoveObject(this, ref pos));

				if (orientation.HasValue)
				{
					Orientation = orientation.Value;
				}
			}
			else
			{
				map.TransferObject(this, pos, wait);

				if (orientation != null)
				{
					Orientation = orientation.Value;
				}

				LastPosition = pos;
			}
		}

		#region Items
		/// <summary>
		/// Adds a new default Item to the given slot, if not already existing
		/// </summary>
		public void EnsureDefaultItem(InventorySlot slot)
		{

		}


		/// <summary>
		/// Adds a new default Item to the given slot, if not already existing
		/// </summary>
		public void EnsureItem(InventorySlot slot)
		{
			Setup.EnsureItemsLoaded();

			EnsureItem(InventorySlot.Invalid, (int)slot);
		}

		public Item EnsureItem(ItemId item)
		{
			return EnsureItem(item, 1);
		}

		public Item EnsureItem(ItemId item, int amount)
		{
			Setup.EnsureBasicSetup();
			ItemMgr.LoadAll();

			return EnsureItem(ItemMgr.GetTemplate(item), amount);
		}

		/// <summary>
		/// Adds a new default Item to the given slot, if not already existing
		/// </summary>
		public Item EnsureItem(ItemTemplate template, int amount)
		{
			var slotId = m_inventory.FindFreeSlot(template, amount);
			Assert.AreNotEqual(BaseInventory.INVALID_SLOT, slotId.Slot);

			var item = slotId.Container.AddUnchecked(slotId.Slot, template, amount, true);
			Assert.IsTrue(item.IsInWorld);
			return item;
		}

		/// <summary>
		/// Adds a new default Item to the given slot, if not already existing
		/// </summary>
		public Item EnsureItem(InventorySlot contSlot, int slot)
		{
			Setup.EnsureItemsLoaded();

			var cont = m_inventory.GetContainer(contSlot, true);

			Assert.IsTrue(cont.IsValidSlot(slot));
			var item = cont[slot];
			if (item == null)
			{
				var msg = cont.TryAdd(Setup.ItemPool.DefaultItemTemplate, slot);
				Assert.AreEqual(InventoryError.OK, msg);
				item = cont[slot];
			}
			return item;
		}
		#endregion

		/// <summary>
		/// Logs out this Character.
		/// </summary>
		public void Remove()
		{
			CancelAllActions();
			if (m_Map != null)
			{
				m_Map.RemoveObject(this);
			}
			World.RemoveCharacter(this);
		}

		protected override void OnEnterMap()
		{
			if (!m_initialized)
			{
				InitializeCharacter();
			}
			base.OnEnterMap();
		}

		/// <summary>
		/// Logs out and deletes this Character.
		/// </summary>
		protected override void DeleteNow()
		{
			Remove();
			Dispose();
		}

		public override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			DeleteAccount();
			FakeClient.ActiveCharacter = null;
			FakeClient.Account = null;
			FakeClient = null;
		}
	}
}