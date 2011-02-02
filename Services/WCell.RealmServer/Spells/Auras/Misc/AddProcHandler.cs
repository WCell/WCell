using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Adds a custom ProcHandler to the Owner while its active
	/// </summary>
	public class AddProcHandler : AuraEffectHandler
	{
		public AddProcHandler(IProcHandler handler)
		{
			ProcHandler = handler;
		}

		public IProcHandler ProcHandler
		{
			get;
			set;
		}

		protected override void Apply()
		{
			Owner.AddProcHandler(ProcHandler);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.RemoveProcHandler(ProcHandler);
		}
	}
}
