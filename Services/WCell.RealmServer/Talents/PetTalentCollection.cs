using System;
using WCell.Constants.Pets;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Talents
{
	/// <summary>
	/// A TalentCollection for pets, which *must* have:
	/// 1. A Master of type <see cref="Character"/>
	/// 2. A Record of type <see cref="PermanentPetRecord"/>
	/// </summary>
	public class PetTalentCollection : TalentCollection
	{
		/// <summary>
		/// Every 2h, the reset tier drops by one
		/// </summary>
		public static int PetResetTierDecayHours = 2;

		public PetTalentCollection(NPC owner)
			: base(owner)
		{
		}

		public PermanentPetRecord Record
		{
			get { return ((NPC)Owner).PermanentPetRecord; }
		}

		/// <summary>
		/// Need to make sure that this collection is not used if pet is not owned by character
		/// </summary>
		public override Character OwnerCharacter
		{
			get { return (Character)Owner.Master; }
		}

		public override bool CanLearn(TalentEntry entry, int rank)
		{
			return entry.Tree.Id == ((NPC)Owner).PetTalentType.GetTalentTreeId() && base.CanLearn(entry, rank);
		}

		public override int FreeTalentPoints
		{
			get { return Record.FreeTalentPoints; }
			set { Record.FreeTalentPoints = value; }
		}

		public override int CurrentSpecIndex
		{
			get { return 0; }
		}

		public override uint[] ResetPricesPerTier
		{
			get { return TalentMgr.PetTalentResetPricesPerTier; }
		}

		protected override int CurrentResetTier
		{
			get { return Record.TalentResetPriceTier; }
			set
			{
				Record.TalentResetPriceTier = value;
			}
		}

		public override DateTime? LastResetTime
		{
			get { return Record.LastTalentResetTime; }
			set { Record.LastTalentResetTime = value; }
		}

		/// <summary>
		/// Every 2 hours count down one tier
		/// </summary>
		public override int ResetTierDecayHours
		{
			get { return PetResetTierDecayHours; }
		}

		public override int GetFreeTalentPointsForLevel(int level)
		{
			if (level < 20)
			{
				return -TotalPointsSpent;

			}
			return (level - 19) / 4 - TotalPointsSpent;
		}

		public override void UpdateFreeTalentPointsSilently(int delta)
		{
			throw new NotImplementedException();
		}
	}
}