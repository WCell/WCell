using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Privileges;
using WCell.RealmServer.Network;
using WCell.RealmServer.Entities;
using WCell.Intercommunication.DataTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Database;
using WCell.RealmServer.RacesClasses;
using WCell.Constants;

namespace WCell.RealmServer.Tests.Misc
{
	/// <summary>
	/// Used for testing purposes only.
	/// Creates a new set of all properties with the "Test" prefix that are also settable (for testing purposes)
	/// </summary>
	public class TestAccount : RealmAccount
	{
		public TestAccount(RoleGroup role, long id)
			: this(null, role, id)
		{
		}

		public TestAccount(string name, RoleGroup role, long id)
			: base(name ?? "TestAccount" + id, new AccountInfo
			{
				AccountId = id,
				ClientId = ClientId.Wotlk,
				EmailAddress = "",
				RoleGroupName = role.Name
			})
		{
			Assert.AreEqual(Role.Rank, role.Rank);
		}

		/// <summary>
		/// The currently active Character of this Account.
		/// </summary>
		public TestCharacter Character
		{
			get;
			set;
		}

		/// <summary>
		/// The database row ID for this account.
		/// </summary>
		public long TestAccountID
		{
			get
			{
				return AccountId;
			}
			set
			{
				m_accountId = value;
			}
		}

		/// <summary>
		/// The username of this account.
		/// </summary>
		public string TestName
		{
			get
			{
				return Name;
			}
			set
			{
				Name = value;
			}
		}

		/// <summary>
		/// The e-mail address of this account.
		/// </summary>
		public string TestEmailAddress
		{
			get
			{
				return EmailAddress;
			}
			set
			{
				m_email = value;
			}
		}

		public RoleGroup TestRole
		{
			get
			{
				return Role;
			}
			set
			{
				Role = value;
			}
		}

		/// <summary>
		/// Whether or not this account is Burning Crusade enabled.
		/// </summary>
		public ClientId TestClientId
		{
			get
			{
				return ClientId;
			}
			set
			{
				ClientId = value;
			}
		}

		public CharacterRecord AddRecord()
		{
			var record = Setup.CreateCharRecord();
			ArchetypeMgr.EnsureInitialize();
			TestCharacter.PrepareRecord(this, record,
				ArchetypeMgr.GetArchetype(RaceId.Human, ClassId.Warrior), true);
			Characters.Add(record);
			return record;
		}
	}
}