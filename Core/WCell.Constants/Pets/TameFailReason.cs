using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Pets
{
	public enum TameFailReason
	{
		Ok,
		InvalidCreature = 1,
		TooManyPets = 2,
		CreatureAlreadyOwned = 3,
		NotTamable = 4,
		SummonActive = 5,
		UnitCantTame = 6,
		NotAvailable = 7,
		Internal = 8,
		TooHighLevel = 9,
		TargetDead = 10,
		TargetNotDead = 11,
        CantControlExotic = 12,
		Unknown
	}
}