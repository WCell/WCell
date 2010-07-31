using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Core;
using WCell.RealmServer.Database;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Talents
{
	public interface IHasTalents
	{
		SpecProfile SpecProfile
		{
			get;
			set;
		}

		TalentCollection Talents
		{
			get;
		}

		SpellCollection Spells
		{
			get;
		}

		ClassId Class
		{
			get;
		}

		int FreeTalentPoints
		{
			get;
			set;
		}

		void UpdateFreeTalentPointsSilently(int delta);

		EntityId EntityId
		{
			get;
		}

		IRealmClient Client
		{
			get;
		}

		PetTalentType PetTalentType
		{
			get;
		}

		int GetTalentResetPrice();

		void ResetFreeTalentPoints();
	}
}