using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModDetectRangeHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			// TODO: add check to Unit.GetAggroRangeSq
		}

		protected override void Remove(bool cancelled)
		{

		}
	}
}
