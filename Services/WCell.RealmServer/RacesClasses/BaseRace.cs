/*************************************************************************
 *
 *   file		: BaseRace.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-08 17:02:58 +0800 (Tue, 08 Apr 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 244 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.NPCs;
using WCell.Util.Data;

namespace WCell.RealmServer.RacesClasses
{
	/// <summary>
	/// Defines the basics of a race.
	/// </summary>
	//[DataHolder]
	public class BaseRace// : IDataHolder
	{
		#region Fields
		/// <summary>
		/// The <see cref="RaceId" /> that this race object represents.
		/// </summary>
		public RaceId Id;

		public string Name;

		/// <summary>
		/// The faction to which players of this Race belong
		/// </summary>
		public FactionTemplateId FactionTemplateId;

		/// <summary>
		/// The faction to which players of this Race belong
		/// </summary>
		[NotPersistent]
		public Faction Faction;

		/// <summary>
		/// The introduction movie (cinematic) for the given race.
		/// </summary>
		public uint IntroductionMovie;

		/// <summary>
		/// 
		/// </summary>
		public uint MaleDisplayId, FemaleDisplayId;

		/// <summary>
		/// 
		/// </summary>
		public UnitModelInfo MaleModel, FemaleModel;

		/// <summary>
		/// The scale that characters should have with their specific model.
		/// </summary>
		/// <remarks>If a model is normally "this big," then we adjust the Scale property to make 
		/// the character's model appear bigger or smaller than normal, with 1f representing the 
		/// normal size, reducing it or increasing it to make the character appear smaller or 
		/// larger, respectively</remarks>
		public float Scale;

		public ClientId ClientId;
		#endregion

        public BaseRace()
		{
		}

		internal BaseRace(RaceId id)
		{
			Id = id;
		}

		public uint GetDisplayId(GenderType gender)
		{
			return gender == GenderType.Female ? FemaleDisplayId : MaleDisplayId;
		}

		public UnitModelInfo GetModel(GenderType gender)
		{
			return gender == GenderType.Female ? FemaleModel : MaleModel;
		}

	    public void FinalizeAfterLoad()
	    {
	    	//Faction = FactionMgr.Get(FactionTemplateId);
	    	FemaleModel = UnitMgr.GetModelInfo(FemaleDisplayId);
			if (FemaleModel != null)
			{
				MaleModel = UnitMgr.GetModelInfo(MaleDisplayId);
				if (MaleModel != null)
				{
					// fix broken values
					if (FemaleModel.BoundingRadius < 0.1)
					{
						FemaleModel.BoundingRadius = MaleModel.BoundingRadius;
					}
					else if (MaleModel.BoundingRadius < 0.1)
					{
						MaleModel.BoundingRadius = FemaleModel.BoundingRadius;
					}
					if (FemaleModel.CombatReach < 0.1)
					{
						FemaleModel.CombatReach = MaleModel.CombatReach;
					}
					else if (MaleModel.CombatReach < 0.1)
					{
						MaleModel.CombatReach = FemaleModel.CombatReach;
					}

					ArchetypeMgr.BaseRaces[(uint) Id] = this;
				}
			}
	    }

		public override string ToString()
		{
			return Id.ToString();
		}
	}
}