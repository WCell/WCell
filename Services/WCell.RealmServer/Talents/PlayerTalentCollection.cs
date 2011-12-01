using System;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Talents
{
	public class PlayerTalentCollection : TalentCollection
	{
		/// <summary>
		/// Every month, the reset tier drops one (but half a month is more than long enough)
		/// </summary>
		public static int PlayerResetTierDecayHours = 24 * 15;

		public PlayerTalentCollection(Character owner)
			: base(owner)
		{
		}

		public override Character OwnerCharacter
		{
			get { return (Character)Owner; }
		}

		public override int FreeTalentPoints
		{
			get { return OwnerCharacter.FreeTalentPoints; }
			set { OwnerCharacter.FreeTalentPoints = value; }
		}

		public override int SpecProfileCount
		{
			get
			{
				return OwnerCharacter.SpecProfiles.Length;
			}
			internal set
			{
				throw new NotImplementedException("TODO: Create/delete SpecProfiles?");
			}
		}

		public override int CurrentSpecIndex
		{
			get { return OwnerCharacter.Record.CurrentSpecIndex; }
		}

		public override uint[] ResetPricesPerTier
		{
			get { return TalentMgr.PlayerTalentResetPricesPerTier; }
		}

		protected override int CurrentResetTier
		{
			get { return OwnerCharacter.Record.TalentResetPriceTier; }
			set { OwnerCharacter.Record.TalentResetPriceTier = value; }
		}

		public override DateTime? LastResetTime
		{
			get { return OwnerCharacter.Record.LastTalentResetTime; }
			set { OwnerCharacter.Record.LastTalentResetTime = value; }
		}

		public override int ResetTierDecayHours
		{
			get { return PlayerResetTierDecayHours; }
		}

		public override int GetFreeTalentPointsForLevel(int level)
		{
			if (level < 10)
			{
				return -TotalPointsSpent;
			}
			return level - 9 - TotalPointsSpent;
		}

		public override void UpdateFreeTalentPointsSilently(int delta)
		{
			OwnerCharacter.SetInt32(PlayerFields.CHARACTER_POINTS, OwnerCharacter.FreeTalentPoints + delta);
		}

		#region Dual Speccing
		public void ChangeTalentGroup(int talentGroupNo)
		{
			// TODO
		}
		#endregion
	}
}