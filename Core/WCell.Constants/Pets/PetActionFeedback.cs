using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Pets
{
	public enum PetActionFeedback : byte
	{
		None,
		PetDead,
		NothingToAttack,
		CantAttackTarget
	}
}