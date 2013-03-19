using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Database.Entities;
using WCell.Util;
using WCell.Core;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Defines an entity with a name.
	/// </summary>
	public class NamedEntity : IEntity, INamed
	{
		public static EntityId CreateId()
		{
			return EntityId.GetPlayerId((uint)CharacterRecord.NextId());
		}


		private readonly EntityId m_EntityId;
		private string m_Name;

		public NamedEntity(string name)
		{
			m_Name = name;
			m_EntityId = CreateId();
		}

		public EntityId EntityId
		{
			get { return m_EntityId; }
		}

		public string Name
		{
			get { return m_Name; }
		}
	}
}